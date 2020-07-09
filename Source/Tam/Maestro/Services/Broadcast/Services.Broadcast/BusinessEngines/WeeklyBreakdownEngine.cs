﻿using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.spotcableXML;
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
        List<WeeklyBreakdownByWeek> GroupWeeklyBreakdownByWeek(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit = 0, List<CreativeLength> creativeLengths = null, bool equivalized = false);
        List<WeeklyBreakdownByWeekBySpotLength> GroupWeeklyBreakdownByWeekBySpotLength(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit, bool equivalized);
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
        List<DaypartDefaultWeightingGoal> GetStandardDaypardWeightingGoals(List<PlanDaypartDto> dayparts);

        WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request);
        List<WeeklyBreakdownByStandardDaypart> GroupWeeklyBreakdownByStandardDaypart(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);

        void RecalculatePercentageOfWeekBasedOnImpressions(List<WeeklyBreakdownWeek> weeks);

        void SetWeekNumber(IEnumerable<WeeklyBreakdownWeek> weeks);

        /// <summary>
        /// Calculates the adu Impressions
        /// </summary>
        /// <param name="week">Week object</param>
        /// <param name="equivalized">Equivalized flag</param>
        /// <param name="impressionsPerUnit">Impressions per unit value</param>
        /// <param name="creativeLengths">List of creative lengths</param>
        /// <returns>Number of adu impressions</returns>
        double CalculateWeeklyADUImpressions(WeeklyBreakdownWeek week, bool equivalized
            , double impressionsPerUnit, List<CreativeLength> creativeLengths);

        /// <summary>
        /// Groups the weekly breakdown by week by daypart
        /// </summary>
        List<WeeklyBreakdownByWeekByDaypart> GroupWeeklyBreakdownByWeekByDaypart(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit, bool equivalized, List<CreativeLength> creativeLengths);

        /// <summary>
        /// Calculates the adu with decimals.
        /// </summary>
        /// <param name="impressionsPerUnit">The impressions per unit.</param>
        /// <param name="aduImpressions">The adu impressions total.</param>
        /// <param name="equivalized">Equivalized flag</param>
        /// <param name="creativeLengths">Creative lengths of the plan.</param>
        /// <returns>Adu number as double</returns>
        double CalculateADUWithDecimals(double impressionsPerUnit, double aduImpressions
            , bool equivalized, List<CreativeLength> creativeLengths = null);
    }

    public class WeeklyBreakdownEngine : IWeeklyBreakdownEngine
    {
        private readonly IPlanValidator _PlanValidator;
        private readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;
        private readonly Dictionary<int, double> _SpotLengthMultiplier;

        //TODO: this needs to be removed after all plans in production have a modified version greater then 20.07 release date
        private const double _DefaultImpressionsPerUnitForOldPlans = 500000;
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

            if(request.ImpressionsPerUnit <= 0) //old plan
            {
                if (request.TotalImpressions < _DefaultImpressionsPerUnitForOldPlans)
                    request.ImpressionsPerUnit = request.TotalImpressions;
                else
                    request.ImpressionsPerUnit = _DefaultImpressionsPerUnitForOldPlans;                    
            }

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
            else if (request.DeliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart)
            {
                var daypartDefaults = GetStandardDaypardWeightingGoals(request.Dayparts);
                if (isInitialLoad)
                    response = _CalculateInitialCustomByWeekByDaypartWeeklyGoalBreadkdown(request, weeks, daypartDefaults);
                else
                    response = _CalculateCustomByWeekByDaypartWeeklyGoalBreadkdown(request, weeks, daypartDefaults);
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
        /// Calculate the initial load of weekly brekdown using the delivery type By Week By Daypart
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateInitialCustomByWeekByDaypartWeeklyGoalBreadkdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks, List<DaypartDefaultWeightingGoal> daypartDefaults)
        {
            var result = new WeeklyBreakdownResponseDto();

            _AddNewWeeksByDaypartToWeeklyBreakdownResult(result, weeks, request, daypartDefaults);
            _CalculateGoalsForCustomByWeekByPercentageOfWeek(request, result.Weeks);
            RecalculatePercentageOfWeekBasedOnImpressions(result.Weeks);

            return result;
        }

        /// <summary>
        /// Calculate the weekly brekdown with existing values using the delivery type By Week By Daypart
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateCustomByWeekByDaypartWeeklyGoalBreadkdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks, List<DaypartDefaultWeightingGoal> daypartDefaults)
        {
            var result = new WeeklyBreakdownResponseDto();

            //remove deleted weeks
            _RemoveOutOfFlightWeeks(request.Weeks, weeks);

            //add the remain weeks
            result.Weeks.AddRange(request.Weeks);

            // Remove dayparts not in use from the remain weeks
            _RemoveDaypartsInTheExistingWeeks(request, result);

            // Recalculate fields in the existing weeks
            _RecalculateExistingWeeks(request, out bool redistributeCustom);

            // Add new dayparts in the remain weeks
            var remainMediaWeekIds = result.Weeks.Select(y => y.MediaWeekId).ToList();
            var remainWeeks = weeks.Where(x => remainMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewDaypartsInTheExistingWeeks(request, result, remainWeeks, daypartDefaults);

            // Add new weeks
            var oldMediaWeekIds = request.Weeks.Select(y => y.MediaWeekId).ToList();
            var newWeeks = weeks.Where(x => !oldMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewWeeksByDaypartToWeeklyBreakdownResult(result, newWeeks, request, daypartDefaults);

            //only adjust first week if redistributing
            if (result.Weeks.Where(w => w.NumberOfActiveDays > 0).Any() && redistributeCustom)
                _UpdateFirstWeekAndBudgetAdjustment(request, result.Weeks, request.TotalImpressions);

            // Recalculate percentage of week
            RecalculatePercentageOfWeekBasedOnImpressions(result.Weeks);

            return result;
        }

        /// <summary>
        /// Checks if the weeks have dayparts that are not in the request
        /// </summary>
        private void _RemoveDaypartsInTheExistingWeeks(WeeklyBreakdownRequest request, WeeklyBreakdownResponseDto response)
        {
            // Get the existing daypart ids in the existing weeks
            var existingWeekDaypartIds = response.Weeks.Select(w => w.DaypartCodeId.Value).Distinct();
            // Get the daypart ids in the request
            var requestDaypartIds = request.Dayparts.Select(w => w.DaypartCodeId).Distinct();

            // Remove dayparts that doesnt exist in the request
            var daypartsToRemove = existingWeekDaypartIds.Where(d => !requestDaypartIds.Contains(d)).ToList();
            if (daypartsToRemove.Any())
            {
                response.Weeks.RemoveAll(w => daypartsToRemove.Contains(w.DaypartCodeId.Value));
            }
        }

        /// <summary>
        /// Checks the weeks are missing dayparts in the request and add it
        /// </summary>
        private void _AddNewDaypartsInTheExistingWeeks(WeeklyBreakdownRequest request, 
            WeeklyBreakdownResponseDto response, 
            List<DisplayMediaWeek> existingWeeks,
            List<DaypartDefaultWeightingGoal> daypartDefaults)
        {
            // Get the existing daypart ids in the existing weeks
            var existingWeekDaypartIds = response.Weeks.Select(w => w.DaypartCodeId.Value).Distinct();

            // Add new dayparts to existin weeks
            var daypartsToAdd = daypartDefaults.Where(d => !existingWeekDaypartIds.Contains(d.DaypartDefaultId)).ToList();
            if (daypartsToAdd.Any())
            {
                _AddNewWeeksByDaypartToWeeklyBreakdownResult(response, existingWeeks, request, daypartsToAdd);
            }
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
            _CalculateGoalsForCustomByWeekByPercentageOfWeek(request, result.Weeks);
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
                        week.WeeklyImpressions = request.TotalRatings <= 0 ? 0 :
                            ProposalMath.RoundDownWithDecimals((week.WeeklyRatings / request.TotalRatings) * request.TotalImpressions, 0);
                        break;
                    case WeeklyBreakdownCalculationFrom.Percentage:
                        week.WeeklyImpressions = ProposalMath.RoundDownWithDecimals((week.WeeklyImpressionsPercentage / 100) * request.TotalImpressions, 0);
                        break;
                    case WeeklyBreakdownCalculationFrom.Units:
                        if (request.Equivalized)
                        {
                            if (week.SpotLengthId.HasValue)
                            {
                                week.WeeklyImpressions = Math.Round(_CalculateUnitImpressionsForSingleSpotLength(request.ImpressionsPerUnit
                                    , week.WeeklyUnits, week.SpotLengthId.Value));
                            }
                            else
                            {
                                week.WeeklyImpressions = Math.Round(_CalculateUnitImpressionsForMultipleSpotLengths(request.CreativeLengths
                                    , request.ImpressionsPerUnit, week.WeeklyUnits));
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

                _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                    , request.TotalBudget, week, week.WeeklyImpressions);
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

        private void _AddNewWeeksByDaypartToWeeklyBreakdownResult(
            WeeklyBreakdownResponseDto result, 
            List<DisplayMediaWeek> weeks, 
            WeeklyBreakdownRequest request, 
            List<DaypartDefaultWeightingGoal> daypartDefaults)
        {
            foreach (DisplayMediaWeek week in weeks)
            {
                var activeDays = _CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, out string activeDaysString);
                foreach (var item in daypartDefaults)
                {
                    result.Weeks.Add(new WeeklyBreakdownWeek
                    {
                        ActiveDays = activeDaysString,
                        NumberOfActiveDays = activeDays,
                        StartDate = week.WeekStartDate,
                        EndDate = week.WeekEndDate,
                        MediaWeekId = week.Id,
                        DaypartCodeId = item.DaypartDefaultId,
                        PercentageOfWeek = item.WeightingGoalPercent
                    });
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
        /// and then goes over each week and distributes its goals based on percentage of week (Custom By Week By Ad Length or Custom By Week By Daypart)
        /// </summary>
        private void _CalculateGoalsForCustomByWeekByPercentageOfWeek(
            WeeklyBreakdownRequest request,
            List<WeeklyBreakdownWeek> weeks)
        {
            var activeWeeks = weeks.Where(x => x.NumberOfActiveDays > 0);
            var activeWeekIds = activeWeeks.Select(x => x.MediaWeekId).Distinct().ToList();
            var impressionsByWeeks = _DistributeImpressionsByWeeks(request.TotalImpressions, activeWeekIds);

            foreach (var weekId in activeWeekIds)
            {
                var impressionsForWeek = impressionsByWeeks[weekId];
                var weeklyBreakdownInSameMediaWeek = activeWeeks.Where(x => x.MediaWeekId == weekId).ToList();

                foreach (var breakdownItem in weeklyBreakdownInSameMediaWeek)
                {
                    var weekFraction = breakdownItem.PercentageOfWeek.Value / 100;
                    var impressions = Math.Floor(impressionsForWeek * weekFraction);

                    _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                     , request.TotalBudget, breakdownItem, impressions);
                }

                _UpdateFirstWeekAndBudgetAdjustment(request, weeklyBreakdownInSameMediaWeek, impressionsForWeek);
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

                _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                    , request.TotalBudget, breakdownItem, impressions);
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

            _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                    , request.TotalBudget, firstWeek, impressions);
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

        public List<WeeklyBreakdownByWeek> GroupWeeklyBreakdownByWeek(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit = 0, List<CreativeLength> creativeLengths = null, bool equivalized = false)
        {
            return weeklyBreakdown
                .GroupBy(x => x.MediaWeekId)
                .Select(grouping =>
                {
                    var first = grouping.First();
                    var allItems = grouping.ToList();
                    double weeklyImpressions = allItems.Sum(x => x.WeeklyImpressions);
                    double unitsImpressions = allItems.Sum(x => x.UnitImpressions);

                    var week = new WeeklyBreakdownByWeek
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        Impressions = weeklyImpressions,
                        Budget = allItems.Sum(x => x.WeeklyBudget),
                        Units = unitsImpressions == 0
                            ? 0
                            : weeklyImpressions / unitsImpressions
                    };
                    if (!creativeLengths.IsNullOrEmpty())
                    {
                        week.Adu = _CalculateADU(impressionsPerUnit, allItems.Sum(x => x.AduImpressions), equivalized, null, creativeLengths);
                    }
                    return week;
                })
                .ToList();
        }

        public List<WeeklyBreakdownByWeekBySpotLength> GroupWeeklyBreakdownByWeekBySpotLength(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit, bool equivalized)
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
                        RatingPoints = allItems.Sum(x => x.WeeklyRatings),
                        Budget = allItems.Sum(x => x.WeeklyBudget),
                        Adu = _CalculateADU(impressionsPerUnit
                                , allItems.Sum(x => x.AduImpressions), equivalized, grouping.Key.SpotLengthId.Value),
                        Units = unitsImpressions == 0 ? 0 : weekImpressions / unitsImpressions
                    };
                }).ToList();
        }

        public List<WeeklyBreakdownByWeekByDaypart> GroupWeeklyBreakdownByWeekByDaypart(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit, bool equivalized, List<CreativeLength> creativeLengths)
        {
            return weeklyBreakdown
                .GroupBy(x => new { x.MediaWeekId, x.DaypartCodeId })
                .Select(grouping =>
                {
                    var first = grouping.First();
                    var allItems = grouping.ToList();
                    var weekImpressions = allItems.Sum(x => x.WeeklyImpressions);
                    var unitsImpressions = allItems.Sum(x => x.UnitImpressions);

                    return new WeeklyBreakdownByWeekByDaypart
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        DaypartCodeId = first.DaypartCodeId.Value,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        Impressions = weekImpressions,
                        Budget = allItems.Sum(x => x.WeeklyBudget),
                        Adu = _CalculateADU(impressionsPerUnit, allItems.Sum(x => x.AduImpressions), equivalized, null, creativeLengths),
                        Units = unitsImpressions == 0 ? 0 : weekImpressions / unitsImpressions
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
                DaypartCodeId = b.DaypartDefaultId,
                Weighting = GeneralMath.ConvertPercentageToFraction(a.Weight.Value) * GeneralMath.ConvertPercentageToFraction(b.WeightingGoalPercent)
            }).ToList();
            return allSpotLengthIdAndStandardDaypartIdCombinations;
        }

        /// <inheritdoc />
        public List<DaypartDefaultWeightingGoal> GetStandardDaypardWeightingGoals(List<PlanDaypartDto> dayparts)
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
                .Select(x => new DaypartDefaultWeightingGoal(x.DaypartCodeId,weightingGoalPercentByStandardDaypartIdDictionary[x.DaypartCodeId]))
                .ToList();
        }

        private double _CalculateEquivalizedImpressionsPerUnit(double impressionPerUnit, int spotLengthId)
            => impressionPerUnit * _SpotLengthMultiplier[spotLengthId];

        private double _CalculateUnitImpressionsForSingleSpotLength(double impressionPerUnit, double units, int spotLengthId)
            => units * _CalculateEquivalizedImpressionsPerUnit(impressionPerUnit, spotLengthId);

        private double _CalculateUnitImpressionsForMultipleSpotLengths(List<CreativeLength> creativeLengths
            , double impressionsPerUnit, double units)
            => units / creativeLengths.Sum(p => GeneralMath.ConvertPercentageToFraction(p.Weight.Value) 
                / _CalculateEquivalizedImpressionsPerUnit(impressionsPerUnit, p.SpotLengthId));

        private double _CalculateUnitsForSingleSpotLength(double impressionsPerUnit, double impressions, int spotLengthId)
            => impressions / _CalculateEquivalizedImpressionsPerUnit(impressionsPerUnit, spotLengthId);

        private double _CalculateUnitsForMultipleSpotLengths(List<CreativeLength> creativeLengths, double impressionsPerUnit, double impressions)
            => creativeLengths
                   .Sum(p => _CalculateUnitsForSingleSpotLength(impressionsPerUnit, impressions, p.SpotLengthId)
                            * GeneralMath.ConvertPercentageToFraction(p.Weight.Value));

        private void _CalculateUnits(List<WeeklyBreakdownWeek> weeks, WeeklyBreakdownRequest request)
        {
            foreach (var week in weeks)
            {
                if (request.Equivalized)
                {
                    if (week.SpotLengthId.HasValue)
                    {
                        week.WeeklyUnits = _CalculateUnitsForSingleSpotLength(request.ImpressionsPerUnit, week.WeeklyImpressions, week.SpotLengthId.Value);
                    }
                    else
                    {
                        if (request.CreativeLengths.Any(x => !x.Weight.HasValue))
                        {
                            request.CreativeLengths = _CreativeLengthEngine.DistributeWeight(request.CreativeLengths);
                        }
                        week.WeeklyUnits = _CalculateUnitsForMultipleSpotLengths(request.CreativeLengths, request.ImpressionsPerUnit, week.WeeklyImpressions);
                    }
                }
                else
                {
                    week.WeeklyUnits = week.WeeklyImpressions / request.ImpressionsPerUnit;
                }
            }
        }

        /// <inheritdoc/>
        public double CalculateWeeklyADUImpressions(WeeklyBreakdownWeek week, bool equivalized
            , double impressionsPerUnit, List<CreativeLength> creativeLengths)
        {
            if (equivalized)
            {
                if (week.SpotLengthId.HasValue)
                {
                    return _CalculateUnitImpressionsForSingleSpotLength(impressionsPerUnit, week.WeeklyAdu, week.SpotLengthId.Value);
                }
                else
                {
                    return _CalculateUnitImpressionsForMultipleSpotLengths(creativeLengths, impressionsPerUnit, week.WeeklyAdu);
                }
            }
            else
            {
                return week.WeeklyAdu * impressionsPerUnit;
            }
        }

        private int _CalculateADU(double impressionsPerUnit, double aduImpressions
            , bool equivalized, int? spotLengthId, List<CreativeLength> creativeLengths = null)
        {
            if (impressionsPerUnit == 0)
            {   //for older plans, where the user did not set an impressions per unit value, we need to show the user the ADU value based on the old math
                return (int)(aduImpressions / _DefaultImpressionsPerUnitForOldPlans);
            }
            if (equivalized)
            {
                if (spotLengthId.HasValue)
                {
                    return (int)_CalculateUnitsForSingleSpotLength(impressionsPerUnit, aduImpressions, spotLengthId.Value);
                }
                else
                {
                    return (int)_CalculateUnitsForMultipleSpotLengths(creativeLengths, impressionsPerUnit, aduImpressions);
                }
            }
            else
            {
                return (int)(aduImpressions / impressionsPerUnit);
            }
        }

        /// <inheritdoc/>
        public double CalculateADUWithDecimals(double impressionsPerUnit, double aduImpressions
            , bool equivalized, List<CreativeLength> creativeLengths = null)
        {
            if (impressionsPerUnit == 0)
            {   //for older plans, where the user did not set an impressions per unit value, we need to show the user the ADU value based on the old math
                return (int)(aduImpressions / _DefaultImpressionsPerUnitForOldPlans);
            }
            if (equivalized)
            {
                return _CalculateUnitsForMultipleSpotLengths(creativeLengths, impressionsPerUnit, aduImpressions);
            }
            else
            {
                return (aduImpressions / impressionsPerUnit);
            }
        }
    }
}
