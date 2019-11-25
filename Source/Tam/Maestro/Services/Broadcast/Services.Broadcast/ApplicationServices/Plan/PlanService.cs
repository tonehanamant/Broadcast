using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
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
        /// The logic for automatic status transitioning
        /// </summary>
        /// <param name="transitionDate">The transition date.</param>
        /// <param name="updatedBy">The updated by.</param>
        /// <param name="updatedDate">The updated date.</param>
        /// <param name="aggregatePlanSynchronously"></param>
        void AutomaticStatusTransitions(DateTime transitionDate, string updatedBy, DateTime updatedDate, bool aggregatePlanSynchronously = false);
    }

    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanValidator _PlanValidator;
        private readonly IPlanBudgetDeliveryCalculator _BudgetCalculator;
        private readonly IMediaMonthAndWeekAggregateCache _MediaWeekCache;
        private readonly IPlanAggregator _PlanAggregator;
        private readonly IPlanSummaryRepository _PlanSummaryRepository;
        private readonly IDaypartCodeRepository _DaypartCodeRepository;
        private readonly ICampaignAggregationJobTrigger _CampaignAggregationJobTrigger;
        private readonly INsiUniverseService _NsiUniverseService;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly ILockingManagerApplicationService _LockingManagerApplicationService;
        private readonly IPlanPricingService _PlanPricingService;

        private const string _DaypartCodeNotFoundMessage = "Unable to find daypart code";

        public PlanService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IPlanValidator planValidator
            , IPlanBudgetDeliveryCalculator planBudgetDeliveryCalculator
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IPlanAggregator planAggregator
            , ICampaignAggregationJobTrigger campaignAggregationJobTrigger
            , INsiUniverseService nsiUniverseService
            , IBroadcastAudiencesCache broadcastAudiencesCache
            , ISpotLengthEngine spotLengthEngine
            , ILockingManagerApplicationService lockingManagerApplicationService
            , IPlanPricingService planPricingService)
        {
            _MediaWeekCache = mediaMonthAndWeekAggregateCache;
            _PlanValidator = planValidator;
            _BudgetCalculator = planBudgetDeliveryCalculator;

            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _PlanSummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanSummaryRepository>();
            _DaypartCodeRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartCodeRepository>();
            _PlanAggregator = planAggregator;
            _CampaignAggregationJobTrigger = campaignAggregationJobTrigger;
            _NsiUniverseService = nsiUniverseService;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
            _SpotLengthEngine = spotLengthEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _PlanPricingService = planPricingService;
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

            plan.Universe = _NsiUniverseService.GetAudienceUniverseForMediaMonth(plan.ShareBookId, plan.AudienceId);
            _CalculateHouseholdDeliveryData(plan);
            _CalculateSecondaryAudiencesDeliveryData(plan);

            if (plan.VersionId == 0 || plan.Id == 0)
            {   //this is a new plan, so we're saving version 1
                plan.VersionNumber = 1;
                _PlanRepository.SaveNewPlan(plan, createdBy, createdDate);
            }
            else
            {
                var key = KeyHelper.GetPlanLockingKey(plan.Id);
                var lockingResult = _LockingManagerApplicationService.LockObject(key);

                if (lockingResult.Success)
                {
                    if (plan.IsDraft)
                    {
                        //this is a draft. we create it if none exist or we update it otherwise
                        plan.VersionNumber = null;
                        _PlanRepository.CreateOrUpdateDraft(plan, createdBy, createdDate);
                    }
                    else
                    {
                        //this is a new version.
                        plan.VersionNumber = _PlanRepository.GetLatestVersionNumberForPlan(plan.Id) + 1;
                        _PlanRepository.SavePlan(plan, createdBy, createdDate);
                    }
                }
                else
                {
                    throw new Exception($"The chosen plan has been locked by {lockingResult.LockedUserName}");
                }
            }

            //we only aggregate data for versions, not drafts
            if (!plan.IsDraft)
            {
                _DispatchPlanAggregation(plan, aggregatePlanSynchronously);
                _CampaignAggregationJobTrigger.TriggerJob(plan.CampaignId, createdBy);
            }

            return plan.Id;
        }

        private static void _ConvertImpressionsToRawFormat(PlanDto plan)
        {
            //the UI is sending the user entered value instead of the raw value. BE needs to adjust
            if (plan.DeliveryImpressions.HasValue)
            {                
                plan.DeliveryImpressions = plan.DeliveryImpressions.Value * 1000;
            }
            foreach(var week in plan.WeeklyBreakdownWeeks)
            {
                week.Impressions = week.Impressions * 1000;
            }
        }

        private static void _ConvertImpressionsToUserFormat(PlanDto plan)
        {
            //the UI is sending the user entered value instead of the raw value. BE needs to adjust
            if (plan.DeliveryImpressions.HasValue)
            {
                plan.DeliveryImpressions = plan.DeliveryImpressions.Value / 1000;
            }
            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                week.Impressions = week.Impressions / 1000;
            }
        }

        private void _CalculateDaypartOverrides(List<PlanDaypartDto> dayparts)
        {
            var daypartCodeDefaults = _DaypartCodeRepository.GetDaypartCodeDefaults();

            foreach (var planDaypart in dayparts)
            {
                var daypartCodeDefault = daypartCodeDefaults.Single(x => x.Id == planDaypart.DaypartCodeId, _DaypartCodeNotFoundMessage);

                planDaypart.IsStartTimeModified = planDaypart.StartTimeSeconds != daypartCodeDefault.DefaultStartTimeSeconds;
                planDaypart.IsEndTimeModified = planDaypart.EndTimeSeconds != daypartCodeDefault.DefaultEndTimeSeconds;
            }
        }

        ///<inheritdoc/>
        public PlanDto GetPlan(int planId, int? versionId = null)
        {
            PlanDto plan = _PlanRepository.GetPlan(planId, versionId);
            
            plan.IsPricingModelRunning = _PlanPricingService.IsPricingModelRunningForPlan(planId);

            _SetWeekNumber(plan.WeeklyBreakdownWeeks);
            DaypartTimeHelper.AddOneSecondToEndTime(plan.Dayparts);

            _SetPlanTotals(plan);
            _SetDefaultDaypartRestrictions(plan);
            _ConvertImpressionsToUserFormat(plan);

            return plan;
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

            foreach(var version in planVersions.Where(x=>x.VersionId != baseVersion.VersionId).ToList())
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
            if(baseVersion.Dayparts.Count() != version.Dayparts.Count())
            {
                return true;
            }

            //check if the dayparts themselves are different
            foreach(var daypart in baseVersion.Dayparts)
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
            if(baseVersion.HiatusDays.Count() != version.HiatusDays.Count())
            {
                return true;
            }

            //check if the hiatus days themselves are different
            foreach(var date in baseVersion.HiatusDays)
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
            }
        }

        private static void _SetPlanTotals(PlanDto plan)
        {
            plan.TotalActiveDays = plan.WeeklyBreakdownWeeks.Select(x => x.NumberOfActiveDays).Sum();
            plan.TotalHiatusDays = plan.FlightHiatusDays.Count();
            plan.TotalShareOfVoice = plan.WeeklyBreakdownWeeks.Select(x => x.ShareOfVoice).Sum();
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
            var deliveryImpressionsHasValue = planBudget.DeliveryImpressions.HasValue;
            if (deliveryImpressionsHasValue)
            {
                // the UI is sending the user entered value instead of the raw value. BE needs to adjust
                // this value is only adjusted for calculations
                planBudget.DeliveryImpressions = planBudget.DeliveryImpressions.Value * 1000;
            }

            planBudget = _BudgetCalculator.CalculateBudget(planBudget);

            if (deliveryImpressionsHasValue)
            {
                // reset the DeliveryImpressions's value to what was entered by the user
                planBudget.DeliveryImpressions = planBudget.DeliveryImpressions.Value / 1000;
            }

            return planBudget;
        }

        ///<inheritdoc/>
        public List<LookupDto> PlanGoalBreakdownTypes()
        {
            return EnumExtensions.ToLookupDtoList<PlanGoalBreakdownTypeEnum>(); ;
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

            if (request.DeliveryType.Equals(PlanGoalBreakdownTypeEnum.Even) || (request.DeliveryType.Equals(PlanGoalBreakdownTypeEnum.Custom) && !request.Weeks.Any()))
            {
                response = _CalculateEvenPlanWeeklyGoalBreakdown(request, weeks);
            }
            else
            {
                response = _CalculateCustomPlanWeeklyGoalBreakdown(request, weeks);
            }
            return response;
        }

        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new int[] { 5 * 60 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
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

        private void _AddMissingWeeks(WeeklyBreakdownResponseDto result, List<DisplayMediaWeek> weeks, List<DateTime> flightHiatusDays, bool isCustom = false)
        {
            var weekNumber = 1;
            foreach (DisplayMediaWeek week in weeks)
            {
                var activeDays = _CalculateActiveDays(week, flightHiatusDays, out string activeDaysString);
                var weeklyBreakdown = new WeeklyBreakdownWeek
                {
                    ActiveDays = activeDaysString,
                    NumberOfActiveDays = activeDays,
                    StartDate = week.WeekStartDate,
                    EndDate = week.WeekEndDate,
                    MediaWeekId = week.Id
                };

                if (!isCustom)
                    weeklyBreakdown.WeekNumber = weekNumber++;

                result.Weeks.Add(weeklyBreakdown);
            }
        }

        private WeeklyBreakdownResponseDto _CalculateEvenPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks)
        {
            var result = new WeeklyBreakdownResponseDto();
            _AddMissingWeeks(result, weeks, request.FlightHiatusDays);

            _CalculateImpressionsAndShareOfVoice(result.Weeks, request.TotalImpressions);
            _CalculateWeeklyGoalBreakdownTotals(result);
            return result;
        }

        private WeeklyBreakdownResponseDto _CalculateCustomPlanWeeklyGoalBreakdown(WeeklyBreakdownRequest request, List<DisplayMediaWeek> weeks)
        {
            var result = new WeeklyBreakdownResponseDto();

            //remove deleted weeks
            _RemoveDeletedWeeks(request.Weeks, weeks);

            //add the remain weeks
            result.Weeks.AddRange(request.Weeks);

            //update ActiveDays reamin weeks
            foreach (var week in result.Weeks)
            {
                week.NumberOfActiveDays = _CalculateActiveDays(week, request.FlightHiatusDays, out string activeDaysString);
                week.ActiveDays = activeDaysString;
                if (week.NumberOfActiveDays < 1)
                {
                    week.Impressions = 0;
                    week.ShareOfVoice = 0;
                }
            }

            //add the missing weeks
            _AddMissingWeeks(result, weeks.Where(x => !request.Weeks.Select(y => y.StartDate).Contains(x.WeekStartDate)).ToList(), request.FlightHiatusDays, true);

            _CalculateWeeklyGoalBreakdownTotals(result);

            //the order of the weeks might be incorect, so do the order
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

        private static void _CalculateWeeklyGoalBreakdownTotals(WeeklyBreakdownResponseDto result)
        {
            result.TotalActiveDays = result.Weeks.Select(x => x.NumberOfActiveDays).Sum();
            result.TotalShareOfVoice = Math.Round(result.Weeks.Select(x => x.ShareOfVoice).Sum(), 2);
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

        private void _CalculateImpressionsAndShareOfVoice(List<WeeklyBreakdownWeek> weeks, double totalImpressions)
        {
            var activeWeeks = weeks.Where(x => x.NumberOfActiveDays > 0);
            foreach (var week in activeWeeks)
            {
                week.Impressions = totalImpressions / activeWeeks.Count();
                week.ShareOfVoice = (double)100 / activeWeeks.Count();
            }
        }

        private int _CalculateActiveDays(DateTime weekStartDate, DateTime weekEndDate, List<DateTime> hiatusDays, out string activeDaysString)
        {
            var daysOfWeek = new List<string> { "M", "Tu", "W", "Th", "F", "Sa", "Su" };
            activeDaysString = string.Empty;
            var hiatusDaysInWeek = hiatusDays.Where(x => weekStartDate <= x && weekEndDate >= x).ToList();

            //if there are no hiatus days in this week just return 7 active days
            if (!hiatusDaysInWeek.Any())
            {
                activeDaysString = "M-Su";
                return 7;
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
            var groupOfActiveDays = daysOfWeek.GroupConnected((a) => string.IsNullOrWhiteSpace(a));
            var activeDaysList = new List<string>();
            foreach (var group in groupOfActiveDays.Where(x => x.Count() > 0))
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
            //number of active days this week is 7 minus number of hiatus days
            return 7 - hiatusDaysInWeek.Count();
        }

        private int _CalculateActiveDays(DisplayMediaWeek week, List<DateTime> hiatusDays, out string activeDaysString)
        {
            return _CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, hiatusDays, out activeDaysString);
        }

        private int _CalculateActiveDays(WeeklyBreakdownWeek week, List<DateTime> hiatusDays, out string activeDaysString)
        {
            return _CalculateActiveDays(week.StartDate, week.EndDate, hiatusDays, out activeDaysString);
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
                CoverageGoalPercent = 80d,
                BlackoutMarkets = new List<PlanBlackoutMarketDto>(),
                FlightHiatusDays = new List<DateTime>(),
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
            };
        }

        private void _CalculateHouseholdDeliveryData(PlanDto plan)
        {
            var householdPlanDeliveryBudget = _BudgetCalculator.CalculateBudget(new PlanDeliveryBudget
            {
                DeliveryImpressions = plan.DeliveryImpressions.Value / plan.Vpvh,
                AudienceId = _BroadcastAudiencesCache.GetDefaultAudience().Id,
                MediaMonthId = plan.ShareBookId,
                Budget = plan.Budget
            });

            plan.HouseholdUniverse = householdPlanDeliveryBudget.Universe.Value;
            plan.HouseholdDeliveryImpressions = householdPlanDeliveryBudget.DeliveryImpressions.Value;
            plan.HouseholdCPM = householdPlanDeliveryBudget.CPM.Value;
            plan.HouseholdRatingPoints = householdPlanDeliveryBudget.DeliveryRatingPoints.Value;
            plan.HouseholdCPP = householdPlanDeliveryBudget.CPP.Value;
        }

        private void _CalculateSecondaryAudiencesDeliveryData(PlanDto plan)
        {
            Parallel.ForEach(plan.SecondaryAudiences, (planAudience) =>
            {
                var planDeliveryBudget = _BudgetCalculator.CalculateBudget(new PlanDeliveryBudget
                {
                    DeliveryImpressions = plan.HouseholdDeliveryImpressions * planAudience.Vpvh,
                    AudienceId = planAudience.AudienceId,
                    MediaMonthId = plan.ShareBookId,
                    Budget = plan.Budget
                });

                planAudience.DeliveryImpressions = planDeliveryBudget.DeliveryImpressions;
                planAudience.DeliveryRatingPoints = planDeliveryBudget.DeliveryRatingPoints;
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
    }
}
