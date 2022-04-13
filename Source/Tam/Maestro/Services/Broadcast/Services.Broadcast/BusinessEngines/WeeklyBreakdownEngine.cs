﻿using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
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
    public interface IWeeklyBreakdownEngine : IApplicationService
    {
        List<WeeklyBreakdownByWeek> GroupWeeklyBreakdownByWeek(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit = 0, List<CreativeLength> creativeLengths = null, bool? equivalized = false);
        List<WeeklyBreakdownByWeekBySpotLength> GroupWeeklyBreakdownByWeekBySpotLength(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit, bool? equivalized);
        Dictionary<int, int> GetWeekNumberByMediaWeekDictionary(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);

        WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request);
        List<WeeklyBreakdownByStandardDaypart> GroupWeeklyBreakdownByStandardDaypart(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown);

        void RecalculatePercentageOfWeekBasedOnImpressions(List<WeeklyBreakdownWeek> weeks);

        void SetWeekNumberAndSpotLengthDuration(IEnumerable<WeeklyBreakdownWeek> weeks);

        /// <summary>
        /// Calculates the adu Impressions
        /// </summary>
        /// <param name="week">Week object</param>
        /// <param name="equivalized">Equivalized flag</param>
        /// <param name="impressionsPerUnit">Impressions per unit value</param>
        /// <param name="creativeLengths">List of creative lengths</param>
        /// <returns>Number of adu impressions</returns>
        double CalculateWeeklyADUImpressions(WeeklyBreakdownWeek week, bool? equivalized
            , double impressionsPerUnit, List<CreativeLength> creativeLengths);

        /// <summary>
        /// Groups the weekly breakdown by week by daypart
        /// </summary>
        List<WeeklyBreakdownByWeekByDaypart> GroupWeeklyBreakdownByWeekByDaypart(IEnumerable<WeeklyBreakdownWeek> weeklyBreakdown
            , double impressionsPerUnit, bool? equivalized, List<CreativeLength> creativeLengths);

        /// <summary>
        /// Calculates the adu with decimals.
        /// </summary>
        /// <param name="impressionsPerUnit">The impressions per unit.</param>
        /// <param name="aduImpressions">The adu impressions total.</param>
        /// <param name="equivalized">Equivalized flag</param>
        /// <param name="spotLengthId">Spot length</param>
        /// <returns>Adu number as double</returns>
        double CalculateADUWithDecimals(double impressionsPerUnit, double aduImpressions
            , bool? equivalized, int spotLengthId);

        List<WeeklyBreakdownWeek> GroupWeeklyBreakdownWeeksBasedOnDeliveryType(PlanDto plan);

        /// <summary>
        /// Based on the plan delivery type, splits weekly breakdown into all combinations of plan ad lengths and dayparts
        /// And distributes goals based on ad length and daypart weights
        /// </summary>
        List<WeeklyBreakdownWeek> DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(
            PlanDto plan,
            double? customImpressionsGoal = null,
            decimal? customBudgetGoal = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        WeeklyBreakdownResponseDto ClearPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request);
    }

    public class WeeklyBreakdownEngine : IWeeklyBreakdownEngine
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

        private IStandardDaypartRepository _StandardDaypartRepository;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private Lazy<bool> _IsWeeklyBreakdownLockEnabled;

        public WeeklyBreakdownEngine(IPlanValidator planValidator,
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

            _SpotLengthDeliveryMultipliers = new Lazy<Dictionary<int, double>>(_GetSpotDeliveryMultipliers);
            _SpotLengthCostMultipliers = new Lazy<Dictionary<int, decimal>>(_GetSpotCostMultipliers);
            _FeatureToggleHelper = featureToggleHelper;
            _IsWeeklyBreakdownLockEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLED_WEEKLY_BREAKDOWN_LOCK));
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

                // In save plan it's distributing goals for spot length, so it's not necessary to call it again
                foreach (var distributedSpotLength in plan.CreativeLengths)
                {
                    var weighting = GeneralMath.ConvertPercentageToFraction(distributedSpotLength.Weight.GetValueOrDefault());

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
                        AduImpressions = aduImpressionsForBreakdownItem * weighting,
                        UnitImpressions = unitsImpressionsForBreakdownItem * weighting,
                        IsLocked = weeklyBreakdown.IsLocked,
                        DaypartOrganizationId = weeklyBreakdown.DaypartOrganizationId,
                        CustomName = weeklyBreakdown.CustomName,
                        DaypartOrganizationName = weeklyBreakdown.DaypartOrganizationName,
                        PlanDaypartId = weeklyBreakdown.PlanDaypartId,
                        WeeklyAdu = weeklyBreakdown.WeeklyAdu
                    };

                    var impressions = weeklyBreakdown.WeeklyImpressions * weighting;

                    _UpdateGoalsForWeeklyBreakdownItem(
                        impressionsGoal,
                        plan.TargetRatingPoints.Value,
                        budgetGoal,
                        newWeeklyBreakdownItem,
                        impressions,
                        roundRatings: false);

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

                    foreach (var item in standardDaypardWeightingGoals)
                    {
                        var weighting = GeneralMath.ConvertPercentageToFraction(item.WeightingGoalPercent);

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
                            AduImpressions = aduImpressionsForBreakdownItem * weighting,
                            UnitImpressions = unitsImpressionsForBreakdownItem * weighting,
                            IsLocked = week.IsLocked,
                            CustomName = item.CustomName,
                            DaypartOrganizationName = item.DaypartOrganizationName,
                            DaypartOrganizationId = item.DaypartOrganizationId,
                            PlanDaypartId = item.PlanDaypartId,
                            WeeklyAdu = week.Adu
                        };

                        var impressions = breakdownItem.WeeklyImpressions * weighting;

                        _UpdateGoalsForWeeklyBreakdownItem(
                            impressionsGoal,
                            plan.TargetRatingPoints.Value,
                            budgetGoal,
                            newWeeklyBreakdownItem,
                            impressions,
                            roundRatings: false);

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
                        AduImpressions = weeklyAduImpressions * combination.Weighting,
                        UnitImpressions = unitsImpressions * combination.Weighting,
                        IsLocked = week.IsLocked,
                        CustomName = combination.CustomName,
                        DaypartOrganizationName = combination.DaypartOrganizationName,
                        DaypartOrganizationId = combination.DaypartOrganizationId,
                        PlanDaypartId = combination.PlanDaypartId,
                        WeeklyAdu = week.WeeklyAdu

                    };

                    var impressions = week.WeeklyImpressions * combination.Weighting;

                    _UpdateGoalsForWeeklyBreakdownItem(
                        impressionsGoal,
                        plan.TargetRatingPoints.Value,
                        budgetGoal,
                        newWeeklyBreakdownItem,
                        impressions,
                        roundRatings: false);

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
                var planDaypart = plan.Dayparts.Where(x => x.DaypartUniquekey == item.DaypartUniquekey).FirstOrDefault();
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

                _UpdateGoalsForWeeklyBreakdownItem(
                       plan.TargetImpressions.Value,
                       plan.TargetRatingPoints.Value,
                       plan.Budget.Value,
                       newWeeklyBreakdownItem,
                       item.Impressions,
                       roundRatings: true);

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

                _UpdateGoalsForWeeklyBreakdownItem(
                    plan.TargetImpressions.Value,
                    plan.TargetRatingPoints.Value,
                    plan.Budget.Value,
                    newWeeklyBreakdownItem,
                    item.Impressions,
                    roundRatings: true);

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

                _UpdateGoalsForWeeklyBreakdownItem(
                       plan.TargetImpressions.Value,
                       plan.TargetRatingPoints.Value,
                       plan.Budget.Value,
                       newWeeklyBreakdownItem,
                       week.Impressions, roundRatings: true);

                result.Add(newWeeklyBreakdownItem);
            }

            return result;
        }

        public WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request)
        {
            _PlanValidator.ValidateWeeklyBreakdown(request);

            if (request.ImpressionsPerUnit <= 0) //old plan
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
                var standardDaypart = PlanGoalHelper.GetStandardDaypardWeightingGoals(request.Dayparts);
                if (isInitialLoad)
                    response = _CalculateInitialCustomByWeekByDaypartWeeklyGoalBreadkdown(request, weeks, standardDaypart);
                else
                    response = _CalculateCustomByWeekByDaypartWeeklyGoalBreadkdown(request, weeks, standardDaypart);
            }
            else
            {
                throw new ApplicationException(_UnsupportedDeliveryTypeMessage);
            }

            _CalculateUnits(response.Weeks, request.Equivalized, request.ImpressionsPerUnit, request.CreativeLengths);
            _AdjustSpotLengthBudget(response.Weeks, request.DeliveryType, request.Equivalized, request.TotalBudget);
            _CalculateWeeklyGoalBreakdownTotals(response, request);
            _OrderWeeks(request, response);
            SetWeekNumberAndSpotLengthDuration(response.Weeks);
            _AddDaypartToWeeklyBreakdownResult(request, response);
            response.RawWeeklyBreakdownWeeks = _PopulateRawWeeklyBreakdownWeeks(request, response.Weeks);
            return response;
        }


        public WeeklyBreakdownResponseDto ClearPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request)
        {
            var result = new WeeklyBreakdownResponseDto();

            if (request.DeliveryType == PlanGoalBreakdownTypeEnum.EvenDelivery)
            {
                request.DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek;
            }
            
            if (_IsWeeklyBreakdownLockEnabled.Value)
            {
                foreach (var week in request.Weeks)
                {                   
                    if (week.IsLocked != true)
                    {                        
                        week.WeeklyBudget = 0;
                        week.WeeklyUnits = 0;
                        week.WeeklyRatings = 0;
                        week.WeeklyImpressions = 0;
                        week.WeeklyImpressionsPercentage = 0;
                        week.WeeklyAdu = 0;
                        week.IsUpdated = false;
                    }
                }
            }
            else
            {
                foreach (var week in request.Weeks)
                {                  
                    week.WeeklyBudget = 0;
                    week.WeeklyUnits = 0;
                    week.WeeklyRatings = 0;
                    week.WeeklyImpressions = 0;
                    week.WeeklyImpressionsPercentage = 0;
                    week.WeeklyAdu = 0;
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
            result.TotalImpressions = request.Weeks.Sum(x => x.WeeklyImpressions); ;
            result.TotalRatingPoints = request.Weeks.Sum(x => x.WeeklyRatings);
            result.TotalImpressionsPercentage = totalImpressionsPercentage;

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

            _AddNewWeeksToWeeklyBreakdownResult(result, weeks, request);
            _CalculateGoalsForEvenDeliveryType(request, result.Weeks);

            return result;
        }

        /// <summary>
        /// Calculate the initial load of weekly brekdown using the delivery type By Week By Daypart
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateInitialCustomByWeekByDaypartWeeklyGoalBreadkdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks, List<StandardDaypartWeightingGoal> standardDayparts)
        {
            var result = new WeeklyBreakdownResponseDto();

            _AddNewWeeksByDaypartToWeeklyBreakdownResult(result, weeks, request, standardDayparts);
            _CalculateGoalsForCustomByWeekByPercentageOfWeek(request, result.Weeks);
            RecalculatePercentageOfWeekBasedOnImpressions(result.Weeks);

            return result;
        }

        /// <summary>
        /// Calculate the weekly brekdown with existing values using the delivery type By Week By Daypart
        /// </summary>
        private WeeklyBreakdownResponseDto _CalculateCustomByWeekByDaypartWeeklyGoalBreadkdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks, List<StandardDaypartWeightingGoal> standardDayparts)
        {
            var result = new WeeklyBreakdownResponseDto();

            //remove deleted weeks
            _RemoveOutOfFlightWeeks(request.Weeks, weeks);
            if (request.Weeks.Any(x => x.DaypartCodeId.HasValue) && request.Weeks.Any(x => x.SpotLengthId.HasValue))
            {
                request.Weeks = request.Weeks.GroupBy(x => new { x.DaypartUniquekey, x.MediaWeekId }).Select(grouping =>
                {
                    var first = grouping.First();
                    var allItems = grouping.ToList();
                    double weeklyImpressions = allItems.Sum(x => x.WeeklyImpressions);
                    decimal weeklyBudget = allItems.Sum(x => x.WeeklyBudget);
                    double weeklyImpressionsPercentage = allItems.Sum(x => x.WeeklyImpressionsPercentage);
                    var week = new WeeklyBreakdownWeek
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        WeeklyImpressions = weeklyImpressions,
                        WeeklyImpressionsPercentage = weeklyImpressionsPercentage,
                        WeeklyRatings = first.WeeklyRatings,
                        WeeklyBudget = weeklyBudget,
                        WeeklyAdu = first.IsLocked ? first.WeeklyAdu : 0,
                        AduImpressions = first.AduImpressions,
                        DaypartCodeId = first.DaypartCodeId,
                        PercentageOfWeek = first.PercentageOfWeek,
                        IsUpdated = first.IsUpdated,
                        UnitImpressions = first.UnitImpressions,
                        IsLocked = first.IsLocked,
                        DaypartOrganizationId = first.DaypartOrganizationId,
                        CustomName = first.CustomName,
                        DaypartOrganizationName = first.DaypartOrganizationName,
                        WeeklyUnits = first.WeeklyUnits
                    };
                    return week;
                }).ToList();
            }
            //add the remain weeks
            result.Weeks.AddRange(request.Weeks);

            // Remove dayparts not in use from the remain weeks
            _RemoveDaypartsInTheExistingWeeks(request, result);
            
            // Recalculate fields in the existing weeks
            _RecalculateExistingWeeks(request, out bool redistributeCustom);

            // Add new dayparts in the remain weeks
            var remainMediaWeekIds = result.Weeks.Select(y => y.MediaWeekId).ToList();
            var remainWeeks = weeks.Where(x => remainMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewDaypartsInTheExistingWeeks(request, result, remainWeeks, standardDayparts);

            // Add new weeks
            var oldMediaWeekIds = request.Weeks.Select(y => y.MediaWeekId).ToList();
            var newWeeks = weeks.Where(x => !oldMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewWeeksByDaypartToWeeklyBreakdownResult(result, newWeeks, request, standardDayparts);

            var shouldRedistribute = _ShouldRedistribute(result, redistributeCustom);

            //only adjust first week if redistributing
            if (shouldRedistribute)
            {
                _UpdateFirstWeekAndBudgetAdjustment(request, result.Weeks, request.TotalImpressions);
            }

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
            var existingWeekDaypartIds = response.Weeks.Select(w => w.DaypartUniquekey).Distinct();
            // Get the daypart ids in the request
            var requestDaypartIds = request.Dayparts.Select(w => w.DaypartUniquekey).Distinct();

            // Remove dayparts that doesnt exist in the request
            if (!existingWeekDaypartIds.Contains("0||"))
            {
                var daypartsToRemove = existingWeekDaypartIds.Where(d => !requestDaypartIds.Contains(d)).ToList();
                if (daypartsToRemove.Any())
                {
                    response.Weeks.RemoveAll(w => daypartsToRemove.Contains(w.DaypartUniquekey));
                }
            }
        }

        /// <summary>
        /// Checks the weeks are missing dayparts in the request and add it
        /// </summary>
        private void _AddNewDaypartsInTheExistingWeeks(WeeklyBreakdownRequest request,
            WeeklyBreakdownResponseDto response,
            List<DisplayMediaWeek> existingWeeks,
            List<StandardDaypartWeightingGoal> standardDayparts)
        {
            foreach (var week in response.Weeks)
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
            var existingWeekDaypartIds = response.Weeks.Select(w => w.DaypartUniquekey).Distinct();

            // Add new dayparts to existin week
            if (!existingWeekDaypartIds.Contains("0||"))
            {
                var daypartsToAdd = standardDayparts.Where(d => !existingWeekDaypartIds.Contains(d.DaypartUniquekey)).ToList();
                if (daypartsToAdd.Any())
                {
                    _AddNewWeeksByDaypartToWeeklyBreakdownResult(response, existingWeeks, request, daypartsToAdd);
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
            var result = new WeeklyBreakdownResponseDto();

            //remove deleted weeks
            _RemoveOutOfFlightWeeks(request.Weeks, weeks);

            //add the remain weeks           
            if (request.Weeks.Any(x => x.DaypartCodeId.HasValue) && request.Weeks.Any(x => x.SpotLengthId.HasValue))
            {
                request.Weeks = request.Weeks.GroupBy(x => x.MediaWeekId).Select(grouping =>
                {
                    var first = grouping.First();
                    var allItems = grouping.ToList();
                    double weeklyImpressions = allItems.Sum(x => x.WeeklyImpressions);
                    decimal weeklyBudget = allItems.Sum(x => x.WeeklyBudget);
                    double weeklyImpressionsPercentage = allItems.Sum(x => x.WeeklyImpressionsPercentage);
                    var week = new WeeklyBreakdownWeek
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        WeeklyImpressions = weeklyImpressions,
                        WeeklyImpressionsPercentage = weeklyImpressionsPercentage,
                        WeeklyRatings = first.WeeklyRatings,
                        WeeklyBudget = weeklyBudget,
                        WeeklyAdu = first.IsLocked ? first.WeeklyAdu : 0,
                        AduImpressions = first.AduImpressions,
                        PercentageOfWeek = first.PercentageOfWeek,
                        IsUpdated = first.IsUpdated,
                        UnitImpressions = first.UnitImpressions,
                        IsLocked = first.IsLocked,
                        DaypartOrganizationId = first.DaypartOrganizationId,
                        CustomName = first.CustomName,
                        DaypartOrganizationName = first.DaypartOrganizationName,
                        WeeklyUnits = first.WeeklyUnits
                    };
                    return week;
                }).ToList();
            }
            result.Weeks.AddRange(request.Weeks);
            _RecalculateExistingWeeks(request, out bool redistributeCustom);

            //add the new weeks
            var oldMediaWeekIds = request.Weeks.Select(y => y.MediaWeekId).ToList();
            var newWeeks = weeks.Where(x => !oldMediaWeekIds.Contains(x.Id)).ToList();
            _AddNewWeeksToWeeklyBreakdownResult(result, newWeeks, request);

            var shouldRedistribute = _ShouldRedistribute(result, redistributeCustom);

            //only adjust first week if redistributing
            if (shouldRedistribute)
            {
                _UpdateFirstWeekAndBudgetAdjustment(request, result.Weeks, request.TotalImpressions);
            }
            return result;
        }

        private bool _ShouldRedistribute(WeeklyBreakdownResponseDto result, bool redistributeCustom)
        {
            // we don't want to redistribute if not yet touched since ClearAll
            var tableWasUpdatedSinceLastClear = _GetTableWasUpdatedSinceLastClear(result.Weeks);
            var hasActiveDays = result.Weeks.Any(w => w.NumberOfActiveDays > 0);
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
            if (request.Weeks.Any(x => x.DaypartCodeId.HasValue) && request.Weeks.Any(x => x.SpotLengthId.HasValue))
            {
                request.Weeks = request.Weeks.GroupBy(x => new { x.SpotLengthId, x.MediaWeekId }).Select(grouping =>
                {
                    var first = grouping.First();
                    var allItems = grouping.ToList();
                    double weeklyImpressions = allItems.Sum(x => x.WeeklyImpressions);
                    decimal weeklyBudget = allItems.Sum(x => x.WeeklyBudget);
                    double weeklyImpressionsPercentage = allItems.Sum(x => x.WeeklyImpressionsPercentage);
                    var week = new WeeklyBreakdownWeek
                    {
                        WeekNumber = first.WeekNumber,
                        MediaWeekId = first.MediaWeekId,
                        StartDate = first.StartDate,
                        EndDate = first.EndDate,
                        NumberOfActiveDays = first.NumberOfActiveDays,
                        ActiveDays = first.ActiveDays,
                        WeeklyImpressions = weeklyImpressions,
                        WeeklyImpressionsPercentage = weeklyImpressionsPercentage,
                        WeeklyRatings = first.WeeklyRatings,
                        WeeklyBudget = weeklyBudget,
                        WeeklyAdu = first.IsLocked ? first.WeeklyAdu : 0,
                        AduImpressions = first.AduImpressions,
                        SpotLengthId = first.SpotLengthId,
                        SpotLengthDuration = first.SpotLengthDuration,
                        PercentageOfWeek = first.PercentageOfWeek,
                        IsUpdated = first.IsUpdated,
                        UnitImpressions = first.UnitImpressions,
                        IsLocked = first.IsLocked,
                        DaypartOrganizationId = first.DaypartOrganizationId,
                        CustomName = first.CustomName,
                        DaypartOrganizationName = first.DaypartOrganizationName,
                        WeeklyUnits = first.WeeklyUnits
                    };
                    return week;
                }).ToList();
            }
            // add the remain weeks
            result.Weeks.AddRange(request.Weeks);

            _RecalculateExistingWeeks(request, out var redistributeCustom);

            var oldMediaWeekIds = request.Weeks.Select(y => y.MediaWeekId).Distinct().ToList();
            var oldCreativeLengthIds = request.Weeks.Select(x => x.SpotLengthId.Value).Distinct().ToList();
            var newWeeks = weeks.Where(x => !oldMediaWeekIds.Contains(x.Id)).ToList();
            var newCreativeLengths = creativeLengths.Where(x => !oldCreativeLengthIds.Contains(x.SpotLengthId)).ToList();

            _AddNewCreativeLengthsToResult(result, newCreativeLengths);
            _AddNewWeeksAndCreativeLengthsToWeeklyBreakdownResult(result, newWeeks, request, creativeLengths);

            var shouldRedistribute = _ShouldRedistribute(result, redistributeCustom);

            //only adjust first week if redistributing
            if (shouldRedistribute)
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

            double oldImpressionTotals = 0;
            List<WeeklyBreakdownWeek> weeksToUpdate;
            var updatedWeek = request.Weeks.FirstOrDefault(x => x.IsUpdated);

            // If Updated Week present compute only for the updated week, else do it for all the weeks.
            if (updatedWeek != null)
            {
                weeksToUpdate = new List<WeeklyBreakdownWeek> { updatedWeek };                
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
                week.NumberOfActiveDays = _CalculateActiveDays(week.StartDate, week.EndDate, request.FlightDays, request.FlightHiatusDays, request.Dayparts, out string activeDaysString);
                if (week.NumberOfActiveDays < 1)
                {
                    week.WeeklyImpressions = 0;
                    week.WeeklyRatings = 0;
                    week.WeeklyImpressionsPercentage = 0;
                    week.WeeklyBudget = 0;
                    week.WeeklyAdu = 0;
                }
                if (!week.IsLocked && week.NumberOfActiveDays > 0)
                {
                    week.ActiveDays = activeDaysString;
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
                            var totalUnlockedActiveRowsCount = request.Weeks.Count(w => w.IsLocked.Equals(false) && w.NumberOfActiveDays > 0);
                            if (redistributeCustom && oldImpressionTotals > 0)
                            {
                                var totalLockedRowsCount = request.Weeks.Count(w => w.IsLocked);                                
                                if (totalLockedRowsCount > 0)
                                {
                                    var totalImpressions = request.TotalImpressions;
                                    var totalLockedImpressions = request.Weeks.Where(w => w.IsLocked).Sum(w => w.WeeklyImpressions);
                                    week.WeeklyImpressions = Math.Floor((totalImpressions - totalLockedImpressions) / totalUnlockedActiveRowsCount);
                                }
                                else
                                {
                                    week.WeeklyImpressions = Math.Floor(request.TotalImpressions / totalUnlockedActiveRowsCount);
                                }
                            }
                            else
                            {
                                if (week.IsUpdated)
                                {
                                    week.WeeklyImpressions = Math.Floor(week.WeeklyImpressions);                                    
                                }
                                else
                                {
                                    week.WeeklyImpressions = Math.Floor(request.TotalImpressions / totalUnlockedActiveRowsCount);
                                }
                            }

                            break;
                    }
                    week.IsUpdated = false;
                }

                _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                    , request.TotalBudget, week, week.WeeklyImpressions, roundRatings: true);
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
            List<StandardDaypartWeightingGoal> standardDayparts)
        {
            foreach (DisplayMediaWeek week in weeks)
            {
                foreach (var item in standardDayparts)
                {
                    var planDayparts = request.Dayparts.Where(d => d.DaypartUniquekey == item.DaypartUniquekey).ToList();
                    var activeDays = _CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, planDayparts, out string activeDaysString);
                    result.Weeks.Add(new WeeklyBreakdownWeek
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
                        PlanDaypartId = item.PlanDaypartId
                    });
                }
            }
        }

        private void _AddNewWeeksToWeeklyBreakdownResult(WeeklyBreakdownResponseDto result, List<DisplayMediaWeek> weeks, WeeklyBreakdownRequest request)
        {
            foreach (DisplayMediaWeek week in weeks)
            {
                var activeDays = _CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, request.Dayparts, out string activeDaysString);

                result.Weeks.Add(new WeeklyBreakdownWeek
                {
                    ActiveDays = activeDaysString,
                    NumberOfActiveDays = activeDays,
                    StartDate = week.WeekStartDate,
                    EndDate = week.WeekEndDate,
                    MediaWeekId = week.Id,
                    IsLocked = request.Weeks.Where(x => x.MediaWeekId == week.Id).Select(x => x.IsLocked).FirstOrDefault()
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
                var activeDays = _CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, request.Dayparts, out string activeDaysString);

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
                        SpotLengthDuration = _GetWeeklySpotLengthDuration(creativeLength.SpotLengthId),
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
                breakdownItem.IsLocked = false;
                _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                    , request.TotalBudget, breakdownItem, impressions, roundRatings: true);
            }
        }

        private void _UpdateFirstWeekAndBudgetAdjustment(
           WeeklyBreakdownRequest request,
           List<WeeklyBreakdownWeek> weeks,
           double totalImpressions)
        {
            var unlockedWeeks = weeks.Where(w => w.IsLocked == false).ToList();
            if (!unlockedWeeks.Any())
            {
                return;
            }

            var firstWeek = unlockedWeeks.First();
            var totalImpressionsRounded = weeks.Sum(w => w.WeeklyImpressions);
            var roundedImpressionsDifference = totalImpressions - totalImpressionsRounded;
            var impressions = Math.Floor(firstWeek.WeeklyImpressions + roundedImpressionsDifference);

            _UpdateGoalsForWeeklyBreakdownItem(request.TotalImpressions, request.TotalRatings
                    , request.TotalBudget, firstWeek, impressions, roundRatings: true);
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
                        SpotLengthDuration = _GetWeeklySpotLengthDuration(creativeLength.SpotLengthId),
                        PercentageOfWeek = creativeLength.Weight
                    });
                }
            }
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

        internal int _CalculateActiveDays(DateTime weekStartDate, DateTime weekEndDate,
            List<int> flightDays, List<DateTime> hiatusDays, List<PlanDaypartDto> planDayparts,
            out string activeDaysString)
        {
            var daysOfWeek = new List<string> { "M", "Tu", "W", "Th", "F", "Sa", "Su" };
            activeDaysString = string.Empty;
            var hiatusDaysInWeek = hiatusDays.Where(x => weekStartDate <= x && weekEndDate >= x).ToList();

            var days = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            var planDaypartDayIds = _GetDaypartDayIds(planDayparts);
            var nonPlanDaypartDays = days.Except(planDaypartDayIds);

            var nonFlightDays = days.Except(flightDays);

            var toRemove = nonFlightDays.Union(nonPlanDaypartDays).Distinct();
            foreach (var day in toRemove)
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
            double impressions,
            bool roundRatings)
        {
            var budgetPerOneImpression = totalBudget / (decimal)totalImpressions;
            var weeklyRatio = impressions / totalImpressions;

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
                week.WeeklyUnits=Math.Round(week.WeeklyUnits,2);
            }
        }

        /// <inheritdoc/>
        public double CalculateWeeklyADUImpressions(WeeklyBreakdownWeek week, bool? equivalized
            , double impressionsPerUnit, List<CreativeLength> creativeLengths)
        {
            if (equivalized ?? false)
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
            , bool? equivalized, int? spotLengthId, List<CreativeLength> creativeLengths = null)
        {
            if (impressionsPerUnit == 0)
            {   //for older plans, where the user did not set an impressions per unit value, we need to show the user the ADU value based on the old math
                return (int)(aduImpressions / _DefaultImpressionsPerUnitForOldPlans);
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
    }
}
