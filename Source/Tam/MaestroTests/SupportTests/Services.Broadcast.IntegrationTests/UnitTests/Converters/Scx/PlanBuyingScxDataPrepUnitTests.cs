using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using FizzWare.NBuilder;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.Converters.Scx
{
    [TestFixture]
    public class PlanBuyingScxDataPrepUnitTests
    {
        private Mock<IPlanBuyingRepository> _PlanBuyingRepository;
        private Mock<IPlanRepository> _PlanRepository;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactory;

        private Mock<IPlanBuyingRequestLogClient> _PlanBuyingRequestLogClient;
        private Mock<ISpotLengthEngine> _SpotLengthEngine;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCache;
        private Mock<IBroadcastAudiencesCache> _BroadcastAudiencesCache;
        private Mock<IInventoryRepository> _InventoryRepository;
        private Mock<IMarketDmaMapRepository> _MarketDmaMapRepository;
        private Mock<IStandardDaypartRepository> _StandardDaypartRepository;
        private Mock<IStationRepository> _StationRepository;

        private PlanBuyingScxDataPrep _GetTestClass()
        {
            return new PlanBuyingScxDataPrep(_DataRepositoryFactory.Object,
                    _SpotLengthEngine.Object,
                    _MediaMonthAndWeekAggregateCache.Object,
                    _BroadcastAudiencesCache.Object,
                    _PlanBuyingRequestLogClient.Object);
        }

        [SetUp]
        public void Setup()
        {
            _PlanBuyingRequestLogClient = new Mock<IPlanBuyingRequestLogClient>();

            _SpotLengthEngine = new Mock<ISpotLengthEngine>();
            _SpotLengthEngine.Setup(s => s.GetDeliveryMultipliers())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);
            _SpotLengthEngine.Setup(s => s.GetCostMultipliers(true))
                .Returns(SpotLengthTestData.GetCostMultipliersBySpotLengthId(applyInventoryPremium:true));
            _SpotLengthEngine.Setup(s => s.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns<int>(SpotLengthTestData.GetSpotLengthValueById);

            _MediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            _MediaMonthAndWeekAggregateCache.Setup(s => s.GetMediaWeekById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaWeek);
            _MediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksIntersecting(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns<DateTime, DateTime>(MediaMonthAndWeekTestData.GetMediaWeeksIntersecting);
            _MediaMonthAndWeekAggregateCache.Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaMonthById);

            _BroadcastAudiencesCache = new Mock<IBroadcastAudiencesCache>();
            _BroadcastAudiencesCache.Setup(s => s.GetAllEntities())
                .Returns(AudienceTestData.GetAllEntities());

            /*** Data Repos ***/
            _DataRepositoryFactory = new Mock<IDataRepositoryFactory>();

            _PlanBuyingRepository = new Mock<IPlanBuyingRepository>();
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IPlanBuyingRepository>())
                .Returns(_PlanBuyingRepository.Object);

            _PlanRepository = new Mock<IPlanRepository>();
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IPlanRepository>())
                .Returns(_PlanRepository.Object);

            _InventoryRepository = new Mock<IInventoryRepository>();
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepository.Object);

            _MarketDmaMapRepository = new Mock<IMarketDmaMapRepository>();
            _MarketDmaMapRepository.Setup(s => s.GetMarketMapFromMarketCodes(It.IsAny<IEnumerable<int>>()))
                .Returns<IEnumerable<int>>(MarketsTestData.GetMarketMapFromMarketCodes);
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IMarketDmaMapRepository>())
                .Returns(_MarketDmaMapRepository.Object);

            _StandardDaypartRepository = new Mock<IStandardDaypartRepository>();
            _StandardDaypartRepository.Setup(s => s.GetStandardDaypartIdDaypartIds())
                .Returns(DaypartsTestData.GetStandardDaypartIdDaypartIds);
            _StandardDaypartRepository.Setup(s => s.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData);
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_StandardDaypartRepository.Object);

            _StationRepository = new Mock<IStationRepository>();
            _StationRepository.Setup(s => s.GetBroadcastStationsByIds(It.IsAny<List<int>>()))
                .Returns<List<int>>(StationsTestData.GetBroadcastStationsByIds);
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IStationRepository>())
                .Returns(_StationRepository.Object);
        }

        [Test]
        public void GetValidatedPlanAndJob()
        {
            // Arrange
            var request = new PlanBuyingScxExportRequest { PlanId = 21, UnallocatedCpmThreshold = 12 };
            var job = new PlanBuyingJob { Id = 1, PlanVersionId = 57 };
            var plan = new PlanDto { Id = 21, TargetCPM = 10 };

            _PlanBuyingRepository.Setup(s => s.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(job);
            _PlanRepository.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            var testClass = _GetTestClass();

            // Act
            testClass._GetValidatedPlanAndJob(request, out var resultPlan, out var resultJob);

            // Assert
            Assert.IsNotNull(resultJob);
            Assert.AreEqual(job.Id, resultJob.Id);
            Assert.AreEqual(job.PlanVersionId, resultJob.PlanVersionId);

            Assert.IsNotNull(resultPlan);
            Assert.AreEqual(plan.Id, resultPlan.Id);
            Assert.AreEqual(plan.TargetCPM, resultPlan.TargetCPM);
        }

        [Test]
        public void GetValidatedPlanAndJob_NoJob()
        {
            var request = new PlanBuyingScxExportRequest { PlanId = 21, UnallocatedCpmThreshold = 12 };
            var expectedMessage = "A buying job execution was not found for plan id '21'.";
            PlanBuyingJob job = null;
            _PlanBuyingRepository.Setup(s => s.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(job);

            var testClass = _GetTestClass();

            var caught = Assert.Throws<CadentException>(() => testClass._GetValidatedPlanAndJob(request, out var resultPlan, out var resultJob));
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetValidatedPlanAndJob_NoPlanVersion()
        {
            var request = new PlanBuyingScxExportRequest { PlanId = 21, UnallocatedCpmThreshold = 12 };
            var expectedMessage = "The buying job '1' for plan '21' does not have a plan version.";
            var job = new PlanBuyingJob { Id = 1, PlanVersionId = null };
            _PlanBuyingRepository.Setup(s => s.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(job);

            var testClass = _GetTestClass();

            var caught = Assert.Throws<InvalidOperationException>(() => testClass._GetValidatedPlanAndJob(request, out var resultPlan, out var resultJob));
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetValidatedPlanAndJob_NoTargetCpm()
        {
            var request = new PlanBuyingScxExportRequest { PlanId = 21, UnallocatedCpmThreshold = 12 };
            var expectedMessage = "The plan '21' version id '57' does not have a required target cpm.";
            var job = new PlanBuyingJob { Id = 1, PlanVersionId = 57 };
            var plan = new PlanDto { Id = 21, TargetCPM = null };

            _PlanBuyingRepository.Setup(s => s.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(job);
            _PlanRepository.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            var testClass = _GetTestClass();

            var caught = Assert.Throws<InvalidOperationException>(() => testClass._GetValidatedPlanAndJob(request, out var resultPlan, out var resultJob));
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ApplyCpmThreshold()
        {
            // Arrange
            var cpmThresholdPercent = 10;
            var goalCpm = 10;
            var beforeSpots = new List<PlanBuyingSpotRaw>
            {
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 10,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 22, Impressions = 2400}, // stays
                        new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 40, Impressions = 3500}, // filtered - too big
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 20,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000}, // stays
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 30,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 40, Impressions = 6000}, // filtered - too small
                        new SpotFrequencyRaw {SpotLengthId = 5, SpotCost = 30, Impressions = 3300}, // stays
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 40,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 3, SpotCost = 40, Impressions = 3500}, // filtered -- too big
                    }
                }
                ,
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 50,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 6, SpotCost = 60, Impressions = 6600}, // stays
                    }
                }
            };
            var expectedResult = new[]
            {
                new {StationInventoryManifestId = 10, SpotLengthId = 1},
                new {StationInventoryManifestId = 20, SpotLengthId = 2},
                new {StationInventoryManifestId = 30, SpotLengthId = 5},
                new {StationInventoryManifestId = 50, SpotLengthId = 6},
            };

            var testClass = _GetTestClass();

            // Act
            var afterSpots = testClass._ApplyCpmThreshold(cpmThresholdPercent, goalCpm, beforeSpots);

            // Assert
            var resultIds = afterSpots.SelectMany(s =>
                    s.SpotFrequenciesRaw.Select(f => new { s.StationInventoryManifestId, f.SpotLengthId }))
                .ToArray();

            Assert.AreEqual(expectedResult.Length, resultIds.Length);
            for (var i = 0; i < expectedResult.Length; i++)
            {
                Assert.AreEqual(expectedResult[i].StationInventoryManifestId, resultIds[i].StationInventoryManifestId);
                Assert.AreEqual(expectedResult[i].SpotLengthId, resultIds[i].SpotLengthId);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScxData()
        {
            var request = new PlanBuyingScxExportRequest { PlanId = 21, UnallocatedCpmThreshold = 12 };
            var job = new PlanBuyingJob { Id = 1, PlanVersionId = 57 };
            var plan = new PlanDto
            {
                Id = 21,
                Name = "MyTestPlan",
                CampaignId = 1,
                CampaignName = "MyTestCampaign",
                TargetCPM = 10,
                FlightStartDate = new DateTime(2020, 8, 31),
                FlightEndDate = new DateTime(2020, 9, 13),
                AudienceId = 31,
                ShareBookId = 460
            };
            var jobParams = new PlanBuyingParametersDto { Margin = 20 };
            var rawResult = new PlanBuyingInventoryRawDto
            {
                SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                PostingType = PostingTypeEnum.NSI,
                AllocatedSpotsRaw = new List<PlanBuyingSpotRaw>
                {
                    new PlanBuyingSpotRaw
                    {
                        StationInventoryManifestId = 11,
                        ContractMediaWeekId = 871,
                        InventoryMediaWeekId = 871,
                        StandardDaypartId = 1,
                        SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                            {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                    },
                    new PlanBuyingSpotRaw
                    {
                        StationInventoryManifestId = 11,
                        ContractMediaWeekId = 872,
                        InventoryMediaWeekId = 872,
                        StandardDaypartId = 1,
                        SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                            {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                    }
                },
                UnallocatedSpotsRaw = new List<PlanBuyingSpotRaw>
                {
                    new PlanBuyingSpotRaw
                    {
                        StationInventoryManifestId = 10,
                        ContractMediaWeekId = 871,
                        InventoryMediaWeekId = 871,
                        StandardDaypartId = 1,
                        SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                        {
                            new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0}, // stay
                            new SpotFrequencyRaw {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0} // filtered - too big
                        }
                    },
                    new PlanBuyingSpotRaw
                    {
                        StationInventoryManifestId = 10,
                        ContractMediaWeekId = 872,
                        InventoryMediaWeekId = 872,
                        StandardDaypartId = 1,
                        SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                        {
                            new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0}, // stay
                            new SpotFrequencyRaw {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0} // filtered - too big
                        }
                    }
                }
            };

            string serilizedResult = JsonConvert.SerializeObject(rawResult);

            var jobResult = new PlanBuyingAllocationResult
            {
                SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                AllocatedSpots = new List<PlanBuyingAllocatedSpot>
                {
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 1, StationInventoryManifestId = 11,
                        ContractMediaWeek = new MediaWeek { Id = 871 },
                        InventoryMediaWeek = new MediaWeek { Id = 871 },
                        StandardDaypart = new StandardDaypartDto{ Id = 1},
                        SpotFrequencies = new List<SpotFrequency>
                            {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 1, StationInventoryManifestId = 11,
                        ContractMediaWeek = new MediaWeek { Id = 872 },
                        InventoryMediaWeek = new MediaWeek { Id = 872 },
                        StandardDaypart = new StandardDaypartDto{ Id = 1},
                        SpotFrequencies = new List<SpotFrequency>
                            {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                    }
                }
            };

            var inventory = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    Id = 10,
                    Station = new DisplayBroadcastStation {Id =2, MarketCode = 101},
                    ManifestRates = new List<StationInventoryManifestRate>
                    {
                        new StationInventoryManifestRate {SpotLengthId = 1, SpotCost = 20},
                        new StationInventoryManifestRate {SpotLengthId = 2, SpotCost = 30},
                        new StationInventoryManifestRate {SpotLengthId = 3, SpotCost = 30}
                    },
                    ManifestDayparts = new List<StationInventoryManifestDaypart>
                    {
                        new StationInventoryManifestDaypart
                        {
                            PrimaryProgramId = 12,
                            Programs = new List<StationInventoryManifestDaypartProgram> {new StationInventoryManifestDaypartProgram {Id=12, ProgramName = "MyTestProgram"}},
                            Daypart = new DisplayDaypart{
                                Id = 59803,
                                Code = "CUS"
                            }
                        }
                    }
                },
                new StationInventoryManifest
                {
                    Id = 11,
                    Station = new DisplayBroadcastStation {Id =2, MarketCode = 101},
                    ManifestRates = new List<StationInventoryManifestRate>
                    {
                        new StationInventoryManifestRate {SpotLengthId = 1, SpotCost = 20},
                        new StationInventoryManifestRate {SpotLengthId = 2, SpotCost = 30},
                        new StationInventoryManifestRate {SpotLengthId = 3, SpotCost = 30}
                    },
                    ManifestDayparts = new List<StationInventoryManifestDaypart>
                    {
                        new StationInventoryManifestDaypart
                        {
                            PrimaryProgramId = 12,
                            Programs = new List<StationInventoryManifestDaypartProgram> {new StationInventoryManifestDaypartProgram {Id=12, ProgramName = "MyTestProgram"}},
                            Daypart = new DisplayDaypart{
                                Id = 59803,
                                Code = "CUS"
                            }
                        }
                    }
                }
            };

            _PlanBuyingRepository.Setup(s => s.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(job);
            _PlanBuyingRepository.Setup(s => s.GetLatestParametersForPlanBuyingJob(It.IsAny<int>()))
                .Returns(jobParams);
            _PlanBuyingRepository.Setup(s => s.GetBuyingApiResultsByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>(), It.IsAny<PostingTypeEnum>()))
                .Returns(jobResult);
            _PlanBuyingRequestLogClient.Setup(s => s.GetBuyingRawInventory(It.IsAny<int>()))
                .Returns(serilizedResult);


            _PlanRepository.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);
            _InventoryRepository.Setup(s => s.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(inventory);

            var generated = new DateTime(2020, 10, 17, 12, 34, 56);
            var testClass = _GetTestClass();

            // Act
            var resultE = testClass.GetScxData(request, generated, SpotAllocationModelMode.Efficiency);
            var resultF = testClass.GetScxData(request, generated, SpotAllocationModelMode.Floor);

            var result = new
           {
                Efficiency = resultE,
                Floor = resultF,
            };

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void UnEquivalize()
        {
            // Arrange          
            var planBuyingInventoryRaw = Builder<PlanBuyingInventoryRawDto>.CreateNew()
                .With(x => x.AllocatedSpotsRaw = Builder<PlanBuyingSpotRaw>.CreateListOfSize(2)
                        .All()
                        .With(y => y.SpotFrequenciesRaw = Builder<SpotFrequencyRaw>.CreateListOfSize(2).Build().ToList())
                        .Build()
                        .ToList())
                .With(x => x.UnallocatedSpotsRaw = Builder<PlanBuyingSpotRaw>.CreateListOfSize(2)
                        .All()
                        .With(y => y.SpotFrequenciesRaw = Builder<SpotFrequencyRaw>.CreateListOfSize(2).Build().ToList())
                        .Build()
                        .ToList())
                .Build();

            var controlledAllocatedSpot = planBuyingInventoryRaw.AllocatedSpotsRaw[1].SpotFrequenciesRaw[1];
            controlledAllocatedSpot.SpotLengthId = 2;
            controlledAllocatedSpot.Impressions = 100;

            var controlledUnAllocatedSpot = planBuyingInventoryRaw.UnallocatedSpotsRaw[1].SpotFrequenciesRaw[1];
            controlledUnAllocatedSpot.SpotLengthId = 3;
            controlledUnAllocatedSpot.Impressions = 200;

            var testClass = _GetTestClass();

            // Act
           testClass._UnEquivalizeSpots(planBuyingInventoryRaw);

            // Assert
            Assert.AreEqual(50, controlledAllocatedSpot.Impressions);
            Assert.AreEqual(400, controlledUnAllocatedSpot.Impressions);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void _UpdateAllocationBuckets_SingleFrequency()
        {
            // Arrange
            var rawResult = new List<PlanBuyingSpotRaw>
            {
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 11,
                    ContractMediaWeekId = 871,
                    InventoryMediaWeekId = 871,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                        {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 0}}
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 11,
                    ContractMediaWeekId = 872,
                    InventoryMediaWeekId = 872,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                        {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 0}}
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 10,
                    ContractMediaWeekId = 871,
                    InventoryMediaWeekId = 871,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0}
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 10,
                    ContractMediaWeekId = 872,
                    InventoryMediaWeekId = 872,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0}
                    }
                }
            };

            var jobResult = new List<PlanBuyingAllocatedSpot>
            {
                new PlanBuyingAllocatedSpot
                {
                    Id = 1, StationInventoryManifestId = 11,
                    ContractMediaWeek = new MediaWeek { Id = 871 },
                    InventoryMediaWeek = new MediaWeek { Id = 871 },
                    StandardDaypart = new StandardDaypartDto{ Id = 1},
                    SpotFrequencies = new List<SpotFrequency>
                        {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                },
                new PlanBuyingAllocatedSpot
                {
                    Id = 1, StationInventoryManifestId = 11,
                    ContractMediaWeek = new MediaWeek { Id = 872 },
                    InventoryMediaWeek = new MediaWeek { Id = 872 },
                    StandardDaypart = new StandardDaypartDto{ Id = 1},
                    SpotFrequencies = new List<SpotFrequency>
                        {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                }
            };

            var testClass = _GetTestClass();

            // Act
            var result = testClass._UpdateAllocationBuckets(jobResult, rawResult, 
                    SpotAllocationModelMode.Efficiency, PostingTypeEnum.NSI);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void _UpdateAllocationBuckets_MultipleFrequencies()
        {
            // Arrange
            var rawResult = new List<PlanBuyingSpotRaw>
            {
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 11,
                    ContractMediaWeekId = 871,
                    InventoryMediaWeekId = 871,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 0}},
                        {new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3400, Spots = 0}}
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 11,
                    ContractMediaWeekId = 872,
                    InventoryMediaWeekId = 872,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 0}},
                        {new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3400, Spots = 0}}
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 10,
                    ContractMediaWeekId = 871,
                    InventoryMediaWeekId = 871,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0},
                        new SpotFrequencyRaw {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0}
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 10,
                    ContractMediaWeekId = 872,
                    InventoryMediaWeekId = 872,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0},
                        new SpotFrequencyRaw {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0}
                    }
                }
            };

            var jobResult = new List<PlanBuyingAllocatedSpot>
            {
                new PlanBuyingAllocatedSpot
                {
                    Id = 1, StationInventoryManifestId = 11,
                    ContractMediaWeek = new MediaWeek { Id = 871 },
                    InventoryMediaWeek = new MediaWeek { Id = 871 },
                    StandardDaypart = new StandardDaypartDto{ Id = 1},
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}},
                        {new SpotFrequency {SpotLengthId = 2, SpotCost = 30, Impressions = 3400, Spots = 5}}
                    }
                },
                new PlanBuyingAllocatedSpot
                {
                    Id = 1, StationInventoryManifestId = 11,
                    ContractMediaWeek = new MediaWeek { Id = 872 },
                    InventoryMediaWeek = new MediaWeek { Id = 872 },
                    StandardDaypart = new StandardDaypartDto{ Id = 1},
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}},
                        {new SpotFrequency {SpotLengthId = 2, SpotCost = 30, Impressions = 3400, Spots = 5}}
                    }
                }
            };

            var testClass = _GetTestClass();

            // Act
            var result = testClass._UpdateAllocationBuckets(jobResult, rawResult,
                    SpotAllocationModelMode.Efficiency, PostingTypeEnum.NSI);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void _UpdateAllocationBuckets_MultipleMixedFrequencies()
        {
            // Arrange
            var rawResult = new List<PlanBuyingSpotRaw>
            {
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 11,
                    ContractMediaWeekId = 871,
                    InventoryMediaWeekId = 871,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 0}},
                        {new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3400, Spots = 0}}
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 11,
                    ContractMediaWeekId = 872,
                    InventoryMediaWeekId = 872,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        {new SpotFrequencyRaw {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 0}},
                        {new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3400, Spots = 0}}
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 10,
                    ContractMediaWeekId = 871,
                    InventoryMediaWeekId = 871,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0},
                        new SpotFrequencyRaw {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0}
                    }
                },
                new PlanBuyingSpotRaw
                {
                    StationInventoryManifestId = 10,
                    ContractMediaWeekId = 872,
                    InventoryMediaWeekId = 872,
                    StandardDaypartId = 1,
                    SpotFrequenciesRaw = new List<SpotFrequencyRaw>
                    {
                        new SpotFrequencyRaw {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0},
                        new SpotFrequencyRaw {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0}
                    }
                }
            };

            var jobResult = new List<PlanBuyingAllocatedSpot>
            {
                new PlanBuyingAllocatedSpot
                {
                    Id = 1, StationInventoryManifestId = 11,
                    ContractMediaWeek = new MediaWeek { Id = 871 },
                    InventoryMediaWeek = new MediaWeek { Id = 871 },
                    StandardDaypart = new StandardDaypartDto{ Id = 1},
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                    }
                },
                new PlanBuyingAllocatedSpot
                {
                    Id = 1, StationInventoryManifestId = 11,
                    ContractMediaWeek = new MediaWeek { Id = 872 },
                    InventoryMediaWeek = new MediaWeek { Id = 872 },
                    StandardDaypart = new StandardDaypartDto{ Id = 1},
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                    }
                }
            };

            var testClass = _GetTestClass();

            // Act
            var result = testClass._UpdateAllocationBuckets(jobResult, rawResult,
                    SpotAllocationModelMode.Efficiency, PostingTypeEnum.NSI);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}