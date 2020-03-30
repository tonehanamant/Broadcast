﻿using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.PlanPricing;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using static Services.Broadcast.BusinessEngines.PlanPricingInventoryEngine;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPlanPricingService : IApplicationService
    {
        PlanPricingJob QueuePricingJob(PlanPricingParametersDto planPricingParametersDto, DateTime currentDate, string username);
        PlanPricingResponseDto GetCurrentPricingExecution(int planId);

        /// <summary>
        /// Cancels the current pricing execution.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>The PlanPricingResponseDto object</returns>
        PlanPricingResponseDto CancelCurrentPricingExecution(int planId);
        [Queue("planpricing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void RunPricingJob(PlanPricingParametersDto planPricingParametersDto, int jobId, CancellationToken token);
        List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId);
        PlanPricingApiRequestDto GetPricingApiRequestPrograms(int planId, PricingInventoryGetRequestParametersDto requestParameters);
        List<PlanPricingInventoryProgram> GetPricingInventory(int planId, PricingInventoryGetRequestParametersDto requestParameters);
        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetUnitCaps();
        PlanPricingDefaults GetPlanPricingDefaults();
        bool IsPricingModelRunningForPlan(int planId);
        string ForceCompletePlanPricingJob(int jobId, string username);
        /// <summary>
        /// For troubleshooting.  This will bypass the queue to allow rerunning directly.
        /// </summary>
        /// <param name="jobId">The id of the job to rerun.</param>
        /// <returns>The new JobId</returns>
        int ReRunPricingJob(int jobId);
    }

    public class PlanPricingService : BroadcastBaseClass, IPlanPricingService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanPricingInventoryEngine _PlanPricingInventoryEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IPricingApiClient _PricingApiClient;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly ICampaignRepository _CampaignRepository;
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IBroadcastLockingManagerApplicationService _LockingManagerApplicationService;
        private readonly IMarketCoverageRepository _MarketCoverageRepository;
        private readonly IDaypartCache _DaypartCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;

        public PlanPricingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                  ISpotLengthEngine spotLengthEngine,
                                  IPricingApiClient pricingApiClient,
                                  IBackgroundJobClient backgroundJobClient,
                                  IPlanPricingInventoryEngine planPricingInventoryEngine,
                                  IBroadcastLockingManagerApplicationService lockingManagerApplicationService,
                                  IDaypartCache daypartCache,
                                  IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _SpotLengthRepository = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _CampaignRepository = broadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _SpotLengthEngine = spotLengthEngine;
            _PricingApiClient = pricingApiClient;
            _BackgroundJobClient = backgroundJobClient;
            _PlanPricingInventoryEngine = planPricingInventoryEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _DaypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
        }

        public PlanPricingJob QueuePricingJob(PlanPricingParametersDto planPricingParametersDto
            , DateTime currentDate, string username)
        {
            // lock the plan so that two requests for the same plan can not get in this area concurrently
            var key = KeyHelper.GetPlanLockingKey(planPricingParametersDto.PlanId);
            var lockObject = _LockingManagerApplicationService.GetNotUserBasedLockObjectForKey(key);

            lock (lockObject)
            {
                if (IsPricingModelRunningForPlan(planPricingParametersDto.PlanId))
                {
                    throw new Exception("The pricing model is already running for the plan");
                }

                var plan = _PlanRepository.GetPlan(planPricingParametersDto.PlanId);

                var job = new PlanPricingJob
                {
                    PlanVersionId = plan.VersionId,
                    Status = BackgroundJobProcessingStatus.Queued,
                    Queued = currentDate
                };
                using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                {
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
            _PlanRepository.SavePlanPricingParameters(planPricingParametersDto);

            return jobId;
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

        public PlanPricingDefaults GetPlanPricingDefaults()
        {
            const int defaultPercent = 0;
            const float defaultMargin = 20;
            var allSources = _InventoryRepository.GetInventorySources();

            return new PlanPricingDefaults
            {
                UnitCaps = 1,
                UnitCapType = UnitCapEnum.Per30Min,
                InventorySourcePercentages = PlanPricingInventorySourceSortEngine.GetSortedInventorySourcePercents(defaultPercent, allSources),
                InventorySourceTypePercentages = PlanPricingInventorySourceSortEngine.GetSortedInventorySourceTypePercents(defaultPercent),
                Margin = defaultMargin
            };
        }

        public PlanPricingResponseDto GetCurrentPricingExecution(int planId)
        {
            var job = _PlanRepository.GetLatestPricingJob(planId);
            PlanPricingResultDto pricingExecutionResult = null;

            if(job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                throw new Exception("Error encountered while running Pricing Model, please contact a system administrator for help");
            }

            if (job != null && job.Status == BackgroundJobProcessingStatus.Succeeded)
            {
                pricingExecutionResult = _PlanRepository.GetPricingResults(planId);
                _ConvertImpressionsToUserFormat(pricingExecutionResult);
            } 

            //pricingExecutionResult might be null when there is no pricing run for the latest version            
            return new PlanPricingResponseDto
            {
                Job = job,
                Result = pricingExecutionResult ?? new PlanPricingResultDto(),
                IsPricingModelRunning = IsPricingModelRunning(job)
            };
        }

        private void _ConvertImpressionsToUserFormat(PlanPricingResultDto planPricingResult)
        {
            if (planPricingResult == null)
                return;

            planPricingResult.Totals.AvgImpressions /= 1000;

            foreach (var program in planPricingResult.Programs)
            {
                program.AvgImpressions /= 1000; 
            }
        }

        /// <inheritdoc />
        public PlanPricingResponseDto CancelCurrentPricingExecution(int planId)
        {
            var job = _PlanRepository.GetLatestPricingJob(planId);
            PlanPricingResultDto pricingExecutionResult = null;

            if (job != null && job.Status == BackgroundJobProcessingStatus.Failed)
            {
                throw new Exception("Error encountered while running Pricing Model, please contact a system administrator for help");
            }

            if (!IsPricingModelRunning(job))
            {
                throw new Exception("Error encountered while canceling Pricing Model, process is not running");
            }

            _BackgroundJobClient.Delete(job.HangfireJobId);
            job.Status = BackgroundJobProcessingStatus.Canceled;

            _PlanRepository.UpdatePlanPricingJob(job);

            //pricingExecutionResult might be null when there is no pricing run for the latest version            
            return new PlanPricingResponseDto
            {
                Job = job,
                Result = pricingExecutionResult ?? new PlanPricingResultDto(),
                IsPricingModelRunning = false
            };
        }

        public static bool IsPricingModelRunning(PlanPricingJob job)
        {
            return job != null && (job.Status == BackgroundJobProcessingStatus.Queued || job.Status == BackgroundJobProcessingStatus.Processing);
        }

        public bool IsPricingModelRunningForPlan(int planId)
        {
            var job = _PlanRepository.GetLatestPricingJob(planId);
            return IsPricingModelRunning(job);
        }

        private PlanPricingApiRequestParametersDto _GetPricingApiRequestParameters(PlanPricingParametersDto planPricingParametersDto, PlanDto plan, List<PlanPricingMarketDto> pricingMarkets)
        {
            var parameters = _MapToApiParametersRequest(planPricingParametersDto);

            parameters.Markets = pricingMarkets;
            parameters.CoverageGoalPercent = plan.CoverageGoalPercent ?? 0;


            return parameters;
        }

        private List<PlanPricingMarketDto> _MapToPlanPricingPrograms(PlanDto plan)
        {
            var pricingMarkets = new List<PlanPricingMarketDto>();

            foreach (var planMarket in plan.AvailableMarkets)
            {
                pricingMarkets.Add(new PlanPricingMarketDto
                {
                    MarketId = planMarket.Id,
                    MarketName = planMarket.Market,
                    MarketShareOfVoice = planMarket.ShareOfVoicePercent ?? 0,
                });
            }

            return pricingMarkets;
        }

        private PlanPricingApiRequestParametersDto _MapToApiParametersRequest(PlanPricingParametersDto planPricingParametersDto)
        {
            var parameters = new PlanPricingApiRequestParametersDto
            {
                PlanId = planPricingParametersDto.PlanId,
                MinCpm = planPricingParametersDto.MinCpm,
                MaxCpm = planPricingParametersDto.MaxCpm,
                ImpressionsGoal = planPricingParametersDto.DeliveryImpressions,
                BudgetGoal = planPricingParametersDto.Budget,
                ProprietaryBlend = planPricingParametersDto.ProprietaryBlend,
                CpmGoal = planPricingParametersDto.CPM,
                CompetitionFactor = planPricingParametersDto.CompetitionFactor,
                InflationFactor = planPricingParametersDto.InflationFactor,
                UnitCaps = planPricingParametersDto.UnitCaps,
                UnitCapType = planPricingParametersDto.UnitCapsType,
                InventorySourcePercentages = planPricingParametersDto.InventorySourcePercentages,
                InventorySourceTypePercentages = planPricingParametersDto.InventorySourceTypePercentages,
                Margin = planPricingParametersDto.Margin
            };

            return parameters;
        }

        public List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId)
        {
            return _PlanRepository.GetPlanPricingRuns(planId);
        }

        private List<PlanPricingInventoryProgramDto> _MapToPlanPricingPrograms(List<ProposalProgramDto> programs, PlanDto plan)
        {
            var pricingPrograms = new List<PlanPricingInventoryProgramDto>();
            var spotLength = _SpotLengthEngine.GetSpotLengthValueById(plan.SpotLengthId);
            var spotLengthsMultipliers = _SpotLengthRepository.GetSpotLengthMultipliers();
            var deliveryMultiplier = spotLengthsMultipliers.Single(s => s.Key == spotLength);

            foreach (var program in programs)
            {
                var programNames = program.ManifestDayparts.Select(d => d.ProgramName);
                var planMarket = plan.AvailableMarkets.FirstOrDefault(m => m.Id == program.Market.Id);
                var marketShareOfVoice = 0d;

                if (planMarket != null)
                    marketShareOfVoice = planMarket.ShareOfVoicePercent ?? 0;

                var pricingProgram = new PlanPricingInventoryProgramDto
                {
                    ProgramNames = new List<string>(programNames),
                    SpotLength = spotLength,
                    DeliveryMultiplier = deliveryMultiplier.Value,
                    Station = program.Station,
                    Rate = program.SpotCost,
                    PlanPricingMarket = new PlanPricingMarketDto
                    {
                        MarketId = program.Market.Id,
                        MarketName = program.Market.Display,
                        MarketShareOfVoice = marketShareOfVoice,
                        // Random value for now.
                        MarketSegment = program.ManifestId % 4 + 1
                    },
                    GuaranteedImpressions = program.ProvidedUnitImpressions ?? 0,
                    ProjectedImpressions = program.UnitImpressions,
                };

                pricingPrograms.Add(pricingProgram);
            }

            return pricingPrograms;
        }

        protected List<PlanPricingApiRequestSpotsDto> _GetPricingModelSpots(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs)
        {
            var marketCoveragesByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var pricingModelSpots = new List<PlanPricingApiRequestSpotsDto>();

            foreach (var program in programs)
            {
                foreach (var daypart in program.ManifestDayparts)
                {
                    var impressions = program.ProvidedImpressions ?? program.ProjectedImpressions;

                    if (!_IsSpotCostValidForPricingModelInput(impressions))
                        continue;

                    if (!_AreImpressionsValidForPricingModelInput(program.SpotCost))
                        continue;

                    var spots = program.ManifestWeeks.Select(manifestWeek => new PlanPricingApiRequestSpotsDto
                    {
                        Id = program.ManifestId,
                        MediaWeekId = manifestWeek.ContractMediaWeekId,
                        DaypartId = program.StandardDaypartId,
                        Impressions = impressions,
                        Cost = program.SpotCost,
                        StationId = program.Station.Id,
                        MarketCode = program.Station.MarketCode.Value,
                        PercentageOfUs = marketCoveragesByMarketCode[program.Station.MarketCode.Value],
                        SpotDays = daypart.Daypart.ActiveDays,
                        SpotHours = daypart.Daypart.GetDurationPerDayInHours()

                        // Data Science API does not yet handle these fields.
                        //Unit = program.Unit,
                        //InventorySource = program.InventorySource,
                        //InventorySourceType = program.InventorySourceType
                    });

                    pricingModelSpots.AddRange(spots);
                }
            }

            return pricingModelSpots;
        }
        
        protected bool _AreImpressionsValidForPricingModelInput(decimal? impressions)
        {
            var result = impressions > 0;
            return result;
        }

        protected bool _IsSpotCostValidForPricingModelInput(double? spotCost)
        {
            var result = spotCost > 0;
            return result;
        }

        protected bool _AreWeeklyImpressionsValidForPricingModelInput(double? impressions)
        {
            var result = impressions > 0;
            return result;
        }

        protected List<PlanPricingApiRequestWeekDto> _GetPricingModelWeeks(
            PlanDto plan, 
            List<PricingEstimate> proprietaryEstimates,
            ProgramInventoryOptionalParametersDto parameters)
        {
            var pricingModelWeeks = new List<PlanPricingApiRequestWeekDto>();
            var marketCoverageGoal = GeneralMath.ConvertPercentageToFraction(plan.CoverageGoalPercent.Value);
            var marketsWithSov = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue);
            var daypartsWithWeighting = plan.Dayparts.Where(x => x.WeightingGoalPercent.HasValue);
            var planPricingParameters = plan.PricingParameters;

            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                if (_AreWeeklyImpressionsValidForPricingModelInput(week.WeeklyImpressions) == false)
                {
                    continue;
                }

                var mediaWeekId = week.MediaWeekId;
                var estimatesForWeek = proprietaryEstimates.Where(x => x.MediaWeekId == mediaWeekId);
                var estimatedImpressions = estimatesForWeek.Sum(x => x.Impressions);
                var estimatedCost = estimatesForWeek.Sum(x => x.Cost);

                var impressionGoal = week.WeeklyImpressions > estimatedImpressions ? week.WeeklyImpressions - estimatedImpressions : 0;
                var weeklyBudget = week.WeeklyBudget > estimatedCost ? week.WeeklyBudget - estimatedCost : 0;
                
                if (parameters.Margin > 0)
                {
                    weeklyBudget = weeklyBudget * (decimal)(1.0 - (parameters.Margin / 100.0));
                }

                var cpmGoal = ProposalMath.CalculateCpm(weeklyBudget, impressionGoal);
                (double capTime, string capType) = _GetFrequencyCapTimeAndCapTypeString(planPricingParameters.UnitCapsType);

                var pricingWeek = new PlanPricingApiRequestWeekDto
                {
                    MediaWeekId = mediaWeekId,
                    ImpressionGoal = impressionGoal,
                    CpmGoal = cpmGoal,
                    MarketCoverageGoal = marketCoverageGoal,
                    FrequencyCapSpots = planPricingParameters.UnitCaps,
                    FrequencyCapTime = capTime,
                    FrequencyCapUnit = capType,
                    ShareOfVoice = marketsWithSov.Select(x => new ShareOfVoice
                    {
                        MarketCode = x.MarketCode,
                        MarketGoal = GeneralMath.ConvertPercentageToFraction(x.ShareOfVoicePercent.Value)
                    }).ToList(),
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

        private (double capTime, string capType) _GetFrequencyCapTimeAndCapTypeString(UnitCapEnum unitCap)
        {
            if (unitCap == UnitCapEnum.Per30Min)
                return (capTime: 0.5d, "hour");

            if (unitCap == UnitCapEnum.PerHour)
                return (capTime: 1d, "hour");

            if (unitCap == UnitCapEnum.PerDay)
                return (capTime: 1d, "day");

            if (unitCap == UnitCapEnum.PerWeek)
                return (capTime: 1d, "week");
            
            throw new ApplicationException("Unsupported unit cap type was discovered");
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
                Queued = DateTime.Now
            };
            var newJobId = _SavePricingJobAndParameters(newJob, jobParams);
            
            // call the job directly
            RunPricingJob(jobParams, newJobId, CancellationToken.None);

            return newJobId;
        }

        public void RunPricingJob(PlanPricingParametersDto planPricingParametersDto, int jobId, CancellationToken token)
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
                var plan = _PlanRepository.GetPlan(planPricingParametersDto.PlanId);
                var pricingMarkets = _MapToPlanPricingPrograms(plan);
                var parameters = _GetPricingApiRequestParameters(planPricingParametersDto, plan, pricingMarkets);
                var programInventoryParameters = new ProgramInventoryOptionalParametersDto
                {
                    MinCPM = planPricingParametersDto.MinCpm,
                    MaxCPM = planPricingParametersDto.MaxCpm,
                    InflationFactor = planPricingParametersDto.InflationFactor,
                    Margin = planPricingParametersDto.Margin
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
                    diagnostic);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_GATHERING_INVENTORY);
                
                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);
                _ApplyMargin(parameters);
                _ValidateInventory(inventory);

                var pricingApiRequest = new PlanPricingApiRequestDto
                {
                    Weeks = _GetPricingModelWeeks(plan, proprietaryEstimates, programInventoryParameters),
                    Spots = _GetPricingModelSpots(plan, inventory)
                };
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_PREPARING_API_REQUEST);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_PARAMETERS);
                _PlanRepository.SavePricingRequest(parameters);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_PRICING_PARAMETERS);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_CALLING_API);
                var apiAllocationResult = _PricingApiClient.GetPricingSpotsResult(pricingApiRequest);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_CALLING_API);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_VALIDATING_AND_MAPPING_API_RESPONSE);
                if (apiAllocationResult.Error != null)
                {
                    var errorMessage = string.Join(",", apiAllocationResult.Error.Messages).Trim(',');
                    throw new Exception($"Pricing API returned the following error: {apiAllocationResult.Error.Name} -  {errorMessage}");
                }
                
                var spots = _MapToResultSpots(apiAllocationResult, pricingApiRequest, inventory);
                var pricingCpm = _CalculatePricingCpm(spots, proprietaryEstimates, parameters.Margin);
                var allocationResult = new PlanPricingAllocationResult
                {
                    RequestId = apiAllocationResult.RequestId,
                    PricingCpm = pricingCpm,
                    Spots = spots
                };

                _ValidateAllocationResult(allocationResult);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_VALIDATING_AND_MAPPING_API_RESPONSE);

                token.ThrowIfCancellationRequested();

                diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);
                var aggregatedResults = AggregateResults(inventory, allocationResult);
                diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_AGGREGATING_ALLOCATION_RESULTS);

                token.ThrowIfCancellationRequested();

                using (var transaction = new TransactionScopeWrapper())
                {
                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);
                    _PlanRepository.SavePricingApiResults(plan.Id, allocationResult);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_ALLOCATION_RESULTS);

                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);
                    _PlanRepository.SavePricingAggregateResults(plan.Id, aggregatedResults);
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SAVING_AGGREGATION_RESULTS);

                    diagnostic.Start(PlanPricingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);
                    var pricingJob = _PlanRepository.GetPlanPricingJob(jobId);
                    pricingJob.Status = BackgroundJobProcessingStatus.Succeeded;
                    pricingJob.Completed = DateTime.Now;
                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_SETTING_JOB_STATUS_TO_SUCCEEDED);

                    diagnostic.End(PlanPricingJobDiagnostic.SW_KEY_TOTAL_DURATION);
                    pricingJob.DiagnosticResult = diagnostic.ToString();

                    _PlanRepository.UpdatePlanPricingJob(pricingJob);

                    transaction.Complete();
                }
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

        private void _ApplyMargin(PlanPricingApiRequestParametersDto parameters)
        {
            if (parameters.Margin > 0)
            {
                parameters.BudgetGoal = parameters.BudgetGoal * (decimal)(1.0 - (parameters.Margin / 100.0));
                parameters.CpmGoal = parameters.BudgetGoal / Convert.ToDecimal(parameters.ImpressionsGoal);
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
                Status = BackgroundJobProcessingStatus.Canceled,
                ErrorMessage = exception.ToString(),
                Completed = DateTime.Now
            });

                _LogError($"{logMessage} : {exception.Message}", exception);
        }

        protected decimal _CalculatePricingCpm(List<PlanPricingAllocatedSpot> spots, List<PricingEstimate> proprietaryEstimates, double margin)
        {
            var allocatedTotalCost = spots.Sum(x => x.TotalCost);
            var allocatedTotalImpressions = spots.Sum(x => x.TotalImpressions);
            var marginAdjustedAllocatedTotalCost = allocatedTotalCost / (decimal)(1.0 - (margin / 100.0));

            var totalCostForProprietary = proprietaryEstimates.Sum(x => x.Cost);
            var totalImpressionsForProprietary = proprietaryEstimates.Sum(x => x.Impressions);

            var totalCost = marginAdjustedAllocatedTotalCost + totalCostForProprietary;
            var totalImpressions = allocatedTotalImpressions + totalImpressionsForProprietary;

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
                    diagnostic);

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
                    diagnostic);

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
                var impressionsPerSpot = program.ProvidedImpressions ?? program.ProjectedImpressions;
                var impressions = impressionsPerSpot * spots * percentageToUse / 100;
                var cost = program.SpotCost * spots * percentageToUse / 100;

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
            PlanPricingApiSpotsResponseDto apiSpotsResults, 
            PlanPricingApiRequestDto pricingApiRequest,
            List<PlanPricingInventoryProgram> inventoryPrograms)
        {
            var results = new List<PlanPricingAllocatedSpot>();
            var daypartDefaultsById = _DaypartDefaultRepository
                .GetAllDaypartDefaults()
                .ToDictionary(x => x.Id, x => x);

            foreach (var allocation in apiSpotsResults.Results)
            {
                var originalSpot = pricingApiRequest.Spots.FirstOrDefault(x => 
                    x.Id == allocation.ManifestId &&
                    x.MediaWeekId == allocation.MediaWeekId);

                if (originalSpot == null)
                    throw new Exception("Response from API contains manifest id not found in sent data");
                
                var program = inventoryPrograms.Single(x => x.ManifestId == allocation.ManifestId);
                var inventoryWeek = program.ManifestWeeks.Single(x => x.ContractMediaWeekId == originalSpot.MediaWeekId);

                var spotResult = new PlanPricingAllocatedSpot
                {
                    Id = originalSpot.Id,
                    SpotCost = originalSpot.Cost,
                    StandardDaypart = daypartDefaultsById[originalSpot.DaypartId],
                    Impressions = originalSpot.Impressions,
                    ContractMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(inventoryWeek.ContractMediaWeekId),
                    InventoryMediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(inventoryWeek.InventoryMediaWeekId),
                    Spots = allocation.Frequency
                };

                results.Add(spotResult);
            }

            return results;
        }

        public void _ValidateAllocationResult(PlanPricingAllocationResult apiResponse)
        {
            if (!apiResponse.Spots.Any())
            {
                var msg = $"The api returned no spots for request '{apiResponse.RequestId}'.";
                throw new Exception(msg);
            }
        }

        public PlanPricingResultDto AggregateResults(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingAllocationResult apiResponse)
        {
            var result = new PlanPricingResultDto();
            var programs = _GetPrograms(inventory, apiResponse);
            var totalCostForAllPrograms = programs.Sum(x => x.TotalCost);
            var totalImpressionsForAllProgams = programs.Sum(x => x.TotalImpressions);
            var totalSpotsForAllPrograms = programs.Sum(x => x.TotalSpots);

            result.Programs.AddRange(programs.Select(x => new PlanPricingProgramDto
            {
                ProgramName = x.ProgramName,
                Genre = x.Genre,
                StationCount = x.Stations.Count,
                MarketCount = x.MarketCodes.Count,
                AvgImpressions = x.AvgImpressions,
                AvgCpm = x.AvgCpm,
                PercentageOfBuy = ProposalMath.CalculateImpressionsPercentage(x.TotalImpressions, totalImpressionsForAllProgams)
            }));

            result.Totals = new PlanPricingTotalsDto
            {
                MarketCount = programs.SelectMany(x => x.MarketCodes).Distinct().Count(),
                StationCount = programs.SelectMany(x => x.Stations).Distinct().Count(),
                AvgImpressions = ProposalMath.CalculateAvgImpressions(totalImpressionsForAllProgams, totalSpotsForAllPrograms),
                AvgCpm = ProposalMath.CalculateCpm(totalCostForAllPrograms, totalImpressionsForAllProgams)
            };

            result.OptimalCpm = apiResponse.PricingCpm;

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
                    Stations = programInventory.Select(x => x.Manifest.Station.LegacyCallLetters).Distinct().ToList(),
                    MarketCodes = programInventory.Select(x => x.Manifest.Station.MarketCode.Value).Distinct().ToList()
                };

                result.Add(program);
            };

            return result;
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
                var spots = apiProgram.Spots;
                var spotCost = apiProgram.SpotCost;
                var totalCost = spots * spotCost;
                var impressionsPerSpot = apiProgram.Impressions;
                var totalImpressions = spots * impressionsPerSpot;

                totalProgramCost += totalCost;
                totalProgramImpressions += totalImpressions;
                totalProgramSpots += spots;
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
                Margin = requestParameters.Margin
            };

            var plan = _PlanRepository.GetPlan(planId);
            var inventorySourceIds = requestParameters.InventorySourceIds.IsEmpty() ?
                _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes()) :
                requestParameters.InventorySourceIds;

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(plan, pricingParams, inventorySourceIds, diagnostic);
            var proprietaryEstimates = _CalculateProprietaryInventorySourceEstimates(plan, pricingParams, diagnostic);

            var pricingApiRequest = new PlanPricingApiRequestDto
            {
                Weeks = _GetPricingModelWeeks(plan, proprietaryEstimates, pricingParams),
                Spots = _GetPricingModelSpots(plan, inventory)
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
                InflationFactor = requestParameters.InflationFactor
            };
            var inventorySourceIds = requestParameters.InventorySourceIds.IsEmpty() ?
                _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes()) :
                requestParameters.InventorySourceIds;

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(plan, pricingParams, inventorySourceIds, diagnostic);
            return inventory;
        }

        public string ForceCompletePlanPricingJob(int jobId, string username)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);
            job.Status = BackgroundJobProcessingStatus.Failed;
            job.ErrorMessage = $"Job status set to error by user '{username}'.";
            job.Completed = DateTime.Now;
            _PlanRepository.UpdatePlanPricingJob(job);

            return $"Job Id '{jobId}' has been forced to complete.";
        }

        private class ProgramWithManifestWeek
        {
            public PlanPricingInventoryProgram Program { get; set; }

            public PlanPricingInventoryProgram.ManifestWeek ManifestWeek { get; set; }
        }
    }
}
