using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Services.Broadcast.Entities;
using Tam.Maestro.Common.DataLayer;
using System.Threading;

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
                    Margin = 20
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
        public void GetCurrentPricingExecutionNoJobQueuedTest()
        {
            using (new TransactionScopeWrapper())
            {
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
        public void GetCurrentPricingExecutionTest()
        {
            using (new TransactionScopeWrapper())
            {
                _PlanPricingService.QueuePricingJob(new PlanPricingParametersDto
                {
                    PlanId = 1196
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
                    PlanId = 1196
                }, new DateTime(2020, 3, 3)
                , "test user");

                _PlanPricingService.ForceCompletePlanPricingJob(result.Id, "Test User");

                Exception problems = new Exception();
                PlanPricingResponseDto resultPayload = new PlanPricingResponseDto();

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
        public void RunPricingJobTest()
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

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var result = _PlanPricingService.GetCurrentPricingExecution(1197);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "HangfireJobId");
                jsonResolver.Ignore(typeof(PlanPricingProgramDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "Completed");
                jsonResolver.Ignore(typeof(PlanPricingJob), "DiagnosticResult");
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
        public void RunPricingJobTest_CalculateAdjustedBudgetAndCPM()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = new PlanPricingParametersDto
                {
                    PlanId = 1196,
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
                    Margin= 20,
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

                var result = _PlanPricingService.GetCurrentPricingExecution(1197);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                jsonResolver.Ignore(typeof(PlanPricingProgramDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "Completed");
                jsonResolver.Ignore(typeof(PlanPricingJob), "DiagnosticResult");
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
        public void GetUnitCaps()
        {
            var unitCaps = _PlanPricingService.GetUnitCaps();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(unitCaps));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetPlanPricingDefaults()
        {
            var ppDefaults = _PlanPricingService.GetPlanPricingDefaults();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(ppDefaults));
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
        [Category("short_running")]
        public void GetPlanPricingParametersTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = new PlanPricingParametersDto
                {
                    PlanId = 1197,
                    MaxCpm = 10m,
                    MinCpm = 1m,
                    Budget = 1000,
                    CompetitionFactor = 0.1,
                    CPM = 5m,
                    DeliveryImpressions = 50000,
                    InflationFactor = 0.5,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapsType = UnitCapEnum.PerDay,
                    CPP = 1000,
                    Currency = PlanCurrenciesEnum.GRP,
                    DeliveryRatingPoints = 1234,
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

                var result = _PlanService.GetPlan(1197);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
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

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var result = _PlanRepository.GetPricingApiResults(planPricingRequestDto.PlanId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingAllocatedSpot), "Id");
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
        public void GetPricingAggregatedResultsTest()
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

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var result = _PlanPricingService.GetCurrentPricingExecution(planPricingRequestDto.PlanId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingApiResultSpotDto), "id");
                jsonResolver.Ignore(typeof(PlanPricingProgramDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "HangfireJobId");
                jsonResolver.Ignore(typeof(PlanPricingJob), "Completed");
                jsonResolver.Ignore(typeof(PlanPricingJob), "DiagnosticResult");
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
        public void GetPricingAggregatedResultsCalculatePricingCpmTest()
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
                    Margin = 20,
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

                var result = _PlanPricingService.GetCurrentPricingExecution(planPricingRequestDto.PlanId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingApiResultSpotDto), "id");
                jsonResolver.Ignore(typeof(PlanPricingProgramDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "HangfireJobId");
                jsonResolver.Ignore(typeof(PlanPricingJob), "Completed");
                jsonResolver.Ignore(typeof(PlanPricingJob), "DiagnosticResult");
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
        public void SavePricingRequestTest()
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

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void RunPricing_ModelReturnsErrors()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IPricingApiClient, PricingApiClientWithErrorsStub>();
                var planPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
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

                var result = _PlanRepository.GetLatestPricingJob(planPricingRequestDto.PlanId);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
                jsonResolver.Ignore(typeof(PlanPricingJob), "HangfireJobId");
                jsonResolver.Ignore(typeof(PlanPricingJob), "Completed");
                jsonResolver.Ignore(typeof(PlanPricingJob), "Queued");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }
    }
}
