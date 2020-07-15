using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.PeerResolvers;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices.Plan
{
    public interface IPlanService : IApplicationService
    {
        /// <summary>
        /// Saves the plan.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="createdBy">The modified by.</param>
        /// <param name="createdDate">The modified date.</param>
        /// <param name="aggregatePlanSynchronously">
        /// Synchronous execution is required for tests 
        /// because the transaction scope locks DB and summary data can not be saved from another thread
        /// </param>
        /// <returns></returns>
        int SavePlan(PlanDto plan, string createdBy, DateTime createdDate, bool aggregatePlanSynchronously = false);

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="versionId">Optional: version id. If nothing is passed, it will return the latest version</param>
        /// <returns></returns>
        PlanDto GetPlan(int planId, int? versionId = null);

        /// <summary>
        /// Checks if a draft exist on the plan and returns the draft id
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>Dravt id</returns>
        int CheckForDraft(int planId);

        /// <summary>
        /// Gets the plan statuses.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetPlanStatuses();

        /// <summary>
        /// Gets the delivery types.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetPlanCurrencies();

        /// <summary>
        /// Calculates the specified plan budget.
        /// </summary>
        /// <param name="planBudget">The plan budget.</param>
        /// <returns>PlanBudgetDeliveryCalculator object</returns>
        PlanDeliveryBudget Calculate(PlanDeliveryBudget planBudget);

        /// <summary>
        /// Gets the delivery spread for the weekly breakdown.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> PlanGoalBreakdownTypes();

        /// <summary>
        /// Calculates the weekly breakdown.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>WeeklyBreakdownResponse object</returns>
        WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request);

        /// <summary>
        /// Gets the plan defaults.
        /// </summary>
        /// <returns></returns>
        PlanDefaultsDto GetPlanDefaults();

        /// <summary>
        /// Locks the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanLockResponse LockPlan(int planId);

        /// <summary>
        /// Gets the plan history.
        /// </summary>
        /// <param name="planId">Plan identifier</param>
        /// <returns>List of PlanHistoryDto objects</returns>
        List<PlanVersionDto> GetPlanHistory(int planId);

        /// <summary>
        /// Deletes the plan draft.
        /// </summary>        
        /// <param name="planId">The plan identifier.</param>
        /// <returns>True if the delete was successful</returns>
        bool DeletePlanDraft(int planId);

        /// <summary>
        /// Automatics the status transitions hangfire job entry point.
        /// </summary>
        [Queue("planstatustransition")]
        void AutomaticStatusTransitionsJobEntryPoint();

        /// <summary>
        /// The logic for automatic status transitioning
        /// </summary>
        /// <param name="transitionDate">The transition date.</param>
        /// <param name="updatedBy">The updated by.</param>
        /// <param name="updatedDate">The updated date.</param>
        /// <param name="aggregatePlanSynchronously"></param>
        void AutomaticStatusTransitions(DateTime transitionDate, string updatedBy, DateTime updatedDate, bool aggregatePlanSynchronously = false);

        CurrentQuartersDto GetCurrentQuarters(DateTime currentDateTime);


        /// <summary>
        /// Calculates the creative length weight.
        /// </summary>
        /// <param name="request">Creative lengths set on the plan.</param>
        /// <returns>List of creative lengths with calculated values</returns>
        List<CreativeLength> CalculateCreativeLengthWeight(List<CreativeLength> request);

        /// <summary>
        /// Calculates the length make up table.
        /// </summary>
        /// <param name="request">The request object containing creative lengths and weekly breakdown table weeks.</param>
        /// <returns>List of LengthMakeUpTableRow objects</returns>
        List<LengthMakeUpTableRow> CalculateLengthMakeUpTable(LengthMakeUpRequest request);
    }

    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanValidator _PlanValidator;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;
        private readonly IPlanBudgetDeliveryCalculator _BudgetCalculator;
        private readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly IPlanAggregator _PlanAggregator;
        private readonly IPlanSummaryRepository _PlanSummaryRepository;
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;
        private readonly ICampaignRepository _CampaignRepository;
        private readonly ICampaignAggregationJobTrigger _CampaignAggregationJobTrigger;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IBroadcastLockingManagerApplicationService _LockingManagerApplicationService;
        private readonly IPlanPricingService _PlanPricingService;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IDaypartDefaultService _DaypartDefaultService;
        private readonly IDayRepository _DayRepository;
        private readonly IWeeklyBreakdownEngine _WeeklyBreakdownEngine;

        private const string _DaypartDefaultNotFoundMessage = "Unable to find daypart default";
        private const string _UnsupportedDeliveryTypeMessage = "Unsupported Delivery Type";

        public PlanService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IPlanValidator planValidator
            , IPlanBudgetDeliveryCalculator planBudgetDeliveryCalculator
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IPlanAggregator planAggregator
            , ICampaignAggregationJobTrigger campaignAggregationJobTrigger
            , ISpotLengthEngine spotLengthEngine
            , IBroadcastLockingManagerApplicationService lockingManagerApplicationService
            , IPlanPricingService planPricingService
            , IQuarterCalculationEngine quarterCalculationEngine
            , IDaypartDefaultService daypartDefaultService
            , IWeeklyBreakdownEngine weeklyBreakdownEngine
            , ICreativeLengthEngine creativeLengthEngine)
        {
            _MediaWeekCache = mediaMonthAndWeekAggregateCache;
            _PlanValidator = planValidator;
            _BudgetCalculator = planBudgetDeliveryCalculator;

            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _CampaignRepository = broadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _PlanSummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanSummaryRepository>();
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
            _DayRepository = broadcastDataRepositoryFactory.GetDataRepository<IDayRepository>();
            _PlanAggregator = planAggregator;
            _CampaignAggregationJobTrigger = campaignAggregationJobTrigger;
            _SpotLengthEngine = spotLengthEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _PlanPricingService = planPricingService;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _DaypartDefaultService = daypartDefaultService;
            _WeeklyBreakdownEngine = weeklyBreakdownEngine;
            _CreativeLengthEngine = creativeLengthEngine;
        }

        ///<inheritdoc/>
        public int SavePlan(PlanDto plan, string createdBy, DateTime createdDate, bool aggregatePlanSynchronously = false)
        {
            if (plan.Id >= 0 && _PlanPricingService.IsPricingModelRunningForPlan(plan.Id))
            {
                throw new Exception("The pricing model is running for the plan");
            }

            if (plan.CreativeLengths.Count == 1)
            {//if there is only 1 creative length, set the weight to 100%
                plan.CreativeLengths.Single().Weight = 100;
            }
            else
            {
                plan.CreativeLengths = _CreativeLengthEngine.DistributeWeight(plan.CreativeLengths);
            }

            DaypartTimeHelper.SubtractOneSecondToEndTime(plan.Dayparts);

            _CalculateDaypartOverrides(plan.Dayparts);
            _PlanValidator.ValidatePlan(plan);

            _ConvertImpressionsToRawFormat(plan);
            _DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);
            _CalculateDeliveryDataPerAudience(plan);
            _SetPlanVersionNumber(plan);
            _SetPlanFlightDays(plan);

            if (plan.Status == PlanStatusEnum.Contracted && plan.GoalBreakdownType != PlanGoalBreakdownTypeEnum.CustomByWeek)
            {
                plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeek;
            }

            _VerifyWeeklyAdu(plan.IsAduEnabled, plan.WeeklyBreakdownWeeks);

            if (plan.VersionId == 0 || plan.Id == 0)
            {
                _PlanRepository.SaveNewPlan(plan, createdBy, createdDate);
            }
            else
            {
                var key = KeyHelper.GetPlanLockingKey(plan.Id);
                var lockingResult = _LockingManagerApplicationService.GetLockObject(key);

                if (lockingResult.Success)
                {
                    if (plan.IsDraft)
                    {
                        _PlanRepository.CreateOrUpdateDraft(plan, createdBy, createdDate);
                    }
                    else
                    {
                        _PlanRepository.SavePlan(plan, createdBy, createdDate);
                    }
                }
                else
                {
                    throw new Exception($"The chosen plan has been locked by {lockingResult.LockedUserName}");
                }
            }
            _UpdateCampaignLastModified(plan.CampaignId, createdDate, createdBy);

            // We only aggregate data and run pricing for versions, not drafts.
            if (!plan.IsDraft)
            {
                _DispatchPlanAggregation(plan, aggregatePlanSynchronously);
                _CampaignAggregationJobTrigger.TriggerJob(plan.CampaignId, createdBy);

                // Running price job on plan creation/edits/saves its temporary disabled
                _SetPlanPricingParameters(plan);
                _PlanRepository.SavePlanPricingParameters(plan.PricingParameters);
                //_PlanPricingService.QueuePricingJob(plan.PricingParameters, createdDate, createdBy);
            }

            return plan.Id;
        }

        /// <summary>
        /// Based on the plan delivery type, splits weekly breakdown into all combinations of plan ad lengths and dayparts
        /// And distributes goals based on ad length and daypart weights
        /// </summary>
        private void _DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(PlanDto plan)
        {
            if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.EvenDelivery ||
                plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeek)
            {
                plan.WeeklyBreakdownWeeks = _DistributeGoals_ByWeekDeliveryType(plan);
            }
            else if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                plan.WeeklyBreakdownWeeks = _DistributeGoals_ByWeekByAdLengthDeliveryType(plan);
            }
            else if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart)
            {
                plan.WeeklyBreakdownWeeks = _DistributeGoals_ByWeekByDaypartDeliveryType(plan);
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
        private List<WeeklyBreakdownWeek> _DistributeGoals_ByWeekByDaypartDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();

            var weeks = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks);

            foreach (var weeklyBreakdown in plan.WeeklyBreakdownWeeks)
            {
                var aduImpressionsForBreakdownItem = _WeeklyBreakdownEngine.CalculateWeeklyADUImpressions(weeklyBreakdown, plan.Equivalized
                                            , plan.ImpressionsPerUnit, plan.CreativeLengths);
                var unitsImpressionsForBreakdownItem = weeklyBreakdown.WeeklyImpressions / weeklyBreakdown.WeeklyUnits;
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
                        DaypartCodeId = weeklyBreakdown.DaypartCodeId,
                        AduImpressions = aduImpressionsForBreakdownItem * weighting,
                        UnitImpressions = unitsImpressionsForBreakdownItem * weighting
                    };

                    var impressions = weeklyBreakdown.WeeklyImpressions * weighting;

                    _UpdateGoalsForWeeklyBreakdownItem(plan.TargetImpressions.Value,
                        plan.TargetRatingPoints.Value,
                        plan.Budget.Value,
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
        private List<WeeklyBreakdownWeek> _DistributeGoals_ByWeekByAdLengthDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();
            var weeks = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks);
            var standardDaypardWeightingGoals = _WeeklyBreakdownEngine.GetStandardDaypardWeightingGoals(plan.Dayparts);
            foreach (var week in weeks)
            {
                foreach (var breakdownItem in plan.WeeklyBreakdownWeeks.Where(x => x.MediaWeekId == week.MediaWeekId))
                {
                    double aduImpressionsForBreakdownItem = _WeeklyBreakdownEngine.CalculateWeeklyADUImpressions(breakdownItem, plan.Equivalized
                        , plan.ImpressionsPerUnit, plan.CreativeLengths);
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
                            DaypartCodeId = item.DaypartDefaultId,
                            AduImpressions = aduImpressionsForBreakdownItem * weighting,
                            UnitImpressions = unitsImpressionsForBreakdownItem * weighting
                        };

                        var impressions = breakdownItem.WeeklyImpressions * weighting;

                        _UpdateGoalsForWeeklyBreakdownItem(
                            plan.TargetImpressions.Value,
                            plan.TargetRatingPoints.Value,
                            plan.Budget.Value,
                            newWeeklyBreakdownItem,
                            impressions,
                            roundRatings: false);

                        newWeeklyBreakdownItem.PercentageOfWeek = _CalculatePercentageOfWeek(impressions, week.Impressions);

                        result.Add(newWeeklyBreakdownItem);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Splits each weekly breakdown record using plan daypart and ad length weighting goals to distribute the goals of each record
        /// </summary>
        /// <returns>A list of breakdown records with 'by week by ad length by daypart' structure</returns>
        private List<WeeklyBreakdownWeek> _DistributeGoals_ByWeekDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();

            var allSpotLengthIdAndStandardDaypartIdCombinations =
                _WeeklyBreakdownEngine.GetWeeklyBreakdownCombinations(plan.CreativeLengths, plan.Dayparts);

            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                var weeklyAduImpressions = _WeeklyBreakdownEngine.CalculateWeeklyADUImpressions(week, plan.Equivalized
                    , plan.ImpressionsPerUnit, plan.CreativeLengths);
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
                        DaypartCodeId = combination.DaypartCodeId,
                        AduImpressions = weeklyAduImpressions * combination.Weighting,
                        UnitImpressions = unitsImpressions * combination.Weighting
                    };

                    var impressions = week.WeeklyImpressions * combination.Weighting;

                    _UpdateGoalsForWeeklyBreakdownItem(
                        plan.TargetImpressions.Value,
                        plan.TargetRatingPoints.Value,
                        plan.Budget.Value,
                        newWeeklyBreakdownItem,
                        impressions,
                        roundRatings: false);

                    newWeeklyBreakdownItem.PercentageOfWeek = _CalculatePercentageOfWeek(impressions, week.WeeklyImpressions);

                    result.Add(newWeeklyBreakdownItem);
                }
            }

            return result;
        }

        private void _UpdateCampaignLastModified(int campaignId, DateTime modifiedDate, string modifiedBy)
        {
            var key = KeyHelper.GetCampaignLockingKey(campaignId);
            var lockingResult = _LockingManagerApplicationService.GetLockObject(key);

            if (lockingResult.Success)
            {
                _CampaignRepository.UpdateCampaignLastModified(campaignId, modifiedDate, modifiedBy);
            }
            else
            {
                throw new Exception($"The chosen campaign has been locked by {lockingResult.LockedUserName}");
            }
        }

        private void _SetPlanPricingParameters(PlanDto plan)
        {
            var pricingDefaults = _PlanPricingService.GetPlanPricingDefaults();

            plan.PricingParameters = new PlanPricingParametersDto
            {
                PlanId = plan.Id,
                Budget = Convert.ToDecimal(plan.Budget),
                CPM = Convert.ToDecimal(plan.TargetCPM),
                CPP = Convert.ToDecimal(plan.TargetCPP),
                Currency = plan.Currency,
                DeliveryImpressions = Convert.ToDouble(plan.TargetImpressions) / 1000,
                DeliveryRatingPoints = Convert.ToDouble(plan.TargetRatingPoints),
                UnitCaps = pricingDefaults.UnitCaps,
                UnitCapsType = pricingDefaults.UnitCapType,
                InventorySourcePercentages = pricingDefaults.InventorySourcePercentages,
                InventorySourceTypePercentages = pricingDefaults.InventorySourceTypePercentages,
                Margin = pricingDefaults.Margin,
                PlanVersionId = plan.VersionId,
                MarketGroup = pricingDefaults.MarketGroup
            };

            _PlanPricingService.ValidateAndApplyMargin(plan.PricingParameters);
        }

        private void _SetPlanFlightDays(PlanDto plan)
        {
            // If no flight days were sent by the FE, we save the default seven days.
            if (plan.FlightDays != null && plan.FlightDays.Any())
                return;

            var days = _DayRepository.GetDays();
            plan.FlightDays = new List<int>();
            plan.FlightDays.AddRange(days.Select(x => x.Id));
        }

        private void _VerifyWeeklyAdu(bool isAduEnabled, List<WeeklyBreakdownWeek> weeks)
        {
            if (isAduEnabled) return;

            foreach (var week in weeks)
                week.AduImpressions = 0;
        }

        private void _ConvertImpressionsToRawFormat(PlanDto plan)
        {
            //the UI is sending the user entered value instead of the raw value. BE needs to adjust
            if (plan.TargetImpressions.HasValue)
            {
                plan.TargetImpressions = plan.TargetImpressions.Value * 1000;
            }
            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                week.WeeklyImpressions = week.WeeklyImpressions * 1000;
            }
        }

        private void _ConvertImpressionsToUserFormat(PlanDto plan)
        {
            //the UI is sending the user entered value instead of the raw value. BE needs to adjust
            if (plan.TargetImpressions.HasValue)
            {
                plan.TargetImpressions /= 1000;
            }
            plan.HHImpressions /= 1000;
            foreach (var audience in plan.SecondaryAudiences)
            {
                if (audience.Impressions.HasValue)
                {
                    audience.Impressions /= 1000;
                }
            }
            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                week.WeeklyImpressions /= 1000;
            }
        }

        private void _CalculateDaypartOverrides(List<PlanDaypartDto> planDayparts)
        {
            var daypartDefaults = _DaypartDefaultRepository.GetAllDaypartDefaultsWithAllData();

            foreach (var planDaypart in planDayparts)
            {
                var daypartDefault = daypartDefaults.Single(x => x.Id == planDaypart.DaypartCodeId, _DaypartDefaultNotFoundMessage);

                planDaypart.IsStartTimeModified = planDaypart.StartTimeSeconds != daypartDefault.DefaultStartTimeSeconds;
                planDaypart.IsEndTimeModified = planDaypart.EndTimeSeconds != daypartDefault.DefaultEndTimeSeconds;
            }
        }

        ///<inheritdoc/>
        public PlanDto GetPlan(int planId, int? versionId = null)
        {
            PlanDto plan = _PlanRepository.GetPlan(planId, versionId);

            plan.IsPricingModelRunning = _PlanPricingService.IsPricingModelRunningForPlan(planId);

            _GroupWeeklyBreakdownWeeksBasedOnDeliveryType(plan);
            _WeeklyBreakdownEngine.SetWeekNumber(plan.WeeklyBreakdownWeeks);
            _SetWeeklyBreakdownTotals(plan);
            DaypartTimeHelper.AddOneSecondToEndTime(plan.Dayparts);

            _SetPlanTotals(plan);
            _SetDefaultDaypartRestrictions(plan);
            _ConvertImpressionsToUserFormat(plan);

            _SortPlanDayparts(plan);
            _SortProgramRestrictions(plan);
            _SortCreativeLengths(plan);
            _SetSpotLengthForPlanBackwardCompatibility(plan);
            return plan;
        }

        /// <summary>
        /// Because in DB we store weekly breakdown split 'by week by ad length by daypart'
        /// we need to group them back based on the plan delivery type so that on UI the breakdown table looks like it looked before saving
        /// </summary>
        private void _GroupWeeklyBreakdownWeeksBasedOnDeliveryType(PlanDto plan)
        {
            if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.EvenDelivery ||
                plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeek)
            {
                plan.WeeklyBreakdownWeeks = _GroupWeeklyBreakdownWeeks_ByWeekDeliveryType(plan);
            }
            else if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                plan.WeeklyBreakdownWeeks = _GroupWeeklyBreakdownWeeks_ByWeekByAdLengthDeliveryType(plan);
            }
            else if (plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart)
            {
                plan.WeeklyBreakdownWeeks = _GroupWeeklyBreakdownWeeks_ByWeekByDaypartDeliveryType(plan);
            }
            else
            {
                throw new ApplicationException(_UnsupportedDeliveryTypeMessage);
            }
        }

        /// <summary>
        /// Because in DB we store weekly breakdown split 'by week by ad length by daypart'
        /// we need to group them back based on the plan delivery type so that on UI the breakdown table looks like it looked before saving
        /// </summary>
        private List<WeeklyBreakdownWeek> _GroupWeeklyBreakdownWeeks_ByWeekByDaypartDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();
            var weeklyBreakdownByWeekByDaypart = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeekByDaypart(plan.WeeklyBreakdownWeeks, plan.ImpressionsPerUnit, plan.Equivalized, plan.CreativeLengths);

            foreach (var item in weeklyBreakdownByWeekByDaypart)
            {
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

            _WeeklyBreakdownEngine.RecalculatePercentageOfWeekBasedOnImpressions(result);

            return result;
        }

        /// <summary>
        /// Groups weekly breakdown by week by ad length
        /// </summary>
        private List<WeeklyBreakdownWeek> _GroupWeeklyBreakdownWeeks_ByWeekByAdLengthDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();
            var weeklyBreakdownByWeekBySpotLength =
                _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeekBySpotLength(plan.WeeklyBreakdownWeeks, plan.ImpressionsPerUnit, plan.Equivalized);

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
                    WeeklyAdu = item.Adu,
                    WeeklyUnits = item.Units,
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

            _WeeklyBreakdownEngine.RecalculatePercentageOfWeekBasedOnImpressions(result);

            return result;
        }

        /// <summary>
        /// Groups weekly breakdown by week
        /// </summary>
        private List<WeeklyBreakdownWeek> _GroupWeeklyBreakdownWeeks_ByWeekDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();
            var weeks = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks, plan.ImpressionsPerUnit
                , plan.CreativeLengths, plan.Equivalized);

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
                    WeeklyUnits = week.Units
                };

                _UpdateGoalsForWeeklyBreakdownItem(
                       plan.TargetImpressions.Value,
                       plan.TargetRatingPoints.Value,
                       plan.Budget.Value,
                       newWeeklyBreakdownItem,
                       week.Impressions,
                       roundRatings: true);

                result.Add(newWeeklyBreakdownItem);
            }

            return result;
        }

        private void _SetSpotLengthForPlanBackwardCompatibility(PlanDto plan)
        {
            if (plan.CreativeLengths == null)
            {
                plan.SpotLengthId = 1;
            }
            else
            {
                plan.SpotLengthId = plan.CreativeLengths.First().SpotLengthId;
            }
        }

        private void _SortCreativeLengths(PlanDto plan)
        {
            var spotLengths = _SpotLengthEngine.GetSpotLengths();
            plan.CreativeLengths = plan.CreativeLengths
                .OrderBy(x => spotLengths.Where(y => y.Value == x.SpotLengthId).Single().Key)
                .ToList();
        }

        private void _SetWeeklyBreakdownTotals(PlanDto plan)
        {
            var weeklyBreakdownByWeek = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks);

            plan.WeeklyBreakdownTotals.TotalActiveDays = weeklyBreakdownByWeek.Sum(w => w.NumberOfActiveDays);

            plan.WeeklyBreakdownTotals.TotalImpressions = Math.Floor(plan.WeeklyBreakdownWeeks.Sum(w => w.WeeklyImpressions) / 1000);
            var impressionsTotalsRatio = plan.TargetImpressions.HasValue && plan.TargetImpressions.Value > 0
                ? plan.WeeklyBreakdownWeeks.Sum(w => w.WeeklyImpressions) / plan.TargetImpressions.Value : 0;

            plan.WeeklyBreakdownTotals.TotalRatingPoints = Math.Round(plan.TargetRatingPoints ?? 0 * impressionsTotalsRatio, 1);
            plan.WeeklyBreakdownTotals.TotalImpressionsPercentage = Math.Round(GeneralMath.ConvertFractionToPercentage(impressionsTotalsRatio), 0);
            plan.WeeklyBreakdownTotals.TotalBudget = plan.Budget.Value * (decimal)impressionsTotalsRatio;
            plan.WeeklyBreakdownTotals.TotalUnits = Math.Round(plan.WeeklyBreakdownWeeks.Sum(w => w.WeeklyUnits), 2);
        }

        private void _SortPlanDayparts(PlanDto plan)
        {
            var daypartDefaults = _DaypartDefaultService.GetAllDaypartDefaults();

            var planDayparts = plan.Dayparts.Select(x => new PlanDaypart
            {
                DaypartCodeId = x.DaypartCodeId,
                DaypartTypeId = x.DaypartTypeId,
                StartTimeSeconds = x.StartTimeSeconds,
                EndTimeSeconds = x.EndTimeSeconds,
                IsEndTimeModified = x.IsEndTimeModified,
                IsStartTimeModified = x.IsStartTimeModified,
                Restrictions = x.Restrictions,
                WeightingGoalPercent = x.WeightingGoalPercent,
                FlightDays = plan.FlightDays.ToList(),
                VpvhForAudiences = x.VpvhForAudiences,
            }).ToList();

            plan.Dayparts = planDayparts.OrderDayparts(daypartDefaults);
        }

        private void _SortProgramRestrictions(PlanDto plan)
        {
            foreach (var daypart in plan.Dayparts)
            {
                daypart.Restrictions.ProgramRestrictions.Programs = daypart.Restrictions.ProgramRestrictions.Programs.OrderBy(x => x.Name).ToList();
            }
        }

        /// <inheritdoc/>
        public List<PlanVersionDto> GetPlanHistory(int planId)
        {
            var planVersions = _PlanRepository.GetPlanHistory(planId);

            List<PlanVersionDto> result = _MapToPlanHistoryDto(planVersions);
            result = result.OrderByDescending(x => x.IsDraft == true).ThenByDescending(x => x.ModifiedDate).ToList();

            _CompareVersions(planVersions, result);

            return result;
        }

        private void _CompareVersions(List<PlanVersion> planVersions, List<PlanVersionDto> result)
        {
            //based on the ordering done in the calling method, the version we compare with is the first one in the result list
            var baseVersion = planVersions.Single(x => x.VersionId == result.First().VersionId);

            foreach (var version in planVersions.Where(x => x.VersionId != baseVersion.VersionId).ToList())
            {
                var resultedVersion = result.Single(x => x.VersionId == version.VersionId);
                resultedVersion.IsModifiedBudget = (baseVersion.Budget != version.Budget);
                resultedVersion.IsModifiedFlight = _CheckIfFlightIsModified(baseVersion, version, resultedVersion);
                resultedVersion.IsModifiedTargetAudience = (baseVersion.TargetAudienceId != version.TargetAudienceId);
                resultedVersion.IsModifiedTargetCPM = (baseVersion.TargetCPM != version.TargetCPM);
                resultedVersion.IsModifiedTargetImpressions = (baseVersion.TargetImpressions != version.TargetImpressions);
                resultedVersion.IsModifiedDayparts = _CheckIfDaypartsAreModified(baseVersion, version, resultedVersion);
            }
        }

        private bool _CheckIfDaypartsAreModified(PlanVersion baseVersion, PlanVersion version, PlanVersionDto resultedVersion)
        {
            //check if number of dayparts is different
            if (baseVersion.Dayparts.Count() != version.Dayparts.Count())
            {
                return true;
            }

            //check if the dayparts themselves are different
            foreach (var daypart in baseVersion.Dayparts)
            {
                if (!version.Dayparts.Contains(daypart))
                {
                    return true;
                }
            }

            return false;
        }

        private bool _CheckIfFlightIsModified(PlanVersion baseVersion, PlanVersion version, PlanVersionDto resultedVersion)
        {
            //check if start or end dates are different
            if (baseVersion.FlightStartDate != version.FlightStartDate || baseVersion.FlightEndDate != version.FlightEndDate)
            {
                return true;
            }

            //check if number of hiatus days is different
            if (baseVersion.HiatusDays.Count() != version.HiatusDays.Count())
            {
                return true;
            }

            //check if the hiatus days themselves are different
            foreach (var date in baseVersion.HiatusDays)
            {
                if (!version.HiatusDays.Contains(date))
                {
                    return true;
                }
            }

            return false;
        }

        //this method only maps the db fields
        private List<PlanVersionDto> _MapToPlanHistoryDto(List<PlanVersion> planVersions)
        {
            return planVersions.Select(x => new PlanVersionDto
            {
                Budget = x.Budget,
                TargetCPM = x.TargetCPM,
                TargetImpressions = x.TargetImpressions,
                FlightEndDate = x.FlightEndDate,
                FlightStartDate = x.FlightStartDate,
                IsDraft = x.IsDraft,
                ModifiedBy = x.ModifiedBy,
                ModifiedDate = x.ModifiedDate,
                Status = EnumHelper.GetEnum<PlanStatusEnum>(x.Status),
                TargetAudienceId = x.TargetAudienceId,
                VersionId = x.VersionId,
                TotalDayparts = x.Dayparts.Count(),
                VersionName = _SetVersionName(x)
            }).ToList();
        }

        private string _SetVersionName(PlanVersion planVersion)
        {
            return planVersion.IsDraft ? "Draft" : $"Version {planVersion.VersionNumber.Value}";
        }

        /// <inheritdoc/>
        public int CheckForDraft(int planId)
        {
            return _PlanRepository.CheckIfDraftExists(planId);
        }

        private void _SetDefaultDaypartRestrictions(PlanDto plan)
        {
            var planDefaults = GetPlanDefaults();

            foreach (var daypart in plan.Dayparts)
            {
                var restrictions = daypart.Restrictions;

                if (restrictions.ShowTypeRestrictions == null)
                {
                    restrictions.ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                    {
                        ContainType = planDefaults.ShowTypeContainType,
                        ShowTypes = new List<LookupDto>()
                    };
                }

                if (restrictions.GenreRestrictions == null)
                {
                    restrictions.GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                    {
                        ContainType = planDefaults.GenreContainType,
                        Genres = new List<LookupDto>()
                    };
                }

                if (restrictions.ProgramRestrictions == null)
                {
                    restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                    {
                        ContainType = planDefaults.ProgramContainType,
                        Programs = new List<ProgramDto>()
                    };
                }

                if (restrictions.AffiliateRestrictions == null)
                {
                    restrictions.AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                    {
                        ContainType = planDefaults.AffiliateContainType,
                        Affiliates = new List<LookupDto>()
                    };
                }
            }
        }

        private static void _SetPlanTotals(PlanDto plan)
        {
            plan.TotalActiveDays = plan.WeeklyBreakdownTotals.TotalActiveDays;
            plan.TotalHiatusDays = plan.FlightHiatusDays.Count();
            plan.TotalShareOfVoice = plan.WeeklyBreakdownTotals.TotalImpressionsPercentage;
        }

        ///<inheritdoc/>
        public bool DeletePlanDraft(int planId)
        {
            _PlanRepository.DeletePlanDraft(planId);
            return true;
        }

        ///<inheritdoc/>
        public List<LookupDto> GetPlanStatuses()
        {
            return EnumExtensions.ToLookupDtoList<PlanStatusEnum>().OrderByDescending(x => x.Id == (int)PlanStatusEnum.Scenario).ThenBy(x => x.Id).ToList();
        }

        ///<inheritdoc/>
        public List<LookupDto> GetPlanCurrencies()
        {
            return Enum.GetValues(typeof(PlanCurrenciesEnum))
                .Cast<PlanCurrenciesEnum>()
                .Select(x => new LookupDto
                {
                    Id = (int)x,
                    Display = x.GetDescriptionAttribute()
                })
                .OrderBy(x => x.Id).ToList();
        }

        ///<inheritdoc/>
        public PlanDeliveryBudget Calculate(PlanDeliveryBudget planBudget)
        {
            var impressionsHasValue = planBudget.Impressions.HasValue;
            if (impressionsHasValue)
            {
                // the UI is sending the user entered value instead of the raw value. BE needs to adjust
                // this value is only adjusted for calculations
                planBudget.Impressions = planBudget.Impressions.Value * 1000;
            }

            planBudget = _BudgetCalculator.CalculateBudget(planBudget);

            planBudget.Impressions = Math.Floor(planBudget.Impressions.Value / 1000);


            return planBudget;
        }

        ///<inheritdoc/>
        public List<LookupDto> PlanGoalBreakdownTypes()
        {
            return EnumExtensions.ToLookupDtoList<PlanGoalBreakdownTypeEnum>();
        }

        /// <summary>
        /// Gets the current quarters.
        ///
        /// The first start date is "the following Monday" from "today".
        /// The list contains the current quarter and the following four quarters.
        /// </summary>
        /// <param name="currentDateTime">The current date time.</param>
        /// <returns></returns>
        public CurrentQuartersDto GetCurrentQuarters(DateTime currentDateTime)
        {
            const int monthModifierForQuery = 14;
            const int totalQuartersToReturn = 5;
            const int daysOfTheWeek = 7;
            const string dateFormat = "MM/dd/yyyy 00:00:00";

            // ... + 7) % 7) to ensure we get range between 0 and 7.
            var daysToAdd = currentDateTime.DayOfWeek == DayOfWeek.Monday ? daysOfTheWeek
                : ((int)DayOfWeek.Monday - (int)currentDateTime.DayOfWeek + daysOfTheWeek) % daysOfTheWeek;
            var followingMonday = DateTime.Parse(currentDateTime.AddDays(daysToAdd).ToString(dateFormat));

            var endDate = currentDateTime.AddMonths(monthModifierForQuery);
            var quarters = _QuarterCalculationEngine.GetAllQuartersBetweenDates(currentDateTime, endDate)
                .Take(totalQuartersToReturn).ToList();

            var currentQuarters = new CurrentQuartersDto
            {
                FirstStartDate = followingMonday,
                Quarters = quarters
            };

            return currentQuarters;
        }

        ///<inheritdoc/>
        public WeeklyBreakdownResponseDto CalculatePlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request)
        {
            return _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);
        }

        private double _CalculatePercentageOfWeek(double breakdownItemImpressions, double weeklyImpressions)
        {
            return weeklyImpressions > 0 ? Math.Round(100 * breakdownItemImpressions / weeklyImpressions) : 0;
        }

        // attribute has to be on the class instead of the interface because this is a recurring job.
        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 5 * 60 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void AutomaticStatusTransitionsJobEntryPoint()
        {
            var transitionDate = DateTime.Today;
            var updatedBy = "automated status update";
            var updatedDate = DateTime.Now;
            AutomaticStatusTransitions(transitionDate, updatedBy, updatedDate, false);
        }

        public void AutomaticStatusTransitions(DateTime transitionDate, string updatedBy, DateTime updatedDate, bool aggregatePlanSynchronously = false)
        {
            var plansToTransition = _PlanRepository.GetPlansForAutomaticTransition(transitionDate);
            foreach (var plan in plansToTransition)
            {
                var key = KeyHelper.GetPlanLockingKey(plan.Id);
                var lockingResult = _LockingManagerApplicationService.LockObject(key);

                if (lockingResult.Success)
                {
                    if (plan.Status == PlanStatusEnum.Contracted)
                    {
                        plan.Status = PlanStatusEnum.Live;
                    }
                    else if (plan.Status == PlanStatusEnum.Live)
                    {
                        plan.Status = PlanStatusEnum.Complete;
                    }

                    _SetPlanVersionNumber(plan);
                    _PlanRepository.SavePlan(plan, updatedBy, updatedDate);

                    _DispatchPlanAggregation(plan, aggregatePlanSynchronously);
                    _CampaignAggregationJobTrigger.TriggerJob(plan.CampaignId, updatedBy);
                }
                else
                {
                    throw new Exception($"The chosen plan has been locked by {lockingResult.LockedUserName}");
                }

                _LockingManagerApplicationService.ReleaseObject(key);
            }
        }

        private void _SetPlanVersionNumber(PlanDto plan)
        {
            if (plan.VersionId == 0 || plan.Id == 0)
            {
                //this is a new plan, so we're saving version 1
                plan.VersionNumber = 1;
            }
            else if (plan.IsDraft)
            {
                //this is a draft. we create it if none exist or we update it otherwise
                plan.VersionNumber = null;
            }
            else
            {
                //this is a new version.
                plan.VersionNumber = _PlanRepository.GetLatestVersionNumberForPlan(plan.Id) + 1;
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
            var budgetPerOneImpression = totalBudget / (decimal)totalImpressions;
            var weeklyRatio = impressions / totalImpressions;

            breakdownItem.WeeklyImpressions = impressions;
            breakdownItem.WeeklyImpressionsPercentage = Math.Round(100 * weeklyRatio);
            breakdownItem.WeeklyBudget = (decimal)impressions * budgetPerOneImpression;

            var ratings = totalRatings * weeklyRatio;
            breakdownItem.WeeklyRatings = roundRatings ? ProposalMath.RoundDownWithDecimals(ratings, 1) : ratings;
        }

        private void _DispatchPlanAggregation(PlanDto plan, bool aggregatePlanSynchronously)
        {
            _PlanSummaryRepository.SetProcessingStatusForPlanSummary(plan.VersionId, PlanAggregationProcessingStatusEnum.InProgress);

            if (aggregatePlanSynchronously)
            {
                _AggregatePlan(plan);
            }
            else
            {
                Task.Factory.StartNew(() => _AggregatePlan(plan));
            }
        }

        private void _AggregatePlan(PlanDto plan)
        {
            try
            {
                var summary = _PlanAggregator.Aggregate(plan);
                summary.ProcessingStatus = PlanAggregationProcessingStatusEnum.Idle;
                _PlanSummaryRepository.SaveSummary(summary);
            }
            catch (Exception)
            {
                _PlanSummaryRepository.SetProcessingStatusForPlanSummary(plan.VersionId, PlanAggregationProcessingStatusEnum.Error);
            }
        }

        public PlanDefaultsDto GetPlanDefaults()
        {
            const int defaultSpotLength = 30;
            var defaultSpotLengthId = _SpotLengthEngine.GetSpotLengthIdByValue(defaultSpotLength);

            return new PlanDefaultsDto
            {
                Name = string.Empty,
                AudienceId = BroadcastConstants.HouseholdAudienceId,
                CreativeLengths = new List<CreativeLength> {
                    new CreativeLength
                    {
                        SpotLengthId = defaultSpotLengthId
                    }
                },
                Equivalized = true,
                AudienceType = AudienceTypeEnum.Nielsen,
                PostingType = PostingTypeEnum.NTI,
                Status = PlanStatusEnum.Working,
                Currency = PlanCurrenciesEnum.Impressions,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                ShowTypeContainType = ContainTypeEnum.Exclude,
                GenreContainType = ContainTypeEnum.Exclude,
                ProgramContainType = ContainTypeEnum.Exclude,
                AffiliateContainType = ContainTypeEnum.Exclude,
                CoverageGoalPercent = 80d,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
            };
        }

        private void _CalculateDeliveryDataPerAudience(PlanDto plan)
        {
            var hhImpressionsByStandardDaypart = _GetHhImpressionsByStandardDaypart(plan);
            var totalHhImpressions = Math.Floor(hhImpressionsByStandardDaypart.Sum(x => x.Value));

            _CalculateHouseholdDeliveryData(plan, totalHhImpressions);
            _CalculateSecondaryAudiencesDeliveryData(plan, hhImpressionsByStandardDaypart, totalHhImpressions);
        }

        private Dictionary<int, double> _GetHhImpressionsByStandardDaypart(PlanDto plan)
        {
            var result = new Dictionary<int, double>();

            var weeklyBreakdownByStandardDaypart = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByStandardDaypart(plan.WeeklyBreakdownWeeks);

            foreach (var item in weeklyBreakdownByStandardDaypart)
            {
                var targetAudienceImpressions = item.Impressions;
                var targetAudienceVpvh = plan
                    .Dayparts.Single(x => x.DaypartCodeId == item.StandardDaypartId)
                    .VpvhForAudiences.Single(x => x.AudienceId == plan.AudienceId)
                    .Vpvh;

                result[item.StandardDaypartId] = ProposalMath.CalculateHhImpressionsUsingVpvh(targetAudienceImpressions, targetAudienceVpvh);
            }

            return result;
        }

        private Dictionary<int, double> _GetAudienceImpressionsByStandardDaypart(
            PlanDto plan,
            Dictionary<int, double> hhImpressionsByStandardDaypart,
            int audienceId)
        {
            var result = new Dictionary<int, double>();

            foreach (var daypart in plan.Dayparts)
            {
                var hhImpressions = hhImpressionsByStandardDaypart[daypart.DaypartCodeId];
                var audienceVpvh = daypart.VpvhForAudiences.Single(x => x.AudienceId == audienceId).Vpvh;

                result[daypart.DaypartCodeId] = ProposalMath.CalculateAudienceImpressionsUsingVpvh(hhImpressions, audienceVpvh);
            }

            return result;
        }

        private void _CalculateHouseholdDeliveryData(PlanDto plan, double hhImpressions)
        {
            var householdPlanDeliveryBudget = _BudgetCalculator.CalculateBudget(new PlanDeliveryBudget
            {
                Impressions = hhImpressions,
                AudienceId = BroadcastConstants.HouseholdAudienceId,
                Budget = plan.Budget
            });

            plan.HHUniverse = householdPlanDeliveryBudget.Universe.Value;
            plan.HHImpressions = householdPlanDeliveryBudget.Impressions.Value;
            plan.HHCPM = householdPlanDeliveryBudget.CPM.Value;
            plan.HHRatingPoints = householdPlanDeliveryBudget.RatingPoints.Value;
            plan.HHCPP = householdPlanDeliveryBudget.CPP.Value;
            plan.Vpvh = ProposalMath.CalculateVpvh(plan.TargetImpressions.Value, hhImpressions);
        }

        private void _CalculateSecondaryAudiencesDeliveryData(
            PlanDto plan,
            Dictionary<int, double> hhImpressionsByStandardDaypart,
            double hhImpressions)
        {
            Parallel.ForEach(plan.SecondaryAudiences, (planAudience) =>
            {
                var totalImpressionsForAudience = Math.Floor(_GetAudienceImpressionsByStandardDaypart(plan, hhImpressionsByStandardDaypart, planAudience.AudienceId).Sum(x => x.Value));

                var planDeliveryBudget = _BudgetCalculator.CalculateBudget(new PlanDeliveryBudget
                {
                    Impressions = totalImpressionsForAudience,
                    AudienceId = planAudience.AudienceId,
                    Budget = plan.Budget
                });

                planAudience.Impressions = planDeliveryBudget.Impressions;
                planAudience.RatingPoints = planDeliveryBudget.RatingPoints;
                planAudience.CPM = planDeliveryBudget.CPM;
                planAudience.CPP = planDeliveryBudget.CPP;
                planAudience.Universe = planDeliveryBudget.Universe.Value;
                planAudience.Vpvh = ProposalMath.CalculateVpvh(totalImpressionsForAudience, hhImpressions);
            });
        }

        public PlanLockResponse LockPlan(int planId)
        {
            var key = KeyHelper.GetPlanLockingKey(planId);
            var lockingResponse = _LockingManagerApplicationService.LockObject(key);
            var planName = _PlanRepository.GetPlanNameById(planId);

            return new PlanLockResponse
            {
                Key = lockingResponse.Key,
                Success = lockingResponse.Success,
                LockTimeoutInSeconds = lockingResponse.LockTimeoutInSeconds,
                LockedUserId = lockingResponse.LockedUserId,
                LockedUserName = lockingResponse.LockedUserName,
                Error = lockingResponse.Error,
                PlanName = planName
            };
        }

        /// <inheritdoc/>
        public List<CreativeLength> CalculateCreativeLengthWeight(List<CreativeLength> request)
        {
            _CreativeLengthEngine.ValidateCreativeLengths(request);

            //if all the creative lengths have a user entered value
            //there is nothing else to do
            if (request.All(x => x.Weight.HasValue))
            {
                return null;
            }

            //if there is only 1 creative length on the plan we default it to 100
            //we need to redistribute that weight, if the user adds more creative lengths
            if (request.Any(x => x.Weight == 100))
            {
                request.Single(x => x.Weight == 100).Weight = null;
            }
            var result = _CreativeLengthEngine.DistributeWeight(request);
            var creativeLengthsWithWeightSet = request.Where(x => x.Weight.HasValue).Select(x => x.SpotLengthId).ToList();

            //we only return the values for placeholders
            result.RemoveAll(x => creativeLengthsWithWeightSet.Contains(x.SpotLengthId));
            return result;
        }

        /// <inheritdoc/>
        public List<LengthMakeUpTableRow> CalculateLengthMakeUpTable(LengthMakeUpRequest request)
        {
            List<LengthMakeUpTableRow> result = new List<LengthMakeUpTableRow>();
            request.CreativeLengths = _CreativeLengthEngine.DistributeWeight(request.CreativeLengths);
            foreach (var creativeLength in request.CreativeLengths)
            {
                var impressions = request.Weeks.Where(x => x.SpotLengthId == creativeLength.SpotLengthId).Sum(x => x.WeeklyImpressions);
                result.Add(new LengthMakeUpTableRow
                {
                    SpotLengthId = creativeLength.SpotLengthId,
                    GoalPercentage = creativeLength.Weight.Value,
                    Budget = Math.Round(request.Weeks.Where(x => x.SpotLengthId == creativeLength.SpotLengthId).Sum(x => x.WeeklyBudget)),
                    Impressions = impressions,
                    ImpressionsPercentage = Math.Round(GeneralMath.ConvertFractionToPercentage(impressions / request.TotalImpressions))
                });
            }
            var spotLengths = _SpotLengthEngine.GetSpotLengths();
            return result.OrderBy(x => spotLengths.Where(y => y.Value == x.SpotLengthId).Single().Key).ToList();
        }
    }
}
