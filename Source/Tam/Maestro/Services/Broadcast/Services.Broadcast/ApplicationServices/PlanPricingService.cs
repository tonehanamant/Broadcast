using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.PlanPricing;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
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
        void SavePricingRequest(int planId, PlanPricingApiRequestDto pricingApiRequest);
        [Queue("savepricingrequest")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void SavePricingRequest(int planId, PlanPricingApiRequestDto_v3 pricingApiRequest);
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
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IMarketRepository _MarketRepository;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IWeeklyBreakdownEngine _WeeklyBreakdownEngine;
        private readonly IPlanPricingBandCalculationEngine _PlanPricingBandCalculationEngine;
        private readonly IPlanPricingStationCalculationEngine _PlanPricingStationCalculationEngine;
        private readonly IPlanPricingMarketResultsEngine _PlanPricingMarketResultsEngine;
        private readonly IPricingRequestLogClient _PricingRequestLogClient;
        private readonly IPlanValidator _PlanValidator;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IAudienceService _AudienceService;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;

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
                                  IPricingRequestLogClient pricingRequestLogClient,
                                  IPlanValidator planValidator,
                                  ISharedFolderService sharedFolderService,
                                  IAudienceService audienceService,
                                  ICreativeLengthEngine creativeLengthEngine)
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
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _MarketRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>();
            _DateTimeEngine = dateTimeEngine;
            _WeeklyBreakdownEngine = weeklyBreakdownEngine;
            _PlanPricingBandCalculationEngine = planPricingBandCalculationEngine;
            _PlanPricingStationCalculationEngine = planPricingStationCalculationEngine;
            _PlanPricingMarketResultsEngine = planPricingMarketResultsEngine;
            _PricingRequestLogClient = pricingRequestLogClient;
            _PlanValidator = planValidator;
            _SharedFolderService = sharedFolderService;
            _AudienceService = audienceService;
            _CreativeLengthEngine = creativeLengthEngine;
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
            // this should be removed in the story that allows user to enter margin
            const int defaultMargin = 20;
            request.Margin = defaultMargin;

            var generatedTimeStamp = _DateTimeEngine.GetCurrentMoment();
            var allAudiences = _AudienceService.GetAudiences();
            var allMarkets = _MarketCoverageRepository.GetMarketsWithLatestCoverage();

            _LogInfo("Starting to gather inventory...");
            var programs = _PlanPricingInventoryEngine.GetInventoryForQuote(request);
            _LogInfo($"Finished gather inventory.  Gathered {programs.Count} programs.");

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
                InventorySourcePercentages = pricingParametersWithoutPlanDto.InventorySourcePercentages,
                InventorySourceTypePercentages = pricingParametersWithoutPlanDto.InventorySourceTypePercentages,
                JobId = pricingParametersWithoutPlanDto.JobId,
                Margin = pricingParametersWithoutPlanDto.Margin,
                MarketGroup = pricingParametersWithoutPlanDto.MarketGroup,
                MaxCpm = pricingParametersWithoutPlanDto.MaxCpm,
                MinCpm = pricingParametersWithoutPlanDto.MinCpm,
                ProprietaryBlend = pricingParametersWithoutPlanDto.ProprietaryBlend,
                UnitCaps = pricingParametersWithoutPlanDto.UnitCaps,
                UnitCapsType = pricingParametersWithoutPlanDto.UnitCapsType
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
            const int defaultPercent = 0;
            const float defaultMargin = 20;
            var allSources = _InventoryRepository.GetInventorySources();

            return new PlanPricingDefaults
            {
                UnitCaps = 1,
                UnitCapType = UnitCapEnum.Per30Min,
                InventorySourcePercentages = PlanInventorySourceSortEngine.GetSortedInventorySourcePercents(defaultPercent, allSources),
                InventorySourceTypePercentages = PlanInventorySourceSortEngine.GetSortedInventorySourceTypePercents(defaultPercent),
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

        public static bool IsPricingModelRunning(PlanPricingJob job)
        {
            return job != null && (job.Status == BackgroundJobProcessingStatus.Queued || job.Status == BackgroundJobProcessingStatus.Processing);
        }

        public bool IsPricingModelRunningForJob(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);
            return IsPricingModelRunning(job);
        }

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

        private List<PlanPricingApiRequestSpotsDto> _GetPricingModelSpots(
            List<IGrouping<PlanPricingInventoryGroup, ProgramWithManifestDaypart>> groupedInventory,
            List<int> skippedWeeksIds)
        {
            var marketCoveragesByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var pricingModelSpots = new List<PlanPricingApiRequestSpotsDto>();

            foreach (var inventoryGroupping in groupedInventory)
            {
                var programsInGrouping = inventoryGroupping.Select(x => x.Program).ToList();
                var manifestId = programsInGrouping.First().ManifestId;

                foreach (var program in programsInGrouping)
                {
                    foreach (var daypart in program.ManifestDayparts)
                    {
                        var impressions = program.Impressions;
                        var spotCost = program.ManifestRates.Single().Cost;

                        if (impressions <= 0)
                            continue;

                        if (spotCost <= 0)
                            continue;

                        //filter out skipped weeks
                        var spots = program.ManifestWeeks
                            .Where(x => !skippedWeeksIds.Contains(x.ContractMediaWeekId))
                            .Select(manifestWeek => new PlanPricingApiRequestSpotsDto
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
                            });

                        pricingModelSpots.AddRange(spots);
                    }
                }
            }

            return pricingModelSpots;
        }

        private List<PlanPricingApiRequestWeekDto> _GetPricingModelWeeks(
            PlanDto plan,
            List<PricingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto parameters,
            out List<int> SkippedWeeksIds)
        {
            SkippedWeeksIds = new List<int>();
            var pricingModelWeeks = new List<PlanPricingApiRequestWeekDto>();
            var marketCoverageGoal = GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value);
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);
            var shareOfVoice = _GetShareOfVoice(topMarkets, marketsWithSov);
            var daypartsWithWeighting = plan.Dayparts.Where(x => x.WeightingGoalPercent.HasValue);
            var planPricingParameters = plan.PricingParameters;

            var planWeeks = _CalculatePlanWeeksWithPricingParameters(plan);
            var weeklyBreakdownByWeek = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(planWeeks);

            foreach (var week in weeklyBreakdownByWeek)
            {
                var mediaWeekId = week.MediaWeekId;
                var planWeekImpressions = week.Impressions;
                var planWeekBudget = week.Budget;

                if (planWeekImpressions <= 0)
                {
                    SkippedWeeksIds.Add(mediaWeekId);
                    continue;
                }

                var estimatesForWeek = proprietaryEstimates.Where(x => x.MediaWeekId == mediaWeekId);
                var estimatedImpressions = estimatesForWeek.Sum(x => x.Impressions);
                var estimatedCost = estimatesForWeek.Sum(x => x.Cost);

                var impressionGoal = planWeekImpressions > estimatedImpressions ? planWeekImpressions - estimatedImpressions : 0;
                if (impressionGoal == 0)
                {   //proprietary fulfills this week goal so we're not sending the week
                    SkippedWeeksIds.Add(mediaWeekId);
                    continue;
                }

                var weeklyBudget = planWeekBudget > estimatedCost ? planWeekBudget - estimatedCost : 0;
                if (weeklyBudget == 0)
                {   //proprietary fulfills this week goal so we're not sending the week
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

        private List<WeeklyBreakdownWeek> _CalculatePlanWeeksWithPricingParameters(PlanDto plan)
        {
            var weeks = _WeeklyBreakdownEngine.GroupWeeklyBreakdownWeeksBasedOnDeliveryType(plan);

            var request = new WeeklyBreakdownRequest
            {
                CreativeLengths = plan.CreativeLengths,
                Dayparts = plan.Dayparts,
                DeliveryType = plan.GoalBreakdownType,
                FlightStartDate = plan.FlightStartDate.Value,
                FlightEndDate = plan.FlightEndDate.Value,
                FlightDays = plan.FlightDays,
                // Use parameter values for budget and impressions.
                TotalBudget = plan.PricingParameters.Budget,
                TotalImpressions = plan.PricingParameters.DeliveryImpressions * 1000,
                FlightHiatusDays = plan.FlightHiatusDays,
                TotalRatings = plan.TargetRatingPoints.Value,
                Weeks = weeks,
                ImpressionsPerUnit = plan.ImpressionsPerUnit,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
            };

            var weeklyBreakdown = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request);

            return weeklyBreakdown.Weeks;
        }

        private List<ShareOfVoice> _GetShareOfVoice(MarketCoverageDto topMarkets, IEnumerable<PlanAvailableMarketDto> marketsWithSov)
        {
            var topMarketsShareOfVoice = topMarkets.MarketCoveragesByMarketCode.Select(x => new ShareOfVoice
            {
                MarketCode = x.Key,
                MarketGoal = GeneralMath.ConvertPercentageToFraction(x.Value)
            }).ToList();

            var planShareOfVoices = marketsWithSov.Select(x => new ShareOfVoice
            {
                MarketCode = x.MarketCode,
                MarketGoal = GeneralMath.ConvertPercentageToFraction(x.ShareOfVoicePercent.Value)
            }).ToList();

            topMarketsShareOfVoice.RemoveAll(x => planShareOfVoices.Select(y => y.MarketCode).Contains(x.MarketCode));

            topMarketsShareOfVoice.AddRange(planShareOfVoices);

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

        private void _RunPricingJob(PlanPricingParametersDto planPricingParametersDto, PlanDto plan, int jobId, CancellationToken token)
        {
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
                    Margin = planPricingParametersDto.Margin,
                    MarketGroup = planPricingParametersDto.MarketGroup
                };
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_FETCHING_PLAN_AND_PARAMETERS);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_INVENTORY_SOURCE_ESTIMATES);
                var proprietaryEstimates = _CalculateProprietaryInventorySourceEstimates(plan, programInventoryParameters, diagnostic);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_INVENTORY_SOURCE_ESTIMATES);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_INVENTORY_SOURCE_ESTIMATES);
                _PlanRepository.SavePlanPricingEstimates(jobId, proprietaryEstimates);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_INVENTORY_SOURCE_ESTIMATES);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_GATHERING_INVENTORY);
                var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());
                var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    programInventoryParameters,
                    inventorySourceIds,
                    diagnostic,
                    isProprietary: false);

                _ValidateInventory(inventory);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_GATHERING_INVENTORY);

                token.ThrowIfCancellationRequested();

                var allocationResult = _SendPricingRequest(
                    jobId,
                    plan,
                    inventory,
                    proprietaryEstimates,
                    programInventoryParameters,
                    token,
                    diagnostic,
                    out var goalsFulfilledByProprietaryInventory);

                token.ThrowIfCancellationRequested();

                var aggregateResultsTask = new Task<PlanPricingResultBaseDto>(() =>
                {
                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                    var aggregatedResults = _AggregateResults(inventory, allocationResult, goalsFulfilledByProprietaryInventory);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                    return aggregatedResults;
                });

                var calculatePricingBandsTask = new Task<PlanPricingBandDto>(() =>
                {
                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_BANDS);
                    var pricingBands = _PlanPricingBandCalculationEngine.CalculatePricingBands(inventory, allocationResult, planPricingParametersDto);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_BANDS);
                    return pricingBands;
                });

                var calculatePricingStationsTask = new Task<PlanPricingStationResultDto>(() =>
                {
                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_STATIONS);
                    var pricingStations = _PlanPricingStationCalculationEngine.Calculate(inventory, allocationResult, planPricingParametersDto);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_STATIONS);
                    return pricingStations;
                });

                var aggregateMarketResultsTask = new Task<PlanPricingResultMarketsDto>(() =>
                {
                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                    var marketCoverages = _MarketCoverageRepository.GetMarketsWithLatestCoverage();
                    var pricingMarketResults = _PlanPricingMarketResultsEngine.Calculate(inventory, allocationResult, planPricingParametersDto, plan, marketCoverages);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                    return pricingMarketResults;
                });

                aggregateResultsTask.Start();
                calculatePricingBandsTask.Start();
                calculatePricingStationsTask.Start();
                aggregateMarketResultsTask.Start();

                token.ThrowIfCancellationRequested();

                aggregateResultsTask.Wait();
                var aggregateTaskResult = aggregateResultsTask.Result;

                calculatePricingBandsTask.Wait();
                var calculatePricingBandTaskResult = calculatePricingBandsTask.Result;

                calculatePricingStationsTask.Wait();
                var calculatePricingStationTaskResult = calculatePricingStationsTask.Result;

                aggregateMarketResultsTask.Wait();
                var aggregateMarketResultsTaskResult = aggregateMarketResultsTask.Result;

                using (var transaction = new TransactionScopeWrapper())
                {
                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);
                    _PlanRepository.SavePricingApiResults(allocationResult);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);

                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);
                    _PlanRepository.SavePricingAggregateResults(aggregateTaskResult);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);

                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_BANDS);
                    _PlanRepository.SavePlanPricingBands(calculatePricingBandTaskResult);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_BANDS);

                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_STATIONS);
                    _PlanRepository.SavePlanPricingStations(calculatePricingStationTaskResult);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_STATIONS);

                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);
                    _PlanRepository.SavePlanPricingMarketResults(aggregateMarketResultsTaskResult);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);

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

        private PlanPricingAllocationResult _SendPricingRequest(
            int jobId,
            PlanDto plan,
            List<PlanPricingInventoryProgram> inventory,
            List<PricingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto programInventoryParameters,
            CancellationToken token,
            PlanPricingJobDiagnostic diagnostic,
            out bool goalsFulfilledByProprietaryInventory)
        {
            goalsFulfilledByProprietaryInventory = false;
            var allocationResult = new PlanPricingAllocationResult
            {
                Spots = new List<PlanPricingAllocatedSpot>(),
                JobId = jobId,
                PlanVersionId = plan.VersionId,
                PricingVersion = BroadcastServiceSystemParameter.PlanPricingEndpointVersion
            };

            if (BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "2")
            {
                _SendPricingRequest_v2(
                    allocationResult,
                    plan,
                    inventory,
                    proprietaryEstimates,
                    programInventoryParameters,
                    token,
                    diagnostic,
                    out goalsFulfilledByProprietaryInventory);
            }
            else if (BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "3")
            {
                _SendPricingRequest_v3(
                    allocationResult,
                    plan,
                    inventory,
                    proprietaryEstimates,
                    programInventoryParameters,
                    token,
                    diagnostic,
                    out goalsFulfilledByProprietaryInventory);
            }
            else
            {
                throw new Exception("Unknown pricing API version was discovered");
            }

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_CPM);
            allocationResult.PricingCpm = _CalculatePricingCpm(allocationResult.Spots, proprietaryEstimates);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALCULATING_PRICING_CPM);

            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);
            _ValidateAllocationResult(allocationResult);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);

            return allocationResult;
        }

        private void _SendPricingRequest_v2(
            PlanPricingAllocationResult allocationResult,
            PlanDto plan,
            List<PlanPricingInventoryProgram> inventory,
            List<PricingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto programInventoryParameters,
            CancellationToken token,
            PlanPricingJobDiagnostic diagnostic,
            out bool goalsFulfilledByProprietaryInventory)
        {
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var pricingModelWeeks = _GetPricingModelWeeks(plan, proprietaryEstimates, programInventoryParameters, out List<int> skippedWeeksIds);
            goalsFulfilledByProprietaryInventory = pricingModelWeeks.IsEmpty();

            var groupedInventory = _GroupInventory(inventory);
            var spots = _GetPricingModelSpots(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            token.ThrowIfCancellationRequested();

            if (!pricingModelWeeks.IsEmpty())
            {
                var pricingApiRequest = new PlanPricingApiRequestDto
                {
                    Weeks = pricingModelWeeks,
                    Spots = spots
                };

                _BackgroundJobClient.Enqueue<IPlanPricingService>(x => x.SavePricingRequest(plan.Id, pricingApiRequest));

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
                allocationResult.Spots = _MapToResultSpots(groupedInventory, apiAllocationResult, pricingApiRequest, inventory, programInventoryParameters);
                allocationResult.RequestId = apiAllocationResult.RequestId;
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
            }
        }

        private void _SendPricingRequest_v3(
            PlanPricingAllocationResult allocationResult,
            PlanDto plan,
            List<PlanPricingInventoryProgram> inventory,
            List<PricingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto programInventoryParameters,
            CancellationToken token,
            PlanPricingJobDiagnostic diagnostic,
            out bool goalsFulfilledByProprietaryInventory)
        {
            diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var pricingModelWeeks = _GetPricingModelWeeks_v3(plan, proprietaryEstimates, programInventoryParameters, out List<int> skippedWeeksIds);
            goalsFulfilledByProprietaryInventory = pricingModelWeeks.IsEmpty();

            var groupedInventory = _GroupInventory(inventory);
            var spots = _GetPricingModelSpots_v3(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            token.ThrowIfCancellationRequested();

            if (!pricingModelWeeks.IsEmpty())
            {
                var pricingApiRequest = new PlanPricingApiRequestDto_v3
                {
                    Weeks = pricingModelWeeks,
                    Spots = spots
                };

                _BackgroundJobClient.Enqueue<IPlanPricingService>(x => x.SavePricingRequest(plan.Id, pricingApiRequest));

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
                allocationResult.Spots = _MapToResultSpots(groupedInventory, apiAllocationResult, pricingApiRequest, inventory, programInventoryParameters, plan);
                allocationResult.RequestId = apiAllocationResult.RequestId;
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
            }
        }

        private List<PlanPricingApiRequestSpotsDto_v3> _GetPricingModelSpots_v3(
            List<IGrouping<PlanPricingInventoryGroup, ProgramWithManifestDaypart>> groupedInventory,
            List<int> skippedWeeksIds)
        {
            var marketCoveragesByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var pricingModelSpots = new List<PlanPricingApiRequestSpotsDto_v3>();

            foreach (var inventoryGroupping in groupedInventory)
            {
                var programsInGrouping = inventoryGroupping.Select(x => x.Program).ToList();
                var manifestId = programsInGrouping.First().ManifestId;

                foreach (var program in programsInGrouping)
                {
                    foreach (var daypart in program.ManifestDayparts)
                    {
                        var impressions = program.Impressions;

                        if (impressions <= 0)
                            continue;

                        //filter out skipped weeks
                        var spots = program.ManifestWeeks
                            .Where(x => !skippedWeeksIds.Contains(x.ContractMediaWeekId))
                            .Select(manifestWeek => new PlanPricingApiRequestSpotsDto_v3
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
                                SpotCost = program.ManifestRates.Select(x => new SpotCost_v3
                                {
                                    SpotLengthId = x.SpotLengthId,
                                    SpotLengthCost = x.Cost
                                }).ToList()
                            });

                        pricingModelSpots.AddRange(spots);
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
            var plan = _PlanRepository.GetPlan(planPricingParametersDto.PlanId.Value);

            _RunPricingJob(planPricingParametersDto, plan, jobId, token);
        }

        private List<PlanPricingApiRequestWeekDto_v3> _GetPricingModelWeeks_v3(
            PlanDto plan,
            List<PricingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto parameters,
            out List<int> SkippedWeeksIds)
        {
            SkippedWeeksIds = new List<int>();
            var pricingModelWeeks = new List<PlanPricingApiRequestWeekDto_v3>();
            var marketCoverageGoal = GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value);
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);
            var shareOfVoice = _GetShareOfVoice(topMarkets, marketsWithSov);
            var daypartsWithWeighting = plan.Dayparts.Where(x => x.WeightingGoalPercent.HasValue);
            var planPricingParameters = plan.PricingParameters;

            var planWeeks = _CalculatePlanWeeksWithPricingParameters(plan);
            var weeklyBreakdownByWeek = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(planWeeks);
            var spotScaleFactorBySpotLengthId = _GetSpotScaleFactorBySpotLengthId(plan);

            foreach (var week in weeklyBreakdownByWeek)
            {
                var mediaWeekId = week.MediaWeekId;
                var planWeekImpressions = week.Impressions;
                var planWeekBudget = week.Budget;

                if (planWeekImpressions <= 0)
                {
                    SkippedWeeksIds.Add(mediaWeekId);
                    continue;
                }

                var estimatesForWeek = proprietaryEstimates.Where(x => x.MediaWeekId == mediaWeekId);
                var estimatedImpressions = estimatesForWeek.Sum(x => x.Impressions);
                var estimatedCost = estimatesForWeek.Sum(x => x.Cost);

                var impressionGoal = planWeekImpressions > estimatedImpressions ? planWeekImpressions - estimatedImpressions : 0;

                if (impressionGoal == 0)
                {
                    // proprietary fulfills this week goal so we're not sending the week
                    SkippedWeeksIds.Add(mediaWeekId);
                    continue;
                }

                var weeklyBudget = planWeekBudget > estimatedCost ? planWeekBudget - estimatedCost : 0;

                if (weeklyBudget == 0)
                {
                    // proprietary fulfills this week goal so we're not sending the week
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
                x => plan.Equivalized ? _SpotLengthEngine.GetDeliveryMultiplierBySpotLengthId(x.SpotLengthId) : 1);
        }

        private List<IGrouping<PlanPricingInventoryGroup, ProgramWithManifestDaypart>> _GroupInventory(List<PlanPricingInventoryProgram> inventory)
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

        public void SavePricingRequest(int planId, PlanPricingApiRequestDto pricingApiRequest)
        {
            try
            {
                _PricingRequestLogClient.SavePricingRequest(planId, pricingApiRequest);
            }
            catch (Exception exception)
            {
                _LogError("Failed to save pricing API request", exception);
            }
        }

        public void SavePricingRequest(int planId, PlanPricingApiRequestDto_v3 pricingApiRequest)
        {
            try
            {
                _PricingRequestLogClient.SavePricingRequest(planId, pricingApiRequest);
            }
            catch (Exception exception)
            {
                _LogError("Failed to save pricing API request", exception);
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

        internal decimal _CalculatePricingCpm(List<PlanPricingAllocatedSpot> spots, List<PricingEstimate> proprietaryEstimates)
        {
            var totalCost = proprietaryEstimates.Sum(x => x.Cost);
            var totalImpressions = proprietaryEstimates.Sum(x => x.Impressions);

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

        private List<PricingEstimate> _CalculateProprietaryInventorySourceEstimates(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            PlanPricingJobDiagnostic diagnostic)
        {
            var result = new List<PricingEstimate>();

            result.AddRange(_GetPricingEstimatesBasedOnInventorySourcePreferences(plan, parameters, diagnostic));
            result.AddRange(_GetPricingEstimatesBasedOnInventorySourceTypePreferences(plan, parameters, diagnostic));

            return result;
        }

        private List<PricingEstimate> _GetPricingEstimatesBasedOnInventorySourcePreferences(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            PlanPricingJobDiagnostic diagnostic)
        {
            var result = new List<PricingEstimate>();

            var supportedInventorySourceIds = _GetInventorySourceIdsByTypes(new List<InventorySourceTypeEnum>
            {
                InventorySourceTypeEnum.Barter,
                InventorySourceTypeEnum.ProprietaryOAndO
            });

            var inventorySourcePreferences = plan.PricingParameters.InventorySourcePercentages
                .Where(x => x.Percentage > 0 && supportedInventorySourceIds.Contains(x.Id))
                .ToList();

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    inventorySourcePreferences.Select(x => x.Id),
                    diagnostic,
                    isProprietary: true);

            foreach (var preference in inventorySourcePreferences)
            {
                var inventorySourceId = preference.Id;
                var programs = inventory.Where(x => x.InventorySource.Id == inventorySourceId);

                var estimates = _GetPricingEstimates(
                    programs,
                    preference.Percentage,
                    inventorySourceId,
                    null);

                result.AddRange(estimates);
            }

            return result;
        }

        private List<PricingEstimate> _GetPricingEstimatesBasedOnInventorySourceTypePreferences(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            PlanPricingJobDiagnostic diagnostic)
        {
            var result = new List<PricingEstimate>();

            var supportedInventorySourceTypes = new List<InventorySourceTypeEnum>
            {
                InventorySourceTypeEnum.Diginet
            };

            var inventorySourceTypePreferences = plan.PricingParameters.InventorySourceTypePercentages
                .Where(x => x.Percentage > 0 && supportedInventorySourceTypes.Contains((InventorySourceTypeEnum)x.Id))
                .ToList();

            var inventorySourceIds = _GetInventorySourceIdsByTypes(inventorySourceTypePreferences.Select(x => (InventorySourceTypeEnum)x.Id));

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    inventorySourceIds,
                    diagnostic,
                    isProprietary: true);

            foreach (var preference in inventorySourceTypePreferences)
            {
                var inventorySourceType = (InventorySourceTypeEnum)preference.Id;
                var programs = inventory.Where(x => x.InventorySource.InventoryType == inventorySourceType);

                var estimates = _GetPricingEstimates(
                    programs,
                    preference.Percentage,
                    null,
                    inventorySourceType);

                result.AddRange(estimates);
            }

            return result;
        }

        private IEnumerable<PricingEstimate> _GetPricingEstimates(
            IEnumerable<PlanPricingInventoryProgram> programs,
            int percentage,
            int? inventorySourceId,
            InventorySourceTypeEnum? inventorySourceType)
        {
            return programs
                .SelectMany(x => x.ManifestWeeks.Select(w => new ProgramWithManifestWeek
                {
                    Program = x,
                    ManifestWeek = w
                }))
                .Where(x => x.ManifestWeek.Spots > 0)
                .GroupBy(x => x.ManifestWeek.ContractMediaWeekId)
                .Select(programsByMediaWeekGrouping =>
                {
                    _CalculateImpressionsAndCost(
                        programsByMediaWeekGrouping,
                        percentage,
                        out var totalWeekImpressions,
                        out var totalWeekCost);

                    return new PricingEstimate
                    {
                        InventorySourceId = inventorySourceId,
                        InventorySourceType = inventorySourceType,
                        MediaWeekId = programsByMediaWeekGrouping.Key,
                        Impressions = totalWeekImpressions,
                        Cost = totalWeekCost
                    };
                });
        }

        private void _CalculateImpressionsAndCost(
            IEnumerable<ProgramWithManifestWeek> programsWithManifestWeeks,
            int percentageToUse,
            out double totalWeekImpressions,
            out decimal totalWeekCost)
        {
            totalWeekImpressions = 0;
            totalWeekCost = 0;

            foreach (var programWithManifestWeek in programsWithManifestWeeks)
            {
                var program = programWithManifestWeek.Program;
                var spots = programWithManifestWeek.ManifestWeek.Spots;
                var impressionsPerSpot = program.Impressions;
                var impressions = impressionsPerSpot * spots * percentageToUse / 100;

                // for proprietary there is only one cost, we don`t calculate cost per each existing spot length
                var cost = program.ManifestRates.Single().Cost * spots * percentageToUse / 100;

                totalWeekImpressions += impressions;
                totalWeekCost += cost;
            }
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
            ProgramInventoryOptionalParametersDto programInventoryParameters)
        {
            var results = new List<PlanPricingAllocatedSpot>();
            var daypartDefaultsById = _DaypartDefaultRepository
                .GetAllDaypartDefaults()
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
                            SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(originalSpot.Cost, programInventoryParameters.Margin),
                            Spots = allocation.Frequency,
                            Impressions = originalSpot.Impressions,
                        }
                    },
                    StandardDaypart = daypartDefaultsById[originalSpot.DaypartId],
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
            ProgramInventoryOptionalParametersDto programInventoryParameters,
            PlanDto plan)
        {
            var results = new List<PlanPricingAllocatedSpot>();
            var daypartDefaultsById = _DaypartDefaultRepository
                .GetAllDaypartDefaults()
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
                            SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(spotCostBySpotLengthId[x.SpotLengthId], programInventoryParameters.Margin),
                            Spots = x.Frequency,
                            Impressions = originalSpot.Impressions30sec * spotScaleFactorBySpotLengthId[x.SpotLengthId]
                        })
                        .ToList(),
                    StandardDaypart = daypartDefaultsById[originalSpot.DaypartId],
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

        private PlanPricingResultBaseDto _AggregateResults(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult apiResponse,
            bool goalsFulfilledByProprietaryInventory = false)
        {
            var result = new PlanPricingResultBaseDto();
            var programs = _GetPrograms(inventory, apiResponse);
            var totalCostForAllPrograms = programs.Sum(x => x.TotalCost);
            var totalImpressionsForAllPrograms = programs.Sum(x => x.TotalImpressions);
            var totalSpotsForAllPrograms = programs.Sum(x => x.TotalSpots);

            result.Programs.AddRange(programs.Select(x => new PlanPricingProgramDto
            {
                ProgramName = x.ProgramName,
                Genre = x.Genre,
                StationCount = x.Stations.Count,
                MarketCount = x.MarketCodes.Count,
                AvgImpressions = x.AvgImpressions,
                Impressions = x.TotalImpressions,
                AvgCpm = x.AvgCpm,
                PercentageOfBuy = ProposalMath.CalculateImpressionsPercentage(x.TotalImpressions, totalImpressionsForAllPrograms),
                Budget = x.TotalCost,
                Spots = x.TotalSpots
            }));

            result.Totals = new PlanPricingProgramTotalsDto
            {
                MarketCount = programs.SelectMany(x => x.MarketCodes).Distinct().Count(),
                StationCount = programs.SelectMany(x => x.Stations).Distinct().Count(),
                AvgImpressions = ProposalMath.CalculateAvgImpressions(totalImpressionsForAllPrograms, totalSpotsForAllPrograms),
                AvgCpm = ProposalMath.CalculateCpm(totalCostForAllPrograms, totalImpressionsForAllPrograms),
                Budget = totalCostForAllPrograms,
                Impressions = totalImpressionsForAllPrograms,
                Spots = totalSpotsForAllPrograms,
            };

            result.GoalFulfilledByProprietary = goalsFulfilledByProprietaryInventory;
            result.OptimalCpm = apiResponse.PricingCpm;
            result.JobId = apiResponse.JobId;
            result.PlanVersionId = apiResponse.PlanVersionId;

            return result;
        }

        private List<PlanPricingProgram> _GetPrograms(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult apiResponse)
        {
            var result = new List<PlanPricingProgram>();
            var inventoryGroupedByProgramName = inventory
                .SelectMany(x => x.ManifestDayparts.Select(d => new PlanPricingManifestWithManifestDaypart
                {
                    Manifest = x,
                    ManifestDaypart = d
                }))
                .GroupBy(x => x.ManifestDaypart.PrimaryProgram.Name);

            foreach (var inventoryByProgramName in inventoryGroupedByProgramName)
            {
                var programInventory = inventoryByProgramName.ToList();
                var allocatedStations = _GetAllocatedStations(apiResponse, programInventory);
                var allocatedProgramSpots = _GetAllocatedProgramSpots(apiResponse, programInventory);

                _CalculateProgramTotals(allocatedProgramSpots, out var programCost, out var programImpressions, out var programSpots);

                if (programSpots == 0)
                    continue;

                var program = new PlanPricingProgram
                {
                    ProgramName = inventoryByProgramName.Key,
                    Genre = inventoryByProgramName.First().ManifestDaypart.PrimaryProgram.Genre, // we assume all programs with the same name have the same genre
                    AvgImpressions = ProposalMath.CalculateAvgImpressions(programImpressions, programSpots),
                    AvgCpm = ProposalMath.CalculateCpm(programCost, programImpressions),
                    TotalImpressions = programImpressions,
                    TotalCost = programCost,
                    TotalSpots = programSpots,
                    Stations = allocatedStations.Select(s => s.LegacyCallLetters).Distinct().ToList(),
                    MarketCodes = allocatedStations.Select(s => s.MarketCode.Value).Distinct().ToList()
                };

                result.Add(program);
            };

            return result;
        }

        private List<DisplayBroadcastStation> _GetAllocatedStations(PlanPricingAllocationResult apiResponse, List<PlanPricingManifestWithManifestDaypart> programInventory)
        {
            var manifestIds = apiResponse.Spots.Select(s => s.Id).Distinct();
            var result = new List<PlanPricingManifestWithManifestDaypart>();
            return programInventory.Where(p => manifestIds.Contains(p.Manifest.ManifestId)).Select(p => p.Manifest.Station).ToList();
        }

        private List<PlanPricingAllocatedSpot> _GetAllocatedProgramSpots(PlanPricingAllocationResult apiResponse, List<PlanPricingManifestWithManifestDaypart> programInventory)
        {
            var result = new List<PlanPricingAllocatedSpot>();

            foreach (var spot in apiResponse.Spots)
            {
                // until we use only OpenMarket inventory it`s fine
                // this needs to be updated when we start using inventory that can have more than one daypart
                // we should match spots by some unique value which represents a combination of a manifest week and a manifest daypart
                // and not by manifest id as it is done now
                if (programInventory.Any(x => x.Manifest.ManifestId == spot.Id))
                {
                    result.Add(spot);
                }
            }

            return result;
        }

        private void _CalculateProgramTotals(
            IEnumerable<PlanPricingAllocatedSpot> allocatedProgramSpots,
            out decimal totalProgramCost,
            out double totalProgramImpressions,
            out int totalProgramSpots)
        {
            totalProgramCost = 0;
            totalProgramImpressions = 0;
            totalProgramSpots = 0;

            foreach (var apiProgram in allocatedProgramSpots)
            {
                totalProgramCost += apiProgram.TotalCostWithMargin;
                totalProgramImpressions += apiProgram.TotalImpressions;
                totalProgramSpots += apiProgram.TotalSpots;
            }
        }

        public PlanPricingApiRequestDto GetPricingApiRequestPrograms(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            var diagnostic = new PlanPricingJobDiagnostic();
            var pricingParams = new ProgramInventoryOptionalParametersDto
            {
                MinCPM = requestParameters.MinCpm,
                MaxCPM = requestParameters.MaxCpm,
                InflationFactor = requestParameters.InflationFactor,
                Margin = requestParameters.Margin,
                MarketGroup = requestParameters.MarketGroup
            };

            var plan = _PlanRepository.GetPlan(planId);
            var inventorySourceIds = requestParameters.InventorySourceIds.IsEmpty() ?
                _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes()) :
                requestParameters.InventorySourceIds;

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                plan,
                pricingParams,
                inventorySourceIds,
                diagnostic,
                isProprietary: !requestParameters.InventorySourceIds.IsEmpty());
            var groupedInventory = _GroupInventory(inventory);
            var proprietaryEstimates = _CalculateProprietaryInventorySourceEstimates(plan, pricingParams, diagnostic);

            var pricingApiRequest = new PlanPricingApiRequestDto
            {
                Weeks = _GetPricingModelWeeks(plan, proprietaryEstimates, pricingParams, out List<int> skippedWeeksIds),
                Spots = _GetPricingModelSpots(groupedInventory, skippedWeeksIds)
            };

            return pricingApiRequest;
        }

        public PlanPricingApiRequestDto_v3 GetPricingApiRequestPrograms_v3(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            var diagnostic = new PlanPricingJobDiagnostic();
            var pricingParams = new ProgramInventoryOptionalParametersDto
            {
                MinCPM = requestParameters.MinCpm,
                MaxCPM = requestParameters.MaxCpm,
                InflationFactor = requestParameters.InflationFactor,
                Margin = requestParameters.Margin,
                MarketGroup = requestParameters.MarketGroup
            };

            var plan = _PlanRepository.GetPlan(planId);
            var inventorySourceIds = requestParameters.InventorySourceIds.IsEmpty() ?
                _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes()) :
                requestParameters.InventorySourceIds;

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                plan,
                pricingParams,
                inventorySourceIds,
                diagnostic,
                isProprietary: !requestParameters.InventorySourceIds.IsEmpty());
            var groupedInventory = _GroupInventory(inventory);
            var proprietaryEstimates = _CalculateProprietaryInventorySourceEstimates(plan, pricingParams, diagnostic);

            var pricingApiRequest = new PlanPricingApiRequestDto_v3
            {
                Weeks = _GetPricingModelWeeks_v3(plan, proprietaryEstimates, pricingParams, out List<int> skippedWeeksIds),
                Spots = _GetPricingModelSpots_v3(groupedInventory, skippedWeeksIds)
            };

            return pricingApiRequest;
        }

        public List<PlanPricingInventoryProgram> GetPricingInventory(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            var diagnostic = new PlanPricingJobDiagnostic();
            var plan = _PlanRepository.GetPlan(planId);
            var pricingParams = new ProgramInventoryOptionalParametersDto
            {
                MinCPM = requestParameters.MinCpm,
                MaxCPM = requestParameters.MaxCpm,
                InflationFactor = requestParameters.InflationFactor,
                MarketGroup = requestParameters.MarketGroup
            };
            var inventorySourceIds = requestParameters.InventorySourceIds.IsEmpty() ?
                _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes()) :
                requestParameters.InventorySourceIds;

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                plan,
                pricingParams,
                inventorySourceIds,
                diagnostic,
                isProprietary: !requestParameters.InventorySourceIds.IsEmpty());

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

            _ConvertPricingBandImpressionsToUserFormat(results);

            return results;
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

            _ConvertPricingMarketResultsToUserFormat(results);

            return results;
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

            _ConvertPricingStationResultDtoToUserFormat(result);

            return result;
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

        private class ProgramWithManifestDaypart
        {
            public PlanPricingInventoryProgram Program { get; set; }

            public PlanPricingInventoryProgram.ManifestDaypart ManifestDaypart { get; set; }
        }

        private class ProgramWithManifestWeek
        {
            public PlanPricingInventoryProgram Program { get; set; }

            public PlanPricingInventoryProgram.ManifestWeek ManifestWeek { get; set; }
        }
    }
}
