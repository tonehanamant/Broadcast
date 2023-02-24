using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Newtonsoft.Json;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.PlanBuying;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.BuyingResults;
using Services.Broadcast.ReportGenerators.ProgramLineup;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices.Plan
{
    public interface IPlanBuyingService : IApplicationService
    {
        Task<PlanBuyingJob> QueueBuyingJobAsync(PlanBuyingParametersDto planBuyingParametersDto, DateTime currentDate, string username);

        CurrentBuyingExecution GetCurrentBuyingExecution(int planId, PostingTypeEnum postingType);

        CurrentBuyingExecutions GetCurrentBuyingExecution_v2(int planId, PostingTypeEnum postingType);

        CurrentBuyingExecution GetCurrentBuyingExecutionByJobId(int jobId);

        /// <summary>
        /// Cancels the current buying execution.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>The PlanBuyingResponseDto object</returns>
        PlanBuyingResponseDto CancelCurrentBuyingExecution(int planId);

        /// <summary>
        /// Cancels the current buying execution.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>The PlanBuyingResponseDto object</returns>
        PlanBuyingResponseDto CancelCurrentBuyingExecutionByJobId(int jobId);

        [Queue("planbuying")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        Task RunBuyingJobAsync(PlanBuyingParametersDto planBuyingParametersDto, int jobId, CancellationToken token);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        List<PlanBuyingApiRequestParametersDto> GetPlanBuyingRuns(int planId);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        PlanBuyingApiRequestDto_v3 GetBuyingApiRequestPrograms_v3(int planId, BuyingInventoryGetRequestParametersDto requestParameters);

        /// <summary>
        /// Gets the buying ownership groups.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="planBuyingFilter">The plan Buying Filter.</param>
        /// <returns></returns>
        PlanBuyingResultOwnershipGroupDto GetBuyingOwnershipGroups(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null);

        /// <summary>
        /// Gets the buying rep firms.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="planBuyingFilter">The plan Buying Filter.</param>
        /// <returns></returns>
        PlanBuyingResultRepFirmDto GetBuyingRepFirms(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        List<PlanBuyingInventoryProgram> GetBuyingInventory(int planId, BuyingInventoryGetRequestParametersDto requestParameters);

        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetUnitCaps();

        /// <summary>
        /// Gets the buying market groups.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetBuyingMarketGroups();

        PlanBuyingDefaults GetPlanBuyingDefaults();

        /// <summary>
        /// Checks if the buying model is running for the plan
        /// </summary>
        /// <param name="planId">Plan id to be checked</param>
        /// <returns>True or False</returns>
        bool IsBuyingModelRunning(int planId);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        string ForceCompletePlanBuyingJob(int jobId, string username);

        /// <summary>
        /// For troubleshooting.  This will bypass the queue to allow rerunning directly.
        /// </summary>
        /// <param name="jobId">The id of the job to rerun.</param>
        /// <returns>The new JobId</returns>
        Task<int> ReRunBuyingJobAsync(int jobId);

        /// <summary>
        /// For troubleshooting.  Generate a buying results report for the chosen plan and version
        /// </summary>
        /// <param name="planId">The plan id</param>
        /// <param name="planVersionNumber">The plan version number</param>
        /// <param name="templatesFilePath">Base path of the file templates</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="postingType">Type of the posting.</param>
        /// <returns>
        /// ReportOutput which contains filename and MemoryStream which actually contains report data
        /// </returns>
        ReportOutput GenerateBuyingResultsReport(int planId, int? planVersionNumber, string templatesFilePath,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency,
            PostingTypeEnum postingType = PostingTypeEnum.NSI);

        void ValidateAndApplyMargin(PlanBuyingParametersDto parameters);

        /// <summary>
        /// Gets the programs.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="planBuyingFilter">The filter parameter to filter the result.</param>
        /// <returns></returns>
        PlanBuyingResultProgramsDto GetPrograms(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null);

        /// <summary>
        /// Gets the stations.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="planBuyingFilter">The plan Buying Filter </param>
        /// <param name="isConversionRequired">User Format Conversion Flag</param>
        /// <returns></returns>
        PlanBuyingStationResultDto GetStations(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null, bool isConversionRequired = true);

        /// <summary>
        /// Retrieves the Buying Results Markets Summary
        /// </summary>
        PlanBuyingResultMarketsDto GetMarkets(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null);

        PlanBuyingBandsDto GetBuyingBands(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null);

        [Queue("savebuyingrequest")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto buyingApiRequest, string apiVersion, SpotAllocationModelMode spotAllocationModelMode);

        [Queue("savebuyingrequest")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto_v3 buyingApiRequest, string apiVersion, SpotAllocationModelMode spotAllocationModelMode);

        Guid ExportPlanBuyingScx(PlanBuyingScxExportRequest request, string username,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency,
            PostingTypeEnum postingType = PostingTypeEnum.NSI);

        /// <summary>Generates the program lineup report.</summary>
        /// <param name="request">ProgramLineupReportRequest object contains selected plan ids</param>
        /// <param name="userName"></param>
        /// <param name="currentDate"></param>
        /// <param name="templatesFilePath">Path to the template files</param>
        /// <returns>The report id</returns>
        Guid GenerateProgramLineupReport(ProgramLineupReportRequest request, string userName, DateTime currentDate, string templatesFilePath);

        /// <summary>
        /// Gets the plan buying parameters. 
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The type of posting</param>
        /// <returns>The PlanBuyingParametersDto object</returns>
        PlanBuyingParametersDto GetPlanBuyingGoals(int planId, PostingTypeEnum postingType);
        /// <summary>
        /// Retrieves list of result Rep Firms  
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The Type of Posting</param>
        /// <param name="spotAllocationModelMode">The Spot Allocation Model Mode</param>      
        /// <returns>The list of Rep firms</returns>
        List<string> GetResultRepFirms(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency);

        /// <summary>
        /// Retrieves list of result Ownership Groups  
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The Type of Posting</param>
        /// <param name="spotAllocationModelMode">The Spot Allocation Model Mode</param>      
        /// <returns>The list of Ownership Groups</returns>
        List<string> GetResultOwnershipGroups(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency);


        /// <summary>
        /// Deletes the saved buying data.
        /// </summary>
        /// <returns></returns>
        bool DeleteSavedBuyingData();

        /// <summary>
        /// Generates the buying results report, saves it then returns a Guid for retreiving the file.
        /// </summary>
        /// <param name="planBuyingResultsReportRequest">The Plan Buying Results Report Request.</param>
        /// <param name="templateFilePath">The template file path.</param>
        /// <param name="createdBy">The createdBy.</param>
        /// <returns> The Guid</returns>
        Guid GenerateBuyingResultsReportAndSave(PlanBuyingResultsReportRequest planBuyingResultsReportRequest, string templateFilePath, string createdBy);
    }

    /// <summary>
    /// This is a temporary solution for running pricing inside plan
    /// </summary>
    public class PlanBuyingService : BroadcastBaseClass, IPlanBuyingService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanBuyingRepository _PlanBuyingRepository;
        private readonly IPlanBuyingInventoryEngine _PlanBuyingInventoryEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IPlanBuyingApiClient _BuyingApiClient;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly ICampaignRepository _CampaignRepository;
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IBroadcastLockingManagerApplicationService _LockingManagerApplicationService;
        private readonly IMarketCoverageRepository _MarketCoverageRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IStandardDaypartRepository _StandardDaypartRepository;
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IMarketRepository _MarketRepository;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IWeeklyBreakdownEngine _WeeklyBreakdownEngine;
        private readonly IPlanBuyingBandCalculationEngine _PlanBuyingBandCalculationEngine;
        private readonly IPlanBuyingStationEngine _PlanBuyingStationCalculationEngine;
        private readonly IPlanBuyingProgramEngine _PlanBuyingProgramEngine;
        private readonly IPlanBuyingMarketResultsEngine _PlanBuyingMarketResultsEngine;
        private readonly IPlanBuyingOwnershipGroupEngine _PlanBuyingOwnershipGroupEngine;
        private readonly IPlanBuyingRepFirmEngine _PlanBuyingRepFirmEngine;
        private readonly IPlanBuyingRequestLogClient _BuyingRequestLogClient;
        private readonly IInventoryProprietarySummaryRepository _InventoryProprietarySummaryRepository;
        private readonly IBroadcastAudienceRepository _AudienceRepository;
        private readonly IAsyncTaskHelper _AsyncTaskHelper;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IPlanBuyingScxDataPrep _PlanBuyingScxDataPrep;
        private readonly IPlanBuyingScxDataConverter _PlanBuyingScxDataConverter;
        private readonly IAabEngine _AabEngine;
        private readonly IAudienceService _AudienceService;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IDaypartCache _DaypartCache;
        private readonly IPlanValidator _PlanValidator;

        private readonly Lazy<bool> _IsPricingModelOpenMarketInventoryEnabled;
        private readonly Lazy<bool> _IsPricingModelBarterInventoryEnabled;
        private readonly Lazy<bool> _IsPricingModelProprietaryOAndOInventoryEnabled;
        private readonly Lazy<bool> _IsParallelPricingEnabled;
        protected Lazy<bool> _IsProgramLineupAllocationByAffiliateEnabled;

        public PlanBuyingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                  ISpotLengthEngine spotLengthEngine,
                                  IPlanBuyingApiClient buyingApiClient,
                                  IBackgroundJobClient backgroundJobClient,
                                  IPlanBuyingInventoryEngine planBuyingInventoryEngine,
                                  IBroadcastLockingManagerApplicationService lockingManagerApplicationService,
                                  IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                  IDateTimeEngine dateTimeEngine,
                                  IWeeklyBreakdownEngine weeklyBreakdownEngine,
                                  IPlanBuyingBandCalculationEngine planBuyingBandCalculationEngine,
                                  IPlanBuyingStationEngine planBuyingStationCalculationEngine,
                                  IPlanBuyingProgramEngine planBuyingProgramEngine,
                                  IPlanBuyingMarketResultsEngine planBuyingMarketResultsEngine,
                                  IPlanBuyingRequestLogClient buyingRequestLogClient,
                                  IPlanBuyingOwnershipGroupEngine planBuyingOwnershipGroupEngine,
                                  IPlanBuyingRepFirmEngine planBuyingRepFirmEngine,
                                  IAsyncTaskHelper asyncTaskHelper,
                                  ISharedFolderService sharedFolderService,
                                  IPlanBuyingScxDataPrep planBuyingScxDataPrep,
                                  IPlanBuyingScxDataConverter planBuyingScxDataConverter,
                                  IFeatureToggleHelper featureToggleHelper,
                                  IAabEngine aabEngine,
                                  IAudienceService audienceService,
                                  IDaypartCache daypartCache, 
                                  IPlanValidator planValidator,
                                  IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _PlanBuyingRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanBuyingRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _CampaignRepository = broadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _SpotLengthEngine = spotLengthEngine;
            _BuyingApiClient = buyingApiClient;
            _BackgroundJobClient = backgroundJobClient;
            _PlanBuyingInventoryEngine = planBuyingInventoryEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _MarketRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>();
            _DateTimeEngine = dateTimeEngine;
            _WeeklyBreakdownEngine = weeklyBreakdownEngine;
            _PlanBuyingBandCalculationEngine = planBuyingBandCalculationEngine;
            _PlanBuyingStationCalculationEngine = planBuyingStationCalculationEngine;
            _PlanBuyingMarketResultsEngine = planBuyingMarketResultsEngine;
            _BuyingRequestLogClient = buyingRequestLogClient;
            _PlanBuyingOwnershipGroupEngine = planBuyingOwnershipGroupEngine;
            _PlanBuyingProgramEngine = planBuyingProgramEngine;
            _PlanBuyingRepFirmEngine = planBuyingRepFirmEngine;
            _InventoryProprietarySummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProprietarySummaryRepository>();
            _AudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _AsyncTaskHelper = asyncTaskHelper;
            _SharedFolderService = sharedFolderService;
            _PlanBuyingScxDataPrep = planBuyingScxDataPrep;
            _PlanBuyingScxDataConverter = planBuyingScxDataConverter;
            _AabEngine = aabEngine;
            _AudienceService = audienceService;
            _SpotLengthRepository = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _DaypartCache = daypartCache;
            _PlanValidator = planValidator;

            _IsPricingModelOpenMarketInventoryEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.PRICING_MODEL_OPEN_MARKET_INVENTORY));
            _IsPricingModelBarterInventoryEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.PRICING_MODEL_BARTER_INVENTORY));
            _IsPricingModelProprietaryOAndOInventoryEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY));
            _IsParallelPricingEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PARALLEL_PRICINGAPICLIENT_REQUESTS));
            _IsProgramLineupAllocationByAffiliateEnabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PROGRAM_LINEUP_ALLOCATION_BY_AFFILIATE));
        }

        public ReportOutput GenerateBuyingResultsReport(int planId, int? planVersionNumber, string templatesFilePath,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency,
            PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            var reportData = GetBuyingResultsReportData(planId, planVersionNumber, spotAllocationModelMode, postingType);
            var reportGenerator = new BuyingResultsReportGenerator(templatesFilePath);
            var report = reportGenerator.Generate(reportData);

            return report;
        }

        public BuyingResultsReportData GetBuyingResultsReportData(int planId, int? planVersionNumber,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency,
            PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            // use passed version or the current version by default
            var planVersionId = planVersionNumber.HasValue ?
                _PlanRepository.GetPlanVersionIdByVersionNumber(planId, planVersionNumber.Value) :
                (int?)null;

            var plan = _PlanRepository.GetPlan(planId, planVersionId);

            PlanBuyingJob buyingJob;
            try
            {
                buyingJob = _PlanBuyingRepository.GetPlanBuyingJobByPlanVersionId(plan.VersionId);
            }
            catch (Exception ex)
            {
                var planVersionNumberMsg = planVersionNumber.HasValue ? $" Version Number '{planVersionNumber.Value}'" : "";
                var msg = $"Buying job not found for Plan Id '{planId}'{planVersionNumberMsg}.";
                throw new CadentException(msg, ex);
            }

            var runResults = _PlanBuyingRepository.GetBuyingApiResultsByJobId(buyingJob.Id, spotAllocationModelMode, postingType);
            var allocatedSpots = runResults.AllocatedSpots;
            var manifestIds = allocatedSpots.Select(x => x.StationInventoryManifestId).Distinct();
            var manifests = _InventoryRepository.GetStationInventoryManifestsByIds(manifestIds);
            var manifestDaypartIds = manifests.SelectMany(x => x.ManifestDayparts).Select(x => x.Id.Value);
            var primaryProgramsByManifestDaypartIds = _StationProgramRepository.GetPrimaryProgramsForManifestDayparts(manifestDaypartIds);
            var markets = _MarketRepository.GetMarketDtos();

            var data = new BuyingResultsReportData(
                plan,
                spotAllocationModelMode,
                postingType,
                allocatedSpots,
                manifests,
                primaryProgramsByManifestDaypartIds,
                markets,
                _WeeklyBreakdownEngine);

            return data;
        }

        public Guid ExportPlanBuyingScx(PlanBuyingScxExportRequest request, string username,
            SpotAllocationModelMode spotAllocationModelMode,
            PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            const string fileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var generated = _DateTimeEngine.GetCurrentMoment();

            _ValidatePlanBuyingScxExportRequest(request);

            var scxData = _PlanBuyingScxDataPrep.GetScxData(request, generated, spotAllocationModelMode, postingType);
            var scxFile = _PlanBuyingScxDataConverter.ConvertData(scxData, spotAllocationModelMode);
            scxFile.spotAllocationModelMode = spotAllocationModelMode.ToString().Substring(0, 1);
            var sharedFile = new SharedFolderFile
            {
                FolderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.PLAN_BUYING_SCX),
                FileNameWithExtension = scxFile.FileName,
                FileMediaType = fileMediaType,
                FileUsage = SharedFolderFileUsage.PlanBuyingScx,
                CreatedDate = generated,
                CreatedBy = username,
                FileContent = scxFile.ScxStream
            };
            var savedFileGuid = _SharedFolderService.SaveFile(sharedFile);

            return savedFileGuid;
        }

        internal void _ValidatePlanBuyingScxExportRequest(PlanBuyingScxExportRequest request)
        {
            if (request.UnallocatedCpmThreshold.HasValue &&
                (request.UnallocatedCpmThreshold.Value < 1 || request.UnallocatedCpmThreshold.Value > 100))
            {
                throw new CadentException($"Invalid value for UnallocatedCpmThreshold : '{request.UnallocatedCpmThreshold.Value}'; Valid range is 1-100.");
            }
        }

        public async Task<PlanBuyingJob> QueueBuyingJobAsync(PlanBuyingParametersDto planBuyingParametersDto
            , DateTime currentDate, string username)
        {
            // lock the plan so that two requests for the same plan can not get in this area concurrently
            var key = KeyHelper.GetPlanLockingKey(planBuyingParametersDto.PlanId.Value);
            var lockObject = _LockingManagerApplicationService.GetNotUserBasedLockObjectForKey(key);

            lock (lockObject)
            {
                if (IsBuyingModelRunning(planBuyingParametersDto.PlanId.Value))
                {
                    throw new CadentException("The buying model is already running for the plan");
                }

                var plan = _PlanRepository.GetPlan(planBuyingParametersDto.PlanId.Value);

                UsePlanForBuyingJob(planBuyingParametersDto, plan);

                ValidateAndApplyMargin(planBuyingParametersDto);

                _PlanValidator.ValidatePlanNotCrossQuartersForBuying(plan);

                var job = new PlanBuyingJob
                {
                    PlanVersionId = plan.VersionId,
                    Status = BackgroundJobProcessingStatus.Queued,
                    Queued = currentDate
                };
                using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                {
                    planBuyingParametersDto.PlanVersionId = plan.VersionId;
                    job.Id = _SaveBuyingJobAndParameters(job, planBuyingParametersDto);
                    _CampaignRepository.UpdateCampaignLastModified(plan.CampaignId, currentDate, username);
                    transaction.Complete();
                }

                job.HangfireJobId = _BackgroundJobClient.Enqueue<IPlanBuyingService>(x => x.RunBuyingJobAsync(planBuyingParametersDto, job.Id
                    , CancellationToken.None));

                _PlanBuyingRepository.UpdateJobHangfireId(job.Id, job.HangfireJobId);

                return job;
            }
        }

        private int _SaveBuyingJobAndParameters(PlanBuyingJob job, PlanBuyingParametersDto planBuyingParametersDto)
        {
            var jobId = _PlanBuyingRepository.AddPlanBuyingJob(job);
            planBuyingParametersDto.JobId = jobId;
            _PlanBuyingRepository.SavePlanBuyingParameters(planBuyingParametersDto);

            return jobId;
        }

        public void ValidateAndApplyMargin(PlanBuyingParametersDto parameters)
        {
            const double allowedMinValue = .01;
            const double allowedMaxValue = 100;

            if (parameters.Margin.HasValue)
            {
                if (parameters.Margin.Value > allowedMaxValue ||
                    parameters.Margin.Value < allowedMinValue)
                {
                    throw new CadentException("A provided Margin value must be between .01% And 100%.");
                }
            }

            if (parameters.Margin > 0)
            {
                parameters.AdjustedBudget = parameters.Budget * (decimal)(1.0 - (parameters.Margin / 100.0));
                parameters.AdjustedCPM = parameters.AdjustedBudget / Convert.ToDecimal(parameters.DeliveryImpressions);
            }
            else
            {
                parameters.AdjustedBudget = parameters.Budget;
                parameters.AdjustedCPM = parameters.CPM;
            }
        }

        public List<LookupDto> GetUnitCaps()
        {
            return Enum.GetValues(typeof(UnitCapEnum))
                .Cast<UnitCapEnum>()
                .Select(e => new LookupDto
                {
                    Id = (int)e,
                    Display = e.GetDescriptionAttribute()
                })
                .OrderBy(x => x.Id)
                .ToList();
        }

        public List<LookupDto> GetBuyingMarketGroups()
        {
            return Enum.GetValues(typeof(MarketGroupEnum))
                .Cast<MarketGroupEnum>()
                .Select(e => new LookupDto
                {
                    Id = (int)e,
                    Display = e.GetDescriptionAttribute()
                })
                .OrderBy(x => x.Id)
                .ToList();
        }

        public PlanBuyingDefaults GetPlanBuyingDefaults()
        {
            const float defaultMargin = 20;

            return new PlanBuyingDefaults
            {
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.Per30Min,
                Margin = defaultMargin,
                MarketGroup = MarketGroupEnum.Top100
            };
        }

        public CurrentBuyingExecution GetCurrentBuyingExecutionByJobId(int jobId)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);

            return _GetCurrentBuyingExecution(job, null);
        }

        private CurrentBuyingExecution _GetCurrentBuyingExecution(PlanBuyingJob job, int? planId, PostingTypeEnum postingType = PostingTypeEnum.NSI,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            CurrentBuyingExecutionResultDto buyingExecutionResult = null;

            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                //in case the error is comming from the Buying Run model, the error message field will have better
                //message then the generic we construct here
                if (string.IsNullOrWhiteSpace(job.DiagnosticResult))
                    throw new CadentException(job.ErrorMessage);
                throw new CadentException(
                    "Error encountered while running Buying Model, please contact a system administrator for help");
            }

            if (job != null && job.Status == BackgroundJobProcessingStatus.Succeeded)
            {
                buyingExecutionResult = _PlanBuyingRepository.GetBuyingResultsByJobId(job.Id, spotAllocationModelMode, postingType);

                if (buyingExecutionResult != null)
                {
                    buyingExecutionResult.Notes = buyingExecutionResult.GoalFulfilledByProprietary
                        ? "Proprietary goals meet plan goals"
                        : string.Empty;
                    if (buyingExecutionResult.JobId.HasValue)
                    {
                        decimal goalCpm;
                        if (buyingExecutionResult.PlanVersionId.HasValue)
                            goalCpm = _PlanBuyingRepository.GetGoalCpm(buyingExecutionResult.PlanVersionId.Value,
                                buyingExecutionResult.JobId.Value, buyingExecutionResult.PostingType);
                        else
                            goalCpm = _PlanBuyingRepository.GetGoalCpm(buyingExecutionResult.JobId.Value, buyingExecutionResult.PostingType);

                        buyingExecutionResult.CpmPercentage =
                            CalculateCpmPercentage(buyingExecutionResult.OptimalCpm, goalCpm);
                    }
                }
            }

            //buyingExecutionResult might be null when there is no buying run for the latest version            
            return new CurrentBuyingExecution
            {
                Job = job,
                Result = buyingExecutionResult ?? new CurrentBuyingExecutionResultDto
                {
                    SpotAllocationModelMode = spotAllocationModelMode,
                    PostingType = postingType
                },
                IsBuyingModelRunning = IsBuyingModelRunning(job)
            };
        }

        private CurrentBuyingExecutions _GetCurrentBuyingExecution_v2(PlanBuyingJob job, int? planId, 
            PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            List<CurrentBuyingExecutionResultDto> buyingExecutionResults = null;

            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                //in case the error is comming from the Buying Run model, the error message field will have better
                //message then the generic we construct here
                if (string.IsNullOrWhiteSpace(job.DiagnosticResult))
                    throw new CadentException(job.ErrorMessage);
                throw new CadentException(
                    "Error encountered while running Buying Model, please contact a system administrator for help");
            }

            if (job != null && job.Status == BackgroundJobProcessingStatus.Succeeded)
            {
                buyingExecutionResults = _PlanBuyingRepository.GetBuyingResultsByJobId(job.Id, postingType);

                if (buyingExecutionResults != null)
                {
                    foreach (var buyingExecutionResult in buyingExecutionResults)
                    {
                        _GetBuyingExecutionResult(buyingExecutionResult);
                    }
                }
            }

            //buyingExecutionResult might be null when there is no buying run for the latest version            
            var result = new CurrentBuyingExecutions
            {
                Job = job,
                Results = buyingExecutionResults ?? _GetDefaultBuyingResultsList(postingType),
                IsBuyingModelRunning = IsBuyingModelRunning(job)
            };

            if (job?.Status != BackgroundJobProcessingStatus.Succeeded)
            {
                return result;
            }

            var jobCompletedWithinLastFiveMinutes = _DidBuyingJobCompleteWithinThreshold(job, thresholdMinutes: 5);
            if (jobCompletedWithinLastFiveMinutes)
            {
                // expecting 2 results
                // This call is Per Posting Type (not like Pricing, which is all at once).
                // Model Modes = 2
                // PostingType = 1
                // 2 * 1 = 2 results
                const int expectedResultCount = 2;
                result = _ValidateBuyingExecutionResult(result, expectedResultCount);
            }
            else
            {
                var filledInResults = _FillInMissingBuyingResultsWithEmptyResults(result.Results, postingType);
                result.Results = filledInResults;
            }

            return result;
        }

        private List<CurrentBuyingExecutionResultDto> _GetDefaultBuyingResultsList()
        {
            var emptyList = new List<CurrentBuyingExecutionResultDto>
            {
                new CurrentBuyingExecutionResultDto() {SpotAllocationModelMode = SpotAllocationModelMode.Efficiency}
            };
            return emptyList;
        }

        private void _AddEmptyBuyingResult(List<CurrentBuyingExecutionResultDto> results, PostingTypeEnum postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            if (!results.Any(a => a.PostingType == postingType && a.SpotAllocationModelMode == spotAllocationModelMode))
            {
                results.Add(new CurrentBuyingExecutionResultDto
                {
                    PostingType = postingType,
                    SpotAllocationModelMode = spotAllocationModelMode
                });
            }
        }

        internal List<CurrentBuyingExecutionResultDto> _FillInMissingBuyingResultsWithEmptyResults(List<CurrentBuyingExecutionResultDto> candidateResults, PostingTypeEnum postingType)
        {
            var results = candidateResults.DeepCloneUsingSerialization();

            _AddEmptyBuyingResult(results, postingType, SpotAllocationModelMode.Efficiency);
            _AddEmptyBuyingResult(results, postingType, SpotAllocationModelMode.Floor);

            return results;
        }

        internal bool _DidBuyingJobCompleteWithinThreshold(PlanBuyingJob job, int thresholdMinutes)
        {
            if (!job.Completed.HasValue)
            {
                return false;
            }

            DateTime thresholdMinutesAgo = _DateTimeEngine.GetCurrentMoment().AddMinutes(-1 * thresholdMinutes);
            var jobCompletedWithinLastFiveMinutes = job.Completed.Value >= thresholdMinutesAgo;
            return jobCompletedWithinLastFiveMinutes;
        }

        internal CurrentBuyingExecutions _ValidateBuyingExecutionResult(CurrentBuyingExecutions result, int expectedResult)
        {
            if (result.IsBuyingModelRunning == false)
            {
                if (result.Results.Count != expectedResult)
                {
                    result.IsBuyingModelRunning = true;
                    result.Results = _GetDefaultBuyingResultsList();
                }
            }
            return result;
        }

        private List<CurrentBuyingExecutionResultDto> _GetDefaultBuyingResultsList(PostingTypeEnum postingType)
        {
            var emptyList = new List<CurrentBuyingExecutionResultDto>
            {
                new CurrentBuyingExecutionResultDto() {PostingType = postingType}
            };
            return emptyList;
        }

        public CurrentBuyingExecution GetCurrentBuyingExecution(int planId, PostingTypeEnum postingType)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            return _GetCurrentBuyingExecution(job, planId, postingType);
        }

        public CurrentBuyingExecutions GetCurrentBuyingExecution_v2(int planId, PostingTypeEnum postingType)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            return _GetCurrentBuyingExecution_v2(job, planId, postingType);
        }

        private void _GetBuyingExecutionResult(CurrentBuyingExecutionResultDto buyingExecutionResult)
        {
            buyingExecutionResult.Notes = buyingExecutionResult.GoalFulfilledByProprietary
                                   ? "Proprietary goals meet plan goals"
                                   : string.Empty;
            if (buyingExecutionResult.JobId.HasValue)
            {
                decimal goalCpm;
                if (buyingExecutionResult.PlanVersionId.HasValue)
                    goalCpm = _PlanBuyingRepository.GetGoalCpm(buyingExecutionResult.PlanVersionId.Value,
                        buyingExecutionResult.JobId.Value, buyingExecutionResult.PostingType);
                else
                    goalCpm = _PlanBuyingRepository.GetGoalCpm(buyingExecutionResult.JobId.Value, buyingExecutionResult.PostingType);

                buyingExecutionResult.CpmPercentage =
                    CalculateCpmPercentage(buyingExecutionResult.OptimalCpm, goalCpm);
            }
        }

        /// <summary>
        /// Goal CPM Percentage Indicator Calculation
        /// </summary>
        /// <returns></returns>
        public int CalculateCpmPercentage(decimal optimalCpm, decimal goalCpm)
        {
            if (goalCpm == 0) return 0;
            return (int)Math.Round(GeneralMath.ConvertFractionToPercentage(optimalCpm / goalCpm));
        }

        /// <inheritdoc />
        public PlanBuyingResponseDto CancelCurrentBuyingExecutionByJobId(int jobId)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);

            return _CancelCurrentBuyingExecution(job);
        }

        private PlanBuyingResponseDto _CancelCurrentBuyingExecution(PlanBuyingJob job)
        {
            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                throw new CadentException("Error encountered while running Buying Model, please contact a system administrator for help");
            }

            if (!IsBuyingModelRunning(job))
            {
                var jobCompletedWithinOneMinute = _DidBuyingJobCompleteWithinThreshold(job, thresholdMinutes: 1);
                if (jobCompletedWithinOneMinute)
                {
                    // if we're here then we hit that time between finished and UI picked it up.
                    // but the user thinks they hit Cancel... 
                    // we want to tell them that the Cancel didn't take affect and the results are new.
                    _LogInfo("The user requested to Cancel, but the job just finished.");
                    throw new CadentException("While attempting to Cancel the job, it completed.");
                }

                throw new CadentException("Error encountered while canceling Buying Model, process is not running");
            }

            if (string.IsNullOrEmpty(job?.HangfireJobId) == false)
            {
                try
                {
                    _BackgroundJobClient.Delete(job.HangfireJobId);
                }
                catch (Exception ex)
                {
                    _LogError($"Exception caught attempting to delete hangfire job '{job.HangfireJobId}'.", ex);
                }
            }

            job.Status = BackgroundJobProcessingStatus.Canceled;
            job.Completed = _DateTimeEngine.GetCurrentMoment();

            _PlanBuyingRepository.UpdatePlanBuyingJob(job);

            return new PlanBuyingResponseDto
            {
                Job = job,
                Result = new PlanBuyingResultDto(),
                IsBuyingModelRunning = false
            };
        }

        /// <inheritdoc />
        public PlanBuyingResponseDto CancelCurrentBuyingExecution(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            return _CancelCurrentBuyingExecution(job);
        }

        public static bool IsBuyingModelRunning(PlanBuyingJob job)
        {
            return job != null && (job.Status == BackgroundJobProcessingStatus.Queued || job.Status == BackgroundJobProcessingStatus.Processing);
        }

        /// <inheritdoc />
        public bool IsBuyingModelRunning(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);
            return IsBuyingModelRunning(job);
        }

        public List<PlanBuyingApiRequestParametersDto> GetPlanBuyingRuns(int planId)
        {
            return _PlanBuyingRepository.GetPlanBuyingRuns(planId);
        }

        private List<ShareOfVoice> _GetShareOfVoice(
            MarketCoverageDto topMarkets,
            IEnumerable<PlanAvailableMarketDto> marketsWithSov,
            ProprietaryInventoryData proprietaryInventoryData,
            double planImpressionsGoal, SpotAllocationModelMode spotAllocationModelMode)
        {
            if (spotAllocationModelMode == SpotAllocationModelMode.Floor)
            {
                return new List<ShareOfVoice>();
            }

            var topMarketsShareOfVoice = topMarkets.MarketCoveragesByMarketCode.Select(x => new ShareOfVoice
            {
                MarketCode = x.Key,
                MarketGoal = x.Value
            }).ToList();

            var planShareOfVoices = marketsWithSov.Select(x => new ShareOfVoice
            {
                MarketCode = x.MarketCode,
                MarketGoal = x.ShareOfVoicePercent.Value
            }).ToList();

            var planMarketCodes = new HashSet<int>(planShareOfVoices.Select(y => y.MarketCode));
            topMarketsShareOfVoice.RemoveAll(x => planMarketCodes.Contains(x.MarketCode));
            topMarketsShareOfVoice.AddRange(planShareOfVoices);

            var impressionsPerOnePercent = planImpressionsGoal / 100;

            foreach (var market in topMarketsShareOfVoice)
            {
                if (proprietaryInventoryData.ImpressionsPerMarket.TryGetValue((short)market.MarketCode, out var proprietaryImpressionsForMarket))
                {
                    var impressionsGoalForMarket = market.MarketGoal * impressionsPerOnePercent;

                    if (impressionsGoalForMarket > proprietaryImpressionsForMarket)
                    {
                        var impressionsForPricing = impressionsGoalForMarket - proprietaryImpressionsForMarket;
                        market.MarketGoal = impressionsForPricing / impressionsPerOnePercent;
                    }
                    else
                    {
                        market.MarketGoal = 0;
                    }
                }
            }

            topMarketsShareOfVoice = topMarketsShareOfVoice.Where(x => x.MarketGoal > 0).ToList();
            topMarketsShareOfVoice.ForEach(x => x.MarketGoal = GeneralMath.ConvertPercentageToFraction(x.MarketGoal));

            return topMarketsShareOfVoice;
        }

        private MarketCoverageDto _GetTopMarkets(MarketGroupEnum buyingMarketSovMinimum)
        {
            switch (buyingMarketSovMinimum)
            {
                case MarketGroupEnum.Top100:
                    return _MarketCoverageRepository.GetLatestTop100MarketCoverages();
                case MarketGroupEnum.Top50:
                    return _MarketCoverageRepository.GetLatestTop50MarketCoverages();
                case MarketGroupEnum.Top25:
                    return _MarketCoverageRepository.GetLatestTop25MarketCoverages();
                case MarketGroupEnum.All:
                    return _MarketCoverageRepository.GetLatestMarketCoverages();
                default:
                    return new MarketCoverageDto();
            }
        }

        /// <inheritdoc />
        public async Task<int> ReRunBuyingJobAsync(int jobId)
        {
            var originalJob = _PlanBuyingRepository.GetPlanBuyingJob(jobId);
            // get the plan params
            var jobParams = _PlanBuyingRepository.GetLatestParametersForPlanBuyingJob(jobId);

            // create the artifacts
            var newJob = new PlanBuyingJob
            {
                PlanVersionId = originalJob.PlanVersionId,
                Status = BackgroundJobProcessingStatus.Queued,
                Queued = _DateTimeEngine.GetCurrentMoment()
            };
            var newJobId = _SaveBuyingJobAndParameters(newJob, jobParams);

            // call the job directly
            await RunBuyingJobAsync(jobParams, newJobId, CancellationToken.None);

            return newJobId;
        }

        private async Task<List<PlanBuyingAllocationResult>> _SendBuyingRequestsAsync(int jobId, PlanDto plan, List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingParametersDto planBuyingParametersDto, ProprietaryInventoryData proprietaryInventoryData,
            CancellationToken token, bool goalsFulfilledByProprietaryInventory,
            PlanBuyingJobDiagnostic diagnostic)
        {

            var results = new List<PlanBuyingAllocationResult>();

            results.Add(new PlanBuyingAllocationResult
            {
                JobId = jobId,
                PlanVersionId = plan.VersionId,
                BuyingVersion = _GetPricingModelVersion().ToString(),
                SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                PostingType = plan.PostingType
            });

            results.Add(new PlanBuyingAllocationResult
            {
                JobId = jobId,
                PlanVersionId = plan.VersionId,
                BuyingVersion = _GetPricingModelVersion().ToString(),
                SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                PostingType = plan.PostingType
            });

            if (!goalsFulfilledByProprietaryInventory)
            {
                if (!_IsParallelPricingEnabled.Value)
                {
                    foreach (var allocationResult in results)
                    {
                        _LogInfo($"Preparing call to Buying Model for Mode '{allocationResult.SpotAllocationModelMode}'.");
                        var pricingModelCallTimer = new Stopwatch();
                        pricingModelCallTimer.Start();

                        try
                        {
                            await _SendBuyingRequest_v3Async(
                                allocationResult,
                                plan,
                                jobId,
                                inventory,
                                token,
                                diagnostic,
                                planBuyingParametersDto,
                                proprietaryInventoryData);
                        }
                        finally
                        {
                            pricingModelCallTimer.Stop();
                            var duration = pricingModelCallTimer.ElapsedMilliseconds;

                            _LogInfo($"Completed call to Buying Model for Mode '{allocationResult.SpotAllocationModelMode}'.  Duration : {duration}ms");
                        }
                    }
                }
                else
                {
                    var tasks = results.Select(async allocationResult =>
                    {
                        _LogInfo($"Preparing call to Buying Model for Mode '{allocationResult.SpotAllocationModelMode}'.");
                        var pricingModelCallTimer = new Stopwatch();
                        pricingModelCallTimer.Start();

                        try
                        {
                            await _SendBuyingRequest_v3Async(
                               allocationResult,
                               plan,
                               jobId,
                               inventory,
                               token,
                               diagnostic,
                               planBuyingParametersDto,
                               proprietaryInventoryData);
                        }
                        finally
                        {
                            pricingModelCallTimer.Stop();
                            var duration = pricingModelCallTimer.ElapsedMilliseconds;

                            _LogInfo($"Completed call to Buying Model for Mode '{allocationResult.SpotAllocationModelMode}'.  Duration : {duration}ms");
                        }
                    });
                    await Task.WhenAll(tasks);
                }
            }
            return results;
        }

        private async Task _RunBuyingJobAsync(PlanBuyingParametersDto planBuyingParametersDto, PlanDto plan, int jobId, CancellationToken token)
        {
            // run buying will not use custom daypart
            if (plan?.Dayparts.Any() ?? false)
            {
                plan.Dayparts = plan.Dayparts.Where(daypart => !EnumHelper.IsCustomDaypart(daypart.DaypartTypeId.GetDescriptionAttribute())).ToList();
            }

            var transactionId = Guid.NewGuid();
            var msgStamp = $"JobId={jobId}; TxId={transactionId};";
            _LogInfo($"Beginning Buying... {msgStamp}");

            var diagnostic = new PlanBuyingJobDiagnostic();
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_TOTAL_DURATION);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_PROCESSING);
            var planBuyingJob = _PlanBuyingRepository.GetPlanBuyingJob(jobId);
            planBuyingJob.Status = BackgroundJobProcessingStatus.Processing;
            _PlanBuyingRepository.UpdatePlanBuyingJob(planBuyingJob);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_PROCESSING);

            try
            {
                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_FETCHING_PLAN_AND_PARAMETERS);

                var programInventoryParameters = new ProgramInventoryOptionalParametersDto
                {
                    MinCPM = planBuyingParametersDto.MinCpm,
                    MaxCPM = planBuyingParametersDto.MaxCpm,
                    InflationFactor = planBuyingParametersDto.InflationFactor,
                    MarketGroup = planBuyingParametersDto.MarketGroup,
                    ShareBookId = planBuyingParametersDto.ShareBookId,
                    HUTBookId = planBuyingParametersDto.HUTBookId
                };
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_FETCHING_PLAN_AND_PARAMETERS);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_GATHERING_INVENTORY);
                var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());
                var inventory = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan,
                    programInventoryParameters,
                    inventorySourceIds,
                    diagnostic);

                _ValidateInventory(inventory);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_GATHERING_INVENTORY);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_PROPRIETARY_DATA);
                var proprietaryInventoryData = _CalculateProprietaryInventoryData(plan, planBuyingParametersDto);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_PROPRIETARY_DATA);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_PROPRIETARY_DATA);
                _ApplyBuyingParametersAndProprietaryInventoryToPlanWeeks(plan, proprietaryInventoryData, out var goalsFulfilledByProprietaryInventory);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_APPLYING_PROPRIETARY_DATA);

                token.ThrowIfCancellationRequested();

                _LogInfo($"Preparing to call the Buying Model... {msgStamp}");
                var modelAllocationResults = await _SendBuyingRequestsAsync(jobId, plan, inventory, planBuyingParametersDto, proprietaryInventoryData,
                    token, goalsFulfilledByProprietaryInventory, diagnostic);
                
                token.ThrowIfCancellationRequested();

                _LogInfo($"Validating results {msgStamp}");

                /*** Validate the Results ***/
                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);
                foreach (var allocationResult in modelAllocationResults)
                {
                    _ValidateAllocationResult(allocationResult);
                }
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);

                token.ThrowIfCancellationRequested();

                _LogInfo($"Calculating plan buying band inventory... {msgStamp}");
                _CalculatePlanBuyingBandInventory(modelAllocationResults, planBuyingParametersDto, inventory, token);

                _LogInfo($"Saving raw inventory... {msgStamp}");
                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_SPOTS_RAW);
                var planBuyingRawInventory = _MapBuyingRawInventory(modelAllocationResults, inventory);
                _SaveBuyingRawInventory(plan.Id, jobId, planBuyingRawInventory);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_SPOTS_RAW);

                foreach (var allocationResult in modelAllocationResults)
                {
                    _LogInfo($"Beginning to aggregate results. {msgStamp}");
                    var aggregationTasks = new List<AggregationTask>();

                    var allocationResults = new Dictionary<PostingTypeEnum, PlanBuyingAllocationResult>();

                    //Always loop the posting type that matches the plan first so that we convert from nti to nsi only one time
                    foreach (var targetPostingType in Enum.GetValues(typeof(PostingTypeEnum)).Cast<PostingTypeEnum>()
                        .OrderByDescending(x => x == plan.PostingType)) //order by desc to prioritize true values
                    {
                        List<PlanBuyingInventoryProgram> postingTypeInventory;
                        PlanBuyingAllocationResult postingAllocationResult;

                        if (targetPostingType == plan.PostingType)
                        {
                            postingTypeInventory = inventory;
                            postingAllocationResult = allocationResult;
                        }
                        else
                        {
                            //We have to make copies of the list and the item for thread-safety and 
                            //not modifying certain properties on original allocation results
                            postingTypeInventory = inventory.DeepCloneUsingSerialization();
                            _PlanBuyingInventoryEngine.ConvertPostingType(targetPostingType, postingTypeInventory);

                            postingAllocationResult = allocationResult.DeepCloneUsingSerialization();
                            // override with our instance posting type.
                            postingAllocationResult.PostingType = targetPostingType;

                            _ValidateInventory(postingTypeInventory);
                            _MapAllocationResultsPostingType(postingAllocationResult, postingTypeInventory, targetPostingType,
                                plan.PostingType, out var nsiNtiConversionFactor);
                        }

                        allocationResults.Add(targetPostingType, postingAllocationResult);
                        /*** Use the Results ***/
                        diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_CPM);
                        postingAllocationResult.BuyingCpm =
                            _CalculateBuyingCpm(postingAllocationResult.AllocatedSpots, proprietaryInventoryData);
                        diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_CPM);

                        token.ThrowIfCancellationRequested();

                        /*** Start Aggregations ***/

                        var calculateBuyingProgramsTask = new Task<PlanBuyingResultBaseDto>(() =>
                        {
                            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                            var programResults = _PlanBuyingProgramEngine.Calculate(postingTypeInventory, postingAllocationResult,
                                goalsFulfilledByProprietaryInventory);
                            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                            return programResults;
                        });
                        aggregationTasks.Add(new AggregationTask(targetPostingType, BuyingJobTaskNameEnum.CalculatePrograms,
                            calculateBuyingProgramsTask));
                        calculateBuyingProgramsTask.Start();

                        var calculateBuyingProgramStationsTask = new Task<PlanBuyingResultBaseDto>(() =>
                        {
                            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                            var programStationResults = _PlanBuyingProgramEngine.CalculateProgramStations(postingTypeInventory, postingAllocationResult,
                                goalsFulfilledByProprietaryInventory);
                            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                            return programStationResults;
                        });
                        aggregationTasks.Add(new AggregationTask(targetPostingType, BuyingJobTaskNameEnum.CalculateProgramStations,
                            calculateBuyingProgramStationsTask));
                        calculateBuyingProgramStationsTask.Start();

                        var calculateBuyingBandsTask = new Task<PlanBuyingBandsDto>(() =>
                        {
                            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_BANDS);
                            var buyingBands = _PlanBuyingBandCalculationEngine.Calculate(postingTypeInventory, postingAllocationResult,
                                planBuyingParametersDto);
                            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_BANDS);
                            return buyingBands;
                        });
                        aggregationTasks.Add(new AggregationTask(targetPostingType, BuyingJobTaskNameEnum.CalculateBands,
                            calculateBuyingBandsTask));
                        calculateBuyingBandsTask.Start();

                        var calculateBuyingStationsTask = new Task<PlanBuyingStationResultDto>(() =>
                        {
                            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_STATIONS);
                            var buyingStations =
                                _PlanBuyingStationCalculationEngine.Calculate(postingTypeInventory, postingAllocationResult,
                                    planBuyingParametersDto);
                            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_STATIONS);
                            return buyingStations;
                        });
                        aggregationTasks.Add(new AggregationTask(targetPostingType, BuyingJobTaskNameEnum.CalculateStations,
                            calculateBuyingStationsTask));
                        calculateBuyingStationsTask.Start();

                        var aggregateMarketResultsTask = new Task<PlanBuyingResultMarketsDto>(() =>
                        {
                            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                            var marketCoverages = _MarketCoverageRepository.GetMarketsWithLatestCoverage();
                            var buyingMarketResults = _PlanBuyingMarketResultsEngine.Calculate(postingTypeInventory, postingAllocationResult,
                                planBuyingParametersDto, plan
                                , marketCoverages);
                            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                            return buyingMarketResults;
                        });
                        aggregationTasks.Add(new AggregationTask(targetPostingType, BuyingJobTaskNameEnum.CalculateMarkets,
                            aggregateMarketResultsTask));
                        aggregateMarketResultsTask.Start();

                        var aggregateOwnershipGroupResultsTask = new Task<PlanBuyingResultOwnershipGroupDto>(() =>
                        {
                            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_OWNERSHIP_GROUP_RESULTS);
                            var buyingOwnershipGroupResults = _PlanBuyingOwnershipGroupEngine.Calculate(postingTypeInventory, postingAllocationResult,
                                 planBuyingParametersDto);
                            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_OWNERSHIP_GROUP_RESULTS);
                            return buyingOwnershipGroupResults;
                        });
                        aggregationTasks.Add(new AggregationTask(targetPostingType, BuyingJobTaskNameEnum.CalculateOwnershipGroups,
                            aggregateOwnershipGroupResultsTask));
                        aggregateOwnershipGroupResultsTask.Start();

                        var aggregateRepFirmResultsTask = new Task<PlanBuyingResultRepFirmDto>(() =>
                        {
                            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_REP_FIRM_RESULTS);
                            var buyingOwnershipGroupResults = _PlanBuyingRepFirmEngine.Calculate(postingTypeInventory, postingAllocationResult,
                                 planBuyingParametersDto);
                            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_REP_FIRM_RESULTS);
                            return buyingOwnershipGroupResults;
                        });
                        aggregationTasks.Add(new AggregationTask(targetPostingType, BuyingJobTaskNameEnum.CalculateRepFirms,
                            aggregateRepFirmResultsTask));
                        aggregateRepFirmResultsTask.Start();
                    }
                    token.ThrowIfCancellationRequested();

                    //Wait for all tasks nti and nsi to finish
                    var allAggregationTasks = Task.WhenAll(aggregationTasks.Select(x => x.Task).ToArray());
                    allAggregationTasks.Wait();
                    /*** Persist the results ***/
                    _SaveBuyingArtifacts(allocationResults, aggregationTasks, diagnostic, transactionId);
                }

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);
                var buyingJob = _PlanBuyingRepository.GetPlanBuyingJob(jobId);
                buyingJob.Status = BackgroundJobProcessingStatus.Succeeded;
                buyingJob.Completed = _DateTimeEngine.GetCurrentMoment();
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);

                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_TOTAL_DURATION);
                buyingJob.DiagnosticResult = diagnostic.ToString();

                _LogInfo($"Job finished.  Updating to indicate Success. {msgStamp}");
                _PlanBuyingRepository.UpdatePlanBuyingJob(buyingJob);
            }
            catch (BuyingModelException exception)
            {
                _LogInfo($"Job finished.  Updating to indicate {BackgroundJobProcessingStatus.Failed}. {msgStamp}");
                _HandleBuyingJobError(jobId, BackgroundJobProcessingStatus.Failed, exception.Message);
            }
            catch (Exception exception) when (exception is ObjectDisposedException || exception is OperationCanceledException)
            {
                _LogInfo($"Job finished.  Updating to indicate {BackgroundJobProcessingStatus.Canceled}. {msgStamp}");
                _HandleBuyingJobException(jobId, BackgroundJobProcessingStatus.Canceled, exception, "Running the buying model was canceled");
            }
            catch (Exception exception)
            {
                _LogInfo($"Job finished.  Updating to indicate {BackgroundJobProcessingStatus.Failed}. {msgStamp}");
                _HandleBuyingJobException(jobId, BackgroundJobProcessingStatus.Failed, exception, "Error attempting to run the buying model");
            }
        }

        /// <summary>
        /// Maps the buying raw inventory.
        /// </summary>
        /// <param name="modelAllocationResults">The model allocation results.</param>
        /// <param name="inventory">The inventory.</param>
        /// <returns></returns>
        internal PlanBuyingInventoryRawDto _MapBuyingRawInventory(List<PlanBuyingAllocationResult> modelAllocationResults, List<PlanBuyingInventoryProgram> inventory)
        {
            const int unallocatedSpot = 0;

            var rawSpots = modelAllocationResults.FirstOrDefault();
            var planBuyingRawInventory = new PlanBuyingInventoryRawDto()
            {
                PostingType = rawSpots.PostingType,
                SpotAllocationModelMode = rawSpots.SpotAllocationModelMode,
                AllocatedSpotsRaw = rawSpots.AllocatedSpots.Select(rawAllocatedSpots => new PlanBuyingSpotRaw()
                {
                    StationInventoryManifestId = rawAllocatedSpots.Id,
                    PostingTypeConversationRate = inventory.First(y => y.StandardDaypartId == rawAllocatedSpots.StandardDaypart.Id).NsiToNtiImpressionConversionRate,
                    InventoryMediaWeekId = rawAllocatedSpots.InventoryMediaWeek.Id,
                    Impressions30sec = rawAllocatedSpots.Impressions30sec,
                    ContractMediaWeekId = rawAllocatedSpots.ContractMediaWeek.Id,
                    StandardDaypartId = rawAllocatedSpots.StandardDaypart.Id,
                    SpotFrequenciesRaw = rawAllocatedSpots.SpotFrequencies.Select(spotFrequencyRaw => new SpotFrequencyRaw()
                    {
                        SpotLengthId = spotFrequencyRaw.SpotLengthId,
                        SpotCost = spotFrequencyRaw.SpotCost,
                        Spots = unallocatedSpot,
                        Impressions = spotFrequencyRaw.Impressions
                    }).ToList()
                }).ToList(),
                UnallocatedSpotsRaw = rawSpots.UnallocatedSpots.Select(rawUnallocatedSpots => new PlanBuyingSpotRaw()
                {
                    StationInventoryManifestId = rawUnallocatedSpots.Id,
                    PostingTypeConversationRate = inventory.First(y => y.StandardDaypartId == rawUnallocatedSpots.StandardDaypart.Id).NsiToNtiImpressionConversionRate,
                    InventoryMediaWeekId = rawUnallocatedSpots.InventoryMediaWeek.Id,
                    Impressions30sec = rawUnallocatedSpots.Impressions30sec,
                    ContractMediaWeekId = rawUnallocatedSpots.ContractMediaWeek.Id,
                    StandardDaypartId = rawUnallocatedSpots.StandardDaypart.Id,
                    SpotFrequenciesRaw = rawUnallocatedSpots.SpotFrequencies.Select(spotFrequencyRaw => new SpotFrequencyRaw()
                    {
                        SpotLengthId = spotFrequencyRaw.SpotLengthId,
                        SpotCost = spotFrequencyRaw.SpotCost,
                        Spots = unallocatedSpot,
                        Impressions = spotFrequencyRaw.Impressions
                    }).ToList()
                }).ToList()
            };
            return planBuyingRawInventory;
        }

        private void _CalculatePlanBuyingBandInventory(List<PlanBuyingAllocationResult> modelAllocationResults, PlanBuyingParametersDto planBuyingParametersDto,
            List<PlanBuyingInventoryProgram> inventory, CancellationToken token)
        {
            var diagnostic = new PlanBuyingJobDiagnostic();
            var buyingBandResult = modelAllocationResults.FirstOrDefault();
            if (buyingBandResult == null)
            {
                return;
            }

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_BAND_INVENTORY);
            var buyingBandInventoryStations = _PlanBuyingBandCalculationEngine.CalculateBandInventoryStation(inventory, buyingBandResult);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_BAND_INVENTORY);

            _SaveBuyingBandInventory(planBuyingParametersDto, buyingBandInventoryStations, diagnostic);

            token.ThrowIfCancellationRequested();
        }

        private void _SaveBuyingRawInventory(int planId, int jobId, PlanBuyingInventoryRawDto planBuyingApiScx)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(planBuyingApiScx, Formatting.Indented);
                var fileName = _BuyingRequestLogClient.SaveBuyingRawInventory(planId, jobId, jsonData);
                _PlanBuyingRepository.UpdateFileName(jobId, fileName);

            }
            catch (Exception exception)
            {
                _LogError("Failed to save buying API raw inventory", exception);
            }
        }

        private void _MapAllocationResultsPostingType(PlanBuyingAllocationResult allocationResult, List<PlanBuyingInventoryProgram> inventory,
            PostingTypeEnum targetPostingType, PostingTypeEnum sourcePostingType, out double nsitoNtiConversionFactor)
        {
            nsitoNtiConversionFactor = 1; //default

            if (targetPostingType == sourcePostingType)
            {
                return;
            }

            var totalImpressions = allocationResult.AllocatedSpots.SelectMany(x => x.SpotFrequencies)
                .Sum(x => x.Impressions);

            double weightedConversionFactorSum = 0;
            int conversionFactorCount = 0;

            foreach (var spot in allocationResult.AllocatedSpots)
            {
                var nsiToNtiConverisonRate = inventory.First(y => y.ManifestId == spot.Id).NsiToNtiImpressionConversionRate;
                foreach (var spotFrequency in spot.SpotFrequencies)
                {
                    if (sourcePostingType == PostingTypeEnum.NSI)
                    {
                        spotFrequency.Impressions *= nsiToNtiConverisonRate;
                    }
                    else if (sourcePostingType == PostingTypeEnum.NTI)
                    {
                        spotFrequency.Impressions /= nsiToNtiConverisonRate;
                    }
                    else
                    {
                        throw new CadentException("Invalid target posting type.");
                    }

                    var impressionWeight = spotFrequency.Impressions / totalImpressions;
                    weightedConversionFactorSum += spotFrequency.Impressions * impressionWeight;
                    conversionFactorCount++;
                }
            }

            nsitoNtiConversionFactor = weightedConversionFactorSum / conversionFactorCount;
        }
        internal enum BuyingJobTaskNameEnum
        {
            CalculatePrograms,
            CalculateBands,
            CalculateStations,
            CalculateMarkets,
            CalculateOwnershipGroups,
            CalculateRepFirms,
            CalculateProgramStations
        }

        internal class AggregationTask
        {
            public PostingTypeEnum PostingType { get; set; }
            public BuyingJobTaskNameEnum TaskName { get; set; }
            public Task Task { get; set; }

            public AggregationTask(PostingTypeEnum postingType, BuyingJobTaskNameEnum taskName, Task task)
            {
                PostingType = postingType;
                TaskName = taskName;
                Task = task;
            }
        }

        internal void _SaveBuyingBandInventory(PlanBuyingParametersDto planBuyingParametersDto, PlanBuyingBandInventoryStationsDto buyingBandInventoryStations,
             PlanBuyingJobDiagnostic diagnostic)
        {
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_BAND_INVENTORY);
            buyingBandInventoryStations.PostingType = planBuyingParametersDto.PostingType;
            _PlanBuyingRepository.SavePlanBuyingBandInventoryStations(buyingBandInventoryStations);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_BAND_INVENTORY);
        }

        internal void _SaveBuyingArtifacts(IDictionary<PostingTypeEnum, PlanBuyingAllocationResult> allocationResults,
            List<AggregationTask> aggregationTasks,
             PlanBuyingJobDiagnostic diagnostic, Guid? transactionId = null)
        {
            var jobId = allocationResults.First().Value.JobId;
            var modelMode = allocationResults.First().Value.SpotAllocationModelMode;
            transactionId = transactionId ?? Guid.NewGuid();
            var msgStamp = $"JobId={jobId}; ModelMode={modelMode}; TxId={transactionId};";

            _LogInfo($"Beginning to save Buying Artifacts. {msgStamp}");

            foreach (var postingType in Enum.GetValues(typeof(PostingTypeEnum)).Cast<PostingTypeEnum>())
            {
                var postingAllocationResult = allocationResults[postingType];

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);
                _LogInfo($"Beginning to SaveBuyingApiResults for Posting Type {postingType}. {msgStamp}");
                _PlanBuyingRepository.SaveBuyingApiResults(postingAllocationResult);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);
                
                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);
                var calculateBuyingProgramsTask = (Task<PlanBuyingResultBaseDto>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == BuyingJobTaskNameEnum.CalculatePrograms)
                    .Task;
                var calculateBuyingProgramsTaskResult = calculateBuyingProgramsTask.Result;
                calculateBuyingProgramsTaskResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                calculateBuyingProgramsTaskResult.PostingType = postingAllocationResult.PostingType;

                var calculateBuyingProgramStationsTask = (Task<PlanBuyingResultBaseDto>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == BuyingJobTaskNameEnum.CalculateProgramStations)
                    .Task;
                var calculateBuyingProgramStationsTaskResult = calculateBuyingProgramStationsTask.Result;
                calculateBuyingProgramStationsTaskResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                calculateBuyingProgramStationsTaskResult.PostingType = postingAllocationResult.PostingType;

                _LogInfo($"Beginning to SaveBuyingAggregateResults for Posting Type {postingType}. {msgStamp}");
                var planVersionBuyingResultId = _PlanBuyingRepository.SaveBuyingAggregateResults(calculateBuyingProgramsTaskResult);
                _LogInfo($"Beginning to SavePlanBuyingResultSpots for Posting Type {postingType}. {msgStamp}");
                _PlanBuyingRepository.SavePlanBuyingResultSpots(planVersionBuyingResultId, calculateBuyingProgramsTaskResult);
                _LogInfo($"Beginning to SavePlanBuyingResultSpotStations for Posting Type {postingType}. {msgStamp}");
                _PlanBuyingRepository.SavePlanBuyingResultSpotStations(planVersionBuyingResultId, calculateBuyingProgramStationsTaskResult);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_BANDS);
                var calculateBuyingBandTask = (Task<PlanBuyingBandsDto>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == BuyingJobTaskNameEnum.CalculateBands)
                    .Task;
                var calculateBuyingBandTaskResult = calculateBuyingBandTask.Result;
                calculateBuyingBandTaskResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                _LogInfo($"Beginning to SavePlanBuyingBands for Posting Type {postingType}. {msgStamp}");
                _PlanBuyingRepository.SavePlanBuyingBands(calculateBuyingBandTaskResult, postingType);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_BANDS);

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_STATIONS);
                var calculateBuyingStationTask = (Task<PlanBuyingStationResultDto>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == BuyingJobTaskNameEnum.CalculateStations)
                    .Task;
                var calculateBuyingStationTaskResult = calculateBuyingStationTask.Result;
                calculateBuyingStationTaskResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                _LogInfo($"Beginning to SavePlanBuyingStations for Posting Type {postingType}. {msgStamp}");
                _PlanBuyingRepository.SavePlanBuyingStations(calculateBuyingStationTaskResult, postingType);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_STATIONS);

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);
                var aggregateMarketResultsTask = (Task<PlanBuyingResultMarketsDto>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == BuyingJobTaskNameEnum.CalculateMarkets)
                    .Task;
                var aggregateMarketResultsTaskResult = aggregateMarketResultsTask.Result;
                aggregateMarketResultsTaskResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                _LogInfo($"Beginning to SavePlanBuyingMarketResults for Posting Type {postingType}. {msgStamp}");
                _PlanBuyingRepository.SavePlanBuyingMarketResults(aggregateMarketResultsTaskResult, postingType);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_OWNERSHIP_GROUP_RESULTS);
                var aggregateOwnershipGroupResultsTask = (Task<PlanBuyingResultOwnershipGroupDto>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == BuyingJobTaskNameEnum.CalculateOwnershipGroups)
                    .Task;
                var aggregateOwnershipGroupResultsTaskResult = aggregateOwnershipGroupResultsTask.Result;
                aggregateOwnershipGroupResultsTaskResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                _LogInfo($"Beginning to SavePlanBuyingOwnershipGroupResults for Posting Type {postingType}. {msgStamp}");
                _PlanBuyingRepository.SavePlanBuyingOwnershipGroupResults(aggregateOwnershipGroupResultsTaskResult, postingType);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_OWNERSHIP_GROUP_RESULTS);

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_REP_FIRM_RESULTS);
                var aggregateRepFirmResultsTask = (Task<PlanBuyingResultRepFirmDto>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == BuyingJobTaskNameEnum.CalculateRepFirms)
                    .Task;
                var aggregateRepFirmResultsTaskResult = aggregateRepFirmResultsTask.Result;
                aggregateRepFirmResultsTaskResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                _LogInfo($"Beginning to SavePlanBuyingRepFirmResults for Posting Type {postingType}. {msgStamp}");
                _PlanBuyingRepository.SavePlanBuyingRepFirmResults(aggregateRepFirmResultsTaskResult, postingType);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_REP_FIRM_RESULTS);
            }
            _LogInfo($"Finished to save Buying Artifacts. {msgStamp}");
        }

        private ProprietaryInventoryData _CalculateProprietaryInventoryData(
            PlanDto plan,
            PlanBuyingParametersDto parameters)
        {
            var result = new ProprietaryInventoryData();

            var proprietarySummaryIds = parameters.ProprietaryInventory.Select(x => x.Id).ToList();
            var proprietarySummaries = _InventoryProprietarySummaryRepository.GetInventoryProprietarySummariesByIds(proprietarySummaryIds);
            var ratingAudienceIds = new HashSet<int>(_AudienceRepository
                .GetRatingsAudiencesByMaestroAudience(new List<int> { plan.AudienceId })
                .Select(am => am.rating_audience_id)
                .Distinct()
                .ToList());

            result.TotalImpressions = proprietarySummaries
                .SelectMany(x => x.SummaryByStationByAudience)
                .Where(x => ratingAudienceIds.Contains(x.AudienceId))
                .Sum(x => x.Impressions);

            result.TotalCost = proprietarySummaries.Sum(x => x.UnitCost);
            result.TotalCostWithMargin = GeneralMath.CalculateCostWithMargin(result.TotalCost, parameters.Margin);

            result.ImpressionsPerMarket = proprietarySummaries
                .SelectMany(x => x.SummaryByStationByAudience)
                .Where(x => ratingAudienceIds.Contains(x.AudienceId))
                .GroupBy(x => x.MarketCode)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Impressions));

            _AdjustProprietaryInventoryDataForActiveWeks(plan, result);
            _AdjustProprietaryInventoryDataFor15SpotLength(plan, result);

            return result;
        }

        private void _AdjustProprietaryInventoryDataForActiveWeks(
                PlanDto plan,
                ProprietaryInventoryData proprietaryInventoryData)
        {
            // take only those weeks that have some goals
            var numberOfPlanWeeksWithGoals = plan.WeeklyBreakdownWeeks
                .GroupBy(x => x.MediaWeekId)
                .Where(x => x.Any(w => w.WeeklyImpressions > 0))
                .Count();

            // multiply proprietary inventory by the number of weeks with goals
            // because proprietary tables store data per one week
            proprietaryInventoryData.TotalImpressions *= numberOfPlanWeeksWithGoals;
            proprietaryInventoryData.TotalCost *= numberOfPlanWeeksWithGoals;
            proprietaryInventoryData.TotalCostWithMargin *= numberOfPlanWeeksWithGoals;

            foreach (var key in proprietaryInventoryData.ImpressionsPerMarket.Keys.ToList())
            {
                proprietaryInventoryData.ImpressionsPerMarket[key] *= numberOfPlanWeeksWithGoals;
            }
        }

        private void _AdjustProprietaryInventoryDataFor15SpotLength(
            PlanDto plan,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            var spotLengthId15 = _SpotLengthEngine.GetSpotLengthIdByValue(15);
            var spotLengthId30 = _SpotLengthEngine.GetSpotLengthIdByValue(30);

            if (plan.CreativeLengths.Any(x => x.SpotLengthId == spotLengthId15) &&
                plan.CreativeLengths.All(x => x.SpotLengthId != spotLengthId30))
            {
                // If the plan is 15 spot length only use half cost and impressions for each unit
                proprietaryInventoryData.TotalImpressions /= 2;
                proprietaryInventoryData.TotalCost /= 2;

                foreach (var key in proprietaryInventoryData.ImpressionsPerMarket.Keys.ToList())
                {
                    proprietaryInventoryData.ImpressionsPerMarket[key] /= 2;
                }
            }
        }

        private void _ApplyBuyingParametersAndProprietaryInventoryToPlanWeeks(
            PlanDto plan,
            ProprietaryInventoryData proprietaryInventoryData,
            out bool goalsFulfilledByProprietaryInventory)
        {
            var planImpressionsGoal = plan.BuyingParameters.DeliveryImpressions * 1000;
            var totalImpressions = planImpressionsGoal - proprietaryInventoryData.TotalImpressions;
            var totalBudget = plan.BuyingParameters.Budget - proprietaryInventoryData.TotalCost;

            goalsFulfilledByProprietaryInventory = totalImpressions <= 0 || totalBudget <= 0;

            if (goalsFulfilledByProprietaryInventory)
            {
                plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>();
            }
            else
            {
                // if any SpotLengthId or DaypartCodeId is not set, then we deal with a breakdown which is already grouped based on delivery type
                var weeks = plan.WeeklyBreakdownWeeks.Any(x => !x.SpotLengthId.HasValue || !x.DaypartCodeId.HasValue) ?
                    plan.WeeklyBreakdownWeeks :
                    _WeeklyBreakdownEngine.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(plan);

                var request = new WeeklyBreakdownRequest
                {
                    CreativeLengths = plan.CreativeLengths,
                    Dayparts = plan.Dayparts,
                    DeliveryType = plan.GoalBreakdownType,
                    FlightStartDate = plan.FlightStartDate.Value,
                    FlightEndDate = plan.FlightEndDate.Value,
                    FlightDays = plan.FlightDays,
                    TotalBudget = totalBudget,
                    TotalImpressions = totalImpressions,
                    FlightHiatusDays = plan.FlightHiatusDays,
                    TotalRatings = plan.TargetRatingPoints.Value,
                    Weeks = weeks,
                    ImpressionsPerUnit = plan.ImpressionsPerUnit.Value,
                    WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                };

                plan.WeeklyBreakdownWeeks = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request).Weeks;
                plan.WeeklyBreakdownWeeks = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);
            }
        }

        private PlanBuyingBudgetCpmLeverEnum _GetPlanBuyingBudgetCpmLeverEnum(BudgetCpmLeverEnum budgetCpmLever)
        {
            switch (budgetCpmLever)
            {
                case BudgetCpmLeverEnum.Budget:
                    return PlanBuyingBudgetCpmLeverEnum.budget;
                case BudgetCpmLeverEnum.Cpm:
                    return PlanBuyingBudgetCpmLeverEnum.impressions;
                default:
                    return PlanBuyingBudgetCpmLeverEnum.impressions;
            }
        }

        private async Task _SendBuyingRequest_v3Async(
            PlanBuyingAllocationResult allocationResult,
            PlanDto plan,
            int jobId,
            List<PlanBuyingInventoryProgram> inventory,
            CancellationToken token,
            PlanBuyingJobDiagnostic diagnostic,
            PlanBuyingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            var apiVersion = "4";
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var buyingModelWeeks = _GetBuyingModelWeeks_v3(plan, parameters, proprietaryInventoryData, out List<int> skippedWeeksIds, allocationResult.SpotAllocationModelMode);
            var groupedInventory = _GroupInventory(inventory);
            var spotsAndMappings = _GetBuyingModelSpots_v3(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var pricingConfiguration = new PlanBuyingApiRequestConfigurationDto
            {
                BudgetCpmLever = _GetPlanBuyingBudgetCpmLeverEnum(parameters.BudgetCpmLever)
            };

            token.ThrowIfCancellationRequested();

            var buyingApiRequest = new PlanBuyingApiRequestDto_v3
            {
                Weeks = buyingModelWeeks,
                Spots = spotsAndMappings.Spots,
                Configuration = pricingConfiguration
            };

            var planSpotLengthIds = plan.CreativeLengths.Select(s => s.SpotLengthId).ToList();
            _HandleMissingSpotCosts(planSpotLengthIds, buyingApiRequest);

            _LogInfo($"Sending buying model input to the S3 log bucket for model '{allocationResult.SpotAllocationModelMode}'.");
            _AsyncTaskHelper.TaskFireAndForget(() => SaveBuyingRequest(plan.Id, jobId, buyingApiRequest, apiVersion, allocationResult.SpotAllocationModelMode));

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALLING_API);
            var apiAllocationResult = await _BuyingApiClient.GetBuyingSpotsResultAsync(buyingApiRequest);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALLING_API);

            token.ThrowIfCancellationRequested();

            if (apiAllocationResult.Error != null)
            {
                var errorMessage = $@"Buying Model Mode ('{allocationResult.SpotAllocationModelMode}') Request Id '{apiAllocationResult.RequestId}' returned the following error: {apiAllocationResult.Error.Name} 
                                -  {string.Join(",", apiAllocationResult.Error.Messages).Trim(',')}";

                throw new BuyingModelException(errorMessage);
            }

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
            var mappedResults = _MapToResultSpotsV3(apiAllocationResult, buyingApiRequest, inventory, parameters, plan, spotsAndMappings.Mappings);
            allocationResult.AllocatedSpots = mappedResults.Allocated;
            allocationResult.UnallocatedSpots = mappedResults.Unallocated;
            allocationResult.RequestId = apiAllocationResult.RequestId;
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
        }

        internal SpotsAndMappingsV3 _GetBuyingModelSpots_v3(
            List<IGrouping<PlanBuyingInventoryGroup, ProgramWithManifestDaypart>> groupedInventory,
            List<int> skippedWeeksIds)
        {
            var results = new SpotsAndMappingsV3();

            var marketCoveragesByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;

            foreach (var inventoryGrouping in groupedInventory)
            {
                var programSpots = new List<ProgramDaypartWeekGroupItem_V3>();

                var programsInGrouping = inventoryGrouping.Select(x => x.Program).ToList();
                var manifestId = programsInGrouping.First().ManifestId;

                foreach (var program in programsInGrouping)
                {
                    // filter out the zero spot costs
                    var validSpotCosts = program.ManifestRates.Where(s => s.Cost > 0).Select(x => new SpotCost_v3
                    {
                        SpotLengthId = x.SpotLengthId,
                        SpotLengthCost = x.Cost
                    }).ToList();

                    if (!validSpotCosts.Any())
                    {
                        continue;
                    }

                    var programMinimumContractMediaWeekId = program.ManifestWeeks.Select(w => w.ContractMediaWeekId).Min();

                    foreach (var daypart in program.ManifestDayparts)
                    {
                        var programInventoryDaypartId = daypart.Daypart.Id;

                        var impressions = program.Impressions;

                        if (impressions <= 0)
                        {
                            continue;
                        }

                        foreach (var programWeek in program.ManifestWeeks)
                        {
                            if (skippedWeeksIds.Contains(programWeek.ContractMediaWeekId))
                            {
                                continue;
                            }

                            var item = new ProgramDaypartWeekGroupItem_V3
                            {
                                ContractedInventoryId = manifestId,
                                ContractedMediaWeekId = programWeek.ContractMediaWeekId,
                                InventoryDaypartId = programInventoryDaypartId,
                                ProgramMinimumContractMediaWeekId = programMinimumContractMediaWeekId,
                                Spot = new PlanBuyingApiRequestSpotsDto_v3
                                {
                                    Id = manifestId,
                                    MediaWeekId = programWeek.ContractMediaWeekId,
                                    Impressions30sec = impressions,
                                    StationId = program.Station.Id,
                                    MarketCode = program.Station.MarketCode.Value,
                                    DaypartId = program.StandardDaypartId,
                                    PercentageOfUs =
                                        GeneralMath.ConvertPercentageToFraction(
                                            marketCoveragesByMarketCode[program.Station.MarketCode.Value]),
                                    SpotDays = daypart.Daypart.ActiveDays,
                                    SpotHours = daypart.Daypart.GetDurationPerDayInHours(),
                                    SpotCost = validSpotCosts
                                },
                                Mapping = new InventorySpotMapping
                                {
                                    SentManifestId = manifestId,
                                    SentMediaWeekId = programWeek.ContractMediaWeekId,
                                    MappedManifestId = program.ManifestId,
                                    MappedMediaWeekId = programWeek.InventoryMediaWeekId
                                }
                            };

                            programSpots.Add(item);
                        }
                    }
                }

                var groupedProgramSpots = programSpots.GroupBy(i => new { i.ContractedInventoryId, i.ContractedMediaWeekId, i.InventoryDaypartId }).ToList();
                foreach (var group in groupedProgramSpots)
                {
                    if (group.Count() == 1)
                    {
                        results.Spots.Add(group.First().Spot);
                        results.Mappings.Add(group.First().Mapping);
                        continue;
                    }

                    // keep the one with the most recent start day : the program would have all the weeks that generated that spot
                    ProgramDaypartWeekGroupItem_V3 keptItem = null;
                    foreach (var item in group)
                    {
                        if (keptItem == null ||
                            item.ProgramMinimumContractMediaWeekId > keptItem.ProgramMinimumContractMediaWeekId)
                        {
                            keptItem = item;
                        }
                    }

                    if (keptItem != null)
                    {
                        results.Spots.Add(keptItem.Spot);
                        results.Mappings.Add(keptItem.Mapping);
                    }
                }
            }

            return results;
        }

        public async Task RunBuyingJobAsync(PlanBuyingParametersDto planBuyingParametersDto, int jobId, CancellationToken token)
        {
            var plan = _PlanRepository.GetPlan(planBuyingParametersDto.PlanId.Value, planBuyingParametersDto.PlanVersionId);

            await _RunBuyingJobAsync(planBuyingParametersDto, plan, jobId, token);
        }

        internal List<PlanBuyingApiRequestWeekDto_v3> _GetBuyingModelWeeks_v3(
            PlanDto plan,
            PlanBuyingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData,
            out List<int> SkippedWeeksIds,
             SpotAllocationModelMode spotAllocationModelMode)
        {
            SkippedWeeksIds = new List<int>();
            var buyingModelWeeks = new List<PlanBuyingApiRequestWeekDto_v3>();
            var planImpressionsGoal = plan.BuyingParameters.DeliveryImpressions * 1000;

            // send 0.001% if any unit is selected
            var marketCoverageGoal = parameters.ProprietaryInventory.IsEmpty() ? GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value) : 0.001;
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);

            var shareOfVoice = spotAllocationModelMode == SpotAllocationModelMode.Floor ?
               new List<ShareOfVoice>() : _GetShareOfVoice(topMarkets, marketsWithSov, proprietaryInventoryData, planImpressionsGoal, spotAllocationModelMode);

            var daypartsWithWeighting = plan.Dayparts.Where(x => x.WeightingGoalPercent.HasValue);
            var planBuyingParameters = plan.BuyingParameters;

            var weeklyBreakdownByWeek = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks);
            var spotScaleFactorBySpotLengthId = _GetSpotScaleFactorBySpotLengthId(plan);

            foreach (var week in weeklyBreakdownByWeek)
            {
                var mediaWeekId = week.MediaWeekId;
                var impressionGoal = week.Impressions;
                var weeklyBudget = week.Budget;

                if (impressionGoal <= 0)
                {
                    SkippedWeeksIds.Add(mediaWeekId);
                    continue;
                }

                if (weeklyBudget <= 0)
                {
                    SkippedWeeksIds.Add(mediaWeekId);
                    continue;
                }

                if (parameters.Margin > 0)
                {
                    weeklyBudget *= (decimal)(1.0 - (parameters.Margin / 100.0));
                }

                // call the plancalculatebudgetbymode to adjust impressions when goal is floor and efficiency
                var budgetResponseByModes = PlanGoalHelper.PlanCalculateBudgetByMode(weeklyBudget, impressionGoal, spotAllocationModelMode, parameters.BudgetCpmLever);
                var fluidityImpressionGoal = planBuyingParameters.FluidityPercentage.HasValue ? (double)(budgetResponseByModes.ImpressionGoal * (100 - planBuyingParameters.FluidityPercentage)) / 100 : budgetResponseByModes.ImpressionGoal;
                var buyingWeek = new PlanBuyingApiRequestWeekDto_v3
                {
                    MediaWeekId = mediaWeekId,
                    ImpressionGoal = fluidityImpressionGoal,
                    CpmGoal = budgetResponseByModes.CpmGoal,
                    MarketCoverageGoal = marketCoverageGoal,
                    FrequencyCap = FrequencyCapHelper.GetFrequencyCap(planBuyingParameters.UnitCapsType, planBuyingParameters.UnitCaps),
                    ShareOfVoice = shareOfVoice,
                    DaypartWeighting = _GetDaypartGoals(plan, mediaWeekId),
                    SpotLengths = _GetSpotLengthGoals(plan, mediaWeekId, spotScaleFactorBySpotLengthId)
                };

                buyingModelWeeks.Add(buyingWeek);
            }

            return buyingModelWeeks;
        }

        private List<DaypartWeighting> _GetDaypartGoals(PlanDto plan, int mediaWeekId)
        {
            var breakdownForWeek = plan.WeeklyBreakdownWeeks.Where(x => x.MediaWeekId == mediaWeekId).ToList();
            var impressionsForWeek = breakdownForWeek.Sum(x => x.WeeklyImpressions);

            var daypartGoals = plan.Dayparts
                .Select(x =>
                {
                    var impressionsForDaypart = breakdownForWeek
                        .Where(y => y.DaypartCodeId == x.DaypartCodeId)
                        .Sum(y => y.WeeklyImpressions);

                    return new DaypartWeighting
                    {
                        DaypartId = x.DaypartCodeId,
                        DaypartGoal = impressionsForDaypart / impressionsForWeek
                    };
                })
                .ToList();

            // add the remaining goal to the first daypart
            var remainingGoal = 1 - daypartGoals.Sum(x => x.DaypartGoal);
            daypartGoals.First().DaypartGoal += remainingGoal;

            return daypartGoals;
        }

        private List<SpotLength_v3> _GetSpotLengthGoals(PlanDto plan, int mediaWeekId, Dictionary<int, double> spotScaleFactorBySpotLengthId)
        {
            var breakdownForWeek = plan.WeeklyBreakdownWeeks.Where(x => x.MediaWeekId == mediaWeekId).ToList();
            var impressionsForWeek = breakdownForWeek.Sum(x => x.WeeklyImpressions);

            var spotLengthGoalBySpotLengthId = plan.CreativeLengths
                .ToDictionary(
                    x => x.SpotLengthId,
                    x =>
                    {
                        var impressionsForSpotLength = breakdownForWeek
                            .Where(y => y.SpotLengthId == x.SpotLengthId)
                            .Sum(y => y.WeeklyImpressions);

                        return impressionsForSpotLength / impressionsForWeek;
                    });


            // add the remaining goal to the first spot length
            var remainingGoal = 1 - spotLengthGoalBySpotLengthId.Sum(x => x.Value);
            spotLengthGoalBySpotLengthId[plan.CreativeLengths.First().SpotLengthId] += remainingGoal;

            return plan.CreativeLengths
                .Select(x => new SpotLength_v3
                {
                    SpotLengthId = x.SpotLengthId,
                    SpotScaleFactor = spotScaleFactorBySpotLengthId[x.SpotLengthId],
                    SpotLengthGoal = spotLengthGoalBySpotLengthId[x.SpotLengthId]
                })
                .ToList();
        }

        private Dictionary<int, double> _GetSpotScaleFactorBySpotLengthId(PlanDto plan)
        {
            return plan.CreativeLengths.ToDictionary(
                x => x.SpotLengthId,
                x => (plan.Equivalized ?? false) ? _SpotLengthEngine.GetDeliveryMultiplierBySpotLengthId(x.SpotLengthId) : 1);
        }

        internal static List<IGrouping<PlanBuyingInventoryGroup, ProgramWithManifestDaypart>> _GroupInventory(List<PlanBuyingInventoryProgram> inventory)
        {
            var flattedProgramsWithDayparts = inventory
                .SelectMany(x => x.ManifestDayparts.Select(d => new ProgramWithManifestDaypart
                {
                    Program = x,
                    ManifestDaypart = d
                })).ToList();

            var grouped = flattedProgramsWithDayparts.GroupBy(x =>
                new PlanBuyingInventoryGroup
                {
                    StationId = x.Program.Station.Id,
                    DaypartId = x.ManifestDaypart.Daypart.Id,
                    PrimaryProgramName = x.ManifestDaypart.PrimaryProgram.Name
                }).ToList();

            return grouped;
        }

        public void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto buyingApiRequest, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            try
            {
                string unZipped = JsonConvert.SerializeObject(buyingApiRequest, Formatting.Indented);
                _BuyingRequestLogClient.SaveBuyingRequest(planId, jobId, unZipped, apiVersion, spotAllocationModelMode);
            }
            catch (Exception exception)
            {
                _LogError("Failed to save buying API request", exception);
            }
        }

        public void SaveBuyingRequest(int planId, int jobId, PlanBuyingApiRequestDto_v3 buyingApiRequest, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            try
            {
                string unZipped = JsonConvert.SerializeObject(buyingApiRequest, Formatting.Indented);
                _BuyingRequestLogClient.SaveBuyingRequest(planId, jobId, unZipped, apiVersion, spotAllocationModelMode);
            }
            catch (Exception exception)
            {
                _LogError("Failed to save buying API request", exception);
            }
        }

        private void _HandleBuyingJobException(
            int jobId,
            BackgroundJobProcessingStatus status,
            Exception exception,
            string logMessage)
        {
            _PlanBuyingRepository.UpdatePlanBuyingJob(new PlanBuyingJob
            {
                Id = jobId,
                Status = status,
                DiagnosticResult = exception.ToString(),
                ErrorMessage = logMessage,
                Completed = _DateTimeEngine.GetCurrentMoment()
            });

            _LogError($"Error attempting to run the buying model : {exception.Message}", exception);
        }

        private void _HandleBuyingJobError(
            int jobId,
            BackgroundJobProcessingStatus status,
            string errorMessages)
        {
            _PlanBuyingRepository.UpdatePlanBuyingJob(new PlanBuyingJob
            {
                Id = jobId,
                Status = status,
                ErrorMessage = errorMessages,
                Completed = _DateTimeEngine.GetCurrentMoment()
            });

            _LogError($"Buying model run ended with errors : {errorMessages}");
        }

        internal decimal _CalculateBuyingCpm(List<PlanBuyingAllocatedSpot> spots, ProprietaryInventoryData proprietaryInventoryData)
        {
            var totalCost = proprietaryInventoryData.TotalCostWithMargin;
            var totalImpressions = proprietaryInventoryData.TotalImpressions;

            if (spots.Any())
            {
                var allocatedTotalCost = spots.Sum(x => x.TotalCostWithMargin);
                var allocatedTotalImpressions = spots.Sum(x => x.TotalImpressions);

                totalCost += allocatedTotalCost;
                totalImpressions += allocatedTotalImpressions;
            }

            var cpm = ProposalMath.CalculateCpm(totalCost, totalImpressions);

            return cpm;
        }

        private List<int> _GetInventorySourceIdsByTypes(IEnumerable<InventorySourceTypeEnum> inventorySourceTypes)
        {
            return _InventoryRepository
                .GetInventorySources()
                .Where(x => inventorySourceTypes.Contains(x.InventoryType))
                .Select(x => x.Id)
                .ToList();
        }

        internal List<InventorySourceTypeEnum> _GetSupportedInventorySourceTypes()
        {
            var result = new List<InventorySourceTypeEnum>();

            if (_IsPricingModelOpenMarketInventoryEnabled.Value)
                result.Add(InventorySourceTypeEnum.OpenMarket);

            if (_IsPricingModelBarterInventoryEnabled.Value)
                result.Add(InventorySourceTypeEnum.Barter);

            if (_IsPricingModelProprietaryOAndOInventoryEnabled.Value)
                result.Add(InventorySourceTypeEnum.ProprietaryOAndO);

            return result;
        }

        private void _ValidateInventory(List<PlanBuyingInventoryProgram> inventory)
        {
            if (!inventory.Any())
            {
                throw new CadentException("No inventory found for buying run");
            }
        }

        internal MappedBuyingResultSpots _MapToResultSpotsV2(
            PlanBuyingApiSpotsResponseDto apiSpotsResults,
            PlanBuyingApiRequestDto buyingApiRequest,
            List<PlanBuyingInventoryProgram> inventoryPrograms,
            PlanBuyingParametersDto parameters,
            List<InventorySpotMapping> mappings)
        {
            var result = new MappedBuyingResultSpots();

            var standardDaypartById = _StandardDaypartRepository
                .GetAllStandardDayparts()
                .ToDictionary(x => x.Id, x => x);

            foreach (var mappedSpot in mappings)
            {
                // this has calculated information to keep and is exactly what was sent.
                var originalSpot = buyingApiRequest.Spots.FirstOrDefault(x =>
                    x.Id == mappedSpot.SentManifestId &&
                    x.MediaWeekId == mappedSpot.SentMediaWeekId);

                var program = inventoryPrograms.Single(x => x.ManifestId == mappedSpot.MappedManifestId);
                var inventoryWeek = program.ManifestWeeks.Single(x => x.InventoryMediaWeekId == mappedSpot.MappedMediaWeekId);

                // was it allocated?
                var allocation = apiSpotsResults.Results.FirstOrDefault(x =>
                    x.ManifestId == mappedSpot.SentManifestId &&
                    x.MediaWeekId == mappedSpot.SentMediaWeekId);

                var frequency = allocation?.Frequency ?? 0;

                var spotResult = new PlanBuyingAllocatedSpot
                {
                    Id = originalSpot.Id,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            SpotLengthId = program.ManifestRates.Single().SpotLengthId,
                            SpotCost = originalSpot.Cost,
                            SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(originalSpot.Cost, parameters.Margin),
                            Spots = frequency,
                            Impressions = originalSpot.Impressions
                        }
                    },
                    StandardDaypart = standardDaypartById[originalSpot.DaypartId],
                    Impressions30sec = originalSpot.Impressions,
                    ContractMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(inventoryWeek.ContractMediaWeekId),
                    InventoryMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(inventoryWeek.InventoryMediaWeekId)
                };

                if (frequency > 0)
                {
                    result.Allocated.Add(spotResult);
                }
                else
                {
                    result.Unallocated.Add(spotResult);
                }
            }
            return result;
        }

        internal MappedBuyingResultSpots _MapToResultSpotsV3(
            PlanBuyingApiSpotsResponseDto_v3 apiSpotsResults,
            PlanBuyingApiRequestDto_v3 buyingApiRequest,
            List<PlanBuyingInventoryProgram> inventoryPrograms,
            PlanBuyingParametersDto parameters,
            PlanDto plan,
            List<InventorySpotMapping> mappings)
        {
            var results = new MappedBuyingResultSpots();

            var standardDaypartById = _StandardDaypartRepository
                .GetAllStandardDayparts()
                .ToDictionary(x => x.Id, x => x);
            var spotScaleFactorBySpotLengthId = _GetSpotScaleFactorBySpotLengthId(plan);

            // Iterate through the mappings to account for everything that was sent.
            foreach (var mappedSpot in mappings)
            {
                // this has calculated information to keep and is exactly what was sent.
                var originalSpot = buyingApiRequest.Spots.FirstOrDefault(x =>
                                x.Id == mappedSpot.SentManifestId &&
                                x.MediaWeekId == mappedSpot.SentMediaWeekId);

                var spotCostBySpotLengthId = originalSpot.SpotCost.ToDictionary(x => x.SpotLengthId, x => x.SpotLengthCost);

                // was it allocated?
                var allocation = apiSpotsResults.Results.FirstOrDefault(x =>
                    x.ManifestId == mappedSpot.SentManifestId &&
                    x.MediaWeekId == mappedSpot.SentMediaWeekId);

                // differentiate status at spot length level
                // if they are the same status then keep them together.
                List<SpotFrequency> allocatedFrequencies;
                List<SpotFrequency> unallocatedFrequencies;

                if (allocation == null)
                {
                    allocatedFrequencies = new List<SpotFrequency>();
                    unallocatedFrequencies = originalSpot.SpotCost.Select(s => new SpotFrequency { SpotLengthId = s.SpotLengthId, Spots = 0 }).ToList();
                }
                else
                {
                    allocatedFrequencies = allocation.Frequencies.Where(a => a.Frequency > 0)
                        .Select(s => new SpotFrequency { SpotLengthId = s.SpotLengthId, Spots = s.Frequency }).ToList();
                    unallocatedFrequencies = allocation.Frequencies.Where(a => a.Frequency <= 0)
                        .Select(s => new SpotFrequency { SpotLengthId = s.SpotLengthId, Spots = 0 }).ToList();
                }

                if (allocatedFrequencies.Any())
                {
                    var spotResult = new PlanBuyingAllocatedSpot
                    {
                        Id = mappedSpot.MappedManifestId,
                        SpotFrequencies = allocatedFrequencies
                            .Select(x => new SpotFrequency
                            {
                                SpotLengthId = x.SpotLengthId,
                                SpotCost = spotCostBySpotLengthId[x.SpotLengthId],
                                SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(spotCostBySpotLengthId[x.SpotLengthId], parameters.Margin),
                                Spots = x.Spots,
                                Impressions = originalSpot.Impressions30sec * spotScaleFactorBySpotLengthId[x.SpotLengthId]
                            })
                            .ToList(),
                        StandardDaypart = standardDaypartById[originalSpot.DaypartId],
                        Impressions30sec = originalSpot.Impressions30sec,
                        ContractMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(mappedSpot.SentMediaWeekId),
                        InventoryMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(mappedSpot.MappedMediaWeekId)
                    };
                    results.Allocated.Add(spotResult);
                }

                if (unallocatedFrequencies.Any())
                {
                    var spotResult = new PlanBuyingAllocatedSpot
                    {
                        Id = mappedSpot.MappedManifestId,
                        SpotFrequencies = unallocatedFrequencies
                            .Select(x => new SpotFrequency
                            {
                                SpotLengthId = x.SpotLengthId,
                                SpotCost = spotCostBySpotLengthId[x.SpotLengthId],
                                SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(spotCostBySpotLengthId[x.SpotLengthId], parameters.Margin),
                                Spots = x.Spots,
                                Impressions = originalSpot.Impressions30sec * spotScaleFactorBySpotLengthId[x.SpotLengthId]
                            })
                            .ToList(),
                        StandardDaypart = standardDaypartById[originalSpot.DaypartId],
                        Impressions30sec = originalSpot.Impressions30sec,
                        ContractMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(mappedSpot.SentMediaWeekId),
                        InventoryMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(mappedSpot.MappedMediaWeekId)
                    };
                    results.Unallocated.Add(spotResult);
                }
            }

            return results;
        }

        public void _ValidateAllocationResult(PlanBuyingAllocationResult apiResponse)
        {
            if (!string.IsNullOrEmpty(apiResponse.RequestId) && !apiResponse.AllocatedSpots.Any())
            {
                var msg = $"The api returned no spots for request '{apiResponse.RequestId}'.";
                throw new CadentException(msg);
            }
        }

        public PlanBuyingApiRequestDto_v3 GetBuyingApiRequestPrograms_v3(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            var diagnostic = new PlanBuyingJobDiagnostic();
            var buyingParams = new ProgramInventoryOptionalParametersDto
            {
                MinCPM = requestParameters.MinCpm,
                MaxCPM = requestParameters.MaxCpm,
                InflationFactor = requestParameters.InflationFactor,
                MarketGroup = requestParameters.MarketGroup,
                HUTBookId = requestParameters.HUTBookId,
                ShareBookId = requestParameters.ShareBookId
            };
            var parameters = new PlanBuyingParametersDto
            {
                MarketGroup = requestParameters.MarketGroup,
                Margin = requestParameters.Margin
            };

            var plan = _PlanRepository.GetPlan(planId);
            var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());

            var inventory = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                plan,
                buyingParams,
                inventorySourceIds,
                diagnostic);
            var groupedInventory = _GroupInventory(inventory);

            var buyingApiRequest = new PlanBuyingApiRequestDto_v3
            {
                Weeks = _GetBuyingModelWeeks_v3(plan, parameters, new ProprietaryInventoryData(), out List<int> skippedWeeksIds, SpotAllocationModelMode.Efficiency),
                Spots = _GetBuyingModelSpots_v3(groupedInventory, skippedWeeksIds).Spots
            };

            return buyingApiRequest;
        }

        public List<PlanBuyingInventoryProgram> GetBuyingInventory(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            var diagnostic = new PlanBuyingJobDiagnostic();
            var plan = _PlanRepository.GetPlan(planId);
            var buyingParams = new ProgramInventoryOptionalParametersDto
            {
                MinCPM = requestParameters.MinCpm,
                MaxCPM = requestParameters.MaxCpm,
                InflationFactor = requestParameters.InflationFactor,
                MarketGroup = requestParameters.MarketGroup,
                HUTBookId = requestParameters.HUTBookId,
                ShareBookId = requestParameters.ShareBookId
            };
            var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());

            var inventory = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                plan,
                buyingParams,
                inventorySourceIds,
                diagnostic);

            return inventory;
        }

        public string ForceCompletePlanBuyingJob(int jobId, string username)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);
            job.Status = BackgroundJobProcessingStatus.Failed;
            job.ErrorMessage = $"Job status set to error by user '{username}'.";
            job.Completed = _DateTimeEngine.GetCurrentMoment();
            _PlanBuyingRepository.UpdatePlanBuyingJob(job);

            return $"Job Id '{jobId}' has been forced to complete.";
        }

        public PlanBuyingResultProgramsDto GetPrograms(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null)
        {
            PlanBuyingResultProgramsDto results = new PlanBuyingResultProgramsDto();
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            postingType = _ResolvePostingType(planId, postingType);
            var stationProgramResult = _PlanBuyingRepository.GetBuyingProgramsResultByJobId_V2(job.Id, postingType, spotAllocationModelMode);
            if (stationProgramResult == null)
                return null;
            stationProgramResult.Details = stationProgramResult.Details.Select(w => { w.RepFirm = w.RepFirm ?? w.LegacyCallLetters; w.OwnerName = w.OwnerName ?? w.LegacyCallLetters; return w; }).ToList();

            if (planBuyingFilter != null)
            {
                if ((planBuyingFilter.RepFirmNames?.Any() ?? false) && (planBuyingFilter.OwnerNames?.Any() ?? false))
                {
                    stationProgramResult.Details = stationProgramResult.Details.Where(x => planBuyingFilter.RepFirmNames.Contains(x.RepFirm) && planBuyingFilter.OwnerNames.Contains(x.OwnerName)).ToList();
                }
                else if (planBuyingFilter.RepFirmNames?.Any() ?? false)
                {
                    stationProgramResult.Details = stationProgramResult.Details.Where(x => planBuyingFilter.RepFirmNames.Contains(x.RepFirm)).ToList();
                }
                else if (planBuyingFilter.OwnerNames?.Any() ?? false)
                {
                    stationProgramResult.Details = stationProgramResult.Details.Where(x => planBuyingFilter.OwnerNames.Contains(x.OwnerName)).ToList();
                }
            }
            results = _PlanBuyingProgramEngine.GetAggregatedProgramStations(stationProgramResult);
            results.Details = results.Details.OrderByDescending(p => p.ImpressionsPercentage)
                                              .ThenByDescending(p => p.ProgramName)
                                              .ToList();
            if (results == null)
                return null;

            _PlanBuyingProgramEngine.ConvertImpressionsToUserFormat(results);

            return results;
        }

        /// <inheritdoc />
        public PlanBuyingBandsDto GetBuyingBands(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null)
        {
            PlanBuyingBandsDto planBuyingBands = null;

            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
            {
                return null;
            }

            postingType = _ResolvePostingType(planId, postingType);
            var latestParametersForPlanBuyingJob = _PlanBuyingRepository.GetLatestParametersForPlanBuyingJob(job.Id);

            var buyingApiResults = _PlanBuyingRepository.GetBuyingApiResultsByJobId(job.Id, spotAllocationModelMode, (PostingTypeEnum)postingType);
            if (buyingApiResults == null)
            {
                return null;
            }
            buyingApiResults.AllocatedSpots.ForEach(allocatedSpot =>
            {
                allocatedSpot.RepFirm = allocatedSpot.RepFirm ?? allocatedSpot.LegacyCallLetters;
                allocatedSpot.OwnerName = allocatedSpot.OwnerName ?? allocatedSpot.LegacyCallLetters;
            });

            var planBuyingBandInventoryStations = new PlanBuyingBandInventoryStationsDto();
            try
            {
                planBuyingBandInventoryStations = _PlanBuyingRepository.GetPlanBuyingBandInventoryStations(job.Id);
            }
            catch (Exception exception)
            {
                _LogError($"No Bands Inventory data found. If happened before May 2022 then the feature hasn't been released and this can be ignored. If after then look into this", exception);
                return null;
            }

            if (planBuyingBandInventoryStations == null)
            {
                return null;
            }

            planBuyingBandInventoryStations.Details.ForEach(planBuyingBandInventoryStation =>
            {
                planBuyingBandInventoryStation.RepFirm = planBuyingBandInventoryStation.RepFirm ?? planBuyingBandInventoryStation.LegacyCallLetters;
                planBuyingBandInventoryStation.OwnerName = planBuyingBandInventoryStation.OwnerName ?? planBuyingBandInventoryStation.LegacyCallLetters;
            });

            if (planBuyingFilter != null)
            {
                if ((planBuyingFilter.RepFirmNames?.Any() ?? false) && (planBuyingFilter.OwnerNames?.Any() ?? false))
                {
                    buyingApiResults.AllocatedSpots = buyingApiResults.AllocatedSpots.Where(x => planBuyingFilter.RepFirmNames.Contains(x.RepFirm) && planBuyingFilter.OwnerNames.Contains(x.OwnerName)).ToList();
                    planBuyingBandInventoryStations.Details = planBuyingBandInventoryStations.Details.Where(x => planBuyingFilter.RepFirmNames.Contains(x.RepFirm) && planBuyingFilter.OwnerNames.Contains(x.OwnerName)).ToList();
                }
                else if (planBuyingFilter.RepFirmNames?.Any() ?? false)
                {
                    buyingApiResults.AllocatedSpots = buyingApiResults.AllocatedSpots.Where(x => planBuyingFilter.RepFirmNames.Contains(x.RepFirm)).ToList();
                    planBuyingBandInventoryStations.Details = planBuyingBandInventoryStations.Details.Where(x => planBuyingFilter.RepFirmNames.Contains(x.RepFirm)).ToList();
                }
                else if (planBuyingFilter.OwnerNames?.Any() ?? false)
                {
                    buyingApiResults.AllocatedSpots = buyingApiResults.AllocatedSpots.Where(x => planBuyingFilter.OwnerNames.Contains(x.OwnerName)).ToList();
                    planBuyingBandInventoryStations.Details = planBuyingBandInventoryStations.Details.Where(x => planBuyingFilter.OwnerNames.Contains(x.OwnerName)).ToList();
                }
            }

            if (planBuyingBandInventoryStations.PostingType != postingType)
            {
                foreach (var details in planBuyingBandInventoryStations.Details)
                {
                    var temp = PostingTypeConversionHelper.ConvertImpressions(details.Impressions,
                        planBuyingBandInventoryStations.PostingType, details.PostingTypeConversionRate);
                    details.Impressions = temp;
                }
            }

            planBuyingBands = _PlanBuyingBandCalculationEngine.Calculate(planBuyingBandInventoryStations, buyingApiResults, latestParametersForPlanBuyingJob);
            planBuyingBands?.Details.OrderBy(p => p.MinBand).ToList();
            if (planBuyingBands == null)
            {
                return null;
            }
            _PlanBuyingBandCalculationEngine.ConvertImpressionsToUserFormat(planBuyingBands);
            return planBuyingBands;
        }

        /// <inheritdoc />
        public PlanBuyingResultMarketsDto GetMarkets(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null)
        {
            bool isConversionRequired = false;
            var stationResult = GetStations(planId, postingType, spotAllocationModelMode, planBuyingFilter, isConversionRequired);
            if (stationResult == null)
            {
                return null;
            }
            var marketCoverages = _MarketCoverageRepository.GetMarketsWithLatestCoverage();
            var plan = _PlanRepository.GetPlan(planId);

            var aggregatedResult = _PlanBuyingMarketResultsEngine.CalculateAggregatedResultOfMarket(stationResult, marketCoverages, plan);
            _PlanBuyingMarketResultsEngine.ConvertImpressionsToUserFormat(aggregatedResult);
            return aggregatedResult;
        }

        /// <inheritdoc />
        public PlanBuyingStationResultDto GetStations(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null, bool isConversionRequired = true)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            postingType = _ResolvePostingType(planId, postingType);

            var result = _PlanBuyingRepository.GetBuyingStationsResultByJobId(job.Id, postingType, spotAllocationModelMode);
            if (result == null)
                return null;

            result.Details = result.Details.Select(w => { w.RepFirm = w.RepFirm ?? w.LegacyCallLetters; w.OwnerName = w.OwnerName ?? w.LegacyCallLetters; return w; }).ToList();

            if (planBuyingFilter != null)
            {
                if ((planBuyingFilter.RepFirmNames?.Any() ?? false) && (planBuyingFilter.OwnerNames?.Any() ?? false))
                {
                    result.Details = result.Details.Where(x => planBuyingFilter.RepFirmNames.Contains(x.RepFirm) && planBuyingFilter.OwnerNames.Contains(x.OwnerName)).ToList();
                }
                else if (planBuyingFilter.RepFirmNames?.Any() ?? false)
                {
                    result.Details = result.Details.Where(x => planBuyingFilter.RepFirmNames.Contains(x.RepFirm)).ToList();
                }
                else if (planBuyingFilter.OwnerNames?.Any() ?? false)
                {
                    result.Details = result.Details.Where(x => planBuyingFilter.OwnerNames.Contains(x.OwnerName)).ToList();
                }
            }

            var aggStationResult = _PlanBuyingStationCalculationEngine.CalculateAggregateOfStations(result);
            if (isConversionRequired)
            {
                _PlanBuyingStationCalculationEngine.ConvertImpressionsToUserFormat(aggStationResult);
            }
            return aggStationResult;
        }

        /// <inheritdoc />
        public PlanBuyingResultOwnershipGroupDto GetBuyingOwnershipGroups(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null)
        {
            bool isConversionRequired = false;
            var result = GetStations(planId, postingType, spotAllocationModelMode, planBuyingFilter, isConversionRequired);

            if (result == null)
            {
                return null;
            }

            var aggResult = _PlanBuyingOwnershipGroupEngine.CalculateAggregateOfOwnershipGroup(result);
            _PlanBuyingOwnershipGroupEngine.ConvertImpressionsToUserFormat(aggResult);
            return aggResult;
        }

        /// <inheritdoc />
        public PlanBuyingResultRepFirmDto GetBuyingRepFirms(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, PlanBuyingFilterDto planBuyingFilter = null)
        {
            bool isConversionRequired = false;
            var result = GetStations(planId, postingType, spotAllocationModelMode, planBuyingFilter, isConversionRequired);

            if (result == null)
            {
                return null;
            }

            var aggResult = _PlanBuyingRepFirmEngine.CalculateAggregateOfRepFirm(result);
            _PlanBuyingRepFirmEngine.ConvertImpressionsToUserFormat(aggResult);
            return aggResult;
        }

        internal int _GetPricingModelVersion()
        {
            return 4;
        }
        public Guid GenerateProgramLineupReport(ProgramLineupReportRequest request, string userName, DateTime currentDate, string templatesFilePath)
        {
            bool isProgramLineupAllocationByAffiliateEnabled = _IsProgramLineupAllocationByAffiliateEnabled.Value;
            var programLineupReportData = GetProgramLineupReportData(request, currentDate, isProgramLineupAllocationByAffiliateEnabled);
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

        public ProgramLineupReportData GetProgramLineupReportData(ProgramLineupReportRequest request, DateTime currentDate, bool isProgramLineupAllocationByAffiliateEnabled = false)
        {
            if (request.SelectedPlans.IsEmpty())
                throw new CadentException("Choose at least one plan");

            // for now we generate reports only for one plan
            var planId = request.SelectedPlans.First();
            _ValidatePlanLocking(planId);
            var buyingJob = _GetLatestBuyingJob(planId);
            var plan = _PlanRepository.GetPlan(planId);
            _ValidateCampaignLocking(plan.CampaignId);
            var campaign = _CampaignRepository.GetCampaign(plan.CampaignId);
            var agency = _GetAgency(campaign);
            var advertiser = _GetAdvertiser(campaign);
            var guaranteedDemo = _AudienceService.GetAudienceById(plan.AudienceId);
            var spotLengths = _SpotLengthRepository.GetSpotLengths();
            var allocatedOpenMarketSpots = _PlanBuyingRepository.GetPlanBuyingAllocatedSpotsByPlanId(planId, request.PostingType, request.SpotAllocationModelMode);
            var proprietaryInventory = _PlanBuyingRepository.GetProprietaryInventoryForBuyingProgramLineup(plan.BuyingParameters.JobId.Value);
            _SetSpotLengthIdAndCalculateImpressions(plan, proprietaryInventory, spotLengths);
            _LoadDaypartData(proprietaryInventory);
            var manifestIdsOpenMarket = allocatedOpenMarketSpots.Select(x => x.StationInventoryManifestId).Distinct();
            var manifestsOpenMarket = _InventoryRepository.GetStationInventoryManifestsByIds(manifestIdsOpenMarket)
                .Where(x => x.Station != null && x.Station.MarketCode.HasValue)
                .ToList();
            var marketCoverages = _MarketCoverageRepository.GetLatestMarketCoveragesWithStations();
            var manifestDaypartIds = manifestsOpenMarket.SelectMany(x => x.ManifestDayparts).Select(x => x.Id.Value).Distinct();
            var primaryProgramsByManifestDaypartIds = _StationProgramRepository.GetPrimaryProgramsForManifestDayparts(manifestDaypartIds);

            var postingType = request.PostingType ?? plan.PostingType;
            var spotAllocationModelMode = request.SpotAllocationModelMode ?? SpotAllocationModelMode.Efficiency;
            var result = new ProgramLineupReportData(
                plan,
                buyingJob,
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

        private PlanBuyingJob _GetLatestBuyingJob(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            if (job == null)
                throw new CadentException("There are no completed buying runs for the chosen plan. Please run buying");

            if (job.Status == BackgroundJobProcessingStatus.Failed)
                throw new CadentException("The latest buying run was failed. Please run buying again or contact the support");

            if (job.Status == BackgroundJobProcessingStatus.Queued || job.Status == BackgroundJobProcessingStatus.Processing)
                throw new CadentException("There is a buying run in progress right now. Please wait until it is completed");

            return job;
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

        private AgencyDto _GetAgency(CampaignDto campaign)
        {
            var result = _AabEngine.GetAgency(campaign.AgencyMasterId.Value);

            return result;
        }

        private AdvertiserDto _GetAdvertiser(CampaignDto campaign)
        {
            var result = _AabEngine.GetAdvertiser(campaign.AdvertiserMasterId.Value);

            return result;
        }
        private PostingTypeEnum? _ResolvePostingType(int planId, PostingTypeEnum? postingType)
        {
            if (!postingType.HasValue)
            {
                var plan = _PlanRepository.GetPlan(planId);
                postingType = plan.PostingType;
                return postingType;
            }
            else
            {
                return postingType;
            }
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

        internal class ProgramWithManifestDaypart
        {
            public PlanBuyingInventoryProgram Program { get; set; }

            public BasePlanInventoryProgram.ManifestDaypart ManifestDaypart { get; set; }
        }

        internal class InventorySpotMapping
        {
            public int SentManifestId { get; set; }
            public int SentMediaWeekId { get; set; }
            public int MappedManifestId { get; set; }
            public int MappedMediaWeekId { get; set; }
        }

        internal class SpotsAndMappings
        {
            public List<PlanBuyingApiRequestSpotsDto> Spots { get; set; } = new List<PlanBuyingApiRequestSpotsDto>();
            public List<InventorySpotMapping> Mappings { get; set; } = new List<InventorySpotMapping>();
        }

        internal class SpotsAndMappingsV3
        {
            public List<PlanBuyingApiRequestSpotsDto_v3> Spots { get; set; } = new List<PlanBuyingApiRequestSpotsDto_v3>();
            public List<InventorySpotMapping> Mappings { get; set; } = new List<InventorySpotMapping>();
        }

        internal class MappedBuyingResultSpots
        {
            public List<PlanBuyingAllocatedSpot> Allocated { get; set; } = new List<PlanBuyingAllocatedSpot>();
            public List<PlanBuyingAllocatedSpot> Unallocated { get; set; } = new List<PlanBuyingAllocatedSpot>();
        }

        private class ProgramDaypartWeekGroupItem
        {
            public int ContractedInventoryId { get; set; }
            public int ContractedMediaWeekId { get; set; }
            public int InventoryDaypartId { get; set; }
            public PlanBuyingApiRequestSpotsDto Spot { get; set; }
            public int ProgramMinimumContractMediaWeekId { get; set; }
            public InventorySpotMapping Mapping { get; set; }
        }

        private class ProgramDaypartWeekGroupItem_V3
        {
            public int ContractedInventoryId { get; set; }
            public int ContractedMediaWeekId { get; set; }
            public int InventoryDaypartId { get; set; }
            public PlanBuyingApiRequestSpotsDto_v3 Spot { get; set; }
            public int ProgramMinimumContractMediaWeekId { get; set; }
            public InventorySpotMapping Mapping { get; set; }
        }

        public PlanBuyingParametersDto GetPlanBuyingGoals(int planId, PostingTypeEnum postingType)
        {
            var plan = _PlanRepository.GetPlan(planId);
            if (plan.BuyingParameters == null)
            {
                _SetPlanBuyingParameters(plan);
            }

            if (plan.BuyingParameters.PostingType != postingType)
            {
                var ntiToNsiConversionRate = _PlanRepository.GetNsiToNtiConversionRate(plan.Dayparts);
                plan.BuyingParameters = _ConvertPlanBuyingParametersToRequestedPostingType(plan.BuyingParameters, ntiToNsiConversionRate);
            }
            return plan.BuyingParameters;
        }

        private void _SetPlanBuyingParameters(PlanDto plan)
        {
            var buyingDefaults = GetPlanBuyingDefaults();
            plan.BuyingParameters = new PlanBuyingParametersDto
            {
                PlanId = plan.Id,
                Budget = Convert.ToDecimal(plan.Budget),
                CPM = Convert.ToDecimal(plan.TargetCPM),
                CPP = Convert.ToDecimal(plan.TargetCPP),
                Currency = plan.Currency,
                DeliveryImpressions = Convert.ToDouble(plan.TargetImpressions) / 1000,
                DeliveryRatingPoints = Convert.ToDouble(plan.TargetRatingPoints),
                UnitCaps = buyingDefaults.UnitCaps,
                UnitCapsType = buyingDefaults.UnitCapsType,
                Margin = buyingDefaults.Margin,
                PlanVersionId = plan.VersionId,
                MarketGroup = buyingDefaults.MarketGroup,
                PostingType = plan.PostingType,
                ShareBookId = plan.ShareBookId,
                HUTBookId = plan.HUTBookId,
                FluidityPercentage = plan.FluidityPercentage
            };

            ValidateAndApplyMargin(plan.BuyingParameters);
        }

        private PlanBuyingParametersDto _ConvertPlanBuyingParametersToRequestedPostingType(PlanBuyingParametersDto planBuyingParameters, double ntiToNsiConversionRate)
        {
            if (planBuyingParameters.PostingType == PostingTypeEnum.NSI)
            {
                planBuyingParameters.DeliveryImpressions = Math.Floor(planBuyingParameters.DeliveryImpressions * ntiToNsiConversionRate);
                planBuyingParameters.PostingType = PostingTypeEnum.NTI;
            }
            else if (planBuyingParameters.PostingType == PostingTypeEnum.NTI)
            {
                planBuyingParameters.DeliveryImpressions = Math.Floor(planBuyingParameters.DeliveryImpressions / ntiToNsiConversionRate);
                planBuyingParameters.PostingType = PostingTypeEnum.NSI;
            }

            if (planBuyingParameters.DeliveryImpressions != 0)
            {
                planBuyingParameters.CPM = (decimal)((double)planBuyingParameters.Budget / planBuyingParameters.DeliveryImpressions);
            }
            else
            {
                planBuyingParameters.CPM = 0;
            }
            return planBuyingParameters;
        }

        internal void _HandleMissingSpotCosts(List<int> planSpotLengthIds, PlanBuyingApiRequestDto_v3 request)
        {
            const decimal missingLengthCost = 100000m;
            var spotLengthCount = planSpotLengthIds.Count;
            var missingSpotCosts = request.Spots.Where(s => s.SpotCost.Count != spotLengthCount).ToList();

            foreach (var missingSpot in missingSpotCosts)
            {
                // which one is missing?
                var accountedFor = missingSpot.SpotCost.Select(s => s.SpotLengthId).ToList();
                var missingLengths = planSpotLengthIds.Except(accountedFor).ToList();

                foreach (var missingLength in missingLengths)
                {
                    // add it with a high cost (decimal.MaxValue)
                    var toAdd = new SpotCost_v3
                    {
                        SpotLengthId = missingLength,
                        SpotLengthCost = missingLengthCost
                    };
                    missingSpot.SpotCost.Add(toAdd);
                }
            }

            // Verify the request was filled in.
            var afterCount = request.Spots.Count(s => s.SpotCost.Count != spotLengthCount);
            if (afterCount > 0)
            {
                throw new CadentException($"Unable to fill the inventory spot costs for sending to the data model.  {afterCount} inventory costs still missing.");
            }
        }
        public List<string> GetResultRepFirms(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            postingType = _ResolvePostingType(planId, postingType);

            var stations = GetStations(planId, postingType, spotAllocationModelMode);

            if (stations == null)
                return new List<string>();

            var result = stations.Details.Select(x => x.RepFirm.ToString()).Distinct().ToList();

            result.OrderByDescending(x => x).ToList();

            return result;
        }

        public List<string> GetResultOwnershipGroups(int planId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            postingType = _ResolvePostingType(planId, postingType);

            var stations = GetStations(planId, postingType, spotAllocationModelMode);

            if (stations == null)
                return new List<string>();

            var result = stations.Details.Select(x => x.OwnerName).Distinct().OrderByDescending(x => x).ToList();

            return result;
        }

        public void UsePlanForBuyingJob(PlanBuyingParametersDto planBuyingParametersDto, PlanDto plan)
        {
            planBuyingParametersDto.Budget = plan.Budget.Value;
            planBuyingParametersDto.CPM = plan.TargetCPM.Value;
            planBuyingParametersDto.CPP = plan.TargetCPP.Value;
            planBuyingParametersDto.Currency = plan.Currency;
            planBuyingParametersDto.DeliveryImpressions = plan.TargetImpressions.Value / 1000;
            planBuyingParametersDto.DeliveryRatingPoints = plan.TargetRatingPoints.Value;
            planBuyingParametersDto.PostingType = plan.PostingType;
        }

        public bool DeleteSavedBuyingData()
        {
            var result = _PlanBuyingRepository.DeleteSavedBuyingData();
            return result;
        }

        public Guid GenerateBuyingResultsReportAndSave(PlanBuyingResultsReportRequest planBuyingResultsReportRequest, string templateFilePath, string createdBy)
        {
            if (planBuyingResultsReportRequest.SpotAllocationModelMode != SpotAllocationModelMode.Efficiency && planBuyingResultsReportRequest.SpotAllocationModelMode != SpotAllocationModelMode.Floor)
            {
                throw new CadentException($"No results were found for the Spot Allocation Model Mode {planBuyingResultsReportRequest.SpotAllocationModelMode}");
            }
            var reportData = GetBuyingResultsReportData(planBuyingResultsReportRequest.PlanId, null, planBuyingResultsReportRequest.SpotAllocationModelMode, PostingTypeEnum.NSI);
            var reportGenerator = new BuyingResultsReportGenerator(templateFilePath);

            _LogInfo($"Starting to generate the file '{reportData.ExportFileName}'....");

            var report = reportGenerator.Generate(reportData);
            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.BUYING_RESULTS_REPORT),
                FileNameWithExtension = report.Filename,
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.PricingResultsReport,
                CreatedDate = _DateTimeEngine.GetCurrentMoment(),
                CreatedBy = createdBy,
                FileContent = report.Stream
            };

            var savedFileGuid = _SharedFolderService.SaveFile(sharedFolderFile);

            _LogInfo($"Saved file '{reportData.ExportFileName}' with guid '{savedFileGuid}'");

            return savedFileGuid;
        }
    }
}
