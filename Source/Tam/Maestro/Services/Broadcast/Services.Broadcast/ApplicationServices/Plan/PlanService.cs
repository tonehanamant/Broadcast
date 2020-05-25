using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
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
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

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
        List<VPVHForAudience> GetVPVHForAudiencesWithBooks(VPVHRequest request);

        /// <summary>
        /// Calculates the creative length weight.
        /// </summary>
        /// <param name="request">Creative lengths set on the plan.</param>
        /// <returns>List of creative lengths with calculated values</returns>
        List<CreativeLength> CalculateCreativeLengthWeight(List<CreativeLength> request);

        List<WeeklyBreakdownWeek> RemapWeeklyBreakdownData(int? planId = null);

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
        private readonly INsiUniverseService _NsiUniverseService;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
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
            , INsiUniverseService nsiUniverseService
            , IBroadcastAudiencesCache broadcastAudiencesCache
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
            _NsiUniverseService = nsiUniverseService;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
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
            _CalculateHouseholdDeliveryData(plan);
            _CalculateSecondaryAudiencesDeliveryData(plan);
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
                //_PlanPricingService.QueuePricingJob(plan.PricingParameters, createdDate);
            }

            return plan.Id;
        }

        /// <summary>
        /// Migrates existing weekly breakdown tables to the new 'by week by ad length by daypart' structure
        /// </summary>
        /// <param name="planVersion">Optional parameter for testing</param>
        /// <returns></returns>
        public List<WeeklyBreakdownWeek> RemapWeeklyBreakdownData(int? planVersion = null)
        {
            List<WeeklyBreakdownWeek> result = new List<WeeklyBreakdownWeek>();
            var plans = _PlanRepository.LoadPlansForWeeklyBreakdownRemapping();

            if (planVersion.HasValue)
            {
                plans = plans.Where(x => x.VersionId == planVersion.Value).ToList();
            }

            foreach (var plan in plans)
            {
                plan.CreativeLengths = _CreativeLengthEngine.DistributeWeight(plan.CreativeLengths);
                _DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);
                _PlanRepository.SaveWeeklyBreakdownDistribution(plan);
                _PlanRepository.SaveCreativeLengths(plan);
                result.AddRange(plan.WeeklyBreakdownWeeks);
            }
            return result;
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
            else
            {
                throw new ApplicationException(_UnsupportedDeliveryTypeMessage);
            }
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
                    var aduImpressionsForBreakdownItem = breakdownItem.WeeklyAdu * BroadcastConstants.ImpressionsPerUnit;

                    foreach (var (StadardDaypartId, WeightingGoalPercent) in standardDaypardWeightingGoals)
                    {
                        var weighting = WeightingGoalPercent / 100;

                        var newWeeklyBreakdownItem = new WeeklyBreakdownWeek
                        {
                            WeekNumber = week.WeekNumber,
                            MediaWeekId = week.MediaWeekId,
                            StartDate = week.StartDate,
                            EndDate = week.EndDate,
                            NumberOfActiveDays = week.NumberOfActiveDays,
                            ActiveDays = week.ActiveDays,
                            SpotLengthId = breakdownItem.SpotLengthId,
                            DaypartCodeId = StadardDaypartId,
                            AduImpressions = aduImpressionsForBreakdownItem * weighting
                        };

                        var impressions = breakdownItem.WeeklyImpressions * weighting;

                        _UpdateGoalsForWeeklyBreakdownItem(
                            plan.TargetImpressions.Value,
                            plan.TargetRatingPoints.Value,
                            plan.Budget.Value,
                            newWeeklyBreakdownItem,
                            impressions);

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
                var weeklyAduImpressions = week.WeeklyAdu * BroadcastConstants.ImpressionsPerUnit;

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
                        AduImpressions = weeklyAduImpressions * combination.Weighting
                    };

                    var impressions = week.WeeklyImpressions * combination.Weighting;

                    _UpdateGoalsForWeeklyBreakdownItem(
                        plan.TargetImpressions.Value,
                        plan.TargetRatingPoints.Value,
                        plan.Budget.Value,
                        newWeeklyBreakdownItem,
                        impressions);

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

            _PlanPricingService.ApplyMargin(plan.PricingParameters);
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
                plan.TargetImpressions /= plan.TargetImpressions.Value;
            }
            plan.HHImpressions /= 1000;
            foreach (var audience in plan.SecondaryAudiences)
            {
                if (audience.Impressions.HasValue)
                {
                    audience.Impressions /= audience.Impressions.Value;
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
            var plan = _PlanRepository.GetPlan(planId, versionId);

            plan.IsPricingModelRunning = _PlanPricingService.IsPricingModelRunningForPlan(planId);

            _GroupWeeklyBreakdownWeeksBasedOnDeliveryType(plan);
            _SetWeekNumber(plan.WeeklyBreakdownWeeks);
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
            else
            {
                throw new ApplicationException(_UnsupportedDeliveryTypeMessage);
            }
        }

        /// <summary>
        /// Groups weekly breakdown by week by ad length
        /// </summary>
        private List<WeeklyBreakdownWeek> _GroupWeeklyBreakdownWeeks_ByWeekByAdLengthDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();
            var weeks = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks);
            var weeklyBreakdownByWeekBySpotLength = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeekBySpotLength(plan.WeeklyBreakdownWeeks);

            foreach (var item in weeklyBreakdownByWeekBySpotLength)
            {
                var impressionsForWeek = weeks.Single(x => x.MediaWeekId == item.MediaWeekId).Impressions;
                var newWeeklyBreakdownItem = new WeeklyBreakdownWeek
                {
                    WeekNumber = item.WeekNumber,
                    MediaWeekId = item.MediaWeekId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    NumberOfActiveDays = item.NumberOfActiveDays,
                    ActiveDays = item.ActiveDays,
                    SpotLengthId = item.SpotLengthId,
                    WeeklyAdu = item.Adu
                };

                _UpdateGoalsForWeeklyBreakdownItem(
                       plan.TargetImpressions.Value,
                       plan.TargetRatingPoints.Value,
                       plan.Budget.Value,
                       newWeeklyBreakdownItem,
                       item.Impressions);

                result.Add(newWeeklyBreakdownItem);
            }

            _RecalculatePercentageOfWeekBasedOnImpressions(result);

            return result;
        }

        /// <summary>
        /// Groups weekly breakdown by week
        /// </summary>
        private List<WeeklyBreakdownWeek> _GroupWeeklyBreakdownWeeks_ByWeekDeliveryType(PlanDto plan)
        {
            var result = new List<WeeklyBreakdownWeek>();
            var weeks = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks);

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
                    WeeklyAdu = week.Adu
                };

                _UpdateGoalsForWeeklyBreakdownItem(
                       plan.TargetImpressions.Value,
                       plan.TargetRatingPoints.Value,
                       plan.Budget.Value,
                       newWeeklyBreakdownItem,
                       week.Impressions);

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
                ? plan.WeeklyBreakdownTotals.TotalImpressions * 1000 //because weekly breakdown is already in thousands
                / plan.TargetImpressions.Value : 0;

            plan.WeeklyBreakdownTotals.TotalRatingPoints = Math.Round(plan.TargetRatingPoints ?? 0 * impressionsTotalsRatio, 1);
            plan.WeeklyBreakdownTotals.TotalImpressionsPercentage = Math.Round(100 * impressionsTotalsRatio, 0);
            plan.WeeklyBreakdownTotals.TotalBudget = plan.Budget.Value * (decimal)impressionsTotalsRatio;
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
                FlightDays = plan.FlightDays.ToList()
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
            return EnumExtensions.ToLookupDtoList<PlanGoalBreakdownTypeEnum>(); ;
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
            _PlanValidator.ValidateWeeklyBreakdown(request);

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

            _CalculateWeeklyGoalBreakdownTotals(response, request);
            _OrderWeeks(request, response);
            _SetWeekNumber(response.Weeks);

            return response;
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
            _RecalculatePercentageOfWeekBasedOnImpressions(result.Weeks);

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

            _RecalculatePercentageOfWeekBasedOnImpressions(result.Weeks);

            return result;
        }

        private void _RecalculatePercentageOfWeekBasedOnImpressions(List<WeeklyBreakdownWeek> weeks)
        {
            var weeklyBreakdownByWeek = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(weeks);

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

        private double _CalculatePercentageOfWeek(double breakdownItemImpressions, double weeklyImpressions)
        {
            return weeklyImpressions > 0 ? Math.Round(100 * breakdownItemImpressions / weeklyImpressions) : 0;
        }

        private void _AddNewCreativeLengthsToResult(
            WeeklyBreakdownResponseDto result,
            List<CreativeLength> creativeLengths)
        {
            var existingWeeks = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(result.Weeks);

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

        private void _CalculateWeeklyGoalBreakdownTotals(WeeklyBreakdownResponseDto weeklyBreakdown, WeeklyBreakdownRequest request)
        {
            weeklyBreakdown.TotalActiveDays = _WeeklyBreakdownEngine
                .GroupWeeklyBreakdownByWeek(weeklyBreakdown.Weeks)
                .Sum(x => x.NumberOfActiveDays);

            weeklyBreakdown.TotalImpressions = weeklyBreakdown.Weeks.Sum(w => w.WeeklyImpressions);
            var impressionsTotalRatio = weeklyBreakdown.TotalImpressions / request.TotalImpressions;

            weeklyBreakdown.TotalShareOfVoice = Math.Round(100 * impressionsTotalRatio, 0);
            weeklyBreakdown.TotalImpressionsPercentage = weeklyBreakdown.TotalShareOfVoice;

            weeklyBreakdown.TotalRatingPoints = Math.Round(request.TotalRatings * impressionsTotalRatio, 1);
            weeklyBreakdown.TotalBudget = request.TotalBudget * (decimal)impressionsTotalRatio;
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

        private void _SetWeekNumber(IEnumerable<WeeklyBreakdownWeek> weeks)
        {
            var weekNumberByMediaWeek = _WeeklyBreakdownEngine.GetWeekNumberByMediaWeekDictionary(weeks);

            foreach (var week in weeks)
            {
                week.WeekNumber = weekNumberByMediaWeek[week.MediaWeekId];
            }
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
            var householdAudienceId = _BroadcastAudiencesCache.GetDefaultAudience();
            var defaultSpotLengthId = _SpotLengthEngine.GetSpotLengthIdByValue(defaultSpotLength);

            return new PlanDefaultsDto
            {
                Name = string.Empty,
                AudienceId = householdAudienceId.Id,
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

        private void _CalculateHouseholdDeliveryData(PlanDto plan)
        {
            var householdPlanDeliveryBudget = _BudgetCalculator.CalculateBudget(new PlanDeliveryBudget
            {
                Impressions = Math.Floor(plan.TargetImpressions.Value / plan.Vpvh),
                AudienceId = _BroadcastAudiencesCache.GetDefaultAudience().Id,
                Budget = plan.Budget
            });

            plan.HHUniverse = householdPlanDeliveryBudget.Universe.Value;
            plan.HHImpressions = householdPlanDeliveryBudget.Impressions.Value;
            plan.HHCPM = householdPlanDeliveryBudget.CPM.Value;
            plan.HHRatingPoints = householdPlanDeliveryBudget.RatingPoints.Value;
            plan.HHCPP = householdPlanDeliveryBudget.CPP.Value;
        }

        private void _CalculateSecondaryAudiencesDeliveryData(PlanDto plan)
        {
            Parallel.ForEach(plan.SecondaryAudiences, (planAudience) =>
            {
                var planDeliveryBudget = _BudgetCalculator.CalculateBudget(new PlanDeliveryBudget
                {
                    Impressions = Math.Floor(plan.HHImpressions * planAudience.Vpvh),
                    AudienceId = planAudience.AudienceId,
                    Budget = plan.Budget
                });

                planAudience.Impressions = planDeliveryBudget.Impressions;
                planAudience.RatingPoints = planDeliveryBudget.RatingPoints;
                planAudience.CPM = planDeliveryBudget.CPM;
                planAudience.CPP = planDeliveryBudget.CPP;
                planAudience.Universe = planDeliveryBudget.Universe.Value;
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

        public List<VPVHForAudience> GetVPVHForAudiencesWithBooks(VPVHRequest request)
        {
            var result = new List<VPVHForAudience>();

            var mediaMonthId = request.HutBookId.HasValue ? request.HutBookId.Value : request.ShareBookId;

            var householdUniverse = _NsiUniverseService.GetAudienceUniverseForMediaMonth(mediaMonthId, BroadcastConstants.HouseholdAudienceId);

            foreach (var audienceId in request.AudienceIds)
            {
                var audienceUniverse = _NsiUniverseService.GetAudienceUniverseForMediaMonth(mediaMonthId, audienceId);
                var audienceVPVH = audienceUniverse / householdUniverse;
                result.Add(new VPVHForAudience
                {
                    AudienceId = audienceId,
                    VPVH = audienceVPVH
                });
            }

            return result;
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

            return result;
        }
    }
}
