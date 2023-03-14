using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.CampaignExport;
using Services.Broadcast.ReportGenerators.ProgramLineup;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;

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
        /// </summary>
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
        /// <param name="templatesFilePath">Path to the template files</param>
        /// <returns>Campaign report identifier</returns>
        Guid GenerateCampaignReport(CampaignReportRequest request, string userName, string templatesFilePath);

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

        /// <summary>
        /// Locks the Campaign.
        /// </summary>
        /// <param name="campaignId">The Campaign identifier.</param>
        /// <returns></returns>
        BroadcastLockResponse LockCampaigns(int campaignId);

        /// <summary>
        /// Unlocks the Campaign.
        /// </summary>
        /// <param name="campaignId">The Campaign identifier.</param>
        /// <returns></returns>
        BroadcastReleaseLockResponse UnlockCampaigns(int campaignId);

        /// <summary>
        /// Gets the campaign Copy.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        CampaignCopyDto GetCampaignCopy(int campaignId);

        /// <summary>
        /// Copy the campaign along with plans.
        /// </summary>
        /// <param name="campaignCopy">The campaign object.</param>
        /// <param name="createdBy">Name of the user.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the new campaign</returns>
        int SaveCampaignCopy(SaveCampaignCopyDto campaignCopy, string createdBy, DateTime createdDate);
        /// <summary>
        /// Get the campaign along with plans excluded draft
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>campaign with plans</returns>
        CampaignExportDto CampaignExportAvailablePlans(int campaignId);
        /// <summary>
        /// Send message  about any update in the unified campaign
        /// </summary>
        /// <param name="campaignId">campaignId
        /// </param>
        /// <param name="updateddBy">Name of the user who last modified campaign</param>
        /// <param name="updatedDate">Last modified date of campaign</param>
        /// <returns>Message response</returns>
        Task<UnifiedCampaignResponseDto> PublishUnifiedCampaign(int campaignId, string updateddBy, DateTime updatedDate);
        /// <summary>
        /// Get The Campaign with Defaults
        /// </summary>
        /// <param name="campaignId">Campaign Id</param>
        /// <returns>Campaign With Defaults</returns>
        CampaignWithDefaultsDto GetCampaignWithDefaults(int campaignId);
    }
     
    /// <summary>
    /// Operations related to the Campaign domain.
    /// </summary>
    /// <seealso cref="ICampaignService" />
    public class CampaignService : BroadcastBaseClass, ICampaignService
    {
        private readonly ICampaignValidator _CampaignValidator;
        private readonly ICampaignRepository _CampaignRepository;
        private readonly IPlanRepository _PlanRepository;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBroadcastLockingManagerApplicationService _LockingManagerApplicationService;
        private readonly ICampaignAggregator _CampaignAggregator;
        private readonly ICampaignSummaryRepository _CampaignSummaryRepository;
        private readonly ICampaignAggregationJobTrigger _CampaignAggregationJobTrigger;
        private readonly IAudienceService _AudienceService;
        private readonly IStandardDaypartService _StandardDaypartService;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IMarketCoverageRepository _MarketCoverageRepository;
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IWeeklyBreakdownEngine _WeeklyBreakdownEngine;
        private readonly IDaypartCache _DaypartCache;
        private readonly IAabEngine _AabEngine;
        private readonly ILockingEngine _LockingEngine;
        private readonly IPlanService _PlanService;
        private readonly IPlanValidator _PlanValidator;
        private readonly ICampaignServiceApiClient _CampaignServiceApiClient;
        protected Lazy<bool> _IsUnifiedCampaignEnabled;
        protected Lazy<bool> _IsProgramLineupAllocationByAffiliateEnabled;

        public CampaignService(
            IDataRepositoryFactory dataRepositoryFactory,
            ICampaignValidator campaignValidator,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            IBroadcastLockingManagerApplicationService lockingManagerApplicationService,
            ICampaignAggregator campaignAggregator,
            ICampaignAggregationJobTrigger campaignAggregationJobTrigger,
            IAudienceService audienceService,
            IStandardDaypartService standardDaypartService,
            ISharedFolderService sharedFolderService,
            IDateTimeEngine _dateTimeEngine,
            IWeeklyBreakdownEngine weeklyBreakdownEngine,
            IDaypartCache daypartCache,
            IFeatureToggleHelper featureToggleHelper,
            IAabEngine aabEngine, IConfigurationSettingsHelper configurationSettingsHelper, ILockingEngine lockingEngine, IPlanService PlanService, IPlanValidator PlanValidator, ICampaignServiceApiClient CampaignServiceApiClient) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _CampaignRepository = dataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _CampaignValidator = campaignValidator;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _CampaignAggregator = campaignAggregator;
            _CampaignSummaryRepository = dataRepositoryFactory.GetDataRepository<ICampaignSummaryRepository>();
            _CampaignAggregationJobTrigger = campaignAggregationJobTrigger;
            _PlanRepository = dataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _AudienceService = audienceService;
            _SpotLengthRepository = dataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _StandardDaypartService = standardDaypartService;
            _SharedFolderService = sharedFolderService;
            _InventoryRepository = dataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _MarketCoverageRepository = dataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _StationProgramRepository = dataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _DateTimeEngine = _dateTimeEngine;
            _WeeklyBreakdownEngine = weeklyBreakdownEngine;
            _DaypartCache = daypartCache;
            _AabEngine = aabEngine;
            _LockingEngine = lockingEngine;
            _PlanService = PlanService;
            _PlanValidator = PlanValidator;
            _CampaignServiceApiClient = CampaignServiceApiClient;
            _IsUnifiedCampaignEnabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_UNIFIED_CAMPAIGN));
            _IsProgramLineupAllocationByAffiliateEnabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PROGRAM_LINEUP_ALLOCATION_BY_AFFILIATE));
        }


        /// <inheritdoc />
        public List<CampaignListItemDto> GetCampaigns(CampaignFilterDto filter, DateTime currentDate)
        {
            if (!_IsFilterValid(filter))
                filter = _GetDefaultFilter(currentDate);

            var quarterDateRange = _GetQuarterDateRange(filter.Quarter);
            var campaigns = _CampaignRepository.GetCampaignsWithSummary(quarterDateRange.Start, quarterDateRange.End, filter.PlanStatus)
                .Select(x => _MapToCampaignListItemDto(x)).ToList();
            foreach (var campaign in campaigns)
            {
                if (!campaign.CampaignStatus.HasValue)
                {
                    campaign.CampaignStatus = PlanStatusEnum.Working;
                }

                campaign.Agency = _GetAgency(campaign);
                campaign.Advertiser = _GetAdvertiser(campaign);
            }
            if (!_IsUnifiedCampaignEnabled.Value)
            {
                var FilterOutUnifiedCampaigns = campaigns.Where(x => x.UnifiedId == null).ToList();
                return FilterOutUnifiedCampaigns;
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
            if (!_IsUnifiedCampaignEnabled.Value)
            {
                if (campaign.UnifiedId != null)
                {
                    throw new CadentException($"Could not find existing campaign with id {campaign.Id}");
                }
            }
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
                MaxFluidityPercent = campaignAndCampaignSummary.Campaign.MaxFluidityPercent,
                UnifiedId = campaignAndCampaignSummary.Campaign.UnifiedId,
                UnifiedCampaignLastSentAt = campaignAndCampaignSummary.Campaign.UnifiedCampaignLastSentAt,
                UnifiedCampaignLastReceivedAt = campaignAndCampaignSummary.Campaign.UnifiedCampaignLastReceivedAt,
                Advertiser = new AdvertiserDto
                {
                    Id = campaignAndCampaignSummary.Campaign.AdvertiserId,
                    MasterId = campaignAndCampaignSummary.Campaign.AdvertiserMasterId
                },
                Agency = new AgencyDto
                {
                    Id = campaignAndCampaignSummary.Campaign.AgencyId,
                    MasterId = campaignAndCampaignSummary.Campaign.AgencyMasterId
                },
                Notes = campaignAndCampaignSummary.Campaign.Notes,
                ModifiedDate = campaignAndCampaignSummary.Campaign.ModifiedDate,
                ModifiedBy = campaignAndCampaignSummary.Campaign.ModifiedBy,

                HasPlans = campaignAndCampaignSummary.Campaign.Plans != null &&
                    campaignAndCampaignSummary.Campaign.Plans.Any(),
                Plans = campaignAndCampaignSummary.Campaign.Plans,
            };
            campaign.Plans?.ForEach(plan => plan.HasHiatus = plan.TotalHiatusDays.HasValue && plan.TotalHiatusDays.Value > 0);
            List<PlanSummaryDto> filteredPlans = new List<PlanSummaryDto>();
            bool isCampaignHasDraft = false;
            if (campaign.Plans != null && campaign.HasPlans)
            {
                isCampaignHasDraft = campaign.Plans.Where(x => x.IsDraft == true).Count() > 0 ? true : false;
                campaign.Plans.RemoveAll(x => x.IsDraft == true);
                foreach (var plan in campaign.Plans)
                {
                    if ((!filteredPlans.Any(x => x.PlanId == plan.PlanId)))
                    {
                        var latestplan = campaign.Plans.Where(x => x.PlanId == plan.PlanId && (x.IsDraft == false || x.IsDraft == null))
                               .OrderByDescending(p => p.VersionId).FirstOrDefault();
                        if (latestplan != null)
                        {
                            filteredPlans.Add(latestplan);
                        }
                    }
                }
                campaign.Plans = filteredPlans;
            }

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
                if (isCampaignHasDraft)
                {
                    campaign.PlanStatuses = _MapToPlanStatusesWithDraftExluded(campaign.Plans);
                }
                else
                {
                    campaign.PlanStatuses = _MapToPlanStatuses(campaignAndCampaignSummary.CampaignSummary);
                }
            }
            return campaign;
        }

        private List<PlansStatusCountDto> _MapToPlanStatusesWithDraftExluded(List<PlanSummaryDto> plans)
        {
            var statuses = new List<PlansStatusCountDto>();
            _EvaluateAndAddPlanStatusWithDraftExluded(statuses, PlanStatusEnum.Working, plans);
            _EvaluateAndAddPlanStatusWithDraftExluded(statuses, PlanStatusEnum.Reserved, plans);
            _EvaluateAndAddPlanStatusWithDraftExluded(statuses, PlanStatusEnum.ClientApproval, plans);
            _EvaluateAndAddPlanStatusWithDraftExluded(statuses, PlanStatusEnum.Contracted, plans);
            _EvaluateAndAddPlanStatusWithDraftExluded(statuses, PlanStatusEnum.Live, plans);
            _EvaluateAndAddPlanStatusWithDraftExluded(statuses, PlanStatusEnum.Complete, plans);
            _EvaluateAndAddPlanStatusWithDraftExluded(statuses, PlanStatusEnum.Scenario, plans);
            return statuses;
        }
        private void _EvaluateAndAddPlanStatusWithDraftExluded(List<PlansStatusCountDto> planStatuses, PlanStatusEnum status, List<PlanSummaryDto> plans)
        {
            if (plans.Any(x => x.Status == status))
            {
                planStatuses.Add(new PlansStatusCountDto { PlanStatus = status, Count = plans.Count(x => x.Status == status) });
            }
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
                var key = KeyHelper.GetCampaignLockingKey(campaign.Id);
                var lockingResult = _LockingManagerApplicationService.LockObject(key);

                if (lockingResult.Success)
                {
                    return _CampaignRepository.UpdateCampaign(campaign);
                }
                else
                {
                    throw new CadentException($"The chosen campaign has been locked by {lockingResult.LockedUserName}");
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
        public Guid GenerateCampaignReport(CampaignReportRequest request, string userName, string templatesFilePath)
        {
            _LogInfo($"Gathering the report data...");
            var campaignReportData = GetAndValidateCampaignReportData(request);
            var reportGenerator = new CampaignReportGenerator(templatesFilePath, _FeatureToggleHelper, _ConfigurationSettingsHelper);
            _LogInfo($"Preparing to generate the file.  templatesFilePath='{templatesFilePath}'");
            var report = reportGenerator.Generate(campaignReportData);

            var folderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.CAMPAIGN_EXPORT_REPORTS);

            _LogInfo($"Saving generated file '{report.Filename}' to folder '{folderPath}'");

            var savedFileGuid = _SharedFolderService.SaveFile(new SharedFolderFile
            {
                FolderPath = folderPath,
                FileNameWithExtension = report.Filename,
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.CampaignExport,
                CreatedDate = _DateTimeEngine.GetCurrentMoment(),
                CreatedBy = userName,
                FileContent = report.Stream
            });

            return savedFileGuid;
        }

        /// <inheritdoc/>
        public CampaignReportData GetAndValidateCampaignReportData(CampaignReportRequest request)
        {
            var campaign = _CampaignRepository.GetCampaign(request.CampaignId);

            if (!request.SelectedPlans.IsEmpty())
            {
                campaign.Plans = campaign.Plans.Where(x => request.SelectedPlans.Contains(x.PlanId)).ToList();
            }
            if (campaign.Plans != null && campaign.HasPlans)
            {
                foreach (var plan in campaign.Plans)
                {
                    if (plan.DraftId > 0)
                    {
                        plan.VersionId = _PlanRepository.GetLatestVersionIdForPlan(plan.PlanId);
                    }
                }
            }
            _ValidateCampaignLocking(campaign.Id);
            _ValidateSelectedPlans(request.ExportType, campaign.Plans);

            AgencyDto agency = _GetAgency(campaign);
            AdvertiserDto advertiser = _GetAdvertiser(campaign);
            Dictionary<int, List<PlanPricingResultsDaypartDto>> planPricingResultsDayparts = new Dictionary<int, List<PlanPricingResultsDaypartDto>>();
            var plans = campaign.Plans
                .Select(x =>
                {
                    var plan = _PlanRepository.GetPlan(x.PlanId, x.VersionId);
                    DaypartTimeHelper.AddOneSecondToEndTime(plan.Dayparts);
                    return plan;
                }).ToList();
            foreach (var plan in plans)
            {
                var dayparts = _GetPlanPricingResultsDayparts(plan);
                if (dayparts != null)
                {
                    planPricingResultsDayparts.Add(plan.Id, dayparts);
                }
            }
            _ValidateGuaranteedAudiences(plans);
            _ValidateSecondaryAudiences(plans);
            List<PlanAudienceDisplay> guaranteedDemos = plans.Select(x => x.AudienceId).Distinct()
                .Select(x => _AudienceService.GetAudienceById(x)).ToList();

            var spotLengths = _SpotLengthRepository.GetSpotLengths();
            var spotLengthDeliveryMultipliers = _SpotLengthRepository.GetDeliveryMultipliersBySpotLengthId();
            var standardDayparts = _StandardDaypartService.GetAllStandardDayparts();
            var audiences = _AudienceService.GetAudiences();

            var campaignReportData = new CampaignReportData(request.ExportType, campaign, plans, agency, advertiser, guaranteedDemos,
                spotLengths,
                spotLengthDeliveryMultipliers,
                standardDayparts,
                audiences,
                 _MediaMonthAndWeekAggregateCache,
                _QuarterCalculationEngine,
                _DateTimeEngine,
                _WeeklyBreakdownEngine, planPricingResultsDayparts, _FeatureToggleHelper);

            return campaignReportData;
        }

        private void _ValidateCampaignLocking(int campaignId)
        {
            const string CAMPAIGN_IS_LOCKED_EXCEPTION = "Campaign with id {0} has been locked by {1}";

            var campaignLockingKey = KeyHelper.GetCampaignLockingKey(campaignId);
            var lockObject = _LockingManagerApplicationService.GetLockObject(campaignLockingKey);

            if (!lockObject.Success)
            {
                var message = string.Format(CAMPAIGN_IS_LOCKED_EXCEPTION, campaignId, lockObject.LockedUserName);
                throw new CadentException(message);
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
                throw new CadentException(message);
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
                throw new CadentException(message);
            }

            if (processingStatus == PlanAggregationProcessingStatusEnum.Error)
            {
                var message = string.Format(PLAN_AGGREGATION_FAILED_EXCEPTION, plan.PlanId);
                throw new CadentException(message);
            }
        }

        private static void _ValidatePostingType(List<PlanSummaryDto> plans)
        {
            const string MULTIPLE_POSTING_TYPES_EXCEPTION = "Cannot have multiple posting types in the export. Please select only plans with the same posting type.";

            if (plans.Select(x => x.PostingType).Distinct().Count() != 1)
            {
                throw new CadentException(MULTIPLE_POSTING_TYPES_EXCEPTION);
            }
        }

        private static void _ValidateGuaranteedAudiences(List<PlanDto> plans)
        {
            const string MULTIPLE_GUARANTEED_AUDIENCES_EXCEPTION = "Cannot have multiple guaranteed audiences in the export. Please select only plans with the same guaranteed audience.";

            if (plans.Select(x => x.AudienceId).Distinct().Count() != 1)
            {
                throw new CadentException(MULTIPLE_GUARANTEED_AUDIENCES_EXCEPTION);
            }
        }

        private static void _ValidateSecondaryAudiences(List<PlanDto> plans)
        {
            const string SECONDARY_AUDIENCES_EXCEPTION = "Cannot have multiple plans with varying secondary audiences in the export. Please select only plans with the same secondary audiences.";
            if (plans.Count == 1)
                return;

            var firstPlanSecondaryAudiencesIds = plans.First().SecondaryAudiences.Select(x => x.AudienceId).ToList();
            foreach (var plan in plans.Skip(1))
            {
                if (plan.SecondaryAudiences.Count != firstPlanSecondaryAudiencesIds.Count ||
                    plan.SecondaryAudiences.Any(x => !firstPlanSecondaryAudiencesIds.Contains(x.AudienceId)))
                {
                    throw new CadentException(SECONDARY_AUDIENCES_EXCEPTION);
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
                    throw new CadentException(INVALID_EXPORT_TYPE_FOR_SELECTED_PLANS);
                }
            }
            else if (distinctPlansStatuses.Contains(PlanStatusEnum.Complete)
                || distinctPlansStatuses.Contains(PlanStatusEnum.Contracted)
                || distinctPlansStatuses.Contains(PlanStatusEnum.Live))
            {
                if (!exportType.Equals(CampaignExportTypeEnum.Contract))
                {
                    throw new CadentException(INVALID_EXPORT_TYPE_FOR_SELECTED_PLANS);
                }
            }
            else
            {
                if (!exportType.Equals(CampaignExportTypeEnum.Proposal))
                {
                    throw new CadentException(INVALID_EXPORT_TYPE_FOR_SELECTED_PLANS);
                }
            }
        }

        public Guid GenerateProgramLineupReport(ProgramLineupReportRequest request, string userName,
            DateTime currentDate, string templatesFilePath)
        {
            var programLineupReportData = GetProgramLineupReportData(request, currentDate);
            var reportGenerator = new ProgramLineupReportGenerator(templatesFilePath);
            var report = reportGenerator.Generate(programLineupReportData);

            return _SharedFolderService.SaveFile(new SharedFolderFile
            {
                FolderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.PROGRAM_LINEUP_REPORTS),
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
                throw new CadentException("Choose at least one plan");

            // for now we generate reports only for one plan
            var planId = request.SelectedPlans.First();
            _ValidatePlanLocking(planId);
            var pricingJob = _GetLatestPricingJob(planId);
            var plan = _PlanRepository.GetPlan(planId);

            if (plan?.Dayparts.Any() ?? false)
            {
                plan.Dayparts = plan.Dayparts.Where(daypart => !EnumHelper.IsCustomDaypart(daypart.DaypartTypeId.GetDescriptionAttribute())).ToList();
            }

            var postingType = request.PostingType ?? plan.PostingType;
            var spotAllocationModelMode = request.SpotAllocationModelMode ?? SpotAllocationModelMode.Quality;

            _ValidateCampaignLocking(plan.CampaignId);
            var campaign = _CampaignRepository.GetCampaign(plan.CampaignId);
            var agency = _GetAgency(campaign);
            var advertiser = _GetAdvertiser(campaign);
            var guaranteedDemo = _AudienceService.GetAudienceById(plan.AudienceId);
            var spotLengths = _SpotLengthRepository.GetSpotLengths();
            var allocatedOpenMarketSpots = _PlanRepository.GetPlanPricingAllocatedSpotsByPlanId(planId, postingType, spotAllocationModelMode);
            var proprietaryInventory = _PlanRepository
                .GetProprietaryInventoryForProgramLineup(plan.PricingParameters.JobId.Value);
            _SetSpotLengthIdAndCalculateImpressions(plan, proprietaryInventory, spotLengths);
            _LoadDaypartData(proprietaryInventory);
            var manifestIdsOpenMarket = allocatedOpenMarketSpots.Select(x => x.StationInventoryManifestId).Distinct();
            var manifestsOpenMarket = _InventoryRepository.GetStationInventoryManifestsByIds(manifestIdsOpenMarket)
                .Where(x => x.Station != null && x.Station.MarketCode.HasValue)
                .ToList();
            var marketCoverages = _MarketCoverageRepository.GetLatestMarketCoveragesWithStations();
            var manifestDaypartIds = manifestsOpenMarket.SelectMany(x => x.ManifestDayparts).Select(x => x.Id.Value).Distinct();
            var primaryProgramsByManifestDaypartIds = _StationProgramRepository.GetPrimaryProgramsForManifestDayparts(manifestDaypartIds);
            var isProgramLineupAllocationByAffiliateEnabled = _IsProgramLineupAllocationByAffiliateEnabled.Value;

            var result = new ProgramLineupReportData(
                plan,
                pricingJob,
                agency,
                advertiser,
                guaranteedDemo,
                spotLengths,
                currentDate,
                allocatedOpenMarketSpots,
                manifestsOpenMarket,
                marketCoverages,
                primaryProgramsByManifestDaypartIds,
                proprietaryInventory,
                postingType,
                spotAllocationModelMode,
                isProgramLineupAllocationByAffiliateEnabled
                );
            return result;
        }

        private void _LoadDaypartData(List<ProgramLineupProprietaryInventory> proprietaryInventory)
        {
            proprietaryInventory.ForEach(x => x.Daypart = _DaypartCache.GetDisplayDaypart(x.DaypartId));
        }

        private void _SetSpotLengthIdAndCalculateImpressions(PlanDto plan
            , List<ProgramLineupProprietaryInventory> proprietaryInventory
            , List<LookupDto> spotLengths)
        {
            const string SPOT_LENGTH_15 = "15";
            const string SPOT_LENGTH_30 = "30";

            int spotLengthIdI5 = spotLengths.Where(x => x.Display.Equals(SPOT_LENGTH_15)).Select(x => x.Id).Single();
            int spotLengthId30 = spotLengths.Where(x => x.Display.Equals(SPOT_LENGTH_30)).Select(x => x.Id).Single();

            int activeWeekCount = plan.WeeklyBreakdownWeeks.Where(w => w.NumberOfActiveDays > 0).Count();
            var planspotLengthIds = plan.CreativeLengths.Select(x => x.SpotLengthId);

            if (planspotLengthIds.Contains(spotLengthIdI5) && !planspotLengthIds.Contains(spotLengthId30))
            {
                proprietaryInventory.ForEach(x =>
                {
                    x.TotalImpressions = x.ImpressionsPerWeek * activeWeekCount / 2;
                    x.SpotLengthId = spotLengthIdI5;
                });
            }
            else
            {
                proprietaryInventory.ForEach(x =>
                {
                    x.TotalImpressions = x.ImpressionsPerWeek * activeWeekCount;
                    x.SpotLengthId = spotLengthId30;
                });
            }
        }

        private PlanPricingJob _GetLatestPricingJob(int planId)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            if (job == null)
                throw new CadentException("There are no completed pricing runs for the chosen plan. Please run pricing");

            if (job.Status == BackgroundJobProcessingStatus.Failed)
                throw new CadentException("The latest pricing run was failed. Please run pricing again or contact the support");

            if (job.Status == BackgroundJobProcessingStatus.Queued || job.Status == BackgroundJobProcessingStatus.Processing)
                throw new CadentException("There is a pricing run in progress right now. Please wait until it is completed");

            return job;
        }


        private AgencyDto _GetAgency(CampaignDto campaign)
        {
            if (campaign == null || !campaign.AgencyMasterId.HasValue) return null;

            var result = _AabEngine.GetAgency(campaign.AgencyMasterId.Value);

            return result;
        }

        private AdvertiserDto _GetAdvertiser(CampaignDto campaign)
        {
            if (campaign == null || !campaign.AdvertiserMasterId.HasValue) return null;

            var result = _AabEngine.GetAdvertiser(campaign.AdvertiserMasterId.Value);

            return result;
        }

        private AgencyDto _GetAgency(CampaignListItemDto campaign)
        {
            if (campaign?.Agency == null || !campaign.Agency.MasterId.HasValue) return null;

            var result = _AabEngine.GetAgency(campaign.Agency.MasterId.Value);


            return result;
        }

        private AdvertiserDto _GetAdvertiser(CampaignListItemDto campaign)
        {
            if (campaign?.Advertiser == null || !campaign.Advertiser.MasterId.HasValue) return null;

            var result = _AabEngine.GetAdvertiser(campaign.Advertiser.MasterId.Value);

            return result;
        }
        private List<PlanPricingResultsDaypartDto> _GetPlanPricingResultsDayparts(PlanDto planDto)
        {
            List<PlanPricingResultsDaypartDto> planPricingResultsDayparts = null;
            var job = _PlanRepository.GetPricingJobForPlanVersion(planDto.VersionId);
            if (job != null)
            {
                var planPricingResult = _PlanRepository.GetPricingResultsByJobId(job.Id, planDto.SpotAllocationModelMode);
                if (planPricingResult != null)
                {
                    planPricingResultsDayparts = _PlanRepository.GetPlanPricingResultsDaypartsByPlanPricingResultId(planPricingResult.Id);
                }
            }
            return planPricingResultsDayparts;
        }

        public BroadcastLockResponse LockCampaigns(int campaignId)
        {
            var broadcastLockResponse = _LockingEngine.LockCampaigns(campaignId);

            return broadcastLockResponse;
        }

        public BroadcastReleaseLockResponse UnlockCampaigns(int campaignId)
        {
            var broadcastReleaseLockResponse = _LockingEngine.UnlockCampaigns(campaignId);

            return broadcastReleaseLockResponse;
        }

        /// <inheritdoc />
        public CampaignCopyDto GetCampaignCopy(int campaignId)
        {
            var campaign = _CampaignRepository.GetCampaignCopy(campaignId);
            List<PlansCopyDto> filteredPlans = new List<PlansCopyDto>();
            if (campaign.Plans != null && campaign.Plans.Count() > 0)
            {
                if (campaign.Plans.Any(x => x.IsDraft == true))
                {
                    campaign.Plans.RemoveAll(x => x.IsDraft == true);
                    foreach (var plan in campaign.Plans)
                    {
                        if ((!filteredPlans.Any(x => x.SourcePlanId == plan.SourcePlanId)))
                        {
                            var latestplan = campaign.Plans.Where(x => x.SourcePlanId == plan.SourcePlanId && (x.IsDraft == false || x.IsDraft == null))
                                   .OrderByDescending(p => p.VersionId).FirstOrDefault();
                            if (latestplan != null)
                            {
                                filteredPlans.Add(latestplan);
                            }
                        }
                    }
                    campaign.Plans = filteredPlans;
                }
                else
                {
                    campaign.Plans = campaign.Plans.Where(x => x.VersionId == x.LatestVersionId).ToList();
                }

            }
            return campaign;
        }


        /// <inheritdoc />
        public int SaveCampaignCopy(SaveCampaignCopyDto campaignCopy, string createdBy, DateTime createdDate)
        {
            int campaignId = 0;
            var campaignDto = new SaveCampaignDto
            {
                Name = campaignCopy.Name,
                AdvertiserMasterId = campaignCopy.AdvertiserMasterId,
                AgencyMasterId = campaignCopy.AgencyMasterId,
                ModifiedBy = createdBy,
                ModifiedDate = createdDate
            };
            _CampaignValidator.Validate(campaignDto);
            var campaign = _CampaignRepository.CheckCampaignExist(campaignDto);
            using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
            {
                if (campaign.Id == 0)
                {
                    campaignId = _CampaignRepository.CreateCampaign(campaignDto, createdBy, createdDate);
                }
                else
                {
                    throw new CadentException($"The Campaign {campaignDto.Name} already exists.");
                }
                if (campaignId > 0)
                {
                    _PlanService.CopyPlans(campaignId, campaignCopy, createdBy, createdDate);

                }
                transaction.Complete();
            }
            _CampaignAggregationJobTrigger.TriggerJob(campaignId, createdBy);
            return campaignId;
        }

        public CampaignExportDto CampaignExportAvailablePlans(int campaignId)
        {
            var campaign = _CampaignRepository.GetCampaignPlanForExport(campaignId);
            if (campaign.HasPlans) { campaign.Plans = GetFilteredPlanwithdraftExcluded(campaign); }
            return campaign;
        }

        private List<PlanExportSummaryDto> GetFilteredPlanwithdraftExcluded(CampaignExportDto campaign)
        {
            campaign.Plans.RemoveAll(x => x.IsDraft == true);
            List<PlanExportSummaryDto> filteredPlans = new List<PlanExportSummaryDto>();
            if (campaign.Plans != null)
            {
                foreach (var plan in campaign.Plans)
                {
                    if ((!filteredPlans.Any(x => x.PlanId == plan.PlanId)))
                    {
                        var latestplan = campaign.Plans.Where(x => x.PlanId == plan.PlanId && (x.IsDraft == false || x.IsDraft == null))
                                .OrderByDescending(p => p.VersionId).FirstOrDefault();
                        if (latestplan != null)
                        {
                            filteredPlans.Add(latestplan);
                        }
                    }
                }
                campaign.Plans = filteredPlans;
            }
            return filteredPlans;
        }

        public async Task<UnifiedCampaignResponseDto> PublishUnifiedCampaign(int campaignId,string updateddBy,DateTime updatedDate)
        {
            UnifiedCampaignResponseDto unifiedCampaignResponse = new UnifiedCampaignResponseDto();
            var campaign = _CampaignRepository.GetCampaign(campaignId);
            unifiedCampaignResponse = await _CampaignServiceApiClient.NotifyCampaignAsync(campaign.UnifiedId);
            if (unifiedCampaignResponse.Success)
            {
                _CampaignRepository.UpdateUnifiedCampaignLastSentAt(campaignId, updatedDate);
                _CampaignRepository.UpdateCampaignLastModified(campaignId, updatedDate, updateddBy);
            }
            return unifiedCampaignResponse;
        }

        public CampaignWithDefaultsDto GetCampaignWithDefaults(int campaignId)
        {
           var campaignWithDefault = _CampaignRepository.GetCampaignWithDefaults(campaignId);

            return campaignWithDefault;
        }
    }
}
