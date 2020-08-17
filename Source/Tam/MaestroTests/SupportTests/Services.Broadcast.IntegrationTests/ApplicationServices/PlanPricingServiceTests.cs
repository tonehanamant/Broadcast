using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Services;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.QuoteReport;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Unity;

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
        public void QueuePricingJob_WithoutPlanTest()
        {
            using (new TransactionScopeWrapper())
            {
                var parameters = _GetPricingParametersWithoutPlanDto();
                var result = _PlanPricingService.QueuePricingJob(parameters, new DateTime(2019, 11, 4), "test user");

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
        public void GetCurrentPricingExecutionByJobId()
        {
            using (new TransactionScopeWrapper())
            {
                var job = _PlanPricingService.QueuePricingJob(_GetPricingParametersWithoutPlanDto(), new DateTime(2019, 11, 4)
                , "test user");

                var result = _PlanPricingService.GetCurrentPricingExecutionByJobId(job.Id);

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
        public void GetCurrentPricingExecutionByJobId_JobCancel()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanPricingService.QueuePricingJob(_GetPricingParametersWithoutPlanDto(), new DateTime(2020, 3, 3)
                , "test user");

                _PlanPricingService.ForceCompletePlanPricingJob(result.Id, "Test User");

                var problems = new Exception();
                try
                {
                    var resultPayload = _PlanPricingService.GetCurrentPricingExecutionByJobId(result.Id);
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
                    MarketGroup = MarketGroupEnum.None,
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
                    MarketGroup = MarketGroupEnum.None,
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
                jsonResolver.Ignore(typeof(PlanDto), "JobId");
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
                    MarketGroup = MarketGroupEnum.None,
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

                var result = _PlanRepository.GetPricingApiResultsByJobId(job.Id);

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

                var bands = _PlanPricingService.GetPricingBands(planPricingRequestDto.PlanId.Value);
                var result = _PlanPricingService.GetCurrentPricingExecution(planPricingRequestDto.PlanId.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
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
        public void GetPricingBandsByJobId()
        {
            using (new TransactionScopeWrapper())
            {
                var parameters = _GetPricingParametersWithoutPlanDto();

                var job = _PlanPricingService.QueuePricingJob(parameters, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingWithoutPlanJob(parameters, job.Id, CancellationToken.None);

                var bands = _PlanPricingService.GetPricingBandsByJobId(job.Id);
                var result = _PlanPricingService.GetCurrentPricingExecutionByJobId(job.Id);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
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
        public void GetProgramsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = _GetPricingRequestDto();

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var programs = _PlanPricingService.GetPrograms(planPricingRequestDto.PlanId.Value);
                var result = _PlanPricingService.GetCurrentPricingExecution(planPricingRequestDto.PlanId.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingProgramProgramDto), "Id");
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
        public void GetProgramsByJobId()
        {
            using (new TransactionScopeWrapper())
            {
                var parameters = _GetPricingParametersWithoutPlanDto();

                var job = _PlanPricingService.QueuePricingJob(parameters, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingWithoutPlanJob(parameters, job.Id, CancellationToken.None);

                var programs = _PlanPricingService.GetProgramsByJobId(job.Id);
                var result = _PlanPricingService.GetCurrentPricingExecutionByJobId(job.Id);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingProgramProgramDto), "Id");
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
        public void GetPricingResultsMarketsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = _GetPricingRequestDto();

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var markets = _PlanPricingService.GetMarkets(planPricingRequestDto.PlanId.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
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
        public void GetMarketsByJobId()
        {
            using (new TransactionScopeWrapper())
            {
                var parameters = _GetPricingParametersWithoutPlanDto();

                var job = _PlanPricingService.QueuePricingJob(parameters, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingWithoutPlanJob(parameters, job.Id, CancellationToken.None);

                var markets = _PlanPricingService.GetMarketsByJobId(job.Id);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
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
        public void GetStationsByJobId()
        {
            using (new TransactionScopeWrapper())
            {
                var parameters = _GetPricingParametersWithoutPlanDto();

                var job = _PlanPricingService.QueuePricingJob(parameters, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingWithoutPlanJob(parameters, job.Id, CancellationToken.None);

                var stations = _PlanPricingService.GetStationsByJobId(job.Id);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingStationResultDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingStationResultDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingStationDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stations, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPricingResultsMarketsPricingExecutedTwiceTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = _GetPricingRequestDto();

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var secondJob = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, secondJob.Id, CancellationToken.None);

                var markets = _PlanPricingService.GetMarkets(planPricingRequestDto.PlanId.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
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
        public void GetPricingResultsMarketsTest_VerifyMargin()
        {
            using (new TransactionScopeWrapper())
            {
                var planPricingRequestDto = _GetPricingRequestDto();
                // the GetPricingResultsMarketsTest test has the Margin included.
                // This test will null out Margin to see that that works
                // and verified by eye that the counts are different between the tests.
                planPricingRequestDto.Margin = null;

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "test user");

                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id, CancellationToken.None);

                var markets = _PlanPricingService.GetMarkets(planPricingRequestDto.PlanId.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
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
                    MarketGroup = MarketGroupEnum.None,
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

                var result = _PlanRepository.GetPlanPricingRuns(planPricingRequestDto.PlanId.Value);

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
                    MarketGroup = MarketGroupEnum.Top25,
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
                    MarketGroup = MarketGroupEnum.None
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

        [Test]
        [Category("long_running")]
        public void RunQuote()
        {
            // Arrange
            const string username = "RunQuoteTestUser";
            const string templatePath = @".\Files\Excel templates";
            var request = _GetQuoteRequest();

            var dateTimeEngine = new DateTimeEngineStub();
            dateTimeEngine.UT_CurrentMoment = new DateTime(2020, 8, 10, 14, 8, 45);
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IDateTimeEngine>(dateTimeEngine);

            var fileService = new FileServiceStub();
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(fileService);

            var planPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();

            // Act
            using (new TransactionScopeWrapper())
            {
                planPricingService.RunQuote(request, username, templatePath);
            }

            // Assert
            var savedFile = fileService.CreatedFileStreams;
            Assert.AreEqual(1, fileService.CreatedFileStreams.Count);
            var savedContentLength = savedFile.First().Item3.Length;
            Assert.IsTrue(savedContentLength > 7000);
        }

        [Test]
        [Category("long_running")]
        public void RunQuoteWithEmptyData()
        {
            // Arrange
            const string username = "RunQuoteTestUser";
            const string templatePath = @".\Files\Excel templates";
            var request = _GetQuoteRequest();
            request.FlightStartDate = new DateTime(2020, 9, 21);
            request.FlightEndDate = new DateTime(2020, 09, 27);

            var dateTimeEngine = new DateTimeEngineStub();
            dateTimeEngine.UT_CurrentMoment = new DateTime(2020, 8, 10, 14, 8, 45);
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IDateTimeEngine>(dateTimeEngine);

            var fileService = new FileServiceStub();
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(fileService);

            var planPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();

            // Act
            using (new TransactionScopeWrapper())
            {
                planPricingService.RunQuote(request, username, templatePath);
            }

            // Assert
            var savedFile = fileService.CreatedFileStreams;
            Assert.AreEqual(1, fileService.CreatedFileStreams.Count);
            var savedContentLength = savedFile.First().Item3.Length;
            Assert.IsTrue(savedContentLength > 6000);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetReportData()
        {
            // Arrange
            var request = _GetQuoteRequest();

            var dateTimeEngine = new DateTimeEngineStub();
            dateTimeEngine.UT_CurrentMoment = new DateTime(2020, 8, 10, 14, 8, 45);
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IDateTimeEngine>(dateTimeEngine);

            var planPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();

            // Act
            var result = ((PlanPricingService)planPricingService).GetQuoteReportData(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetReportDataWithEmptyData()
        {
            // Arrange
            var request = _GetQuoteRequest();
            request.FlightStartDate = new DateTime(2020, 9, 21);
            request.FlightEndDate = new DateTime(2020, 09, 27);

            var dateTimeEngine = new DateTimeEngineStub();
            dateTimeEngine.UT_CurrentMoment = new DateTime(2020, 8, 10, 14, 8, 45);
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IDateTimeEngine>(dateTimeEngine);

            var planPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();

            // Act
            var result = ((PlanPricingService) planPricingService).GetQuoteReportData(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private static QuoteRequestDto _GetQuoteRequest()
        {
            var request = new QuoteRequestDto
            {
                FlightStartDate = new DateTime(2018,12,17),
                FlightEndDate = new DateTime(2018,12,23),
                FlightHiatusDays = new List<DateTime>(),
                FlightDays = new List<int> {1,2,3,4,5,6,7},
                CreativeLengths = new List<CreativeLength> {new CreativeLength {SpotLengthId = 1, Weight = 100 }},
                Equivalized = true,
                AudienceId = 31,
                SecondaryAudiences = new List<PlanAudienceDto>
                {
                    new PlanAudienceDto { Type = AudienceTypeEnum.Nielsen, AudienceId = 287 },
                    new PlanAudienceDto { Type = AudienceTypeEnum.Nielsen, AudienceId = 420 }
                },
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                HUTBookId = 434,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 8,
                        StartTimeSeconds = 0,
                        EndTimeSeconds = 14400,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto(),
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto(),
                            AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto(),
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                Programs = new List<ProgramDto> {new ProgramDto {Name = "Early news"}}
                            }
                        }
                    }
                }
            };
            return request;
        }

        private int SetupPlanWithOneVersionAndADraftWithPricingRuns()
        {
            var plan = _GetNewPlan();
            var currentDate = new DateTime(2019, 01, 01);
            var username = "integration_test";
            var planId = _PlanService.SavePlan(plan, username, currentDate);
            var pricingParameters = new PlanPricingParametersDto
            {
                PlanId = planId,
                Budget = 5000,
                DeliveryImpressions = 5000,
                UnitCaps = 10,
                UnitCapsType = UnitCapEnum.Per30Min
            };
            var job = _PlanPricingService.QueuePricingJob(pricingParameters, currentDate, username);

            _PlanPricingService.RunPricingJob(pricingParameters, job.Id, CancellationToken.None);

            plan.Id = planId;
            plan.VersionId = 1;
            plan.IsDraft = true;

            var draftId = _PlanService.SavePlan(plan, username, currentDate);
            var draftVersionId = _PlanService.CheckForDraft(planId);

            pricingParameters.DeliveryImpressions = 2000;
            pricingParameters.PlanVersionId = draftVersionId;

            var draftJob = _PlanPricingService.QueuePricingJob(pricingParameters, currentDate, username);

            _PlanPricingService.RunPricingJob(pricingParameters, draftJob.Id, CancellationToken.None);

            return planId;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPricingBandsWithDraftTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();

                var bands = _PlanPricingService.GetPricingBands(planId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(bands, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetMarketsWithDraftTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();

                var markets = _PlanPricingService.GetMarkets(planId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
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
        public void GetStationsWithDraftTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();

                var stations = _PlanPricingService.GetStations(planId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingStationResultDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingStationResultDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingStationResultDto), "PlanVersionId");
                jsonResolver.Ignore(typeof(PlanPricingStationDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stations, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetProgramsWithDraftTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();

                var programs = _PlanPricingService.GetPrograms(planId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingProgramProgramDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(programs, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetProgramsForVersionTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();

                var plan = _PlanRepository.GetPlan(planId);

                var programs = _PlanPricingService.GetProgramsForVersion(planId, plan.VersionId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingProgramProgramDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(programs, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetMarketsForVersionTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();

                var plan = _PlanRepository.GetPlan(planId);

                var markets = _PlanPricingService.GetMarkets(planId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
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
        public void GetStationsForVersionTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();

                var plan = _PlanRepository.GetPlan(planId);

                var stations = _PlanPricingService.GetStations(planId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingStationResultDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingStationResultDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingStationResultDto), "PlanVersionId");
                jsonResolver.Ignore(typeof(PlanPricingStationDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(stations, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPricingBandsForVersionTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();

                var plan = _PlanRepository.GetPlan(planId);

                var bands = _PlanPricingService.GetPricingBands(planId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "Id");
                jsonResolver.Ignore(typeof(PlanPricingBandDto), "JobId");
                jsonResolver.Ignore(typeof(PlanPricingBandDetailDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(bands, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPricingParametersForDraftTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();

                var plan = _PlanService.GetPlan(planId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PlanPricingParametersDto), "PlanId");
                jsonResolver.Ignore(typeof(PlanPricingParametersDto), "PlanVersionId");
                jsonResolver.Ignore(typeof(PlanPricingParametersDto), "JobId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(plan.PricingParameters, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPricingParametersForPlanWithVersiontTest()
        {
            using (new TransactionScopeWrapper())
            {
                var planId = SetupPlanWithOneVersionAndADraftWithPricingRuns();
                var planVersionId = _PlanService.CheckForDraft(planId);
                var plan = _PlanService.GetPlan(planId, planVersionId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(plan.PricingParameters));
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
                MarketGroup = MarketGroupEnum.None,
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
        }

        private PricingParametersWithoutPlanDto _GetPricingParametersWithoutPlanDto()
        {
            return new PricingParametersWithoutPlanDto
            {
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
                    },
                AudienceId = 31,
                Equivalized = true,
                CreativeLengths = new List<CreativeLength> {
                    new CreativeLength { SpotLengthId = 2, Weight = 100}
                },
                FlightStartDate = new DateTime(2018, 10, 1),
                FlightEndDate = new DateTime(2018, 10, 31),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightHiatusDays = new List<DateTime>(),
                HUTBookId = 437,
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                TargetRatingPoints = 100d,
                CoverageGoalPercent = 80.5,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto {
                        Id = 2557,
                        MarketCode = 100,
                        MarketCoverageFileId = 1,
                        PercentageOfUS = 48,
                        Rank = 1,
                        ShareOfVoicePercent = 22.2,
                        Market = "Portland-Auburn"
                    },
                    new PlanAvailableMarketDto {
                        Id = 2558,
                        MarketCode = 101,
                        MarketCoverageFileId = 1,
                        PercentageOfUS = 32.5,
                        Rank = 2,
                        ShareOfVoicePercent = 34.5,
                        Market = "New York"
                    }
                },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 2,
                        DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews,
                        StartTimeSeconds = 0,
                        EndTimeSeconds = 1999,
                        WeightingGoalPercent = 100,
                        IsEndTimeModified = true,
                        IsStartTimeModified = true,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>(),
                        Restrictions = new PlanDaypartDto.RestrictionsDto()
                    }
                },
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1, MediaWeekId = 784,
                        StartDate = new DateTime(2018,12,31), EndDate = new DateTime(2019,01,06),
                        NumberOfActiveDays = 6, ActiveDays = "Tu-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyAdu = 5,
                        SpotLengthId = 1,
                        DaypartCodeId = 1,
                        PercentageOfWeek = 50,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2, MediaWeekId = 785,
                        StartDate = new DateTime(2019,01,07), EndDate = new DateTime(2019,01,13),
                        NumberOfActiveDays = 7, ActiveDays = "M-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3, MediaWeekId = 786,
                        StartDate = new DateTime(2019,01,14), EndDate = new DateTime(2019,01,20),
                        NumberOfActiveDays = 6, ActiveDays = "M-Sa", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4, MediaWeekId = 787,
                        StartDate = new DateTime(2019,01,21), EndDate = new DateTime(2019,01,27),
                        NumberOfActiveDays = 6, ActiveDays = "M-W,F-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 5, MediaWeekId = 788,
                        StartDate = new DateTime(2019,01,28), EndDate = new DateTime(2019,02,03),
                        NumberOfActiveDays = 4, ActiveDays = "M-Th", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyAdu = 30,
                        WeeklyUnits = 4
                    }
                },
                ImpressionsPerUnit = 5
            };
        }

        private static PlanDto _GetNewPlan()
        {
            const int AudienceID = 31;

            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "Plan with Draft Test",
                ProductId = 1,
                CreativeLengths = new List<CreativeLength> {
                    new CreativeLength { SpotLengthId = 1, Weight = 50},
                    new CreativeLength{ SpotLengthId = 2}
                },
                Status = PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2018, 10, 1),
                FlightEndDate = new DateTime(2018, 10, 31),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightNotes = "Sample notes",
                FlightHiatusDays = new List<DateTime>(),
                AudienceId = AudienceID,       //HH
                AudienceType = AudienceTypeEnum.Nielsen,
                HUTBookId = 437,
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                TargetCPM = 12m,
                TargetImpressions = 100d,
                TargetCPP = 12m,
                TargetUniverse = 111222d,
                Currency = PlanCurrenciesEnum.Impressions,
                TargetRatingPoints = 100d,
                CoverageGoalPercent = 80.5,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 48, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn"},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York"}
                },
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto {MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUS = 5.5, Rank = 5, Market = "Burlington-Plattsburgh" },
                    new PlanBlackoutMarketDto {MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUS = 2.5, Rank = 8, Market = "Amarillo" },
                },
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 2,
                        DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews,
                        StartTimeSeconds = 0,
                        EndTimeSeconds = 2000,
                        WeightingGoalPercent = 28.0,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.8,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 11,
                        DaypartTypeId = DaypartTypeEnum.News,
                        StartTimeSeconds = 1500,
                        EndTimeSeconds = 2788,
                        WeightingGoalPercent = 33,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.9,
                                VpvhType = VpvhTypeEnum.LastYear,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                ShowTypes = new List<LookupDto> { new LookupDto { Id = 2 } }
                            },
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                Genres = new List<LookupDto> { new LookupDto { Id = 20 } }
                            },
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        ContentRating = "G",
                                        Genre = new LookupDto { Id = 25},
                                        Name = "Teletubbies"
                                    }
                                }
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 3,
                        DaypartTypeId = DaypartTypeEnum.ROS,
                        StartTimeSeconds = 57600,
                        EndTimeSeconds = 68400,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.7,
                                VpvhType = VpvhTypeEnum.PreviousQuarter,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                ShowTypes = new List<LookupDto>
                                {
                                    new LookupDto { Id = 9 },
                                    new LookupDto { Id = 11 }
                                }
                            },
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Genres = new List<LookupDto>
                                {
                                    new LookupDto { Id = 12 },
                                    new LookupDto { Id = 14 }
                                }
                            },
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        ContentRating = "R",
                                        Genre = new LookupDto { Id = 25},
                                        Name = "Power Rangers"
                                    }
                                }
                            }
                        }
                    }
                },
                Vpvh = 0.012,
                IsAduEnabled = true,
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1, MediaWeekId = 784,
                        StartDate = new DateTime(2018,12,31), EndDate = new DateTime(2019,01,06),
                        NumberOfActiveDays = 6, ActiveDays = "Tu-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyAdu = 5,
                        SpotLengthId = 1,
                        DaypartCodeId = 1,
                        PercentageOfWeek = 50,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2, MediaWeekId = 785,
                        StartDate = new DateTime(2019,01,07), EndDate = new DateTime(2019,01,13),
                        NumberOfActiveDays = 7, ActiveDays = "M-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3, MediaWeekId = 786,
                        StartDate = new DateTime(2019,01,14), EndDate = new DateTime(2019,01,20),
                        NumberOfActiveDays = 6, ActiveDays = "M-Sa", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4, MediaWeekId = 787,
                        StartDate = new DateTime(2019,01,21), EndDate = new DateTime(2019,01,27),
                        NumberOfActiveDays = 6, ActiveDays = "M-W,F-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 5, MediaWeekId = 788,
                        StartDate = new DateTime(2019,01,28), EndDate = new DateTime(2019,02,03),
                        NumberOfActiveDays = 4, ActiveDays = "M-Th", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyAdu = 30,
                        WeeklyUnits = 4
                    }
                },
                ImpressionsPerUnit = 5
            };
        }
    }
}
