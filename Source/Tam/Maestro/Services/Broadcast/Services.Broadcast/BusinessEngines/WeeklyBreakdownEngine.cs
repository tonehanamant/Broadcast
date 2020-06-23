using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.BusinessEngines
{
    public interface IWeeklyBreakdownEngine : IApplicationService
    {
        List<WeeklyBreakdownByWeek> GroupWeeklyBreakdownByWeek(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);
        List<WeeklyBreakdownByWeekBySpotLength> GroupWeeklyBreakdownByWeekBySpotLength(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);
        Dictionary<int, int> GetWeekNumberByMediaWeekDictionary(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);

        /// <summary>
        /// Gets all the weekly breakdown combination of creative lengths and dayparts with the combined weight
        /// </summary>
        /// <param name="creativeLengths">List of creative lengths with the weight assigned</param>
        /// <param name="dayparts">List of dayparts with the weight assigned or not</param>
        /// <returns>List of WeeklyBreakdownCombination objects</returns>
        List<WeeklyBreakdownCombination> GetWeeklyBreakdownCombinations(List<CreativeLength> creativeLengths
            , List<PlanDaypartDto> dayparts);

        /// <summary>
        /// Takes a list of dayparts and calculates weights for those dayparts that does not have weight set
        /// Remaining weight is distributed evenly
        /// When weight can not be split evenly we split it into equal pieces and add what`s left to the first daypart that takes part in the distribution
        /// </summary>
        List<(int StadardDaypartId, double WeightingGoalPercent)> GetStandardDaypardWeightingGoals(List<PlanDaypartDto> dayparts);

        WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request);
        List<WeeklyBreakdownByStandardDaypart> GroupWeeklyBreakdownByStandardDaypart(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);

        void RecalculatePercentageOfWeekBasedOnImpressions(List<WeeklyBreakdownWeek> weeks);

        void SetWeekNumber(IEnumerable<WeeklyBreakdownWeek> weeks);
    }

    public class WeeklyBreakdownEngine : IWeeklyBreakdownEngine
    {
        private readonly IPlanValidator _PlanValidator;
        private readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;
        private readonly Dictionary<int, double> _SpotLengthMultiplier;

        private const string _UnsupportedDeliveryTypeMessage = "Unsupported Delivery Type";

        public WeeklyBreakdownEngine(IPlanValidator planValidator,
                                         IMediaMonthAndWeekAggregateCache mediaWeekCache,
                                         ICreativeLengthEngine creativeLengthEngine,
                                         ISpotLengthEngine spotLengthEngine)
        {
            _PlanValidator = planValidator;
            _MediaWeekCache = mediaWeekCache;
            _CreativeLengthEngine = creativeLengthEngine;
            _SpotLengthMultiplier = spotLengthEngine.GetSpotLengthMultipliers();

        }

        public WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request)
        {
            _PlanValidator.ValidateWeeklyBreakdown(request);
            _PlanValidator.ValidateImpressionsPerUnit(request.ImpressionsPerUnit, request.TotalImpressions);

            //calculate flight weeks based on start/end date of the flight
            List<DisplayMediaWeek> weeks = _MediaWeekCache.GetDisplayMediaWeekByFlight(request.FlightStartDate, request.FlightEndDate);

            //add all the days outside of the flight for the first and last week as hiatus days
            request.FlightHiatusDays.AddRange(_GetDaysOutsideOfTheFlight(request.FlightStartDate, request.FlightEndDate, weeks));

            var isInitialLoad = request.Weeks.IsEmpty();
            WeeklyBreakdownResponseDto response;
            if (request.DeliveryType == PlanGoalBreakdownTypeEnum.EvenDelivery)
            {
                response = _CalculateEvenDeliveryPlanWeeklyGoalBreakdown(request, weeks);
            }
            else if (request.DeliveryType == PlanGoalBreakdownTypeEnum.CustomByWeek)
            {
                // we can use EvenDelivery calculation for the request because it has the same structure
                if (isInitialLoad)
                    response = _CalculateEvenDeliveryPlanWeeklyGoalBreakdown(request, weeks);
                else
                    response = _CalculateCustomByWeekPlanWeeklyGoalBreakdown(request, weeks);
            }
            else if (request.DeliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                var creativeLengths = _CreativeLengthEngine.DistributeWeight(request.CreativeLengths);

                if (isInitialLoad)
                    response = _CalculateInitialCustomByWeekByAdLengthPlanWeeklyGoalBreakdown(request, weeks, creativeLengths);
                else
                    response = _CalculateCustomByWeekByAdLengthPlanWeeklyGoalBreakdown(request, weeks, creativeLengths);
            }
            else
            {
                throw new ApplicationException(_UnsupportedDeliveryTypeMessage);
            }

            _CalculateUnits(response.Weeks, request);
            _CalculateWeeklyGoalBreakdownTotals(response, request);
            _OrderWeeks(request, response);
            SetWeekNumber(response.Weeks);

            return response;
        }

        private void _CalculateUnits(List<WeeklyBreakdownWeek> weeks, WeeklyBreakdownRequest request)
        {
            foreach (var week in weeks)
            {
                if (request.Equivalized)
                {
                    if (week.SpotLengthId.HasValue)
                    {
                        week.WeeklyUnits = _CalculateUnitsForSingleSpotLength(request.ImpressionsPerUnit, week);
                    }
                    else
                    {
                        request.CreativeLengths = _CreativeLengthEngine.DistributeWeight(request.CreativeLengths);
                        week.WeeklyUnits = _CalculateUnitsForMultipleSpotLengths(request, week);
                    }
                }
                else
                {
                    week.WeeklyUnits = week.WeeklyImpressions / request.ImpressionsPerUnit;
                }
            }
        }

        private List<DateTime> _GetDaysOutsideOfTheFlight(DateTime flightStartDate, DateTime flightEndDate, List<DisplayMediaWeek> weeks)
        {
            List<DateTime> result = new List<DateTime>();
            DateTime current = weeks.First().WeekStartDate;
            while (current < flightStartDate)
            {
                result.Add(current);
                current = current.AddDays(1);
            }
            current = weeks.Last().WeekEndDate;
            while (flightEndDate < current)
            {
                result.Add(current);
                current = current.AddDays(-1);
            }

            return result;
        }

        /// <summary>
        /// Distributes plan goal evenly by weeks(with first week adjustment)
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateEvenDeliveryPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks)
        {
            var result = new WeeklyBreakdownResponseDto();

            _AddNewWeeksToWeeklyBreakdownResult(result, weeks, request);
            _CalculateGoalsForEvenDeliveryType(request, result.Weeks);

            return result;
        }

        /// <summary>
        /// Removes records that are out of flight
        /// Either recalculates goals based on the recalculation type when 1 record is updated or recalculates all records proportionally
        /// Adds new flight weeks with zero goals
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateCustomByWeekPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks)
        {
            var result = new WeeklyBreakdownResponseDto();

            //remove deleted weeks
            _RemoveOutOfFlightWeeks(request.Weeks, weeks);

            //add the remain weeks
            result.Weeks.AddRange(request.Weeks);

            _RecalculateExistingWeeks(request, out bool redistributeCustom);

            //add the new weeks
            var oldMediaWeekIds = request.Weeks.Select(y => y.MediaWeekId).ToList();
            var newWeeks = weeks.Where(x => !oldMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewWeeksToWeeklyBreakdownResult(result, newWeeks, request);

            //only adjust first week if redistributing
            if (result.Weeks.Where(w => w.NumberOfActiveDays > 0).Any() && redistributeCustom)
            {
                _UpdateFirstWeekAndBudgetAdjustment(request, result.Weeks, request.TotalImpressions);
            }

            return result;
        }

        /// <summary>
        /// Distributes plan goal evenly by weeks(with first week adjustment)
        /// and then goes over each week and distributes its goals based on plan creative length weights(with first creative length adjustment)
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateInitialCustomByWeekByAdLengthPlanWeeklyGoalBreakdown(
            WeeklyBreakdownRequest request,
            List<DisplayMediaWeek> weeks,
            List<CreativeLength> creativeLengths)
        {
            var result = new WeeklyBreakdownResponseDto();

            _AddNewWeeksAndCreativeLengthsToWeeklyBreakdownResult(result, weeks, request, creativeLengths);
            _CalculateGoalsForCustomByWeekByAdLengthDeliveryType(request, result.Weeks);
            RecalculatePercentageOfWeekBasedOnImpressions(result.Weeks);

            return result;
        }

        /// <summary>
        /// Removes records that are out of flight
        /// Removes records that are not in the plan creative lengths list
        /// Either recalculates goals based on the recalculation type when 1 record is updated or recalculates all records proportionally
        /// Adds new creative lengths with zero goals
        /// Adds new flight weeks with zero goals
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateCustomByWeekByAdLengthPlanWeeklyGoalBreakdown(
            WeeklyBreakdownRequest request,
            List<DisplayMediaWeek> weeks,
            List<CreativeLength> creativeLengths)
        {
            var result = new WeeklyBreakdownResponseDto();

            _RemoveOutOfFlightWeeks(request.Weeks, weeks);
            _RemoveWeeksWithCreativeLengthNotPresentingInCurrentCreativeLengthsList(request.Weeks, creativeLengths);

            // add the remain weeks
            result.Weeks.AddRange(request.Weeks);

            _RecalculateExistingWeeks(request, out var redistributeCustom);

            var oldMediaWeekIds = request.Weeks.Select(y => y.MediaWeekId).Distinct().ToList();
            var oldCreativeLengthIds = request.Weeks.Select(x => x.SpotLengthId.Value).Distinct().ToList();
            var newWeeks = weeks.Where(x => !oldMediaWeekIds.Contains(x.Id)).ToList();
            var newCreativeLengths = creativeLengths.Where(x => !oldCreativeLengthIds.Contains(x.SpotLengthId)).ToList();

            _AddNewCreativeLengthsToResult(result, newCreativeLengths);
            _AddNewWeeksAndCreativeLengthsToWeeklyBreakdownResult(result, newWeeks, request, creativeLengths);

            //only adjust first week if redistributing
            if (result.Weeks.Where(w => w.NumberOfActiveDays > 0).Any() && redistributeCustom)
            {
                _UpdateFirstWeekAndBudgetAdjustment(request, result.Weeks, request.TotalImpressions);
            }

            RecalculatePercentageOfWeekBasedOnImpressions(result.Weeks);

            return result;
        }

        private void _CalculateWeeklyGoalBreakdownTotals(WeeklyBreakdownResponseDto weeklyBreakdown, WeeklyBreakdownRequest request)
        {
            weeklyBreakdown.TotalActiveDays = GroupWeeklyBreakdownByWeek(weeklyBreakdown.Weeks)
                .Sum(x => x.NumberOfActiveDays);

            weeklyBreakdown.TotalImpressions = weeklyBreakdown.Weeks.Sum(w => w.WeeklyImpressions);
            var impressionsTotalRatio = weeklyBreakdown.TotalImpressions / request.TotalImpressions;

            weeklyBreakdown.TotalShareOfVoice = Math.Round(100 * impressionsTotalRatio, 0);
            weeklyBreakdown.TotalImpressionsPercentage = weeklyBreakdown.TotalShareOfVoice;

            weeklyBreakdown.TotalRatingPoints = Math.Round(request.TotalRatings * impressionsTotalRatio, 1);
            weeklyBreakdown.TotalBudget = request.TotalBudget * (decimal)impressionsTotalRatio;
            weeklyBreakdown.TotalUnits = weeklyBreakdown.Weeks.Sum(w => w.WeeklyUnits);
        }

        private void _RemoveOutOfFlightWeeks(List<WeeklyBreakdownWeek> requestWeeks, List<DisplayMediaWeek> flightWeeks)
        {
            var flightMediaWeekIds = flightWeeks.Select(x => x.Id).ToList();
            var weeksToRemove = requestWeeks.Where(x => !flightMediaWeekIds.Contains(x.MediaWeekId)).ToList();
            weeksToRemove.ForEach(x => requestWeeks.Remove(x));
        }

        private void _RemoveWeeksWithCreativeLengthNotPresentingInCurrentCreativeLengthsList(List<WeeklyBreakdownWeek> requestWeeks, List<CreativeLength> creativeLengths)
        {
            var spotLengthIds = creativeLengths.Select(x => x.SpotLengthId).ToList();
            var weeksToRemove = requestWeeks.Where(x => !spotLengthIds.Contains(x.SpotLengthId.Value)).ToList();
            weeksToRemove.ForEach(x => requestWeeks.Remove(x));
        }

        public void SetWeekNumber(IEnumerable<WeeklyBreakdownWeek> weeks)
        {
            var weekNumberByMediaWeek = GetWeekNumberByMediaWeekDictionary(weeks);

            foreach (var week in weeks)
            {
                week.WeekNumber = weekNumberByMediaWeek[week.MediaWeekId];
            }
        }

        /// <summary>
        /// Orders weekly breakdown based on delivery type
        /// </summary>
        private void _OrderWeeks(WeeklyBreakdownRequest request, WeeklyBreakdownResponseDto response)
        {
            if (request.DeliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                var creativeLengthsOrder = request.CreativeLengths
                    .Select((item, index) => new { item.SpotLengthId, Order = index + 1 })
                    .ToDictionary(x => x.SpotLengthId, x => x.Order);

                response.Weeks = response.Weeks
                    .OrderBy(x => x.StartDate)
                    .ThenBy(x => creativeLengthsOrder[x.SpotLengthId.Value])
                    .ToList();
            }
            else
            {
                response.Weeks = response.Weeks.OrderBy(x => x.StartDate).ToList();
            }
        }

        /// <summary>
        /// Either recalculates goals based on the recalculation type when 1 record is updated or recalculates all records proportionally
        /// </summary>
        private void _RecalculateExistingWeeks(WeeklyBreakdownRequest request, out bool redistributeCustom)
        {
            redistributeCustom = false;
            double oldImpressionTotals = 0;
            List<WeeklyBreakdownWeek> weeksToUpdate;
            var updatedWeek = request.Weeks.SingleOrDefault(x => x.IsUpdated);

            // If Updated Week present compute only for the updated week, else do it for all the weeks.
            if (updatedWeek != null)
            {
                weeksToUpdate = new List<WeeklyBreakdownWeek> { updatedWeek };
                updatedWeek.IsUpdated = false;
            }
            else
            {
                weeksToUpdate = request.Weeks.ToList();

                //if recalculating the whole week, always calculate from impressions
                request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions;
                redistributeCustom = true; //redistribute goal impressions in same proportions
                oldImpressionTotals = weeksToUpdate.Sum(w => w.WeeklyImpressions);
            }

            foreach (var week in weeksToUpdate)
            {
                week.NumberOfActiveDays = _CalculateActiveDays(week.StartDate, week.EndDate, request.FlightDays, request.FlightHiatusDays, out string activeDaysString);
                week.ActiveDays = activeDaysString;

                if (week.NumberOfActiveDays < 1)
                {
                    week.WeeklyImpressions = 0;
                    week.WeeklyRatings = 0;
                    week.WeeklyImpressionsPercentage = 0;
                    week.WeeklyBudget = 0;
                }

                switch (request.WeeklyBreakdownCalculationFrom)
                {
                    case WeeklyBreakdownCalculationFrom.Ratings:
                        week.WeeklyImpressions = request.TotalRatings <= 0 ? 0 : Math.Floor((week.WeeklyRatings / request.TotalRatings) * request.TotalImpressions);
                        break;
                    case WeeklyBreakdownCalculationFrom.Percentage:
                        week.WeeklyImpressions = Math.Floor((week.WeeklyImpressionsPercentage / 100) * request.TotalImpressions);
                        break;
                    case WeeklyBreakdownCalculationFrom.Units:
                        if (request.Equivalized)
                        {
                            if (week.SpotLengthId.HasValue)
                            {
                                week.WeeklyImpressions = _CalculateImpressionsForSingleSpotLength(request.ImpressionsPerUnit, week);
                            }
                            else
                            {
                                week.WeeklyImpressions = _CalculateImpressionsForMultipleSpotLengths(request, week);
                            }
                        }
                        else
                        {
                            week.WeeklyImpressions = week.WeeklyUnits * request.ImpressionsPerUnit;
                        }
                        break;
                    default:
                        if (redistributeCustom && oldImpressionTotals > 0)
                        {
                            week.WeeklyImpressions = Math.Floor(request.TotalImpressions * week.WeeklyImpressions / oldImpressionTotals);
                        }
                        else
                        {
                            week.WeeklyImpressions = Math.Floor(week.WeeklyImpressions);
                        }

                        break;
                }

                _UpdateGoalsForWeeklyBreakdownItem(request, week, week.WeeklyImpressions);
            }
        }

        public void RecalculatePercentageOfWeekBasedOnImpressions(List<WeeklyBreakdownWeek> weeks)
        {
            var weeklyBreakdownByWeek = GroupWeeklyBreakdownByWeek(weeks);

            foreach (var week in weeklyBreakdownByWeek)
            {
                var breakdownItems = weeks.Where(x => x.MediaWeekId == week.MediaWeekId);

                foreach (var breakdownItem in breakdownItems)
                {
                    breakdownItem.PercentageOfWeek = _CalculatePercentageOfWeek(breakdownItem.WeeklyImpressions, week.Impressions);
                }

                // do not set 100% to the first record when week does not have immpressions
                if (breakdownItems.Any(x => x.WeeklyImpressions > 0))
                {
                    // add remaining percentages to the first item
                    breakdownItems.First().PercentageOfWeek += 100 - breakdownItems.Sum(x => x.PercentageOfWeek);
                }
            }
        }

        private void _AddNewWeeksToWeeklyBreakdownResult(WeeklyBreakdownResponseDto result, List<DisplayMediaWeek> weeks, WeeklyBreakdownRequest request)
        {
            foreach (DisplayMediaWeek week in weeks)
            {
                var activeDays = _CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, out string activeDaysString);

                result.Weeks.Add(new WeeklyBreakdownWeek
                {
                    ActiveDays = activeDaysString,
                    NumberOfActiveDays = activeDays,
                    StartDate = week.WeekStartDate,
                    EndDate = week.WeekEndDate,
                    MediaWeekId = week.Id
                });
            }
        }

        private void _AddNewWeeksAndCreativeLengthsToWeeklyBreakdownResult(
           WeeklyBreakdownResponseDto result,
           List<DisplayMediaWeek> weeks,
           WeeklyBreakdownRequest request,
           List<CreativeLength> creativeLengths)
        {
            foreach (DisplayMediaWeek week in weeks)
            {
                var activeDays = _CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, out string activeDaysString);

                foreach (var creativeLength in creativeLengths)
                {
                    result.Weeks.Add(new WeeklyBreakdownWeek
                    {
                        ActiveDays = activeDaysString,
                        NumberOfActiveDays = activeDays,
                        StartDate = week.WeekStartDate,
                        EndDate = week.WeekEndDate,
                        MediaWeekId = week.Id,
                        SpotLengthId = creativeLength.SpotLengthId,
                        PercentageOfWeek = creativeLength.Weight
                    });
                }
            }
        }

        /// <summary>
        /// Distributes plan goal evenly by weeks(with first week adjustment)
        /// and then goes over each week and distributes its goals based on plan creative length weights(with first creative length adjustment)
        /// </summary>
        private void _CalculateGoalsForCustomByWeekByAdLengthDeliveryType(
            WeeklyBreakdownRequest request,
            List<WeeklyBreakdownWeek> weeks)
        {
            var activeWeeks = weeks.Where(x => x.NumberOfActiveDays > 0);
            var activeWeekIds = activeWeeks.Select(x => x.MediaWeekId).Distinct().ToList();
            var impressionsByWeeks = _DistributeImpressionsByWeeks(request.TotalImpressions, activeWeekIds);

            foreach (var weekId in activeWeekIds)
            {
                var impressionsForWeek = impressionsByWeeks[weekId];
                var breakdownByAdLengthForWeek = activeWeeks.Where(x => x.MediaWeekId == weekId).ToList();

                foreach (var breakdownItem in breakdownByAdLengthForWeek)
                {
                    var adLengthFraction = breakdownItem.PercentageOfWeek.Value / 100;
                    var impressions = Math.Floor(impressionsForWeek * adLengthFraction);

                    _UpdateGoalsForWeeklyBreakdownItem(request, breakdownItem, impressions);
                }

                _UpdateFirstWeekAndBudgetAdjustment(request, breakdownByAdLengthForWeek, impressionsForWeek);
            }
        }

        /// <summary>
        /// Distributes plan goal evenly by weeks(with first week adjustment)
        /// </summary>
        private void _CalculateGoalsForEvenDeliveryType(WeeklyBreakdownRequest request, List<WeeklyBreakdownWeek> weeks)
        {
            var activeWeeks = weeks.Where(x => x.NumberOfActiveDays > 0);
            var activeWeekIds = activeWeeks.Select(x => x.MediaWeekId).ToList();
            var impressionsByWeeks = _DistributeImpressionsByWeeks(request.TotalImpressions, activeWeekIds);

            foreach (var breakdownItem in activeWeeks)
            {
                var impressions = impressionsByWeeks[breakdownItem.MediaWeekId];

                _UpdateGoalsForWeeklyBreakdownItem(request, breakdownItem, impressions);
            }
        }

        private void _UpdateFirstWeekAndBudgetAdjustment(
           WeeklyBreakdownRequest request,
           List<WeeklyBreakdownWeek> weeks,
           double totalImpressions)
        {
            var firstWeek = weeks.First();
            var totalImpressionsRounded = weeks.Sum(w => w.WeeklyImpressions);
            var roundedImpressionsDifference = totalImpressions - totalImpressionsRounded;
            var impressions = Math.Floor(firstWeek.WeeklyImpressions + roundedImpressionsDifference);

            _UpdateGoalsForWeeklyBreakdownItem(request, firstWeek, impressions);
        }

        private void _AddNewCreativeLengthsToResult(
            WeeklyBreakdownResponseDto result,
            List<CreativeLength> creativeLengths)
        {
            var existingWeeks = GroupWeeklyBreakdownByWeek(result.Weeks);

            foreach (var existingWeek in existingWeeks)
            {
                foreach (var creativeLength in creativeLengths)
                {
                    result.Weeks.Add(new WeeklyBreakdownWeek
                    {
                        ActiveDays = existingWeek.ActiveDays,
                        NumberOfActiveDays = existingWeek.NumberOfActiveDays,
                        StartDate = existingWeek.StartDate,
                        EndDate = existingWeek.EndDate,
                        MediaWeekId = existingWeek.MediaWeekId,
                        SpotLengthId = creativeLength.SpotLengthId,
                        PercentageOfWeek = creativeLength.Weight
                    });
                }
            }
        }

        private int _CalculateActiveDays(DateTime weekStartDate, DateTime weekEndDate, List<int> flightDays, List<DateTime> hiatusDays, out string activeDaysString)
        {
            var daysOfWeek = new List<string> { "M", "Tu", "W", "Th", "F", "Sa", "Su" };
            activeDaysString = string.Empty;
            var hiatusDaysInWeek = hiatusDays.Where(x => weekStartDate <= x && weekEndDate >= x).ToList();
            var days = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            var daysToRemove = days.Except(flightDays);

            foreach (var day in daysToRemove)
            {
                daysOfWeek[day - 1] = null;
            }

            //if all the week is hiatus, return 0 active days
            if (hiatusDaysInWeek.Count == 7)
            {
                return 0;
            }

            //construct the active days string
            //null the hiatus days in the week
            for (int i = 0; i < daysOfWeek.Count; i++)
            {
                if (hiatusDaysInWeek.Contains(weekStartDate.AddDays(i)))
                {
                    daysOfWeek[i] = null;
                }
            }

            //group the active days that are not null
            var groupOfActiveDays = daysOfWeek.GroupConnectedItems((a, b) => !string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b));
            var activeDaysList = new List<string>();
            foreach (var group in groupOfActiveDays)
            {
                //if the group contains 1 or 2 elements, join them by comma
                if (group.Count() == 1 || group.Count() == 2)
                {
                    activeDaysList.Add(string.Join(",", group));
                }
                else  //if the group contains more then 3 elements, join the first and the last one with "-"
                {
                    activeDaysList.Add($"{group.First()}-{group.Last()}");
                }
            }

            activeDaysString = string.Join(",", activeDaysList);
            var numberOfActiveDays = daysOfWeek.Where(x => x != null).Count();
            //number of active days this week is 7 minus number of hiatus days
            return numberOfActiveDays;
        }

        private void _UpdateGoalsForWeeklyBreakdownItem(
            WeeklyBreakdownRequest request,
            WeeklyBreakdownWeek breakdownItem,
            double impressions)
        {
            _UpdateGoalsForWeeklyBreakdownItem(
                request.TotalImpressions,
                request.TotalRatings,
                request.TotalBudget,
                breakdownItem,
                impressions);
        }

        private void _UpdateGoalsForWeeklyBreakdownItem(
            double totalImpressions,
            double totalRatings,
            decimal totalBudget,
            WeeklyBreakdownWeek breakdownItem,
            double impressions)
        {
            var budgetPerOneImpression = totalBudget / (decimal)totalImpressions;
            var weeklyRatio = impressions / totalImpressions;

            breakdownItem.WeeklyImpressions = impressions;
            breakdownItem.WeeklyImpressionsPercentage = Math.Round(100 * weeklyRatio);
            breakdownItem.WeeklyRatings = ProposalMath.RoundDownWithDecimals(totalRatings * weeklyRatio, 1);
            breakdownItem.WeeklyBudget = (decimal)impressions * budgetPerOneImpression;
        }

        private double _CalculatePercentageOfWeek(double breakdownItemImpressions, double weeklyImpressions)
        {
            return weeklyImpressions > 0 ? Math.Round(100 * breakdownItemImpressions / weeklyImpressions) : 0;
        }

        /// <summary>
        /// Evenly distributes impressions by each week and adds remaining impressions to the first week
        /// </summary>
        private Dictionary<int, double> _DistributeImpressionsByWeeks(double totalImpressions, List<int> weekIds)
        {
            var result = new Dictionary<int, double>();

            if (!weekIds.Any())
                return result;

            var totalWeeks = weekIds.Count();
            var impressionsPerWeek = Math.Floor(totalImpressions / totalWeeks);

            // add undistributed impressions to the first week
            var undistributedImpressions = totalImpressions - (totalWeeks * impressionsPerWeek);
            var impressionsForFirstWeek = Math.Floor(impressionsPerWeek + undistributedImpressions);

            for (var i = 0; i < weekIds.Count(); i++)
            {
                var impressions = i == 0 ? impressionsForFirstWeek : impressionsPerWeek;
                var weekId = weekIds[i];

                result[weekId] = impressions;
            }

            return result;
        }

        public Dictionary<int, int> GetWeekNumberByMediaWeekDictionary(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown)
        {
            var weeklyBreakdownByWeek = GroupWeeklyBreakdownByWeek(weeklyBreakdown);

            return weeklyBreakdownByWeek
                .OrderBy(x => x.MediaWeekId)
                .Select((item, index) => new { item.MediaWeekId, weekNumber = index + 1 })
                .ToDictionary(x => x.MediaWeekId, x => x.weekNumber);
        }

        public List<WeeklyBreakdownByWeek> GroupWeeklyBreakdownByWeek(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown)
        {
            return weeklyBreakdown
                .GroupBy(x => x.MediaWeekId)
                .Select(grouping =>
                {
                    var first = grouping.First();
                    var allItems = grouping.ToList();
                    double weeklyImpressions = allItems.Sum(x => x.WeeklyImpressions);
                    double unitsImpressions = allItems.Sum(x => x.UnitImpressions);

                    return new WeeklyBreakdownByWeek
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        Impressions = weeklyImpressions,
                        Budget = allItems.Sum(x => x.WeeklyBudget),
                        Adu = (int)(allItems.Sum(x => x.AduImpressions) / BroadcastConstants.ImpressionsPerUnit),
                        Units = unitsImpressions == 0
                            ? 0
                            : weeklyImpressions / unitsImpressions
                    };
                })
                .ToList();
        }

        public List<WeeklyBreakdownByWeekBySpotLength> GroupWeeklyBreakdownByWeekBySpotLength(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown)
        {
            return weeklyBreakdown
                .GroupBy(x => new { x.MediaWeekId, x.SpotLengthId })
                .Select(grouping =>
                {
                    var first = grouping.First();
                    var allItems = grouping.ToList();
                    double weekImpressions = allItems.Sum(x => x.WeeklyImpressions);
                    double unitsImpressions = allItems.Sum(x => x.UnitImpressions);

                    return new WeeklyBreakdownByWeekBySpotLength
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        SpotLengthId = first.SpotLengthId.Value,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        Impressions = weekImpressions,
                        Budget = allItems.Sum(x => x.WeeklyBudget),
                        Adu = (int)(allItems.Sum(x => x.AduImpressions) / BroadcastConstants.ImpressionsPerUnit),
                        Units = unitsImpressions == 0
                            ? 0
                            : weekImpressions / unitsImpressions
                    };
                }).ToList();
        }

        public List<WeeklyBreakdownByStandardDaypart> GroupWeeklyBreakdownByStandardDaypart(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown)
        {
            return weeklyBreakdown
                .GroupBy(x => x.DaypartCodeId.Value)
                .Select(grouping =>
                {
                    return new WeeklyBreakdownByStandardDaypart
                    {
                        StandardDaypartId = grouping.Key,
                        Impressions = grouping.Sum(x => x.WeeklyImpressions),
                        Budget = grouping.Sum(x => x.WeeklyBudget)
                    };
                })
                .ToList();
        }

        /// <inheritdoc />
        public List<WeeklyBreakdownCombination> GetWeeklyBreakdownCombinations(List<CreativeLength> creativeLengths
            , List<PlanDaypartDto> dayparts)
        {
            var standardDaypardWeightingGoals = GetStandardDaypardWeightingGoals(dayparts);
            var allSpotLengthIdAndStandardDaypartIdCombinations = creativeLengths.SelectMany(x => standardDaypardWeightingGoals, (a, b) =>
            new WeeklyBreakdownCombination
            {
                SpotLengthId = a.SpotLengthId,
                DaypartCodeId = b.StadardDaypartId,
                Weighting = GeneralMath.ConvertPercentageToFraction(a.Weight.Value) * GeneralMath.ConvertPercentageToFraction(b.WeightingGoalPercent)
            }).ToList();
            return allSpotLengthIdAndStandardDaypartIdCombinations;
        }

        /// <inheritdoc />
        public List<(int StadardDaypartId, double WeightingGoalPercent)> GetStandardDaypardWeightingGoals(List<PlanDaypartDto> dayparts)
        {
            var weightingGoalPercentByStandardDaypartIdDictionary = new Dictionary<int, double>();

            var daypartsWithWeighting = dayparts.Where(x => x.WeightingGoalPercent.HasValue).ToList();
            var daypartsWithoutWeighting = dayparts.Where(x => !x.WeightingGoalPercent.HasValue).ToList();

            if (daypartsWithoutWeighting.Any())
            {
                var undistributedWeighing = 100 - daypartsWithWeighting.Sum(x => x.WeightingGoalPercent.Value);
                var undistributedWeighingPerDaypart = Math.Floor(undistributedWeighing / daypartsWithoutWeighting.Count);

                var remainingWeighing = undistributedWeighing - (undistributedWeighingPerDaypart * daypartsWithoutWeighting.Count);
                var firstDaypartWeighing = undistributedWeighingPerDaypart + remainingWeighing;
                var firstDaypart = daypartsWithoutWeighting.TakeOut(0);

                weightingGoalPercentByStandardDaypartIdDictionary[firstDaypart.DaypartCodeId] = firstDaypartWeighing;
                daypartsWithoutWeighting.ForEach(x => weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartCodeId] = undistributedWeighingPerDaypart);
            }

            daypartsWithWeighting.ForEach(x => weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartCodeId] = x.WeightingGoalPercent.Value);

            // to keep the original order
            return dayparts
                .Select(x => (
                    StadardDaypartId: x.DaypartCodeId,
                    WeightingGoalPercent: weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartCodeId]))
                .ToList();
        }

        private double _CalculateImpressionsForMultipleSpotLengths(WeeklyBreakdownRequest request, WeeklyBreakdownWeek week)
        {
            return request.CreativeLengths
                .Sum(p => week.WeeklyUnits * _CalculateEquivalizedImpressionsPerUnit(request.ImpressionsPerUnit, p.SpotLengthId)
                            * GeneralMath.ConvertPercentageToFraction(p.Weight.Value));
        }

        private double _CalculateImpressionsForSingleSpotLength(double impressionPerUnit, WeeklyBreakdownWeek week)
        {
            return week.WeeklyUnits * _CalculateEquivalizedImpressionsPerUnit(impressionPerUnit, week.SpotLengthId.Value);
        }

        private double _CalculateEquivalizedImpressionsPerUnit(double impressionPerUnit, int spotLengthId)
        {
            return impressionPerUnit * _SpotLengthMultiplier[spotLengthId];
        }

        private double _CalculateUnitsForMultipleSpotLengths(WeeklyBreakdownRequest request, WeeklyBreakdownWeek week)
        {
            return request.CreativeLengths
                   .Sum(p => week.WeeklyImpressions * GeneralMath.ConvertPercentageToFraction(p.Weight.Value)
                   / _CalculateEquivalizedImpressionsPerUnit(request.ImpressionsPerUnit, p.SpotLengthId));
        }

        private double _CalculateUnitsForSingleSpotLength(double impressionsPerUnit, WeeklyBreakdownWeek week)
        {
            return week.WeeklyImpressions / _CalculateEquivalizedImpressionsPerUnit(impressionsPerUnit, week.SpotLengthId.Value);
        }
    }
}
