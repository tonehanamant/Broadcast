using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.Stubs.Plan;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices.Plan.PlanBuying
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PlanBuyingServiceTests
    {
        private IPlanBuyingService _PlanBuyingService;
        private IPlanService _PlanService;
        private IPlanRepository _PlanRepository;
        private IPlanBuyingRepository _PlanBuyingRepository;
        private InventoryFileTestHelper _InventoryFileTestHelper;
        private IInventoryProprietarySummaryService _InventoryProprietarySummaryService;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;

        [SetUp]
        public void SetUp()
        {
            _LaunchDarklyClientStub = (LaunchDarklyClientStub)IntegrationTestApplicationServiceFactory.Instance.Resolve<ILaunchDarklyClient>();
            // TODO SDE : this should be reworked for these to be true, as they are in production
            //_LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ALLOW_MULTIPLE_CREATIVE_LENGTHS] = false;
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_PRICING_EFFICIENCY_MODEL] = false;

            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IPlanBuyingApiClient, PlanBuyingApiClientStub>();
            _PlanBuyingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
            _PlanService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();
            _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _PlanBuyingRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanBuyingRepository>();
            _InventoryProprietarySummaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryProprietarySummaryService>();

            _InventoryFileTestHelper = new InventoryFileTestHelper();
        }

        private void _SetFeatureToggle(string feature, bool activate)
        {
            if (_LaunchDarklyClientStub.FeatureToggles.ContainsKey(feature))
                _LaunchDarklyClientStub.FeatureToggles[feature] = activate;
            else
                _LaunchDarklyClientStub.FeatureToggles.Add(feature, activate);
        }

        [Test]
        [Category("short_running")]
        public async void QueueBuyingJobTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = await _PlanBuyingService.QueueBuyingJobAsync(new PlanBuyingParametersDto
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
                _PlanBuyingService.QueueBuyingJobAsync(new PlanBuyingParametersDto
                {
                    PlanId = 1196,
                    Budget = 1000,
                    DeliveryImpressions = 10
                }, new DateTime(2019, 11, 4)
                , "test user");

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1196, PostingTypeEnum.NSI);

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

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1196, PostingTypeEnum.NSI);

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
                    UnitCapsType = UnitCapEnum.Per30Min,
                    HUTBookId = 437,
                    ShareBookId = 437
                };
                BuyingInventoryGetRequestParametersDto parameters = new BuyingInventoryGetRequestParametersDto();
                parameters.HUTBookId = 437;
                parameters.ShareBookId = 437;
                var job = _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "integration test user");

                var result = _PlanBuyingService.GetBuyingApiRequestPrograms_v3(1197, parameters);

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

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1196, PostingTypeEnum.NSI);

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

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1196, PostingTypeEnum.NSI);

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
        public async void RunBuyingJobTwiceOnSamePlanTest()
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
                    UnitCapsType = UnitCapEnum.Per30Min,
                    MarketGroup = MarketGroupEnum.None,
                    PostingType =PostingTypeEnum.NTI,
                    HUTBookId = 437,
                    ShareBookId = 437
                };

                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                planBuyingRequestDto.Budget = 1200;
                planBuyingRequestDto.UnitCapsType = UnitCapEnum.Per30Min;

                var job2 = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job2.Id, CancellationToken.None);

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1197, PostingTypeEnum.NTI);

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
        public async void GetLastestPlanBuyingParametersTest()
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
                    UnitCapsType = UnitCapEnum.PerWeek,
                    CPP = 14.6m,
                    Currency = PlanCurrenciesEnum.Impressions,
                    DeliveryRatingPoints = 1234,
                    MarketGroup = MarketGroupEnum.None
                };

                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                planBuyingRequestDto.UnitCapsType = UnitCapEnum.PerWeek;
                planBuyingRequestDto.Budget = 1200;

                await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

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
        public async void GetBuyingBandsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                var bands = _PlanBuyingService.GetBuyingBands(planBuyingRequestDto.PlanId.Value, PostingTypeEnum.NTI);
                var result = _PlanBuyingService.GetCurrentBuyingExecution(planBuyingRequestDto.PlanId.Value, PostingTypeEnum.NTI);
                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingBandsDto>();
                Assert.AreEqual(result.Result.OptimalCpm, bands.Totals.AvgCpm);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(bands, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public async void GetBuyingStationsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                var stations = _PlanBuyingService.GetStations(planBuyingRequestDto.PlanId.Value, null);
                var result = _PlanBuyingService.GetCurrentBuyingExecution(planBuyingRequestDto.PlanId.Value, PostingTypeEnum.NTI);
                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingStationResultDto>();
                Assert.AreEqual(result.Result.OptimalCpm, stations.Totals.AvgCpm);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stations, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public async void GetBuyingRepFirmsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                var repFirms = _PlanBuyingService.GetBuyingRepFirms(planBuyingRequestDto.PlanId.Value, null);
                var result = _PlanBuyingService.GetCurrentBuyingExecution(planBuyingRequestDto.PlanId.Value, PostingTypeEnum.NTI);
                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultRepFirmDto>();
                Assert.AreEqual(result.Result.OptimalCpm, repFirms.Totals.AvgCpm);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(repFirms, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public async void GetProgramsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                var programs = _PlanBuyingService.GetPrograms(planBuyingRequestDto.PlanId.Value, null);
                var result = _PlanBuyingService.GetCurrentBuyingExecution(planBuyingRequestDto.PlanId.Value, PostingTypeEnum.NTI);

                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultProgramsDto>();
                Assert.AreEqual(result.Result.OptimalCpm, programs.Totals.AvgCpm);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(programs, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public async void GetBuyingResultsMarketsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                var markets = _PlanBuyingService.GetMarkets(planBuyingRequestDto.PlanId.Value, null);
                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultMarketsDto>();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public async void GetBuyingResultsMarketsBuyingExecutedTwiceTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                var secondJob = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, secondJob.Id, CancellationToken.None);

                var markets = _PlanBuyingService.GetMarkets(planBuyingRequestDto.PlanId.Value, null);

                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultMarketsDto>();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public async void GetBuyingResultsMarketsTest_VerifyMargin()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();
                // the GetBuyingResultsMarketsTest test has the Margin included.
                // This test will null out Margin to see that that works
                // and verified by eye that the counts are different between the tests.
                planBuyingRequestDto.Margin = null;

                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                var markets = _PlanBuyingService.GetMarkets(planBuyingRequestDto.PlanId.Value, null);
                JsonSerializerSettings jsonSettings = _GetJsonSettings<PlanBuyingResultMarketsDto>();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public async void GetBuyingResultsOwnershipGroupsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();
                var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

                var markets = _PlanBuyingService.GetBuyingOwnershipGroups(planBuyingRequestDto.PlanId.Value, null);
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
        public async void MarketCoveragesAreSentDividedBy100ToBuyingApiTest()
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
                    UnitCapsType = UnitCapEnum.Per30Min,
                    MarketGroup = MarketGroupEnum.Top25
                };

                var job = await planBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await planBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

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
        public async void WeeklyValuesAreUpdatedForBuyingRequest()
        {
            using (new TransactionScopeWrapper())
            {
                var proprietaryInventorySummaryIds = _CreateProprietaryInventorySummary();

                var apiClient = new PlanBuyingApiClientStub();

                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance(apiClient);

                // New service with injected API client.
                var planBuyingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();

                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = await planBuyingService.QueueBuyingJobAsync(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                await planBuyingService.RunBuyingJobAsync(planBuyingRequestDto, job.Id, CancellationToken.None);

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
                UnitCapsType = UnitCapEnum.Per30Min,
                MarketGroup = MarketGroupEnum.None,
                Margin = 20,
                PostingType = PostingTypeEnum.NTI,
                HUTBookId=437,
                ShareBookId=437
            };
        }

        #region Allocation Model Versions

        private async Task<PlanBuyingJob> _SavePlanAndRunBuyingJobAsync(PlanDto plan)
        {
            var savedDate = new DateTime(2019, 11, 4);
            var planId = _PlanService.SavePlan(plan, "testUser", savedDate, true);
            var savedPlan = _PlanService.GetPlan(planId);

            var planBuyingRequest = new PlanBuyingParametersDto
            {
                PlanId = savedPlan.Id,
                PlanVersionId = savedPlan.VersionId,
                MaxCpm = 100m,
                MinCpm = 1m,
                Budget = 1000,
                CompetitionFactor = 0.1,
                CPM = 5m,
                DeliveryImpressions = 50000,
                InflationFactor = 0.5,
                ProprietaryBlend = 0.2,
                UnitCaps = 10,
                UnitCapsType = UnitCapEnum.Per30Min,
                MarketGroup = MarketGroupEnum.None,
                PostingType = PostingTypeEnum.NSI,
                ShareBookId=437,
                HUTBookId=437               
            };

            var job = await _PlanBuyingService.QueueBuyingJobAsync(planBuyingRequest, savedDate, "test user");

            await _PlanBuyingService.RunBuyingJobAsync(planBuyingRequest, job.Id, CancellationToken.None);

            return job;
        }

        private JsonSerializerSettings _GetJsonSettingsForBuyingResults()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(PlanBuyingAllocationResult), "JobId");
            jsonResolver.Ignore(typeof(PlanBuyingAllocationResult), "PlanVersionId");

            jsonResolver.Ignore(typeof(PlanBuyingStationResultDto), "Id");
            jsonResolver.Ignore(typeof(PlanBuyingStationResultDto), "JobId");
            jsonResolver.Ignore(typeof(PlanBuyingStationResultDto), "PlanVersionId");
            jsonResolver.Ignore(typeof(PlanBuyingStationResultDto), "BuyingJobId");
            jsonResolver.Ignore(typeof(PlanBuyingStationDto), "Id");

            jsonResolver.Ignore(typeof(PlanBuyingBandsDto), "BuyingJobId");

            jsonResolver.Ignore(typeof(PlanBuyingResultOwnershipGroupDto), "PlanVersionId");
            jsonResolver.Ignore(typeof(PlanBuyingResultOwnershipGroupDto), "BuyingJobId");

            jsonResolver.Ignore(typeof(PlanBuyingResultRepFirmDto), "PlanVersionId");
            jsonResolver.Ignore(typeof(PlanBuyingResultRepFirmDto), "BuyingJobId");

            jsonResolver.Ignore(typeof(PlanBuyingResultMarketsDto), "PlanVersionId");
            jsonResolver.Ignore(typeof(PlanBuyingResultMarketsDto), "BuyingJobId");

            jsonResolver.Ignore(typeof(PlanBuyingAllocatedSpot), "Id");

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
        public async void SaveBuyingResultsTest()
        {
            string resultsToVerify;
            var jsonSettings = _GetJsonSettingsForBuyingResults();
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRun();

            using (new TransactionScopeWrapper())
            {
                var job = await _SavePlanAndRunBuyingJobAsync(plan);

                var apiResult = _PlanBuyingRepository.GetBuyingApiResultsByJobId(job.Id, SpotAllocationModelMode.Efficiency, PostingTypeEnum.NSI);
                var executionResult = _PlanBuyingService.GetCurrentBuyingExecutionByJobId(job.Id);
                Assert.AreEqual(SpotAllocationModelMode.Efficiency, executionResult.Result.SpotAllocationModelMode);

                resultsToVerify = IntegrationTestHelper.ConvertToJson(apiResult, jsonSettings);
            }

            Approvals.Verify(resultsToVerify);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async void SavePricingResultsTestWithMultiLengthAndEfficiency()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_PRICING_EFFICIENCY_MODEL, true);

            string resultsToVerify;
            var jsonSettings = _GetJsonSettingsForBuyingResults();
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            using (new TransactionScopeWrapper())
            {
                var job = await _SavePlanAndRunBuyingJobAsync(plan);

                var resultDefault = _PlanBuyingRepository.GetBuyingApiResultsByJobId(job.Id, SpotAllocationModelMode.Efficiency, PostingTypeEnum.NSI);
                var resultE = _PlanBuyingRepository.GetBuyingApiResultsByJobId(job.Id, SpotAllocationModelMode.Efficiency, PostingTypeEnum.NSI);
                var resultF = _PlanBuyingRepository.GetBuyingApiResultsByJobId(job.Id, SpotAllocationModelMode.Floor, PostingTypeEnum.NSI);

                var result = new
                {
                    Default = resultDefault,
                    Efficiency = resultE,
                    Floor = resultF
                };

                resultsToVerify = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
            }

            Approvals.Verify(resultsToVerify);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetBandsWithEfficiencyModelEnabled()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_PRICING_EFFICIENCY_MODEL, true);

            string resultsToVerify;
            var jsonSettings = _GetJsonSettingsForBuyingResults();
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            using (new TransactionScopeWrapper())
            {
                var job = _SavePlanAndRunBuyingJobAsync(plan);

                var resultDefault = _PlanBuyingService.GetBuyingBands(plan.Id, null);
                var resultE = _PlanBuyingService.GetBuyingBands(plan.Id, null, SpotAllocationModelMode.Efficiency);
                var resultF = _PlanBuyingService.GetBuyingBands(plan.Id, null, SpotAllocationModelMode.Floor);

                var result = new
                {
                    Default = resultDefault,
                    Efficiency = resultE,
                    Floor = resultF
                };

                resultsToVerify = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
            }

            Approvals.Verify(resultsToVerify);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetStationsWithEfficiencyModelEnabled()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_PRICING_EFFICIENCY_MODEL, true);

            string resultsToVerify;
            var jsonSettings = _GetJsonSettingsForBuyingResults();
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            using (new TransactionScopeWrapper())
            {
                var job = _SavePlanAndRunBuyingJobAsync(plan);

                var resultDefault = _PlanBuyingService.GetStations(plan.Id, null);
                var resultE = _PlanBuyingService.GetStations(plan.Id, null, SpotAllocationModelMode.Efficiency);
                var resultF = _PlanBuyingService.GetStations(plan.Id, null, SpotAllocationModelMode.Floor);

                var result = new
                {
                    Default = resultDefault,
                    Efficiency = resultE,
                    Floor = resultF
                };

                resultsToVerify = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
            }

            Approvals.Verify(resultsToVerify);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetMarketsWithEfficiencyModelEnabled()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_PRICING_EFFICIENCY_MODEL, true);

            string resultsToVerify;
            var jsonSettings = _GetJsonSettingsForBuyingResults();
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            using (new TransactionScopeWrapper())
            {
                var job = _SavePlanAndRunBuyingJobAsync(plan);

                var resultDefault = _PlanBuyingService.GetMarkets(plan.Id, null);
                var resultE = _PlanBuyingService.GetMarkets(plan.Id, null, SpotAllocationModelMode.Efficiency);
                var resultF = _PlanBuyingService.GetMarkets(plan.Id, null, SpotAllocationModelMode.Floor);

                var result = new
                {
                    Default = resultDefault,
                    Efficiency = resultE,
                    Floor = resultF
                };

                resultsToVerify = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
            }

            Approvals.Verify(resultsToVerify);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetProgramsWithEfficiencyModelEnabled()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_PRICING_EFFICIENCY_MODEL, true);

            string resultsToVerify;
            var jsonSettings = _GetJsonSettingsForBuyingResults();
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            using (new TransactionScopeWrapper())
            {
                var job = _SavePlanAndRunBuyingJobAsync(plan);

                var resultDefault = _PlanBuyingService.GetPrograms(plan.Id, null);
                var resultE = _PlanBuyingService.GetPrograms(plan.Id, null, SpotAllocationModelMode.Efficiency);
                var resultF = _PlanBuyingService.GetPrograms(plan.Id, null, SpotAllocationModelMode.Floor);

                var result = new
                {
                    Default = resultDefault,
                    Efficiency = resultE,
                    Floor = resultF
                };

                resultsToVerify = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
            }

            Approvals.Verify(resultsToVerify);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetBuyingOwnershipGroupsWithEfficiencyModelEnabled()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_PRICING_EFFICIENCY_MODEL, true);

            string resultsToVerify;
            var jsonSettings = _GetJsonSettingsForBuyingResults();
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            using (new TransactionScopeWrapper())
            {
                var job = _SavePlanAndRunBuyingJobAsync(plan);

                var resultDefault = _PlanBuyingService.GetBuyingOwnershipGroups(plan.Id, null);
                var resultE = _PlanBuyingService.GetBuyingOwnershipGroups(plan.Id, null, SpotAllocationModelMode.Efficiency);
                var resultF = _PlanBuyingService.GetBuyingOwnershipGroups(plan.Id, null, SpotAllocationModelMode.Floor);

                var result = new
                {
                    Default = resultDefault,
                    Efficiency = resultE,
                    Floor = resultF
                };

                resultsToVerify = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
            }

            Approvals.Verify(resultsToVerify);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetBuyingRepFirmsWithEfficiencyModelEnabled()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_PRICING_EFFICIENCY_MODEL, true);

            string resultsToVerify;
            var jsonSettings = _GetJsonSettingsForBuyingResults();
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            using (new TransactionScopeWrapper())
            {
                var job = _SavePlanAndRunBuyingJobAsync(plan);

                var resultDefault = _PlanBuyingService.GetBuyingRepFirms(plan.Id, null);
                var resultE = _PlanBuyingService.GetBuyingRepFirms(plan.Id, null, SpotAllocationModelMode.Efficiency);
                var resultF = _PlanBuyingService.GetBuyingRepFirms(plan.Id, null, SpotAllocationModelMode.Floor);

                var result = new
                {
                    Default = resultDefault,
                    Efficiency = resultE,
                    Floor = resultF
                };

                resultsToVerify = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
            }

            Approvals.Verify(resultsToVerify);
        }
        #endregion // #region Allocation Model Versions
    }
}
