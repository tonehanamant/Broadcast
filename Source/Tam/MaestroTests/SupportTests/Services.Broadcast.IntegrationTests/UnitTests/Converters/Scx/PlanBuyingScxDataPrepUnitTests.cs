using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests.Converters.Scx
{
    [TestFixture]
    public class PlanBuyingScxDataPrepUnitTests
    {
        private Mock<IPlanBuyingRepository> _PlanBuyingRepository;
        private Mock<IPlanRepository> _PlanRepository;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactory;

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
                    _BroadcastAudiencesCache.Object);
        }

        [SetUp]
        public void Setup()
        {
            _SpotLengthEngine = new Mock<ISpotLengthEngine>();
            _SpotLengthEngine.Setup(s => s.GetDeliveryMultipliers())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);
            _SpotLengthEngine.Setup(s => s.GetCostMultipliers())
                .Returns(SpotLengthTestData.GetCostMultipliersBySpotLengthId);
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

            var caught = Assert.Throws<InvalidOperationException>(() => testClass._GetValidatedPlanAndJob(request, out var resultPlan, out var resultJob));
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
            var beforeSpots = new List<PlanBuyingAllocatedSpot>
            {
                new PlanBuyingAllocatedSpot
                {
                    Id = 1, StationInventoryManifestId = 10,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency {SpotLengthId = 1, SpotCost = 22, Impressions = 2400}, // stays
                        new SpotFrequency {SpotLengthId = 2, SpotCost = 40, Impressions = 3500}, // filtered - too big
                    }
                },
                new PlanBuyingAllocatedSpot
                {
                    Id = 2, StationInventoryManifestId = 20,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency {SpotLengthId = 2, SpotCost = 30, Impressions = 3000}, // stays
                    }
                },
                new PlanBuyingAllocatedSpot
                {
                    Id = 3, StationInventoryManifestId = 30,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency {SpotLengthId = 1, SpotCost = 40, Impressions = 6000}, // filtered - too small
                        new SpotFrequency {SpotLengthId = 5, SpotCost = 30, Impressions = 3300}, // stays
                    }
                },
                new PlanBuyingAllocatedSpot
                {
                    Id = 4, StationInventoryManifestId = 40,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency {SpotLengthId = 3, SpotCost = 40, Impressions = 3500}, // filtered -- too big
                    }
                }
                ,
                new PlanBuyingAllocatedSpot
                {
                    Id = 5, StationInventoryManifestId = 50,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency {SpotLengthId = 6, SpotCost = 60, Impressions = 6600}, // stays
                    }
                }
            };
            var expectedResult = new[]
            {
                new {Id = 1, SpotLengthId = 1},
                new {Id = 2, SpotLengthId = 2},
                new {Id = 3, SpotLengthId = 5},
                new {Id = 5, SpotLengthId = 6},
            };

            var testClass = _GetTestClass();

            // Act
            var afterSpots = testClass._ApplyCpmThreshold(cpmThresholdPercent, goalCpm, beforeSpots);

            // Assert
            var resultIds = afterSpots.SelectMany(s =>
                    s.SpotFrequencies.Select(f => new { s.Id, f.SpotLengthId }))
                .ToArray();

            Assert.AreEqual(expectedResult.Length, resultIds.Length);
            for (var i = 0; i < expectedResult.Length; i++)
            {
                Assert.AreEqual(expectedResult[i].Id, resultIds[i].Id);
                Assert.AreEqual(expectedResult[i].SpotLengthId, resultIds[i].SpotLengthId);
            }
        }
   
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScxData()
        {
            var request = new PlanBuyingScxExportRequest {PlanId = 21, UnallocatedCpmThreshold = 12};
            var job = new PlanBuyingJob {Id = 1, PlanVersionId = 57};
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
            var jobParams = new PlanBuyingParametersDto {Margin = 20};
            var jobResult = new PlanBuyingAllocationResult
            {
                SpotAllocationModelMode = SpotAllocationModelMode.Quality,
                AllocatedSpots = new List<PlanBuyingAllocatedSpot>
                {
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 1, StationInventoryManifestId = 10,
                        ContractMediaWeek = new MediaWeek { Id = 871 },
                        StandardDaypart = new StandardDaypartDto{ Id = 1},
                        SpotFrequencies = new List<SpotFrequency>
                            {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 1, StationInventoryManifestId = 10,
                        ContractMediaWeek = new MediaWeek { Id = 872 },
                        StandardDaypart = new StandardDaypartDto{ Id = 1},
                        SpotFrequencies = new List<SpotFrequency>
                            {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                    }
                },
                UnallocatedSpots = new List<PlanBuyingAllocatedSpot>
                {
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 1, StationInventoryManifestId = 10,
                        ContractMediaWeek = new MediaWeek { Id = 871 },
                        StandardDaypart = new StandardDaypartDto{ Id = 1},
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0}, // stay
                            new SpotFrequency {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0} // filtered - too big
                        } 
                    },
                    new PlanBuyingAllocatedSpot
                    {
                        Id = 1, StationInventoryManifestId = 10,
                        ContractMediaWeek = new MediaWeek { Id = 872 },
                        StandardDaypart = new StandardDaypartDto{ Id = 1},
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency {SpotLengthId = 2, SpotCost = 30, Impressions = 3000, Spots = 0}, // stay
                            new SpotFrequency {SpotLengthId = 3, SpotCost = 15, Impressions = 3500, Spots = 0} // filtered - too big
                        }
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
                            Programs = new List<StationInventoryManifestDaypartProgram> {new StationInventoryManifestDaypartProgram {Id=12, ProgramName = "MyTestProgram"}}
                        }
                    }
                }
            };
            
            _PlanBuyingRepository.Setup(s => s.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(job);
            _PlanBuyingRepository.Setup(s => s.GetLatestParametersForPlanBuyingJob(It.IsAny<int>()))
                .Returns(jobParams);
            _PlanBuyingRepository.Setup(s => s.GetBuyingApiResultsByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(jobResult);

            _PlanRepository.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            _InventoryRepository.Setup(s => s.GetPlanBuyingScxInventory(It.IsAny<int>()))
                .Returns(inventory);

            var generated = new DateTime(2020, 10, 17, 12, 34, 56);
            var testClass = _GetTestClass();

            // Act
            var resultQ = testClass.GetScxData(request, generated, SpotAllocationModelMode.Quality);
            var resultE = testClass.GetScxData(request, generated, SpotAllocationModelMode.Efficiency);
            var resultF = testClass.GetScxData(request, generated, SpotAllocationModelMode.Floor);

            var result = new            {
              
                Quality = resultQ,
                Efficiency = resultE,
                Floor = resultF,
            };

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}