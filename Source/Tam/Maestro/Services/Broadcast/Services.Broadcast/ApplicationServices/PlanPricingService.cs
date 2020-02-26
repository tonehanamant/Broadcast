﻿using Common.Services.ApplicationServices;
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
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Common.Utilities.Logging;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using static Services.Broadcast.BusinessEngines.PlanPricingInventoryEngine;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPlanPricingService : IApplicationService
    {
        PlanPricingJob QueuePricingJob(PlanPricingParametersDto planPricingParametersDto, DateTime currentDate);
        PlanPricingResponseDto GetCurrentPricingExecution(int planId);
        [Queue("planpricing")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void RunPricingJob(PlanPricingParametersDto planPricingParametersDto, int jobId);
        List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId);

        PlanPricingApiRequestDto GetPricingInventory(int planId, PricingInventoryGetRequestParametersDto requestParameters);

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

    public class PlanPricingService : IPlanPricingService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanPricingInventoryEngine _PlanPricingInventoryEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IPricingApiClient _PricingApiClient;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IBroadcastLockingManagerApplicationService _LockingManagerApplicationService;

        public PlanPricingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                  ISpotLengthEngine spotLengthEngine,
                                  IPricingApiClient pricingApiClient,
                                  IBackgroundJobClient backgroundJobClient,
                                  IPlanPricingInventoryEngine planPricingInventoryEngine,
                                  IBroadcastLockingManagerApplicationService lockingManagerApplicationService)
        {
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _SpotLengthRepository = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _SpotLengthEngine = spotLengthEngine;
            _PricingApiClient = pricingApiClient;
            _BackgroundJobClient = backgroundJobClient;
            _PlanPricingInventoryEngine = planPricingInventoryEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
        }

        public PlanPricingJob QueuePricingJob(PlanPricingParametersDto planPricingParametersDto, DateTime currentDate)
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

                _SavePricingJobAndParameters(job, planPricingParametersDto);

                _BackgroundJobClient.Enqueue<IPlanPricingService>(x => x.RunPricingJob(planPricingParametersDto, job.Id));

                return job;
            }
        }

        private int _SavePricingJobAndParameters(PlanPricingJob job, PlanPricingParametersDto planPricingParametersDto)
        {
            using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
            {
                var jobId = _PlanRepository.AddPlanPricingJob(job);
                job.Id = jobId;
                _PlanRepository.SavePlanPricingParameters(planPricingParametersDto);

                transaction.Complete();
                return jobId;
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

        public PlanPricingDefaults GetPlanPricingDefaults()
        {
            const int default_percent = 10;
            const float default_margin = 20;
            var allSources = _InventoryRepository.GetInventorySources();

            return new PlanPricingDefaults
            {
                UnitCaps = 1,
                UnitCapType = UnitCapEnum.Per30Min,
                InventorySourcePercentages = PlanPricingInventorySourceSortEngine.GetSortedInventorySourcePercents(default_percent, allSources),
                InventorySourceTypePercentages = PlanPricingInventorySourceSortEngine.GetSortedInventorySourceTypePercents(default_percent),
                Margin = default_margin
            };
        }

        public PlanPricingResponseDto GetCurrentPricingExecution(int planId)
        {
            var job = _PlanRepository.GetLatestPricingJob(planId);

            PlanPricingResultDto pricingExecutionResult = null;

            if (job != null && job.Status == BackgroundJobProcessingStatus.Succeeded)
            {
                pricingExecutionResult = _PlanRepository.GetPricingResults(planId);
            }
            else
            {
                pricingExecutionResult = new PlanPricingResultDto
                {
                    Totals = new PlanPricingTotalsDto(),
                    Programs = new List<PlanPricingProgramDto>()
                };
            }

            return new PlanPricingResponseDto
            {
                Job = job,
                Result = pricingExecutionResult,
                IsPricingModelRunning = IsPricingModelRunning(job)
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

        protected List<PlanPricingApiRequestSpotsDto> _GetPricingModelSpots(List<PlanPricingInventoryProgram> programs)
        {
            var pricingModelSpots = new List<PlanPricingApiRequestSpotsDto>();

            foreach (var program in programs)
            {
                foreach (var daypart in program.ManifestDayparts)
                {
                    var impressions = program.ProvidedImpressions ?? program.ProjectedImpressions;
                    if (_IsSpotCostValidForPricingModelInput(impressions) == false)
                    {
                        continue;
                    }
                    if (_AreImpressionsValidForPricingModelInput(program.SpotCost) == false)
                    {
                        continue;
                    }

                    var spots = program.ManifestWeeks.Select(manifestWeek => new PlanPricingApiRequestSpotsDto
                    {
                        Id = program.ManifestId,
                        MediaWeekId = manifestWeek.MediaWeekId,
                        DaypartId = daypart.Daypart.Id,
                        Impressions = impressions,
                        Cost = program.SpotCost,
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

        protected List<PlanPricingApiRequestWeekDto> _GetPricingModelWeeks(PlanDto plan)
        {
            var pricingModelWeeks = new List<PlanPricingApiRequestWeekDto>();

            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                if (_AreWeeklyImpressionsValidForPricingModelInput(week.WeeklyImpressions) == false)
                {
                    continue;
                }

                pricingModelWeeks.Add(new PlanPricingApiRequestWeekDto
                {
                    MediaWeekId = week.MediaWeekId,
                    ImpressionGoal = week.WeeklyImpressions,
                    CpmGoal = (week.WeeklyBudget / (decimal)week.WeeklyImpressions) * 1000
                });
            }

            return pricingModelWeeks;
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
            RunPricingJob(jobParams, newJobId);

            return newJobId;
        }

        public void RunPricingJob(PlanPricingParametersDto planPricingParametersDto, int jobId)
        {
            var planPricingJobDiagnostic = new PlanPricingJobDiagnostic { JobId = jobId };

            planPricingJobDiagnostic.RecordStart();

            _PlanRepository.UpdatePlanPricingJob(new PlanPricingJob
            {
                Id = jobId,
                Status = BackgroundJobProcessingStatus.Processing,
            });

            try
            {
                var plan = _PlanRepository.GetPlan(planPricingParametersDto.PlanId);
                var pricingMarkets = _MapToPlanPricingPrograms(plan);
                var parameters = _GetPricingApiRequestParameters(planPricingParametersDto, plan, pricingMarkets);
                var programInventoryParameters = new ProgramInventoryOptionalParametersDto
                {
                    MinCPM = planPricingParametersDto.MinCpm,
                    MaxCPM = planPricingParametersDto.MaxCpm,
                    InflationFactor = planPricingParametersDto.InflationFactor
                };

                planPricingJobDiagnostic.RecordInventorySourceEstimatesCalculationStart();

                var inventorySourceEstimates = _CalculateProprietaryInventorySourceEstimates(plan, programInventoryParameters);
                _PlanRepository.SavePlanPricingEstimates(jobId, inventorySourceEstimates);

                planPricingJobDiagnostic.RecordInventorySourceEstimatesCalculationEnd();

                planPricingJobDiagnostic.RecordGatherInventoryStart();

                var inventorySourceIds = _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes());
                var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    programInventoryParameters,
                    inventorySourceIds);

                if (parameters.Margin > 0)
                {
                    parameters.BudgetGoal = parameters.BudgetGoal * (decimal)(1.0 - (parameters.Margin / 100.0));
                    parameters.CpmGoal = parameters.BudgetGoal / Convert.ToDecimal(parameters.ImpressionsGoal);
                }
                
                planPricingJobDiagnostic.RecordGatherInventoryEnd();

                _ValidateInventory(inventory);

                var pricingApiRequest = new PlanPricingApiRequestDto
                {
                    Weeks = _GetPricingModelWeeks(plan),
                    Spots = _GetPricingModelSpots(inventory)
                };

                _PlanRepository.SavePricingRequest(parameters);

                planPricingJobDiagnostic.RecordApiCallStart();

                var apiCpmResult = _PricingApiClient.GetPricingCalculationResult(pricingApiRequest);
                var apiAllocationResult = _PricingApiClient.GetPricingSpotsResult(pricingApiRequest);
                var spots = _MapToResultSpots(apiAllocationResult, pricingApiRequest);
                var combinedApiResponse = new PlanPricingApiResponseDto
                {
                    RequestId = apiAllocationResult.RequestId,
                    Results = new PlanPricingApiResultDto
                    {
                        OptimalCpm = apiCpmResult.Results.MinimumCost,
                        Spots = spots
                    }
                };

                _ValidateApiResponse(combinedApiResponse);

                planPricingJobDiagnostic.RecordApiCallEnd();

                planPricingJobDiagnostic.RecordEnd();

                var aggregatedResults = AggregateResults(inventory, combinedApiResponse);

                using (var transaction = new TransactionScopeWrapper())
                {
                    _PlanRepository.SavePricingApiResults(plan.Id, combinedApiResponse);
                    _PlanRepository.SavePricingAggregateResults(plan.Id, aggregatedResults);
                    _PlanRepository.UpdatePlanPricingJob(new PlanPricingJob
                    {
                        Id = jobId,
                        Status = BackgroundJobProcessingStatus.Succeeded,
                        Completed = DateTime.Now,
                        DiagnosticResult = planPricingJobDiagnostic.ToString()
                    });

                    transaction.Complete();
                }
            }
            catch (Exception exception)
            {
                _PlanRepository.UpdatePlanPricingJob(new PlanPricingJob
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Failed,
                    ErrorMessage = exception.ToString(),
                    Completed = DateTime.Now
                });

                LogHelper.Logger.Error($"Error attempting to run the pricing model : {exception.Message}", exception);
            }
        }

        private List<PricingEstimate> _CalculateProprietaryInventorySourceEstimates(
            PlanDto plan,
            ProgramInventoryOptionalParametersDto parameters)
        {
            var result = new List<PricingEstimate>();
            var supportedInventorySourceIds = _GetInventorySourceIdsByTypes(new List<InventorySourceTypeEnum>
            {
                InventorySourceTypeEnum.Barter,
                InventorySourceTypeEnum.ProprietaryOAndO,
                InventorySourceTypeEnum.Diginet
            });

            var inventorySourcePreferences = plan.PricingParameters.InventorySourcePercentages
                .Where(x => x.Percentage > 0 && supportedInventorySourceIds.Contains(x.Id))
                .ToList();

            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(
                    plan,
                    parameters,
                    inventorySourcePreferences.Select(x => x.Id));

            foreach (var inventorySourcePreference in inventorySourcePreferences)
            {
                var inventorySourceId = inventorySourcePreference.Id;

                var estimates = inventory
                    .Where(x => x.InventorySource.Id == inventorySourceId)
                    .SelectMany(x => x.ManifestWeeks.Select(w => new ProgramWithManifestWeek
                    {
                        Program = x,
                        ManifestWeek = w
                    }))
                    .Where(x => x.ManifestWeek.Spots > 0)
                    .GroupBy(x => x.ManifestWeek.MediaWeekId)
                    .Select(programsByMediaWeekGrouping =>
                    {
                        _CalculateImpressionsAndCost(
                            programsByMediaWeekGrouping,
                            inventorySourcePreference.Percentage,
                            out var totalWeekImpressions,
                            out var totalWeekCost);

                        return new PricingEstimate
                        {
                            InventorySourceId = inventorySourceId,
                            MediaWeekId = programsByMediaWeekGrouping.Key,
                            Impressions = totalWeekImpressions,
                            Cost = totalWeekCost
                        };
                    });

                result.AddRange(estimates);
            }

            return result;
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

        private List<int> _GetInventorySourceIdsByTypes(List<InventorySourceTypeEnum> inventorySourceTypes)
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

        private List<PlanPricingApiResultSpotDto> _MapToResultSpots(PlanPricingApiSpotsResponseDto apiSpotsResults, PlanPricingApiRequestDto pricingApiRequest)
        {
            var results = new List<PlanPricingApiResultSpotDto>();

            foreach (var weeklyAllocation in apiSpotsResults.Results)
            {
                foreach (var manifestId in weeklyAllocation.AllocatedManifestIds)
                {
                    var originalSpot = pricingApiRequest.Spots
                                        .FirstOrDefault(x => x.MediaWeekId == weeklyAllocation.MediaWeekId &&
                                                             manifestId == x.Id);

                    if (originalSpot == null)
                        throw new Exception("Response from API contains manifest id not found in sent data");

                    var spotResult = new PlanPricingApiResultSpotDto
                    {
                        Id = originalSpot.Id,
                        Cost = originalSpot.Cost,
                        DaypartId = originalSpot.DaypartId,
                        Impressions = originalSpot.Impressions,
                        MediaWeekId = originalSpot.MediaWeekId,
                        Spots = 1
                    };

                    results.Add(spotResult);
                }
            }

            return results;
        }

        public void _ValidateApiResponse(PlanPricingApiResponseDto apiResponse)
        {
            if (apiResponse.Results == null)
            {
                var msg = $"The api returned no results for request '{apiResponse.RequestId}'.";
                throw new Exception(msg);
            }

            if (!apiResponse.Results.Spots.Any())
            {
                var msg = $"The api returned no spots for request '{apiResponse.RequestId}'.";
                throw new Exception(msg);
            }
        }

        public PlanPricingResultDto AggregateResults(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingApiResponseDto apiResponse)
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
                AvgCpm = ProposalMath.CalculateAvgCpm(totalCostForAllPrograms, totalImpressionsForAllProgams)
            };

            result.OptimalCpm = apiResponse.Results.OptimalCpm;

            return result;
        }

        private List<PlanPricingProgram> _GetPrograms(
            List<PlanPricingInventoryProgram> inventory,
            PlanPricingApiResponseDto apiResponse)
        {
            var result = new List<PlanPricingProgram>();
            var inventoryGroupedByProgramName = inventory
                .SelectMany(x => x.ManifestDayparts.Select(d => new PlanPricingManifestWithManifestDaypart
                {
                    Manifest = x,
                    ManifestDaypart = d
                }))
                .Where(x => x.ManifestDaypart.PrimaryProgram != null)
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
                    Genre = inventoryByProgramName.First().ManifestDaypart.PrimaryProgram.MaestroGenre,
                    AvgImpressions = ProposalMath.CalculateAvgImpressions(programImpressions, programSpots),
                    AvgCpm = ProposalMath.CalculateAvgCpm(programCost, programImpressions),
                    TotalImpressions = programImpressions,
                    TotalCost = programCost,
                    TotalSpots = programSpots,
                    Stations = programInventory.Select(x => x.Manifest.StationLegacyCallLetters).Distinct().ToList(),
                    MarketCodes = programInventory.Select(x => x.Manifest.MarketCode).Distinct().ToList()
                };

                result.Add(program);
            };

            return result;
        }

        private List<PlanPricingApiResultSpotDto> _GetAllocatedProgramSpots(PlanPricingApiResponseDto apiResponse, List<PlanPricingManifestWithManifestDaypart> programInventory)
        {
            var result = new List<PlanPricingApiResultSpotDto>();

            foreach (var spot in apiResponse.Results.Spots)
            {
                if (programInventory.Any(x => x.Manifest.ManifestId == spot.Id && x.ManifestDaypart.Daypart.Id == spot.DaypartId))
                {
                    result.Add(spot);
                }
            }

            return result;
        }

        private void _CalculateProgramTotals(
            IEnumerable<PlanPricingApiResultSpotDto> allocatedProgramSpots,
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
                var spotCost = apiProgram.Cost;
                var totalCost = spots * spotCost;
                var impressionsPerSpot = apiProgram.Impressions;
                var totalImpressions = spots * impressionsPerSpot;

                totalProgramCost += totalCost;
                totalProgramImpressions += totalImpressions;
                totalProgramSpots += spots;
            }
        }

        public PlanPricingApiRequestDto GetPricingInventory(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            var plan = _PlanRepository.GetPlan(planId);
            var pricingParams = new PlanPricingInventoryEngine.ProgramInventoryOptionalParametersDto
            {
                MinCPM = requestParameters.MinCpm,
                MaxCPM = requestParameters.MaxCpm,
                InflationFactor = requestParameters.InflationFactor
            };

            var inventorySourceIds = requestParameters.InventorySourceIds.IsEmpty() ?
                _GetInventorySourceIdsByTypes(_GetSupportedInventorySourceTypes()) :
                requestParameters.InventorySourceIds;
            
            var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(plan, pricingParams, inventorySourceIds);

            var pricingApiRequest = new PlanPricingApiRequestDto
            {
                Weeks = _GetPricingModelWeeks(plan),
                Spots = _GetPricingModelSpots(inventory)
            };

            return pricingApiRequest;
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
