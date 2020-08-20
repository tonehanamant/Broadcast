﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.IntegrationTests.Stubs.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices.Plan.PlanBuying
{
    [TestFixture]
    public class PlanBuyingServiceTests
    {
        private readonly IPlanBuyingService _PlanBuyingService;
        private readonly IPlanService _PlanService;
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanBuyingRepository _PlanBuyingRepository;

        public PlanBuyingServiceTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IPlanBuyingApiClient, PlanBuyingApiClientStub>();
            _PlanBuyingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
            _PlanService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();
            _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _PlanBuyingRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanBuyingRepository>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
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
        [UseReporter(typeof(DiffReporter))]
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
        [UseReporter(typeof(DiffReporter))]
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
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        [Ignore("Test returns null because there are no buying parameters set to the plan")]
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
                    UnitCapsType = UnitCapEnum.PerDay,
                    InventorySourcePercentages = new List<PlanInventorySourceDto>
                    {
                        new PlanInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanInventorySourceTypeDto>
                    {
                        new PlanInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Diginet, Percentage = 11 },
                        new PlanInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Syndication, Percentage = 12 }
                    }
                };

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "integration test user");

                var result = _PlanBuyingService.GetBuyingApiRequestPrograms(1197, new BuyingInventoryGetRequestParametersDto());

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
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
        [UseReporter(typeof(DiffReporter))]
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
        [UseReporter(typeof(DiffReporter))]
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
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        [Ignore("There is no inventory loaded for this plan in broadcast_integration db")]
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
                    MarketGroup = MarketGroupEnum.None,
                    InventorySourcePercentages = new List<PlanInventorySourceDto>
                    {
                        new PlanInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanInventorySourceTypeDto>
                    {
                        new PlanInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Diginet, Percentage = 11 },
                        new PlanInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Syndication, Percentage = 12 }
                    }
                };

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                planBuyingRequestDto.Budget = 1200;
                planBuyingRequestDto.UnitCapsType = UnitCapEnum.Per30Min;

                var job2 = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job2.Id, CancellationToken.None);

                var result = _PlanBuyingService.GetCurrentBuyingExecution(1849);

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
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetBuyingInventoryTest()
        {
            var result = _PlanBuyingService.GetBuyingInventory(1197, new BuyingInventoryGetRequestParametersDto());
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
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
                    MarketGroup = MarketGroupEnum.None,
                    InventorySourcePercentages = new List<PlanInventorySourceDto>
                    {
                        new PlanInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanInventorySourceDto{Id = 12, Percentage = 8},
                    }
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
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        [Ignore("There is no inventory loaded for this plan in broadcast_integration db")]
        public void SaveBuyingResultsTest()
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
                    MarketGroup = MarketGroupEnum.None,
                    InventorySourcePercentages = new List<PlanInventorySourceDto>
                    {
                        new PlanInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanInventorySourceTypeDto>
                    {
                        new PlanInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Diginet, Percentage = 11 },
                        new PlanInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Syndication, Percentage = 12 }
                    }
                };

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var result = _PlanBuyingRepository.GetBuyingApiResultsByJobId(job.Id);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingAllocatedSpot), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingAllocationResult), "JobId");
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
        [Ignore("No buying")]
        public void GetBuyingBandsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var bands = _PlanBuyingService.GetBuyingBands(planBuyingRequestDto.PlanId.Value);
                var result = _PlanBuyingService.GetCurrentBuyingExecution(planBuyingRequestDto.PlanId.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingBandsDto), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingBandsDto), "JobId");
                jsonResolver.Ignore(typeof(PlanBuyingBandDetailDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Assert.AreEqual(result.Result.OptimalCpm, bands.Totals.Cpm);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(bands, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        [Ignore("Test returns null because there are no buying parameters set to the plan")]
        public void GetProgramsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var programs = _PlanBuyingService.GetPrograms(planBuyingRequestDto.PlanId.Value);
                var result = _PlanBuyingService.GetCurrentBuyingExecution(planBuyingRequestDto.PlanId.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingProgramProgramDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Assert.AreEqual(result.Result.OptimalCpm, programs.Totals.AvgCpm);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(programs, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        [Ignore("Test returns null because there are no buying parameters set to the plan")]
        public void GetBuyingResultsMarketsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planBuyingRequestDto = _GetBuyingRequestDto();

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var markets = _PlanBuyingService.GetMarkets(planBuyingRequestDto.PlanId.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingBandsDto), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingBandsDto), "JobId");
                jsonResolver.Ignore(typeof(PlanBuyingBandDetailDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        [Ignore("Test returns null because there are no buying parameters set to the plan")]
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

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingBandsDto), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingBandsDto), "JobId");
                jsonResolver.Ignore(typeof(PlanBuyingBandDetailDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        [Ignore("Test returns null because there are no buying parameters set to the plan")]
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

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingBandsDto), "Id");
                jsonResolver.Ignore(typeof(PlanBuyingBandsDto), "JobId");
                jsonResolver.Ignore(typeof(PlanBuyingBandDetailDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(markets, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void SaveBuyingRequestTest()
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
                    MarketGroup = MarketGroupEnum.None,
                    InventorySourcePercentages = new List<PlanInventorySourceDto>
                    {
                        new PlanInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanInventorySourceDto{Id = 6, Percentage = 10},
                        new PlanInventorySourceDto{Id = 7, Percentage = 9},
                        new PlanInventorySourceDto{Id = 10, Percentage = 8},
                        new PlanInventorySourceDto{Id = 11, Percentage = 7},
                        new PlanInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanInventorySourceTypeDto>
                    {
                        new PlanInventorySourceTypeDto { Id = 4, Name = "Syndication", Percentage = 11 },
                        new PlanInventorySourceTypeDto { Id = 5, Name = "Diginet", Percentage = 22 }
                    }
                };

                var job = _PlanBuyingService.QueueBuyingJob(planBuyingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanBuyingService.RunBuyingJob(planBuyingRequestDto, job.Id, CancellationToken.None);

                var result = _PlanBuyingRepository.GetPlanBuyingRuns(planBuyingRequestDto.PlanId.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanBuyingApiRequestParametersDto), "JobId");
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
        [Ignore("Test returns null because there are no buying parameters set to the plan")]
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
                    MarketGroup = MarketGroupEnum.Top25,
                    InventorySourcePercentages = new List<PlanInventorySourceDto>
                    {
                        new PlanInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanInventorySourceDto{Id = 6, Percentage = 10},
                        new PlanInventorySourceDto{Id = 7, Percentage = 9},
                        new PlanInventorySourceDto{Id = 10, Percentage = 8},
                        new PlanInventorySourceDto{Id = 11, Percentage = 7},
                        new PlanInventorySourceDto{Id = 12, Percentage = 8},
                    },
                    InventorySourceTypePercentages = new List<PlanInventorySourceTypeDto>
                    {
                        new PlanInventorySourceTypeDto { Id = 4, Name = "Syndication", Percentage = 11 },
                        new PlanInventorySourceTypeDto { Id = 5, Name = "Diginet", Percentage = 22 }
                    }
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        [Ignore("There is no inventory loaded for this plan in broadcast_integration db")]
        public void WeeklyValuesAreUpdatedForBuyingRequest()
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
                    Budget = 8000,
                    CPM = 5m,
                    DeliveryImpressions = 50000,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapsType = UnitCapEnum.PerDay,
                    MarketGroup = MarketGroupEnum.None
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
                Margin = 20,
                InventorySourcePercentages = new List<PlanInventorySourceDto>
                    {
                        new PlanInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanInventorySourceDto{Id = 12, Percentage = 8},
                    },
                InventorySourceTypePercentages = new List<PlanInventorySourceTypeDto>
                    {
                        new PlanInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Diginet, Percentage = 11 },
                        new PlanInventorySourceTypeDto { Id = (int)InventorySourceTypeEnum.Syndication, Percentage = 12 }
                    }
            };
        }
    }
}