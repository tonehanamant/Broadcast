using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.IntegrationTests.Stubs.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices.Plan.PlanBuying
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PlanBuyingServiceTests
    {
        private readonly IPlanBuyingService _PlanBuyingService;
        private readonly IPlanService _PlanService;
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanBuyingRepository _PlanBuyingRepository;
        private InventoryFileTestHelper _InventoryFileTestHelper;
        private readonly IInventoryProprietarySummaryService _InventoryProprietarySummaryService;

        public PlanBuyingServiceTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IPlanBuyingApiClient, PlanBuyingApiClientStub>();
            _PlanBuyingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
            _PlanService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();
            _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _PlanBuyingRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanBuyingRepository>();
            _InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryProprietarySummaryService>();
        }

        [SetUp]
        public void SetUp()
        {
            _InventoryFileTestHelper = new InventoryFileTestHelper();
        }

        [Test]
        [Category("short_running")]
        public void QueueBuyingJobTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanBuyingService.QueueBuyingJob(new PlanBuyingParametersDto
                {
                    PlanId = 1196,
                    Margin = 20,
                    Budget = 1000,
                    DeliveryImpressions = 10
                }, new DateTime(2019, 11, 4)
                , "test user");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingJob), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingJob), "HangfireJobId");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Category("short_running")]
        public void GetCurrentBuyingExecutionTest()
        {
            using (new TransactionScopeWrapper())
            {
                _PlanBuyingService.QueueBuyingJob(new PlanBuyingParametersDto
                {
                    PlanId = 1196,
                    Budget = 1000,
                    DeliveryImpressions = 10
                }, new DateTime(2019, 11, 4)
                , "test user");

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1196);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingJob), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingJob), "HangfireJobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Category("short_running")]
        public void GetCurrentProcessingBuyingExecution_WithCompletedJobs()
        {
            var currentDate = new DateTime(2019, 11, 4);

            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1196);

                _PopulateSomeBuyingJobs(plan, currentDate);

                _PlanBuyingRepository.AddPlanBuyingJob(new PlanBuyingJob
                {
                    PlanVersionId = plan.VersionId,
                    Status = BackgroundJobProcessingStatus.Processing,
                    Queued = currentDate.AddSeconds(6),
                });

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1196);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingJob), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetBuyingRequestTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = new PlanBuyingParametersDto
                {
                    PlanId = 1197,
                    MaxCpm = 100m,
                    MinCpm = 1m,
                    Budget = 1000,
                    CompetitionFactor = 0.1,
                    CPM = 5m,
                    DeliveryImpressions = 50000,
                    InflationFactor = 0.5,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapsType = UnitCapEnum.PerDay
                };

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "integration test user");

                var result = _PlanBuyingService.GetBuyingApiRequestPrograms(1197, new BuyingInventoryGetRequestParametersDto());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("short_running")]
        public void GetLatestProcessingBuyingExecution_WithCanceledJobs()
        {
            var currentDate = new DateTime(2019, 11, 4);

            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1196);

                _PopulateSomeBuyingJobs(plan, currentDate);

                _PlanBuyingRepository.AddPlanBuyingJob(new PlanBuyingJob
                {
                    PlanVersionId = plan.VersionId,
                    Status = BackgroundJobProcessingStatus.Canceled,
                    Queued = currentDate.AddSeconds(6),
                });

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1196);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingJob), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Category("short_running")]
        public void GetCurrentQueuedBuyingExecution_WithCompletedJobs()
        {
            var currentDate = new DateTime(2019, 11, 4);

            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1196);

                _PopulateSomeBuyingJobs(plan, currentDate);

                _PlanBuyingRepository.AddPlanBuyingJob(new PlanBuyingJob
                {
                    PlanVersionId = plan.VersionId,
                    Status = BackgroundJobProcessingStatus.Queued,
                    Queued = currentDate.AddSeconds(6)
                });

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1196);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingJob), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        private void _PopulateSomeBuyingJobs(PlanDto plan, DateTime currentDate)
        {
            _PlanBuyingRepository.AddPlanBuyingJob(new PlanBuyingJob
            {
                PlanVersionId = plan.VersionId,
                Status = BackgroundJobProcessingStatus.Succeeded,
                Queued = currentDate,
                Completed = currentDate.AddSeconds(2)
            });

            _PlanBuyingRepository.AddPlanBuyingJob(new PlanBuyingJob
            {
                PlanVersionId = plan.VersionId,
                Status = BackgroundJobProcessingStatus.Failed,
                Queued = currentDate.AddSeconds(2),
                Completed = currentDate.AddSeconds(3)
            });

            _PlanBuyingRepository.AddPlanBuyingJob(new PlanBuyingJob
            {
                PlanVersionId = plan.VersionId,
                Status = BackgroundJobProcessingStatus.Succeeded,
                Queued = currentDate.AddSeconds(4),
                Completed = currentDate.AddSeconds(5)
            });
        }

        [Test]
        [Category("short_running")]
        public void GetPlanBuyingRunsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanBuyingService.GetPlanBuyingRuns(1196);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("long_running")]
        public void RunBuyingJobTwiceOnSamePlanTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = new PlanBuyingParametersDto
                {
                    PlanId = 1197,
                    PlanVersionId = 47,
                    MaxCpm = 100m,
                    MinCpm = 1m,
                    Budget = 1000,
                    CompetitionFactor = 0.1,
                    CPM = 5m,
                    DeliveryImpressions = 50000,
                    InflationFactor = 0.5,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapsType = UnitCapEnum.PerDay,
                    MarketGroup = MarketGroupEnum.None
                };

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                planBuyingRequestDto.Budget = 1200;
                planBuyingRequestDto.UnitCapsType = UnitCapEnum.Per30Min;

                var job2 = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job2.Id, CancellationToken.None);

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1197);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingJob), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingJob), "HangfireJobId");
                jsonResolver.Ignore(typeof(PlanBuyingProgramDto), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingJob), "Completed");
                jsonResolver.Ignore(typeof(PlanBuyingJob), "DiagnosticResult");
                jsonResolver.Ignore(typeof(PlanBuyingResultBaseDto), "JobId");
                jsonResolver.Ignore(typeof(CurrentBuyingExecutionResultDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetBuyingInventoryTest()
        {
            var result = _PlanBuyingService.GetBuyingInventory(1197, new BuyingInventoryGetRequestParametersDto());
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [Category("long_running")]
        public void GetLastestPlanBuyingParametersTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = new PlanBuyingParametersDto
                {
                    PlanId = 1849,
                    PlanVersionId = 7236,
                    MaxCpm = 35m,
                    MinCpm = 1m,
                    Budget = 500000,
                    CompetitionFactor = 0.1,
                    CPM = 25m,
                    DeliveryImpressions = 20000,
                    InflationFactor = 0.5,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapsType = UnitCapEnum.PerDay,
                    CPP = 14.6m,
                    Currency = PlanCurrenciesEnum.Impressions,
                    DeliveryRatingPoints = 1234,
                    MarketGroup = MarketGroupEnum.None
                };

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                planBuyingRequestDto.UnitCapsType = UnitCapEnum.PerWeek;
                planBuyingRequestDto.Budget = 1200;

                _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                var result = _PlanService.GetPlan(1849);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingParametersDto), "JobId");
                jsonResolver.Ignore(typeof(PlanDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetBuyingBandsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var bands = _PlanBuyingService.GetBuyingBands(planBuyingRequestDto.PlanId.Value);
                var result = _PlanBuyingService.GetCurrentBuyingExecution(planBuyingRequestDto.PlanId.Value);
                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingBandsDto>();
                Assert.AreEqual(result.Result.OptimalCpm, bands.Totals.AvgCpm);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(bands, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetProgramsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var programs = _PlanBuyingService.GetPrograms(planBuyingRequestDto.PlanId.Value);
                var result = _PlanBuyingService.GetCurrentBuyingExecution(planBuyingRequestDto.PlanId.Value);

                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultProgramsDto>();
                Assert.AreEqual(result.Result.OptimalCpm, programs.Totals.AvgCpm);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(programs, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetBuyingResultsMarketsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var markets = _PlanBuyingService.GetMarkets(planBuyingRequestDto.PlanId.Value);
                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultMarketsDto>();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetBuyingResultsMarketsBuyingExecutedTwiceTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var secondJob = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, secondJob.Id, CancellationToken.None);

                var markets = _PlanBuyingService.GetMarkets(planBuyingRequestDto.PlanId.Value);

                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultMarketsDto>();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetBuyingResultsMarketsTest_VerifyMargin()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();
                // the GetBuyingResultsMarketsTest test has the Margin included.
                // This test will null out Margin to see that that works
                // and verified by eye that the counts are different between the tests.
                planBuyingRequestDto.Margin = null;

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var markets = _PlanBuyingService.GetMarkets(planBuyingRequestDto.PlanId.Value);
                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultMarketsDto>();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void GetBuyingResultsOwnershipGroupsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();
                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var markets = _PlanBuyingService.GetBuyingOwnershipGroups(planBuyingRequestDto.PlanId.Value);
                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultOwnershipGroupDto>();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        private static JsonSerializerSettings _GetJsonSettings<T>()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(T), "Id");
            jsonResolver.Ignore(typeof(T), "BuyingJobId");
            jsonResolver.Ignore(typeof(T), "PlanVersionId");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            return jsonSettings;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void MarketCoveragesAreSentDividedBy100ToBuyingApiTest()
        {
            using (new TransactionScopeWrapper())
            {
                var apiClient = new PlanBuyingApiClientStub();

                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance(apiClient);

                // New service with injected API client.
                var planBuyingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();

                var planBuyingRequestDto = new PlanBuyingParametersDto
                {
                    PlanId = 1197,
                    PlanVersionId = 47,
                    MaxCpm = 100m,
                    MinCpm = 1m,
                    Budget = 1000,
                    CompetitionFactor = 0.1,
                    CPM = 5m,
                    DeliveryImpressions = 50000,
                    InflationFactor = 0.5,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapsType = UnitCapEnum.PerDay,
                    MarketGroup = MarketGroupEnum.Top25
                };

                var job = planBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                planBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var sentParameters = apiClient.LastSentRequest;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingApiRequestParametersDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(sentParameters, jsonSettings));
            }
        }

        private List<int> _CreateProprietaryInventorySummary()
        {
            const int inventorySourceId = 5;
            var startDate = new DateTime(2018, 1, 1);
            var endDate = new DateTime(2018, 3, 20);

            _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_CNN.xlsx", processInventoryRatings: true);

            _InventoryProprietarySummaryService.AggregateInventoryProprietarySummary(
                inventorySourceId, 
                startDate,
                endDate);

            var planDaypartRequests = new List<PlanDaypartRequest>
                {
                    new PlanDaypartRequest
                    {
                        DefaultDayPartId = 1,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    },
                    new PlanDaypartRequest
                    {
                        DefaultDayPartId = 4,
                        StartTimeSeconds = 14400,
                        EndTimeSeconds = 36000
                    }
                };

            var request = new InventoryProprietarySummaryRequest
            {
                FlightStartDate = startDate,
                FlightEndDate = endDate,
                PlanDaypartRequests = planDaypartRequests,
                AudienceId = 5,
                SpotLengthIds = new List<int> { 1 },
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek() }
            };

            var result = _InventoryProprietarySummaryService.GetInventoryProprietarySummaries(request);

            return result.summaries.Select(x => x.Id).ToList();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void WeeklyValuesAreUpdatedForBuyingRequest()
        {
            using (new TransactionScopeWrapper())
            {
                var proprietaryInventorySummaryIds = _CreateProprietaryInventorySummary();

                var apiClient = new PlanBuyingApiClientStub();

                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance(apiClient);

                // New service with injected API client.
                var planBuyingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();

                var planBuyingRequestDto = new PlanBuyingParametersDto
                {
                    PlanId = 1197,
                    PlanVersionId = 47,
                    Budget = 8000,
                    CPM = 5m,
                    DeliveryImpressions = 50000,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapsType = UnitCapEnum.PerDay,
                    MarketGroup = MarketGroupEnum.Top100,
                    ProprietaryInventory = proprietaryInventorySummaryIds.Select(x => new InventoryProprietarySummary { Id = x }).ToList()
                };

                var job = planBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                planBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var request = apiClient.LastSentRequest;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingApiRequestParametersDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(request, jsonSettings));
            }
        }

        private static PlanBuyingParametersDto _GetBuyingRequestDto()
        {
            return new PlanBuyingParametersDto
            {
                PlanId = 1197,
                PlanVersionId = 47,
                MaxCpm = 100m,
                MinCpm = 1m,
                Budget = 1000,
                CompetitionFactor = 0.1,
                CPM = 5m,
                DeliveryImpressions = 50000,
                InflationFactor = 0.5,
                ProprietaryBlend = 0.2,
                UnitCaps = 10,
                UnitCapsType = UnitCapEnum.PerDay,
                MarketGroup = MarketGroupEnum.None,
                Margin = 20
            };
        }
    }
}
