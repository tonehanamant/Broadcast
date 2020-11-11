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
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.PricingResults;
using Services.Broadcast.ReportGenerators.Quote;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices.Plan
{
    public interface IPlanPricingService : IApplicationService
    {
        PlanPricingJob QueuePricingJob(PlanPricingParametersDto planPricingParametersDto, DateTime currentDate, string username);
        PlanPricingJob QueuePricingJob(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto, DateTime currentDate, string username);
        CurrentPricingExecution GetCurrentPricingExecution(int planId);
        CurrentPricingExecution GetCurrentPricingExecution(int planId, int? planVersionId);
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
        void RunPricingJob(PlanPricingParametersDto planPricingParametersDto, int jobId, CancellationToken token);
        [Queue("planpricing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void RunPricingWithoutPlanJob(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto, int jobId, CancellationToken token);
        /// <summary>
        /// For troubleshooting
        /// </summary>
        List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId);
        /// <summary>
        /// For troubleshooting
        /// </summary>
        PlanPricingApiRequestDto GetPricingApiRequestPrograms(int planId, PricingInventoryGetRequestParametersDto requestParameters);
        /// <summary>
        /// For troubleshooting
        /// </summary>
        PlanPricingApiRequestDto_v3 GetPricingApiRequestPrograms_v3(int planId, PricingInventoryGetRequestParametersDto requestParameters);
        /// <summary>
        /// For troubleshooting
        /// </summary>
        List<PlanPricingInventoryProgram> GetPricingInventory(int planId, PricingInventoryGetRequestParametersDto requestParameters);
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
        /// For troubleshooting
        /// </summary>
        string ForceCompletePlanPricingJob(int jobId, string username);
        /// <summary>
        /// For troubleshooting.  This will bypass the queue to allow rerunning directly.
        /// </summary>
        /// <param name="jobId">The id of the job to rerun.</param>
        /// <returns>The new JobId</returns>
        int ReRunPricingJob(int jobId);
        /// <summary>
        /// For troubleshooting.  Generate a pricing results report for the chosen plan and version
        /// </summary>
        /// <param name="planId">The plan id</param>
        /// <param name="planVersionNumber">The plan version number</param>
        /// <param name="templatesFilePath">Base path of the file templates</param>
        /// <returns>ReportOutput which contains filename and MemoryStream which actually contains report data</returns>
        ReportOutput GeneratePricingResultsReport(int planId, int? planVersionNumber, string templatesFilePath);
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
        PlanPricingResultMarketsDto GetMarketsForVersion(int planId, int planVersionId);
        PlanPricingBandDto GetPricingBands(int planId);
        PlanPricingBandDto GetPricingBandsByJobId(int jobId);
        PlanPricingBandDto GetPricingBandsForVersion(int planId, int planVersionId);
        [Queue("savepricingrequest")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto pricingApiRequest);
        [Queue("savepricingrequest")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto_v3 pricingApiRequest);
        Guid RunQuote(QuoteRequestDto request, string userName, string templatesFilePath);
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
        private readonly IPlanValidator _PlanValidator;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IAudienceService _AudienceService;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;
        private readonly IInventoryProprietarySummaryRepository _InventoryProprietarySummaryRepository;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IAsyncTaskHelper _AsyncTaskHelper;

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
                                  IPlanValidator planValidator,
                                  ISharedFolderService sharedFolderService,
                                  IAudienceService audienceService,
                                  ICreativeLengthEngine creativeLengthEngine,
                                  IAsyncTaskHelper asyncTaskHelper)
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
            _PlanValidator = planValidator;
            _SharedFolderService = sharedFolderService;
            _AudienceService = audienceService;
            _CreativeLengthEngine = creativeLengthEngine;
            _InventoryProprietarySummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProprietarySummaryRepository>();
            _BroadcastAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _AsyncTaskHelper = asyncTaskHelper;
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

            var reportData = new QuoteReportData(request, generatedTimeStamp, allAudiences, allMarkets, programs);
            return reportData;
        }

        public ReportOutput GeneratePricingResultsReport(int planId, int? planVersionNumber, string templatesFilePath)
        {
            var reportData = GetPricingResultsReportData(planId, planVersionNumber);
            var reportGenerator = new PricingResultsReportGenerator(templatesFilePath);
            var report = reportGenerator.Generate(reportData);

            return report;
        }

        public PricingResultsReportData GetPricingResultsReportData(int planId, int? planVersionNumber)
        {
            // use passed version or the current version by default
            var planVersionId = planVersionNumber.HasValue ?
                _PlanRepository.GetPlanVersionIdByVersionNumber(planId, planVersionNumber.Value) :
                (int?)null;

            var plan = _PlanRepository.GetPlan(planId, planVersionId);
            var allocatedSpots = _PlanRepository.GetPlanPricingAllocatedSpotsByPlanVersionId(plan.VersionId);
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
                _WeeklyBreakdownEngine);
        }

        public PlanPricingJob QueuePricingJob(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto
            , DateTime currentDate, string username)
        {
            if (pricingParametersWithoutPlanDto.JobId.HasValue && IsPricingModelRunningForJob(pricingParametersWithoutPlanDto.JobId.Value))
            {
                throw new Exception("The pricing model is already running");
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

            job.HangfireJobId = _BackgroundJobClient.Enqueue<IPlanPricingService>(x => x.RunPricingWithoutPlanJob(pricingParametersWithoutPlanDto, job.Id, CancellationToken.None));

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
                    throw new Exception("The pricing model is already running for the plan");
                }

                var plan = _PlanRepository.GetPlan(planPricingParametersDto.PlanId.Value);

                ValidateAndApplyMargin(planPricingParametersDto);

                int planVersionId;

                // For drafts, we use the plan version id sent as parameter.
                // This is because a draft is not considered the latest version of a plan.
                if (planPricingParametersDto.PlanVersionId.HasValue)
                    planVersionId = planPricingParametersDto.PlanVersionId.Value;
                else
                    planVersionId = plan.VersionId;

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

                job.HangfireJobId = _BackgroundJobClient.Enqueue<IPlanPricingService>(x => x.RunPricingJob(planPricingParametersDto, job.Id, CancellationToken.None));

                _PlanRepository.UpdateJobHangfireId(job.Id, job.HangfireJobId);

                return job;
            }
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

            if (parameters.Margin.HasValue)
            {
                if (parameters.Margin.Value > allowedMaxValue ||
                    parameters.Margin.Value < allowedMinValue)
                {
                    throw new InvalidOperationException("A provided Margin value must be between .01% And 100%.");
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
                MarketGroup = MarketGroupEnum.Top100
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

            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                //in case the error is comming from the Pricing Run model, the error message field will have better
                //message then the generic we construct here
                if (string.IsNullOrWhiteSpace(job.DiagnosticResult))
                    throw new Exception(job.ErrorMessage);
                throw new Exception(
                    "Error encountered while running Pricing Model, please contact a system administrator for help");
            }

            if (job != null && job.Status == BackgroundJobProcessingStatus.Succeeded)
            {
                pricingExecutionResult = _PlanRepository.GetPricingResultsByJobId(job.Id);

                if (pricingExecutionResult != null)
                {
                    pricingExecutionResult.Notes = pricingExecutionResult.GoalFulfilledByProprietary
                        ? "Proprietary goals meet plan goals"
                        : string.Empty;
                    if (pricingExecutionResult.JobId.HasValue)
                    {
                        decimal goalCpm;
                        if (pricingExecutionResult.PlanVersionId.HasValue)
                            goalCpm = _PlanRepository.GetGoalCpm(pricingExecutionResult.PlanVersionId.Value,
                                pricingExecutionResult.JobId.Value);
                        else
                            goalCpm = _PlanRepository.GetGoalCpm(pricingExecutionResult.JobId.Value);

                        pricingExecutionResult.CpmPercentage =
                            CalculateCpmPercentage(pricingExecutionResult.OptimalCpm, goalCpm);
                    }
                }
            }

            //pricingExecutionResult might be null when there is no pricing run for the latest version            
            return new CurrentPricingExecution
            {
                Job = job,
                Result = pricingExecutionResult ?? new CurrentPricingExecutionResultDto(),
                IsPricingModelRunning = IsPricingModelRunning(job)
            };
        }

        public CurrentPricingExecution GetCurrentPricingExecution(int planId)
        {
            return GetCurrentPricingExecution(planId, null);
        }

        public CurrentPricingExecution GetCurrentPricingExecution(int planId, int? planVersionId)
        {
            PlanPricingJob job;

            if (planVersionId.HasValue)
                job = _PlanRepository.GetPricingJobForPlanVersion(planVersionId.Value);
            else
                job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);

            return _GetCurrentPricingExecution(job);
        }

        /// <summary>
        /// Goal CPM Percentage Indicator Calculation
        /// </summary>
        /// <returns></returns>
        public int CalculateCpmPercentage(decimal optimalCpm, decimal goalCpm)
        {
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
            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                throw new Exception("Error encountered while running Pricing Model, please contact a system administrator for help");
            }

            if (!IsPricingModelRunning(job))
            {
                throw new Exception("Error encountered while canceling Pricing Model, process is not running");
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
            job.Completed = _GetCurrentDateTime();

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

        internal List<PlanPricingApiRequestSpotsDto> _GetPricingModelSpots(
            List<IGrouping<PlanPricingInventoryGroup, ProgramWithManifestDaypart>> groupedInventory,
            List<int> skippedWeeksIds)
        {
            var marketCoveragesByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var pricingModelSpots = new List<PlanPricingApiRequestSpotsDto>();

            foreach (var inventoryGrouping in groupedInventory)
            {
                var programSpots = new List<ProgramDaypartWeekGroupItem>();

                var programsInGrouping = inventoryGrouping.Select(x => x.Program).ToList();
                var manifestId = programsInGrouping.First().ManifestId;

                foreach (var program in programsInGrouping)
                {
                    var programMinimumContractMediaWeekId = program.ManifestWeeks.Select(w => w.ContractMediaWeekId).Min();

                    foreach (var daypart in program.ManifestDayparts)
                    {
                        var programInventoryDaypartId = daypart.Daypart.Id;

                        var impressions = program.Impressions;
                        var spotCost = program.ManifestRates.Single().Cost;

                        if (impressions <= 0)
                            continue;

                        if (spotCost <= 0)
                            continue;

                        //filter out skipped weeks
                        var spots = program.ManifestWeeks
                            .Where(x => !skippedWeeksIds.Contains(x.ContractMediaWeekId))
                            .Select(manifestWeek => new ProgramDaypartWeekGroupItem
                            {
                                ContractedInventoryId = manifestId,
                                ContractedMediaWeekId = manifestWeek.ContractMediaWeekId,
                                InventoryDaypartId = programInventoryDaypartId,
                                ProgramMinimumContractMediaWeekId = programMinimumContractMediaWeekId,
                                Spot = new PlanPricingApiRequestSpotsDto
                                {
                                    Id = manifestId,
                                    MediaWeekId = manifestWeek.ContractMediaWeekId,
                                    DaypartId = program.StandardDaypartId,
                                    Impressions = impressions,
                                    Cost = spotCost,
                                    StationId = program.Station.Id,
                                    MarketCode = program.Station.MarketCode.Value,
                                    PercentageOfUs = GeneralMath.ConvertPercentageToFraction(marketCoveragesByMarketCode[program.Station.MarketCode.Value]),
                                    SpotDays = daypart.Daypart.ActiveDays,
                                    SpotHours = daypart.Daypart.GetDurationPerDayInHours()
                                }
                            });

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
                    ProgramDaypartWeekGroupItem keptItem = null;
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

        private List<PlanPricingApiRequestWeekDto> _GetPricingModelWeeks(
            PlanDto plan,
            PlanPricingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData,
            out List<int> SkippedWeeksIds)
        {
            SkippedWeeksIds = new List<int>();
            var pricingModelWeeks = new List<PlanPricingApiRequestWeekDto>();
            var planImpressionsGoal = plan.PricingParameters.DeliveryImpressions * 1000;

            // send 0.001% if any unit is selected
            var marketCoverageGoal = parameters.ProprietaryInventory.IsEmpty() ? GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value) : 0.001;
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);
            var shareOfVoice = _GetShareOfVoice(topMarkets, marketsWithSov, proprietaryInventoryData, planImpressionsGoal);
            var daypartsWithWeighting = plan.Dayparts.Where(x => x.WeightingGoalPercent.HasValue);
            var planPricingParameters = plan.PricingParameters;
            var weeklyBreakdownByWeek = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(plan.WeeklyBreakdownWeeks);

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

                var cpmGoal = ProposalMath.CalculateCpm(weeklyBudget, impressionGoal);
                (double capTime, string capType) = FrequencyCapHelper.GetFrequencyCapTimeAndCapTypeString(planPricingParameters.UnitCapsType);

                var pricingWeek = new PlanPricingApiRequestWeekDto
                {
                    MediaWeekId = mediaWeekId,
                    ImpressionGoal = impressionGoal,
                    CpmGoal = cpmGoal,
                    MarketCoverageGoal = marketCoverageGoal,
                    FrequencyCapSpots = planPricingParameters.UnitCaps,
                    FrequencyCapTime = capTime,
                    FrequencyCapUnit = capType,
                    ShareOfVoice = shareOfVoice,
                    DaypartWeighting = daypartsWithWeighting.Select(x => new DaypartWeighting
                    {
                        DaypartId = x.DaypartCodeId,
                        DaypartGoal = GeneralMath.ConvertPercentageToFraction(x.WeightingGoalPercent.Value)
                    }).ToList()
                };

                pricingModelWeeks.Add(pricingWeek);
            }

            return pricingModelWeeks;
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
                    ImpressionsPerUnit = plan.ImpressionsPerUnit,
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
            double planImpressionsGoal)
        {
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
        public int ReRunPricingJob(int jobId)
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
            RunPricingJob(jobParams, newJobId, CancellationToken.None);

            return newJobId;
        }

        private enum PricingJobTaskNameEnum
        {
            AggregateResults,
            CalculatePricingBands,
            CalculatePricingStations,
            AggregateMarketResults
        }

        private void _RunPricingJob(PlanPricingParametersDto planPricingParametersDto, PlanDto plan, int jobId, CancellationToken token)
        {
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
                    processingId,
                    true);

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

                var allocationResult = new PlanPricingAllocationResult
                {
                    Spots = new List<PlanPricingAllocatedSpot>(),
                    JobId = jobId,
                    PlanVersionId = plan.VersionId,
                    PricingVersion = BroadcastServiceSystemParameter.PlanPricingEndpointVersion,
                    PostingType = plan.PostingType
                };

                //Send it to the DS Model based on one posting type, as selected in the plan detail.
                var planPostingTypeInventory = inventory.Where(x => x.PostingType == plan.PostingType)
                            .ToList();

                if (!goalsFulfilledByProprietaryInventory)
                {
                    _SendPricingRequest(
                        allocationResult,
                        plan,
                        jobId,
                        planPostingTypeInventory,
                        token,
                        diagnostic,
                        planPricingParametersDto,
                        proprietaryInventoryData);
                }

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_CPM);
                allocationResult.PricingCpm = _CalculatePricingCpm(allocationResult.Spots, proprietaryInventoryData);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_CPM);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);
                _ValidateAllocationResult(allocationResult);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);

                token.ThrowIfCancellationRequested();

                var aggregationTasks = new List<(PostingTypeEnum PostingType, PricingJobTaskNameEnum TaskName, Task Task)>();

                foreach (var postingType in Enum.GetValues(typeof(PostingTypeEnum)).Cast<PostingTypeEnum>())
                {
                    var postingTypeInventory = inventory.Where(x => x.PostingType == postingType)
                        .ToList();

                    _ValidateInventory(postingTypeInventory);

                    token.ThrowIfCancellationRequested();

                    var aggregateResultsTask = new Task<PlanPricingResultBaseDto>(() =>
                    {
                        diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                        var aggregatedResults = _PlanPricingProgramCalculationEngine.CalculateProgramResults(postingTypeInventory, allocationResult, goalsFulfilledByProprietaryInventory, proprietaryInventoryData, postingType);
                        diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                        return aggregatedResults;
                    });

                    aggregationTasks.Add((postingType, PricingJobTaskNameEnum.AggregateResults, aggregateResultsTask));
                    aggregateResultsTask.Start();


                    var calculatePricingBandsTask = new Task<PlanPricingBand>(() =>
                    {
                        diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_BANDS);
                        var pricingBands = _PlanPricingBandCalculationEngine.CalculatePricingBands(postingTypeInventory, allocationResult, planPricingParametersDto, proprietaryInventoryData, postingType);
                        diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_BANDS);
                        return pricingBands;
                    });

                    aggregationTasks.Add((postingType, PricingJobTaskNameEnum.CalculatePricingBands, calculatePricingBandsTask));
                    calculatePricingBandsTask.Start();


                    var calculatePricingStationsTask = new Task<PlanPricingStationResult>(() =>
                    {
                        diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_STATIONS);
                        var pricingStations = _PlanPricingStationCalculationEngine.Calculate(postingTypeInventory, allocationResult, planPricingParametersDto, proprietaryInventoryData, postingType);
                        diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_STATIONS);
                        return pricingStations;
                    });

                    aggregationTasks.Add((postingType, PricingJobTaskNameEnum.CalculatePricingStations, calculatePricingStationsTask));
                    calculatePricingStationsTask.Start();

                    var aggregateMarketResultsTask = new Task<PlanPricingResultMarkets>(() =>
                    {
                        diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                        var marketCoverages = _MarketCoverageRepository.GetMarketsWithLatestCoverage();
                        var pricingMarketResults = _PlanPricingMarketResultsEngine.Calculate(postingTypeInventory, allocationResult, plan, marketCoverages, proprietaryInventoryData, postingType);
                        diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                        return pricingMarketResults;
                    });

                    aggregationTasks.Add((postingType, PricingJobTaskNameEnum.AggregateMarketResults, aggregateMarketResultsTask));
                    aggregateMarketResultsTask.Start();
                }

                token.ThrowIfCancellationRequested();

                //Wait for all tasks nti and nsi to finish
                var allAggregationTasks = Task.WhenAll(aggregationTasks.Select(x => x.Task).ToArray());
                allAggregationTasks.Wait();

                using (var transaction = new TransactionScopeWrapper())
                {
                    //We only get one set of allocation results
                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);
                    _PlanRepository.SavePricingApiResults(allocationResult);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);

                    foreach (var postingType in Enum.GetValues(typeof(PostingTypeEnum)).Cast<PostingTypeEnum>())
                    {
                        var calculatePricingBandTask = (Task<PlanPricingBand>)aggregationTasks
                            .First(x => x.PostingType == postingType && x.TaskName == PricingJobTaskNameEnum.CalculatePricingBands)
                            .Task;

                        diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_BANDS);
                        _PlanRepository.SavePlanPricingBands(calculatePricingBandTask.Result);
                        diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_BANDS);

                        var aggregateTask = (Task<PlanPricingResultBaseDto>)aggregationTasks
                            .First(x => x.PostingType == postingType && x.TaskName == PricingJobTaskNameEnum.AggregateResults)
                            .Task;
                        diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);
                        _PlanRepository.SavePricingAggregateResults(aggregateTask.Result);
                        diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);

                        var calculatePricingStationTask = (Task<PlanPricingStationResult>)aggregationTasks
                            .First(x => x.PostingType == postingType && x.TaskName == PricingJobTaskNameEnum.CalculatePricingStations)
                            .Task;
                        diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_STATIONS);
                        _PlanRepository.SavePlanPricingStations(calculatePricingStationTask.Result);
                        diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_STATIONS);


                        var aggregateMarketResultsTask = (Task<PlanPricingResultMarkets>)aggregationTasks
                            .First(x => x.PostingType == postingType && x.TaskName == PricingJobTaskNameEnum.AggregateMarketResults)
                            .Task;
                        diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);
                        _PlanRepository.SavePlanPricingMarketResults(aggregateMarketResultsTask.Result);
                        diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);
                    }

                    //Finsh up the job
                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);
                    var pricingJob = _PlanRepository.GetPlanPricingJob(jobId);
                    pricingJob.Status = BackgroundJobProcessingStatus.Succeeded;
                    pricingJob.Completed = _DateTimeEngine.GetCurrentMoment();
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);

                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_TOTAL_DURATION);
                    pricingJob.DiagnosticResult = diagnostic.ToString();

                    _PlanRepository.UpdatePlanPricingJob(pricingJob);

                    transaction.Complete();
                }
            }
            catch (PricingModelException exception)
            {
                _HandlePricingJobError(jobId, BackgroundJobProcessingStatus.Failed, exception.Message);
            }
            catch (Exception exception) when (exception is ObjectDisposedException || exception is OperationCanceledException)
            {
                _HandlePricingJobException(jobId, BackgroundJobProcessingStatus.Canceled, exception, "Running the pricing model was canceled");
            }
            catch (Exception exception)
            {
                _HandlePricingJobException(jobId, BackgroundJobProcessingStatus.Failed, exception, "Error attempting to run the pricing model");
            }
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
                .Where(x => x.Any(w => w.WeeklyImpressions > 0))
                .Count();

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

        private void _SendPricingRequest(
            PlanPricingAllocationResult allocationResult,
            PlanDto plan,
            int jobId,
            List<PlanPricingInventoryProgram> inventory,
            CancellationToken token,
            PlanPricingJobDiagnostic diagnostic,
            PlanPricingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            if (BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "2")
            {
                _SendPricingRequest_v2(
                    allocationResult,
                    plan,
                    jobId,
                    inventory,
                    token,
                    diagnostic,
                    parameters,
                    proprietaryInventoryData);
            }
            else if (BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "3")
            {
                _SendPricingRequest_v3(
                    allocationResult,
                    plan,
                    jobId,
                    inventory,
                    token,
                    diagnostic,
                    parameters,
                    proprietaryInventoryData);
            }
            else
            {
                throw new Exception("Unknown pricing API version was discovered");
            }
        }

        private void _SendPricingRequest_v2(
            PlanPricingAllocationResult allocationResult,
            PlanDto plan,
            int jobId,
            List<PlanPricingInventoryProgram> inventory,
            CancellationToken token,
            PlanPricingJobDiagnostic diagnostic,
            PlanPricingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var pricingModelWeeks = _GetPricingModelWeeks(
                plan,
                parameters,
                proprietaryInventoryData,
                out List<int> skippedWeeksIds);

            var groupedInventory = _GroupInventory(inventory);
            var spots = _GetPricingModelSpots(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            token.ThrowIfCancellationRequested();

            var pricingApiRequest = new PlanPricingApiRequestDto
            {
                Weeks = pricingModelWeeks,
                Spots = spots
            };

            _AsyncTaskHelper.TaskFireAndForget(() => SavePricingRequest(plan.Id, jobId, pricingApiRequest));

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALLING_API);
            var apiAllocationResult = _PricingApiClient.GetPricingSpotsResult(pricingApiRequest);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALLING_API);

            token.ThrowIfCancellationRequested();

            if (apiAllocationResult.Error != null)
            {
                var errorMessage = $@"Pricing Model returned the following error: {apiAllocationResult.Error.Name} 
                                -  {string.Join(",", apiAllocationResult.Error.Messages).Trim(',')}";
                throw new PricingModelException(errorMessage);
            }

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
            allocationResult.Spots = _MapToResultSpots(groupedInventory, apiAllocationResult, pricingApiRequest, inventory, parameters);
            allocationResult.RequestId = apiAllocationResult.RequestId;
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
        }

        private void _SendPricingRequest_v3(
            PlanPricingAllocationResult allocationResult,
            PlanDto plan,
            int jobId,
            List<PlanPricingInventoryProgram> inventory,
            CancellationToken token,
            PlanPricingJobDiagnostic diagnostic,
            PlanPricingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var pricingModelWeeks = _GetPricingModelWeeks_v3(
                plan,
                parameters,
                proprietaryInventoryData,
                out List<int> skippedWeeksIds);

            var groupedInventory = _GroupInventory(inventory);
            var spots = _GetPricingModelSpots_v3(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            token.ThrowIfCancellationRequested();

            var pricingApiRequest = new PlanPricingApiRequestDto_v3
            {
                Weeks = pricingModelWeeks,
                Spots = spots
            };

            _AsyncTaskHelper.TaskFireAndForget(() => SavePricingRequest(plan.Id, jobId, pricingApiRequest));

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALLING_API);
            var apiAllocationResult = _PricingApiClient.GetPricingSpotsResult(pricingApiRequest);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALLING_API);

            token.ThrowIfCancellationRequested();

            if (apiAllocationResult.Error != null)
            {
                var errorMessage = $@"Pricing Model returned the following error: {apiAllocationResult.Error.Name} 
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

                        var impressions = program.Impressions;

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

                var groupedProgramSpots = programSpots.GroupBy(i => new {i.ContractedInventoryId, i.ContractedMediaWeekId, i.InventoryDaypartId}).ToList();
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

        public void RunPricingWithoutPlanJob(PricingParametersWithoutPlanDto pricingParametersWithoutPlanDto, int jobId, CancellationToken token)
        {
            var pricingParameters = _ConvertPricingWihtoutPlanParametersToPlanPricingParameters(pricingParametersWithoutPlanDto);
            var plan = _ConvertPricingWihtoutPlanParametersToPlanDto(pricingParametersWithoutPlanDto);

            _RunPricingJob(pricingParameters, plan, jobId, token);
        }

        public void RunPricingJob(PlanPricingParametersDto planPricingParametersDto, int jobId, CancellationToken token)
        {
            var plan = _PlanRepository.GetPlan(planPricingParametersDto.PlanId.Value, planPricingParametersDto.PlanVersionId);
            _RunPricingJob(planPricingParametersDto, plan, jobId, token);
        }

        private List<PlanPricingApiRequestWeekDto_v3> _GetPricingModelWeeks_v3(
            PlanDto plan,
            PlanPricingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData,
            out List<int> SkippedWeeksIds)
        {
            SkippedWeeksIds = new List<int>();
            var pricingModelWeeks = new List<PlanPricingApiRequestWeekDto_v3>();
            var planImpressionsGoal = plan.PricingParameters.DeliveryImpressions * 1000;

            // send 0.001% if any unit is selected
            var marketCoverageGoal = parameters.ProprietaryInventory.IsEmpty() ? GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value) : 0.001;
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);
            var shareOfVoice = _GetShareOfVoice(topMarkets, marketsWithSov, proprietaryInventoryData, planImpressionsGoal);
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

                var pricingWeek = new PlanPricingApiRequestWeekDto_v3
                {
                    MediaWeekId = mediaWeekId,
                    ImpressionGoal = impressionGoal,
                    CpmGoal = ProposalMath.CalculateCpm(weeklyBudget, impressionGoal),
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
                x => plan.Equivalized ? _SpotLengthEngine.GetDeliveryMultiplierBySpotLengthId(x.SpotLengthId) : 1);
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

        public void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto pricingApiRequest)
        {
            _LogInfo($"Saving the pricing API Request.  PlanId = '{planId}'.");
            try
            {
                _PricingRequestLogClient.SavePricingRequest(planId, jobId, pricingApiRequest);
            }
            catch (Exception exception)
            {
                _LogError($"Failed to save pricing API request.  PlanId = '{planId}'.", exception);
            }
        }

        public void SavePricingRequest(int planId, int jobId, PlanPricingApiRequestDto_v3 pricingApiRequest)
        {
            _LogInfo($"Saving the pricing API Request.  PlanId = '{planId}'.");
            try
            {
                _PricingRequestLogClient.SavePricingRequest(planId, jobId, pricingApiRequest);
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
            string logMessage)
        {
            _PlanRepository.UpdatePlanPricingJob(new PlanPricingJob
            {
                Id = jobId,
                Status = status,
                DiagnosticResult = exception.ToString(),
                ErrorMessage = logMessage,
                Completed = _DateTimeEngine.GetCurrentMoment()
            });

            _LogError($"Error attempting to run the pricing model : {exception.Message}", exception);
        }

        private void _HandlePricingJobError(
            int jobId,
            BackgroundJobProcessingStatus status,
            string errorMessages)
        {
            _PlanRepository.UpdatePlanPricingJob(new PlanPricingJob
            {
                Id = jobId,
                Status = status,
                ErrorMessage = errorMessages,
                Completed = _DateTimeEngine.GetCurrentMoment()
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

        private List<InventorySourceTypeEnum> _GetSupportedInventorySourceTypes()
        {
            var result = new List<InventorySourceTypeEnum>();

            if (BroadcastServiceSystemParameter.EnableOpenMarketInventoryForPricingModel)
                result.Add(InventorySourceTypeEnum.OpenMarket);

            if (BroadcastServiceSystemParameter.EnableBarterInventoryForPricingModel)
                result.Add(InventorySourceTypeEnum.Barter);

            if (BroadcastServiceSystemParameter.EnableProprietaryOAndOInventoryForPricingModel)
                result.Add(InventorySourceTypeEnum.ProprietaryOAndO);

            return result;
        }

        private void _ValidateInventory(List<PlanPricingInventoryProgram> inventory)
        {
            if (!inventory.Any())
            {
                throw new Exception("No inventory found for pricing run");
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
                    throw new Exception("Couldn't find the program in grouped inventory");

                var originalProgram = originalProgramGroup
                    .FirstOrDefault(x => x.Program.ManifestWeeks.Select(y => y.ContractMediaWeekId).Contains(allocation.MediaWeekId));

                if (originalProgram == null)
                    throw new Exception("Couldn't find the program and week combination from the allocation data");

                var originalSpot = pricingApiRequest.Spots.FirstOrDefault(x =>
                    x.Id == allocation.ManifestId &&
                    x.MediaWeekId == allocation.MediaWeekId);

                if (originalSpot == null)
                    throw new Exception("Response from API contains manifest id not found in sent data");

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
                    throw new Exception("Couldn't find the program in grouped inventory");

                var originalProgram = originalProgramGroup
                    .FirstOrDefault(x => x.Program.ManifestWeeks.Select(y => y.ContractMediaWeekId).Contains(allocation.MediaWeekId));

                if (originalProgram == null)
                    throw new Exception("Couldn't find the program and week combination from the allocation data");

                var originalSpot = pricingApiRequest.Spots.FirstOrDefault(x =>
                    x.Id == allocation.ManifestId &&
                    x.MediaWeekId == allocation.MediaWeekId);

                if (originalSpot == null)
                    throw new Exception("Response from API contains manifest id not found in sent data");

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
                    InventoryMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(inventoryWeek.InventoryMediaWeekId)
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
                throw new Exception(msg);
            }
        }

        public PlanPricingApiRequestDto GetPricingApiRequestPrograms(int planId, PricingInventoryGetRequestParametersDto requestParameters)
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
            var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                plan,
                pricingParams,
                inventorySourceIds,
                diagnostic,
                processingId);
            var groupedInventory = _GroupInventory(inventory);

            var pricingApiRequest = new PlanPricingApiRequestDto
            {
                Weeks = _GetPricingModelWeeks(plan, parameters, new ProprietaryInventoryData(), out List<int> skippedWeeksIds),
                Spots = _GetPricingModelSpots(groupedInventory, skippedWeeksIds)
            };

            return pricingApiRequest;
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

        /// <inheritdoc />
        public PlanPricingResultMarketsDto GetMarketsByJobId(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);

            return _GetMarkets(job);
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
    }
}
