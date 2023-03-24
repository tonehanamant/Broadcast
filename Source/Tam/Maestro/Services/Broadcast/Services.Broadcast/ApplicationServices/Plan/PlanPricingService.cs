using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.PlanPricing;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.PricingResults;
using Services.Broadcast.ReportGenerators.Quote;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Newtonsoft.Json;
using Services.Broadcast.Converters.Scx;

namespace Services.Broadcast.ApplicationServices.Plan
{
    public interface IPlanPricingService : IApplicationService
    {        
        PlanPricingJob QueuePricingJob(PlanPricingParametersDto planPricingParametersDto, DateTime currentDate, string username);
        PlanPricingJob QueuePricingJob(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto, DateTime currentDate, string username);
        CurrentPricingExecution GetCurrentPricingExecution(int planId);
        CurrentPricingExecution GetCurrentPricingExecution(int planId, int? planVersionId);
        CurrentPricingExecutions GetAllCurrentPricingExecutions(int planId, int? planVersionId);
        CurrentPricingExecution GetCurrentPricingExecutionByJobId(int jobId);
        /// <summary>
        /// Cancels the current pricing execution.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>The PlanPricingResponseDto object</returns>
        PlanPricingResponseDto CancelCurrentPricingExecution(int planId);
        /// <summary>
        /// Cancels the current pricing execution.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>The PlanPricingResponseDto object</returns>
        PlanPricingResponseDto CancelCurrentPricingExecutionByJobId(int jobId);
        [Queue("planpricing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        Task RunPricingJobAsync(PlanPricingParametersDto planPricingParametersDto, int jobId, CancellationToken token);
        [Queue("planpricing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        Task RunPricingWithoutPlanJobAsync(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto, int jobId, CancellationToken token);
        /// <summary>
        /// For troubleshooting
        /// </summary>
        List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId);
        
        PricingProgramsResultDto_v2 GetPrograms_v2(int planId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        PlanPricingApiRequestDto_v3 GetPricingApiRequestPrograms_v3(int planId, PricingInventoryGetRequestParametersDto requestParameters);
        /// <summary>
        /// For troubleshooting
        /// </summary>
        List<PlanPricingInventoryProgram> GetPricingInventory(int planId, PricingInventoryGetRequestParametersDto requestParameters);
        PricingProgramsResultDto_v2 GetProgramsForVersion_v2(int planId, int planVersionId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetUnitCaps();
        /// <summary>
        /// Gets the pricing market groups.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetPricingMarketGroups();
        PlanPricingDefaults GetPlanPricingDefaults();
        bool IsPricingModelRunningForPlan(int planId);
        bool IsPricingModelRunningForJob(int jobId);

        /// <summary>
        /// Gets the latest pricing job.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanPricingJob _GetLatestPricingJob(int planId);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        string ForceCompletePlanPricingJob(int jobId, string username);

        /// <summary>
        /// For troubleshooting.  This will bypass the queue to allow rerunning directly.
        /// </summary>
        /// <param name="jobId">The id of the job to rerun.</param>
        /// <returns>The new JobId</returns>
        Task<int> ReRunPricingJobAsync(int jobId);

        /// <summary>
        /// For troubleshooting.  Generate a pricing results report for the chosen plan
        /// </summary>
        /// <param name="planId">The plan id</param>
        /// <param name="spotAllocationModelMode">The Spot Allocation Model Mode</param>
        /// <param name="templatesFilePath">Base path of the file templates</param>
        /// <returns>ReportOutput which contains filename and MemoryStream which actually contains report data</returns>
        ReportOutput GeneratePricingResultsReport(int planId,SpotAllocationModelMode spotAllocationModelMode, string templatesFilePath);

        /// <summary>
        /// Generates the pricing results report, saves it then returns a Guid for retreiving the file.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="templatesFilePath">The templates file path.</param>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        Guid GeneratePricingResultsReportAndSave(PlanPricingResultsReportRequest request,
            string templatesFilePath, string username);

        void ValidateAndApplyMargin(PlanPricingParametersDto parameters);
        PricingProgramsResultDto GetPrograms(int planId);
        PricingProgramsResultDto GetProgramsByJobId(int jobId);
        PricingProgramsResultDto GetProgramsForVersion(int planId, int planVersionId);
        PlanPricingStationResultDto GetStations(int planId);
        PlanPricingStationResultDto GetStationsByJobId(int jobId);
        PlanPricingStationResultDto GetStationsForVersion(int planId, int planVersionId);
        /// <summary>
        /// Retrieves the Pricing Results Markets Summary
        /// </summary>
        PlanPricingResultMarketsDto GetMarkets(int planId);
        /// <summary>
        /// Retrieves the Pricing Results Markets Summary
        /// </summary>
        PlanPricingResultMarketsDto GetMarketsByJobId(int jobId);
        PlanPricingResultMarketsDto_v2 GetMarketsByJobId_v2(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);
        PlanPricingResultMarketsDto GetMarketsForVersion(int planId, int planVersionId);
        PlanPricingBandDto GetPricingBands(int planId);
        PlanPricingBandDto GetPricingBandsByJobId(int jobId);
        PlanPricingBandDto GetPricingBandsForVersion(int planId, int planVersionId);
        [Queue("savepricingrequest")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto pricingApiRequest, string apiVersion, SpotAllocationModelMode spotAllocationModelMode);
        [Queue("savepricingrequest")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto_v3 pricingApiRequest, string apiVersion, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Exports the plan pricing SCX.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="username">The username.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="postingType">Type of the posting.</param>
        /// <returns></returns>
        Guid ExportPlanPricingScx(int planId, string username, SpotAllocationModelMode spotAllocationModelMode,
            PostingTypeEnum postingType);

        Guid RunQuote(QuoteRequestDto request, string userName, string templatesFilePath);

        PlanPricingStationResultDto_v2 GetStations_v2(int planId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);
        PlanPricingStationResultDto_v2 GetStationsForVersion_v2(int planId, int planVersionId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        PlanPricingResultMarketsDto_v2 GetMarkets_v2(int planId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);
        PlanPricingResultMarketsDto_v2 GetMarketsForVersion_v2(int planId, int planVersionId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        PlanPricingBandDto_v2 GetPricingBandsForVersion_v2(int planId, int planVersionId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        PlanPricingBandDto_v2 GetPricingBands_v2(int planId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        /// <summary>
        /// Tests the repository query for getting the Inventory Programs.
        /// Query dimensions are configured per the given Job Id.
        /// </summary>
        string TestGetProgramsForPricingModel(int jobId);
    }

    public class PlanPricingService : BroadcastBaseClass, IPlanPricingService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanPricingInventoryEngine _PlanPricingInventoryEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IPricingApiClient _PricingApiClient;
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
        private readonly IPlanPricingBandCalculationEngine _PlanPricingBandCalculationEngine;
        private readonly IPlanPricingStationCalculationEngine _PlanPricingStationCalculationEngine;
        private readonly IPlanPricingMarketResultsEngine _PlanPricingMarketResultsEngine;
        private readonly IPlanPricingProgramCalculationEngine _PlanPricingProgramCalculationEngine;
        private readonly IPricingRequestLogClient _PricingRequestLogClient;
        private readonly IPlanPricingScxDataPrep _PlanPricingScxDataPrep;
        private readonly IPlanPricingScxDataConverter _PlanPricingScxDataConverter;
        private readonly IPlanValidator _PlanValidator;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IAudienceService _AudienceService;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;
        private readonly IInventoryProprietarySummaryRepository _InventoryProprietarySummaryRepository;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IAsyncTaskHelper _AsyncTaskHelper;       
        private readonly Lazy<bool> _IsPricingModelBarterInventoryEnabled;
        private readonly Lazy<bool> _IsPricingModelProprietaryOAndOInventoryEnabled;
        private readonly Lazy<bool> _IsParallelPricingEnabled;

        public PlanPricingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                  ISpotLengthEngine spotLengthEngine,
                                  IPricingApiClient pricingApiClient,
                                  IBackgroundJobClient backgroundJobClient,
                                  IPlanPricingInventoryEngine planPricingInventoryEngine,
                                  IBroadcastLockingManagerApplicationService lockingManagerApplicationService,
                                  IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                  IDateTimeEngine dateTimeEngine,
                                  IWeeklyBreakdownEngine weeklyBreakdownEngine,
                                  IPlanPricingBandCalculationEngine planPricingBandCalculationEngine,
                                  IPlanPricingStationCalculationEngine planPricingStationCalculationEngine,
                                  IPlanPricingMarketResultsEngine planPricingMarketResultsEngine,
                                  IPlanPricingProgramCalculationEngine planPricingProgramCalculationEngine,
                                  IPricingRequestLogClient pricingRequestLogClient,
                                  IPlanPricingScxDataPrep planPricingScxDataPrep,
                                  IPlanPricingScxDataConverter planPricingScxDataConverter,
                                  IPlanValidator planValidator,
                                  ISharedFolderService sharedFolderService,
                                  IAudienceService audienceService,
                                  ICreativeLengthEngine creativeLengthEngine,                                  
                                  IAsyncTaskHelper asyncTaskHelper,
                                  IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _CampaignRepository = broadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _SpotLengthEngine = spotLengthEngine;
            _PricingApiClient = pricingApiClient;
            _BackgroundJobClient = backgroundJobClient;
            _PlanPricingInventoryEngine = planPricingInventoryEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _MarketRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>();
            _DateTimeEngine = dateTimeEngine;
            _WeeklyBreakdownEngine = weeklyBreakdownEngine;
            _PlanPricingBandCalculationEngine = planPricingBandCalculationEngine;
            _PlanPricingStationCalculationEngine = planPricingStationCalculationEngine;
            _PlanPricingMarketResultsEngine = planPricingMarketResultsEngine;
            _PlanPricingProgramCalculationEngine = planPricingProgramCalculationEngine;
            _PricingRequestLogClient = pricingRequestLogClient;
            _PlanPricingScxDataPrep = planPricingScxDataPrep;
            _PlanPricingScxDataConverter = planPricingScxDataConverter;
            _PlanValidator = planValidator;
            _SharedFolderService = sharedFolderService;
            _AudienceService = audienceService;
            _CreativeLengthEngine = creativeLengthEngine;
            _InventoryProprietarySummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProprietarySummaryRepository>();
            _BroadcastAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _AsyncTaskHelper = asyncTaskHelper;            
            _IsPricingModelBarterInventoryEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.PRICING_MODEL_BARTER_INVENTORY));
            _IsPricingModelProprietaryOAndOInventoryEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY));
            _IsParallelPricingEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PARALLEL_PRICINGAPICLIENT_REQUESTS));
        }        

        public Guid RunQuote(QuoteRequestDto request, string userName, string templatesFilePath)
        {
            var quoteReportData = GetQuoteReportData(request);
            var reportGenerator = new QuoteReportGenerator(templatesFilePath);

            _LogInfo($"Starting to generate the file '{quoteReportData.ExportFileName}'....");
            var report = reportGenerator.Generate(quoteReportData);

            var savedFileGuid = _SharedFolderService.SaveFile(new SharedFolderFile
            {
                FolderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.QUOTE_REPORTS),
                FileNameWithExtension = report.Filename,
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.Quote,
                CreatedDate = _DateTimeEngine.GetCurrentMoment(),
                CreatedBy = userName,
                FileContent = report.Stream
            });

            _LogInfo($"Saved file '{quoteReportData.ExportFileName}' with guid '{savedFileGuid}'");

            return savedFileGuid;
        }

        internal QuoteReportData GetQuoteReportData(QuoteRequestDto request)
        {
            // used to tie the logging messages together.
            var processingId = Guid.NewGuid();

            // this should be removed in the story that allows user to enter margin
            const int defaultMargin = 20;
            request.Margin = defaultMargin;

            var generatedTimeStamp = _DateTimeEngine.GetCurrentMoment();
            var allAudiences = _AudienceService.GetAudiences();
            var allMarkets = _MarketCoverageRepository.GetMarketsWithLatestCoverage();

            _LogInfo("Starting to gather inventory...", processingId);
            var programs = _PlanPricingInventoryEngine.GetInventoryForQuote(request, processingId);
            _LogInfo($"Finished gather inventory.  Gathered {programs.Count} programs.", processingId);

            if (programs.Count == 0)
            {
                const string no_inventory_message = 
                    "There is no inventory available for the selected program restrictions in the requested quarter. " +
                    "Previous quarters might have available inventory that will be utilized when pricing is executed.";
                throw new CadentException(no_inventory_message);
            }

            var reportData = new QuoteReportData(request, generatedTimeStamp, allAudiences, allMarkets, programs);
            return reportData;
        }

        public Guid GeneratePricingResultsReportAndSave(PlanPricingResultsReportRequest request,
            string templatesFilePath, string username)
        {
            var reportData = GetPricingResultsReportData(request.PlanId, request.SpotAllocationModelMode, request.PostingType);
            var reportGenerator = new PricingResultsReportGenerator(templatesFilePath);

            _LogInfo($"Starting to generate the file '{reportData.ExportFileName}'....");

            var report = reportGenerator.Generate(reportData);

            var savedFileGuid = _SharedFolderService.SaveFile(new SharedFolderFile
            {
                FolderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.PRICING_RESULTS_REPORT),
                FileNameWithExtension = report.Filename,
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.PricingResultsReport,
                CreatedDate = _DateTimeEngine.GetCurrentMoment(),
                CreatedBy = username,
                FileContent = report.Stream
            });

            _LogInfo($"Saved file '{reportData.ExportFileName}' with guid '{savedFileGuid}'");

            return savedFileGuid;
        }

        /// <inheritdoc/>
        public ReportOutput GeneratePricingResultsReport(int planId,SpotAllocationModelMode spotAllocationModelMode, string templatesFilePath)
        {
            var reportData = GetPricingResultsReportData(planId, spotAllocationModelMode);
            var reportGenerator = new PricingResultsReportGenerator(templatesFilePath);
            var report = reportGenerator.Generate(reportData);

            return report;
        }

        public PricingResultsReportData GetPricingResultsReportData(int planId,SpotAllocationModelMode spotAllocationModelMode, PostingTypeEnum? postingType = null)
        {
            var plan = _PlanRepository.GetPlan(planId);

            var reportPostingType = postingType ?? plan.PostingType;

            var allocatedSpots = _PlanRepository.GetPlanPricingAllocatedSpotsByPlanVersionId(planId,plan.VersionId, reportPostingType, spotAllocationModelMode);
            var manifestIds = allocatedSpots.Select(x => x.StationInventoryManifestId).Distinct();
            var manifests = _InventoryRepository.GetStationInventoryManifestsByIds(manifestIds);
            var manifestDaypartIds = manifests.SelectMany(x => x.ManifestDayparts).Select(x => x.Id.Value);
            var primaryProgramsByManifestDaypartIds = _StationProgramRepository.GetPrimaryProgramsForManifestDayparts(manifestDaypartIds);
            var markets = _MarketRepository.GetMarketDtos();

            return new PricingResultsReportData(
                plan,
                allocatedSpots,
                manifests,
                primaryProgramsByManifestDaypartIds,
                markets,
                _WeeklyBreakdownEngine,
                spotAllocationModelMode,
                reportPostingType);
        }

        public PlanPricingJob QueuePricingJob(PlanPricingParametersDto planPricingParametersDto
            , DateTime currentDate, string username)
        {
            // lock the plan so that two requests for the same plan can not get in this area concurrently
            var key = KeyHelper.GetPlanLockingKey(planPricingParametersDto.PlanId.Value);
            var lockObject = _LockingManagerApplicationService.GetNotUserBasedLockObjectForKey(key);

            lock (lockObject)
            {
                if (IsPricingModelRunningForPlan(planPricingParametersDto.PlanId.Value))
                {
                    throw new CadentException("The pricing model is already running for the plan");
                }

                var plan = _PlanRepository.GetPlan(planPricingParametersDto.PlanId.Value);

                ValidateAndApplyMargin(planPricingParametersDto);
                _ValidateDaypart(plan.Dayparts);
                int planVersionId;

                // For drafts, we use the plan version id sent as parameter.
                // This is because a draft is not considered the latest version of a plan.
                if (planPricingParametersDto.PlanVersionId.HasValue)
                    planVersionId = planPricingParametersDto.PlanVersionId.Value;
                else
                    planVersionId = plan.VersionId;

                if (plan.IsDraft == false && planVersionId != plan.VersionId)
                {
                    throw new CadentException("The current plan that you are viewing has been updated. Please close the plan and reopen in order to view the most current information");
                }

                _PlanValidator.ValidatePlanNotCrossQuartersForPricing(plan);

                var job = new PlanPricingJob
                {
                    PlanVersionId = planVersionId,
                    Status = BackgroundJobProcessingStatus.Queued,
                    Queued = currentDate
                };

                using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                {
                    planPricingParametersDto.PlanVersionId = planVersionId;
                    _SavePricingJobAndParameters(job, planPricingParametersDto);
                    _CampaignRepository.UpdateCampaignLastModified(plan.CampaignId, currentDate, username);
                    transaction.Complete();
                }

                job.HangfireJobId = _BackgroundJobClient.Enqueue<IPlanPricingService>(x => x.RunPricingJobAsync(planPricingParametersDto, job.Id, CancellationToken.None));

                _PlanRepository.UpdateJobHangfireId(job.Id, job.HangfireJobId);

                return job;
            }
        }

        public PlanPricingJob QueuePricingJob(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto
            , DateTime currentDate, string username)
        {
            if (pricingParametersWithoutPlanDto.JobId.HasValue && IsPricingModelRunningForJob(pricingParametersWithoutPlanDto.JobId.Value))
            {
                throw new CadentException("The pricing model is already running");
            }

            var pricingParameters = _ConvertPricingWihtoutPlanParametersToPlanPricingParameters(pricingParametersWithoutPlanDto);

            ValidateAndApplyMargin(pricingParameters);
            pricingParametersWithoutPlanDto.AdjustedBudget = pricingParameters.AdjustedBudget;
            pricingParametersWithoutPlanDto.AdjustedCPM = pricingParameters.AdjustedCPM;

            _ValidatePlan(pricingParametersWithoutPlanDto);

            var job = new PlanPricingJob
            {
                Status = BackgroundJobProcessingStatus.Queued,
                Queued = currentDate
            };
            using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
            {
                _SavePricingJobAndParameters(job, pricingParameters);
                pricingParametersWithoutPlanDto.JobId = job.Id;
                transaction.Complete();
            }

            job.HangfireJobId = _BackgroundJobClient.Enqueue<IPlanPricingService>(x => x.RunPricingWithoutPlanJobAsync(pricingParametersWithoutPlanDto, job.Id, CancellationToken.None));

            _PlanRepository.UpdateJobHangfireId(job.Id, job.HangfireJobId);

            return job;
        }

        private void _ValidatePlan(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto)
        {
            var plan = _ConvertPricingWihtoutPlanParametersToPlanDto(pricingParametersWithoutPlanDto);
            _PlanValidator.ValidatePlanForPricing(plan);
        }

        private PlanPricingParametersDto _ConvertPricingWihtoutPlanParametersToPlanPricingParameters(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto)
        {
            return new PlanPricingParametersDto
            {
                AdjustedBudget = pricingParametersWithoutPlanDto.AdjustedBudget,
                AdjustedCPM = pricingParametersWithoutPlanDto.AdjustedCPM,
                Budget = pricingParametersWithoutPlanDto.Budget,
                CompetitionFactor = pricingParametersWithoutPlanDto.CompetitionFactor,
                CPM = pricingParametersWithoutPlanDto.CPM,
                CPP = pricingParametersWithoutPlanDto.CPP,
                Currency = pricingParametersWithoutPlanDto.Currency,
                DeliveryImpressions = pricingParametersWithoutPlanDto.DeliveryImpressions,
                DeliveryRatingPoints = pricingParametersWithoutPlanDto.DeliveryRatingPoints,
                InflationFactor = pricingParametersWithoutPlanDto.InflationFactor,
                JobId = pricingParametersWithoutPlanDto.JobId,
                Margin = pricingParametersWithoutPlanDto.Margin,
                MarketGroup = pricingParametersWithoutPlanDto.MarketGroup,
                MaxCpm = pricingParametersWithoutPlanDto.MaxCpm,
                MinCpm = pricingParametersWithoutPlanDto.MinCpm,
                ProprietaryBlend = pricingParametersWithoutPlanDto.ProprietaryBlend,
                UnitCaps = pricingParametersWithoutPlanDto.UnitCaps,
                UnitCapsType = pricingParametersWithoutPlanDto.UnitCapsType,
                ProprietaryInventory = pricingParametersWithoutPlanDto.ProprietaryInventory
            };
        }

        private PlanDto _ConvertPricingWihtoutPlanParametersToPlanDto(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto)
        {
            var plan = new PlanDto
            {
                AudienceId = pricingParametersWithoutPlanDto.AudienceId,
                AvailableMarkets = pricingParametersWithoutPlanDto.AvailableMarkets,
                CoverageGoalPercent = pricingParametersWithoutPlanDto.CoverageGoalPercent,
                CreativeLengths = _CreativeLengthEngine.DistributeWeight(pricingParametersWithoutPlanDto.CreativeLengths),
                Dayparts = pricingParametersWithoutPlanDto.Dayparts,
                Equivalized = pricingParametersWithoutPlanDto.Equivalized,
                FlightDays = pricingParametersWithoutPlanDto.FlightDays,
                FlightEndDate = pricingParametersWithoutPlanDto.FlightEndDate,
                FlightHiatusDays = pricingParametersWithoutPlanDto.FlightHiatusDays,
                FlightStartDate = pricingParametersWithoutPlanDto.FlightStartDate,
                GoalBreakdownType = pricingParametersWithoutPlanDto.GoalBreakdownType,
                HUTBookId = pricingParametersWithoutPlanDto.HUTBookId,
                ImpressionsPerUnit = pricingParametersWithoutPlanDto.ImpressionsPerUnit,
                PostingType = pricingParametersWithoutPlanDto.PostingType,
                ShareBookId = pricingParametersWithoutPlanDto.ShareBookId,
                TargetRatingPoints = pricingParametersWithoutPlanDto.TargetRatingPoints,
                WeeklyBreakdownWeeks = pricingParametersWithoutPlanDto.WeeklyBreakdownWeeks,
                PricingParameters = _ConvertPricingWihtoutPlanParametersToPlanPricingParameters(pricingParametersWithoutPlanDto),
                TargetImpressions = pricingParametersWithoutPlanDto.DeliveryImpressions,
                Budget = pricingParametersWithoutPlanDto.Budget
            };

            return plan;
        }

        /// <inheritdoc/>
        public Guid ExportPlanPricingScx(int planId, string username, SpotAllocationModelMode spotAllocationModelMode,
            PostingTypeEnum postingType)
        {
            const string fileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var generated = _DateTimeEngine.GetCurrentMoment();

            var scxData = _PlanPricingScxDataPrep.GetScxData(planId, generated, spotAllocationModelMode, postingType);
            var scxFile = _PlanPricingScxDataConverter.ConvertData(scxData, spotAllocationModelMode);
            scxFile.spotAllocationModelMode = spotAllocationModelMode.ToString().Substring(0, 1);
            var sharedFile = new SharedFolderFile
            {
                FolderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.PLAN_PRICING_SCX),
                FileNameWithExtension = scxFile.FileName,
                FileMediaType = fileMediaType,
                FileUsage = SharedFolderFileUsage.PlanPricingScx,
                CreatedDate = generated,
                CreatedBy = username,
                FileContent = scxFile.ScxStream
            };
            var savedFileGuid = _SharedFolderService.SaveFile(sharedFile);

            return savedFileGuid;
        }        

        private int _SavePricingJobAndParameters(PlanPricingJob job, PlanPricingParametersDto planPricingParametersDto)
        {
            var jobId = _PlanRepository.AddPlanPricingJob(job);
            job.Id = jobId;
            planPricingParametersDto.JobId = jobId;
            _PlanRepository.SavePlanPricingParameters(planPricingParametersDto);

            return jobId;
        }

        public void ValidateAndApplyMargin(PlanPricingParametersDto parameters)
        {
            const double allowedMinValue = .01;
            const double allowedMaxValue = 100;

            if (parameters.Margin.HasValue && (parameters.Margin.Value > allowedMaxValue ||
                    parameters.Margin.Value < allowedMinValue))
            {
                throw new CadentException("A provided Margin value must be between .01% And 100%.");
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
        private void _ValidateDaypart(List<PlanDaypartDto> Dayparts)
        {
            Dayparts.RemoveAll(x => x.DaypartCodeId == 24);
            if (Dayparts.Count==0)
            {
                throw new CadentException("Only custom dayparts are selected hence no inventory found for pricing run");
               
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

        public List<LookupDto> GetPricingMarketGroups()
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

        public PlanPricingDefaults GetPlanPricingDefaults()
        {
            const float defaultMargin = 20;

            return new PlanPricingDefaults
            {
                UnitCaps = 1,
                UnitCapsType = UnitCapEnum.Per30Min,
                Margin = defaultMargin,
                MarketGroup = MarketGroupEnum.All
            };
        }

        public CurrentPricingExecution GetCurrentPricingExecutionByJobId(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);
            return _GetCurrentPricingExecution(job);
        }

        private CurrentPricingExecution _GetCurrentPricingExecution(PlanPricingJob job)
        {
            CurrentPricingExecutionResultDto pricingExecutionResult = null;
            _PricingRunmodelJobValidation(job);

            if (job != null && job.Status == BackgroundJobProcessingStatus.Succeeded)
            {
                pricingExecutionResult = _PlanRepository.GetPricingResultsByJobId(job.Id);

                if (pricingExecutionResult != null)
                {
                    _GetPricingExecutionResult(pricingExecutionResult);
                }
            }

            //pricingExecutionResult might be null when there is no pricing run for the latest version            
            return new CurrentPricingExecution
            {
                Job = job,
                Result = pricingExecutionResult ?? new CurrentPricingExecutionResultDto
                {
                    SpotAllocationModelMode = SpotAllocationModelMode.Quality
                },
                IsPricingModelRunning = IsPricingModelRunning(job)
            };
        }

        private void _GetPricingExecutionResult(CurrentPricingExecutionResultDto pricingExecutionResult)
        {
            pricingExecutionResult.Notes = pricingExecutionResult.GoalFulfilledByProprietary
                                    ? "Proprietary goals meet plan goals"
                                    : string.Empty;
            if (pricingExecutionResult.JobId.HasValue)
            {
                decimal goalCpm;

                goalCpm = _PlanRepository.GetGoalCpm(pricingExecutionResult.JobId.Value, pricingExecutionResult.PostingType, pricingExecutionResult.PlanVersionId);

                pricingExecutionResult.CalculatedVpvh = GetCalculatedDaypartVPVH(pricingExecutionResult.Id);
                pricingExecutionResult.CpmPercentage = CalculateCpmPercentage(pricingExecutionResult.OptimalCpm, goalCpm);
            }
        }
        internal double GetCalculatedDaypartVPVH(int PlanVersionPricingResultId)
        {
            double calculatedVpvhAvg = 0.0;
            List<PlanPricingResultsDaypartDto> planVersionPricingResultsDayparts = null;
            planVersionPricingResultsDayparts = _PlanRepository.GetPricingResultsDayparts(PlanVersionPricingResultId);
            if (planVersionPricingResultsDayparts != null && planVersionPricingResultsDayparts.Count > 0)
            {
                calculatedVpvhAvg = (from rows in planVersionPricingResultsDayparts
                                     select rows).Average(e => e.CalculatedVpvh);
            }

            return calculatedVpvhAvg;

        }
        private CurrentPricingExecutions _GetAllCurrentPricingExecutions(PlanPricingJob job)
        {
            List<CurrentPricingExecutionResultDto> pricingExecutionResults = null;
            _PricingRunmodelJobValidation(job);
            if (job != null && job.Status == BackgroundJobProcessingStatus.Succeeded)
            {
                pricingExecutionResults = _PlanRepository.GetAllPricingResultsByJobIds(job.Id);

                if (pricingExecutionResults != null)
                {
                    foreach (var pricingExecutionResult in pricingExecutionResults)
                    {
                        _GetPricingExecutionResult(pricingExecutionResult);
                    }
                }
            }
            //pricingExecutionResult might be null when there is no pricing run for the latest version 
            var result = new CurrentPricingExecutions
            {
                Job = job,
                Results = pricingExecutionResults ?? _GetDefaultPricingResultsList(),
                IsPricingModelRunning = IsPricingModelRunning(job)
            };

            if (job?.Status != BackgroundJobProcessingStatus.Succeeded)
            {
                return result;
            }

            var jobCompletedWithinLastFiveMinutes = _DidPricingJobCompleteWithinThreshold(job, thresholdMinutes: 5);
            if (jobCompletedWithinLastFiveMinutes)
            {
                // expecting 6 results
                // Model Modes = 3
                // PostingType = 2
                // 3 * 2 = 6 results
                const int expectedResultCount = 6;
                result = ValidatePricingExecutionResult(result, expectedResultCount);
            }
            else
            {
                var filledInResults = FillInMissingPricingResultsWithEmptyResults(result.Results);
                result.Results = filledInResults;
            }

            return result;
        }

        private List<CurrentPricingExecutionResultDto> _GetDefaultPricingResultsList()
        {
            var emptyList = new List<CurrentPricingExecutionResultDto>
            {
                new CurrentPricingExecutionResultDto() {SpotAllocationModelMode = SpotAllocationModelMode.Quality}
            };
            return emptyList;
        }

        private void _AddEmptyPricingResult(List<CurrentPricingExecutionResultDto> results, PostingTypeEnum postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            if (!results.Any(a => a.PostingType == postingType && a.SpotAllocationModelMode == spotAllocationModelMode))
            {
                results.Add(new CurrentPricingExecutionResultDto
                {
                    PostingType = postingType,
                    SpotAllocationModelMode = spotAllocationModelMode
                });
            }
        }

        internal List<CurrentPricingExecutionResultDto> FillInMissingPricingResultsWithEmptyResults(List<CurrentPricingExecutionResultDto> candidateResults)
        {
            // We only have to worry about the three use cases
            // 1) Neither toggle is enabled
            // 2) isPostingTypeToggleEnabled is enabled and isPricingEfficiencyModelEnabled is not enabled
            // 3) Both are enabled.
            var results = candidateResults.DeepCloneUsingSerialization();

            _AddEmptyPricingResult(results, PostingTypeEnum.NSI, SpotAllocationModelMode.Quality);
            _AddEmptyPricingResult(results, PostingTypeEnum.NTI, SpotAllocationModelMode.Quality);

            _AddEmptyPricingResult(results, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency);
            _AddEmptyPricingResult(results, PostingTypeEnum.NTI, SpotAllocationModelMode.Efficiency);

            _AddEmptyPricingResult(results, PostingTypeEnum.NSI, SpotAllocationModelMode.Floor);
            _AddEmptyPricingResult(results, PostingTypeEnum.NTI, SpotAllocationModelMode.Floor);

            return results;
        }

        internal bool _DidPricingJobCompleteWithinThreshold(PlanPricingJob job, int thresholdMinutes)
        {
            if (!job.Completed.HasValue)
            {
                return false;
            }

            DateTime thresholdMinutesAgo = _DateTimeEngine.GetCurrentMoment().AddMinutes(-1 * thresholdMinutes);
            var jobCompletedWithinLastFiveMinutes = job.Completed.Value >= thresholdMinutesAgo;
            return jobCompletedWithinLastFiveMinutes;
        }

        internal CurrentPricingExecutions ValidatePricingExecutionResult(CurrentPricingExecutions result, int expectedResult)
        {
            if (!result.IsPricingModelRunning && 
                result.Results.Count != expectedResult)
            {
                result.IsPricingModelRunning = true;
                result.Results = _GetDefaultPricingResultsList();
            }
            return result;
        }

        private static void _PricingRunmodelJobValidation(PlanPricingJob job)
        {
            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                //in case the error is comming from the Pricing Run model, the error message field will have better
                //message then the generic we construct here
                if (string.IsNullOrWhiteSpace(job.DiagnosticResult))
                    throw new CadentException(job.ErrorMessage);
                throw new CadentException(
                    "Error encountered while running Pricing Model, please contact a system administrator for help");
            }
        }

        public CurrentPricingExecution GetCurrentPricingExecution(int planId)
        {
            return GetCurrentPricingExecution(planId, null);
        }

        public CurrentPricingExecution GetCurrentPricingExecution(int planId, int? planVersionId)
        {
            PlanPricingJob job = _GetPricingJobForPlanAndLatestPlanVersion(planId, planVersionId);

            return _GetCurrentPricingExecution(job);
        }

        private PlanPricingJob _GetPricingJobForPlanAndLatestPlanVersion(int planId, int? planVersionId)
        {
            PlanPricingJob job;

            if (planVersionId.HasValue)
                job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId.Value);
            else
                job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);
            return job;
        }

        public CurrentPricingExecutions GetAllCurrentPricingExecutions(int planId, int? planVersionId)
        {
            PlanPricingJob job = _GetPricingJobForPlanAndLatestPlanVersion(planId, planVersionId);

            return _GetAllCurrentPricingExecutions(job);
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
        public PlanPricingResponseDto CancelCurrentPricingExecutionByJobId(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);

            return _CancelCurrentPricingExecution(job);
        }

        private PlanPricingResponseDto _CancelCurrentPricingExecution(PlanPricingJob job)
        {
            var cancelId = Guid.NewGuid();
            _LogInfo("Cancel operation started. Entered on CancelCurrentPricingExecution...", cancelId);
            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                throw new CadentException("Error encountered while running Pricing Model, please contact a system administrator for help");
            }

            if (!IsPricingModelRunning(job))
            {
                var jobCompletedWithinOneMinute = _DidPricingJobCompleteWithinThreshold(job, thresholdMinutes: 1);
                if (jobCompletedWithinOneMinute)
                {
                    // if we're here then we hit that time between finished and UI picked it up.
                    // but the user thinks they hit Cancel... 
                    // we want to tell them that the Cancel didn't take affect and the results are new.
                    _LogInfo("The user requested to Cancel, but the job just finished.");
                    throw new CadentException("While attempting to Cancel the job, it completed.");
                }

                throw new CadentException("Error encountered while canceling Pricing Model, process is not running");
            }

            if (!string.IsNullOrEmpty(job?.HangfireJobId))
            {
                try
                {
                    var deleteId = Guid.NewGuid();
                    _LogInfo("Cancel operation started. Entered on CancelCurrentPricingExecution...", deleteId);
                    _BackgroundJobClient.Delete(job.HangfireJobId);
                }
                catch (Exception ex)
                {
                    _LogError($"Exception caught attempting to delete hangfire job '{job.HangfireJobId}'.", ex);
                }
            }
            var cancelDBId = Guid.NewGuid();
            _LogInfo("Cancel DB operation started. Entered on CancelCurrentPricingExecution...", cancelDBId);
            job.Status = BackgroundJobProcessingStatus.Canceled;
            job.Completed = _DateTimeEngine.GetCurrentMoment();

            _PlanRepository.UpdatePlanPricingJob(job);

            return new PlanPricingResponseDto
            {
                Job = job,
                Result = new PlanPricingResultDto(),
                IsPricingModelRunning = false
            };
        }

        /// <inheritdoc />
        public PlanPricingResponseDto CancelCurrentPricingExecution(int planId)
        {
            var job = _GetLatestPricingJob(planId);

            return _CancelCurrentPricingExecution(job);
        }

        /// <inheritdoc />
        public static bool IsPricingModelRunning(PlanPricingJob job)
        {
            return job != null && (job.Status == BackgroundJobProcessingStatus.Queued || job.Status == BackgroundJobProcessingStatus.Processing);
        }

        /// <inheritdoc />
        public bool IsPricingModelRunningForJob(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);
            return IsPricingModelRunning(job);
        }

        /// <inheritdoc />
        public bool IsPricingModelRunningForPlan(int planId)
        {
            var job = _GetLatestPricingJob(planId);
            return IsPricingModelRunning(job);
        }

        public PlanPricingJob _GetLatestPricingJob(int planId)
        {
            var planDraftId = _PlanRepository.CheckIfDraftExists(planId);
            var isDraft = planDraftId != 0;
            PlanPricingJob job;

            if (isDraft)
            {
                job = _PlanRepository.GetPricingJobForPlanVersion(planDraftId);
            }
            else
            {
                job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);
            }

            return job;
        }

        public List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId)
        {
            return _PlanRepository.GetPlanPricingRuns(planId);
        }

        private void _ApplyPricingParametersAndProprietaryInventoryToPlanWeeks(
            PlanDto plan,
            ProprietaryInventoryData proprietaryInventoryData,
            out bool goalsFulfilledByProprietaryInventory)
        {
            var planImpressionsGoal = plan.PricingParameters.DeliveryImpressions * 1000;
            var totalImpressions = planImpressionsGoal - proprietaryInventoryData.TotalImpressions;
            var totalBudget = plan.PricingParameters.Budget - proprietaryInventoryData.TotalCost;

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
                plan.WeeklyBreakdownWeeks = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan, totalImpressions, totalBudget);
            }
        }

        private List<ShareOfVoice> _GetShareOfVoice(
            MarketCoverageDto topMarkets,
            IEnumerable<PlanAvailableMarketDto> marketsWithSov,
            ProprietaryInventoryData proprietaryInventoryData,
            double planImpressionsGoal,
            SpotAllocationModelMode spotAllocationModelMode)
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
            var impressionsPerMarket = proprietaryInventoryData.ProprietarySummaries
                .SelectMany(x => x.ProprietarySummaryByStations)
                .GroupBy(x => x.MarketCode)
                .ToDictionary(x => x.Key,
                              x => x.Sum(y => y.TotalImpressions));

            foreach (var market in topMarketsShareOfVoice)
            {
                if (impressionsPerMarket.TryGetValue((short)market.MarketCode, out var proprietaryImpressionsForMarket))
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

        private MarketCoverageDto _GetTopMarkets(MarketGroupEnum pricingMarketSovMinimum)
        {
            switch (pricingMarketSovMinimum)
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
        public async Task<int> ReRunPricingJobAsync(int jobId)
        {
            var originalJob = _PlanRepository.GetPlanPricingJob(jobId);
            // get the plan params
            var jobParams = _PlanRepository.GetLatestParametersForPlanPricingJob(jobId);

            // create the artifacts
            var newJob = new PlanPricingJob
            {
                PlanVersionId = originalJob.PlanVersionId,
                Status = BackgroundJobProcessingStatus.Queued,
                Queued = _DateTimeEngine.GetCurrentMoment()
            };
            var newJobId = _SavePricingJobAndParameters(newJob, jobParams);

            // call the job directly
            await RunPricingJobAsync(jobParams, newJobId, CancellationToken.None);

            return newJobId;
        }

        private async Task<List<PlanPricingAllocationResult>> _SendPricingRequestsAsync(int jobId, PlanDto plan, List<PlanPricingInventoryProgram> inventory,
            PlanPricingParametersDto planPricingParametersDto, ProprietaryInventoryData proprietaryInventoryData,
            CancellationToken token, bool goalsFulfilledByProprietaryInventory,
            PlanPricingJobDiagnostic diagnostic)
        {
            var results = new List<PlanPricingAllocationResult>();

            results.Add(new PlanPricingAllocationResult
            {
                Spots = new List<PlanPricingAllocatedSpot>(),
                JobId = jobId,
                PlanVersionId = plan.VersionId,
                PricingVersion = _GetPricingModelVersion().ToString(),
                PostingType = plan.PostingType,
                SpotAllocationModelMode = SpotAllocationModelMode.Quality
            });
                results.Add(new PlanPricingAllocationResult
                {
                    Spots = new List<PlanPricingAllocatedSpot>(),
                    JobId = jobId,
                    PlanVersionId = plan.VersionId,
                    PricingVersion = _GetPricingModelVersion().ToString(),
                    PostingType = plan.PostingType,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency
                });

                results.Add(new PlanPricingAllocationResult
                {
                    Spots = new List<PlanPricingAllocatedSpot>(),
                    JobId = jobId,
                    PlanVersionId = plan.VersionId,
                    PricingVersion = _GetPricingModelVersion().ToString(),
                    PostingType = plan.PostingType,
                    SpotAllocationModelMode = SpotAllocationModelMode.Floor
                });

            if (!goalsFulfilledByProprietaryInventory)
            {
                if (!_IsParallelPricingEnabled.Value)
                {
                    foreach (var allocationResult in results)
                    {
                        _LogInfo($"Preparing call to Pricing Model for Mode '{allocationResult.SpotAllocationModelMode}'.");
                        var pricingModelCallTimer = new Stopwatch();
                        pricingModelCallTimer.Start();
                        try
                        {
                            await _SendPricingRequest_v3Async(
                               allocationResult,
                               plan,
                               jobId,
                               inventory,
                               token,
                               diagnostic,
                               planPricingParametersDto,
                               proprietaryInventoryData);
                        }
                        finally
                        {
                            pricingModelCallTimer.Stop();
                            var duration = pricingModelCallTimer.ElapsedMilliseconds;

                            _LogInfo($"Completed call to Pricing Model for Mode '{allocationResult.SpotAllocationModelMode}'.  Duration : {duration}ms");
                        }
                    }
                }
                else
                {
                    var tasks = results.Select(async allocationResult =>
                    {
                        _LogInfo($"Preparing call to Pricing Model for Mode '{allocationResult.SpotAllocationModelMode}'.");
                        var pricingModelCallTimer = new Stopwatch();
                        pricingModelCallTimer.Start();

                        try
                        {
                            await _SendPricingRequest_v3Async(
                               allocationResult,
                               plan,
                               jobId,
                               inventory,
                               token,
                               diagnostic,
                               planPricingParametersDto,
                               proprietaryInventoryData);
                        }
                        finally
                        {
                            pricingModelCallTimer.Stop();
                            var duration = pricingModelCallTimer.ElapsedMilliseconds;

                            _LogInfo($"Completed call to Pricing Model for Mode '{allocationResult.SpotAllocationModelMode}'.  Duration : {duration}ms");
                        }
                    });
                    await Task.WhenAll(tasks);
                }
            }
            return results;
        }

        private async Task _RunPricingJobAsync(PlanPricingParametersDto planPricingParametersDto, PlanDto plan, int jobId, CancellationToken token)
        {
            // run pricing will not use custom daypart
            if (plan?.Dayparts.Any() ?? false)
            {
                plan.Dayparts = plan.Dayparts.Where(daypart => !EnumHelper.IsCustomDaypart(daypart.DaypartTypeId.GetDescriptionAttribute())).ToList();
            }

            // used to tie the logging messages together.
            var processingId = Guid.NewGuid();
            _LogInfo("Starting...", processingId);

            var diagnostic = new PlanPricingJobDiagnostic();
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_TOTAL_DURATION);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_PROCESSING);
            var planPricingJob = _PlanRepository.GetPlanPricingJob(jobId);
            planPricingJob.Status = BackgroundJobProcessingStatus.Processing;
            _PlanRepository.UpdatePlanPricingJob(planPricingJob);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_PROCESSING);

            try
            {
                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_FETCHING_PLAN_AND_PARAMETERS);

                var programInventoryParameters = new ProgramInventoryOptionalParametersDto
                {
                    MinCPM = planPricingParametersDto.MinCpm,
                    MaxCPM = planPricingParametersDto.MaxCpm,
                    InflationFactor = planPricingParametersDto.InflationFactor,
                    MarketGroup = planPricingParametersDto.MarketGroup
                };
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FETCHING_PLAN_AND_PARAMETERS);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_GATHERING_INVENTORY);
                var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());
                var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    programInventoryParameters,
                    inventorySourceIds,
                    diagnostic,
                    processingId);

                _ValidateInventory(inventory);

                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_GATHERING_INVENTORY);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PROPRIETARY_DATA);
                var proprietaryInventoryData = _CalculateProprietaryInventoryData(plan, planPricingParametersDto);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PROPRIETARY_DATA);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROPRIETARY_DATA);
                _ApplyPricingParametersAndProprietaryInventoryToPlanWeeks(plan, proprietaryInventoryData, out var goalsFulfilledByProprietaryInventory);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_APPLYING_PROPRIETARY_DATA);

                token.ThrowIfCancellationRequested();

                //Send it to the DS Model based on the plan posting type, as selected in the plan detail.
                var modelAllocationResults = await _SendPricingRequestsAsync(jobId, plan, inventory, planPricingParametersDto, proprietaryInventoryData,
                    token, goalsFulfilledByProprietaryInventory, diagnostic);

                token.ThrowIfCancellationRequested();
                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);
                foreach (var allocationResult in modelAllocationResults)
                {
                    _ValidateAllocationResult(allocationResult);
                }

                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);

                token.ThrowIfCancellationRequested();

                foreach (var allocationResult in modelAllocationResults)
                {
                    var aggregationTasks = new List<AggregationTask>();

                    var allocationResults = new Dictionary<PostingTypeEnum, PlanPricingAllocationResult>();

                    //Always loop the posting type that matches the plan first so that we convert from nti to nsi only one time
                    foreach (var targetPostingType in Enum.GetValues(typeof(PostingTypeEnum)).Cast<PostingTypeEnum>()
                        .OrderByDescending(x => x == plan.PostingType)) //order by desc to prioritize true values
                    {
                        List<PlanPricingInventoryProgram> postingTypeInventory;
                        PlanPricingAllocationResult postingAllocationResult;

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
                            _PlanPricingInventoryEngine.ConvertPostingType(targetPostingType, postingTypeInventory);

                            postingAllocationResult = allocationResult.DeepCloneUsingSerialization();
                            // override with our instance posting type.
                            postingAllocationResult.PostingType = targetPostingType;

                            _ValidateInventory(postingTypeInventory);
                            _MapAllocationResultsPostingType(postingAllocationResult, postingTypeInventory, targetPostingType,
                                plan.PostingType, out var nsiNtiConversionFactor);
                        }

                        allocationResults.Add(targetPostingType, postingAllocationResult);

                        diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_CPM);
                        postingAllocationResult.PricingCpm = _CalculatePricingCpm(postingAllocationResult.Spots, proprietaryInventoryData);
                        diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_CPM);

                        token.ThrowIfCancellationRequested();

                        var aggregateResultsTask = new Task<PlanPricingResultBaseDto>(() =>
                        {
                            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                            var aggregatedResults = _PlanPricingProgramCalculationEngine.CalculateProgramResults(postingTypeInventory, postingAllocationResult, goalsFulfilledByProprietaryInventory, proprietaryInventoryData, targetPostingType);
                            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                            return aggregatedResults;
                        });

                        aggregationTasks.Add(new AggregationTask(targetPostingType, PricingJobTaskNameEnum.AggregateResults, aggregateResultsTask));
                        aggregateResultsTask.Start();

                        var calculatePricingBandsTask = new Task<PlanPricingBand>(() =>
                        {
                            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_BANDS);
                            var pricingBands = _PlanPricingBandCalculationEngine.CalculatePricingBands(postingTypeInventory, postingAllocationResult, planPricingParametersDto, proprietaryInventoryData, targetPostingType);
                            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_BANDS);
                            return pricingBands;
                        });

                        aggregationTasks.Add(new AggregationTask(targetPostingType, PricingJobTaskNameEnum.CalculatePricingBands, calculatePricingBandsTask));
                        calculatePricingBandsTask.Start();

                        var calculatePricingStationsTask = new Task<PlanPricingStationResult>(() =>
                        {
                            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_STATIONS);
                            var pricingStations = _PlanPricingStationCalculationEngine.Calculate(postingTypeInventory, postingAllocationResult, planPricingParametersDto, proprietaryInventoryData, targetPostingType);
                            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_STATIONS);
                            return pricingStations;
                        });

                        aggregationTasks.Add(new AggregationTask(targetPostingType, PricingJobTaskNameEnum.CalculatePricingStations, calculatePricingStationsTask));
                        calculatePricingStationsTask.Start();

                        var aggregateMarketResultsTask = new Task<PlanPricingResultMarkets>(() =>
                        {
                            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                            var marketCoverages = _MarketCoverageRepository.GetMarketsWithLatestCoverage();
                            var pricingMarketResults = _PlanPricingMarketResultsEngine.Calculate(postingTypeInventory, postingAllocationResult, plan, marketCoverages, proprietaryInventoryData, targetPostingType);
                            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                            return pricingMarketResults;
                        });

                        aggregationTasks.Add(new AggregationTask(targetPostingType, PricingJobTaskNameEnum.AggregateMarketResults, aggregateMarketResultsTask));
                        aggregateMarketResultsTask.Start();
                    }

                    token.ThrowIfCancellationRequested();

                    //Wait for all tasks nti and nsi to finish
                    var allAggregationTasks = Task.WhenAll(aggregationTasks.Select(x => x.Task).ToArray());
                    allAggregationTasks.Wait();

                    using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                    {
                        _SavePricingArtifacts(allocationResults, aggregationTasks, diagnostic);
                        transaction.Complete();
                    }
                }

                //Finish up the job
                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);
                var pricingJob = _PlanRepository.GetPlanPricingJob(jobId);
                pricingJob.Status = BackgroundJobProcessingStatus.Succeeded;
                pricingJob.Completed = _DateTimeEngine.GetCurrentMoment();
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);

                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_TOTAL_DURATION);
                pricingJob.DiagnosticResult = diagnostic.ToString();

                _PlanRepository.UpdatePlanPricingJob(pricingJob);

            }
            catch (PricingModelException exception)
            {
                _HandlePricingJobError(jobId, BackgroundJobProcessingStatus.Failed, exception.Message, diagnostic);
            }
            catch (Exception exception) when (exception is ObjectDisposedException || exception is OperationCanceledException)
            {
                var cancelMessageId = Guid.NewGuid();
                _LogInfo("Cancel error messages in Db process start...", cancelMessageId);
                _HandlePricingJobException(jobId, BackgroundJobProcessingStatus.Canceled, exception, "Running the pricing model was canceled.", diagnostic);
            }
            catch (Exception exception)
            {
                _LogInfo(String.Format("Error attempting to run the pricing model.{0}", exception.Message.ToString()));
                _HandlePricingJobException(jobId, BackgroundJobProcessingStatus.Failed, exception, "Error attempting to run the pricing model.", diagnostic);
            }
        }

        internal void _SavePricingArtifacts(IDictionary<PostingTypeEnum, PlanPricingAllocationResult> allocationResults,
            List<AggregationTask> aggregationTasks,
            PlanPricingJobDiagnostic diagnostic)
        {


            foreach (var postingType in Enum.GetValues(typeof(PostingTypeEnum)).Cast<PostingTypeEnum>())
            {
                var postingAllocationResult = allocationResults[postingType];

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);
                _PlanRepository.SavePricingApiResults(postingAllocationResult);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);

                var calculatePricingBandTask = (Task<PlanPricingBand>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == PricingJobTaskNameEnum.CalculatePricingBands)
                    .Task;
                var pricingBandResult = calculatePricingBandTask.Result;
                pricingBandResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_BANDS);
                _PlanRepository.SavePlanPricingBands(pricingBandResult);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_BANDS);

                var aggregateTask = (Task<PlanPricingResultBaseDto>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == PricingJobTaskNameEnum.AggregateResults)
                    .Task;
                var pricingProgramsResult = aggregateTask.Result;
                pricingProgramsResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);
                _PlanRepository.SavePricingAggregateResults(pricingProgramsResult);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);

                var calculatePricingStationTask = (Task<PlanPricingStationResult>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == PricingJobTaskNameEnum.CalculatePricingStations)
                    .Task;
                var pricingStationResult = calculatePricingStationTask.Result;
                pricingStationResult.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_STATIONS);
                _PlanRepository.SavePlanPricingStations(pricingStationResult);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_STATIONS);

                var aggregateMarketResultsTask = (Task<PlanPricingResultMarkets>)aggregationTasks
                    .First(x => x.PostingType == postingType && x.TaskName == PricingJobTaskNameEnum.AggregateMarketResults)
                    .Task;
                var pricingMarketResults = aggregateMarketResultsTask.Result;
                pricingMarketResults.SpotAllocationModelMode = postingAllocationResult.SpotAllocationModelMode;
                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);
                _PlanRepository.SavePlanPricingMarketResults(pricingMarketResults);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);
            }
        }

        private void _MapAllocationResultsPostingType(PlanPricingAllocationResult allocationResult, List<PlanPricingInventoryProgram> inventory,
            PostingTypeEnum targetPostingType, PostingTypeEnum sourcePostingType, out double nsitoNtiConversionFactor)
        {
            nsitoNtiConversionFactor = 1; //default

            if (targetPostingType == sourcePostingType)
            {
                return;
            }

            var totalImpressions = allocationResult.Spots.SelectMany(x => x.SpotFrequencies)
                .Sum(x => x.Impressions);

            double weightedConversionFactorSum = 0;
            int conversionFactorCount = 0;

            foreach (var spot in allocationResult.Spots)
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

        private ProprietaryInventoryData _CalculateProprietaryInventoryData(
            PlanDto plan,
            PlanPricingParametersDto parameters)
        {
            var result = new ProprietaryInventoryData();

            var proprietarySummaryIds = parameters.ProprietaryInventory.Select(x => x.Id).ToList();
            var proprietarySummaries = _InventoryProprietarySummaryRepository.GetInventoryProprietarySummariesByIds(proprietarySummaryIds);

            // do not change the order
            result.ProprietarySummaries = _GroupProprietaryInventory(plan, proprietarySummaries);
            _AdjustProprietaryInventoryFor15SpotLength(plan, result);
            _CalculateProprietaryInventoryBasedOnActiveWeks(plan, result, parameters);
            _CalculateCPMForEachProprietaryInventoryUnit(result);

            return result;
        }

        private List<ProprietarySummary> _GroupProprietaryInventory(
            PlanDto plan,
            List<InventoryProprietaryQuarterSummaryDto> proprietarySummaries)
        {
            var ratingAudienceIds = new HashSet<int>(_BroadcastAudienceRepository
                .GetRatingsAudiencesByMaestroAudience(new List<int> { plan.AudienceId })
                .Select(am => am.rating_audience_id)
                .Distinct()
                .ToList());

            return proprietarySummaries
                .Select(summary => new ProprietarySummary
                {
                    ProprietarySummaryId = summary.Id,
                    ProgramName = summary.ProgramName,
                    Genre = summary.Genre,
                    ProprietarySummaryByStations = summary.SummaryByStationByAudience
                        .Where(x => ratingAudienceIds.Contains(x.AudienceId))
                        .GroupBy(x => x.StationId)
                        .Select(groupingByStation =>
                        {
                            var first = groupingByStation.First();

                            return new ProprietarySummaryByStation
                            {
                                StationId = first.StationId,
                                MarketCode = first.MarketCode,

                                // Number of spots and cost is the same for each audience within summary and station
                                SpotsPerWeek = first.SpotsPerWeek,
                                CostPerWeek = first.CostPerWeek,
                                ProprietarySummaryByAudiences = groupingByStation
                                    .Select(x => new ProprietarySummaryByAudience
                                    {
                                        AudienceId = x.AudienceId,
                                        ImpressionsPerWeek = x.Impressions
                                    })
                                    .ToList()
                            };
                        })
                        .ToList()
                })
                .ToList();
        }

        private void _CalculateCPMForEachProprietaryInventoryUnit(ProprietaryInventoryData proprietaryInventoryData)
        {
            foreach (var summary in proprietaryInventoryData.ProprietarySummaries)
            {
                summary.Cpm = ProposalMath.CalculateCpm(summary.TotalCostWithMargin, summary.TotalImpressions);
            }
        }

        private void _CalculateProprietaryInventoryBasedOnActiveWeks(
            PlanDto plan,
            ProprietaryInventoryData proprietaryInventoryData,
            PlanPricingParametersDto parameters)
        {
            // take only those weeks that have some goals
            var numberOfPlanWeeksWithGoals = plan.WeeklyBreakdownWeeks
                .GroupBy(x => x.MediaWeekId)
                .Count(x => x.Any(w => w.WeeklyImpressions > 0));

            // multiply proprietary inventory by the number of weeks with goals
            proprietaryInventoryData.ProprietarySummaries
                .SelectMany(x => x.ProprietarySummaryByStations).ForEach(x =>
                {
                    x.TotalSpots = x.SpotsPerWeek * numberOfPlanWeeksWithGoals;
                    x.TotalCost = x.CostPerWeek * numberOfPlanWeeksWithGoals;
                    x.TotalCostWithMargin = GeneralMath.CalculateCostWithMargin(x.TotalCost, parameters.Margin);

                    x.ProprietarySummaryByAudiences.ForEach(y => y.TotalImpressions = y.ImpressionsPerWeek * numberOfPlanWeeksWithGoals);
                });
        }

        private void _AdjustProprietaryInventoryFor15SpotLength(
            PlanDto plan,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            var spotLengthId15 = _SpotLengthEngine.GetSpotLengthIdByValue(15);
            var spotLengthId30 = _SpotLengthEngine.GetSpotLengthIdByValue(30);

            if (plan.CreativeLengths.Any(x => x.SpotLengthId == spotLengthId15) &&
                plan.CreativeLengths.All(x => x.SpotLengthId != spotLengthId30))
            {
                // If the plan is 15 spot length only use half cost and impressions for each unit
                proprietaryInventoryData.ProprietarySummaries
                    .SelectMany(x => x.ProprietarySummaryByStations)
                    .ForEach(x =>
                    {
                        x.CostPerWeek /= 2;
                        x.ProprietarySummaryByAudiences.ForEach(y => y.ImpressionsPerWeek /= 2);
                    });
            }
        }        

        internal void _HandleMissingSpotCosts(List<int> planSpotLengthIds, PlanPricingApiRequestDto_v3 request)
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

        private PlanPricingBudgetCpmLeverEnum _GetPlanPricingBudgetCpmLeverEnum(BudgetCpmLeverEnum budgetCpmLever)
        {
            switch (budgetCpmLever)
            {
                case BudgetCpmLeverEnum.Budget:
                    return PlanPricingBudgetCpmLeverEnum.budget;
                case BudgetCpmLeverEnum.Cpm:
                    return PlanPricingBudgetCpmLeverEnum.impressions;
                default:
                    return PlanPricingBudgetCpmLeverEnum.impressions;
            }
        }

        private async Task _SendPricingRequest_v3Async(
            PlanPricingAllocationResult allocationResult,
            PlanDto plan,
            int jobId,
            List<PlanPricingInventoryProgram> inventory,
            CancellationToken token,
            PlanPricingJobDiagnostic diagnostic,
            PlanPricingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            var apiVersion = _GetPricingModelVersion().ToString();
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var pricingModelWeeks = _GetPricingModelWeeks_v3(
                plan,
                parameters,
                proprietaryInventoryData,
                out List<int> skippedWeeksIds,
                allocationResult.SpotAllocationModelMode);

            var groupedInventory = _GroupInventory(inventory);
            var spots = _GetPricingModelSpots_v3(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var pricingConfiguration = new PlanPricingApiRequestConfigurationDto
            {
                BudgetCpmLever = _GetPlanPricingBudgetCpmLeverEnum(parameters.BudgetCpmLever)
            };

            token.ThrowIfCancellationRequested();

            var pricingApiRequest = new PlanPricingApiRequestDto_v3
            {
                Weeks = pricingModelWeeks,
                Spots = spots,
                Configuration = pricingConfiguration
            };

            var planSpotLengthIds = plan.CreativeLengths.Select(s => s.SpotLengthId).ToList();
            _HandleMissingSpotCosts(planSpotLengthIds, pricingApiRequest);

            _LogInfo($"Sending pricing model input to the S3 log bucket for model '{allocationResult.SpotAllocationModelMode}'.");
            _AsyncTaskHelper.TaskFireAndForget(() => SavePricingRequest(plan.Id, jobId, pricingApiRequest, apiVersion, allocationResult.SpotAllocationModelMode));

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALLING_API);
            var apiAllocationResult = await _PricingApiClient.GetPricingSpotsResultAsync(pricingApiRequest);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALLING_API);

            token.ThrowIfCancellationRequested();

            if (apiAllocationResult.Error != null)
            {
                var errorMessage = $@"Pricing Model Mode ('{allocationResult.SpotAllocationModelMode}') Request Id '{apiAllocationResult.RequestId}' returned the following error: {apiAllocationResult.Error.Name} 
                                -  {string.Join(",", apiAllocationResult.Error.Messages).Trim(',')}";
                throw new PricingModelException(errorMessage);
            }

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
            allocationResult.Spots = _MapToResultSpots(groupedInventory, apiAllocationResult, pricingApiRequest, inventory, parameters, plan);
            allocationResult.RequestId = apiAllocationResult.RequestId;
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
        }

        internal List<PlanPricingApiRequestSpotsDto_v3> _GetPricingModelSpots_v3(
            List<IGrouping<PlanPricingInventoryGroup, ProgramWithManifestDaypart>> groupedInventory,
            List<int> skippedWeeksIds)
        {
            var marketCoveragesByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var pricingModelSpots = new List<PlanPricingApiRequestSpotsDto_v3>();

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

                        var impressions = program.PostingTypeImpressions;

                        if (impressions <= 0)
                            continue;

                        //filter out skipped weeks
                        var spots = program.ManifestWeeks
                            .Where(x => !skippedWeeksIds.Contains(x.ContractMediaWeekId))
                            .Select(manifestWeek => new ProgramDaypartWeekGroupItem_V3
                            {
                                ContractedInventoryId = manifestId,
                                ContractedMediaWeekId = manifestWeek.ContractMediaWeekId,
                                InventoryDaypartId = programInventoryDaypartId,
                                ProgramMinimumContractMediaWeekId = programMinimumContractMediaWeekId,
                                Spot = new PlanPricingApiRequestSpotsDto_v3
                                {
                                    Id = manifestId,
                                    MediaWeekId = manifestWeek.ContractMediaWeekId,
                                    Impressions30sec = impressions,
                                    StationId = program.Station.Id,
                                    MarketCode = program.Station.MarketCode.Value,
                                    DaypartId = program.StandardDaypartId,
                                    PercentageOfUs = GeneralMath.ConvertPercentageToFraction(marketCoveragesByMarketCode[program.Station.MarketCode.Value]),
                                    SpotDays = daypart.Daypart.ActiveDays,
                                    SpotHours = daypart.Daypart.GetDurationPerDayInHours(),
                                    SpotCost = validSpotCosts
                                }
                            }).ToList();

                        programSpots.AddRange(spots);
                    }
                }

                var groupedProgramSpots = programSpots.GroupBy(i => new { i.ContractedInventoryId, i.ContractedMediaWeekId, i.InventoryDaypartId }).ToList();
                foreach (var group in groupedProgramSpots)
                {
                    if (group.Count() == 1)
                    {
                        pricingModelSpots.Add(group.First().Spot);
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
                        pricingModelSpots.Add(keptItem.Spot);
                    }
                }
            }

            return pricingModelSpots;
        }

        public async Task RunPricingWithoutPlanJobAsync(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto, int jobId, CancellationToken token)
        {
            var pricingParameters = _ConvertPricingWihtoutPlanParametersToPlanPricingParameters(pricingParametersWithoutPlanDto);
            var plan = _ConvertPricingWihtoutPlanParametersToPlanDto(pricingParametersWithoutPlanDto);

            await _RunPricingJobAsync(pricingParameters, plan, jobId, token);
        }

        public async Task RunPricingJobAsync(PlanPricingParametersDto planPricingParametersDto, int jobId, CancellationToken token)
        {
            var plan = _PlanRepository.GetPlan(planPricingParametersDto.PlanId.Value, planPricingParametersDto.PlanVersionId);
            await _RunPricingJobAsync(planPricingParametersDto, plan, jobId, token);
        }

        internal List<PlanPricingApiRequestWeekDto_v3> _GetPricingModelWeeks_v3(
            PlanDto plan,
            PlanPricingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData,
            out List<int> SkippedWeeksIds,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            SkippedWeeksIds = new List<int>();
            var pricingModelWeeks = new List<PlanPricingApiRequestWeekDto_v3>();
            var planImpressionsGoal = plan.PricingParameters.DeliveryImpressions * 1000;

            // send 0.001% if any unit is selected
            var marketCoverageGoal = parameters.ProprietaryInventory.IsEmpty() ? GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value) : 0.001;
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);

            var shareOfVoice = spotAllocationModelMode == SpotAllocationModelMode.Floor ?
                new List<ShareOfVoice>() : _GetShareOfVoice(topMarkets, marketsWithSov, proprietaryInventoryData, planImpressionsGoal, spotAllocationModelMode);

            var daypartsWithWeighting = plan.Dayparts.Where(x => x.WeightingGoalPercent.HasValue);
            var planPricingParameters = plan.PricingParameters;

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
                var fluidityImpressionGoal = planPricingParameters.FluidityPercentage.HasValue ? (double)(budgetResponseByModes.ImpressionGoal * (100 - planPricingParameters.FluidityPercentage)) / 100 : budgetResponseByModes.ImpressionGoal;
                var pricingWeek = new PlanPricingApiRequestWeekDto_v3
                {
                    MediaWeekId = mediaWeekId,
                    ImpressionGoal = fluidityImpressionGoal,
                    CpmGoal = budgetResponseByModes.CpmGoal,
                    MarketCoverageGoal = marketCoverageGoal,
                    FrequencyCap = FrequencyCapHelper.GetFrequencyCap(planPricingParameters.UnitCapsType, planPricingParameters.UnitCaps),
                    ShareOfVoice = shareOfVoice,
                    DaypartWeighting = _GetDaypartGoals(plan, mediaWeekId),
                    SpotLengths = _GetSpotLengthGoals(plan, mediaWeekId, spotScaleFactorBySpotLengthId)
                };

                pricingModelWeeks.Add(pricingWeek);
            }

            return pricingModelWeeks;
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

        private List<SpotLength_v3> _GetSpotLengthGoals(
            PlanDto plan,
            int mediaWeekId,
            Dictionary<int, double> spotScaleFactorBySpotLengthId)
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

        internal static List<IGrouping<PlanPricingInventoryGroup, ProgramWithManifestDaypart>> _GroupInventory(List<PlanPricingInventoryProgram> inventory)
        {
            var flattedProgramsWithDayparts = inventory
                .SelectMany(x => x.ManifestDayparts.Select(d => new ProgramWithManifestDaypart
                {
                    Program = x,
                    ManifestDaypart = d
                }));

            var grouped = flattedProgramsWithDayparts.GroupBy(x =>
                new PlanPricingInventoryGroup
                {
                    StationId = x.Program.Station.Id,
                    DaypartId = x.ManifestDaypart.Daypart.Id,
                    PrimaryProgramName = x.ManifestDaypart.PrimaryProgram.Name
                });

            return grouped.ToList();
        }

        public void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto pricingApiRequest, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            _LogInfo($"Saving the pricing API Request.  PlanId = '{planId}'.");
            try
            {
                string unZipped = JsonConvert.SerializeObject(pricingApiRequest, Formatting.Indented);
                _PricingRequestLogClient.SavePricingRequest(planId, jobId, unZipped, apiVersion, spotAllocationModelMode);
            }
            catch (Exception exception)
            {
                _LogError($"Failed to save pricing API request.  PlanId = '{planId}'.", exception);
            }
        }

        public void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto_v3 pricingApiRequest, string apiVersion, SpotAllocationModelMode spotAllocationModelMode)
        {
            _LogInfo($"Saving the pricing API Request.  PlanId = '{planId}'.");
            try
            {
                string unZipped = JsonConvert.SerializeObject(pricingApiRequest, Formatting.Indented);
                _PricingRequestLogClient.SavePricingRequest(planId, jobId, unZipped, apiVersion, spotAllocationModelMode);
            }
            catch (Exception exception)
            {
                _LogError("Failed to save pricing API request.  PlanId = '{planId}'.", exception);
            }
        }

        private void _HandlePricingJobException(
            int jobId,
            BackgroundJobProcessingStatus status,
            Exception exception,
            string logMessage,
            PlanPricingJobDiagnostic diagnostic)
        {
            _PlanRepository.UpdatePlanPricingJob(new PlanPricingJob
            {
                Id = jobId,
                Status = status,
                ErrorMessage = logMessage,
                Completed = _DateTimeEngine.GetCurrentMoment(),
                DiagnosticResult = $"{exception} : Diagnostic Results : {diagnostic}",
            });

            _LogError($"Error attempting to run the pricing model : {exception.Message}", exception);
        }

        private void _HandlePricingJobError(
            int jobId,
            BackgroundJobProcessingStatus status,
            string errorMessages,
            PlanPricingJobDiagnostic diagnostic)
        {
            _PlanRepository.UpdatePlanPricingJob(new PlanPricingJob
            {
                Id = jobId,
                Status = status,
                ErrorMessage = errorMessages,
                Completed = _DateTimeEngine.GetCurrentMoment(),
                DiagnosticResult = diagnostic.ToString()
            });

            _LogError($"Pricing model run ended with errors : {errorMessages}");
        }

        internal decimal _CalculatePricingCpm(List<PlanPricingAllocatedSpot> spots, ProprietaryInventoryData proprietaryInventoryData)
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
            
            result.Add(InventorySourceTypeEnum.OpenMarket);

            if (_IsPricingModelBarterInventoryEnabled.Value)
                result.Add(InventorySourceTypeEnum.Barter);

            if (_IsPricingModelProprietaryOAndOInventoryEnabled.Value)
                result.Add(InventorySourceTypeEnum.ProprietaryOAndO);

            return result;
        }

        private void _ValidateInventory(List<PlanPricingInventoryProgram> inventory)
        {
            if (!inventory.Any())
            {
                throw new CadentException("No inventory found for pricing run");
            }
        }

        private List<PlanPricingAllocatedSpot> _MapToResultSpots(
            List<IGrouping<PlanPricingInventoryGroup,
            ProgramWithManifestDaypart>> groupedInventory,
            PlanPricingApiSpotsResponseDto apiSpotsResults,
            PlanPricingApiRequestDto pricingApiRequest,
            List<PlanPricingInventoryProgram> inventoryPrograms,
            PlanPricingParametersDto parameters)
        {
            var results = new List<PlanPricingAllocatedSpot>();
            var standardDaypartsById = _StandardDaypartRepository
                .GetAllStandardDayparts()
                .ToDictionary(x => x.Id, x => x);

            foreach (var allocation in apiSpotsResults.Results)
            {
                var originalProgramGroup = groupedInventory
                    .FirstOrDefault(x => x.Any(y => y.Program.ManifestId == allocation.ManifestId));

                if (originalProgramGroup == null)
                    throw new CadentException("Couldn't find the program in grouped inventory");

                var originalProgram = originalProgramGroup
                    .FirstOrDefault(x => x.Program.ManifestWeeks.Select(y => y.ContractMediaWeekId).Contains(allocation.MediaWeekId));

                if (originalProgram == null)
                    throw new CadentException("Couldn't find the program and week combination from the allocation data");

                var originalSpot = pricingApiRequest.Spots.FirstOrDefault(x =>
                    x.Id == allocation.ManifestId &&
                    x.MediaWeekId == allocation.MediaWeekId);

                if (originalSpot == null)
                    throw new CadentException("Response from API contains manifest id not found in sent data");

                var program = inventoryPrograms.Single(x => x.ManifestId == originalProgram.Program.ManifestId);
                var inventoryWeek = program.ManifestWeeks.Single(x => x.ContractMediaWeekId == originalSpot.MediaWeekId);

                var spotResult = new PlanPricingAllocatedSpot
                {
                    Id = originalProgram.Program.ManifestId,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            SpotLengthId = program.ManifestRates.Single().SpotLengthId,
                            SpotCost = originalSpot.Cost,
                            SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(originalSpot.Cost, parameters.Margin),
                            Spots = allocation.Frequency,
                            Impressions = originalSpot.Impressions,
                        }
                    },
                    StandardDaypart = standardDaypartsById[originalSpot.DaypartId],
                    Impressions30sec = originalSpot.Impressions,
                    ContractMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(inventoryWeek.ContractMediaWeekId),
                    InventoryMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(inventoryWeek.InventoryMediaWeekId)
                };

                results.Add(spotResult);
            }

            return results;
        }

        private List<PlanPricingAllocatedSpot> _MapToResultSpots(
            List<IGrouping<PlanPricingInventoryGroup,
            ProgramWithManifestDaypart>> groupedInventory,
            PlanPricingApiSpotsResponseDto_v3 apiSpotsResults,
            PlanPricingApiRequestDto_v3 pricingApiRequest,
            List<PlanPricingInventoryProgram> inventoryPrograms,
            PlanPricingParametersDto parameters,
            PlanDto plan)
        {
            var results = new List<PlanPricingAllocatedSpot>();
            var standardDaypartsById = _StandardDaypartRepository
                .GetAllStandardDayparts()
                .ToDictionary(x => x.Id, x => x);
            var spotScaleFactorBySpotLengthId = _GetSpotScaleFactorBySpotLengthId(plan);

            foreach (var allocation in apiSpotsResults.Results)
            {
                var originalProgramGroup = groupedInventory
                    .FirstOrDefault(x => x.Any(y => y.Program.ManifestId == allocation.ManifestId));

                if (originalProgramGroup == null)
                    throw new CadentException("Couldn't find the program in grouped inventory");

                var originalProgram = originalProgramGroup
                    .FirstOrDefault(x => x.Program.ManifestWeeks.Select(y => y.ContractMediaWeekId).Contains(allocation.MediaWeekId));

                if (originalProgram == null)
                    throw new CadentException("Couldn't find the program and week combination from the allocation data");

                var originalSpot = pricingApiRequest.Spots.FirstOrDefault(x =>
                    x.Id == allocation.ManifestId &&
                    x.MediaWeekId == allocation.MediaWeekId);

                if (originalSpot == null)
                    throw new CadentException("Response from API contains manifest id not found in sent data");

                var program = inventoryPrograms.Single(x => x.ManifestId == originalProgram.Program.ManifestId);
                var inventoryWeek = program.ManifestWeeks.Single(x => x.ContractMediaWeekId == originalSpot.MediaWeekId);
                var spotCostBySpotLengthId = originalSpot.SpotCost.ToDictionary(x => x.SpotLengthId, x => x.SpotLengthCost);
                var frequencies = allocation.Frequencies.Where(x => x.Frequency > 0).ToList();

                var spotResult = new PlanPricingAllocatedSpot
                {
                    Id = originalProgram.Program.ManifestId,
                    SpotFrequencies = frequencies
                        .Select(x => new SpotFrequency
                        {
                            SpotLengthId = x.SpotLengthId,
                            SpotCost = spotCostBySpotLengthId[x.SpotLengthId],
                            SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(spotCostBySpotLengthId[x.SpotLengthId], parameters.Margin),
                            Spots = x.Frequency,
                            Impressions = originalSpot.Impressions30sec * spotScaleFactorBySpotLengthId[x.SpotLengthId]
                        })
                        .ToList(),
                    StandardDaypart = standardDaypartsById[originalSpot.DaypartId],
                    Impressions30sec = originalSpot.Impressions30sec,
                    ContractMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(inventoryWeek.ContractMediaWeekId),
                    InventoryMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(inventoryWeek.InventoryMediaWeekId),
                    ProjectedImpressions = program.ProjectedImpressions,
                    HouseholdProjectedImpressions = program.HouseholdProjectedImpressions
                };

                results.Add(spotResult);
            }

            return results;
        }

        public void _ValidateAllocationResult(PlanPricingAllocationResult apiResponse)
        {
            if (!string.IsNullOrEmpty(apiResponse.RequestId) && !apiResponse.Spots.Any())
            {
                var msg = $"The api returned no spots for request '{apiResponse.RequestId}'.";
                throw new CadentException(msg);
            }
        }

        private void _SetPlanPricingParameters(PlanDto plan)
        {
            var pricingDefaults = GetPlanPricingDefaults();

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
            };

            ValidateAndApplyMargin(plan.PricingParameters);
        }

        public PlanPricingApiRequestDto_v3 GetPricingApiRequestPrograms_v3(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            // used to tie the logging messages together.
            var processingId = Guid.NewGuid();
            _LogInfo("Starting...", processingId);

            var diagnostic = new PlanPricingJobDiagnostic();
            var pricingParams = new ProgramInventoryOptionalParametersDto
            {
                MinCPM = requestParameters.MinCpm,
                MaxCPM = requestParameters.MaxCpm,
                InflationFactor = requestParameters.InflationFactor,
                MarketGroup = requestParameters.MarketGroup
            };
            var parameters = new PlanPricingParametersDto
            {
                MarketGroup = requestParameters.MarketGroup,
                Margin = requestParameters.Margin
            };

            var plan = _PlanRepository.GetPlan(planId);
            if (plan.PricingParameters == null)
            {
                _SetPlanPricingParameters(plan);
            }
            var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                plan,
                pricingParams,
                inventorySourceIds,
                diagnostic,
                processingId);
            var groupedInventory = _GroupInventory(inventory);

            var pricingApiRequest = new PlanPricingApiRequestDto_v3
            {
                Weeks = _GetPricingModelWeeks_v3(plan, parameters, new ProprietaryInventoryData(), out List<int> skippedWeeksIds),
                Spots = _GetPricingModelSpots_v3(groupedInventory, skippedWeeksIds)
            };

            return pricingApiRequest;
        }

        public List<PlanPricingInventoryProgram> GetPricingInventory(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            // used to tie the logging messages together.
            var processingId = Guid.NewGuid();
            _LogInfo("Starting...", processingId);

            var diagnostic = new PlanPricingJobDiagnostic();
            var plan = _PlanRepository.GetPlan(planId);
            var pricingParams = new ProgramInventoryOptionalParametersDto
            {
                MinCPM = requestParameters.MinCpm,
                MaxCPM = requestParameters.MaxCpm,
                InflationFactor = requestParameters.InflationFactor,
                MarketGroup = requestParameters.MarketGroup
            };
            var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                plan,
                pricingParams,
                inventorySourceIds,
                diagnostic,
                processingId);

            return inventory;
        }

        public string ForceCompletePlanPricingJob(int jobId, string username)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);
            job.Status = BackgroundJobProcessingStatus.Failed;
            job.ErrorMessage = $"Job status set to error by user '{username}'.";
            job.Completed = _DateTimeEngine.GetCurrentMoment();
            _PlanRepository.UpdatePlanPricingJob(job);

            return $"Job Id '{jobId}' has been forced to complete.";
        }

        public PricingProgramsResultDto GetProgramsByJobId(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);

            return _GetPrograms(job);
        }

        private PricingProgramsResultDto _GetPrograms(PlanPricingJob job)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var results = _PlanRepository.GetPricingProgramsResultByJobId(job.Id);

            if (results == null)
                return null;

            results.Totals.ImpressionsPercentage = 100;

            _ConvertImpressionsToUserFormat(results);

            return results;
        }

        private PricingProgramsResultDto_v2 _GetPrograms_v2(PlanPricingJob job,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var results = _PlanRepository.GetPricingProgramsResultByJobId_v2(job.Id, spotAllocationModelMode);

            if (results == null) return null;

            if (results.NsiResults != null)
            {
                results.NsiResults.Totals.ImpressionsPercentage = 100;
            }


            if (results.NtiResults != null)
            {
                results.NtiResults.Totals.ImpressionsPercentage = 100;
            }

            _ConvertImpressionsToUserFormat_v2(results);

            return results;
        }

        public PricingProgramsResultDto GetPrograms(int planId)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            return _GetPrograms(job);
        }

        public PricingProgramsResultDto GetProgramsForVersion(int planId, int planVersionId)
        {
            var job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId);

            return _GetPrograms(job);
        }

        public PricingProgramsResultDto_v2 GetPrograms_v2(int planId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            return _GetPrograms_v2(job, spotAllocationModelMode);
        }

        public PricingProgramsResultDto_v2 GetProgramsForVersion_v2(int planId, int planVersionId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId);

            return _GetPrograms_v2(job, spotAllocationModelMode);
        }

        private void _ConvertImpressionsToUserFormat(PricingProgramsResultDto planPricingResult)
        {
            if (planPricingResult == null)
                return;

            planPricingResult.Totals.AvgImpressions /= 1000;
            planPricingResult.Totals.Impressions /= 1000;

            foreach (var program in planPricingResult.Programs)
            {
                program.AvgImpressions /= 1000;
                program.Impressions /= 1000;
            }
        }

        private void _ConvertImpressionsToUserFormat_v2(PricingProgramsResultDto_v2 planPricingResult)
        {
            if (planPricingResult == null)
                return;

            if (planPricingResult.NsiResults != null)
            {
                planPricingResult.NsiResults.Totals.AvgImpressions /= 1000;
                planPricingResult.NsiResults.Totals.Impressions /= 1000;

                foreach (var program in planPricingResult.NsiResults.ProgramDetails)
                {
                    program.AvgImpressions /= 1000;
                    program.Impressions /= 1000;
                }
            }

            if (planPricingResult.NtiResults != null)
            {
                planPricingResult.NtiResults.Totals.AvgImpressions /= 1000;
                planPricingResult.NtiResults.Totals.Impressions /= 1000;

                foreach (var program in planPricingResult.NtiResults.ProgramDetails)
                {
                    program.AvgImpressions /= 1000;
                    program.Impressions /= 1000;
                }
            }
        }

        public PlanPricingBandDto GetPricingBandsByJobId(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);

            return _GetPricingBands(job);
        }

        private PlanPricingBandDto _GetPricingBands(PlanPricingJob job)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var results = _PlanRepository.GetPlanPricingBandByJobId(job.Id);

            if (results == null)
                return null;

            results.Bands = _GroupBandsByRange(results.Bands);

            _ConvertPricingBandImpressionsToUserFormat(results);

            results.Bands = results.Bands.OrderBy(x => x.MinBand).ToList();

            return results;
        }

        private List<PlanPricingBandDetailDto> _GroupBandsByRange(List<PlanPricingBandDetailDto> bands)
        {
            return bands
                .GroupBy(x => new { x.MinBand, x.MaxBand })
                .Select(x =>
                {
                    var impressions = x.Sum(y => y.Impressions);
                    var budget = x.Sum(y => y.Budget);

                    return new PlanPricingBandDetailDto
                    {
                        MinBand = x.Key.MinBand,
                        MaxBand = x.Key.MaxBand,
                        Spots = x.Sum(y => y.Spots),
                        Impressions = impressions,
                        Budget = budget,
                        Cpm = ProposalMath.CalculateCpm(budget, impressions),
                        ImpressionsPercentage = x.Sum(y => y.ImpressionsPercentage),
                        AvailableInventoryPercent = x.Sum(y => y.AvailableInventoryPercent)
                    };
                })
                .ToList();
        }

        public PlanPricingBandDto GetPricingBands(int planId)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            return _GetPricingBands(job);
        }

        public PlanPricingBandDto GetPricingBandsForVersion(int planId, int planVersionId)
        {
            var job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId);

            return _GetPricingBands(job);
        }

        public PlanPricingBandDto_v2 GetPricingBands_v2(int planId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            return _GetPricingBands_v2(job, spotAllocationModelMode);
        }

        public PlanPricingBandDto_v2 GetPricingBandsForVersion_v2(int planId, int planVersionId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId);

            return _GetPricingBands_v2(job, spotAllocationModelMode);
        }

        private PlanPricingBandDto_v2 _GetPricingBands_v2(PlanPricingJob job,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var results = _PlanRepository.GetPlanPricingBandByJobId_v2(job.Id, spotAllocationModelMode);

            if (results == null) return null;

            if (results.NsiResults != null)
            {
                results.NsiResults.BandsDetails = _GroupBandsByRange(results.NsiResults.BandsDetails);

                _ConvertPricingBandImpressionsToUserFormat_v2(results.NsiResults);

                results.NsiResults.BandsDetails = results.NsiResults.BandsDetails.OrderBy(x => x.MinBand).ToList();
            }

            if (results.NtiResults != null)
            {
                results.NtiResults.BandsDetails = _GroupBandsByRange(results.NtiResults.BandsDetails);

                _ConvertPricingBandImpressionsToUserFormat_v2(results.NtiResults);

                results.NtiResults.BandsDetails = results.NtiResults.BandsDetails.OrderBy(x => x.MinBand).ToList();
            }
            return results;
        }

        private void _ConvertPricingBandImpressionsToUserFormat_v2(PostingTypePlanPricingResultBands results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var band in results.BandsDetails)
            {
                band.Impressions /= 1000;
            }
        }

        /// <inheritdoc />
        public PlanPricingResultMarketsDto GetMarketsByJobId(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);

            return _GetMarkets(job);
        }

        /// <inheritdoc />
        public PlanPricingResultMarketsDto_v2 GetMarketsByJobId_v2(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);

            return _GetMarkets_v2(job, spotAllocationModelMode);
        }

        private PlanPricingResultMarketsDto _GetMarkets(PlanPricingJob job)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
            {
                return null;
            }

            var results = _PlanRepository.GetPlanPricingResultMarketsByJobId(job.Id);

            if (results == null)
            {
                return null;
            }

            results.MarketDetails = _GroupMarketDetailsByMarket(results.MarketDetails);

            _ConvertPricingMarketResultsToUserFormat(results);

            results.MarketDetails = results.MarketDetails.OrderBy(s => s.Rank).ToList();

            return results;
        }

        private PlanPricingResultMarketsDto_v2 _GetMarkets_v2(PlanPricingJob job,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
            {
                return null;
            }

            var results = _PlanRepository.GetPlanPricingResultMarketsByJobId_v2(job.Id, spotAllocationModelMode);

            if (results == null)
            {
                return null;
            }

            if (results.NsiResults != null)
            {
                results.NsiResults.MarketDetails = _GroupMarketDetailsByMarket(results.NsiResults.MarketDetails)
                    .OrderBy(s => s.Rank).ToList();

                _ConvertPricingMarketResultsToUserFormat_v2(results.NsiResults);
            }

            if (results.NtiResults != null)
            {
                results.NtiResults.MarketDetails = _GroupMarketDetailsByMarket(results.NtiResults?.MarketDetails)
                .OrderBy(s => s.Rank).ToList();
                _ConvertPricingMarketResultsToUserFormat_v2(results.NtiResults);
            }

            return results;
        }



        private List<PlanPricingResultMarketDetailsDto> _GroupMarketDetailsByMarket(List<PlanPricingResultMarketDetailsDto> details)
        {
            return details
                .GroupBy(x => x.MarketName)
                .Select(x =>
                {
                    var first = x.First();
                    var impressions = x.Sum(y => y.Impressions);
                    var budget = x.Sum(y => y.Budget);

                    return new PlanPricingResultMarketDetailsDto
                    {
                        Rank = first.Rank,
                        MarketName = first.MarketName,
                        MarketCoveragePercent = first.MarketCoveragePercent,
                        ShareOfVoiceGoalPercentage = first.ShareOfVoiceGoalPercentage,
                        Stations = first.Stations, // Contains stations from both OpenMarket and Proprietary inventory, look at repo where it`s set
                        Impressions = impressions,
                        Budget = budget,
                        Cpm = ProposalMath.CalculateCpm(budget, impressions),
                        Spots = x.Sum(y => y.Spots),
                        ImpressionsPercentage = x.Sum(y => y.ImpressionsPercentage)
                    };
                })
                .ToList();
        }

        /// <inheritdoc />
        public PlanPricingResultMarketsDto GetMarkets(int planId)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            return _GetMarkets(job);
        }

        public PlanPricingResultMarketsDto GetMarketsForVersion(int planId, int planVersionId)
        {
            var job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId);

            return _GetMarkets(job);
        }

        public PlanPricingResultMarketsDto_v2 GetMarkets_v2(int planId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            return _GetMarkets_v2(job, spotAllocationModelMode);
        }

        public PlanPricingResultMarketsDto_v2 GetMarketsForVersion_v2(int planId, int planVersionId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId);

            return _GetMarkets_v2(job, spotAllocationModelMode);
        }

        public PlanPricingStationResultDto GetStationsByJobId(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);

            return _GetStations(job);
        }

        private PlanPricingStationResultDto _GetStations(PlanPricingJob job)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var result = _PlanRepository.GetPricingStationsResultByJobId(job.Id);

            if (result == null)
                return null;

            result.Stations = _GroupStationDetailsByStation(result.Stations);

            _ConvertPricingStationResultDtoToUserFormat(result);

            return result;
        }

        private List<PlanPricingStationDto> _GroupStationDetailsByStation(List<PlanPricingStationDto> stations)
        {
            return stations
                .GroupBy(x => x.Station)
                .Select(x =>
                {
                    var impressions = x.Sum(y => y.Impressions);
                    var budget = x.Sum(y => y.Budget);

                    return new PlanPricingStationDto
                    {
                        Station = x.Key,
                        Market = x.First().Market,
                        Spots = x.Sum(y => y.Spots),
                        Impressions = impressions,
                        Budget = budget,
                        ImpressionsPercentage = x.Sum(y => y.ImpressionsPercentage),
                        Cpm = ProposalMath.CalculateCpm(budget, impressions)
                    };
                })
                .ToList();
        }

        public PlanPricingStationResultDto GetStations(int planId)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            return _GetStations(job);
        }

        public PlanPricingStationResultDto GetStationsForVersion(int planId, int planVersionId)
        {
            var job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId);

            return _GetStations(job);
        }

        public PlanPricingStationResultDto_v2 GetStations_v2(int planId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            return _GetStations_v2(job, spotAllocationModelMode);
        }

        public PlanPricingStationResultDto_v2 GetStationsForVersion_v2(int planId, int planVersionId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId);

            return _GetStations_v2(job, spotAllocationModelMode);
        }

        private PlanPricingStationResultDto_v2 _GetStations_v2(PlanPricingJob job,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var result = _PlanRepository.GetPricingStationsResultByJobId_v2(job.Id, spotAllocationModelMode);

            if (result == null) return null;

            if (result.NsiResults != null)
            {
                result.NsiResults.StationDetails = _GroupStationDetailsByStation(result.NsiResults.StationDetails);

                _ConvertPricingStationResultDtoToUserFormat(result.NsiResults);
            }

            if (result.NtiResults != null)
            {
                result.NtiResults.StationDetails = _GroupStationDetailsByStation(result.NtiResults.StationDetails);

                _ConvertPricingStationResultDtoToUserFormat(result.NtiResults);
            }

            return result;
        }

        private void _ConvertPricingStationResultDtoToUserFormat(PostingTypePlanPricingResultStations results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var band in results.StationDetails)
            {
                band.Impressions /= 1000;
            }
        }

        private void _ConvertPricingStationResultDtoToUserFormat(PlanPricingStationResultDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var band in results.Stations)
            {
                band.Impressions /= 1000;
            }
        }

        private void _ConvertPricingBandImpressionsToUserFormat(PlanPricingBandDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var band in results.Bands)
            {
                band.Impressions /= 1000;
            }
        }

        private void _ConvertPricingMarketResultsToUserFormat(PlanPricingResultMarketsDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var detail in results.MarketDetails)
            {
                detail.Impressions /= 1000;
            }
        }

        private void _ConvertPricingMarketResultsToUserFormat_v2(PostingTypePlanPricingResultMarkets results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var detail in results.MarketDetails)
            {
                detail.Impressions /= 1000;
            }
        }

        internal int _GetPricingModelVersion()
        {
            return 4;
        }

        internal class ProgramWithManifestDaypart
        {
            public PlanPricingInventoryProgram Program { get; set; }

            public PlanPricingInventoryProgram.ManifestDaypart ManifestDaypart { get; set; }
        }

        private class ProgramDaypartWeekGroupItem
        {
            public int ContractedInventoryId { get; set; }
            public int ContractedMediaWeekId { get; set; }
            public int InventoryDaypartId { get; set; }
            public PlanPricingApiRequestSpotsDto Spot { get; set; }
            public int ProgramMinimumContractMediaWeekId { get; set; }
        }

        private class ProgramDaypartWeekGroupItem_V3
        {
            public int ContractedInventoryId { get; set; }
            public int ContractedMediaWeekId { get; set; }
            public int InventoryDaypartId { get; set; }
            public PlanPricingApiRequestSpotsDto_v3 Spot { get; set; }
            public int ProgramMinimumContractMediaWeekId { get; set; }
        }

        internal enum PricingJobTaskNameEnum
        {
            AggregateResults,
            CalculatePricingBands,
            CalculatePricingStations,
            AggregateMarketResults
        }

        internal class AggregationTask
        {
            public PostingTypeEnum PostingType { get; set; }
            public PricingJobTaskNameEnum TaskName { get; set; }
            public Task Task { get; set; }

            public AggregationTask(PostingTypeEnum postingType, PricingJobTaskNameEnum taskName, Task task)
            {
                PostingType = postingType;
                TaskName = taskName;
                Task = task;
            }
        }

        /// <inheritdoc/>
        public string TestGetProgramsForPricingModel(int jobId)
        {
            var txId = Guid.NewGuid();
            var logMsgSuffix = $"Job Id {jobId};";

            _LogInfo($"Beginning. {logMsgSuffix}", txId);

            var job = _PlanRepository.GetPlanPricingJob(jobId);
            var planId = _PlanRepository.GetPlanIdFromPlanVersion(job.PlanVersionId.Value);
            var plan = _PlanRepository.GetPlan(planId.Value, job.PlanVersionId.Value);

            var startDate = plan.FlightStartDate.Value;
            var endDate = plan.FlightEndDate.Value;
            var spotLengthIds = plan.CreativeLengths.Select(c => c.SpotLengthId).ToList();
            var inventorySourceIds = new List<int> { (int)InventorySourceEnum.OpenMarket };
            var availableMarkets = plan.AvailableMarkets.Select(m => m.MarketCode).ToList();

            var stationIds = _StationRepository.GetBroadcastStationsWithLatestDetailsByMarketCodes(availableMarkets)
                .Select(s => s.Id)
                .ToList();

            _LogInfo($"Found {stationIds.Count} station Ids. {logMsgSuffix}", txId);

            var querySw = new Stopwatch();
            querySw.Start();

            var results = ((PlanPricingInventoryEngine) _PlanPricingInventoryEngine)._GetProgramsForPricingModel(
                startDate, endDate, spotLengthIds, inventorySourceIds, stationIds, txId);

            querySw.Stop();
            var durationMs = querySw.ElapsedMilliseconds;

            var resultsMsg = $"Found {results.Count} results in {durationMs}ms. {logMsgSuffix}";
            _LogInfo(resultsMsg, txId);
            return resultsMsg;
        }
    }    
}
