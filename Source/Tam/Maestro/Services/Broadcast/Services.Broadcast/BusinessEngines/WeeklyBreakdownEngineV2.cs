using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.BusinessEngines
{
    public class WeeklyBreakdownEngineV2 : IWeeklyBreakdownEngine
    {
        private readonly IPlanValidator _PlanValidator;
        private readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;

        private readonly Lazy<Dictionary<int, double>> _SpotLengthDeliveryMultipliers;
        private readonly Lazy<Dictionary<int, decimal>> _SpotLengthCostMultipliers;

        //TODO: this needs to be removed after all plans in production have a modified version greater then 20.07 release date
        private const double _DefaultImpressionsPerUnitForOldPlans = 500000;
        private const string _UnsupportedDeliveryTypeMessage = "Unsupported Delivery Type";

        private readonly IStandardDaypartRepository _StandardDaypartRepository;

        private readonly Lazy<bool> _IsAduForPlanningv2Enabled;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public WeeklyBreakdownEngineV2(IPlanValidator planValidator,
                                         IMediaMonthAndWeekAggregateCache mediaWeekCache,
                                         ICreativeLengthEngine creativeLengthEngine,
                                         ISpotLengthEngine spotLengthEngine,
                                         IDataRepositoryFactory broadcastDataRepositoryFactory, IFeatureToggleHelper featureToggleHelper)
        {
            _PlanValidator = planValidator;
            _MediaWeekCache = mediaWeekCache;
            _CreativeLengthEngine = creativeLengthEngine;
            _SpotLengthEngine = spotLengthEngine;
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            _FeatureToggleHelper = featureToggleHelper;

            _SpotLengthDeliveryMultipliers = new Lazy<Dictionary<int, double>>(_GetSpotDeliveryMultipliers);
            _SpotLengthCostMultipliers = new Lazy<Dictionary<int, decimal>>(_GetSpotCostMultipliers);

            _IsAduForPlanningv2Enabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_ADU_FOR_PLANNING_V2));
        }

        private Dictionary<int, double> _GetSpotDeliveryMultipliers()
        {
            var result = _SpotLengthEngine.GetDeliveryMultipliers();
            return result;
        }

        private Dictionary<int, decimal> _GetSpotCostMultipliers()
        {
            var result = _SpotLengthEngine.GetCostMultipliers(applyInventoryPremium: false);
            return result;
        }

        public List<WeeklyBreakdownWeek> DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(
            PlanDto plan,
            double? customImpressionsGoal = null,
            decimal? customBudgetGoal = null)
        {
            var impressionsGoal = customImpressionsGoal ?? plan.TargetImpressions.Value;
            var budgetGoal = customBudgetGoal ?? plan.Budget.Value;

            if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.EvenDelivery ||
                plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeek)
            {
                return _DistributeGoals_ByWeekDeliveryType(plan, impressionsGoal, budgetGoal);
            }
            else if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                return _DistributeGoals_ByWeekByAdLengthDeliveryType(plan, impressionsGoal, budgetGoal);
            }
            else if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart)
            {
                return _DistributeGoals_ByWeekByDaypartDeliveryType(plan, impressionsGoal, budgetGoal);
            }
            else
            {
                throw new ApplicationException(_UnsupportedDeliveryTypeMessage);
            }
        }

        /// <summary>
        /// Splits each weekly breakdown record using ad length weighting goals to distribute the goals of each record
        /// </summary>
        /// <returns>A list of breakdown records with 'by week by ad length by daypart' structure</returns>
        private List<WeeklyBreakdownWeek> _DistributeGoals_ByWeekByDaypartDeliveryType(
            PlanDto plan,
            double impressionsGoal,
            decimal budgetGoal)
        {
            var result = new List<WeeklyBreakdownWeek>();

            var weeks = GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks);

            foreach (var weeklyBreakdown in plan.WeeklyBreakdownWeeks)
            {
                var aduImpressionsForBreakdownItem = CalculateWeeklyADUImpressions(
                    weeklyBreakdown,
                    plan.Equivalized,
                    plan.ImpressionsPerUnit.Value,
                    plan.CreativeLengths);               

                var unitsImpressionsForBreakdownItem = weeklyBreakdown.WeeklyUnits == 0 ? 0 : weeklyBreakdown.WeeklyImpressions / weeklyBreakdown.WeeklyUnits;
                var week = weeks.Single(w => w.MediaWeekId == weeklyBreakdown.MediaWeekId);

                // The given weeks is grouped by Week and Daypart with the Daypart weights distributed.
                // Here we are "manufacturing" the Spot Length records in the grid and thus Manufacturing them.
                // we will use the weights at the plan level to do this.
                foreach (var distributedSpotLength in plan.CreativeLengths)
                {
                    var weighting = GeneralMath.ConvertPercentageToFraction(distributedSpotLength.Weight.GetValueOrDefault());

                    var itemAduImpressions = aduImpressionsForBreakdownItem * weighting;
                    var itemWeeklyAdu = weeklyBreakdown.WeeklyAdu;
                    if (_IsAduForPlanningv2Enabled.Value)
                    {
                        itemWeeklyAdu = Convert.ToInt32(itemAduImpressions);
                    }

                    var newWeeklyBreakdownItem = new WeeklyBreakdownWeek
                    {
                        WeekNumber = weeklyBreakdown.WeekNumber,
                        MediaWeekId = weeklyBreakdown.MediaWeekId,
                        StartDate = weeklyBreakdown.StartDate,
                        EndDate = weeklyBreakdown.EndDate,
                        NumberOfActiveDays = weeklyBreakdown.NumberOfActiveDays,
                        ActiveDays = weeklyBreakdown.ActiveDays,
                        SpotLengthId = distributedSpotLength.SpotLengthId,
                        SpotLengthDuration = _GetWeeklySpotLengthDuration(distributedSpotLength.SpotLengthId),
                        DaypartCodeId = weeklyBreakdown.DaypartCodeId,
                        AduImpressions = itemAduImpressions,
                        UnitImpressions = unitsImpressionsForBreakdownItem * weighting,
                        IsLocked = weeklyBreakdown.IsLocked,
                        DaypartOrganizationId = weeklyBreakdown.DaypartOrganizationId,
                        CustomName = weeklyBreakdown.CustomName,
                        DaypartOrganizationName = weeklyBreakdown.DaypartOrganizationName,
                        PlanDaypartId = weeklyBreakdown.PlanDaypartId,
                        WeeklyAdu = itemWeeklyAdu
                    };

                    var impressions = weeklyBreakdown.WeeklyImpressions * weighting;
                    if (plan.TargetRatingPoints.HasValue)
                    {
                        _UpdateGoalsForWeeklyBreakdownItem(
                        impressionsGoal,
                        plan.TargetRatingPoints.Value,
                        budgetGoal,
                        newWeeklyBreakdownItem,
                        impressions,
                        roundRatings: false);
                    }
                    newWeeklyBreakdownItem.PercentageOfWeek = _CalculatePercentageOfWeek(impressions, week.Impressions);

                    result.Add(newWeeklyBreakdownItem);
                }

            }
            return result;
        }

        /// <summary>
        /// Splits each weekly breakdown record using plan daypart weighting goals to distribute the goals of each record
        /// </summary>
        /// <returns>A list of breakdown records with 'by week by ad length by daypart' structure</returns>
        private List<WeeklyBreakdownWeek> _DistributeGoals_ByWeekByAdLengthDeliveryType(
            PlanDto plan,
            double impressionsGoal,
            decimal budgetGoal)
        {
            var result = new List<WeeklyBreakdownWeek>();
            var weeks = GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks);
            var standardDaypardWeightingGoals = PlanGoalHelper.GetStandardDaypardWeightingGoals(plan.Dayparts);

            foreach (var week in weeks)
            {
                foreach (var breakdownItem in plan.WeeklyBreakdownWeeks.Where(x => x.MediaWeekId == week.MediaWeekId))
                {
                    double aduImpressionsForBreakdownItem = CalculateWeeklyADUImpressions(
                        breakdownItem,
                        plan.Equivalized,
                        plan.ImpressionsPerUnit.Value,
                        plan.CreativeLengths);

                    double unitsImpressionsForBreakdownItem = breakdownItem.WeeklyUnits == 0 ? 0 : breakdownItem.WeeklyImpressions / breakdownItem.WeeklyUnits;

                    // The given weeks is grouped by Week and Spot Length with the Spot Length weights distributed.
                    // Here we are "manufacturing" the Daypart records in the grid and thus Manufacturing them.
                    // we will use the weights at the plan level to do this.
                    foreach (var item in standardDaypardWeightingGoals)
                    {
                        var weighting = GeneralMath.ConvertPercentageToFraction(item.WeightingGoalPercent);

                        var itemAduImpressions = aduImpressionsForBreakdownItem * weighting;
                        var itemWeeklyAdu = week.Adu;
                        if (_IsAduForPlanningv2Enabled.Value)
                        {
                            itemWeeklyAdu = Convert.ToInt32(itemAduImpressions);
                        }

                        var newWeeklyBreakdownItem = new WeeklyBreakdownWeek
                        {
                            WeekNumber = week.WeekNumber,
                            MediaWeekId = week.MediaWeekId,
                            StartDate = week.StartDate,
                            EndDate = week.EndDate,
                            NumberOfActiveDays = week.NumberOfActiveDays,
                            ActiveDays = week.ActiveDays,
                            SpotLengthId = breakdownItem.SpotLengthId,
                            SpotLengthDuration = _GetWeeklySpotLengthDuration(breakdownItem.SpotLengthId),
                            DaypartCodeId = item.StandardDaypartId,
                            AduImpressions = itemAduImpressions,
                            UnitImpressions = unitsImpressionsForBreakdownItem * weighting,
                            IsLocked = week.IsLocked,
                            CustomName = item.CustomName,
                            DaypartOrganizationName = item.DaypartOrganizationName,
                            DaypartOrganizationId = item.DaypartOrganizationId,
                            PlanDaypartId = item.PlanDaypartId,
                            WeeklyAdu = itemWeeklyAdu
                        };

                        var impressions = Math.Floor(breakdownItem.WeeklyImpressions * weighting);
                        if (plan.TargetRatingPoints.HasValue)
                        {
                            _UpdateGoalsForWeeklyBreakdownItem(
                            impressionsGoal,
                            plan.TargetRatingPoints.Value,
                            budgetGoal,
                            newWeeklyBreakdownItem,
                            impressions,
                            roundRatings: false);
                        }
                        newWeeklyBreakdownItem.PercentageOfWeek = _CalculatePercentageOfWeek(impressions, week.Impressions);

                        result.Add(newWeeklyBreakdownItem);
                    }
                }
            }

            _CalculateUnits(result, plan.Equivalized, plan.ImpressionsPerUnit.Value, plan.CreativeLengths);
            _AdjustSpotLengthBudget(result, plan.GoalBreakdownType, plan.Equivalized, plan.Budget.Value);

            return result;
        }

        /// <summary>
        /// Splits each weekly breakdown record using plan daypart and ad length weighting goals to distribute the goals of each record
        /// </summary>
        /// <returns>A list of breakdown records with 'by week by ad length by daypart' structure</returns>
        private List<WeeklyBreakdownWeek> _DistributeGoals_ByWeekDeliveryType(
            PlanDto plan,
            double impressionsGoal,
            decimal budgetGoal)
        {
            var result = new List<WeeklyBreakdownWeek>();

            var allSpotLengthIdAndStandardDaypartIdCombinations = PlanGoalHelper.GetWeeklyBreakdownCombinations(plan.CreativeLengths, plan.Dayparts);

            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                var weeklyAduImpressions = CalculateWeeklyADUImpressions(
                    week,
                    plan.Equivalized,
                    plan.ImpressionsPerUnit.Value,
                    plan.CreativeLengths);                

                var unitsImpressions = week.WeeklyUnits == 0 ? 0 : week.WeeklyImpressions / week.WeeklyUnits;

                foreach (var combination in allSpotLengthIdAndStandardDaypartIdCombinations)
                {
                    var itemAduImpressions = weeklyAduImpressions * combination.Weighting;
                    var itemWeeklyAdu = week.WeeklyAdu;
                    if (_IsAduForPlanningv2Enabled.Value)
                    {
                        itemWeeklyAdu = Convert.ToInt32(itemAduImpressions);
                    }

                    var newWeeklyBreakdownItem = new WeeklyBreakdownWeek
                    {
                        WeekNumber = week.WeekNumber,
                        MediaWeekId = week.MediaWeekId,
                        StartDate = week.StartDate,
                        EndDate = week.EndDate,
                        NumberOfActiveDays = week.NumberOfActiveDays,
                        ActiveDays = week.ActiveDays,
                        SpotLengthId = combination.SpotLengthId,
                        SpotLengthDuration = _GetWeeklySpotLengthDuration(combination.SpotLengthId),
                        DaypartCodeId = combination.DaypartCodeId,
                        AduImpressions = itemAduImpressions,
                        UnitImpressions = unitsImpressions * combination.Weighting,
                        IsLocked = week.IsLocked,
                        CustomName = combination.CustomName,
                        DaypartOrganizationName = combination.DaypartOrganizationName,
                        DaypartOrganizationId = combination.DaypartOrganizationId,
                        PlanDaypartId = combination.PlanDaypartId,
                        WeeklyAdu = itemWeeklyAdu
                    };

                    var impressions = week.WeeklyImpressions * combination.Weighting;
                    if (plan.TargetRatingPoints.HasValue)
                    {
                        _UpdateGoalsForWeeklyBreakdownItem(
                        impressionsGoal,
                        plan.TargetRatingPoints.Value,
                        budgetGoal,
                        newWeeklyBreakdownItem,
                        impressions,
                        roundRatings: false);
                    }
                    newWeeklyBreakdownItem.PercentageOfWeek = _CalculatePercentageOfWeek(impressions, week.WeeklyImpressions);

                    result.Add(newWeeklyBreakdownItem);
                }
            }
            return result;
        }

        public List<WeeklyBreakdownWeek> GroupWeeklyBreakdownWeeksBasedOnDeliveryType(PlanDto plan)
        {
            if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.EvenDelivery ||
                plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeek)
            {
                return _GroupWeeklyBreakdownWeeks_ByWeekDeliveryType(plan);
            }
            else if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                return _GroupWeeklyBreakdownWeeks_ByWeekByAdLengthDeliveryType(plan);
            }
            else if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart)
            {
                return _GroupWeeklyBreakdownWeeks_ByWeekByDaypartDeliveryType(plan);
            }
            else
            {
                throw new ApplicationException(_UnsupportedDeliveryTypeMessage);
            }
        }

        private List<WeeklyBreakdownWeek> _GroupWeeklyBreakdownWeeks_ByWeekByDaypartDeliveryType(PlanDto plan)
        {
            int planDaypartId = 0;
            var result = new List<WeeklyBreakdownWeek>();
            var weeklyBreakdownByWeekByDaypart = GroupWeeklyBreakdownByWeekByDaypart(plan.WeeklyBreakdownWeeks, plan.ImpressionsPerUnit.Value, plan.Equivalized, plan.CreativeLengths);

            foreach (var item in weeklyBreakdownByWeekByDaypart)
            {
                var planDaypart = plan.Dayparts.FirstOrDefault(x => x.DaypartUniquekey == item.DaypartUniquekey);
                if (planDaypart != null)
                {
                    planDaypartId = planDaypart.PlanDaypartId;
                }
                var newWeeklyBreakdownItem = new WeeklyBreakdownWeek
                {
                    WeekNumber = item.WeekNumber,
                    MediaWeekId = item.MediaWeekId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    NumberOfActiveDays = item.NumberOfActiveDays,
                    ActiveDays = item.ActiveDays,
                    DaypartCodeId = item.DaypartCodeId,
                    WeeklyAdu = item.Adu,
                    WeeklyUnits = item.Units,
                    IsLocked = item.IsLocked,
                    DaypartOrganizationId = item.DaypartOrganizationId,
                    CustomName = item.CustomName,
                    DaypartOrganizationName = item.DaypartOrganizationName,
                    PlanDaypartId = planDaypartId
                };

                if (_IsAduForPlanningv2Enabled.Value)
                {
                    newWeeklyBreakdownItem.AduImpressions = item.Adu;
                }

                if (plan.TargetImpressions.HasValue && plan.TargetRatingPoints.HasValue && plan.Budget.HasValue)
                {
                    _UpdateGoalsForWeeklyBreakdownItem(
                       plan.TargetImpressions.Value,
                       plan.TargetRatingPoints.Value,
                       plan.Budget.Value,
                       newWeeklyBreakdownItem,
                       item.Impressions,
                       roundRatings: true);
                }
                result.Add(newWeeklyBreakdownItem);
            }

            RecalculatePercentageOfWeekBasedOnImpressions(result);

            return result;
        }

        /// <summary>
        /// Groups weekly breakdown by week by ad length
        /// </summary>
        private List<WeeklyBreakdownWeek> _GroupWeeklyBreakdownWeeks_ByWeekByAdLengthDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();
            var weeklyBreakdownByWeekBySpotLength = GroupWeeklyBreakdownByWeekBySpotLength(plan.WeeklyBreakdownWeeks, plan.ImpressionsPerUnit.Value, plan.Equivalized);

            foreach (var item in weeklyBreakdownByWeekBySpotLength)
            {
                var newWeeklyBreakdownItem = new WeeklyBreakdownWeek
                {
                    WeekNumber = item.WeekNumber,
                    MediaWeekId = item.MediaWeekId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    NumberOfActiveDays = item.NumberOfActiveDays,
                    ActiveDays = item.ActiveDays,
                    SpotLengthId = item.SpotLengthId,
                    SpotLengthDuration = _GetWeeklySpotLengthDuration(item.SpotLengthId),
                    WeeklyAdu = item.Adu,
                    WeeklyUnits = item.Units,
                    IsLocked = item.IsLocked
                };

                if (_IsAduForPlanningv2Enabled.Value)
                {
                    newWeeklyBreakdownItem.AduImpressions = item.Adu;
                }

                if (plan.TargetImpressions.HasValue && plan.TargetRatingPoints.HasValue && plan.Budget.HasValue)
                {
                    _UpdateGoalsForWeeklyBreakdownItem(
                    plan.TargetImpressions.Value,
                    plan.TargetRatingPoints.Value,
                    plan.Budget.Value,
                    newWeeklyBreakdownItem,
                    item.Impressions,
                    roundRatings: true);
                }
                result.Add(newWeeklyBreakdownItem);
            }

            RecalculatePercentageOfWeekBasedOnImpressions(result);
            _AdjustSpotLengthBudget(result, plan.GoalBreakdownType, plan.Equivalized, plan.Budget.Value);

            return result;
        }

        /// <summary>
        /// Groups weekly breakdown by week
        /// </summary>
        private List<WeeklyBreakdownWeek> _GroupWeeklyBreakdownWeeks_ByWeekDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();
            var weeks = GroupWeeklyBreakdownByWeek(
                plan.WeeklyBreakdownWeeks,
                plan.ImpressionsPerUnit.Value,
                plan.CreativeLengths,
                plan.Equivalized);

            foreach (var week in weeks)
            {
                var newWeeklyBreakdownItem = new WeeklyBreakdownWeek
                {
                    WeekNumber = week.WeekNumber,
                    MediaWeekId = week.MediaWeekId,
                    StartDate = week.StartDate,
                    EndDate = week.EndDate,
                    NumberOfActiveDays = week.NumberOfActiveDays,
                    ActiveDays = week.ActiveDays,
                    WeeklyAdu = week.Adu,
                    WeeklyUnits = week.Units,
                    IsLocked = week.IsLocked
                };

                if (_IsAduForPlanningv2Enabled.Value)
                {
                    newWeeklyBreakdownItem.AduImpressions = week.Adu;
                }

                if (plan.TargetImpressions.HasValue && plan.TargetRatingPoints.HasValue && plan.Budget.HasValue)
                {
                    _UpdateGoalsForWeeklyBreakdownItem(
                       plan.TargetImpressions.Value,
                       plan.TargetRatingPoints.Value,
                       plan.Budget.Value,
                       newWeeklyBreakdownItem,
                       week.Impressions, roundRatings: true);
                }
                result.Add(newWeeklyBreakdownItem);
            }

            return result;
        }

        private bool _IsDeliveryTypeChangeRequest(WeeklyBreakdownRequest request)
        {
            var isDeliveryTypeChangeRequest = _IsRawWeeklyBreakdownRequest(request);
            return isDeliveryTypeChangeRequest;
        }

        private bool _IsRawWeeklyBreakdownRequest(WeeklyBreakdownRequest request)
        {
            // If all week items are present then this is a raw weekly breakdown.
            var hasSpotLengths = request.Weeks.Any(w => w.SpotLengthId.HasValue);
            if (!hasSpotLengths)
            {
                return false;
            }

            var hasDayparts = request.Weeks.Any(w => w.DaypartCodeId.HasValue);
            if (!hasDayparts)
            {
                return false;
            }

            return true;
        }

        private List<WeeklyBreakdownWeek> _CalculateResponseWeeksForDeliveryTypeChange(WeeklyBreakdownRequest request, PlanGoalBreakdownTypeEnum deliveryType)
        {
            // determine up front if we need to worry about handling a remainder.
            var incomingTotalImpressions = request.Weeks.Sum(w => w.WeeklyImpressions);
            var incomingHasRemainder = (request.TotalImpressions - incomingTotalImpressions) > 0;
            var shouldDistributeRemainder = !incomingHasRemainder;

            // group to the target delivery type without changing the distributions
            var plan = new PlanDto
            {
                GoalBreakdownType = deliveryType,
                CreativeLengths = request.CreativeLengths,
                Dayparts = request.Dayparts,
                WeeklyBreakdownWeeks = request.Weeks,
                TargetRatingPoints = request.TotalRatings,
                Budget = request.TotalBudget,
                Equivalized = request.Equivalized,
                ImpressionsPerUnit = request.ImpressionsPerUnit,
                TargetImpressions = request.TotalImpressions,
                FlightDays = request.FlightDays,
                FlightEndDate = request.FlightEndDate,
                FlightHiatusDays = request.FlightHiatusDays
            };
            var groupedWeeks = GroupWeeklyBreakdownWeeksBasedOnDeliveryType(plan);

            // post processing aligns with the other global recalculations.
            foreach (var week in groupedWeeks)
            {
                // use Math.Round in this context.  Math.Floor lost impressions in many weeks.
                var roundedWeeklyImpressions = Math.Floor(week.WeeklyImpressions);
                week.WeeklyImpressions = roundedWeeklyImpressions;
            }

            // handle the remainder
            // when the "unseen" were calculated it could result in a remainder.
            if (shouldDistributeRemainder)
            {
                var totalExistingImpressions = groupedWeeks.Sum(w => w.WeeklyImpressions);
                var impressionsRemainder = request.TotalImpressions - totalExistingImpressions;
                if (impressionsRemainder > 0)
                {
                    _UpdateFirstWeekAndBudgetAdjustment(request, groupedWeeks, request.TotalImpressions);
                }
            }

            return groupedWeeks;
        }

        /// <inheritdoc/>
        public WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request)
        {
            /*** Validate the input ***/
            _PlanValidator.ValidateWeeklyBreakdown(request);

            // validate the weights
            var weightedSpotLengths = _CreativeLengthEngine.DistributeWeight(request.CreativeLengths);
            var weightedDayparts = PlanGoalHelper.GetStandardDaypardWeightingGoals(request.Dayparts);
            _PlanValidator.ValidateWeeklyBreakdownItemWeights(weightedDayparts, weightedSpotLengths, request.DeliveryType);

            if (_IsAduForPlanningv2Enabled.Value)
            {
                foreach (var week in request.Weeks)
                {
                    week.AduImpressions = week.WeeklyAdu;
                }
            }

            if (!request.IsAduOnly)
            {
                if (request.ImpressionsPerUnit <= 0) //old plan
                {
                    if (request.TotalImpressions < _DefaultImpressionsPerUnitForOldPlans)
                        request.ImpressionsPerUnit = request.TotalImpressions;
                    else
                        request.ImpressionsPerUnit = _DefaultImpressionsPerUnitForOldPlans;
                }

                _PlanValidator.ValidateImpressionsPerUnit(request.ImpressionsPerUnit, request.TotalImpressions);
            }

            /*** Prepare to calculate ***/

            // apply the fully defined and validated weights on the request.
            request.CreativeLengths = weightedSpotLengths;
            try
            {
                request.Dayparts.ForEach(d =>
                {
                    d.WeightingGoalPercent = weightedDayparts.Single(w => w.DaypartUniquekey == d.DaypartUniquekey).WeightingGoalPercent;
                });
            }
            catch (Exception)
            {
                throw new CadentException("Same Daypart with same Organization and Name are not allowed");
            }

            //calculate flight weeks based on start/end date of the flight
            List<DisplayMediaWeek> weeks = _MediaWeekCache.GetDisplayMediaWeekByFlight(request.FlightStartDate, request.FlightEndDate);

            //add all the days outside of the flight for the first and last week as hiatus days
            request.FlightHiatusDays.AddRange(_GetDaysOutsideOfTheFlight(request.FlightStartDate, request.FlightEndDate, weeks));

            /*** Determine the Type of Request ***/

            // determine what state we are going towards.
            var isInitialLoad = false;
            var isDeliveryTypeChange = false;

            isInitialLoad = request.Weeks.IsEmpty();

            if (!isInitialLoad)
            {
                isDeliveryTypeChange = _IsDeliveryTypeChangeRequest(request);
            }

            /*** Calculate per the Delivery Type  ***/
            WeeklyBreakdownResponseDto response;

            if (_IsAduForPlanningv2Enabled.Value)
            {
                response = _DoCalculateResponse_V2(request, weeks, weightedSpotLengths, weightedDayparts, isDeliveryTypeChange, isInitialLoad);
            }
            else
            {
                response = _DoCalculateResponse_V1(request, weeks, weightedSpotLengths, weightedDayparts, isDeliveryTypeChange, isInitialLoad);
            }

            /*** Calculate what is not Delivery Type specific  ***/
            _CalculateUnits(response.Weeks, request.Equivalized, request.ImpressionsPerUnit, request.CreativeLengths);
            _AdjustSpotLengthBudget(response.Weeks, request.DeliveryType, request.Equivalized, request.TotalBudget);
            _CalculateWeeklyGoalBreakdownTotals(response, request);
            SetWeekNumberAndSpotLengthDuration(response.Weeks);
            _OrderWeeks(request, response);
            _AddDaypartToWeeklyBreakdownResult(request, response);
            RecalculatePercentageOfWeekBasedOnImpressions(response.Weeks);

            /*** Get the Weekly Breakdown ***/
            if (isDeliveryTypeChange && request.DeliveryType != PlanGoalBreakdownTypeEnum.EvenDelivery)
            {
                // pass these back as they were the raw weekly breakdown
                response.RawWeeklyBreakdownWeeks = request.Weeks;
            }
            else
            {
                response.RawWeeklyBreakdownWeeks = _PopulateRawWeeklyBreakdownWeeks(request, response.Weeks);
            }

            return response;
        }        

        private WeeklyBreakdownResponseDto _DoCalculateResponse_V2(
            WeeklyBreakdownRequest request,
            List<DisplayMediaWeek> weeks,
            List<CreativeLength> weightedSpotLengths,
            List<StandardDaypartWeightingGoal> weightedDayparts, 
            bool isDeliveryTypeChange, bool isInitialLoad
            )
        {
            WeeklyBreakdownResponseDto response;

            if (isDeliveryTypeChange)
            {
                var responseWeeks = _CalculateResponseWeeksForDeliveryTypeChange(request, request.DeliveryType);
                response = new WeeklyBreakdownResponseDto { Weeks = responseWeeks };
            }
            else if (request.DeliveryType == PlanGoalBreakdownTypeEnum.EvenDelivery)
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
                if (isInitialLoad)
                    response = _CalculateInitialCustomByWeekByAdLengthPlanWeeklyGoalBreakdown(request, weeks, weightedSpotLengths);
                else
                    response = _CalculateCustomByWeekByAdLengthPlanWeeklyGoalBreakdown(request, weeks, weightedSpotLengths);
            }
            else if (request.DeliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart)
            {
                if (isInitialLoad)
                    response = _CalculateInitialCustomByWeekByDaypartWeeklyGoalBreadkdown(request, weeks, weightedDayparts);
                else
                    response = _CalculateCustomByWeekByDaypartWeeklyGoalBreadkdown(request, weeks, weightedDayparts);
            }
            else
            {
                throw new CadentException(_UnsupportedDeliveryTypeMessage);
            }

            return response;
        }

        private WeeklyBreakdownResponseDto _DoCalculateResponse_V1(
            WeeklyBreakdownRequest request,
            List<DisplayMediaWeek> weeks,
            List<CreativeLength> weightedSpotLengths,
            List<StandardDaypartWeightingGoal> weightedDayparts,
            bool isDeliveryTypeChange, bool isInitialLoad
            )
        {
            WeeklyBreakdownResponseDto response;
            
            if (request.DeliveryType == PlanGoalBreakdownTypeEnum.EvenDelivery)
            {
                response = _CalculateEvenDeliveryPlanWeeklyGoalBreakdown(request, weeks);
            }
            else if (isDeliveryTypeChange)
            {
                var responseWeeks = _CalculateResponseWeeksForDeliveryTypeChange(request, request.DeliveryType);
                response = new WeeklyBreakdownResponseDto { Weeks = responseWeeks };
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
                if (isInitialLoad)
                    response = _CalculateInitialCustomByWeekByAdLengthPlanWeeklyGoalBreakdown(request, weeks, weightedSpotLengths);
                else
                    response = _CalculateCustomByWeekByAdLengthPlanWeeklyGoalBreakdown(request, weeks, weightedSpotLengths);
            }
            else if (request.DeliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart)
            {
                if (isInitialLoad)
                    response = _CalculateInitialCustomByWeekByDaypartWeeklyGoalBreadkdown(request, weeks, weightedDayparts);
                else
                    response = _CalculateCustomByWeekByDaypartWeeklyGoalBreadkdown(request, weeks, weightedDayparts);
            }
            else
            {
                throw new CadentException(_UnsupportedDeliveryTypeMessage);
            }

            return response;
        }

        public WeeklyBreakdownResponseDto ClearPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request)
        {
            var result = new WeeklyBreakdownResponseDto();

            if (request.DeliveryType == PlanGoalBreakdownTypeEnum.EvenDelivery)
            {
                request.DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek;
            }
                foreach (var week in request.Weeks)
                {
                    if (!week.IsLocked)
                    {
                        week.WeeklyBudget = 0;
                        week.WeeklyUnits = 0;
                        week.WeeklyRatings = 0;
                        week.WeeklyImpressions = 0;
                        week.WeeklyImpressionsPercentage = 0;
                        week.WeeklyAdu = 0;
                        week.AduImpressions = 0;
                        week.IsUpdated = false;
                    }
                }

            var totalImpressionsPercentage = 0d;
            foreach (var week in request.Weeks)
            {
                var weeklyRatio = week.WeeklyImpressions / request.TotalImpressions;
                totalImpressionsPercentage += 100 * weeklyRatio;
            }

            result.Weeks.AddRange(request.Weeks);
            result.TotalActiveDays = request.Weeks.Sum(x => x.NumberOfActiveDays);
            result.TotalBudget = request.Weeks.Sum(x => x.WeeklyBudget);
            result.TotalUnits = request.Weeks.Sum(x => x.WeeklyUnits);
            result.TotalImpressions = request.Weeks.Sum(x => x.WeeklyImpressions);
            result.TotalRatingPoints = request.Weeks.Sum(x => x.WeeklyRatings);
            result.TotalImpressionsPercentage = totalImpressionsPercentage;
            result.TotalAduImpressions = request.Weeks.Sum(x => x.AduImpressions);

            result.RawWeeklyBreakdownWeeks = _PopulateRawWeeklyBreakdownWeeks(request, result.Weeks);
            foreach (var rawWeek in result.RawWeeklyBreakdownWeeks)
            {
                rawWeek.WeeklyAdu = result.Weeks.Where(x => x.WeekNumber == rawWeek.WeekNumber).Select(x => x.WeeklyAdu).FirstOrDefault();
            }
            return result;
        }

        private void _AdjustSpotLengthBudget(List<WeeklyBreakdownWeek> weeks, PlanGoalBreakdownTypeEnum deliveryType,
            bool? equivalized, decimal totalBudget)
        {
            if (deliveryType != PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                return;
            }

            if (equivalized ?? false)
            {
                return;
            }

            var spotCostMultipliers = _SpotLengthCostMultipliers.Value;
            var baseCostDenom = weeks.Sum(w => Convert.ToDecimal(w.WeeklyUnits) * spotCostMultipliers[w.SpotLengthId.Value]);
            var baseCost = baseCostDenom > 0 ? totalBudget / (baseCostDenom) : 0;
            foreach (var week in weeks)
            {
                if (!week.IsLocked)
                {
                    var costPerUnit = baseCost * spotCostMultipliers[week.SpotLengthId.Value];
                    week.WeeklyBudget = Convert.ToDecimal(week.WeeklyUnits) * costPerUnit;
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

            _AddNewWeeksToWeeklyBreakdownResult(result.Weeks, weeks, request);
            _CalculateGoalsForEvenDeliveryType(request, result.Weeks);

            return result;
        }

        /// <summary>
        /// Calculate the initial load of weekly brekdown using the delivery type By Week By Daypart
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateInitialCustomByWeekByDaypartWeeklyGoalBreadkdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks, List<StandardDaypartWeightingGoal> standardDayparts)
        {
            var result = new WeeklyBreakdownResponseDto();

            _AddNewWeeksByDaypartToWeeklyBreakdownResult(result.Weeks, weeks, request, standardDayparts);
            _CalculateGoalsForCustomByWeekByPercentageOfWeek(request, result.Weeks);
            RecalculatePercentageOfWeekBasedOnImpressions(result.Weeks);

            return result;
        }

        /// <summary>
        /// Calculate the weekly brekdown with existing values using the delivery type By Week By Daypart
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateCustomByWeekByDaypartWeeklyGoalBreadkdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks, List<StandardDaypartWeightingGoal> standardDayparts)
        {
            var resultWeeks = request.Weeks;

            // Handle removal of weeks and week items.
            _RemoveOutOfFlightWeeks(request.Weeks, weeks);
            _RemoveDaypartsInTheExistingWeeks(request.Weeks, standardDayparts);

            // Handle week item additions
            var existingMediaWeekIds = resultWeeks.Select(y => y.MediaWeekId).ToList();
            var existingWeeks = weeks.Where(x => existingMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewDaypartsInTheExistingWeeks(request, resultWeeks, existingWeeks, standardDayparts);

            // distribute impressions
            // do this after week item additions because they contribute to the item weights within the week.
            _RecalculateExistingWeeks(request, out bool redistributeCustom);

            // Handle the impressions distribution 
            var shouldRedistribute = _ShouldRedistribute(resultWeeks, redistributeCustom);
            if (shouldRedistribute)
            {
                // add the remainder to the first week
                _UpdateFirstWeekAndBudgetAdjustment(request, resultWeeks, request.TotalImpressions);
            }

            // Handle adding weeks to the flight - we want these to be empty.
            var oldMediaWeekIds = resultWeeks.Select(y => y.MediaWeekId).ToList();
            var newWeeks = weeks.Where(x => !oldMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewWeeksByDaypartToWeeklyBreakdownResult(resultWeeks, newWeeks, request, standardDayparts);

            var result = new WeeklyBreakdownResponseDto
            {
                Weeks = resultWeeks
            };

            return result;
        }

        /// <summary>
        /// Checks if the weeks have dayparts that are not in the request
        /// </summary>
        private void _RemoveDaypartsInTheExistingWeeks(List<WeeklyBreakdownWeek> weeks, List<StandardDaypartWeightingGoal> dayparts)
        {
            // Get the existing daypart ids in the existing weeks
            var existingWeekDaypartIds = weeks.Select(w => w.DaypartUniquekey).Distinct();
            // Get the daypart ids in the request
            var requestDaypartIds = dayparts.Select(w => w.DaypartUniquekey).Distinct();

            // Remove dayparts that doesnt exist in the request
            var daypartsToRemove = existingWeekDaypartIds.Where(d => !requestDaypartIds.Contains(d)).ToList();
            if (daypartsToRemove.Any())
            {
                weeks.RemoveAll(w => daypartsToRemove.Contains(w.DaypartUniquekey));
            }
        }

        /// <summary>
        /// Checks the weeks are missing dayparts in the request and add it
        /// </summary>
        private void _AddNewDaypartsInTheExistingWeeks(WeeklyBreakdownRequest request,
            List<WeeklyBreakdownWeek> resultWeeks,
            List<DisplayMediaWeek> existingWeeks,
            List<StandardDaypartWeightingGoal> standardDayparts)
        {
            // flesh out the item values that contribute to the unique key.
            foreach (var week in resultWeeks)
            {
                var itemRefCode = standardDayparts.FirstOrDefault(d => d.PlanDaypartId == week.PlanDaypartId);
                if (itemRefCode != null)
                {
                    week.DaypartCodeId = itemRefCode.StandardDaypartId;
                    week.CustomName = itemRefCode.CustomName;
                    week.DaypartOrganizationId = itemRefCode.DaypartOrganizationId;
                }
            }
            // Get the existing daypart ids in the existing weeks
            var existingWeekDaypartIds = resultWeeks.Select(w => w.DaypartUniquekey).Distinct();

            // Add new dayparts to existin week
            if (!existingWeekDaypartIds.Contains("0||"))
            {
                var daypartsToAdd = standardDayparts.Where(d => !existingWeekDaypartIds.Contains(d.DaypartUniquekey)).ToList();
                if (daypartsToAdd.Any())
                {
                    _AddNewWeeksByDaypartToWeeklyBreakdownResult(resultWeeks, existingWeeks, request, daypartsToAdd);
                }
            }
        }

        /// <summary>
        /// Removes records that are out of flight
        /// Either recalculates goals based on the recalculation type when 1 record is updated or recalculates all records proportionally
        /// Adds new flight weeks with zero goals
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateCustomByWeekPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks)
        {
            var resultWeeks = request.Weeks;

            // Handle removal of weeks
            _RemoveOutOfFlightWeeks(request.Weeks, weeks);

            // distribute impressions
            _RecalculateExistingWeeks(request, out bool redistributeCustom);

            // Handle the Remainder
            var shouldRedistribute = _ShouldRedistribute(resultWeeks, redistributeCustom);
            if (shouldRedistribute)
            {
                // add the remainder to the first week.
                _UpdateFirstWeekAndBudgetAdjustment(request, resultWeeks, request.TotalImpressions);
            }

            // handle adding weeks to the flight - we want these to be empty.
            var oldMediaWeekIds = request.Weeks.Select(y => y.MediaWeekId).ToList();
            var newWeeks = weeks.Where(x => !oldMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewWeeksToWeeklyBreakdownResult(resultWeeks, newWeeks, request);

            var result = new WeeklyBreakdownResponseDto
            {
                Weeks = resultWeeks
            };

            return result;
        }

        private bool _ShouldRedistribute(List<WeeklyBreakdownWeek> resultWeeks, bool redistributeCustom)
        {
            // we don't want to redistribute if not yet touched since ClearAll
            var tableWasUpdatedSinceLastClear = _GetTableWasUpdatedSinceLastClear(resultWeeks);
            var hasActiveDays = resultWeeks.Any(w => w.NumberOfActiveDays > 0);
            var shouldRedistribute = tableWasUpdatedSinceLastClear &&
                                     hasActiveDays &&
                                     redistributeCustom;

            return shouldRedistribute;
        }

        private bool _GetTableWasUpdatedSinceLastClear(List<WeeklyBreakdownWeek> weeks)
        {
            var hasUpdatedRows = weeks.Any(w => w.IsUpdated);
            if (hasUpdatedRows)
            {
                return true;
            }

            var unlockedRows = weeks.Where(w => !w.IsLocked).ToList();
            if (!unlockedRows.Any())
            {
                return true;
            }

            foreach (var unlockedRow in unlockedRows)
            {
                var isCleared = unlockedRow.NumberOfActiveDays > 0 &&
                                unlockedRow.WeeklyImpressions <= 0;
                if (!isCleared)
                {
                    return true;
                }
            }

            return false;
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

            _AddNewWeeksAndCreativeLengthsToWeeklyBreakdownResult(result.Weeks, weeks, request, creativeLengths);
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
            var resultWeeks = request.Weeks;

            // Handle removal of weeks and week items
            _RemoveOutOfFlightWeeks(request.Weeks, weeks);
            _RemoveWeeksWithCreativeLengthNotPresentingInCurrentCreativeLengthsList(request.Weeks, creativeLengths);

            // Handle week item additions
            var oldCreativeLengthIds = request.Weeks.Select(x => x.SpotLengthId.Value).Distinct().ToList();
            var newCreativeLengths = creativeLengths.Where(x => !oldCreativeLengthIds.Contains(x.SpotLengthId)).ToList();
            _AddNewCreativeLengthsToResult(resultWeeks, newCreativeLengths);

            // distribute impressions
            // do this after week item additions because they contribute to the item weights within the week.
            _RecalculateExistingWeeks(request, out var redistributeCustom);

            // Handle the impressions distribution remainder 
            var shouldRedistribute = _ShouldRedistribute(resultWeeks, redistributeCustom);
            if (shouldRedistribute)
            {
                // add the remainder to the first week
                _UpdateFirstWeekAndBudgetAdjustment(request, resultWeeks, request.TotalImpressions);
            }

            // Handle adding weeks to the flight - we want these to be empty.
            var oldMediaWeekIds = request.Weeks.Select(y => y.MediaWeekId).Distinct().ToList();
            var newWeeks = weeks.Where(x => !oldMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewWeeksAndCreativeLengthsToWeeklyBreakdownResult(resultWeeks, newWeeks, request, creativeLengths);

            var result = new WeeklyBreakdownResponseDto
            {
                Weeks = resultWeeks
            };

            return result;
        }

        private void _CalculateWeeklyGoalBreakdownTotals(WeeklyBreakdownResponseDto weeklyBreakdown, WeeklyBreakdownRequest request)
        {
            weeklyBreakdown.TotalActiveDays = GroupWeeklyBreakdownByWeek(weeklyBreakdown.Weeks)
                .Sum(x => x.NumberOfActiveDays);

            weeklyBreakdown.TotalAduImpressions = weeklyBreakdown.Weeks.Sum(x => x.AduImpressions);

            if (request.IsAduOnly && _IsAduForPlanningv2Enabled.Value)
            {
                weeklyBreakdown.TotalImpressions = 0;
                weeklyBreakdown.TotalShareOfVoice = 0;
                weeklyBreakdown.TotalImpressionsPercentage = 0;

                weeklyBreakdown.TotalRatingPoints = 0;
                weeklyBreakdown.TotalBudget = 0;
                weeklyBreakdown.TotalUnits = 0;
                return;
            }

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

        public void SetWeekNumberAndSpotLengthDuration(IEnumerable<WeeklyBreakdownWeek> weeks)
        {
            var weekNumberByMediaWeek = GetWeekNumberByMediaWeekDictionary(weeks);

            foreach (var week in weeks)
            {
                week.WeekNumber = weekNumberByMediaWeek[week.MediaWeekId];
                week.SpotLengthDuration = _GetWeeklySpotLengthDuration(week.SpotLengthId);
            }
        }

        private int? _GetWeeklySpotLengthDuration(int? weeklySpotLengthId)
        {
            var spotLengthDuration = weeklySpotLengthId.HasValue
                ? _SpotLengthEngine.GetSpotLengthValueById(weeklySpotLengthId.Value)
                : (int?)null;

            return spotLengthDuration;
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
        /// 
        /// Assume that adding\deleting an item is outside this scope.
        /// 1. Locked weeks remain locked and their values stay the same.
        /// 2. The remainder is distributed evenly in the unlocked weeks
        /// </summary>
        private void _RecalculateExistingWeeks(WeeklyBreakdownRequest request, out bool redistributeCustom)
        {
            redistributeCustom = false;

            //All weeks have been updated or Locked do not redistribute
            if (request.Weeks.Count > 1 && request.Weeks.All(x => x.IsLocked))
            {
                //reset update flag
                foreach (var week in request.Weeks)
                {
                    week.IsUpdated = false;
                }
                return;
            }

            var totalImpressionsToDistribute = 0d;
            List<WeeklyBreakdownWeek> weeksToUpdate;

            // This method handles two scenarios 
            // 1. One rows was updated within the grid and so only one row is updated
            // 2. Something external is changing the grid and so recalculate all unlocked rows.

            // figure out which scenario we are in 
            var updatedWeek = request.Weeks.SingleOrDefault(x => x.IsUpdated);

            // if we have an updated week then setup for scenario 1.
            if (updatedWeek != null)
            {
                // add the week to list we'll work from
                weeksToUpdate = new List<WeeklyBreakdownWeek> { updatedWeek };
                // do not auto-distribute
                redistributeCustom = false;
                // reset the flag for the return trip to the UI
                updatedWeek.IsUpdated = false;
            }
            // this is setup for scenario 2.
            else
            {
                weeksToUpdate = request.Weeks;

                // if recalculating the whole week, always calculate from impressions
                request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions;
                // redistribute goal impressions in same proportions accross the unlocked weeks
                redistributeCustom = true;

                // process hiatus days and weeks.
                // hiatus weeks will be wiped out even when locked.
                // we want that added to the amount to redistribute.
                foreach (var week in weeksToUpdate)
                {
                    List<int> daypartDayIds = _GetDaypartDayIds(request.Dayparts);
                    week.NumberOfActiveDays = CalculatorHelper.CalculateActiveDays(week.StartDate, week.EndDate, request.FlightDays, request.FlightHiatusDays, daypartDayIds, out string activeDaysString);
                    week.ActiveDays = activeDaysString;

                    // handle hiatus weeks
                    if (week.NumberOfActiveDays < 1)
                    {
                        week.WeeklyImpressions = 0;
                        week.WeeklyRatings = 0;
                        week.WeeklyImpressionsPercentage = 0;
                        week.WeeklyBudget = 0;
                        week.WeeklyAdu = 0;
                    }
                }

                // locked impressions cannot be included in the total distribution.
                var totalLockedImpressions = request.Weeks.Where(w => w.IsLocked).Sum(w => w.WeeklyImpressions);
                totalImpressionsToDistribute = request.TotalImpressions - totalLockedImpressions;
            }

            // calculate an even distribution accross unlocked weeks
            // when dealing with a multiple items per week view (i.e. custom by week by ad length, etc) we need to distribute evenly per week, not per list item.
            var weekIds = weeksToUpdate.Where(w => !w.IsLocked && w.NumberOfActiveDays > 0).Select(w => w.MediaWeekId).Distinct().ToList();
            var weekImpressions = _DistributeImpressionsByWeeks(totalImpressionsToDistribute, weekIds);

            // this updates per week item 
            // for custom by week view then each item is a week.
            // for custom by week by ad length view each item is a week/spot length.
            // for custom by week by daypart view each item is a week/daypart.
            foreach (var week in weeksToUpdate)
            {
                if (!week.IsLocked && week.NumberOfActiveDays > 0)
                {
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
                            if (request.Equivalized ?? false)
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
                            if (redistributeCustom && totalImpressionsToDistribute > 0)
                            {
                                // this is for the whole week, of which we have one component, potentially.
                                var impressionsForThisWeek = weekImpressions[week.MediaWeekId];

                                // get the components
                                var daypartWeight = _GetDaypartWeight(week.DaypartUniquekey, request.Dayparts, request.DeliveryType);
                                var spotLengthWeight = _GetSpotLengthWeight(week.SpotLengthId, request.CreativeLengths, request.DeliveryType);

                                var currentImpressions = impressionsForThisWeek * daypartWeight * spotLengthWeight;
                                week.WeeklyImpressions = Math.Floor(currentImpressions);
                            }
                            else
                            {
                                week.WeeklyImpressions = Math.Floor(week.WeeklyImpressions);
                            }
                            break;
                    }
                }

                _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                    , request.TotalBudget, week, week.WeeklyImpressions, roundRatings: true);
            }
        }

        private double _GetDaypartWeight(string daypartUniqueKey, List<PlanDaypartDto> dayparts, PlanGoalBreakdownTypeEnum deliveryType)
        {
            const double default_weight = 1.0;
            var daypartWeight = deliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart
                                    ? (dayparts.Where(x => x.DaypartUniquekey == daypartUniqueKey).Select(x => x.WeightingGoalPercent).FirstOrDefault() ?? default_weight) / 100
                                    : default_weight;

            return daypartWeight;
        }

        private double _GetSpotLengthWeight(int? spotLengthId, List<CreativeLength> spotLengths, PlanGoalBreakdownTypeEnum deliveryType)
        {
            const double default_weight = 1.0;
            if (!spotLengthId.HasValue)
            {
                return default_weight;
            }

            var spotLengthWeight = deliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength
                                    ? (spotLengths.Where(x => x.SpotLengthId == spotLengthId).Select(x => x.Weight).FirstOrDefault() ?? 1.0) / 100
                                    : 1.0;

            return spotLengthWeight;
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
            List<WeeklyBreakdownWeek> resultWeeks,
            List<DisplayMediaWeek> weeks,
            WeeklyBreakdownRequest request,
            List<StandardDaypartWeightingGoal> standardDayparts)
        {
            foreach (DisplayMediaWeek week in weeks)
            {
                foreach (var item in standardDayparts)
                {
                    var planDayparts = request.Dayparts.Where(d => d.DaypartUniquekey == item.DaypartUniquekey).ToList();
                    List<int> daypartDayIds = _GetDaypartDayIds(planDayparts);
                    var activeDays = CalculatorHelper.CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, daypartDayIds, out string activeDaysString);

                    var isWeekLocked = request.Weeks.Where(w => w.StartDate == week.WeekStartDate).FirstOrDefault()?.IsLocked ?? false;

                    resultWeeks.Add(new WeeklyBreakdownWeek
                    {
                        ActiveDays = activeDaysString,
                        NumberOfActiveDays = activeDays,
                        StartDate = week.WeekStartDate,
                        EndDate = week.WeekEndDate,
                        MediaWeekId = week.Id,
                        DaypartCodeId = item.StandardDaypartId,
                        PercentageOfWeek = item.WeightingGoalPercent,
                        DaypartOrganizationId = planDayparts.Select(x => x.DaypartOrganizationId).FirstOrDefault(),
                        DaypartOrganizationName = planDayparts.Select(x => x.DaypartOrganizationName).FirstOrDefault(),
                        CustomName = planDayparts.Select(x => x.CustomName).FirstOrDefault(),
                        PlanDaypartId = item.PlanDaypartId,
                        IsLocked = isWeekLocked
                    });
                }
            }
        }

        private void _AddNewWeeksToWeeklyBreakdownResult(List<WeeklyBreakdownWeek> resultWeeks, List<DisplayMediaWeek> weeks, WeeklyBreakdownRequest request)
        {
            foreach (DisplayMediaWeek week in weeks)
            {
                List<int> daypartDayIds = _GetDaypartDayIds(request.Dayparts);
                var activeDays = CalculatorHelper.CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, daypartDayIds, out string activeDaysString);

                resultWeeks.Add(new WeeklyBreakdownWeek
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
           List<WeeklyBreakdownWeek> resultWeeks,
           List<DisplayMediaWeek> weeks,
           WeeklyBreakdownRequest request,
           List<CreativeLength> creativeLengths)
        {
            foreach (DisplayMediaWeek week in weeks)
            {
                List<int> daypartDayIds = _GetDaypartDayIds(request.Dayparts);
                var activeDays = CalculatorHelper.CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, daypartDayIds, out string activeDaysString);

                foreach (var spotLenghtId in creativeLengths.Select(c => c.SpotLengthId))
                {
                    resultWeeks.Add(new WeeklyBreakdownWeek
                    {
                        ActiveDays = activeDaysString,
                        NumberOfActiveDays = activeDays,
                        StartDate = week.WeekStartDate,
                        EndDate = week.WeekEndDate,
                        MediaWeekId = week.Id,
                        SpotLengthId = spotLenghtId,
                        SpotLengthDuration = _GetWeeklySpotLengthDuration(spotLenghtId)
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
                    var weekFraction = breakdownItem.PercentageOfWeek == null ? 0 : breakdownItem.PercentageOfWeek.Value / 100;
                    var impressions = Math.Floor(impressionsForWeek * weekFraction);

                    _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                     , request.TotalBudget, breakdownItem, impressions, roundRatings: true);
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
                    , request.TotalBudget, breakdownItem, impressions, roundRatings: true);

                // handle ADU Impressions
                var weekAduImpressions = request.Weeks.Where(w => w.MediaWeekId == breakdownItem.MediaWeekId).Sum(s => s.AduImpressions);
                var weeklyAdu = request.Weeks.Where(w => w.MediaWeekId == breakdownItem.MediaWeekId).Sum(s => s.WeeklyAdu);
                breakdownItem.AduImpressions = weekAduImpressions;
                breakdownItem.WeeklyAdu = weeklyAdu;
            }
        }

        private void _UpdateFirstWeekAndBudgetAdjustment(
           WeeklyBreakdownRequest request,
           List<WeeklyBreakdownWeek> weeks,
           double totalImpressions)
        {
            var availableWeeks = weeks.Where(w => !w.IsLocked && w.NumberOfActiveDays > 0).ToList();
            if (!availableWeeks.Any())
            {
                return;
            }

            // look for a remainder
            var totalImpressionsRounded = weeks.Sum(w => w.WeeklyImpressions);
            var roundedImpressionsDifference = totalImpressions - totalImpressionsRounded;

            // if no remainder then just return
            if (roundedImpressionsDifference <= 0)
            {
                return;
            }

            // get all the components for the first week
            var firstWeekStart = availableWeeks.Min(w => w.StartDate);
            var firstWeek = availableWeeks.Where(w => w.StartDate.Equals(firstWeekStart)).ToList();

            // distribute the remainder accross the week
            foreach (var weekItem in firstWeek)
            {
                // distribute them per their weight per the current DeliveryType
                var daypartWeight = _GetDaypartWeight(weekItem.DaypartUniquekey, request.Dayparts, request.DeliveryType);
                var spotLengthWeight = _GetSpotLengthWeight(weekItem.SpotLengthId, request.CreativeLengths, request.DeliveryType);
                var remainderWeeklyImpressions = Math.Floor(roundedImpressionsDifference * daypartWeight * spotLengthWeight);

                if (remainderWeeklyImpressions > 0)
                {
                    var newWeeklyImpressions = weekItem.WeeklyImpressions + remainderWeeklyImpressions;

                    _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                        , request.TotalBudget, weekItem, newWeeklyImpressions, roundRatings: true);
                }
            }

            // double check just in case there is still a remainder due to the percentage math
            var newTotalImpressionsRounded = weeks.Sum(w => w.WeeklyImpressions);
            var newRoundedImpressionsDifference = totalImpressions - newTotalImpressionsRounded;

            // if no remainder then just return
            if (newRoundedImpressionsDifference <= 0)
            {
                return;
            }

            // if still a remainder then just put it all on the very first unlocked item.
            var firstWeekItem = firstWeek.First();
            var newFirstWeekItemImpressions = firstWeekItem.WeeklyImpressions + newRoundedImpressionsDifference;

            _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                    , request.TotalBudget, firstWeekItem, newFirstWeekItemImpressions, roundRatings: true);

            // nothing lef to do
        }

        private void _AddNewCreativeLengthsToResult(
            List<WeeklyBreakdownWeek> weeks,
            List<CreativeLength> creativeLengths)
        {
            var existingWeeks = GroupWeeklyBreakdownByWeek(weeks);

            foreach (var existingWeek in existingWeeks)
            {
                foreach (var spotLengthId in creativeLengths.Select(c => c.SpotLengthId))
                {
                    weeks.Add(new WeeklyBreakdownWeek
                    {
                        ActiveDays = existingWeek.ActiveDays,
                        NumberOfActiveDays = existingWeek.NumberOfActiveDays,
                        StartDate = existingWeek.StartDate,
                        EndDate = existingWeek.EndDate,
                        MediaWeekId = existingWeek.MediaWeekId,
                        SpotLengthId = spotLengthId,
                        SpotLengthDuration = _GetWeeklySpotLengthDuration(spotLengthId),
                        IsLocked = existingWeek.IsLocked
                    });
                }
            }
        }

        private void _UpdateGoalsForWeeklyBreakdownItem(
            double totalImpressions,
            double totalRatings,
            decimal totalBudget,
            WeeklyBreakdownWeek breakdownItem,
            double impressions,
            bool roundRatings)
        {
            decimal budgetPerOneImpression = 0;
            double weeklyRatio = 0;
            if (totalImpressions > 0)
            {
                budgetPerOneImpression = totalBudget / (decimal)totalImpressions;
                weeklyRatio = impressions / totalImpressions;
            }

            breakdownItem.WeeklyImpressions = impressions;
            breakdownItem.WeeklyImpressionsPercentage = Math.Round(100 * weeklyRatio);
            breakdownItem.WeeklyBudget = (decimal)impressions * budgetPerOneImpression;

            var ratings = totalRatings * weeklyRatio;
            breakdownItem.WeeklyRatings = roundRatings ? ProposalMath.RoundUpWithDecimals(ratings, 2) : ratings;
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

            var totalWeeks = weekIds.Count;
            var impressionsPerWeek = Math.Floor(totalImpressions / totalWeeks);

            // add undistributed impressions to the first week
            var undistributedImpressions = totalImpressions - (totalWeeks * impressionsPerWeek);
            var impressionsForFirstWeek = Math.Floor(impressionsPerWeek + undistributedImpressions);

            for (var i = 0; i < weekIds.Count; i++)
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
            , double impressionsPerUnit = 0, List<CreativeLength> creativeLengths = null, bool? equivalized = false)
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
                            : weeklyImpressions / unitsImpressions,
                        IsLocked = first.IsLocked,
                        SpotLengthId = first.SpotLengthId,
                        DaypartCodeId = first.DaypartCodeId,
                        CustomName = first.CustomName,
                        DaypartOrganizationName = first.DaypartOrganizationName,
                        Adu = first.WeeklyAdu
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
            , double impressionsPerUnit, bool? equivalized)
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
                        Units = unitsImpressions == 0 ? 0 : weekImpressions / unitsImpressions,
                        IsLocked = first.IsLocked
                    };
                }).ToList();
        }

        public List<WeeklyBreakdownByWeekByDaypart> GroupWeeklyBreakdownByWeekByDaypart(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit, bool? equivalized, List<CreativeLength> creativeLengths)
        {
            return weeklyBreakdown
                .GroupBy(x => new { x.MediaWeekId, x.DaypartUniquekey })
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
                        Units = unitsImpressions == 0 ? 0 : weekImpressions / unitsImpressions,
                        IsLocked = first.IsLocked,
                        DaypartOrganizationId = first.DaypartOrganizationId,
                        CustomName = first.CustomName,
                        DaypartOrganizationName = first.DaypartOrganizationName
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
                        AduImpressions = grouping.Sum(x => x.AduImpressions),
                        Budget = grouping.Sum(x => x.WeeklyBudget)
                    };
                })
                .ToList();
        }

        private double _CalculateEquivalizedImpressionsPerUnit(double impressionPerUnit, int spotLengthId)
            => impressionPerUnit * _SpotLengthDeliveryMultipliers.Value[spotLengthId];

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

        private void _CalculateUnits(List<WeeklyBreakdownWeek> weeks, bool? equivalized, double impressionsPerUnit, List<CreativeLength> creativeLengths)
        {
            foreach (var week in weeks)
            {
                if (equivalized ?? false)
                {
                    if (week.SpotLengthId.HasValue)
                    {
                        week.WeeklyUnits = _CalculateUnitsForSingleSpotLength(impressionsPerUnit, week.WeeklyImpressions, week.SpotLengthId.Value);
                    }
                    else
                    {
                        if (creativeLengths.Any(x => !x.Weight.HasValue))
                        {
                            var creativeLengthsWithDistributedWeight = _CreativeLengthEngine.DistributeWeight(creativeLengths);
                            creativeLengths = creativeLengthsWithDistributedWeight;
                        }
                        week.WeeklyUnits = _CalculateUnitsForMultipleSpotLengths(creativeLengths, impressionsPerUnit, week.WeeklyImpressions);
                    }
                }
                else
                {
                    week.WeeklyUnits = week.WeeklyImpressions / impressionsPerUnit;
                }
                week.WeeklyUnits = Math.Round(week.WeeklyUnits, 2);
            }
        }

        /// <inheritdoc/>
        public double CalculateWeeklyADUImpressions(WeeklyBreakdownWeek week, bool? equivalized
            , double impressionsPerUnit, List<CreativeLength> creativeLengths)
        {
            if (_IsAduForPlanningv2Enabled.Value)
            {
                // When enabled then the given Weekly ADU is Impression, not Units, and does not use the ImpressionsPerUnit property.
                // But we still want the Weighting functionality.
                // To minimize risk due to large change we will set the impressionsPerUnit to 1 and do the rest of the calcualtion as-is.
                impressionsPerUnit = 1;
            }

            if (equivalized ?? false)
            {
                if (week.SpotLengthId.HasValue)
                {
                    var result = _CalculateUnitImpressionsForSingleSpotLength(impressionsPerUnit, week.WeeklyAdu, week.SpotLengthId.Value);
                    return result;
                }
                else
                {
                    var result = _CalculateUnitImpressionsForMultipleSpotLengths(creativeLengths, impressionsPerUnit, week.WeeklyAdu);
                    return result;
                }
            }
            else
            {
                var result = week.WeeklyAdu * impressionsPerUnit;
                return result;
            }
        }

        private int _CalculateADU(double impressionsPerUnit, double aduImpressions
            , bool? equivalized, int? spotLengthId, List<CreativeLength> creativeLengths = null)
        {
            if (impressionsPerUnit == 0)
            {   //for older plans, where the user did not set an impressions per unit value, we need to show the user the ADU value based on the old math
                return (int)(aduImpressions / _DefaultImpressionsPerUnitForOldPlans);
            }

            if (_IsAduForPlanningv2Enabled.Value)
            {
                // When enabled then the given Weekly ADU is Impression, not Units, and does not use the ImpressionsPerUnit property.
                // But we still want the Weighting functionality.
                // To minimize risk due to large change we will set the impressionsPerUnit to 1 and do the rest of the calcualtion as-is.
                // It's also expected that the WeeklyAdu has been converted to Impressions by now following (000) rules.

                impressionsPerUnit = 1;
            }

            if (equivalized ?? false)
            {
                if (spotLengthId.HasValue)
                {
                    return (int)Math.Round(_CalculateUnitsForSingleSpotLength(impressionsPerUnit, aduImpressions, spotLengthId.Value), 0);
                }
                else
                {
                    return (int)Math.Round(_CalculateUnitsForMultipleSpotLengths(creativeLengths, impressionsPerUnit, aduImpressions), 0);
                }
            }
            else
            {
                return (int)(aduImpressions / impressionsPerUnit);
            }
        }

        /// <inheritdoc/>
        public double CalculateADUWithDecimals(double impressionsPerUnit, double aduImpressions
            , bool? equivalized, int spotLengthId)
        {
            if (impressionsPerUnit == 0)
            {   //for older plans, where the user did not set an impressions per unit value, we need to show the user the ADU value based on the old math
                return (int)(aduImpressions / _DefaultImpressionsPerUnitForOldPlans);
            }

            if (_IsAduForPlanningv2Enabled.Value)
            {
                // When enabled then the given Weekly ADU is Impression, not Units, and does not use the ImpressionsPerUnit property.
                // But we still want the Weighting functionality.
                // To minimize risk due to large change we will set the impressionsPerUnit to 1 and do the rest of the calcualtion as-is.
                // It's also expected that the WeeklyAdu has been converted to Impressions by now following (000) rules.

                impressionsPerUnit = 1;
            }

            if (equivalized ?? false)
            {
                return _CalculateUnitsForSingleSpotLength(impressionsPerUnit, aduImpressions, spotLengthId);
            }
            else
            {
                return (aduImpressions / impressionsPerUnit);
            }
        }

        private void _AddDaypartToWeeklyBreakdownResult(WeeklyBreakdownRequest request, WeeklyBreakdownResponseDto response)
        {
            foreach (var week in response.Weeks)
            {
                foreach (var item in request.Dayparts)
                {
                    if (item.DaypartUniquekey == week.DaypartUniquekey)
                    {
                        week.DaypartOrganizationId = item.DaypartOrganizationId;
                        week.CustomName = item.CustomName;
                        week.DaypartOrganizationName = item.DaypartOrganizationName;
                        week.PlanDaypartId = item.PlanDaypartId;
                    }
                }
            }
        }

        private List<WeeklyBreakdownWeek> _PopulateRawWeeklyBreakdownWeeks(WeeklyBreakdownRequest request, List<WeeklyBreakdownWeek> weeks)
        {
            PlanDto plan = new PlanDto();
            plan.GoalBreakdownType = request.DeliveryType;
            plan.CreativeLengths = request.CreativeLengths;
            plan.Dayparts = request.Dayparts;
            plan.WeeklyBreakdownWeeks = weeks;
            plan.TargetRatingPoints = request.TotalRatings;
            plan.Budget = request.TotalBudget;
            plan.Equivalized = request.Equivalized;
            plan.ImpressionsPerUnit = request.ImpressionsPerUnit;
            plan.TargetImpressions = request.TotalImpressions;
            plan.FlightDays = request.FlightDays;
            plan.FlightEndDate = request.FlightEndDate;
            plan.FlightHiatusDays = request.FlightHiatusDays;
            return DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);
        }

        private List<int> _GetDaypartDayIds(List<PlanDaypartDto> planDayparts)
        {
            // the FE sends with at least 1 empty daypart...
            // look for valid dayparts for this calculation
            var validDayparts = planDayparts?.Where(d => d.DaypartCodeId > 0).ToList();
            if (validDayparts?.Any() != true)
            {
                return new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            }

            var planDefaultDaypartIds = planDayparts.Select(d => d.DaypartCodeId).ToList();
            var dayIds = _StandardDaypartRepository.GetDayIdsFromStandardDayparts(planDefaultDaypartIds);
            return dayIds;
        }
    }
}
