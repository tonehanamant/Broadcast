using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.StationInventory;
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
    public class PlanPricingScxDataPrepUnitTests
    {
        private Mock<IDataRepositoryFactory> _DataRepositoryFactory;
        private Mock<IPlanRepository> _PlanRepository;
        private Mock<IInventoryRepository> _InventoryRepository;
        private Mock<IMarketDmaMapRepository> _MarketDmaMapRepository;
        private Mock<IStandardDaypartRepository> _StandardDaypartRepository;

        private Mock<ISpotLengthEngine> _SpotLengthEngine;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCache;
        private Mock<IBroadcastAudiencesCache> _BroadcastAudiencesCache;
        private Mock<IStationRepository> _StationRepository;

        private PlanPricingScxDataPrep _GetTestClass()
        {
            return new PlanPricingScxDataPrep(_DataRepositoryFactory.Object,
                    _SpotLengthEngine.Object,
                    _MediaMonthAndWeekAggregateCache.Object,
                    _BroadcastAudiencesCache.Object);
        }

        [SetUp]
        public void Setup()
        {
            _SpotLengthEngine = new Mock<ISpotLengthEngine>();
            _SpotLengthEngine.Setup(s => s.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns<int>(SpotLengthTestData.GetSpotLengthValueById);
            _SpotLengthEngine.Setup(s => s.GetDeliveryMultipliers())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);

            _MediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
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

            _PlanRepository = new Mock<IPlanRepository>();
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IPlanRepository>())
                .Returns(_PlanRepository.Object);

            _InventoryRepository = new Mock<IInventoryRepository>();
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepository.Object);

            _MarketDmaMapRepository = new Mock<IMarketDmaMapRepository>();
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IMarketDmaMapRepository>())
                .Returns(_MarketDmaMapRepository.Object);
            _MarketDmaMapRepository.Setup(s => s.GetMarketMapFromMarketCodes(It.IsAny<IEnumerable<int>>()))
                .Returns<IEnumerable<int>>(MarketsTestData.GetMarketMapFromMarketCodes);

            _StandardDaypartRepository = new Mock<IStandardDaypartRepository>();
            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_StandardDaypartRepository.Object);
            _StandardDaypartRepository.Setup(s => s.GetStandardDaypartIdDaypartIds())
                .Returns(DaypartsTestData.GetStandardDaypartIdDaypartIds);
            _StandardDaypartRepository.Setup(s => s.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData);

            _StationRepository = new Mock<IStationRepository>();
            _DataRepositoryFactory.Setup(x => x.GetDataRepository<IStationRepository>())
                .Returns(_StationRepository.Object);
            _StationRepository.Setup(s => s.GetBroadcastStationsByIds(It.IsAny<List<int>>()))
                .Returns<List<int>>(StationsTestData.GetBroadcastStationsByIds);
        }

        [Test]
        public void GetValidatedPlanAndJob()
        {
            // Arrange
            int planId = 21;
            var job = new PlanPricingJob { Id = 1, PlanVersionId = 57 };
            var plan = new PlanDto { Id = 21, TargetCPM = 10 };

            _PlanRepository.Setup(s => s.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(job);
            _PlanRepository.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            var testClass = _GetTestClass();

            // Act
            testClass._GetValidatedPlanAndPricingJob(planId, out var resultPlan, out var resultJob);

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
            // Arrange
            int planId = 21;
            var expectedMessage = "A pricing job execution was not found for plan id '21'.";

            PlanPricingJob job = null;
            _PlanRepository.Setup(s => s.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(job);

            var testClass = _GetTestClass();

            // Act
            var caught = Assert.Throws<InvalidOperationException>(() => testClass._GetValidatedPlanAndPricingJob(planId, out var resultPlan, out var resultJob));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetValidatedPlanAndJob_NoPlanVersion()
        {
            // Arrange
            int planId = 21;
            var expectedMessage = "The pricing job '1' for plan '21' does not have a plan version.";
            var job = new PlanPricingJob { Id = 1, PlanVersionId = null };

            _PlanRepository.Setup(s => s.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(job);

            var testClass = _GetTestClass();

            // Act
            var caught = Assert.Throws<InvalidOperationException>(() => testClass._GetValidatedPlanAndPricingJob(planId, out var resultPlan, out var resultJob));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetValidatedPlanAndJob_NoTargetCpm()
        {
            // Arrange
            int planId = 21;
            var expectedMessage = "The plan '21' version id '57' does not have a required target cpm.";
            var job = new PlanPricingJob { Id = 1, PlanVersionId = 57 };
            var plan = new PlanDto { Id = 21, TargetCPM = null };

            _PlanRepository.Setup(s => s.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(job);
            _PlanRepository.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            var testClass = _GetTestClass();

            // Act
            var caught = Assert.Throws<InvalidOperationException>(() => testClass._GetValidatedPlanAndPricingJob(planId, out var resultPlan, out var resultJob));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScxData()
        {
            // Arrange
            int planId = 21;
            var job = new PlanPricingJob { Id = 1, PlanVersionId = 57 };
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
            var jobParams = new PlanPricingParametersDto { Margin = 20 };
            var jobResult = new PlanPricingAllocationResult
            {
                SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        StationInventoryManifestId = 10,
                        ContractMediaWeek = new MediaWeek { Id = 871 },
                        StandardDaypart = new StandardDaypartDto { Id = 1 },
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                        }
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        StationInventoryManifestId = 10,
                        ContractMediaWeek = new MediaWeek { Id = 872 },
                        StandardDaypart = new StandardDaypartDto { Id = 1 },
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
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

            _PlanRepository.Setup(s => s.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(job);
            _PlanRepository.Setup(s => s.GetLatestParametersForPlanPricingJob(It.IsAny<int>()))
                .Returns(jobParams);
            _PlanRepository.Setup(s => s.GetPricingApiResultsByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>(), It.IsAny<PostingTypeEnum>()))
                .Returns(jobResult);
            _PlanRepository.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);
            _InventoryRepository.Setup(s => s.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(inventory);

            var generated = new DateTime(2020, 10, 17, 12, 34, 56);
            var testClass = _GetTestClass();

            // Act
            var resultQ = testClass.GetScxData(planId, generated, SpotAllocationModelMode.Quality, PostingTypeEnum.NSI);
            var resultE = testClass.GetScxData(planId, generated, SpotAllocationModelMode.Efficiency, PostingTypeEnum.NSI);
            var resultF = testClass.GetScxData(planId, generated, SpotAllocationModelMode.Floor, PostingTypeEnum.NSI);

            var result = new
            {
                Quality = resultQ,
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
            var planPricingAllocatedSpots = Builder<PlanPricingAllocationResult>.CreateNew()
                .With(x => x.Spots = Builder<PlanPricingAllocatedSpot>.CreateListOfSize(2)
                        .All()
                        .With(y => y.SpotFrequencies = Builder<SpotFrequency>.CreateListOfSize(2).Build().ToList())
                        .Build()
                        .ToList())
                .Build();

            var controlledAllocatedSpot = planPricingAllocatedSpots.Spots[1].SpotFrequencies[1];
            controlledAllocatedSpot.SpotLengthId = 2;
            controlledAllocatedSpot.Impressions = 100;

            var testClass = _GetTestClass();

            // Act
            testClass._UnEquivalizeSpots(planPricingAllocatedSpots);

            // Assert
            Assert.AreEqual(50, controlledAllocatedSpot.Impressions);
        }
    }
}
