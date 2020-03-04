using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
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
    }

    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanValidator _PlanValidator;
        private readonly IPlanBudgetDeliveryCalculator _BudgetCalculator;
        private readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly IPlanAggregator _PlanAggregator;
        private readonly IPlanSummaryRepository _PlanSummaryRepository;
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;
        private readonly ICampaignAggregationJobTrigger _CampaignAggregationJobTrigger;
        private readonly INsiUniverseService _NsiUniverseService;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IBroadcastLockingManagerApplicationService _LockingManagerApplicationService;
        private readonly IPlanPricingService _PlanPricingService;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IDaypartDefaultService _DaypartDefaultService;
        private readonly IDayRepository _DayRepository;

        private const string _DaypartDefaultNotFoundMessage = "Unable to find daypart default";

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
            , IDaypartDefaultService daypartDefaultService)
        {
            _MediaWeekCache = mediaMonthAndWeekAggregateCache;
            _PlanValidator = planValidator;
            _BudgetCalculator = planBudgetDeliveryCalculator;

            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
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
        }

        ///<inheritdoc/>
        public int SavePlan(PlanDto plan, string createdBy, DateTime createdDate, bool aggregatePlanSynchronously = false)
        {
            if (plan.Id >= 0 && _PlanPricingService.IsPricingModelRunningForPlan(plan.Id))
            {
                throw new Exception("The pricing model is running for the plan");
            }

            DaypartTimeHelper.SubtractOneSecondToEndTime(plan.Dayparts);

            _CalculateDaypartOverrides(plan.Dayparts);
            _PlanValidator.ValidatePlan(plan);

            _ConvertImpressionsToRawFormat(plan);

            plan.TargetUniverse = _NsiUniverseService.GetAudienceUniverseForMediaMonth(plan.ShareBookId, plan.AudienceId);
            _CalculateHouseholdDeliveryData(plan);
            _CalculateSecondaryAudiencesDeliveryData(plan);
            _SetPlanVersionNumber(plan);
            _SetPlanFlightDays(plan);

            if (plan.Status == PlanStatusEnum.Contracted && plan.GoalBreakdownType != PlanGoalBreakdownTypeEnum.Custom)
            {
                plan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.Custom;
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
                Margin = pricingDefaults.Margin
            };
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
                week.WeeklyAdu = 0;
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
                plan.TargetImpressions = plan.TargetImpressions.Value / 1000;
            }
            foreach (var audience in plan.SecondaryAudiences)
            {
                audience.Impressions = audience.Impressions.Value / 1000;
            }
            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                week.WeeklyImpressions = week.WeeklyImpressions / 1000;
            }
        }

        private void _CalculateDaypartOverrides(List<PlanDaypartDto> planDayparts)
        {
            var daypartDefaults = _DaypartDefaultRepository.GetAllActiveDaypartDefaultsWithAllData();

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

            _SetWeekNumber(plan.WeeklyBreakdownWeeks);
            _SetWeeklyBreakdownTotals(plan);
            DaypartTimeHelper.AddOneSecondToEndTime(plan.Dayparts);

            _SetPlanTotals(plan);
            _SetDefaultDaypartRestrictions(plan);
            _ConvertImpressionsToUserFormat(plan);

            _SortPlanDayparts(plan);
            _SortProgramRestrictions(plan);

            return plan;
        }

        private void _SetWeeklyBreakdownTotals(PlanDto plan)
        {
            plan.WeeklyBreakdownTotals.TotalActiveDays = plan.WeeklyBreakdownWeeks.Sum(w => w.NumberOfActiveDays);
            plan.WeeklyBreakdownTotals.TotalBudget = plan.WeeklyBreakdownWeeks.Sum(w => w.WeeklyBudget);
            plan.WeeklyBreakdownTotals.TotalImpressions = Math.Floor(plan.WeeklyBreakdownWeeks.Sum(w => w.WeeklyImpressions)/1000);
            var impressionsTotalsRatio = plan.TargetImpressions.HasValue && plan.TargetImpressions.Value > 0 
                ? plan.WeeklyBreakdownTotals.TotalImpressions / plan.TargetImpressions.Value : 0;

            plan.WeeklyBreakdownTotals.TotalRatingPoints = Math.Round(plan.TargetRatingPoints ?? 0 * impressionsTotalsRatio, 1);
            plan.WeeklyBreakdownTotals.TotalImpressionsPercentage = Math.Round(100 * impressionsTotalsRatio, 0);
        }

        private void _SortPlanDayparts(PlanDto plan)
        {
            var daypartDefaults = _DaypartDefaultService.GetAllDaypartDefaults();
            plan.Dayparts = plan.Dayparts.OrderDayparts(daypartDefaults);
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
            WeeklyBreakdownResponseDto response = new WeeklyBreakdownResponseDto();
            _PlanValidator.ValidateWeeklyBreakdown(request);

            //calculate flight weeks based on start/end date of the flight
            List<DisplayMediaWeek> weeks = _MediaWeekCache.GetDisplayMediaWeekByFlight(request.FlightStartDate, request.FlightEndDate);

            //add all the days outside of the flight for the first and last week as hiatus days
            request.FlightHiatusDays.AddRange(_GetDaysOutsideOfTheFlight(request.FlightStartDate, request.FlightEndDate, weeks));

            if (request.DeliveryType.Equals(PlanGoalBreakdownTypeEnum.Even) ||
                (request.DeliveryType.Equals(PlanGoalBreakdownTypeEnum.Custom) && !request.Weeks.Any()))
            {
                response = _CalculateEvenPlanWeeklyGoalBreakdown(request, weeks);
            }
            else
            {
                response = _CalculateCustomPlanWeeklyGoalBreakdown(request, weeks);
            }
            return response;
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

        private void _MapWeeksToResult(WeeklyBreakdownResponseDto result, List<DisplayMediaWeek> weeks, WeeklyBreakdownRequest request, bool isCustom = false)
        {
            var weekNumber = 1;
            foreach (DisplayMediaWeek week in weeks)
            {
                var activeDays = _CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, request.FlightDays, request.FlightHiatusDays, out string activeDaysString);
                var weeklyBreakdownWeek = new WeeklyBreakdownWeek
                {
                    ActiveDays = activeDaysString,
                    NumberOfActiveDays = activeDays,
                    StartDate = week.WeekStartDate,
                    EndDate = week.WeekEndDate,
                    MediaWeekId = week.Id
                };

                if (!isCustom)
                {
                    weeklyBreakdownWeek.WeekNumber = weekNumber++;

                    if (request.Weeks?.Any(w => w.StartDate == week.WeekStartDate && w.EndDate == week.WeekEndDate) == true)
                    {
                        weeklyBreakdownWeek.WeeklyAdu = request.Weeks.Find(w => w.StartDate == week.WeekStartDate && w.EndDate == week.WeekEndDate).WeeklyAdu;
                    }
                }

                result.Weeks.Add(weeklyBreakdownWeek);
            }
        }

        private WeeklyBreakdownResponseDto _CalculateEvenPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks)
        {
            var result = new WeeklyBreakdownResponseDto();
            _MapWeeksToResult(result, weeks, request);

            _CalculateEvenRatingPointsImpressionsAndPercentage(request, result.Weeks);
            _CalculateWeeklyGoalBreakdownTotals(result, request.TotalImpressions, request.TotalRatings);
            return result;
        }

        private WeeklyBreakdownResponseDto _CalculateCustomPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks)
        {
            var result = new WeeklyBreakdownResponseDto();

            //remove deleted weeks
            _RemoveDeletedWeeks(request.Weeks, weeks);

            //add the remain weeks
            result.Weeks.AddRange(request.Weeks);

            List<WeeklyBreakdownWeek> weeksToUpdate;
            // If Updated Week present compute only for the updated week, else do it for all the weeks.
            bool redistributeCustom = false;
            double oldImpressionTotals = 0;
            if(request.UpdatedWeek <= 0)
            {
                weeksToUpdate = request.Weeks.ToList();
                //if recalculating the whole week, always calculate from impressions
                request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions;
                redistributeCustom = true; //redistribute goal impressions in same proportions
                oldImpressionTotals = weeksToUpdate.Sum(w => w.WeeklyImpressions);
            }
            else
            {
                weeksToUpdate = request.Weeks.Where(w => w.WeekNumber == request.UpdatedWeek).ToList();
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
                        if (redistributeCustom && oldImpressionTotals > 0) {
                            week.WeeklyImpressions = Math.Floor(request.TotalImpressions * week.WeeklyImpressions / oldImpressionTotals);
                        }
                        else
                        {
                            week.WeeklyImpressions = Math.Floor(week.WeeklyImpressions);
                        }
                        
                        break;
                }
                var weeklyRatio = request.TotalImpressions <= 0 ? 0 : week.WeeklyImpressions / request.TotalImpressions;
                week.WeeklyImpressionsPercentage = Math.Round(weeklyRatio * 100);
                week.WeeklyRatings = ProposalMath.RoundDownWithDecimals(request.TotalRatings * weeklyRatio, 1);

                _CalculateWeeklyBudget(request, week);
            }
            //add the missing weeks
            _MapWeeksToResult(result, weeks.Where(x => !request.Weeks.Select(y => y.StartDate).Contains(x.WeekStartDate)).ToList(), request, true);

            //only adjust first week if redistributing
            if (result.Weeks.Where(w => w.NumberOfActiveDays > 0).Any() && redistributeCustom)
            {
                UpdateFirstWeekAndBudgetAdjustment(request, result.Weeks);
            }
            _CalculateWeeklyGoalBreakdownTotals(result, request.TotalImpressions, request.TotalRatings);


            //the order of the weeks might be incorrect, so do the order
            result.Weeks = result.Weeks.OrderBy(x => x.StartDate).ToList();
            _SetWeekNumber(result.Weeks);

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

        private static void _CalculateWeeklyGoalBreakdownTotals(WeeklyBreakdownResponseDto weeklyBreakdown, double goalImpressions, double goalRatingPoints)
        {
            weeklyBreakdown.TotalActiveDays = weeklyBreakdown.Weeks.Sum(x => x.NumberOfActiveDays);
            weeklyBreakdown.TotalBudget = weeklyBreakdown.Weeks.Sum(w => w.WeeklyBudget);
            weeklyBreakdown.TotalImpressions = weeklyBreakdown.Weeks.Sum(w => w.WeeklyImpressions);
            var impressionsTotalRatio = goalImpressions > 0 ? weeklyBreakdown.TotalImpressions / goalImpressions : 0;

            weeklyBreakdown.TotalShareOfVoice = Math.Round(100 * impressionsTotalRatio, 0);
            weeklyBreakdown.TotalImpressionsPercentage = weeklyBreakdown.TotalShareOfVoice;

            weeklyBreakdown.TotalRatingPoints = Math.Round(goalRatingPoints * impressionsTotalRatio, 1);

        }

        private void _RemoveDeletedWeeks(List<WeeklyBreakdownWeek> requestWeeks, List<DisplayMediaWeek> flightWeeks)
        {
            var flightStartDates = flightWeeks.Select(x => x.WeekStartDate).ToList();
            var weeksToRemove = requestWeeks.Where(x => !flightStartDates.Contains(x.StartDate)).ToList();
            weeksToRemove.ForEach(x => requestWeeks.Remove(x));
        }

        private void _SetWeekNumber(IEnumerable<WeeklyBreakdownWeek> weeks)
        {
            int i = 1;
            foreach (var week in weeks)
            {
                week.WeekNumber = i++;
            }
        }

        private void _CalculateEvenRatingPointsImpressionsAndPercentage(WeeklyBreakdownRequest request, List<WeeklyBreakdownWeek> weeks)
        {
            var activeWeeks = weeks.Where(x => x.NumberOfActiveDays > 0);
            var totalActiveWeeks = activeWeeks.Count();

            var roundedImpressions = Math.Floor(request.TotalImpressions / totalActiveWeeks);
            var roundedImpressionsPercentage = Math.Floor(100 * roundedImpressions / request.TotalImpressions);
            var roundedRatingPoints = ProposalMath.RoundDownWithDecimals(request.TotalRatings * roundedImpressions/request.TotalImpressions, 1);
            foreach (var week in activeWeeks)
            {               
                week.WeeklyImpressions = roundedImpressions;

                week.WeeklyImpressionsPercentage = roundedImpressionsPercentage;

                week.WeeklyRatings = roundedRatingPoints;

                _CalculateWeeklyBudget(request, week);
            }

            if (activeWeeks.Any())
            {
                UpdateFirstWeekAndBudgetAdjustment(request, weeks);
            }
        }

        private void UpdateFirstWeekAndBudgetAdjustment(WeeklyBreakdownRequest request, List<WeeklyBreakdownWeek> weeks)
        {

            var totalImpressionsRounded = weeks.Sum(w => w.WeeklyImpressions);
            var totalImpressionsPercentageRounded = weeks.Sum(w => w.WeeklyImpressionsPercentage);
            var totalRatingPointsRounded = weeks.Sum(w => w.WeeklyRatings);

            var firstWeek = weeks.First();

            var roundedImpressionsDifference = request.TotalImpressions - totalImpressionsRounded;
            var roundedPercentageDifference = 100 - totalImpressionsPercentageRounded;
            var roundedRatingPointsDifference = request.TotalRatings - totalRatingPointsRounded;

            firstWeek.WeeklyImpressions = Math.Floor(firstWeek.WeeklyImpressions + roundedImpressionsDifference);
            firstWeek.WeeklyImpressionsPercentage = Math.Floor(firstWeek.WeeklyImpressionsPercentage + roundedPercentageDifference);
            firstWeek.WeeklyRatings = Math.Round(firstWeek.WeeklyRatings + roundedRatingPointsDifference, 1);

            _CalculateWeeklyBudget(request, firstWeek);

        }

        private void _CalculateWeeklyBudget(WeeklyBreakdownRequest request, WeeklyBreakdownWeek week)
        {
            if(request.TotalImpressions <= 0 )
            {
                week.WeeklyBudget = 0;
            }
            week.WeeklyBudget = (decimal)week.WeeklyImpressions * (request.TotalBudget / (decimal)request.TotalImpressions);
        }

        private int _CalculateActiveDays(DateTime weekStartDate, DateTime weekEndDate, List<int> flightDays, List<DateTime> hiatusDays, out string activeDaysString)
        {
            var daysOfWeek = new List<string> { "M", "Tu", "W", "Th", "F", "Sa", "Su" };
            activeDaysString = string.Empty;
            var hiatusDaysInWeek = hiatusDays.Where(x => weekStartDate <= x && weekEndDate >= x).ToList();
            var days = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            var daysToRemove = days.Except(flightDays);

            foreach(var day in daysToRemove)
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
                SpotLengthId = defaultSpotLengthId,
                Equivalized = true,
                AudienceType = AudienceTypeEnum.Nielsen,
                PostingType = PostingTypeEnum.NTI,
                Status = PlanStatusEnum.Working,
                Currency = PlanCurrenciesEnum.Impressions,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.Even,
                ShowTypeContainType = ContainTypeEnum.Exclude,
                GenreContainType = ContainTypeEnum.Exclude,
                ProgramContainType = ContainTypeEnum.Exclude,
                AffiliateContainType = ContainTypeEnum.Exclude,
                CoverageGoalPercent = 80d,
                BlackoutMarkets = new List<PlanBlackoutMarketDto>(),
                FlightHiatusDays = new List<DateTime>(),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
            };
        }

        private void _CalculateHouseholdDeliveryData(PlanDto plan)
        {
            var householdPlanDeliveryBudget = _BudgetCalculator.CalculateBudget(new PlanDeliveryBudget
            {
                Impressions = Math.Floor(plan.TargetImpressions.Value / plan.Vpvh),
                AudienceId = _BroadcastAudiencesCache.GetDefaultAudience().Id,
                MediaMonthId = plan.ShareBookId,
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
                    MediaMonthId = plan.ShareBookId,
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

    }
}
