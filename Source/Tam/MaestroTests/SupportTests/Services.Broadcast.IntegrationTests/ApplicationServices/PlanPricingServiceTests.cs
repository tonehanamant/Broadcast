using ApprovalTests;
using ApprovalTests.Reporters;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanPricingServiceTests
    {
        private readonly IPlanPricingService _PlanPricingService;
        private readonly IPlanService _PlanService;
        private readonly IPlanRepository _PlanRepository;

        public PlanPricingServiceTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IPricingApiClient, PricingApiClientStub>();
            _PlanPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
            _PlanService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();
            _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void QueuePricingJobTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingService.QueuePricingJob(new PlanPricingParametersDto
                {
                    PlanId = 1196,
                    Margin = 20,
                    Budget = 1000,
                    DeliveryImpressions = 10
                }, new DateTime(2019, 11, 4)
                , "test user");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "HangfireJobId");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetCurrentPricingExecutionTest()
        {
            using (new TransactionScopeWrapper())
            {
                _PlanPricingService.QueuePricingJob(new PlanPricingParametersDto
                {
                    PlanId = 1196,
                    Budget = 1000,
                    DeliveryImpressions = 10
                }, new DateTime(2019, 11, 4)
                , "test user");

                var result = _PlanPricingService.GetCurrentPricingExecution(1196);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "HangfireJobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetCurrentPricingExecutionFailedTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingService.QueuePricingJob(new PlanPricingParametersDto
                {
                    PlanId = 1196,
                    Budget = 1000,
                    DeliveryImpressions = 10
                }, new DateTime(2020, 3, 3)
                , "test user");

                _PlanPricingService.ForceCompletePlanPricingJob(result.Id, "Test User");

                var problems = new Exception();
                var resultPayload = new CurrentPricingExecution();

                try
                {
                    resultPayload = _PlanPricingService.GetCurrentPricingExecution(1196);
                }
                catch (Exception e)
                {
                    problems = e;
                }

                Assert.IsNotEmpty(problems.Message);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetCurrentProcessingPricingExecution_WithCompletedJobs()
        {
            var currentDate = new DateTime(2019, 11, 4);

            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1196);

                _PopulateSomePricingJobs(plan, currentDate);

                _PlanRepository.AddPlanPricingJob(new PlanPricingJob
                {
                    PlanVersionId = plan.VersionId,
                    Status = BackgroundJobProcessingStatus.Processing,
                    Queued = currentDate.AddSeconds(6),
                });

                var result = _PlanPricingService.GetCurrentPricingExecution(1196);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPricingRequestTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = new PlanPricingParametersDto
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
                    UnitCapsType = UnitCapEnum.PerDay,
                    InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                    {
                        new PlanPricingInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanPricingInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanPricingInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanPricingInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanPricingInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanPricingInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanPricingInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                    {
                        new PlanPricingInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Diginet, Percentage = 11 },
                        new PlanPricingInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Syndication, Percentage = 12 }
                    }
                };

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "integration test user");

                var result = _PlanPricingService.GetPricingApiRequestPrograms(1197, new PricingInventoryGetRequestParametersDto());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetLatestProcessingPricingExecution_WithCanceledJobs()
        {
            var currentDate = new DateTime(2019, 11, 4);

            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1196);

                _PopulateSomePricingJobs(plan, currentDate);

                _PlanRepository.AddPlanPricingJob(new PlanPricingJob
                {
                    PlanVersionId = plan.VersionId,
                    Status = BackgroundJobProcessingStatus.Canceled,
                    Queued = currentDate.AddSeconds(6),
                });

                var result = _PlanPricingService.GetCurrentPricingExecution(1196);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetCurrentQueuedPricingExecution_WithCompletedJobs()
        {
            var currentDate = new DateTime(2019, 11, 4);

            using (new TransactionScopeWrapper())
            {
                var plan = _PlanRepository.GetPlan(1196);

                _PopulateSomePricingJobs(plan, currentDate);

                _PlanRepository.AddPlanPricingJob(new PlanPricingJob
                {
                    PlanVersionId = plan.VersionId,
                    Status = BackgroundJobProcessingStatus.Queued,
                    Queued = currentDate.AddSeconds(6)
                });

                var result = _PlanPricingService.GetCurrentPricingExecution(1196);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        private void _PopulateSomePricingJobs(PlanDto plan, DateTime currentDate)
        {
            _PlanRepository.AddPlanPricingJob(new PlanPricingJob
            {
                PlanVersionId = plan.VersionId,
                Status = BackgroundJobProcessingStatus.Succeeded,
                Queued = currentDate,
                Completed = currentDate.AddSeconds(2)
            });

            _PlanRepository.AddPlanPricingJob(new PlanPricingJob
            {
                PlanVersionId = plan.VersionId,
                Status = BackgroundJobProcessingStatus.Failed,
                Queued = currentDate.AddSeconds(2),
                Completed = currentDate.AddSeconds(3)
            });

            _PlanRepository.AddPlanPricingJob(new PlanPricingJob
            {
                PlanVersionId = plan.VersionId,
                Status = BackgroundJobProcessingStatus.Succeeded,
                Queued = currentDate.AddSeconds(4),
                Completed = currentDate.AddSeconds(5)
            });
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetPlanPricingRunsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingService.GetPlanPricingRuns(1196);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        [Ignore("There is no inventory loaded for this plan in broadcast_integration db")]
        public void RunPricingJobTwiceOnSamePlanTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = new PlanPricingParametersDto
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
                    MarketGroup = PricingMarketGroupEnum.None,
                    InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                    {
                        new PlanPricingInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanPricingInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanPricingInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanPricingInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanPricingInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanPricingInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanPricingInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                    {
                        new PlanPricingInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Diginet, Percentage = 11 },
                        new PlanPricingInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Syndication, Percentage = 12 }
                    }
                };

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                planPricingRequestDto.Budget = 1200;
                planPricingRequestDto.UnitCapsType = UnitCapEnum.Per30Min;

                var job2 = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job2.Id, CancellationToken.None);

                var result = _PlanPricingService.GetCurrentPricingExecution(1849);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "HangfireJobId");
                jsonResolver.Ignore(typeof(PlanPricingProgramDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "Completed");
                jsonResolver.Ignore(typeof(PlanPricingJob), "DiagnosticResult");
                jsonResolver.Ignore(typeof(PlanPricingResultBaseDto), "JobId");
                jsonResolver.Ignore(typeof(CurrentPricingExecutionResultDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPricingInventoryTest()
        {
            var result = _PlanPricingService.GetPricingInventory(1197, new PricingInventoryGetRequestParametersDto());
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetLastestPlanPricingParametersTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = new PlanPricingParametersDto
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
                    MarketGroup = PricingMarketGroupEnum.None,
                    InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                    {
                        new PlanPricingInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanPricingInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanPricingInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanPricingInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanPricingInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanPricingInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanPricingInventorySourceDto{Id = 12, Percentage = 8},
                    }
                };

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                planPricingRequestDto.UnitCapsType = UnitCapEnum.PerWeek;
                planPricingRequestDto.Budget = 1200;

                _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                var result = _PlanService.GetPlan(1849);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingParametersDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void SavePricingResultsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = new PlanPricingParametersDto
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
                    MarketGroup = PricingMarketGroupEnum.None,
                    InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                    {
                        new PlanPricingInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanPricingInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanPricingInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanPricingInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanPricingInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanPricingInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanPricingInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                    {
                        new PlanPricingInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Diginet, Percentage = 11 },
                        new PlanPricingInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Syndication, Percentage = 12 }
                    }
                };

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var result = _PlanRepository.GetPricingApiResults(planPricingRequestDto.PlanId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingAllocatedSpot), "Id");
                jsonResolver.Ignore(typeof(PlanPricingAllocationResult), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPricingBandsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = _GetPricingRequestDto();

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var result = _PlanPricingService.GetPricingBands(planPricingRequestDto.PlanId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        private static PlanPricingParametersDto _GetPricingRequestDto()
        {
            return new PlanPricingParametersDto
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
                MarketGroup = PricingMarketGroupEnum.None,
                InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                    {
                        new PlanPricingInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanPricingInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanPricingInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanPricingInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanPricingInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanPricingInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanPricingInventorySourceDto{Id = 12, Percentage = 8},
                    },
                InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                    {
                        new PlanPricingInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Diginet, Percentage = 11 },
                        new PlanPricingInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Syndication, Percentage = 12 }
                    }
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void SavePricingRequestTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = new PlanPricingParametersDto
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
                    MarketGroup = PricingMarketGroupEnum.None,
                    InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                    {
                        new PlanPricingInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanPricingInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanPricingInventorySourceDto{Id = 6, Percentage = 10},
                        new PlanPricingInventorySourceDto{Id = 7, Percentage = 9},
                        new PlanPricingInventorySourceDto{Id = 10, Percentage = 8},
                        new PlanPricingInventorySourceDto{Id = 11, Percentage = 7},
                        new PlanPricingInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                    {
                        new PlanPricingInventorySourceTypeDto { Id = 4, Name = "Syndication", Percentage = 11 },
                        new PlanPricingInventorySourceTypeDto { Id = 5, Name = "Diginet", Percentage = 22 }
                    }
                };

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var result = _PlanRepository.GetPlanPricingRuns(planPricingRequestDto.PlanId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingApiRequestParametersDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void MarketCoveragesAreSentDividedBy100ToPricingApiTest()
        {
            using (new TransactionScopeWrapper())
            {
                var apiClient = new PricingApiClientStub();

                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance(apiClient);

                // New service with injected API client.
                var planPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();

                var planPricingRequestDto = new PlanPricingParametersDto
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
                    MarketGroup = PricingMarketGroupEnum.Top25,
                    InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                    {
                        new PlanPricingInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanPricingInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanPricingInventorySourceDto{Id = 6, Percentage = 10},
                        new PlanPricingInventorySourceDto{Id = 7, Percentage = 9},
                        new PlanPricingInventorySourceDto{Id = 10, Percentage = 8},
                        new PlanPricingInventorySourceDto{Id = 11, Percentage = 7},
                        new PlanPricingInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                    {
                        new PlanPricingInventorySourceTypeDto { Id = 4, Name = "Syndication", Percentage = 11 },
                        new PlanPricingInventorySourceTypeDto { Id = 5, Name = "Diginet", Percentage = 22 }
                    }
                };

                var job = planPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                planPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var sentParameters = apiClient.LastSentRequest;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingApiRequestParametersDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(sentParameters, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void WeeklyValuesAreUpdatedForPricingRequest()
        {
            using (new TransactionScopeWrapper())
            {
                var apiClient = new PricingApiClientStub();

                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance(apiClient);

                // New service with injected API client.
                var planPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();

                var planPricingRequestDto = new PlanPricingParametersDto
                {
                    PlanId = 1197,
                    PlanVersionId = 47,
                    Budget = 8000,
                    CPM = 5m,
                    DeliveryImpressions = 50000,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapsType = UnitCapEnum.PerDay,
                    MarketGroup = PricingMarketGroupEnum.None
                };

                var job = planPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                planPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var request = apiClient.LastSentRequest;

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingApiRequestParametersDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(request, jsonSettings));
            }
        }
    }
}
