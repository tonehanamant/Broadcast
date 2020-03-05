using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.CampaignExport;
using Services.Broadcast.ReportGenerators.ProgramLineup;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    /// <summary>
    /// Operations related to the Campaign domain.
    /// </summary>
    /// <seealso cref="IApplicationService" />
    public interface ICampaignService : IApplicationService
    {
        /// <summary>
        /// Gets all campaigns.
        /// </summary>
        /// <returns></returns>
        List<CampaignListItemDto> GetCampaigns(CampaignFilterDto filter, DateTime currentDate);

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        CampaignDto GetCampaignById(int campaignId);

        /// <summary>
        /// Saves the campaign.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the new campaign</returns>
        int SaveCampaign(SaveCampaignDto campaign, string userName, DateTime createdDate);

        /// <summary>
        /// Gets the quarters.
        /// <param name="planStatus">The status to filter quarter by</param>
        /// <param name="currentDate">The date for the default quarter.</param>
        /// <returns></returns>
        CampaignQuartersDto GetQuarters(PlanStatusEnum? planStatus, DateTime currentDate);

        /// <summary>
        /// Gets the statuses based on the quarter.
        /// </summary>
        /// <param name="quarter">The quarter</param>
        /// <param name="year">The year</param>
        /// <returns></returns>
        List<LookupDto> GetStatuses(int? quarter, int? year);

        /// <summary>
        /// Triggers the campaign aggregation job.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="queuedBy">The queued by.</param>
        string TriggerCampaignAggregationJob(int campaignId, string queuedBy);

        /// <summary>
        /// Processes the campaign aggregation.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        [Queue("campaignaggregation")]
        [DisableConcurrentExecution(300)]
        void ProcessCampaignAggregation(int campaignId);

        /// <summary>
        /// Gets the campaign defaults.
        /// </summary>
        /// <returns>Gets the default values for creating a campaign in the form of <see cref="CampaignDefaultsDto"/></returns>
        CampaignDefaultsDto GetCampaignDefaults();

        /// <summary>
        /// Generates the campaign report.
        /// </summary>
        /// <param name="request">CampaignReportRequest object containing the campaign id and the selected plans id</param>
        /// <param name="userName"></param>
        /// <param name="currentDate"></param>
        /// <param name="templatesFilePath">Path to the template files</param>
        /// <returns>Campaign report identifier</returns>
        Guid GenerateCampaignReport(CampaignReportRequest request, string userName, DateTime currentDate, string templatesFilePath);

        /// <summary>
        /// Gets the campaign report data. Method used for testing purposes
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>CampaignReport object</returns>
        CampaignReportData GetAndValidateCampaignReportData(CampaignReportRequest request);

        /// <summary>
        /// Generates the program lineup report.
        /// </summary>
        /// <param name="request">ProgramLineupReportRequest object contains selected plan ids</param>
        /// <param name="userName"></param>
        /// <param name="currentDate"></param>
        /// <param name="templatesFilePath">Path to the template files</param>
        /// <returns>The report id</returns>
        Guid GenerateProgramLineupReport(ProgramLineupReportRequest request, string userName, DateTime currentDate, string templatesFilePath);

        /// <summary>
        /// Gets the program lineup report data. Method used for testing purposes
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="currentDate"></param>
        /// <returns>ProgramLineupReportData object</returns>
        ProgramLineupReportData GetProgramLineupReportData(ProgramLineupReportRequest request, DateTime currentDate);
    }

    /// <summary>
    /// Operations related to the Campaign domain.
    /// </summary>
    /// <seealso cref="ICampaignService" />
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignValidator _CampaignValidator;
        private readonly ICampaignRepository _CampaignRepository;
        private readonly IPlanRepository _PlanRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBroadcastLockingManagerApplicationService _LockingManagerApplicationService;
        private readonly ICampaignAggregator _CampaignAggregator;
        private readonly ICampaignSummaryRepository _CampaignSummaryRepository;
        private readonly ICampaignAggregationJobTrigger _CampaignAggregationJobTrigger;
        private readonly ITrafficApiCache _TrafficApiCache;
        private readonly IAudienceService _AudienceService;
        private readonly ISpotLengthService _SpotLengthService;
        private readonly IDaypartDefaultService _DaypartDefaultService;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IMarketCoverageRepository _MarketCoverageRepository;
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IStandartDaypartEngine _StandartDaypartEngine;

        public CampaignService(
            IDataRepositoryFactory dataRepositoryFactory,
            ICampaignValidator campaignValidator,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            IBroadcastLockingManagerApplicationService lockingManagerApplicationService,
            ICampaignAggregator campaignAggregator,
            ICampaignAggregationJobTrigger campaignAggregationJobTrigger,
            ITrafficApiCache trafficApiCache,
            IAudienceService audienceService,
            ISpotLengthService spotLengthService,
            IDaypartDefaultService daypartDefaultService,
            ISharedFolderService sharedFolderService,
            IStandartDaypartEngine standartDaypartEngine)
        {
            _CampaignRepository = dataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _CampaignValidator = campaignValidator;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _CampaignAggregator = campaignAggregator;
            _CampaignSummaryRepository = dataRepositoryFactory.GetDataRepository<ICampaignSummaryRepository>();
            _CampaignAggregationJobTrigger = campaignAggregationJobTrigger;
            _TrafficApiCache = trafficApiCache;
            _PlanRepository = dataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _AudienceService = audienceService;
            _SpotLengthService = spotLengthService;
            _DaypartDefaultService = daypartDefaultService;
            _SharedFolderService = sharedFolderService;
            _InventoryRepository = dataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _MarketCoverageRepository = dataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _StationProgramRepository = dataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _StandartDaypartEngine = standartDaypartEngine;
        }

        /// <inheritdoc />
        public List<CampaignListItemDto> GetCampaigns(CampaignFilterDto filter, DateTime currentDate)
        {
            if (!_IsFilterValid(filter))
                filter = _GetDefaultFilter(currentDate);

            var quarterDateRange = _GetQuarterDateRange(filter.Quarter);
            var campaigns = _CampaignRepository.GetCampaignsWithSummary(quarterDateRange.Start, quarterDateRange.End, filter.PlanStatus)
                .Select(x => _MapToCampaignListItemDto(x)).ToList();
            var cacheAdvertisers = new BaseMemoryCache<List<AdvertiserDto>>("localAdvertisersCache");

            foreach (var campaign in campaigns)
            {
                if (!campaign.CampaignStatus.HasValue)
                {
                    campaign.CampaignStatus = PlanStatusEnum.Working;
                }

                campaign.Agency = _TrafficApiCache.GetAgency(campaign.Agency.Id);
                campaign.Advertiser = _TrafficApiCache.GetAdvertiser(campaign.Advertiser.Id);
            }

            return campaigns;
        }

        private DateRange _GetQuarterDateRange(QuarterDto quarter)
        {
            if (quarter == null)
                return new DateRange(null, null);
            return _QuarterCalculationEngine.GetQuarterDateRange(quarter.Quarter, quarter.Year);
        }

        /// <inheritdoc />
        public CampaignDto GetCampaignById(int campaignId)
        {
            var campaign = _CampaignRepository.GetCampaign(campaignId);
            var summary = _CampaignSummaryRepository.GetSummaryForCampaign(campaignId);

            _HydrateCampaignWithSummary(campaign, summary);

            return campaign;
        }

        private void _HydrateCampaignWithSummary(CampaignDto campaign, CampaignSummaryDto summary)
        {
            if (summary == null)
            {
                return;
            }

            campaign.FlightStartDate = summary.FlightStartDate;
            campaign.FlightEndDate = summary.FlightEndDate;
            campaign.FlightHiatusDays = summary.FlightHiatusDays;
            campaign.FlightActiveDays = summary.FlightActiveDays;
            campaign.HasHiatus = campaign.FlightHiatusDays > 0;

            campaign.Budget = summary.Budget;
            campaign.HHCPM = summary.HHCPM;
            campaign.HHImpressions = summary.HHImpressions;
            campaign.HHRatingPoints = summary.HHRatingPoints;

            campaign.CampaignStatus = summary.CampaignStatus;
            campaign.PlanStatuses = _MapToPlanStatuses(summary);
        }

        private CampaignListItemDto _MapToCampaignListItemDto(CampaignWithSummary campaignAndCampaignSummary)
        {
            var campaign = new CampaignListItemDto
            {
                Id = campaignAndCampaignSummary.Campaign.Id,
                Name = campaignAndCampaignSummary.Campaign.Name,
                Advertiser = new AdvertiserDto { Id = campaignAndCampaignSummary.Campaign.AdvertiserId },
                Agency = new AgencyDto { Id = campaignAndCampaignSummary.Campaign.AgencyId },
                Notes = campaignAndCampaignSummary.Campaign.Notes,
                ModifiedDate = campaignAndCampaignSummary.Campaign.ModifiedDate,
                ModifiedBy = campaignAndCampaignSummary.Campaign.ModifiedBy,

                HasPlans = campaignAndCampaignSummary.Campaign.Plans != null &&
                    campaignAndCampaignSummary.Campaign.Plans.Any(),
                Plans = campaignAndCampaignSummary.Campaign.Plans,
            };
            campaign.Plans?.ForEach(plan => plan.HasHiatus = plan.TotalHiatusDays.HasValue && plan.TotalHiatusDays.Value > 0);

            if (campaignAndCampaignSummary.CampaignSummary != null)
            {
                campaign.FlightStartDate = campaignAndCampaignSummary.CampaignSummary.FlightStartDate;
                campaign.FlightEndDate = campaignAndCampaignSummary.CampaignSummary.FlightEndDate;
                campaign.FlightHiatusDays = campaignAndCampaignSummary.CampaignSummary.FlightHiatusDays;
                campaign.FlightActiveDays = campaignAndCampaignSummary.CampaignSummary.FlightActiveDays;
                campaign.HasHiatus =
                    campaignAndCampaignSummary.CampaignSummary.FlightHiatusDays.HasValue &&
                    campaignAndCampaignSummary.CampaignSummary.FlightHiatusDays.Value > 0;

                campaign.Budget = campaignAndCampaignSummary.CampaignSummary.Budget;
                campaign.HHCPM = campaignAndCampaignSummary.CampaignSummary.HHCPM;
                campaign.HHImpressions = campaignAndCampaignSummary.CampaignSummary.HHImpressions;
                campaign.HHRatingPoints = campaignAndCampaignSummary.CampaignSummary.HHRatingPoints;
                campaign.CampaignStatus = campaignAndCampaignSummary.CampaignSummary.CampaignStatus;

                campaign.PlanStatuses = _MapToPlanStatuses(campaignAndCampaignSummary.CampaignSummary);
            }

            return campaign;
        }

        private List<PlansStatusCountDto> _MapToPlanStatuses(CampaignSummaryDto summary)
        {
            var statuses = new List<PlansStatusCountDto>();
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Working, summary.PlanStatusCountWorking);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Reserved, summary.PlanStatusCountReserved);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.ClientApproval, summary.PlanStatusCountClientApproval);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Contracted, summary.PlanStatusCountContracted);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Live, summary.PlanStatusCountLive);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Complete, summary.PlanStatusCountComplete);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Scenario, summary.PlanStatusCountScenario);
            return statuses;
        }

        private void _EvaluateAndAddPlanStatus(List<PlansStatusCountDto> planStatuses, PlanStatusEnum status, int? candidate)
        {
            if (candidate > 0)
            {
                planStatuses.Add(new PlansStatusCountDto { PlanStatus = status, Count = candidate.Value });
            }
        }

        /// <inheritdoc />
        public int SaveCampaign(SaveCampaignDto campaign, string createdBy, DateTime createdDate)
        {
            _CampaignValidator.Validate(campaign);

            campaign.ModifiedBy = createdBy;
            campaign.ModifiedDate = createdDate;

            if (campaign.Id == 0)
            {
                return _CampaignRepository.CreateCampaign(campaign, createdBy, createdDate);
            }
            else
            {
                //if (SafeBroadcastServiceSystemParameter.EnableCampaignsLocking)
                if (true)
                {
                    var key = KeyHelper.GetCampaignLockingKey(campaign.Id);
                    var lockingResult = _LockingManagerApplicationService.LockObject(key);

                    if (lockingResult.Success)
                    {
                        return _CampaignRepository.UpdateCampaign(campaign);
                    }
                    else
                    {
                        throw new ApplicationException($"The chosen campaign has been locked by {lockingResult.LockedUserName}");
                    }
                }
                else
                {
                    return _CampaignRepository.UpdateCampaign(campaign);
                }
            }
        }

        /// <inheritdoc />
        public CampaignQuartersDto GetQuarters(PlanStatusEnum? planStatus, DateTime currentDate)
        {
            var dateRanges = _CampaignRepository.GetCampaignsDateRanges(planStatus);
            var quarters = _QuarterCalculationEngine.GetQuartersForDateRanges(dateRanges);

            var currentQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentDate);

            if (!quarters.Any(x => x.Quarter == currentQuarter.Quarter &&
                                   x.Year == currentQuarter.Year))
                quarters.Add(currentQuarter);

            return new CampaignQuartersDto
            {
                DefaultQuarter = currentQuarter,
                Quarters = quarters.OrderByDescending(x => x.Year).ThenByDescending(x => x.Quarter).ToList()
            };
        }

        /// <inheritdoc />
        public List<LookupDto> GetStatuses(int? quarter, int? year)
        {
            QuarterDto quarterDto = null;

            if (quarter.HasValue && year.HasValue)
            {
                quarterDto = new QuarterDto
                {
                    Quarter = quarter.Value,
                    Year = year.Value
                };
            }

            var dateRange = _GetQuarterDateRange(quarterDto);

            var statuses = _CampaignRepository.GetCampaignsStatuses(dateRange.Start, dateRange.End);

            return statuses.Select(x => new LookupDto { Id = (int)x, Display = x.Description() })
                .OrderByDescending(x => x.Id == (int)PlanStatusEnum.Scenario)
                .ThenBy(x => x.Id).ToList();
        }

        /// <inheritdoc />
        public string TriggerCampaignAggregationJob(int campaignId, string queuedBy)
        {
            return _CampaignAggregationJobTrigger.TriggerJob(campaignId, queuedBy);
        }

        /// <summary>
        /// Processes the campaign aggregation.
        /// </summary>
        /// <remarks>
        /// Called by an external process.
        /// Let the exceptions bubble out so they are recorded by the external processor.
        /// </remarks>
        /// <param name="campaignId">The campaign identifier.</param>
        public void ProcessCampaignAggregation(int campaignId)
        {
            try
            {
                var summary = _CampaignAggregator.Aggregate(campaignId);
                summary.ProcessingStatus = CampaignAggregationProcessingStatusEnum.Completed;
                summary.LastAggregated = DateTime.Now;
                summary.ProcessingErrorMessage = null;
                _CampaignSummaryRepository.SaveSummary(summary);
            }
            catch (Exception e)
            {
                _CampaignSummaryRepository.SetSummaryProcessingStatusToError(campaignId, $"Exception caught during processing : {e.Message}");
                // re-throw so that the caller can track the failure.
                throw;
            }
        }

        private List<DateRange> _ValidateDateRanges(List<DateRange> dateRanges)
        {
            var nonEmptyRanges = dateRanges.Where(x => !x.IsEmpty());
            var validStartDate = nonEmptyRanges.Where(x => x.Start != null);
            var hasEndDate = validStartDate.Where(x => x.End != null);
            var missingEndDate = validStartDate.Where(x => x.End == null);

            foreach (var dateRange in missingEndDate)
                dateRange.End = dateRange.Start;

            var allValidDateRanges = hasEndDate.Concat(missingEndDate).ToList();

            return allValidDateRanges;
        }

        private bool _IsFilterValid(CampaignFilterDto filter)
        {
            return filter != null && (filter.Quarter != null || filter.PlanStatus != null);
        }

        private CampaignFilterDto _GetDefaultFilter(DateTime currentDate)
        {
            var quarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentDate);

            return new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Quarter = quarter.Quarter,
                    Year = quarter.Year
                },
                PlanStatus = null
            };
        }

        /// <inheritdoc />
        public CampaignDefaultsDto GetCampaignDefaults()
        {
            // return a blank object now, until requirements get updated
            var campaignDefaultValues = new CampaignDefaultsDto
            {
                Name = string.Empty,
                AgencyId = null,
                AdvertiserId = null,
                Notes = string.Empty
            };

            return campaignDefaultValues;
        }

        /// <inheritdoc/>
        public Guid GenerateCampaignReport(CampaignReportRequest request, string userName, DateTime currentDate, string templatesFilePath)
        {
            var campaignReportData = GetAndValidateCampaignReportData(request);
            var reportGenerator = new CampaignReportGenerator(templatesFilePath);
            var report = reportGenerator.Generate(campaignReportData);

            return _SharedFolderService.SaveFile(new SharedFolderFile
            {
                FolderPath = $@"{BroadcastServiceSystemParameter.BroadcastSharedFolder}\{BroadcastServiceSystemParameter.CampaignExportReportsFolder}",
                FileNameWithExtension = report.Filename,
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.CampaignExport,
                CreatedDate = currentDate,
                CreatedBy = userName,
                FileContent = report.Stream
            });
        }

        /// <inheritdoc/>
        public CampaignReportData GetAndValidateCampaignReportData(CampaignReportRequest request)
        {
            var campaign = _CampaignRepository.GetCampaign(request.CampaignId);
            
            if (!request.SelectedPlans.IsEmpty())
            {
                campaign.Plans = campaign.Plans.Where(x => request.SelectedPlans.Contains(x.PlanId)).ToList();
            }
            
            _ValidateCampaignLocking(campaign.Id);
            _ValidateSelectedPlans(request.ExportType, campaign.Plans);

            AgencyDto agency = _TrafficApiCache.GetAgency(campaign.AgencyId);
            AdvertiserDto advertiser = _TrafficApiCache.GetAdvertiser(campaign.AdvertiserId);
            var plans = campaign.Plans
                .Select(x =>
                {
                    var plan = _PlanRepository.GetPlan(x.PlanId);
                    DaypartTimeHelper.AddOneSecondToEndTime(plan.Dayparts);
                    return plan;
                }).ToList();
            _ValidateGuaranteedAudiences(plans);
            _ValidateSecondaryAudiences(plans);
            List<PlanAudienceDisplay> guaranteedDemos = plans.Select(x => x.AudienceId).Distinct()
                .Select(x => _AudienceService.GetAudienceById(x)).ToList();

            return new CampaignReportData(request.ExportType, campaign, plans, agency, advertiser, guaranteedDemos,
                _SpotLengthService.GetAllSpotLengths(),
                _DaypartDefaultService.GetAllDaypartDefaults(),
                 _AudienceService.GetAudiences(),
                 _MediaMonthAndWeekAggregateCache,
                _QuarterCalculationEngine);
        }

        private void _ValidateCampaignLocking(int campaignId)
        {
            const string CAMPAIGN_IS_LOCKED_EXCEPTION = "Campaign with id {0} has been locked by {1}";

            var campaignLockingKey = KeyHelper.GetCampaignLockingKey(campaignId);
            var lockObject = _LockingManagerApplicationService.GetLockObject(campaignLockingKey);

            if (!lockObject.Success)
            {
                var message = string.Format(CAMPAIGN_IS_LOCKED_EXCEPTION, campaignId, lockObject.LockedUserName);
                throw new ApplicationException(message);
            }
        }

        private void _ValidateSelectedPlans(CampaignExportTypeEnum exportType, List<PlanSummaryDto> plans)
        {
            _ValidatePostingType(plans);
            _ValidateExportType(exportType, plans);
            
            foreach (var plan in plans)
            {
                _ValidatePlanLocking(plan.PlanId);
                _ValidatePlanAggregationStatus(plan);
            }
        }

        private void _ValidatePlanLocking(int planId)
        {
            const string PLAN_IS_LOCKED_EXCEPTION = "Plan with id {0} has been locked by {1}";

            var planLockingKey = KeyHelper.GetPlanLockingKey(planId);
            var lockObject = _LockingManagerApplicationService.GetLockObject(planLockingKey);

            if (!lockObject.Success)
            {
                var message = string.Format(PLAN_IS_LOCKED_EXCEPTION, planId, lockObject.LockedUserName);
                throw new ApplicationException(message);
            }
        }

        private void _ValidatePlanAggregationStatus(PlanSummaryDto plan)
        {
            const string PLAN_AGGREGATION_IS_IN_PROGRESS_EXCEPTION = "Data aggregation for the plan with id {0} is in progress. Please wait until the process is done";
            const string PLAN_AGGREGATION_FAILED_EXCEPTION = "Data aggregation for the plan with id {0} has failed. Please contact the support";

            var processingStatus = plan.ProcessingStatus;

            if (processingStatus == PlanAggregationProcessingStatusEnum.InProgress)
            {
                var message = string.Format(PLAN_AGGREGATION_IS_IN_PROGRESS_EXCEPTION, plan.PlanId);
                throw new ApplicationException(message);
            }

            if (processingStatus == PlanAggregationProcessingStatusEnum.Error)
            {
                var message = string.Format(PLAN_AGGREGATION_FAILED_EXCEPTION, plan.PlanId);
                throw new ApplicationException(message);
            }
        }

        private static void _ValidatePostingType(List<PlanSummaryDto> plans)
        {
            const string MULTIPLE_POSTING_TYPES_EXCEPTION = "Cannot have multiple posting types in the export. Please select only plans with the same posting type.";

            if (plans.Select(x => x.PostingType).Distinct().Count() != 1)
            {
                throw new ApplicationException(MULTIPLE_POSTING_TYPES_EXCEPTION);
            }
        }

        private static void _ValidateGuaranteedAudiences(List<PlanDto> plans)
        {
            const string MULTIPLE_GUARANTEED_AUDIENCES_EXCEPTION = "Cannot have multiple guaranteed audiences in the export. Please select only plans with the same guaranteed audience.";

            if (plans.Select(x => x.AudienceId).Distinct().Count() != 1)
            {
                throw new ApplicationException(MULTIPLE_GUARANTEED_AUDIENCES_EXCEPTION);
            }
        }

        private static void _ValidateSecondaryAudiences(List<PlanDto> plans)
        {
            const string SECONDARY_AUDIENCES_EXCEPTION = "Cannot have multiple plans with varying secondary audiences in the export. Please select only plans with the same secondary audiences.";
            if (plans.Count == 1)
                return;

            var firstPlanSecondaryAudiencesIds = plans.First().SecondaryAudiences.Select(x => x.AudienceId).ToList();
            foreach(var plan in plans.Skip(1))
            {
                if (plan.SecondaryAudiences.Count != firstPlanSecondaryAudiencesIds.Count ||
                    plan.SecondaryAudiences.Any(x => !firstPlanSecondaryAudiencesIds.Contains(x.AudienceId)))
                {
                    throw new ApplicationException(SECONDARY_AUDIENCES_EXCEPTION);
                }
            }
        }

        private static void _ValidateExportType(CampaignExportTypeEnum exportType, List<PlanSummaryDto> plans)
        {
            const string INVALID_EXPORT_TYPE_FOR_SELECTED_PLANS = "Invalid export type for selected plans.";
            List<PlanStatusEnum> distinctPlansStatuses = plans.Select(x => x.Status).Distinct().ToList();
            if (distinctPlansStatuses.Contains(PlanStatusEnum.Scenario)
                || distinctPlansStatuses.Contains(PlanStatusEnum.Rejected)
                || distinctPlansStatuses.Contains(PlanStatusEnum.Canceled))
            {
                if (!exportType.Equals(CampaignExportTypeEnum.Proposal))
                {
                    throw new ApplicationException(INVALID_EXPORT_TYPE_FOR_SELECTED_PLANS);
                }
            }
            else if (distinctPlansStatuses.Contains(PlanStatusEnum.Complete)
                || distinctPlansStatuses.Contains(PlanStatusEnum.Contracted)
                || distinctPlansStatuses.Contains(PlanStatusEnum.Live))
            {
                if (!exportType.Equals(CampaignExportTypeEnum.Contract))
                {
                    throw new ApplicationException(INVALID_EXPORT_TYPE_FOR_SELECTED_PLANS);
                }
            }
            else
            {
                if (!exportType.Equals(CampaignExportTypeEnum.Proposal))
                {
                    throw new ApplicationException(INVALID_EXPORT_TYPE_FOR_SELECTED_PLANS);
                }
            }
        }
        
        public Guid GenerateProgramLineupReport(ProgramLineupReportRequest request, string userName, DateTime currentDate, string templatesFilePath)
        {
            var programLineupReportData = GetProgramLineupReportData(request, currentDate);
            var reportGenerator = new ProgramLineupReportGenerator(templatesFilePath);
            var report = reportGenerator.Generate(programLineupReportData);

            return _SharedFolderService.SaveFile(new SharedFolderFile
            {
                FolderPath = $@"{BroadcastServiceSystemParameter.BroadcastSharedFolder}\{BroadcastServiceSystemParameter.ProgramLineupReportsFolder}",
                FileNameWithExtension = report.Filename,
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.ProgramLineup,
                CreatedDate = currentDate,
                CreatedBy = userName,
                FileContent = report.Stream
            });
        }
        
        public ProgramLineupReportData GetProgramLineupReportData(ProgramLineupReportRequest request, DateTime currentDate)
        {
            if (request.SelectedPlans.IsEmpty())
                throw new ApplicationException("Choose at least one plan");

            // for now we generate reports only for one plan
            var planId = request.SelectedPlans.First();
            var plan = _PlanRepository.GetPlan(planId);
            var campaign = _CampaignRepository.GetCampaign(plan.CampaignId);
            var pricingJob = _GetLatestPricingJob(planId);
            var agency = _TrafficApiCache.GetAgency(campaign.AgencyId);
            var advertiser = _TrafficApiCache.GetAdvertiser(campaign.AdvertiserId);
            var guaranteedDemo = _AudienceService.GetAudienceById(plan.AudienceId);
            var spotLengths = _SpotLengthService.GetAllSpotLengths();
            var allocatedSpots = _PlanRepository.GetPlanPricingAllocatedSpots(planId);
            var manifestIds = allocatedSpots.Select(x => x.StationInventoryManifestId).Distinct();
            var manifests = _InventoryRepository.GetStationInventoryManifestsByIds(manifestIds)
                .Where(x => x.Station != null && x.Station.MarketCode.HasValue)
                .ToList();
            var marketCoverages = _MarketCoverageRepository.GetLatestMarketCoveragesWithStations();
            var manifestDaypartIds = manifests.SelectMany(x => x.ManifestDayparts).Select(x => x.Id.Value);
            var primaryProgramsByManifestDaypartIds = _StationProgramRepository.GetPrimaryProgramsForManifestDayparts(manifestDaypartIds);
            
            return new ProgramLineupReportData(
                plan, 
                pricingJob, 
                agency, 
                advertiser, 
                guaranteedDemo, 
                spotLengths, 
                currentDate,
                allocatedSpots,
                manifests,
                marketCoverages,
                primaryProgramsByManifestDaypartIds,
                _StandartDaypartEngine);
        }

        private PlanPricingJob _GetLatestPricingJob(int planId)
        {
            var job = _PlanRepository.GetLatestPricingJob(planId);

            if (job == null)
                throw new ApplicationException("There are no completed pricing runs for the chosen plan. Please run pricing");

            if (job.Status == BackgroundJobProcessingStatus.Failed)
                throw new ApplicationException("The latest pricing run was failed. Please run pricing again or contact the support");

            if (job.Status == BackgroundJobProcessingStatus.Queued || job.Status == BackgroundJobProcessingStatus.Processing)
                throw new ApplicationException("There is a pricing run in progress right now. Please wait until it is completed");

            return job;
        }
    }
}
