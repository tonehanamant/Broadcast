using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.PlanBuying;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.BuyingResults;
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
using static Services.Broadcast.BusinessEngines.PlanBuyingInventoryEngine;

namespace Services.Broadcast.ApplicationServices.Plan
{
    public interface IPlanBuyingService : IApplicationService
    {
        PlanBuyingJob QueueBuyingJob(PlanBuyingParametersDto planBuyingParametersDto, DateTime currentDate, string username);

        CurrentBuyingExecution GetCurrentBuyingExecution(int planId);

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
        void RunBuyingJob(PlanBuyingParametersDto planBuyingParametersDto, int jobId, CancellationToken token);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        List<PlanBuyingApiRequestParametersDto> GetPlanBuyingRuns(int planId);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        PlanBuyingApiRequestDto GetBuyingApiRequestPrograms(int planId, BuyingInventoryGetRequestParametersDto requestParameters);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        PlanBuyingApiRequestDto_v3 GetBuyingApiRequestPrograms_v3(int planId, BuyingInventoryGetRequestParametersDto requestParameters);

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

        bool IsBuyingModelRunningForPlan(int planId);
        bool IsBuyingModelRunningForJob(int jobId);

        /// <summary>
        /// For troubleshooting
        /// </summary>
        string ForceCompletePlanBuyingJob(int jobId, string username);

        /// <summary>
        /// For troubleshooting.  This will bypass the queue to allow rerunning directly.
        /// </summary>
        /// <param name="jobId">The id of the job to rerun.</param>
        /// <returns>The new JobId</returns>
        int ReRunBuyingJob(int jobId);

        /// <summary>
        /// For troubleshooting.  Generate a buying results report for the chosen plan and version
        /// </summary>
        /// <param name="planId">The plan id</param>
        /// <param name="planVersionNumber">The plan version number</param>
        /// <param name="templatesFilePath">Base path of the file templates</param>
        /// <returns>ReportOutput which contains filename and MemoryStream which actually contains report data</returns>
        ReportOutput GenerateBuyingResultsReport(int planId, int? planVersionNumber, string templatesFilePath);

        void ValidateAndApplyMargin(PlanBuyingParametersDto parameters);

        BuyingProgramsResultDto GetPrograms(int planId);

        BuyingProgramsResultDto GetProgramsByJobId(int jobId);

        PlanBuyingStationResultDto GetStations(int planId);

        PlanBuyingStationResultDto GetStationsByJobId(int jobId);

        /// <summary>
        /// Retrieves the Buying Results Markets Summary
        /// </summary>
        PlanBuyingResultMarketsDto GetMarkets(int planId);

        /// <summary>
        /// Retrieves the Buying Results Markets Summary
        /// </summary>
        PlanBuyingResultMarketsDto GetMarketsByJobId(int jobId);

        PlanBuyingBandsDto GetBuyingBands(int planId);
        PlanBuyingBandsDto GetBuyingBandsByJobId(int jobId);

        [Queue("savebuyingrequest")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto buyingApiRequest);

        [Queue("savebuyingrequest")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto_v3 buyingApiRequest);
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
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IMarketRepository _MarketRepository;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IWeeklyBreakdownEngine _WeeklyBreakdownEngine;
        private readonly IPlanBuyingBandCalculationEngine _PlanBuyingBandCalculationEngine;
        private readonly IPlanBuyingStationCalculationEngine _PlanBuyingStationCalculationEngine;
        private readonly IPlanBuyingMarketResultsEngine _PlanBuyingMarketResultsEngine;
        private readonly IPlanBuyingRequestLogClient _BuyingRequestLogClient;
        private readonly IPlanValidator _PlanValidator;

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
                                  IPlanBuyingStationCalculationEngine planBuyingStationCalculationEngine,
                                  IPlanBuyingMarketResultsEngine planBuyingMarketResultsEngine,
                                  IPlanBuyingRequestLogClient buyingRequestLogClient,
                                  IPlanValidator planValidator)
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
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _MarketRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>();
            _DateTimeEngine = dateTimeEngine;
            _WeeklyBreakdownEngine = weeklyBreakdownEngine;
            _PlanBuyingBandCalculationEngine = planBuyingBandCalculationEngine;
            _PlanBuyingStationCalculationEngine = planBuyingStationCalculationEngine;
            _PlanBuyingMarketResultsEngine = planBuyingMarketResultsEngine;
            _BuyingRequestLogClient = buyingRequestLogClient;
            _PlanValidator = planValidator;
        }

        public ReportOutput GenerateBuyingResultsReport(int planId, int? planVersionNumber, string templatesFilePath)
        {
            var reportData = GetBuyingResultsReportData(planId, planVersionNumber);
            var reportGenerator = new BuyingResultsReportGenerator(templatesFilePath);
            var report = reportGenerator.Generate(reportData);

            return report;
        }

        public BuyingResultsReportData GetBuyingResultsReportData(int planId, int? planVersionNumber)
        {
            // use passed version or the current version by default
            var planVersionId = planVersionNumber.HasValue ?
                _PlanRepository.GetPlanVersionIdByVersionNumber(planId, planVersionNumber.Value) :
                (int?)null;

            var plan = _PlanRepository.GetPlan(planId, planVersionId);
            var allocatedSpots = _PlanBuyingRepository.GetPlanBuyingAllocatedSpotsByPlanVersionId(plan.VersionId);
            var manifestIds = allocatedSpots.Select(x => x.StationInventoryManifestId).Distinct();
            var manifests = _InventoryRepository.GetStationInventoryManifestsByIds(manifestIds);
            var manifestDaypartIds = manifests.SelectMany(x => x.ManifestDayparts).Select(x => x.Id.Value);
            var primaryProgramsByManifestDaypartIds = _StationProgramRepository.GetPrimaryProgramsForManifestDayparts(manifestDaypartIds);
            var markets = _MarketRepository.GetMarketDtos();

            return new BuyingResultsReportData(
                plan,
                allocatedSpots,
                manifests,
                primaryProgramsByManifestDaypartIds,
                markets,
                _WeeklyBreakdownEngine);
        }

        public PlanBuyingJob QueueBuyingJob(PlanBuyingParametersDto planBuyingParametersDto
            , DateTime currentDate, string username)
        {
            // lock the plan so that two requests for the same plan can not get in this area concurrently
            var key = KeyHelper.GetPlanLockingKey(planBuyingParametersDto.PlanId.Value);
            var lockObject = _LockingManagerApplicationService.GetNotUserBasedLockObjectForKey(key);

            lock (lockObject)
            {
                if (IsBuyingModelRunningForPlan(planBuyingParametersDto.PlanId.Value))
                {
                    throw new Exception("The buying model is already running for the plan");
                }

                var plan = _PlanRepository.GetPlan(planBuyingParametersDto.PlanId.Value);

                ValidateAndApplyMargin(planBuyingParametersDto);

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

                job.HangfireJobId = _BackgroundJobClient.Enqueue<IPlanBuyingService>(x => x.RunBuyingJob(planBuyingParametersDto, job.Id
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
            const int defaultPercent = 0;
            const float defaultMargin = 20;
            var allSources = _InventoryRepository.GetInventorySources();

            return new PlanBuyingDefaults
            {
                UnitCaps = 1,
                UnitCapType = UnitCapEnum.Per30Min,
                InventorySourcePercentages = PlanInventorySourceSortEngine.GetSortedInventorySourcePercents(defaultPercent, allSources),
                InventorySourceTypePercentages = PlanInventorySourceSortEngine.GetSortedInventorySourceTypePercents(defaultPercent),
                Margin = defaultMargin,
                MarketGroup = MarketGroupEnum.Top100
            };
        }

        public CurrentBuyingExecution GetCurrentBuyingExecutionByJobId(int jobId)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);

            return _GetCurrentBuyingExecution(job, null);
        }

        private CurrentBuyingExecution _GetCurrentBuyingExecution(PlanBuyingJob job, int? planId)
        {
            CurrentBuyingExecutionResultDto buyingExecutionResult = null;

            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                //in case the error is comming from the Buying Run model, the error message field will have better
                //message then the generic we construct here
                if (string.IsNullOrWhiteSpace(job.DiagnosticResult))
                    throw new Exception(job.ErrorMessage);
                throw new Exception(
                    "Error encountered while running Buying Model, please contact a system administrator for help");
            }

            if (job != null && job.Status == BackgroundJobProcessingStatus.Succeeded)
            {
                buyingExecutionResult = _PlanBuyingRepository.GetBuyingResultsByJobId(job.Id);

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
                                buyingExecutionResult.JobId.Value);
                        else
                            goalCpm = _PlanBuyingRepository.GetGoalCpm(buyingExecutionResult.JobId.Value);

                        buyingExecutionResult.CpmPercentage =
                            CalculateCpmPercentage(buyingExecutionResult.OptimalCpm, goalCpm);
                    }
                }
            }

            //buyingExecutionResult might be null when there is no buying run for the latest version            
            return new CurrentBuyingExecution
            {
                Job = job,
                Result = buyingExecutionResult ?? new CurrentBuyingExecutionResultDto(),
                IsBuyingModelRunning = IsBuyingModelRunning(job)
            };
        }

        public CurrentBuyingExecution GetCurrentBuyingExecution(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            return _GetCurrentBuyingExecution(job, planId);
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
        public PlanBuyingResponseDto CancelCurrentBuyingExecutionByJobId(int jobId)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);

            return _CancelCurrentBuyingExecution(job);
        }

        private PlanBuyingResponseDto _CancelCurrentBuyingExecution(PlanBuyingJob job)
        {
            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                throw new Exception("Error encountered while running Buying Model, please contact a system administrator for help");
            }

            if (!IsBuyingModelRunning(job))
            {
                throw new Exception("Error encountered while canceling Buying Model, process is not running");
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

        public bool IsBuyingModelRunningForJob(int jobId)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);
            return IsBuyingModelRunning(job);
        }

        public bool IsBuyingModelRunningForPlan(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);
            return IsBuyingModelRunning(job);
        }

        public List<PlanBuyingApiRequestParametersDto> GetPlanBuyingRuns(int planId)
        {
            return _PlanBuyingRepository.GetPlanBuyingRuns(planId);
        }

        private List<PlanBuyingApiRequestSpotsDto> _GetBuyingModelSpots(
            List<IGrouping<PlanBuyingInventoryGroup, ProgramWithManifestDaypart>> groupedInventory,
            List<int> skippedWeeksIds)
        {
            var marketCoveragesByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var buyingModelSpots = new List<PlanBuyingApiRequestSpotsDto>();

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
                            .Select(manifestWeek => new PlanBuyingApiRequestSpotsDto
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

                        buyingModelSpots.AddRange(spots);
                    }
                }
            }

            return buyingModelSpots;
        }

        private List<PlanBuyingApiRequestWeekDto> _GetBuyingModelWeeks(
            PlanDto plan,
            List<PlanBuyingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto parameters,
            out List<int> SkippedWeeksIds)
        {
            SkippedWeeksIds = new List<int>();
            var buyingModelWeeks = new List<PlanBuyingApiRequestWeekDto>();
            var marketCoverageGoal = GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value);
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);
            var shareOfVoice = _GetShareOfVoice(topMarkets, marketsWithSov);
            var daypartsWithWeighting = plan.Dayparts.Where(x => x.WeightingGoalPercent.HasValue);
            var planBuyingParameters = plan.BuyingParameters;

            var planWeeks = _CalculatePlanWeeksWitBuyingParameters(plan);
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
                (double capTime, string capType) = FrequencyCapHelper.GetFrequencyCapTimeAndCapTypeString(planBuyingParameters.UnitCapsType);

                var buyingWeek = new PlanBuyingApiRequestWeekDto
                {
                    MediaWeekId = mediaWeekId,
                    ImpressionGoal = impressionGoal,
                    CpmGoal = cpmGoal,
                    MarketCoverageGoal = marketCoverageGoal,
                    FrequencyCapSpots = planBuyingParameters.UnitCaps,
                    FrequencyCapTime = capTime,
                    FrequencyCapUnit = capType,
                    ShareOfVoice = shareOfVoice,
                    DaypartWeighting = daypartsWithWeighting.Select(x => new DaypartWeighting
                    {
                        DaypartId = x.DaypartCodeId,
                        DaypartGoal = GeneralMath.ConvertPercentageToFraction(x.WeightingGoalPercent.Value)
                    }).ToList()
                };

                buyingModelWeeks.Add(buyingWeek);
            }

            return buyingModelWeeks;
        }

        private List<WeeklyBreakdownWeek> _CalculatePlanWeeksWitBuyingParameters(PlanDto plan)
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
                TotalBudget = plan.Budget.Value,
                TotalImpressions = plan.TargetImpressions.Value * 1000,
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
        public int ReRunBuyingJob(int jobId)
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
            RunBuyingJob(jobParams, newJobId, CancellationToken.None);

            return newJobId;
        }

        private void _RunBuyingJob(PlanBuyingParametersDto planBuyingParametersDto, PlanDto plan, int jobId, CancellationToken token)
        {
            var diagnostic = new PlanBuyingJobDiagnostic();
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_TOTAL_DURATION);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_PROCESSING);
            var PlanBuyingJob = _PlanBuyingRepository.GetPlanBuyingJob(jobId);
            PlanBuyingJob.Status = BackgroundJobProcessingStatus.Processing;
            _PlanBuyingRepository.UpdatePlanBuyingJob(PlanBuyingJob);
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
                    Margin = planBuyingParametersDto.Margin,
                    MarketGroup = planBuyingParametersDto.MarketGroup
                };
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_FETCHING_PLAN_AND_PARAMETERS);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_INVENTORY_SOURCE_ESTIMATES);
                var proprietaryEstimates = _CalculateProprietaryInventorySourceEstimates(plan, programInventoryParameters, diagnostic);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_INVENTORY_SOURCE_ESTIMATES);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_INVENTORY_SOURCE_ESTIMATES);
                _PlanBuyingRepository.SavePlanBuyingEstimates(jobId, proprietaryEstimates);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_INVENTORY_SOURCE_ESTIMATES);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_GATHERING_INVENTORY);
                var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());
                var inventory = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan,
                    programInventoryParameters,
                    inventorySourceIds,
                    diagnostic,
                    isProprietary: false);

                _ValidateInventory(inventory);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_GATHERING_INVENTORY);

                token.ThrowIfCancellationRequested();

                var allocationResult = _SendBuyingRequest(
                    jobId,
                    plan,
                    inventory,
                    proprietaryEstimates,
                    programInventoryParameters,
                    token,
                    diagnostic,
                    out var goalsFulfilledByProprietaryInventory);

                token.ThrowIfCancellationRequested();

                var aggregateResultsTask = new Task<PlanBuyingResultBaseDto>(() =>
                {
                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                    var aggregatedResults = _AggregateResults(inventory, allocationResult, goalsFulfilledByProprietaryInventory);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                    return aggregatedResults;
                });

                var calculateBuyingBandsTask = new Task<PlanBuyingBandsDto>(() =>
                {
                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_BANDS);
                    var buyingBands = _PlanBuyingBandCalculationEngine.CalculateBuyingBands(inventory, allocationResult, planBuyingParametersDto);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_BANDS);
                    return buyingBands;
                });

                var calculateBuyingStationsTask = new Task<PlanBuyingStationResultDto>(() =>
                {
                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_STATIONS);
                    var buyingStations = _PlanBuyingStationCalculationEngine.Calculate(inventory, allocationResult, planBuyingParametersDto);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_STATIONS);
                    return buyingStations;
                });

                var aggregateMarketResultsTask = new Task<PlanBuyingResultMarketsDto>(() =>
                {
                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                    var marketCoverages = _MarketCoverageRepository.GetMarketsWithLatestCoverage();
                    var buyingMarketResults = _PlanBuyingMarketResultsEngine.Calculate(inventory, allocationResult, planBuyingParametersDto, plan, marketCoverages);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                    return buyingMarketResults;
                });

                aggregateResultsTask.Start();
                calculateBuyingBandsTask.Start();
                calculateBuyingStationsTask.Start();
                aggregateMarketResultsTask.Start();

                token.ThrowIfCancellationRequested();

                aggregateResultsTask.Wait();
                var aggregateTaskResult = aggregateResultsTask.Result;

                calculateBuyingBandsTask.Wait();
                var calculateBuyingBandTaskResult = calculateBuyingBandsTask.Result;

                calculateBuyingStationsTask.Wait();
                var calculateBuyingStationTaskResult = calculateBuyingStationsTask.Result;

                aggregateMarketResultsTask.Wait();
                var aggregateMarketResultsTaskResult = aggregateMarketResultsTask.Result;

                using (var transaction = new TransactionScopeWrapper())
                {
                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);
                    _PlanBuyingRepository.SaveBuyingApiResults(allocationResult);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);

                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);
                    _PlanBuyingRepository.SaveBuyingAggregateResults(aggregateTaskResult);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);

                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_BANDS);
                    _PlanBuyingRepository.SavePlanBuyingBands(calculateBuyingBandTaskResult);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_BANDS);

                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_STATIONS);
                    _PlanBuyingRepository.SavePlanBuyingStations(calculateBuyingStationTaskResult);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_BUYING_STATIONS);

                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);
                    _PlanBuyingRepository.SavePlanBuyingMarketResults(aggregateMarketResultsTaskResult);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_MARKET_RESULTS);

                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);
                    var buyingJob = _PlanBuyingRepository.GetPlanBuyingJob(jobId);
                    buyingJob.Status = BackgroundJobProcessingStatus.Succeeded;
                    buyingJob.Completed = _DateTimeEngine.GetCurrentMoment();
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);

                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_TOTAL_DURATION);
                    buyingJob.DiagnosticResult = diagnostic.ToString();

                    _PlanBuyingRepository.UpdatePlanBuyingJob(buyingJob);

                    transaction.Complete();
                }
            }
            catch (BuyingModelException exception)
            {
                _HandleBuyingJobError(jobId, BackgroundJobProcessingStatus.Failed, exception.Message);
            }
            catch (Exception exception) when (exception is ObjectDisposedException || exception is OperationCanceledException)
            {
                _HandleBuyingJobException(jobId, BackgroundJobProcessingStatus.Canceled, exception, "Running the buying model was canceled");
            }
            catch (Exception exception)
            {
                _HandleBuyingJobException(jobId, BackgroundJobProcessingStatus.Failed, exception, "Error attempting to run the buying model");
            }
        }

        private PlanBuyingAllocationResult _SendBuyingRequest(
            int jobId,
            PlanDto plan,
            List<PlanBuyingInventoryProgram> inventory,
            List<PlanBuyingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto programInventoryParameters,
            CancellationToken token,
            PlanBuyingJobDiagnostic diagnostic,
            out bool goalsFulfilledByProprietaryInventory)
        {
            goalsFulfilledByProprietaryInventory = false;
            var allocationResult = new PlanBuyingAllocationResult
            {
                Spots = new List<PlanBuyingAllocatedSpot>(),
                JobId = jobId,
                PlanVersionId = plan.VersionId,
                BuyingVersion = BroadcastServiceSystemParameter.PlanPricingEndpointVersion
            };

            if (BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "2")
            {
                _SendBuyingRequest_v2(
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
                _SendBuyingRequest_v3(
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
                throw new Exception("Unknown buying API version was discovered");
            }

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_CPM);
            allocationResult.BuyingCpm = _CalculateBuyingCpm(allocationResult.Spots, proprietaryEstimates);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_CPM);

            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);
            _ValidateAllocationResult(allocationResult);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);

            return allocationResult;
        }

        private void _SendBuyingRequest_v2(
            PlanBuyingAllocationResult allocationResult,
            PlanDto plan,
            List<PlanBuyingInventoryProgram> inventory,
            List<PlanBuyingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto programInventoryParameters,
            CancellationToken token,
            PlanBuyingJobDiagnostic diagnostic,
            out bool goalsFulfilledByProprietaryInventory)
        {
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var buyingModelWeeks = _GetBuyingModelWeeks(plan, proprietaryEstimates, programInventoryParameters, out List<int> skippedWeeksIds);
            goalsFulfilledByProprietaryInventory = buyingModelWeeks.IsEmpty();

            var groupedInventory = _GroupInventory(inventory);
            var spots = _GetBuyingModelSpots(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            token.ThrowIfCancellationRequested();

            if (!buyingModelWeeks.IsEmpty())
            {
                var buyingApiRequest = new PlanBuyingApiRequestDto
                {
                    Weeks = buyingModelWeeks,
                    Spots = spots
                };

                _BackgroundJobClient.Enqueue<IPlanBuyingService>(x => x.SaveBuyingRequest(plan.Id, buyingApiRequest));

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALLING_API);
                var apiAllocationResult = _BuyingApiClient.GetBuyingSpotsResult(buyingApiRequest);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALLING_API);

                token.ThrowIfCancellationRequested();

                if (apiAllocationResult.Error != null)
                {
                    var errorMessage = $@"Buying Model returned the following error: {apiAllocationResult.Error.Name} 
                                -  {string.Join(",", apiAllocationResult.Error.Messages).Trim(',')}";
                    throw new BuyingModelException(errorMessage);
                }

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
                allocationResult.Spots = _MapToResultSpots(groupedInventory, apiAllocationResult, buyingApiRequest, inventory, programInventoryParameters);
                allocationResult.RequestId = apiAllocationResult.RequestId;
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
            }
        }

        private void _SendBuyingRequest_v3(
            PlanBuyingAllocationResult allocationResult,
            PlanDto plan,
            List<PlanBuyingInventoryProgram> inventory,
            List<PlanBuyingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto programInventoryParameters,
            CancellationToken token,
            PlanBuyingJobDiagnostic diagnostic,
            out bool goalsFulfilledByProprietaryInventory)
        {
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var buyingModelWeeks = _GetBuyingModelWeeks_v3(plan, proprietaryEstimates, programInventoryParameters, out List<int> skippedWeeksIds);
            goalsFulfilledByProprietaryInventory = buyingModelWeeks.IsEmpty();

            var groupedInventory = _GroupInventory(inventory);
            var spots = _GetBuyingModelSpots_v3(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            token.ThrowIfCancellationRequested();

            if (!buyingModelWeeks.IsEmpty())
            {
                var buyingApiRequest = new PlanBuyingApiRequestDto_v3
                {
                    Weeks = buyingModelWeeks,
                    Spots = spots
                };

                _BackgroundJobClient.Enqueue<IPlanBuyingService>(x => x.SaveBuyingRequest(plan.Id, buyingApiRequest));

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALLING_API);
                var apiAllocationResult = _BuyingApiClient.GetBuyingSpotsResult(buyingApiRequest);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALLING_API);

                token.ThrowIfCancellationRequested();

                if (apiAllocationResult.Error != null)
                {
                    var errorMessage = $@"Buying Model returned the following error: {apiAllocationResult.Error.Name} 
                                -  {string.Join(",", apiAllocationResult.Error.Messages).Trim(',')}";
                    throw new BuyingModelException(errorMessage);
                }

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
                allocationResult.Spots = _MapToResultSpots(groupedInventory, apiAllocationResult, buyingApiRequest, inventory, programInventoryParameters, plan);
                allocationResult.RequestId = apiAllocationResult.RequestId;
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
            }
        }

        private List<PlanBuyingApiRequestSpotsDto_v3> _GetBuyingModelSpots_v3(
            List<IGrouping<PlanBuyingInventoryGroup, ProgramWithManifestDaypart>> groupedInventory,
            List<int> skippedWeeksIds)
        {
            var marketCoveragesByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var buyingModelSpots = new List<PlanBuyingApiRequestSpotsDto_v3>();

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
                            .Select(manifestWeek => new PlanBuyingApiRequestSpotsDto_v3
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

                        buyingModelSpots.AddRange(spots);
                    }
                }
            }

            return buyingModelSpots;
        }

        public void RunBuyingJob(PlanBuyingParametersDto PlanBuyingParametersDto, int jobId, CancellationToken token)
        {
            var plan = _PlanRepository.GetPlan(PlanBuyingParametersDto.PlanId.Value);

            _RunBuyingJob(PlanBuyingParametersDto, plan, jobId, token);
        }

        private List<PlanBuyingApiRequestWeekDto_v3> _GetBuyingModelWeeks_v3(
            PlanDto plan,
            List<PlanBuyingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto parameters,
            out List<int> SkippedWeeksIds)
        {
            SkippedWeeksIds = new List<int>();
            var buyingModelWeeks = new List<PlanBuyingApiRequestWeekDto_v3>();
            var marketCoverageGoal = GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value);
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);
            var shareOfVoice = _GetShareOfVoice(topMarkets, marketsWithSov);
            var daypartsWithWeighting = plan.Dayparts.Where(x => x.WeightingGoalPercent.HasValue);
            var planBuyingParameters = plan.BuyingParameters;

            var planWeeks = _CalculatePlanWeeksWitBuyingParameters(plan);
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

                var buyingWeek = new PlanBuyingApiRequestWeekDto_v3
                {
                    MediaWeekId = mediaWeekId,
                    ImpressionGoal = impressionGoal,
                    CpmGoal = ProposalMath.CalculateCpm(weeklyBudget, impressionGoal),
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
                x => plan.Equivalized ? _SpotLengthEngine.GetDeliveryMultiplierBySpotLengthId(x.SpotLengthId) : 1);
        }

        private List<IGrouping<PlanBuyingInventoryGroup, ProgramWithManifestDaypart>> _GroupInventory(List<PlanBuyingInventoryProgram> inventory)
        {
            var flattedProgramsWithDayparts = inventory
                .SelectMany(x => x.ManifestDayparts.Select(d => new ProgramWithManifestDaypart
                {
                    Program = x,
                    ManifestDaypart = d
                }));

            var grouped = flattedProgramsWithDayparts.GroupBy(x =>
                new PlanBuyingInventoryGroup
                {
                    StationId = x.Program.Station.Id,
                    DaypartId = x.ManifestDaypart.Daypart.Id,
                    PrimaryProgramName = x.ManifestDaypart.PrimaryProgram.Name
                });

            return grouped.ToList();
        }

        public void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto buyingApiRequest)
        {
            try
            {
                _BuyingRequestLogClient.SaveBuyingRequest(planId, buyingApiRequest);
            }
            catch (Exception exception)
            {
                _LogError("Failed to save buying API request", exception);
            }
        }

        public void SaveBuyingRequest(int planId, PlanBuyingApiRequestDto_v3 buyingApiRequest)
        {
            try
            {
                _BuyingRequestLogClient.SaveBuyingRequest(planId, buyingApiRequest);
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

        internal decimal _CalculateBuyingCpm(List<PlanBuyingAllocatedSpot> spots, List<PlanBuyingEstimate> proprietaryEstimates)
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

        private List<PlanBuyingEstimate> _CalculateProprietaryInventorySourceEstimates(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            PlanBuyingJobDiagnostic diagnostic)
        {
            var result = new List<PlanBuyingEstimate>();

            result.AddRange(_GetBuyingEstimatesBasedOnInventorySourcePreferences(plan, parameters, diagnostic));
            result.AddRange(_GetBuyingEstimatesBasedOnInventorySourceTypePreferences(plan, parameters, diagnostic));

            return result;
        }

        private List<PlanBuyingEstimate> _GetBuyingEstimatesBasedOnInventorySourcePreferences(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            PlanBuyingJobDiagnostic diagnostic)
        {
            var result = new List<PlanBuyingEstimate>();

            var supportedInventorySourceIds = _GetInventorySourceIdsByTypes(new List<InventorySourceTypeEnum>
            {
                InventorySourceTypeEnum.Barter,
                InventorySourceTypeEnum.ProprietaryOAndO
            });

            var inventorySourcePreferences = plan.BuyingParameters.InventorySourcePercentages
                .Where(x => x.Percentage > 0 && supportedInventorySourceIds.Contains(x.Id))
                .ToList();

            var inventory = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    inventorySourcePreferences.Select(x => x.Id),
                    diagnostic,
                    isProprietary: true);

            foreach (var preference in inventorySourcePreferences)
            {
                var inventorySourceId = preference.Id;
                var programs = inventory.Where(x => x.InventorySource.Id == inventorySourceId);

                var estimates = _GetBuyingEstimates(
                    programs,
                    preference.Percentage,
                    inventorySourceId,
                    null);

                result.AddRange(estimates);
            }

            return result;
        }

        private List<PlanBuyingEstimate> _GetBuyingEstimatesBasedOnInventorySourceTypePreferences(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters,
            PlanBuyingJobDiagnostic diagnostic)
        {
            var result = new List<PlanBuyingEstimate>();

            var supportedInventorySourceTypes = new List<InventorySourceTypeEnum>
            {
                InventorySourceTypeEnum.Diginet
            };

            var inventorySourceTypePreferences = plan.BuyingParameters.InventorySourceTypePercentages
                .Where(x => x.Percentage > 0 && supportedInventorySourceTypes.Contains((InventorySourceTypeEnum)x.Id))
                .ToList();

            var inventorySourceIds = _GetInventorySourceIdsByTypes(inventorySourceTypePreferences.Select(x => (InventorySourceTypeEnum)x.Id));

            var inventory = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    inventorySourceIds,
                    diagnostic,
                    isProprietary: true);

            foreach (var preference in inventorySourceTypePreferences)
            {
                var inventorySourceType = (InventorySourceTypeEnum)preference.Id;
                var programs = inventory.Where(x => x.InventorySource.InventoryType == inventorySourceType);

                var estimates = _GetBuyingEstimates(
                    programs,
                    preference.Percentage,
                    null,
                    inventorySourceType);

                result.AddRange(estimates);
            }

            return result;
        }

        private IEnumerable<PlanBuyingEstimate> _GetBuyingEstimates(
            IEnumerable<PlanBuyingInventoryProgram> programs,
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

                    return new PlanBuyingEstimate
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

        private void _ValidateInventory(List<PlanBuyingInventoryProgram> inventory)
        {
            if (!inventory.Any())
            {
                throw new Exception("No inventory found for buying run");
            }
        }

        private List<PlanBuyingAllocatedSpot> _MapToResultSpots(
            List<IGrouping<PlanBuyingInventoryGroup,
            ProgramWithManifestDaypart>> groupedInventory,
            PlanBuyingApiSpotsResponseDto apiSpotsResults,
            PlanBuyingApiRequestDto buyingApiRequest,
            List<PlanBuyingInventoryProgram> inventoryPrograms,
            ProgramInventoryOptionalParametersDto programInventoryParameters)
        {
            var results = new List<PlanBuyingAllocatedSpot>();
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

                var originalSpot = buyingApiRequest.Spots.FirstOrDefault(x =>
                    x.Id == allocation.ManifestId &&
                    x.MediaWeekId == allocation.MediaWeekId);

                if (originalSpot == null)
                    throw new Exception("Response from API contains manifest id not found in sent data");

                var program = inventoryPrograms.Single(x => x.ManifestId == originalProgram.Program.ManifestId);
                var inventoryWeek = program.ManifestWeeks.Single(x => x.ContractMediaWeekId == originalSpot.MediaWeekId);

                var spotResult = new PlanBuyingAllocatedSpot
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
                            Impressions = originalSpot.Impressions
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

        private List<PlanBuyingAllocatedSpot> _MapToResultSpots(
            List<IGrouping<PlanBuyingInventoryGroup,
            ProgramWithManifestDaypart>> groupedInventory,
            PlanBuyingApiSpotsResponseDto_v3 apiSpotsResults,
            PlanBuyingApiRequestDto_v3 buyingApiRequest,
            List<PlanBuyingInventoryProgram> inventoryPrograms,
            ProgramInventoryOptionalParametersDto programInventoryParameters,
            PlanDto plan)
        {
            var results = new List<PlanBuyingAllocatedSpot>();
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

                var originalSpot = buyingApiRequest.Spots.FirstOrDefault(x =>
                    x.Id == allocation.ManifestId &&
                    x.MediaWeekId == allocation.MediaWeekId);

                if (originalSpot == null)
                    throw new Exception("Response from API contains manifest id not found in sent data");

                var program = inventoryPrograms.Single(x => x.ManifestId == originalProgram.Program.ManifestId);
                var inventoryWeek = program.ManifestWeeks.Single(x => x.ContractMediaWeekId == originalSpot.MediaWeekId);
                var spotCostBySpotLengthId = originalSpot.SpotCost.ToDictionary(x => x.SpotLengthId, x => x.SpotLengthCost);
                var frequencies = allocation.Frequencies.Where(x => x.Frequency > 0).ToList();

                var spotResult = new PlanBuyingAllocatedSpot
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

        public void _ValidateAllocationResult(PlanBuyingAllocationResult apiResponse)
        {
            if (!string.IsNullOrEmpty(apiResponse.RequestId) && !apiResponse.Spots.Any())
            {
                var msg = $"The api returned no spots for request '{apiResponse.RequestId}'.";
                throw new Exception(msg);
            }
        }

        private PlanBuyingResultBaseDto _AggregateResults(
            List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult apiResponse,
            bool goalsFulfilledByProprietaryInventory = false)
        {
            var result = new PlanBuyingResultBaseDto();
            var programs = _GetPrograms(inventory, apiResponse);
            var totalCostForAllPrograms = programs.Sum(x => x.TotalCost);
            var totalImpressionsForAllPrograms = programs.Sum(x => x.TotalImpressions);
            var totalSpotsForAllPrograms = programs.Sum(x => x.TotalSpots);

            result.Programs.AddRange(programs.Select(x => new Entities.Plan.Buying.PlanBuyingProgramDto
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

            result.Totals = new PlanBuyingProgramTotalsDto
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
            result.OptimalCpm = apiResponse.BuyingCpm;
            result.JobId = apiResponse.JobId;
            result.PlanVersionId = apiResponse.PlanVersionId;

            return result;
        }

        private List<PlanBuyingProgram> _GetPrograms(
            List<PlanBuyingInventoryProgram> inventory,
            PlanBuyingAllocationResult apiResponse)
        {
            var result = new List<PlanBuyingProgram>();
            var inventoryGroupedByProgramName = inventory
                .SelectMany(x => x.ManifestDayparts.Select(d => new PlanBuyingManifestWithManifestDaypart
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

                var program = new PlanBuyingProgram
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

        private List<DisplayBroadcastStation> _GetAllocatedStations(PlanBuyingAllocationResult apiResponse, List<PlanBuyingManifestWithManifestDaypart> programInventory)
        {
            var manifestIds = apiResponse.Spots.Select(s => s.Id).Distinct();
            var result = new List<PlanBuyingManifestWithManifestDaypart>();
            return programInventory.Where(p => manifestIds.Contains(p.Manifest.ManifestId)).Select(p => p.Manifest.Station).ToList();
        }

        private List<PlanBuyingAllocatedSpot> _GetAllocatedProgramSpots(PlanBuyingAllocationResult apiResponse
            , List<PlanBuyingManifestWithManifestDaypart> programInventory)
        {
            var result = new List<PlanBuyingAllocatedSpot>();

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
            IEnumerable<PlanBuyingAllocatedSpot> allocatedProgramSpots,
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

        public PlanBuyingApiRequestDto GetBuyingApiRequestPrograms(int planId,  BuyingInventoryGetRequestParametersDto requestParameters)
        {
            var diagnostic = new PlanBuyingJobDiagnostic();
            var buyingParams = new ProgramInventoryOptionalParametersDto
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

            var inventory = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                plan,
                buyingParams,
                inventorySourceIds,
                diagnostic,
                isProprietary: !requestParameters.InventorySourceIds.IsEmpty());
            var groupedInventory = _GroupInventory(inventory);
            var proprietaryEstimates = _CalculateProprietaryInventorySourceEstimates(plan, buyingParams, diagnostic);

            var buyingApiRequest = new PlanBuyingApiRequestDto
            {
                Weeks = _GetBuyingModelWeeks(plan, proprietaryEstimates, buyingParams, out List<int> skippedWeeksIds),
                Spots = _GetBuyingModelSpots(groupedInventory, skippedWeeksIds)
            };

            return buyingApiRequest;
        }

        public PlanBuyingApiRequestDto_v3 GetBuyingApiRequestPrograms_v3(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            var diagnostic = new PlanBuyingJobDiagnostic();
            var buyingParams = new ProgramInventoryOptionalParametersDto
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

            var inventory = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                plan,
                buyingParams,
                inventorySourceIds,
                diagnostic,
                isProprietary: !requestParameters.InventorySourceIds.IsEmpty());
            var groupedInventory = _GroupInventory(inventory);
            var proprietaryEstimates = _CalculateProprietaryInventorySourceEstimates(plan, buyingParams, diagnostic);

            var buyingApiRequest = new PlanBuyingApiRequestDto_v3
            {
                Weeks = _GetBuyingModelWeeks_v3(plan, proprietaryEstimates, buyingParams, out List<int> skippedWeeksIds),
                Spots = _GetBuyingModelSpots_v3(groupedInventory, skippedWeeksIds)
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
                MarketGroup = requestParameters.MarketGroup
            };
            var inventorySourceIds = requestParameters.InventorySourceIds.IsEmpty() ?
                _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes()) :
                requestParameters.InventorySourceIds;

            var inventory = _PlanBuyingInventoryEngine.GetInventoryForPlan(
                plan,
                buyingParams,
                inventorySourceIds,
                diagnostic,
                isProprietary: !requestParameters.InventorySourceIds.IsEmpty());

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

        public BuyingProgramsResultDto GetProgramsByJobId(int jobId)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);

            return _GetPrograms(job, null);
        }

        private BuyingProgramsResultDto _GetPrograms(PlanBuyingJob job, int? planId)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var results = _PlanBuyingRepository.GetBuyingProgramsResultByJobId(job.Id);

            if (results == null)
                return null;

            results.Totals.ImpressionsPercentage = 100;

            _ConvertImpressionsToUserFormat(results);

            return results;
        }

        public BuyingProgramsResultDto GetPrograms(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            return _GetPrograms(job, planId);
        }

        private void _ConvertImpressionsToUserFormat(BuyingProgramsResultDto planBuyingResult)
        {
            if (planBuyingResult == null)
                return;

            planBuyingResult.Totals.AvgImpressions /= 1000;
            planBuyingResult.Totals.Impressions /= 1000;

            foreach (var program in planBuyingResult.Programs)
            {
                program.AvgImpressions /= 1000;
                program.Impressions /= 1000;
            }
        }

        public PlanBuyingBandsDto GetBuyingBandsByJobId(int jobId)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);

            return _GetBuyingBands(job, null);
        }

        private PlanBuyingBandsDto _GetBuyingBands(PlanBuyingJob job, int? planId)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var results = _PlanBuyingRepository.GetPlanBuyingBandByJobId(job.Id);

            if (results == null)
                return null;

            _ConvertBuyingBandImpressionsToUserFormat(results);

            return results;
        }

        public PlanBuyingBandsDto GetBuyingBands(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            return _GetBuyingBands(job, planId);
        }

        /// <inheritdoc />
        public PlanBuyingResultMarketsDto GetMarketsByJobId(int jobId)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);

            return _GetMarkets(job, null);
        }

        private PlanBuyingResultMarketsDto _GetMarkets(PlanBuyingJob job, int? planId)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
            {
                return null;
            }

            var results = _PlanBuyingRepository.GetPlanBuyingResultMarketsByJobId(job.Id);

            if (results == null)
            {
                return null;
            }

            _ConvertBuyingMarketResultsToUserFormat(results);

            return results;
        }

        /// <inheritdoc />
        public PlanBuyingResultMarketsDto GetMarkets(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            return _GetMarkets(job, planId);
        }

        public PlanBuyingStationResultDto GetStationsByJobId(int jobId)
        {
            var job = _PlanBuyingRepository.GetPlanBuyingJob(jobId);
            return _GetStations(job, null);
        }

        private PlanBuyingStationResultDto _GetStations(PlanBuyingJob job, int? planId)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var result = _PlanBuyingRepository.GetBuyingStationsResultByJobId(job.Id);
            if (result == null)
                return null;

            _ConvertBuyingStationResultDtoToUserFormat(result);

            return result;
        }

        public PlanBuyingStationResultDto GetStations(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);
            return _GetStations(job, planId);
        }

        private void _ConvertBuyingStationResultDtoToUserFormat(PlanBuyingStationResultDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var band in results.Stations)
            {
                band.Impressions /= 1000;
            }
        }

        private void _ConvertBuyingBandImpressionsToUserFormat(PlanBuyingBandsDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var band in results.Bands)
            {
                band.Impressions /= 1000;
            }
        }

        private void _ConvertBuyingMarketResultsToUserFormat(PlanBuyingResultMarketsDto results)
        {
            results.Totals.Impressions /= 1000;

            foreach (var detail in results.MarketDetails)
            {
                detail.Impressions /= 1000;
            }
        }

        private class ProgramWithManifestDaypart
        {
            public PlanBuyingInventoryProgram Program { get; set; }

            public BasePlanInventoryProgram.ManifestDaypart ManifestDaypart { get; set; }
        }

        private class ProgramWithManifestWeek
        {
            public PlanBuyingInventoryProgram Program { get; set; }

            public PlanBuyingInventoryProgram.ManifestWeek ManifestWeek { get; set; }
        }
    }
}
