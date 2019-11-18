using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.PlanPricing;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPlanPricingService : IApplicationService
    {
        PlanPricingJob QueuePricingJob(PlanPricingRequestDto planPricingRequestDto, DateTime currentDate);
        PlanPricingResponseDto GetCurrentPricingExecution(int planId);
        void RunPricingJob(PlanPricingRequestDto planPricingRequestDto, int jobId);
        List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId);

        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetUnitCaps();
        PlanPricingDefaults GetPlanPricingDefaults();
    }

    public class PlanPricingService : IPlanPricingService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IPlanPricingInventoryEngine _PlanPricingInventoryEngine;
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IPricingApiClient _PricingApiClient;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IBackgroundJobClient _BackgroundJobClient;

        public PlanPricingService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                  IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                  IDaypartCache daypartCache,
                                  IBroadcastAudiencesCache audienceCache,
                                  IImpressionAdjustmentEngine impressionAdjustmentEngine,
                                  IImpressionsCalculationEngine impressionsCalculationEngine,
                                  ISpotLengthEngine spotLengthEngine,
                                  IPricingApiClient pricingApiClient,
                                  IBackgroundJobClient backgroundJobClient,
                                  IPlanPricingInventoryEngine planPricingInventoryEngine)
        {
            _BroadcastAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _SpotLengthRepository = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _AudienceCache = audienceCache;
            _SpotLengthEngine = spotLengthEngine;
            _PricingApiClient = pricingApiClient;
            _BackgroundJobClient = backgroundJobClient;
            _PlanPricingInventoryEngine = planPricingInventoryEngine;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        public PlanPricingJob QueuePricingJob(PlanPricingRequestDto planPricingRequestDto, DateTime currentDate)
        {
            var plan = _PlanRepository.GetPlan(planPricingRequestDto.PlanId);

            var job = new PlanPricingJob
            {
                PlanVersionId = plan.VersionId,
                Status = BackgroundJobProcessingStatus.Queued,
                Queued = currentDate
            };

            var jobId = _PlanRepository.AddPlanPricingJob(job);

            job.Id = jobId;

            _PlanRepository.SavePlanPricingParameters(planPricingRequestDto);

            _BackgroundJobClient.Enqueue<IPlanPricingService>(x => x.RunPricingJob(planPricingRequestDto, jobId));

            return job;
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
            // ids are different between environments so must go off the name
            var planPricingSourceNames = new List<string>
            {
                "CNN", 
                "TVB", 
                "Sinclair",
                "LilaMax",
                "ABC O&O",
                "KATZ",
                "NBC O&O"
            };

            var ppDefaults = new PlanPricingDefaults
            {
                UnitCap = 2,
                InventorySources = _InventoryRepository.GetInventorySources().Where(
                        s => planPricingSourceNames.Contains(s.Name))
                    .Select(s => new PlanPricingInventorySourceDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Percentage = 10
                    }).ToList()
            };

            return ppDefaults;
        }

        public PlanPricingResponseDto GetCurrentPricingExecution(int planId)
        {
            var job = _PlanRepository.GetLatestPricingJob(planId);

            return new PlanPricingResponseDto
            {
                Job = job,
                Result = new PlanPricingResultDto
                {
                    Programs = _GetMockPrograms(job)
                }
            };
        }

        private List<PlanPricingProgramDto> _GetMockPrograms(PlanPricingJob job)
        {
            if (job == null || job.Status != BackgroundJobProcessingStatus.Succeeded)
                return null;

            return new List<PlanPricingProgramDto>
            {
                new PlanPricingProgramDto
                {
                    ProgramName = "NFL Sunday Night Football",
                    Genre = "Sports",
                    MarketCount = 72,
                    StationCount = 72,
                    AvgCpm = 13.5m,
                    AvgImpressions = 250,
                    PercentageOfBuy = 2.05
                },
                new PlanPricingProgramDto
                {
                    ProgramName = "Good Morning America",
                    Genre = "News",
                    MarketCount = 14,
                    StationCount = 23,
                    AvgCpm = 18,
                    AvgImpressions = 150,
                    PercentageOfBuy = 97.95
                }
            };
        }

        private PlanPricingApiRequestParametersDto _GetPricingApiRequestParameters(PlanPricingRequestDto planPricingRequestDto, PlanDto plan, List<PlanPricingMarketDto> pricingMarkets)
        {
            var parameters = _MapToApiParametersRequest(planPricingRequestDto);

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

        private PlanPricingApiRequestParametersDto _MapToApiParametersRequest(PlanPricingRequestDto planPricingRequestDto)
        {
            var parameters = new PlanPricingApiRequestParametersDto
            {
                PlanId = planPricingRequestDto.PlanId,
                MinCpm = planPricingRequestDto.MinCpm,
                MaxCpm = planPricingRequestDto.MaxCpm,
                ImpressionsGoal = planPricingRequestDto.ImpressionsGoal,
                BudgetGoal = planPricingRequestDto.BudgetGoal,
                ProprietaryBlend = planPricingRequestDto.ProprietaryBlend,
                CpmGoal = planPricingRequestDto.CpmGoal,
                CompetitionFactor = planPricingRequestDto.CompetitionFactor,
                InflationFactor = planPricingRequestDto.InflationFactor,
                UnitCaps = planPricingRequestDto.UnitCaps,
                UnitCapType = planPricingRequestDto.UnitCapType,
                InventorySourcePercentages = planPricingRequestDto.InventorySourcePercentages
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

        private List<PricingModelSpotsDto> _GetPricingModelSpots(List<ProposalProgramDto> programs)
        {
            var pricingModelSpots = new List<PricingModelSpotsDto>();
            var householdAudienceId = _AudienceCache.GetDefaultAudience().Id;

            foreach (var program in programs)
            {
                foreach (var programWeek in program.FlightWeeks)
                {
                    pricingModelSpots.Add(new PricingModelSpotsDto
                    {
                        Id = program.ManifestId,
                        MediaWeekId = programWeek.MediaWeekId,
                        Impressions = program.ProvidedUnitImpressions ?? 0,
                        Cost = program.SpotCost
                    });
                }
            }

            return pricingModelSpots;
        }

        private List<PricingModelWeekInputDto> _GetPricingModelWeeks(PlanDto plan)
        {
            var pricingModelWeeks = new List<PricingModelWeekInputDto>();

            foreach (var week in plan.WeeklyBreakdownWeeks)
            {
                pricingModelWeeks.Add(new PricingModelWeekInputDto
                {
                    MediaWeekId = week.MediaWeekId,
                    Impressions = week.Impressions
                });
            }

            return pricingModelWeeks;
        }

        public void RunPricingJob(PlanPricingRequestDto planPricingRequestDto, int jobId)
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
                var plan = _PlanRepository.GetPlan(planPricingRequestDto.PlanId);
                planPricingJobDiagnostic.RecordGatherInventoryStart();
                var inventory = _PlanPricingInventoryEngine.GetInventoryForPlan(planPricingRequestDto.PlanId);
                planPricingJobDiagnostic.RecordGatherInventoryEnd();
                var pricingMarkets = _MapToPlanPricingPrograms(plan);
                var parameters = _GetPricingApiRequestParameters(planPricingRequestDto, plan, pricingMarkets);

                var pricingApiRequest = new PlanPricingApiRequestDto
                {
                    Weeks = _GetPricingModelWeeks(plan),
                    Spots = _GetPricingModelSpots(inventory),
                    Parameters = parameters
                };

                planPricingJobDiagnostic.RecordApiCallStart();

                var result = _PricingApiClient.GetPricingCalculationResult(pricingApiRequest);

                planPricingJobDiagnostic.RecordApiCallEnd();

                planPricingJobDiagnostic.RecordEnd();

                using (var transaction = new TransactionScopeWrapper())
                {
                    _PlanRepository.SavePricingRequest(parameters);

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
                    ErrorMessage = exception.Message
                });
            }
        }
    }
}
