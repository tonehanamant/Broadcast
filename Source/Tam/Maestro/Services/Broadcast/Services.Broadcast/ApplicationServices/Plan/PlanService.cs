﻿using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
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
        /// Gets the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="versionId">Optional: version id. If nothing is passed, it will return the latest version</param>
        /// <returns></returns>
        PlanDto_v2 GetPlan_v2(int planId, int? versionId = null);

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
        /// <returns>PlanDeliveryBudget object</returns>
        PlanDeliveryBudget CalculateBudget(PlanDeliveryBudget planBudget);

        /// <summary>
        /// Calculates the posting type budgets.
        /// </summary>
        /// <param name="budgetRequest">The budget request.</param>
        /// <returns></returns>
        List<PlanDeliveryPostingTypeBudget> CalculatePostingTypeBudgets(PlanDeliveryPostingTypeBudget budgetRequest);

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

        /// <summary>
        /// Calculates the default plan available markets.
        /// </summary>
        /// <returns></returns>
        PlanAvailableMarketCalculationResult CalculateDefaultPlanAvailableMarkets();

        /// <summary>
        /// Calculates and distributes the market weights.
        /// </summary>
        /// <param name="availableMarkets">The available markets.</param>
        /// <param name="modifiedMarketCode">The modified market code.</param>
        /// <param name="userEnteredValue">The user entered value.</param>
        /// <returns></returns>
        PlanAvailableMarketCalculationResult CalculateMarketWeightChange(List<PlanAvailableMarketDto> availableMarkets,
            short modifiedMarketCode, double? userEnteredValue);

        /// <summary>
        /// Adds the markets and calculates and distributes the market weights.
        /// </summary>
        /// <param name="beforeMarkets">The before markets.</param>
        /// <param name="addedMarkets">The added markets.</param>
        /// <returns></returns>
        PlanAvailableMarketCalculationResult CalculateMarketsAdded(List<PlanAvailableMarketDto> beforeMarkets,
            List<PlanAvailableMarketDto> addedMarkets);

        /// <summary>
        /// Removes the markets and calculates and distributes the market weights.
        /// </summary>
        /// <param name="beforeMarkets">The before markets.</param>
        /// <param name="removedMarketCodes">The removed market codes.</param>
        /// <returns></returns>
        PlanAvailableMarketCalculationResult CalculateMarketsRemoved(List<PlanAvailableMarketDto> beforeMarkets,
            List<short> removedMarketCodes);

        /// <summary>
        /// Clears the user entered values and recalculates the weights.
        /// </summary>
        /// <param name="availableMarkets">The available markets.</param>
        PlanAvailableMarketCalculationResult CalculateMarketWeightsClearAll(List<PlanAvailableMarketDto> availableMarkets);

        /// <summary>
        /// Updates the plan spot allocation mode.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="postingType">Type of the posting.</param>
        /// <param name="username">The username.</param>
        /// /// <param name="aggregatePlanSynchronously">if set to <c>true</c> [aggregate plan synchronously].</param>
        /// <returns>True if the operation was successful, otherwise false.</returns>
        bool CommitPricingAllocationModel(int planId, SpotAllocationModelMode spotAllocationModelMode, PostingTypeEnum postingType, 
            string username, bool aggregatePlanSynchronously = false);
    }

    public class PlanService : BroadcastBaseClass, IPlanService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IInventoryProprietarySummaryRepository _InventoryProprietarySummaryRepository;
        private readonly IPlanValidator _PlanValidator;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;
        private readonly IPlanBudgetDeliveryCalculator _BudgetCalculator;
        private readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly IPlanAggregator _PlanAggregator;
        private readonly IPlanSummaryRepository _PlanSummaryRepository;
        private readonly IStandardDaypartRepository _StandardDaypartRepository;
        private readonly ICampaignRepository _CampaignRepository;
        private readonly ICampaignAggregationJobTrigger _CampaignAggregationJobTrigger;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IBroadcastLockingManagerApplicationService _LockingManagerApplicationService;
        private readonly IPlanPricingService _PlanPricingService;
        private readonly IPlanBuyingService _PlanBuyingService;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IStandardDaypartService _StandardDaypartService;
        private readonly IDayRepository _DayRepository;
        private readonly IWeeklyBreakdownEngine _WeeklyBreakdownEngine;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly IPlanMarketSovCalculator _PlanMarketSovCalculator;
        private readonly IMarketCoverageRepository _MarketCoverageRepository;
        private readonly INtiToNsiConversionRepository _NtiToNsiConversionRepository;

        private const string _StandardDaypartNotFoundMessage = "Unable to find standard daypart";

        private Lazy<bool> _IsMarketSovCalculationEnabled;

        public PlanService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IPlanValidator planValidator
            , IPlanBudgetDeliveryCalculator planBudgetDeliveryCalculator
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IPlanAggregator planAggregator
            , ICampaignAggregationJobTrigger campaignAggregationJobTrigger
            , ISpotLengthEngine spotLengthEngine
            , IBroadcastLockingManagerApplicationService lockingManagerApplicationService
            , IPlanPricingService planPricingService
            , IPlanBuyingService planBuyingService
            , IQuarterCalculationEngine quarterCalculationEngine
            , IStandardDaypartService standardDaypartService
            , IWeeklyBreakdownEngine weeklyBreakdownEngine
            , ICreativeLengthEngine creativeLengthEngine
            , IFeatureToggleHelper featureToggleHelper
            , IPlanMarketSovCalculator planMarketSovCalculator)
        {
            _MediaWeekCache = mediaMonthAndWeekAggregateCache;
            _PlanValidator = planValidator;
            _BudgetCalculator = planBudgetDeliveryCalculator;

            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _InventoryProprietarySummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProprietarySummaryRepository>();
            _CampaignRepository = broadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _PlanSummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanSummaryRepository>();
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            _DayRepository = broadcastDataRepositoryFactory.GetDataRepository<IDayRepository>();
            _MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _PlanAggregator = planAggregator;
            _CampaignAggregationJobTrigger = campaignAggregationJobTrigger;
            _SpotLengthEngine = spotLengthEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _PlanPricingService = planPricingService;
            _PlanBuyingService = planBuyingService;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _StandardDaypartService = standardDaypartService;
            _WeeklyBreakdownEngine = weeklyBreakdownEngine;
            _CreativeLengthEngine = creativeLengthEngine;
            _FeatureToggleHelper = featureToggleHelper;
            _PlanMarketSovCalculator = planMarketSovCalculator;
            _NtiToNsiConversionRepository = broadcastDataRepositoryFactory.GetDataRepository<INtiToNsiConversionRepository>();

            _IsMarketSovCalculationEnabled = new Lazy<bool>(() => 
                _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PLAN_MARKET_SOV_CALCULATIONS));
        }        

        internal void _OnSaveHandlePlanAvailableMarketSovFeature(PlanDto plan)
        {
            // When the flag is disabled we want to fill in the missing values
            if (!_IsMarketSovCalculationEnabled.Value)
            {
                plan.AvailableMarkets.Where(s => s.ShareOfVoicePercent.HasValue).ToList()
                    .ForEach(s =>s.IsUserShareOfVoicePercent = true);

                var result = _PlanMarketSovCalculator.CalculateMarketWeights(plan.AvailableMarkets);
                plan.AvailableMarkets = result.AvailableMarkets;
                plan.AvailableMarketsSovTotal = result.TotalWeight;
            }
        }

        internal void _OnGetHandlePlanAvailableMarketSovFeature(PlanDto plan)
        {
            // when the flag is disabled then we want to hide the feature values
            if (!_IsMarketSovCalculationEnabled.Value)
            {
                plan.AvailableMarkets.ForEach(s =>
                {
                    if (!s.IsUserShareOfVoicePercent)
                    {
                        s.ShareOfVoicePercent = null;
                    }
                });
            }
        }

        ///<inheritdoc/>
        public int SavePlan(PlanDto plan, string createdBy, DateTime createdDate, bool aggregatePlanSynchronously = false)
        {
            const string SW_KEY_TOTAL_DURATION = "Total duration";
            const string SW_KEY_PRE_PLAN_VALIDATION = "Pre Plan Validation";
            const string SW_KEY_PLAN_VALIDATION = "Plan Validation";
            const string SW_KEY_PRE_PLAN_SAVE = "Pre Plan Save";
            const string SW_KEY_PLAN_SAVE = "Plan Save";
            const string SW_KEY_POST_PLAN_SAVE = "Post Plan Save";

            _LogInfo($"SavePlan starting for planID '{plan.Id}'", createdBy);

            var processTimers = new ProcessWorkflowTimers();
            processTimers.Start(SW_KEY_TOTAL_DURATION);
            processTimers.Start(SW_KEY_PRE_PLAN_VALIDATION);

            if (plan.Id > 0 && _PlanPricingService.IsPricingModelRunningForPlan(plan.Id))
            {
                throw new Exception("The pricing model is running for the plan");
            }

            PlanDto beforePlan;
            var saveState = _DeriveSaveState(plan, out beforePlan);

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
            _OnSaveHandlePlanAvailableMarketSovFeature(plan);

            processTimers.End(SW_KEY_PRE_PLAN_VALIDATION);
            processTimers.Start(SW_KEY_PLAN_VALIDATION);

            _PlanValidator.ValidatePlan(plan);

            processTimers.End(SW_KEY_PLAN_VALIDATION);
            processTimers.Start(SW_KEY_PRE_PLAN_SAVE);

            _ConvertImpressionsToRawFormat(plan);
            plan.WeeklyBreakdownWeeks = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);
            _CalculateDeliveryDataPerAudience(plan);
            _SetPlanVersionNumber(plan);
            _SetPlanFlightDays(plan);

            if (plan.Status == PlanStatusEnum.Contracted && plan.GoalBreakdownType == PlanGoalBreakdownTypeEnum.EvenDelivery)
            {
                plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeek;
            }

            _VerifyWeeklyAdu(plan.IsAduEnabled, plan.WeeklyBreakdownWeeks);

            processTimers.End(SW_KEY_PRE_PLAN_SAVE);
            processTimers.Start(SW_KEY_PLAN_SAVE);

            if (saveState == SaveState.CreatingNewPlan)
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

            processTimers.End(SW_KEY_PLAN_SAVE);
            processTimers.Start(SW_KEY_POST_PLAN_SAVE);

            _UpdateCampaignLastModified(plan.CampaignId, createdDate, createdBy);

            // We only aggregate data for versions, not drafts.
            if (!plan.IsDraft)
            {
                _DispatchPlanAggregation(plan, aggregatePlanSynchronously);
                _CampaignAggregationJobTrigger.TriggerJob(plan.CampaignId, createdBy);
            }

            /*** Handle Pricing and Buying ***/
            // This plan.id and plan.versionId were updated in their respective saves.
            // if a new version was published then the VersionId is the latest published version.
            // if a draft was saved then the VersionId is the draft version instead.
            var afterPlan = _PlanRepository.GetPlan(plan.Id, plan.VersionId);
            if (!plan.IsDraft)
            {
                _HandlePricingOnPlanSave(saveState, plan, beforePlan, afterPlan, createdDate, createdBy);
                _HandleBuyingOnPlanSave(saveState, plan, beforePlan, afterPlan);
            }
            processTimers.End(SW_KEY_POST_PLAN_SAVE);
            processTimers.End(SW_KEY_TOTAL_DURATION);
            var timersReport = processTimers.ToString();
            _LogInfo($"Plan Save Process Timers Report : '{timersReport}'");

            return plan.Id;
        }

        internal void _HandleBuyingOnPlanSave(SaveState saveState, PlanDto plan, PlanDto beforePlan, PlanDto afterPlan)
        {
            if (plan.BuyingParameters != null)
            {
                if (saveState == SaveState.CreatingNewPlan)
                {
                    _LogInfo($"Relating previous buying results to the new plan version. Plan.Id = {plan.Id} BeforeVersion = 'null'; AfterVersion = {afterPlan.VersionId}");
                    _PlanRepository.SetBuyingPlanVersionId(plan.BuyingParameters.JobId.Value, plan.VersionId);
                }
                else
                {
                    _LogInfo($"Relating previous buying results to the new plan version. Plan.Id = {plan.Id} BeforeVersion = {beforePlan?.VersionId ?? 0}; AfterVersion = {afterPlan.VersionId}");
                    _PlanRepository.UpdatePlanBuyingVersionId(afterPlan.VersionId, beforePlan.VersionId);
                }
            }
        }

        internal void _HandlePricingOnPlanSave(SaveState saveState, PlanDto plan, PlanDto beforePlan, PlanDto afterPlan, DateTime createdDate, string createdBy)
        {
            // the plan passed up by the UI may not relate to the last pricing run, so ignore them.
            // This sets the default parameters in case we don't promote existing results.
            _SetPlanPricingParameters(plan);            
            var shouldPromotePricingResults = _ShouldPromotePricingResultsOnPlanSave(saveState, beforePlan, afterPlan);

            _FinalizePricingOnPlanSave(saveState, plan, beforePlan, afterPlan, createdDate, createdBy, shouldPromotePricingResults);
        }

        internal void _FinalizePricingOnPlanSave(SaveState saveState, PlanDto plan, PlanDto beforePlan, PlanDto afterPlan, DateTime createdDate, string createdBy,
            bool shouldPromotePricingResults)
        {
            if (shouldPromotePricingResults)
            {                               
                 _LogInfo($"Relating previous pricing results to the new plan version. Plan.Id = {plan.Id} BeforeVersion = {beforePlan?.VersionId ?? 0}; AfterVersion = {afterPlan.VersionId}");
                 _PlanRepository.UpdatePlanPricingVersionId(afterPlan.VersionId, beforePlan.VersionId);      
            }
            else
            {
                _PlanRepository.SavePlanPricingParameters(plan.PricingParameters);
            }
        }

        internal bool _ShouldPromotePricingResultsOnPlanSave(SaveState saveState, PlanDto beforePlan, PlanDto afterPlan)
        {
            var shouldPromotePricingResults = false;
                if(saveState != SaveState.CreatingNewPlan)               
                {
                    var goalsHaveChanged = PlanComparisonHelper.DidPlanPricingInputsChange(beforePlan, afterPlan);
                    shouldPromotePricingResults = !goalsHaveChanged;
                }             
            return shouldPromotePricingResults;
        }

        public enum SaveState
        {
            CreatingNewPlan,
            CreatingNewDraft,
            PromotingDraft,
            UpdatingExisting
        }

        internal SaveState _DeriveSaveState(PlanDto plan, out PlanDto beforePlan)
        {
            beforePlan = null;
            if (plan.VersionId == 0 || plan.Id == 0)
            {
                return SaveState.CreatingNewPlan;
            }
            beforePlan = _PlanRepository.GetPlan(plan.Id, plan.VersionId);

            if (!beforePlan.IsDraft && plan.IsDraft)
            {
                return SaveState.CreatingNewDraft;
            }

            if (beforePlan.IsDraft && !plan.IsDraft)
            {
                return SaveState.PromotingDraft;
            }

            return SaveState.UpdatingExisting;
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
                UnitCapsType = pricingDefaults.UnitCapsType,
                Margin = pricingDefaults.Margin,
                PlanVersionId = plan.VersionId,
                MarketGroup = pricingDefaults.MarketGroup,
                PostingType = plan.PostingType
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
            var standardDayparts = _StandardDaypartRepository.GetAllStandardDaypartsWithAllData();

            foreach (var planDaypart in planDayparts)
            {
                var standardDaypart = standardDayparts.Single(x => x.Id == planDaypart.DaypartCodeId, _StandardDaypartNotFoundMessage);

                planDaypart.IsStartTimeModified = planDaypart.StartTimeSeconds != standardDaypart.DefaultStartTimeSeconds;
                planDaypart.IsEndTimeModified = planDaypart.EndTimeSeconds != standardDaypart.DefaultEndTimeSeconds;
            }
        }

        ///<inheritdoc/>
        public PlanDto GetPlan(int planId, int? versionId = null)
        {
            PlanDto plan = _PlanRepository.GetPlan(planId, versionId);

            plan.IsPricingModelRunning = _PlanPricingService.IsPricingModelRunningForPlan(planId);
            plan.IsBuyingModelRunning = _PlanBuyingService.IsBuyingModelRunning(planId);

            if (plan.PricingParameters != null && plan.PricingParameters.ProprietaryInventory.Any())
            {
                _PopulateProprietaryInventoryData(plan.PricingParameters.ProprietaryInventory);
            }

            // Because in DB we store weekly breakdown split 'by week by ad length by daypart'
            // we need to group them back based on the plan delivery type
            plan.WeeklyBreakdownWeeks = _WeeklyBreakdownEngine.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(plan);

            _WeeklyBreakdownEngine.SetWeekNumber(plan.WeeklyBreakdownWeeks);
            _SetWeeklyBreakdownTotals(plan);
            DaypartTimeHelper.AddOneSecondToEndTime(plan.Dayparts);

            _SetDefaultDaypartRestrictions(plan);
            _ConvertImpressionsToUserFormat(plan);

            _SortPlanDayparts(plan);
            _SortProgramRestrictions(plan);
            _SortCreativeLengths(plan);

            _OnGetHandlePlanAvailableMarketSovFeature(plan);
            _HandleAvailableMarketSovs(plan);

            return plan;
        }

        private void _HandleAvailableMarketSovs(PlanDto plan)
        {
            var calculationResults = _PlanMarketSovCalculator.CalculateMarketWeights(plan.AvailableMarkets);
            plan.AvailableMarkets = calculationResults.AvailableMarkets;
            plan.AvailableMarketsSovTotal = calculationResults.TotalWeight;
        }

        public PlanDto_v2 GetPlan_v2(int planId, int? versionId = null)
        {
            var plan = GetPlan(planId, versionId);
            var conversionRate = _PlanRepository.GetNsiToNtiConversionRate(plan.Dayparts);
            var plan_v2 = _MapPlanDtoToPlanDto_v2(plan, conversionRate);
            return plan_v2;
        }

        private PlanDto_v2 _MapPlanDtoToPlanDto_v2(PlanDto plan, double ntiToNsiConversionRate)
        {
            var dto = new PlanDto_v2
            {
                Id = plan.Id,
                CampaignId = plan.CampaignId,
                CampaignName = plan.CampaignName,
                Name = plan.Name,
                CreativeLengths = plan.CreativeLengths,
                Equivalized = plan.Equivalized,
                Status = plan.Status,
                ProductId = plan.ProductId,
                ProductMasterId = plan.ProductMasterId,
                FlightDays = plan.FlightDays,
                FlightStartDate = plan.FlightStartDate,
                FlightEndDate = plan.FlightEndDate,
                FlightNotes = plan.FlightNotes,
                AudienceId = plan.AudienceId,
                AudienceType = plan.AudienceType,
                HUTBookId = plan.HUTBookId,
                ShareBookId = plan.ShareBookId,
                PostingType = plan.PostingType,
                FlightHiatusDays = plan.FlightHiatusDays,
                Budget = plan.Budget,
                TargetImpressions = plan.TargetImpressions,
                TargetCPM = plan.TargetCPM,
                TargetRatingPoints = plan.TargetRatingPoints,
                TargetCPP = plan.TargetCPP,
                Currency = plan.Currency,
                GoalBreakdownType = plan.GoalBreakdownType,
                SecondaryAudiences = plan.SecondaryAudiences,
                Dayparts = plan.Dayparts,
                CoverageGoalPercent = plan.CoverageGoalPercent,
                AvailableMarkets = plan.AvailableMarkets,
                BlackoutMarkets = plan.BlackoutMarkets,
                AvailableMarketsSovTotal = plan.AvailableMarketsSovTotal,
                WeeklyBreakdownWeeks = plan.WeeklyBreakdownWeeks,
                WeeklyBreakdownTotals = plan.WeeklyBreakdownTotals,
                ModifiedBy = plan.ModifiedBy,
                ModifiedDate = plan.ModifiedDate,
                Vpvh = plan.Vpvh,
                TargetUniverse = plan.TargetUniverse,
                HHCPM = plan.HHCPM,
                HHCPP = plan.HHCPP,
                HHImpressions = plan.HHImpressions,
                HHRatingPoints = plan.HHRatingPoints,
                HHUniverse = plan.HHUniverse,
                AvailableMarketsWithSovCount = plan.AvailableMarketsWithSovCount,
                BlackoutMarketCount = plan.BlackoutMarketCount,
                BlackoutMarketTotalUsCoveragePercent = plan.BlackoutMarketTotalUsCoveragePercent,
                IsDraft = plan.IsDraft,
                VersionNumber = plan.VersionNumber,
                VersionId = plan.VersionId,
                IsAduEnabled = plan.IsAduEnabled,
                ImpressionsPerUnit = plan.ImpressionsPerUnit,
                BuyingParameters = plan.BuyingParameters,
                JobId = plan.JobId,
                SpotAllocationModelMode = plan.SpotAllocationModelMode
            };
            
            dto.PricingParameters = new List<PlanPricingParametersDto>();

            if (plan.PricingParameters != null)
            {
                var convertedParameters = plan.PricingParameters.DeepCloneUsingSerialization();

                //Add original parameters
                plan.PricingParameters.IsSelected = true;
                dto.PricingParameters.Add(plan.PricingParameters);

                //Convert parameters
                if (convertedParameters.PostingType == PostingTypeEnum.NSI)
                {
                    convertedParameters.DeliveryImpressions = Math.Floor(convertedParameters.DeliveryImpressions * ntiToNsiConversionRate);
                    convertedParameters.PostingType = PostingTypeEnum.NTI;
                }
                else if (convertedParameters.PostingType == PostingTypeEnum.NTI)
                {
                    convertedParameters.DeliveryImpressions = Math.Floor(convertedParameters.DeliveryImpressions / ntiToNsiConversionRate);
                    convertedParameters.PostingType = PostingTypeEnum.NSI;
                }

                if (convertedParameters.DeliveryImpressions != 0)
                {
                    convertedParameters.CPM = (decimal)((double)convertedParameters.Budget / convertedParameters.DeliveryImpressions);
                }
                else
                {
                    convertedParameters.CPM = 0;
                    _LogWarning($"Delivery impressions are 0 for the plan with id {plan.Id} version {plan.VersionNumber} for {convertedParameters.PostingType} posting type. " +
                                $"The cpm for the parameters could not be calculated properly and is set to 0.");
                }

                dto.PricingParameters.Add(convertedParameters);
            }

            return dto;
        }

        private void _PopulateProprietaryInventoryData(List<InventoryProprietarySummary> proprietaryInventory)
        {
            Dictionary<int, InventoryProprietarySummary> data = _InventoryProprietarySummaryRepository
                .GetInventorySummaryDataById(proprietaryInventory.Select(y => y.Id));
            proprietaryInventory.ForEach(x =>
            {
                x.InventorySourceName = data[x.Id].InventorySourceName;
                x.UnitType = data[x.Id].UnitType;
                x.DaypartName = data[x.Id].DaypartName;
            });
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
            var standardDayparts = _StandardDaypartService.GetAllStandardDayparts();

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
                WeekdaysWeighting = x.WeekdaysWeighting,
                WeekendWeighting = x.WeekendWeighting,
                FlightDays = plan.FlightDays.ToList(),
                VpvhForAudiences = x.VpvhForAudiences,
            }).ToList();

            plan.Dayparts = planDayparts.OrderDayparts(standardDayparts);
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
        public PlanDeliveryBudget CalculateBudget(PlanDeliveryBudget planBudget)
        {
            var impressionsHasValue = planBudget.Impressions.HasValue;
            if (impressionsHasValue)
            {
                // the UI is sending the user entered value instead of the raw value. BE needs to adjust
                // this value is only adjusted for calculations
                planBudget.Impressions = planBudget.Impressions.Value * 1000;
            }

            var result = _BudgetCalculator.CalculateBudget(planBudget);
            // if the UI is sending the user entered value keep it as it is or calculated value means round it off
            if (impressionsHasValue)
            {
                result.Impressions =result.Impressions.Value / 1000;
            }
            else
            {
                result.Impressions = Math.Floor(result.Impressions.Value / 1000);
            }          
            // BP-2482 : Found that the database plan_version.target_cpm, target_cpp and budget columns are Sql Type Money.
            // On insert their values will be rounded to the 4th decimal.
            // We will do the same here to close the loop.
            result.CPP = result.CPP.AsSqlTypeMoney();
            result.CPM = result.CPM.AsSqlTypeMoney();
            result.Budget = result.Budget.AsSqlTypeMoney();

            return result;
        }

        ///<inheritdoc/>
        public List<PlanDeliveryPostingTypeBudget> CalculatePostingTypeBudgets(PlanDeliveryPostingTypeBudget budgetRequest)
        {
            // calculate for the given posting type
            var originalBudgetInput = _MapRequestToBudget(budgetRequest);
            var originalBudgetCalculationResult = CalculateBudget(originalBudgetInput);
            var originalPostingTypeResults = _MapBudgetToResult(budgetRequest.PostingType, budgetRequest.StandardDaypartId, originalBudgetCalculationResult);

            // convert and calculate for the other posting type
            var otherPostingType = budgetRequest.PostingType == PostingTypeEnum.NSI
                ? PostingTypeEnum.NTI
                : PostingTypeEnum.NSI;

            var conversionRates = _NtiToNsiConversionRepository.GetLatestNtiToNsiConversionRates();
            var conversionRate = conversionRates.Single(r => r.StandardDaypartId == budgetRequest.StandardDaypartId);
            
            var convertedImpressions = budgetRequest.DeliveryImpressions;
            var convertedRatingPoints = budgetRequest.DeliveryRatingPoints;

            var otherBudgetInput = _MapRequestToBudget(budgetRequest);

            // NTI should be lower than NSI
            // calculating rate or budget; the other is static;
            if (budgetRequest.DeliveryImpressions.HasValue)
            {
                convertedImpressions =
                    Math.Floor(otherPostingType == PostingTypeEnum.NTI
                        ? budgetRequest.DeliveryImpressions.Value * conversionRate.ConversionRate
                        : budgetRequest.DeliveryImpressions.Value / conversionRate.ConversionRate);
            }
            else if (budgetRequest.DeliveryRatingPoints.HasValue)
            {
                convertedRatingPoints =
                    otherPostingType == PostingTypeEnum.NTI
                        ? budgetRequest.DeliveryRatingPoints.Value * conversionRate.ConversionRate
                        : budgetRequest.DeliveryRatingPoints.Value / conversionRate.ConversionRate;
            }
            // calculating delivery; budget is static;
            else
            {
                // copy the budget to keep it static
                // force calculation of the rate by clearing out the other fields.
                otherBudgetInput.Budget = budgetRequest.Budget;
                otherBudgetInput.CPM = null;
                otherBudgetInput.CPP = null;
                // convert from the previous results per the correct delivery type
                convertedRatingPoints = null;
                convertedImpressions = null;
                if (budgetRequest.CPM.HasValue)
                {
                    convertedImpressions =
                        otherPostingType == PostingTypeEnum.NTI
                            ? originalBudgetCalculationResult.Impressions.Value * conversionRate.ConversionRate
                            : originalBudgetCalculationResult.Impressions.Value / conversionRate.ConversionRate;
                }
                else
                {
                    convertedRatingPoints =
                        otherPostingType == PostingTypeEnum.NTI
                            ? originalBudgetCalculationResult.RatingPoints.Value * conversionRate.ConversionRate
                            : originalBudgetCalculationResult.RatingPoints.Value / conversionRate.ConversionRate;
                }
            }
            
            otherBudgetInput.Impressions = convertedImpressions;
            otherBudgetInput.RatingPoints = convertedRatingPoints;

            var otherBudgetCalculationResult = CalculateBudget(otherBudgetInput);
            var otherPostingTypeResults = _MapBudgetToResult(otherPostingType, budgetRequest.StandardDaypartId, otherBudgetCalculationResult);

            var results = new List<PlanDeliveryPostingTypeBudget>
            {
                originalPostingTypeResults,
                otherPostingTypeResults
            };
            return results;
        }

        private PlanDeliveryBudget _MapRequestToBudget(PlanDeliveryPostingTypeBudget budgetRequest)
        {
            var result = new PlanDeliveryBudget
            {
                Budget = budgetRequest.Budget,
                Impressions = budgetRequest.DeliveryImpressions,
                RatingPoints = budgetRequest.DeliveryRatingPoints,
                CPM = budgetRequest.CPM,
                CPP= budgetRequest.CPP,
                Universe = budgetRequest.Universe,
                AudienceId = budgetRequest.AudienceId
            };
            return result;
        }

        private PlanDeliveryPostingTypeBudget _MapBudgetToResult(PostingTypeEnum postingType, int standardDaypartId, PlanDeliveryBudget calculationResult)
        {
            var result = new PlanDeliveryPostingTypeBudget
            {
                PostingType = postingType,
                StandardDaypartId = standardDaypartId,
                Budget = calculationResult.Budget,
                DeliveryImpressions = calculationResult.Impressions,
                DeliveryRatingPoints = calculationResult.RatingPoints,
                CPM = calculationResult.CPM,
                CPP = calculationResult.CPP,
                Universe = calculationResult.Universe,
                AudienceId = calculationResult.AudienceId
            };
            return result;
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

                    int oldPlanVersionId = plan.VersionId;

                    _SetPlanVersionNumber(plan);
                    _PlanRepository.SavePlan(plan, updatedBy, updatedDate);
                    _PlanRepository.UpdatePlanPricingVersionId(plan.VersionId, oldPlanVersionId);
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
                WeekdaysWeighting = 70d,
                WeekendWeighting = 30d,
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

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateDefaultPlanAvailableMarkets()
        {
            var latestMarketCoverages = _MarketCoverageRepository.GetMarketsWithLatestCoverage();
            var markets = latestMarketCoverages.Select(m => new PlanAvailableMarketDto
            {
                MarketCode = Convert.ToInt16(m.MarketCode),
                MarketCoverageFileId = m.MarketCoverageFileId,
                Rank = m.Rank.Value,
                PercentageOfUS = m.PercentageOfUS,
                Market = m.Market,
                ShareOfVoicePercent = m.PercentageOfUS,
                IsUserShareOfVoicePercent = false
            }).ToList();

            var result = _PlanMarketSovCalculator.CalculateMarketWeights(markets);
            return result;
        }

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketWeightChange(List<PlanAvailableMarketDto> availableMarkets,
            short modifiedMarketCode, double? userEnteredValue)
        {
            var result = _PlanMarketSovCalculator.CalculateMarketWeightChange(availableMarkets, modifiedMarketCode, userEnteredValue);
            return result;
        }

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketsAdded(List<PlanAvailableMarketDto> beforeMarkets,
            List<PlanAvailableMarketDto> addedMarkets)
        {
            var result = _PlanMarketSovCalculator.CalculateMarketsAdded(beforeMarkets, addedMarkets);
            return result;
        }

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketsRemoved(List<PlanAvailableMarketDto> beforeMarkets,
            List<short> removedMarketCodes)
        {
            var result = _PlanMarketSovCalculator.CalculateMarketsRemoved(beforeMarkets, removedMarketCodes);
            return result;
        }

        /// <inheritdoc />
        public PlanAvailableMarketCalculationResult CalculateMarketWeightsClearAll(List<PlanAvailableMarketDto> availableMarkets)
        {
            var result = _PlanMarketSovCalculator.CalculateMarketWeightsClearAll(availableMarkets);
            return result;
        }

        /// <inheritdoc />
        public bool CommitPricingAllocationModel(int planId, SpotAllocationModelMode spotAllocationModelMode,
            PostingTypeEnum postingType, string username, bool aggregatePlanSynchronously = false)
        {
            var beforePlan = _PlanRepository.GetPlan(planId);
            var beforePlanVersionId = beforePlan.VersionId;

            // get the latest pricing results per the given 
            var jobExecutions = _PlanPricingService.GetAllCurrentPricingExecutions(planId, null);
            if (jobExecutions.Job == null)
            {
                throw new InvalidOperationException($"Did not find a pricing job for PlanId='{planId}'.");
            }

            var pricingResults = jobExecutions.Results?.SingleOrDefault(s => 
                s.SpotAllocationModelMode == spotAllocationModelMode && 
                s.PostingType == postingType);

            if (pricingResults == null)
            {
                throw new InvalidOperationException(
                    $"Did not find pricing results for PlanId='{beforePlan.Id}'; JobId='{jobExecutions.Job.Id}'; SpotAllocationMode='{spotAllocationModelMode}'; PostingType='{postingType}';");
            }

            // Set these known values 
            beforePlan.SpotAllocationModelMode = pricingResults.SpotAllocationModelMode;
            beforePlan.PostingType = pricingResults.PostingType;

            beforePlan.TargetCPM = pricingResults.OptimalCpm;
            beforePlan.Budget = pricingResults.TotalBudget;

            // We will recalculate the impressions and ratings components.
            var planDeliveryBudget = new PlanDeliveryBudget
            {
                AudienceId = beforePlan.AudienceId,
                Budget = pricingResults.TotalBudget,
                CPM = pricingResults.OptimalCpm
            };
            var calculatedBudget = CalculateBudget(planDeliveryBudget);

            // multiply by 1000 for Imps (000) because the CalculateMethod returns without them.
            beforePlan.TargetImpressions = calculatedBudget.Impressions * 1000;
            beforePlan.TargetRatingPoints = calculatedBudget.RatingPoints;
            beforePlan.TargetCPP = calculatedBudget.CPP;

            // recalculate the weekly breakdown table
            var weeklyBreakdownRequest = new WeeklyBreakdownRequest
            {
                FlightStartDate = beforePlan.FlightStartDate.Value,
                FlightEndDate = beforePlan.FlightEndDate.Value,
                FlightDays = beforePlan.FlightDays,
                FlightHiatusDays = beforePlan.FlightHiatusDays,
                DeliveryType = beforePlan.GoalBreakdownType,
                TotalImpressions = beforePlan.TargetImpressions.Value,
                TotalRatings = beforePlan.TargetRatingPoints.Value,
                TotalBudget = beforePlan.Budget.Value,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                Weeks = beforePlan.WeeklyBreakdownWeeks,
                CreativeLengths = beforePlan.CreativeLengths,
                Dayparts = beforePlan.Dayparts,
                ImpressionsPerUnit = beforePlan.ImpressionsPerUnit,
                Equivalized = beforePlan.Equivalized
            };

            var calculatedWeeklyBreakdown = CalculatePlanWeeklyGoalBreakdown(weeklyBreakdownRequest);

            beforePlan.WeeklyBreakdownWeeks = calculatedWeeklyBreakdown.Weeks;
            beforePlan.WeeklyBreakdownTotals.TotalBudget = calculatedWeeklyBreakdown.TotalBudget;
            beforePlan.WeeklyBreakdownTotals.TotalImpressions = calculatedWeeklyBreakdown.TotalImpressions;
            beforePlan.WeeklyBreakdownTotals.TotalRatingPoints = calculatedWeeklyBreakdown.TotalRatingPoints;
            beforePlan.WeeklyBreakdownTotals.TotalImpressionsPercentage = calculatedWeeklyBreakdown.TotalImpressionsPercentage;
            beforePlan.WeeklyBreakdownTotals.TotalActiveDays = calculatedWeeklyBreakdown.TotalActiveDays;
            beforePlan.WeeklyBreakdownTotals.TotalUnits = calculatedWeeklyBreakdown.TotalUnits;

            beforePlan.WeeklyBreakdownWeeks = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(beforePlan);

            // finalize pre-save
            _CalculateDeliveryDataPerAudience(beforePlan);
            _SetPlanVersionNumber(beforePlan);
            
            // save
            var currentDateTime = _GetCurrentDateTime();
            _PlanRepository.SavePlan(beforePlan, username, currentDateTime);

            // finalize post-save
            _UpdateCampaignLastModified(beforePlan.CampaignId, currentDateTime, username);
            _DispatchPlanAggregation(beforePlan, aggregatePlanSynchronously); 
            _CampaignAggregationJobTrigger.TriggerJob(beforePlan.CampaignId, username);

            // promote the existing results to the new plan version
            var afterPlan = _PlanRepository.GetPlan(planId);
            var afterPlanVersionId = afterPlan.VersionId;
            
            _PlanRepository.UpdatePlanPricingVersionId(afterPlanVersionId, beforePlanVersionId);
            _PlanRepository.UpdatePlanBuyingVersionId(afterPlanVersionId, beforePlanVersionId);

            return true;
        }
    }
}