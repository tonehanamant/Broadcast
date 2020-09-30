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
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.BuyingResults;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
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
        /// Gets the buying ownership groups.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>PlanBuyingResultOwnershipGroupDto object</returns>
        PlanBuyingResultOwnershipGroupDto GetBuyingOwnershipGroups(int planId);

        /// <summary>
        /// Gets the buying rep firms.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>PlanBuyingResultRepFirmDto object</returns>
        PlanBuyingResultRepFirmDto GetBuyingRepFirms(int planId);

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

        PlanBuyingResultProgramsDto GetPrograms(int planId);

        PlanBuyingStationResultDto GetStations(int planId);

        /// <summary>
        /// Retrieves the Buying Results Markets Summary
        /// </summary>
        PlanBuyingResultMarketsDto GetMarkets(int planId);

        PlanBuyingBandsDto GetBuyingBands(int planId);

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
                                  IAsyncTaskHelper asyncTaskHelper)
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
                if (IsBuyingModelRunning(planBuyingParametersDto.PlanId.Value))
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
            PlanBuyingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData,
            out List<int> SkippedWeeksIds)
        {
            SkippedWeeksIds = new List<int>();
            var buyingModelWeeks = new List<PlanBuyingApiRequestWeekDto>();
            var planImpressionsGoal = plan.BuyingParameters.DeliveryImpressions * 1000;

            // send 0.001% if any unit is selected
            var marketCoverageGoal = parameters.ProprietaryInventory.IsEmpty() ? GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value) : 0.001;
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);
            var shareOfVoice = _GetShareOfVoice(topMarkets, marketsWithSov, proprietaryInventoryData, planImpressionsGoal);
            var daypartsWithWeighting = plan.Dayparts.Where(x => x.WeightingGoalPercent.HasValue);
            var planBuyingParameters = plan.BuyingParameters;
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
                    MarketGroup = planBuyingParametersDto.MarketGroup
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

                var allocationResult = new PlanBuyingAllocationResult
                {
                    Spots = new List<PlanBuyingAllocatedSpot>(),
                    JobId = jobId,
                    PlanVersionId = plan.VersionId,
                    BuyingVersion = BroadcastServiceSystemParameter.PlanPricingEndpointVersion
                };

                if (!goalsFulfilledByProprietaryInventory)
                {
                    _SendBuyingRequest(
                        allocationResult,
                        plan,
                        inventory,
                        token,
                        diagnostic,
                        planBuyingParametersDto,
                        proprietaryInventoryData);
                }

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_CPM);
                allocationResult.BuyingCpm = _CalculateBuyingCpm(allocationResult.Spots, proprietaryInventoryData);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_CPM);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);
                _ValidateAllocationResult(allocationResult);
                diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_VALIDATING_ALLOCATION_RESULT);

                token.ThrowIfCancellationRequested();

                var calculateBuyingProgramsTask = new Task<PlanBuyingResultBaseDto>(() =>
                {
                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                    var programResults = _PlanBuyingProgramEngine.Calculate(inventory, allocationResult, goalsFulfilledByProprietaryInventory);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                    return programResults;
                });

                var calculateBuyingBandsTask = new Task<PlanBuyingBandsDto>(() =>
                {
                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_CALCULATING_BUYING_BANDS);
                    var buyingBands = _PlanBuyingBandCalculationEngine.Calculate(inventory, allocationResult, planBuyingParametersDto);
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
                    var buyingMarketResults = _PlanBuyingMarketResultsEngine.Calculate(inventory, allocationResult, planBuyingParametersDto, plan
                        , marketCoverages);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_MARKET_RESULTS);
                    return buyingMarketResults;
                });

                var aggregateOwnershipGroupResultsTask = new Task<PlanBuyingResultOwnershipGroupDto>(() =>
                {
                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_OWNERSHIP_GROUP_RESULTS);
                    var buyingOwnershipGroupResults = _PlanBuyingOwnershipGroupEngine.Calculate(inventory, allocationResult
                        , planBuyingParametersDto);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_OWNERSHIP_GROUP_RESULTS);
                    return buyingOwnershipGroupResults;
                });

                var aggregateRepFirmResultsTask = new Task<PlanBuyingResultRepFirmDto>(() =>
                {
                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_OWNERSHIP_GROUP_RESULTS);
                    var buyingOwnershipGroupResults = _PlanBuyingRepFirmEngine.Calculate(inventory, allocationResult
                        , planBuyingParametersDto);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_AGGREGATING_OWNERSHIP_GROUP_RESULTS);
                    return buyingOwnershipGroupResults;
                });

                calculateBuyingProgramsTask.Start();
                calculateBuyingBandsTask.Start();
                calculateBuyingStationsTask.Start();
                aggregateMarketResultsTask.Start();
                aggregateOwnershipGroupResultsTask.Start();
                aggregateRepFirmResultsTask.Start();

                token.ThrowIfCancellationRequested();

                
                var aggregateTaskResult = calculateBuyingProgramsTask.GetAwaiter().GetResult();
                var calculateBuyingBandTaskResult = calculateBuyingBandsTask.GetAwaiter().GetResult();
                var calculateBuyingStationTaskResult = calculateBuyingStationsTask.GetAwaiter().GetResult();
                var aggregateMarketResultsTaskResult = aggregateMarketResultsTask.GetAwaiter().GetResult();
                var aggregateOwnershipGroupResultsTaskResult = aggregateOwnershipGroupResultsTask.GetAwaiter().GetResult();
                var aggregateRepFirmResultsTaskResult = aggregateRepFirmResultsTask.GetAwaiter().GetResult();

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

                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_OWNERSHIP_GROUP_RESULTS);
                    _PlanBuyingRepository.SavePlanBuyingOwnershipGroupResults(aggregateOwnershipGroupResultsTaskResult);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_OWNERSHIP_GROUP_RESULTS);

                    diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_SAVING_REP_FIRM_RESULTS);
                    _PlanBuyingRepository.SavePlanBuyingRepFirmResults(aggregateRepFirmResultsTaskResult);
                    diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_SAVING_REP_FIRM_RESULTS);

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
                    ImpressionsPerUnit = plan.ImpressionsPerUnit,
                    WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                };

                plan.WeeklyBreakdownWeeks = _WeeklyBreakdownEngine.CalculatePlanWeeklyGoalBreakdown(request).Weeks;
                plan.WeeklyBreakdownWeeks = _WeeklyBreakdownEngine.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(plan);
            }
        }

        private void _SendBuyingRequest(
            PlanBuyingAllocationResult allocationResult,
            PlanDto plan,
            List<PlanBuyingInventoryProgram> inventory,
            CancellationToken token,
            PlanBuyingJobDiagnostic diagnostic,
            PlanBuyingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            if (BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "2")
            {
                _SendBuyingRequest_v2(
                    allocationResult,
                    plan,
                    inventory,
                    token,
                    diagnostic,
                    parameters,
                    proprietaryInventoryData);
            }
            else if (BroadcastServiceSystemParameter.PlanPricingEndpointVersion == "3")
            {
                _SendBuyingRequest_v3(
                    allocationResult,
                    plan,
                    inventory,
                    token,
                    diagnostic,
                    parameters,
                    proprietaryInventoryData);
            }
            else
            {
                throw new Exception("Unknown buying API version was discovered");
            }
        }

        private void _SendBuyingRequest_v2(
            PlanBuyingAllocationResult allocationResult,
            PlanDto plan,
            List<PlanBuyingInventoryProgram> inventory,
            CancellationToken token,
            PlanBuyingJobDiagnostic diagnostic,
            PlanBuyingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var buyingModelWeeks = _GetBuyingModelWeeks(plan, parameters, proprietaryInventoryData, out List<int> skippedWeeksIds);
            var groupedInventory = _GroupInventory(inventory);
            var spots = _GetBuyingModelSpots(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            token.ThrowIfCancellationRequested();

            var buyingApiRequest = new PlanBuyingApiRequestDto
            {
                Weeks = buyingModelWeeks,
                Spots = spots
            };

            _AsyncTaskHelper.TaskFireAndForget(() => SaveBuyingRequest(plan.Id, buyingApiRequest));

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
            allocationResult.Spots = _MapToResultSpots(groupedInventory, apiAllocationResult, buyingApiRequest, inventory, parameters);
            allocationResult.RequestId = apiAllocationResult.RequestId;
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
        }

        private void _SendBuyingRequest_v3(
            PlanBuyingAllocationResult allocationResult,
            PlanDto plan,
            List<PlanBuyingInventoryProgram> inventory,
            CancellationToken token,
            PlanBuyingJobDiagnostic diagnostic,
            PlanBuyingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData)
        {
            diagnostic.Start(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            var buyingModelWeeks = _GetBuyingModelWeeks_v3(plan, parameters, proprietaryInventoryData, out List<int> skippedWeeksIds);
            var groupedInventory = _GroupInventory(inventory);
            var spots = _GetBuyingModelSpots_v3(groupedInventory, skippedWeeksIds);
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

            token.ThrowIfCancellationRequested();

            var buyingApiRequest = new PlanBuyingApiRequestDto_v3
            {
                Weeks = buyingModelWeeks,
                Spots = spots
            };

            _AsyncTaskHelper.TaskFireAndForget(() => SaveBuyingRequest(plan.Id, buyingApiRequest));

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
            allocationResult.Spots = _MapToResultSpots(groupedInventory, apiAllocationResult, buyingApiRequest, inventory, parameters, plan);
            allocationResult.RequestId = apiAllocationResult.RequestId;
            diagnostic.End(PlanBuyingJobDiagnostic.SW_KEY_MAPPING_ALLOCATED_SPOTS);
        }

        internal List<PlanBuyingApiRequestSpotsDto_v3> _GetBuyingModelSpots_v3(
            List<IGrouping<PlanBuyingInventoryGroup, ProgramWithManifestDaypart>> groupedInventory,
            List<int> skippedWeeksIds)
        {
            var marketCoveragesByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var buyingModelSpots = new List<PlanBuyingApiRequestSpotsDto_v3>();

            foreach (var inventoryGrouping in groupedInventory)
            {
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
                                SpotCost = validSpotCosts
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
            PlanBuyingParametersDto parameters,
            ProprietaryInventoryData proprietaryInventoryData,
            out List<int> SkippedWeeksIds)
        {
            SkippedWeeksIds = new List<int>();
            var buyingModelWeeks = new List<PlanBuyingApiRequestWeekDto_v3>();
            var planImpressionsGoal = plan.BuyingParameters.DeliveryImpressions * 1000;

            // send 0.001% if any unit is selected
            var marketCoverageGoal = parameters.ProprietaryInventory.IsEmpty() ? GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value) : 0.001;
            var topMarkets = _GetTopMarkets(parameters.MarketGroup);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);
            var shareOfVoice = _GetShareOfVoice(topMarkets, marketsWithSov, proprietaryInventoryData, planImpressionsGoal);
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
            PlanBuyingParametersDto parameters)
        {
            var results = new List<PlanBuyingAllocatedSpot>();
            var standardDaypartById = _StandardDaypartRepository
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
                            SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(originalSpot.Cost, parameters.Margin),
                            Spots = allocation.Frequency,
                            Impressions = originalSpot.Impressions
                        }
                    },
                    StandardDaypart = standardDaypartById[originalSpot.DaypartId],
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
            PlanBuyingParametersDto parameters,
            PlanDto plan)
        {
            var results = new List<PlanBuyingAllocatedSpot>();
            var standardDaypartById = _StandardDaypartRepository
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
                            SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(spotCostBySpotLengthId[x.SpotLengthId], parameters.Margin),
                            Spots = x.Frequency,
                            Impressions = originalSpot.Impressions30sec * spotScaleFactorBySpotLengthId[x.SpotLengthId]
                        })
                        .ToList(),
                    StandardDaypart = standardDaypartById[originalSpot.DaypartId],
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

        public PlanBuyingApiRequestDto GetBuyingApiRequestPrograms(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            var diagnostic = new PlanBuyingJobDiagnostic();
            var buyingParams = new ProgramInventoryOptionalParametersDto
            {
                MinCPM = requestParameters.MinCpm,
                MaxCPM = requestParameters.MaxCpm,
                InflationFactor = requestParameters.InflationFactor,
                MarketGroup = requestParameters.MarketGroup
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

            var buyingApiRequest = new PlanBuyingApiRequestDto
            {
                Weeks = _GetBuyingModelWeeks(plan, parameters, new ProprietaryInventoryData(), out List<int> skippedWeeksIds),
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
                MarketGroup = requestParameters.MarketGroup
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
                Weeks = _GetBuyingModelWeeks_v3(plan, parameters, new ProprietaryInventoryData(), out List<int> skippedWeeksIds),
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

        public PlanBuyingResultProgramsDto GetPrograms(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var results = _PlanBuyingRepository.GetBuyingProgramsResultByJobId(job.Id);

            if (results == null)
                return null;

            _PlanBuyingProgramEngine.ConvertImpressionsToUserFormat(results);

            return results;
        }

        /// <inheritdoc />
        public PlanBuyingBandsDto GetBuyingBands(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var results = _PlanBuyingRepository.GetPlanBuyingBandByJobId(job.Id);

            if (results == null)
                return null;

            _PlanBuyingBandCalculationEngine.ConvertImpressionsToUserFormat(results);

            return results;
        }

        /// <inheritdoc />
        public PlanBuyingResultMarketsDto GetMarkets(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
            {
                return null;
            }

            var results = _PlanBuyingRepository.GetPlanBuyingResultMarketsByJobId(job.Id);

            if (results == null)
            {
                return null;
            }

            _PlanBuyingMarketResultsEngine.ConvertImpressionsToUserFormat(results);

            return results;
        }

        /// <inheritdoc />
        public PlanBuyingStationResultDto GetStations(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            var result = _PlanBuyingRepository.GetBuyingStationsResultByJobId(job.Id);
            if (result == null)
                return null;

            _PlanBuyingStationCalculationEngine.ConvertImpressionsToUserFormat(result);

            return result;
        }

        /// <inheritdoc />
        public PlanBuyingResultOwnershipGroupDto GetBuyingOwnershipGroups(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
            {
                return null;
            }

            PlanBuyingResultOwnershipGroupDto results = _PlanBuyingRepository.GetBuyingOwnershipGroupsByJobId(job.Id);

            if (results == null)
            {
                return null;
            }

            _PlanBuyingOwnershipGroupEngine.ConvertImpressionsToUserFormat(results);

            return results;
        }

        /// <inheritdoc />
        public PlanBuyingResultRepFirmDto GetBuyingRepFirms(int planId)
        {
            var job = _PlanBuyingRepository.GetLatestBuyingJob(planId);

            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
            {
                return null;
            }

            PlanBuyingResultRepFirmDto results = _PlanBuyingRepository.GetBuyingRepFirmsByJobId(job.Id);

            if (results == null)
            {
                return null;
            }

            _PlanBuyingRepFirmEngine.ConvertImpressionsToUserFormat(results);

            return results;
        }

        internal class ProgramWithManifestDaypart
        {
            public PlanBuyingInventoryProgram Program { get; set; }

            public BasePlanInventoryProgram.ManifestDaypart ManifestDaypart { get; set; }
        }

        internal class ProprietaryInventoryData
        {
            public double TotalImpressions { get; set; }

            public decimal TotalCost { get; set; }

            public decimal TotalCostWithMargin { get; set; }

            public Dictionary<short, double> ImpressionsPerMarket { get; set; } = new Dictionary<short, double>();
        }
    }
}
