using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.BusinessEngines.PlanPricingInventoryEngine;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram.ManifestDaypart;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Plans
{
    [TestFixture]
    [Category("short_running")]
    public class PlanPricingServiceUnitTests
    {
        private Mock<ISpotLengthEngine> _SpotLengthEngineMock;
        private Mock<IPricingApiClient> _PricingApiClientMock;
        private Mock<IBackgroundJobClient> _BackgroundJobClientMock;
        private Mock<IPlanPricingInventoryEngine> _PlanPricingInventoryEngineMock;
        private Mock<IBroadcastLockingManagerApplicationService> _BroadcastLockingManagerApplicationServiceMock;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IPlanRepository> _PlanRepositoryMock;
        private Mock<IInventoryRepository> _InventoryRepositoryMock;
        private Mock<IDaypartCache> _DaypartCacheMock;
        private Mock<IMarketCoverageRepository> _MarketCoverageRepositoryMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private Mock<IStationProgramRepository> _StationProgramRepositoryMock;
        private Mock<IMarketRepository> _MarketRepositoryMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IDaypartDefaultRepository> _DaypartDefaultRepositoryMock;
        private Mock<ICampaignRepository> _CampaignRepositoryMock;
        private Mock<IWeeklyBreakdownEngine> _WeeklyBreakdownEngineMock;
        private Mock<IPlanPricingBandCalculationEngine> _PlanPricingBandCalculationEngine;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _PricingApiClientMock = new Mock<IPricingApiClient>();
            _BackgroundJobClientMock = new Mock<IBackgroundJobClient>();
            _PlanPricingInventoryEngineMock = new Mock<IPlanPricingInventoryEngine>();
            _BroadcastLockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();
            _InventoryRepositoryMock = new Mock<IInventoryRepository>();
            _DaypartCacheMock = new Mock<IDaypartCache>();
            _MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _StationProgramRepositoryMock = new Mock<IStationProgramRepository>();
            _MarketRepositoryMock = new Mock<IMarketRepository>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _DaypartDefaultRepositoryMock = new Mock<IDaypartDefaultRepository>();
            _CampaignRepositoryMock = new Mock<ICampaignRepository>();
            _WeeklyBreakdownEngineMock = new Mock<IWeeklyBreakdownEngine>();
            _PlanPricingBandCalculationEngine = new Mock<IPlanPricingBandCalculationEngine>();

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 2, 4, 15, 31, 27));

            _DaypartDefaultRepositoryMock
                .Setup(x => x.GetAllDaypartDefaults())
                .Returns(_GetDaypartDefaults());

            _InventoryRepositoryMock
                .Setup(x => x.GetInventorySources())
                .Returns(_GetInventorySources());

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ICampaignRepository>())
                .Returns(_CampaignRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanRepository>())
                .Returns(_PlanRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IMarketCoverageRepository>())
                .Returns(_MarketCoverageRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IStationProgramRepository>())
                .Returns(_StationProgramRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IDaypartDefaultRepository>())
                .Returns(_DaypartDefaultRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IMarketRepository>())
                .Returns(_MarketRepositoryMock.Object);

            var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
            SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);
        }

        private List<DaypartDefaultDto> _GetDaypartDefaults()
        {
            return new List<DaypartDefaultDto>
            {
                new DaypartDefaultDto { Id = 1, Code = "EMN", FullName = "Early Morning News"},
                new DaypartDefaultDto { Id = 2, Code = "MDN", FullName = "Midday News"},
                new DaypartDefaultDto { Id = 3, Code = "EN", FullName = "Evening News"},
                new DaypartDefaultDto { Id = 4, Code = "LN", FullName = "Late News"},
                new DaypartDefaultDto { Id = 5, Code = "ENLN", FullName = "Evening News/Late News"},
                new DaypartDefaultDto { Id = 6, Code = "EF", FullName = "Early Fringe"},
                new DaypartDefaultDto { Id = 7, Code = "PA", FullName = "Prime Access"},
                new DaypartDefaultDto { Id = 8, Code = "PT", FullName = "Prime"},
                new DaypartDefaultDto { Id = 9, Code = "LF", FullName = "Late Fringe"},
                new DaypartDefaultDto { Id = 10, Code = "SYN", FullName = "Total Day Syndication"},
                new DaypartDefaultDto { Id = 11, Code = "OVN", FullName = "Overnights"},
                new DaypartDefaultDto { Id = 12, Code = "DAY", FullName = "Daytime"},
                new DaypartDefaultDto { Id = 14, Code = "EM", FullName = "Early morning"},
                new DaypartDefaultDto { Id = 15, Code = "AMN", FullName = "AM News"},
                new DaypartDefaultDto { Id = 16, Code = "PMN", FullName = "PM News"},
                new DaypartDefaultDto { Id = 17, Code = "TDN", FullName = "Total Day News"},
                new DaypartDefaultDto { Id = 19, Code = "ROSS", FullName = "ROS Syndication"},
                new DaypartDefaultDto { Id = 20, Code = "SPORTS", FullName = "ROS Sports"},
                new DaypartDefaultDto { Id = 21, Code = "ROSP", FullName = "ROS Programming"},
                new DaypartDefaultDto { Id = 22, Code = "TDNS", FullName = "Total Day News and Syndication"},
            };
        }

        [Test]
        public void UsesCurrentPlanVersion_WhenVersionIsNotSpecified_OnPricingResultsReportGeneration()
        {
            // Arrange
            const int planId = 1;
            var planVersionNumber = (int?)null;

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    VersionNumber = 3,
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanVersionId(It.IsAny<int>()))
                .Returns(new List<PlanPricingAllocatedSpot>());

            _InventoryRepositoryMock
                .Setup(x => x.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(new List<StationInventoryManifest>());

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, Program>());

            _MarketRepositoryMock
                .Setup(x => x.GetMarketDtos())
                .Returns(new List<LookupDto>());

            var service = _GetService();

            // Act
            service.GetPricingResultsReportData(planId, planVersionNumber);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlanVersionIdByVersionNumber(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
        }

        [Test]
        public void UsesPassedPlanVersion_WhenVersionIsSpecified_OnPricingResultsReportGeneration()
        {
            // Arrange
            const int planId = 1;
            var planVersionNumber = 2;
            var planVersionId = 5;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanVersionIdByVersionNumber(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(planVersionId);

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    VersionId = planVersionId,
                    VersionNumber = planVersionNumber,
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanVersionId(It.IsAny<int>()))
                .Returns(new List<PlanPricingAllocatedSpot>());

            _InventoryRepositoryMock
                .Setup(x => x.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(new List<StationInventoryManifest>());

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, Program>());

            _MarketRepositoryMock
                .Setup(x => x.GetMarketDtos())
                .Returns(new List<LookupDto>());

            var service = _GetService();

            // Act
            service.GetPricingResultsReportData(planId, planVersionNumber);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlanVersionIdByVersionNumber(planId, planVersionNumber), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, planVersionId), Times.Once);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PricingResultsReportGeneration()
        {
            // Arrange
            const int planId = 1;
            var planVersionNumber = 2;
            var planVersionId = 5;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanVersionIdByVersionNumber(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(planVersionId);

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    VersionId = planVersionId,
                    VersionNumber = planVersionNumber,
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            MediaWeekId = 7
                        },
                        new WeeklyBreakdownWeek
                        {
                            MediaWeekId = 9
                        },
                        new WeeklyBreakdownWeek
                        {
                            MediaWeekId = 8,
                        },
                        new WeeklyBreakdownWeek
                        {
                            MediaWeekId = 10
                        },
                        new WeeklyBreakdownWeek
                        {
                            MediaWeekId = 11
                        }
                    }
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetWeekNumberByMediaWeekDictionary(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new Dictionary<int, int>
                {
                    { 7, 1 },
                    { 9, 3 },
                    { 8, 2 },
                    { 10, 4 },
                    { 11, 5 }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanVersionId(It.IsAny<int>()))
                .Returns(new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Impressions = 1000,
                        SpotCost = 10,
                        Spots = 1,
                        StationInventoryManifestId = 1,
                        StandardDaypart = new DaypartDefaultDto
                        {
                            Code = "EF"
                        },
                        ContractMediaWeek = new MediaWeek
                        {
                            Id = 7
                        },
                        InventoryMediaWeek = new MediaWeek
                        {
                            StartDate = new DateTime(2020, 4, 6),
                            EndDate = new DateTime(2020, 4, 12)
                        }
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Impressions = 800,
                        SpotCost = 8,
                        Spots = 2,
                        StationInventoryManifestId = 1,
                        StandardDaypart = new DaypartDefaultDto
                        {
                            Code = "EF"
                        },
                        ContractMediaWeek = new MediaWeek
                        {
                            Id = 8
                        },
                        InventoryMediaWeek = new MediaWeek
                        {
                            StartDate = new DateTime(2020, 4, 13),
                            EndDate = new DateTime(2020, 4, 19)
                        }
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Impressions = 500,
                        SpotCost = 5,
                        Spots = 3,
                        StationInventoryManifestId = 2,
                        StandardDaypart = new DaypartDefaultDto
                        {
                            Code = "PA"
                        },
                        ContractMediaWeek = new MediaWeek
                        {
                            Id = 9
                        },
                        InventoryMediaWeek = new MediaWeek
                        {
                            StartDate = new DateTime(2020, 4, 20),
                            EndDate = new DateTime(2020, 4, 26)
                        }
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Impressions = 400,
                        SpotCost = 3,
                        Spots = 4,
                        StationInventoryManifestId = 3,
                        StandardDaypart = new DaypartDefaultDto
                        {
                            Code = "PT"
                        },
                        ContractMediaWeek = new MediaWeek
                        {
                            Id = 11
                        },
                        InventoryMediaWeek = new MediaWeek
                        {
                            StartDate = new DateTime(2020, 4, 27),
                            EndDate = new DateTime(2020, 5, 3)
                        }
                    }
                });

            _InventoryRepositoryMock
                .Setup(x => x.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(new List<StationInventoryManifest>
                {
                    new StationInventoryManifest
                    {
                        Id = 1,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "WCSH",
                            MarketCode = 100
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 4,
                                Daypart = new DisplayDaypart
                                {
                                    StartTime = 54000,
                                    EndTime = 64799
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 2,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "WFUT",
                            MarketCode = 101
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 5,
                                Daypart = new DisplayDaypart
                                {
                                    StartTime = 64800,
                                    EndTime = 71999
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 3,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "EBNG",
                            MarketCode = 102
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 6,
                                Daypart = new DisplayDaypart
                                {
                                    StartTime = 72000,
                                    EndTime = 82799
                                }
                            }
                        }
                    }
                });

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, Program>
                {
                    {
                        4,
                        new Program
                        {
                            Name = "Parasite",
                            Genre = "Comedy",
                            ShowType = "Movie"
                        }
                    },
                    {
                        5,
                        new Program
                        {
                            Name = "Jojo Rabbit",
                            Genre = "Comedy",
                            ShowType = "Movie"
                        }
                    },
                    {
                        6,
                        new Program
                        {
                            Name = "Joker",
                            Genre = "Crime",
                            ShowType = "Movie"
                        }
                    }
                });

            _MarketRepositoryMock
                .Setup(x => x.GetMarketDtos())
                .Returns(new List<LookupDto>
                {
                    new LookupDto
                    {
                        Id = 100,
                        Display = "Portland-Auburn"
                    },
                    new LookupDto
                    {
                        Id = 101,
                        Display = "New York"
                    },
                    new LookupDto
                    {
                        Id = 102,
                        Display = "Binghamton"
                    }
                });

            var service = _GetService();

            // Act
            var result = service.GetPricingResultsReportData(planId, planVersionNumber);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePricingCpmTest()
        {
            var allocationResult = new PlanPricingAllocationResult
            {
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        Spots = 2,
                        SpotCost = 200,
                        Impressions = 10000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 2,
                        Spots = 4,
                        SpotCost = 300,
                        Impressions = 50000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 3,
                        Spots = 3,
                        SpotCost = 500,
                        Impressions = 20000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 4,
                        Spots = 1,
                        SpotCost = 100,
                        Impressions = 30000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 5,
                        Spots = 3,
                        SpotCost = 300,
                        Impressions = 10000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 6,
                        Spots = 2,
                        SpotCost = 400,
                        Impressions = 50000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 7,
                        Spots = 1,
                        SpotCost = 250,
                        Impressions = 20000,
                    }
                }
            };

            var estimates = new List<PricingEstimate>
            {
                new PricingEstimate
                {
                    InventorySourceId = 3,
                    InventorySourceType = InventorySourceTypeEnum.Barter,
                    Cost = 5000,
                    Impressions = 10000,
                    MediaWeekId  = 4
                }
            };

            var service = _GetService();

            var result = service._CalculatePricingCpm(allocationResult.Spots, estimates, 20);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePricingCpmTestMultipleSpots()
        {
            var allocationResult = new PlanPricingAllocationResult
            {
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        Spots = 10,
                        SpotCost = 200,                        
                        // Total cost = 2000
                        Impressions = 10000,
                        // Total impressions = 100000
                    },
                }
            };

            var estimates = new List<PricingEstimate>
            {
                new PricingEstimate
                {
                    InventorySourceId = 3,
                    InventorySourceType = InventorySourceTypeEnum.Barter,
                    Cost = 5000,
                    Impressions = 10000,
                    MediaWeekId  = 4
                }
            };

            // CPM = (2000 + 5000 /  10000 + 100000)  * 1000 = 63.636363...

            var service = _GetService();

            var result = service._CalculatePricingCpm(allocationResult.Spots, estimates, 0);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatePricingCpmWithNoSpots()
        {
            var estimates = new List<PricingEstimate>
            {
                new PricingEstimate
                {
                    InventorySourceId = 3,
                    InventorySourceType = InventorySourceTypeEnum.Barter,
                    Cost = 5000,
                    Impressions = 10000,
                    MediaWeekId  = 4
                }
            };

            var service = _GetService();

            var result = service._CalculatePricingCpm(new List<PlanPricingAllocatedSpot>(), estimates, 0);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        public void CanNotQueuePricingJobWhenThereIsOneActive(BackgroundJobProcessingStatus status)
        {
            const string expectedMessage = "The pricing model is already running for the plan";

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob { Status = status });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetNotUserBasedLockObjectForKey(It.IsAny<string>()))
                .Returns(new object());

            var service = _GetService();

            var exception = Assert.Throws<Exception>(() => service.QueuePricingJob(
                new PlanPricingParametersDto()
                , new DateTime(2019, 10, 23)
                , "test user"));

            Assert.AreEqual(expectedMessage, exception.Message);
        }

        private MarketCoverageDto _GetLatestMarketCoverages()
        {
            return new MarketCoverageDto
            {
                MarketCoverageFileId = 1,
                MarketCoveragesByMarketCode = new Dictionary<int, double>
                {
                    { 101, 0.101d },
                    { 100, 0.1d },
                    { 302, 0.302d }
                }
            };
        }

        private MarketCoverageDto _GetTop100Markets()
        {
            return new MarketCoverageDto
            {
                MarketCoverageFileId = 1,
                MarketCoveragesByMarketCode = new Dictionary<int, double>
                {
                    { 101, 0.101d },
                    { 100, 0.1d },
                    { 302, 0.302d },
                    { 403, 0.04743d },
                    { 202, 0.02942d },
                }
            };
        }

        private MarketCoverageDto _GetTop25Markets()
        {
            return new MarketCoverageDto
            {
                MarketCoverageFileId = 1,
                MarketCoveragesByMarketCode = new Dictionary<int, double>
                {
                    { 403, 0.04743d },
                    { 202, 0.02942d },
                }
            };
        }

        private MarketCoverageDto _GetTop50Markets()
        {
            return new MarketCoverageDto
            {
                MarketCoverageFileId = 1,
                MarketCoveragesByMarketCode = new Dictionary<int, double>
                {
                    { 100, 0.1d },
                    { 403, 0.04743d },
                    { 202, 0.02942d },
                }
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ThrowsException_WhenWrongUnitCapType_IsPassed_WhenRunningPricing()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

            var planParameters = _GetPlanPricingParametersDto();
            planParameters.UnitCapsType = UnitCapEnum.PerMonth;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CoverageGoalPercent = 80,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                        new PlanAvailableMarketDto
                        {
                            Id = 5,
                            MarketCode = 101,
                            ShareOfVoicePercent = 12
                        },
                        new PlanAvailableMarketDto
                        {
                            Id = 6,
                            MarketCode = 102,
                            ShareOfVoicePercent = 13
                        }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40
                        }
                    },
                    PricingParameters = planParameters,
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        }
                    }
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByWeek>
                {
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 150,
                        Budget = 15,
                        MediaWeekId = 100
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 250,
                        Budget = 15m,
                        MediaWeekId = 101
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 100,
                        Budget = 15m,
                        MediaWeekId = 102
                    }
                });

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate =>
                 {
                     // to skip row number 
                     jobUpdate.DiagnosticResult = jobUpdate.DiagnosticResult?.Substring(0, 80);
                     jobUpdates.Add(jobUpdate);
                 });

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates));
        }

        protected PlanPricingServiceUnitTestClass _GetService()
        {
            return new PlanPricingServiceUnitTestClass(
                _DataRepositoryFactoryMock.Object,
                _SpotLengthEngineMock.Object,
                _PricingApiClientMock.Object,
                _BackgroundJobClientMock.Object,
                _PlanPricingInventoryEngineMock.Object,
                _BroadcastLockingManagerApplicationServiceMock.Object,
                _DaypartCacheMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _DateTimeEngineMock.Object,
                _WeeklyBreakdownEngineMock.Object,
                _PlanPricingBandCalculationEngine.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculatesEstimates_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    AvailableMarkets = new List<PlanAvailableMarketDto>(),
                    PricingParameters = _GetPlanPricingParametersDto(),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } }
                });

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(_GetPlanPricingInventoryPrograms());

            List<PricingEstimate> passedParameters = null;
            _PlanRepositoryMock
                .Setup(x => x.SavePlanPricingEstimates(It.IsAny<int>(), It.IsAny<List<PricingEstimate>>()))
                .Callback<int, List<PricingEstimate>>((p, p1) => { passedParameters = p1; });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            _PlanPricingInventoryEngineMock
                .Verify(x => x.GetInventoryForPlan(
                    It.IsAny<PlanDto>(),
                    It.IsAny<ProgramInventoryOptionalParametersDto>(),
                    It.Is<IEnumerable<int>>(list => list.SequenceEqual(new List<int> { 3, 5, 6, 7, 10, 11, 12 })),
                    It.IsAny<PlanPricingJobDiagnostic>()), Times.Once);

            _PlanPricingInventoryEngineMock
                .Verify(x => x.GetInventoryForPlan(
                    It.IsAny<PlanDto>(),
                    It.IsAny<ProgramInventoryOptionalParametersDto>(),
                    It.Is<IEnumerable<int>>(list => list.SequenceEqual(new List<int> { 17, 18, 19, 20, 21, 22, 23, 24, 25 })),
                    It.IsAny<PlanPricingJobDiagnostic>()), Times.Once);

            _PlanRepositoryMock
                .Verify(x => x.SavePlanPricingEstimates(jobId, It.IsAny<List<PricingEstimate>>()), Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        private PlanPricingParametersDto _GetPlanPricingParametersDto()
        {
            return new PlanPricingParametersDto
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
                MarketGroup = PricingMarketGroupEnum.All,
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

        private List<InventorySource> _GetInventorySources()
        {
            return new List<InventorySource>
            {
                new InventorySource
                {
                    Id = 1,
                    Name = "Open Market",
                    InventoryType = InventorySourceTypeEnum.OpenMarket
                },
                new InventorySource
                {
                    Id = 3,
                    Name = "TVB",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 4,
                    Name = "TTWN",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 5,
                    Name = "CNN",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 6,
                    Name = "Sinclair",
                    InventoryType = InventorySourceTypeEnum.ProprietaryOAndO
                },
                new InventorySource
                {
                    Id = 7,
                    Name = "LilaMax",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 8,
                    Name = "MLB",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 9,
                    Name = "Ference Media",
                    InventoryType = InventorySourceTypeEnum.Barter
                },
                new InventorySource
                {
                    Id = 10,
                    Name = "ABC O&O",
                    InventoryType = InventorySourceTypeEnum.ProprietaryOAndO
                },
                new InventorySource
                {
                    Id = 11,
                    Name = "NBC O&O",
                    InventoryType = InventorySourceTypeEnum.ProprietaryOAndO
                },
                new InventorySource
                {
                    Id = 12,
                    Name = "KATZ",
                    InventoryType = InventorySourceTypeEnum.ProprietaryOAndO
                },
                new InventorySource
                {
                    Id = 13,
                    Name = "20th Century Fox (Twentieth Century)",
                    InventoryType = InventorySourceTypeEnum.Syndication
                },
                new InventorySource
                {
                    Id = 14,
                    Name = "CBS Synd",
                    InventoryType = InventorySourceTypeEnum.Syndication
                },
                new InventorySource
                {
                    Id = 15,
                    Name = "NBCU Syn",
                    InventoryType = InventorySourceTypeEnum.Syndication
                },
                new InventorySource
                {
                    Id = 16,
                    Name = "WB Syn",
                    InventoryType = InventorySourceTypeEnum.Syndication
                },
                new InventorySource
                {
                    Id = 17,
                    Name = "Antenna TV",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 18,
                    Name = "Bounce",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 19,
                    Name = "BUZZR",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 20,
                    Name = "COZI",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 21,
                    Name = "Escape",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 22,
                    Name = "Grit",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 23,
                    Name = "HITV",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 24,
                    Name = "Laff",
                    InventoryType = InventorySourceTypeEnum.Diginet
                },
                new InventorySource
                {
                    Id = 25,
                    Name = "Me TV",
                    InventoryType = InventorySourceTypeEnum.Diginet
                }
            };
        }

        private List<PlanPricingInventoryProgram> _GetPlanPricingInventoryPrograms()
        {
            return new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "wnbc",
                        MarketCode = 101,
                    },
                    ProvidedImpressions = 1000,
                    ProjectedImpressions = 1100,
                    SpotCost = 50,
                    InventorySource = new InventorySource
                    {
                        Id = 3,
                        InventoryType = InventorySourceTypeEnum.Barter
                    },
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 1,
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "seinfeld",
                                Genre = "News"
                            }
                        }
                    },
                    ManifestWeeks = new List<ManifestWeek>
                    {
                        new ManifestWeek
                        {
                            Spots = 2,
                            ContractMediaWeekId = 100,
                        },
                        new ManifestWeek
                        {
                            Spots = 3,
                            ContractMediaWeekId = 101,
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 2,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "wabc",
                        MarketCode = 101,
                    },
                    ProvidedImpressions = 1000,
                    ProjectedImpressions = 1100,
                    SpotCost = 50,
                    InventorySource = new InventorySource
                    {
                        Id = 5,
                        InventoryType = InventorySourceTypeEnum.Barter
                    },
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 2
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "seinfeld",
                                Genre = "News"
                            }
                        }
                    },
                    ManifestWeeks = new List<ManifestWeek>
                    {
                        new ManifestWeek
                        {
                            Spots = 5,
                            ContractMediaWeekId = 100
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 3,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
                    },
                    ProvidedImpressions = 1000,
                    ProjectedImpressions = 1100,
                    SpotCost = 50,
                    InventorySource = new InventorySource
                    {
                        Id = 7,
                        InventoryType = InventorySourceTypeEnum.Barter
                    },
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 3
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "seinfeld",
                                Genre = "News"
                            }
                        }
                    },
                    ManifestWeeks = new List<ManifestWeek>
                    {
                        new ManifestWeek
                        {
                            Spots = 1,
                            ContractMediaWeekId = 100
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 4,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kabc",
                        MarketCode = 302,
                    },
                    ProvidedImpressions = 3000,
                    ProjectedImpressions = 2900,
                    SpotCost = 130,
                    InventorySource = new InventorySource
                    {
                        Id = 10,
                        InventoryType = InventorySourceTypeEnum.ProprietaryOAndO
                    },
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 4
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "seinfeld",
                                Genre = "News"
                            }
                        }
                    },
                    ManifestWeeks = new List<ManifestWeek>
                    {
                        new ManifestWeek
                        {
                            Spots = 2,
                            ContractMediaWeekId = 100
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 5,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "wnbc",
                        MarketCode = 101,
                    },
                    InventorySource = new InventorySource
                    {
                        Id = 13,
                        InventoryType = InventorySourceTypeEnum.Syndication
                    },
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 5
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "Good morning america",
                                    Genre = "Early News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "Good morning america",
                                Genre = "Early News"
                            }
                        }
                    },
                    ManifestWeeks = new List<ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 6,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "wabc",
                        MarketCode = 101,
                    },
                    InventorySource = new InventorySource
                    {
                        Id = 17,
                        InventoryType = InventorySourceTypeEnum.Diginet
                    },
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 6
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "Good morning america",
                                    Genre = "Early News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "Good morning america",
                                Genre = "Early News"
                            }
                        }
                    },
                    ManifestWeeks = new List<ManifestWeek>()
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 7,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
                    },
                    ProvidedImpressions = null,
                    ProjectedImpressions = 700,
                    SpotCost = 40,
                    InventorySource = new InventorySource
                    {
                        Id = 3,
                        InventoryType = InventorySourceTypeEnum.Barter
                    },
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 7
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "Good morning america",
                                    Genre = "Early News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "Good morning america",
                                Genre = "Early News"
                            }
                        }
                    },
                    ManifestWeeks = new List<ManifestWeek>
                    {
                        new ManifestWeek
                        {
                            Spots = 6,
                            ContractMediaWeekId = 100,
                        },
                        new ManifestWeek
                        {
                            Spots = 7,
                            ContractMediaWeekId = 101,
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 7,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
                    },
                    ProvidedImpressions = null,
                    ProjectedImpressions = 700,
                    SpotCost = 40,
                    InventorySource = new InventorySource
                    {
                        Id = 18,
                        InventoryType = InventorySourceTypeEnum.Diginet
                    },
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 7
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "Good morning america",
                                    Genre = "Early News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "Good morning america",
                                Genre = "Early News"
                            }
                        }
                    },
                    ManifestWeeks = new List<ManifestWeek>
                    {
                        new ManifestWeek
                        {
                            Spots = 1,
                            ContractMediaWeekId = 100,
                        },
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 7,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
                    },
                    ProvidedImpressions = null,
                    ProjectedImpressions = 700,
                    SpotCost = 40,
                    InventorySource = new InventorySource
                    {
                        Id = 19,
                        InventoryType = InventorySourceTypeEnum.Diginet
                    },
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 7
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "Good morning america",
                                    Genre = "Early News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "Good morning america",
                                Genre = "Early News"
                            }
                        }
                    },
                    ManifestWeeks = new List<ManifestWeek>
                    {
                        new ManifestWeek
                        {
                            Spots = 1,
                            ContractMediaWeekId = 100,
                        }
                    }
                }
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CancelsPricing_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;
            var cancellationTokenSource = new CancellationTokenSource();

            var parameters = _GetPlanPricingParametersDto();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CoverageGoalPercent = 80,
                    AvailableMarkets = new List<PlanAvailableMarketDto>(),
                    PricingParameters = _GetPlanPricingParametersDto()
                });

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(_GetPlanPricingInventoryPrograms());

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate => jobUpdates.Add(jobUpdate));

            var service = _GetService();

            // Act
            var task = Task.Run(() => service.RunPricingJob(parameters, jobId, cancellationTokenSource.Token));
            cancellationTokenSource.Cancel();
            task.GetAwaiter().GetResult();

            // Assert
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "DiagnosticResult");
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PassesOnlyChosenInventorySourceTypes_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;

            StubbedConfigurationWebApiClient.RunTimeParameters["EnableOpenMarketInventoryForPricingModel"] = "False";
            StubbedConfigurationWebApiClient.RunTimeParameters["EnableBarterInventoryForPricingModel"] = "True";
            StubbedConfigurationWebApiClient.RunTimeParameters["EnableProprietaryOAndOInventoryForPricingModel"] = "False";

            try
            {
                var parameters = _GetPlanPricingParametersDto();

                _PlanRepositoryMock
                    .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                    .Returns(new PlanPricingJob());

                _PlanRepositoryMock
                    .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                    .Returns(new PlanDto
                    {
                        CoverageGoalPercent = 80,
                        CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                        AvailableMarkets = new List<PlanAvailableMarketDto>(),
                        PricingParameters = _GetPlanPricingParametersDto()
                    });

                var passedInventorySourceIds = new List<IEnumerable<int>>();
                _PlanPricingInventoryEngineMock
                    .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                    .Callback<PlanDto, ProgramInventoryOptionalParametersDto, IEnumerable<int>, PlanPricingJobDiagnostic>((p1, p2, p3, p4) =>
                    {
                        passedInventorySourceIds.Add(p3);
                    })
                    .Returns(_GetPlanPricingInventoryPrograms());

                var service = _GetService();

                // Act
                service.RunPricingJob(parameters, jobId, CancellationToken.None);

                // Assert
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedInventorySourceIds));
            }
            finally
            {
                StubbedConfigurationWebApiClient.RunTimeParameters["EnableOpenMarketInventoryForPricingModel"] = "True";
                StubbedConfigurationWebApiClient.RunTimeParameters["EnableBarterInventoryForPricingModel"] = "True";
                StubbedConfigurationWebApiClient.RunTimeParameters["EnableProprietaryOAndOInventoryForPricingModel"] = "True";
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void NoInventoryFound_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CoverageGoalPercent = 80,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    AvailableMarkets = new List<PlanAvailableMarketDto>(),
                    PricingParameters = _GetPlanPricingParametersDto()
                });

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(new List<PlanPricingInventoryProgram>());

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate =>
                {
                    // to skip row number 
                    jobUpdate.DiagnosticResult = jobUpdate.DiagnosticResult?.Substring(0, 60);
                    jobUpdates.Add(jobUpdate);
                });

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GoalsFulfilledByProprietaryInventory_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByWeek>
                {
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 50,
                        Budget = 5,
                        MediaWeekId = 100
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 250,
                        Budget = 0.5m,
                        MediaWeekId = 101
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 0,
                        Budget = 0.5m,
                        MediaWeekId = 102
                    }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CoverageGoalPercent = 80,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    AvailableMarkets = new List<PlanAvailableMarketDto>(),
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 50,
                            WeeklyBudget = 5,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 0.5m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 0.5m,
                            MediaWeekId = 102
                        }
                    }
                });

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        Station = new DisplayBroadcastStation
                        {
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    }
                });

            var savedAggregationResults = new List<PlanPricingResultBaseDto>();
            _PlanRepositoryMock
                .Setup(x => x.SavePricingAggregateResults(It.IsAny<PlanPricingResultBaseDto>()))
                .Callback<PlanPricingResultBaseDto>((p1) => savedAggregationResults.Add(p1));

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedAggregationResults));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SendsRequestToDSAPI_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CoverageGoalPercent = 80,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                        new PlanAvailableMarketDto
                        {
                            Id = 5,
                            MarketCode = 101,
                            ShareOfVoicePercent = 12
                        },
                        new PlanAvailableMarketDto
                        {
                            Id = 6,
                            MarketCode = 102,
                            ShareOfVoicePercent = 13
                        }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40
                        }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByWeek>
                {
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 150,
                        Budget = 15,
                        MediaWeekId = 100
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 250,
                        Budget = 15m,
                        MediaWeekId = 101
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 100,
                        Budget = 15m,
                        MediaWeekId = 102
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 0,
                        Budget = 15m,
                        MediaWeekId = 103
                    }
                });

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 0,
                        ProjectedImpressions = 0,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 4,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 0,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    }
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            var requests = new List<PlanPricingApiRequestDto>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Callback<PlanPricingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PricingModelRetunedErrors()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CoverageGoalPercent = 80,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                        new PlanAvailableMarketDto
                        {
                            Id = 5,
                            MarketCode = 101,
                            ShareOfVoicePercent = 12
                        },
                        new PlanAvailableMarketDto
                        {
                            Id = 6,
                            MarketCode = 102,
                            ShareOfVoicePercent = 13
                        }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40
                        }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByWeek>
                {
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 150,
                        Budget = 15,
                        MediaWeekId = 100
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 250,
                        Budget = 15m,
                        MediaWeekId = 101
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 100,
                        Budget = 15m,
                        MediaWeekId = 102
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 0,
                        Budget = 15m,
                        MediaWeekId = 103
                    }
                });

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 0,
                        ProjectedImpressions = 0,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 4,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 0,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    }
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Returns(new PlanPricingApiSpotsResponseDto
                {
                    Error = new PlanPricingApiSpotsErrorDto
                    {
                        Messages = new List<string>
                        {
                            "Message 1",
                            "Error 2"
                        }
                    }
                });

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate =>
                {
                    // to skip row number 
                    jobUpdate.DiagnosticResult = jobUpdate.DiagnosticResult?.Substring(0, 60);
                    jobUpdates.Add(jobUpdate);
                });

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PricingModelRetuned_UnknownSpot()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CoverageGoalPercent = 80,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                        new PlanAvailableMarketDto
                        {
                            Id = 5,
                            MarketCode = 101,
                            ShareOfVoicePercent = 12
                        },
                        new PlanAvailableMarketDto
                        {
                            Id = 6,
                            MarketCode = 102,
                            ShareOfVoicePercent = 13
                        }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40
                        }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByWeek>
                {
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 150,
                        Budget = 15,
                        MediaWeekId = 100
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 250,
                        Budget = 15m,
                        MediaWeekId = 101
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 100,
                        Budget = 15m,
                        MediaWeekId = 102
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 0,
                        Budget = 15m,
                        MediaWeekId = 103
                    }
                });

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 0,
                        ProjectedImpressions = 0,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 4,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 0,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    }
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Returns(new PlanPricingApiSpotsResponseDto
                {
                    Results = new List<PlanPricingApiSpotsResultDto>
                    {
                        new PlanPricingApiSpotsResultDto
                        {
                            ManifestId = 1000,
                            MediaWeekId = 1000,
                            Frequency = 5
                        }
                    }
                });

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate =>
                {
                    // to skip row number 
                    jobUpdate.DiagnosticResult = jobUpdate.DiagnosticResult?.Substring(0, 100);
                    jobUpdates.Add(jobUpdate);
                });

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PricingModelRetuned_NoSpots()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CoverageGoalPercent = 80,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                        new PlanAvailableMarketDto
                        {
                            Id = 5,
                            MarketCode = 101,
                            ShareOfVoicePercent = 12
                        },
                        new PlanAvailableMarketDto
                        {
                            Id = 6,
                            MarketCode = 102,
                            ShareOfVoicePercent = 13
                        }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40
                        }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByWeek>
                {
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 150,
                        Budget = 15,
                        MediaWeekId = 100
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 250,
                        Budget = 15m,
                        MediaWeekId = 101
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 100,
                        Budget = 15m,
                        MediaWeekId = 102
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 0,
                        Budget = 15m,
                        MediaWeekId = 103
                    }
                });

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 0,
                        ProjectedImpressions = 0,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 4,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 0,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    }
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Returns(new PlanPricingApiSpotsResponseDto
                {
                    RequestId = "#q1w2e3",
                    Results = new List<PlanPricingApiSpotsResultDto>()
                });

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate =>
                {
                    // to skip row number 
                    jobUpdate.DiagnosticResult = jobUpdate.DiagnosticResult?.Substring(0, 100);
                    jobUpdates.Add(jobUpdate);
                });

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesPricingApiResults_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    VersionId = 77,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    CoverageGoalPercent = 80,
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                        new PlanAvailableMarketDto
                        {
                            Id = 5,
                            MarketCode = 101,
                            ShareOfVoicePercent = 12
                        },
                        new PlanAvailableMarketDto
                        {
                            Id = 6,
                            MarketCode = 102,
                            ShareOfVoicePercent = 13
                        }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40
                        }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByWeek>
                {
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 150,
                        Budget = 15,
                        MediaWeekId = 100
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 250,
                        Budget = 15m,
                        MediaWeekId = 101
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 100,
                        Budget = 15m,
                        MediaWeekId = 102
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 0,
                        Budget = 15m,
                        MediaWeekId = 103
                    }
                });

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                                InventoryMediaWeekId = 90
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                                InventoryMediaWeekId = 91
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                                InventoryMediaWeekId = 92
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                                InventoryMediaWeekId = 90
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 0,
                        ProjectedImpressions = 0,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 4,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 0,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    }
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Returns(new PlanPricingApiSpotsResponseDto
                {
                    RequestId = "#q1w2e3",
                    Results = new List<PlanPricingApiSpotsResultDto>
                    {
                        new PlanPricingApiSpotsResultDto
                        {
                            ManifestId = 1,
                            MediaWeekId = 100,
                            Frequency = 1
                        },
                        new PlanPricingApiSpotsResultDto
                        {
                            ManifestId = 1,
                            MediaWeekId = 101,
                            Frequency = 2
                        },
                        new PlanPricingApiSpotsResultDto
                        {
                            ManifestId = 2,
                            MediaWeekId = 100,
                            Frequency = 3
                        }
                    }
                });

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(x => x.GetMediaWeekById(It.IsAny<int>()))
                .Returns<int>(weekId => new MediaWeek
                {
                    Id = weekId
                });

            var passedParameters = new List<PlanPricingAllocationResult>();
            _PlanRepositoryMock
                .Setup(x => x.SavePricingApiResults(It.IsAny<PlanPricingAllocationResult>()))
                .Callback<PlanPricingAllocationResult>(p => passedParameters.Add(p));

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesPricingAggregateResults_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    VersionId = 77,
                    CoverageGoalPercent = 80,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                        new PlanAvailableMarketDto
                        {
                            Id = 5,
                            MarketCode = 101,
                            ShareOfVoicePercent = 12
                        },
                        new PlanAvailableMarketDto
                        {
                            Id = 6,
                            MarketCode = 102,
                            ShareOfVoicePercent = 13
                        }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40
                        }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(new List<WeeklyBreakdownByWeek>
                {
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 150,
                        Budget = 15,
                        MediaWeekId = 100
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 250,
                        Budget = 15m,
                        MediaWeekId = 101
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 100,
                        Budget = 15m,
                        MediaWeekId = 102
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 0,
                        Budget = 15m,
                        MediaWeekId = 103
                    }
                });

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                })
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld_2",
                                        Genre = "Sport"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld_2",
                                    Genre = "Sport"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                                InventoryMediaWeekId = 90
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                                InventoryMediaWeekId = 91
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                                InventoryMediaWeekId = 92
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                                InventoryMediaWeekId = 90
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 0,
                        ProjectedImpressions = 0,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 4,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 0,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    }
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Returns(new PlanPricingApiSpotsResponseDto
                {
                    RequestId = "#q1w2e3",
                    Results = new List<PlanPricingApiSpotsResultDto>
                    {
                        new PlanPricingApiSpotsResultDto
                        {
                            ManifestId = 1,
                            MediaWeekId = 100,
                            Frequency = 1
                        },
                        new PlanPricingApiSpotsResultDto
                        {
                            ManifestId = 1,
                            MediaWeekId = 101,
                            Frequency = 2
                        },
                        new PlanPricingApiSpotsResultDto
                        {
                            ManifestId = 2,
                            MediaWeekId = 100,
                            Frequency = 3
                        }
                    }
                });

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(x => x.GetMediaWeekById(It.IsAny<int>()))
                .Returns<int>(weekId => new MediaWeek
                {
                    Id = weekId
                });

            var passedParameters = new List<PlanPricingResultBaseDto>();
            _PlanRepositoryMock
                .Setup(x => x.SavePricingAggregateResults(It.IsAny<PlanPricingResultBaseDto>()))
                .Callback<PlanPricingResultBaseDto>(p => passedParameters.Add(p));

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        public void IsPricingModelRunning_ReturnsFalse_WhenNoJobsFound()
        {
            // Arrange
            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns((PlanPricingJob)null);

            var service = _GetService();

            // Act
            var result = service.IsPricingModelRunningForPlan(planId: 5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Queued, true)]
        [TestCase(BackgroundJobProcessingStatus.Processing, true)]
        [TestCase(BackgroundJobProcessingStatus.Succeeded, false)]
        [TestCase(BackgroundJobProcessingStatus.Failed, false)]
        [TestCase(BackgroundJobProcessingStatus.Canceled, false)]
        public void IsPricingModelRunning_ChecksJobStatus(BackgroundJobProcessingStatus status, bool expectedResult)
        {
            // Arrange
            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = status
                });

            var service = _GetService();

            // Act
            var actualResult = service.IsPricingModelRunningForPlan(planId: 5);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanPricingDefaultsTest()
        {
            // Arrange
            var service = _GetService();

            // Act
            var result = service.GetPlanPricingDefaults();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetUnitCapsTest()
        {
            // Arrange
            var service = _GetService();

            // Act
            var unitCaps = service.GetUnitCaps();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(unitCaps));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AddsNewPricingJob_WhenQueuesPricing()
        {
            // Arrange
            const string user = "test user";
            const int jobId = 5;

            var now = new DateTime(2019, 10, 23);

            var parameters = new PlanPricingParametersDto
            {
                PlanId = 222,
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
                Currency = PlanCurrenciesEnum.Impressions,
                CPP = 1.1m,
                DeliveryRatingPoints = 1.3,
                Margin = 14,
                InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                {
                    new PlanPricingInventorySourceDto{ Id = 3, Percentage = 12 },
                    new PlanPricingInventorySourceDto{ Id = 5, Percentage = 13 },
                    new PlanPricingInventorySourceDto{ Id = 6, Percentage = 14 },
                    new PlanPricingInventorySourceDto{ Id = 7, Percentage = 15 },
                    new PlanPricingInventorySourceDto{ Id = 10, Percentage = 16 },
                    new PlanPricingInventorySourceDto{ Id = 11, Percentage = 17 },
                    new PlanPricingInventorySourceDto{ Id = 12, Percentage = 8 },
                },
                InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                {
                    new PlanPricingInventorySourceTypeDto { Id = 5, Percentage = 3 }
                }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = 17,
                    VersionId = 77,
                    CoverageGoalPercent = 80,
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                                    new PlanAvailableMarketDto
                                    {
                                        Id = 5,
                                        MarketCode = 101,
                                        ShareOfVoicePercent = 12
                                    },
                                    new PlanAvailableMarketDto
                                    {
                                        Id = 6,
                                        MarketCode = 102,
                                        ShareOfVoicePercent = 13
                                    }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                                    new PlanDaypartDto
                                    {
                                        DaypartCodeId = 15,
                                        WeightingGoalPercent = 60
                                    },
                                    new PlanDaypartDto
                                    {
                                        DaypartCodeId = 16,
                                        WeightingGoalPercent = 40
                                    }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob { Status = BackgroundJobProcessingStatus.Succeeded });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetNotUserBasedLockObjectForKey(It.IsAny<string>()))
                .Returns(new object());

            var passedParameters = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.AddPlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(p => passedParameters.Add(p))
                .Returns(jobId);

            var service = _GetService();

            // Act
            service.QueuePricingJob(parameters, now, user);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesPricingParameters_WhenQueuesPricing()
        {
            // Arrange
            const string user = "test user";
            const int jobId = 5;

            var now = new DateTime(2019, 10, 23);

            var parameters = new PlanPricingParametersDto
            {
                PlanId = 222,
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
                Currency = PlanCurrenciesEnum.Impressions,
                CPP = 1.1m,
                DeliveryRatingPoints = 1.3,
                Margin = 14,
                MarketGroup = PricingMarketGroupEnum.Top100,
                InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                {
                    new PlanPricingInventorySourceDto{ Id = 3, Percentage = 12 },
                    new PlanPricingInventorySourceDto{ Id = 5, Percentage = 13 },
                    new PlanPricingInventorySourceDto{ Id = 6, Percentage = 14 },
                    new PlanPricingInventorySourceDto{ Id = 7, Percentage = 15 },
                    new PlanPricingInventorySourceDto{ Id = 10, Percentage = 16 },
                    new PlanPricingInventorySourceDto{ Id = 11, Percentage = 17 },
                    new PlanPricingInventorySourceDto{ Id = 12, Percentage = 8 },
                },
                InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                {
                    new PlanPricingInventorySourceTypeDto { Id = 5, Percentage = 3 }
                }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = 17,
                    VersionId = 77,
                    CoverageGoalPercent = 80,
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                                    new PlanAvailableMarketDto
                                    {
                                        Id = 5,
                                        MarketCode = 101,
                                        ShareOfVoicePercent = 12
                                    },
                                    new PlanAvailableMarketDto
                                    {
                                        Id = 6,
                                        MarketCode = 102,
                                        ShareOfVoicePercent = 13
                                    }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                                    new PlanDaypartDto
                                    {
                                        DaypartCodeId = 15,
                                        WeightingGoalPercent = 60
                                    },
                                    new PlanDaypartDto
                                    {
                                        DaypartCodeId = 16,
                                        WeightingGoalPercent = 40
                                    }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob { Status = BackgroundJobProcessingStatus.Succeeded });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetNotUserBasedLockObjectForKey(It.IsAny<string>()))
                .Returns(new object());

            _PlanRepositoryMock
                .Setup(x => x.AddPlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Returns(jobId);

            var passedParameters = new List<PlanPricingParametersDto>();
            _PlanRepositoryMock
                .Setup(x => x.SavePlanPricingParameters(It.IsAny<PlanPricingParametersDto>()))
                .Callback<PlanPricingParametersDto>(p => passedParameters.Add(p));

            var service = _GetService();

            // Act
            service.QueuePricingJob(parameters, now, user);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdatesCampaignLastModified_WhenQueuesPricing()
        {
            // Arrange
            const string user = "test user";
            const int jobId = 5;

            var now = new DateTime(2019, 10, 23);

            var parameters = new PlanPricingParametersDto
            {
                PlanId = 222,
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
                Currency = PlanCurrenciesEnum.Impressions,
                CPP = 1.1m,
                DeliveryRatingPoints = 1.3,
                Margin = 14,
                InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                {
                    new PlanPricingInventorySourceDto{ Id = 3, Percentage = 12 },
                    new PlanPricingInventorySourceDto{ Id = 5, Percentage = 13 },
                    new PlanPricingInventorySourceDto{ Id = 6, Percentage = 14 },
                    new PlanPricingInventorySourceDto{ Id = 7, Percentage = 15 },
                    new PlanPricingInventorySourceDto{ Id = 10, Percentage = 16 },
                    new PlanPricingInventorySourceDto{ Id = 11, Percentage = 17 },
                    new PlanPricingInventorySourceDto{ Id = 12, Percentage = 8 },
                },
                InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                {
                    new PlanPricingInventorySourceTypeDto { Id = 5, Percentage = 3 }
                }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = 17,
                    VersionId = 77,
                    CoverageGoalPercent = 80,
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                                    new PlanAvailableMarketDto
                                    {
                                        Id = 5,
                                        MarketCode = 101,
                                        ShareOfVoicePercent = 12
                                    },
                                    new PlanAvailableMarketDto
                                    {
                                        Id = 6,
                                        MarketCode = 102,
                                        ShareOfVoicePercent = 13
                                    }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                                    new PlanDaypartDto
                                    {
                                        DaypartCodeId = 15,
                                        WeightingGoalPercent = 60
                                    },
                                    new PlanDaypartDto
                                    {
                                        DaypartCodeId = 16,
                                        WeightingGoalPercent = 40
                                    }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob { Status = BackgroundJobProcessingStatus.Succeeded });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetNotUserBasedLockObjectForKey(It.IsAny<string>()))
                .Returns(new object());

            _PlanRepositoryMock
                .Setup(x => x.AddPlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Returns(jobId);

            var passedParameters = new List<object>();
            _CampaignRepositoryMock
                .Setup(x => x.UpdateCampaignLastModified(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback<int, DateTime, string>((campaignId, currentDate, currentUser) =>
                {
                    passedParameters.Add(new { campaignId, currentDate, currentUser });
                });

            var service = _GetService();

            // Act
            service.QueuePricingJob(parameters, now, user);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void QueuesHangfireJob_WhenQueuesPricing()
        {
            // Arrange
            const string user = "test user";
            const int jobId = 5;

            var now = new DateTime(2019, 10, 23);

            var parameters = new PlanPricingParametersDto
            {
                PlanId = 222,
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
                Currency = PlanCurrenciesEnum.Impressions,
                CPP = 1.1m,
                DeliveryRatingPoints = 1.3,
                Margin = 14,
                MarketGroup = PricingMarketGroupEnum.Top100,
                InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                {
                    new PlanPricingInventorySourceDto{ Id = 3, Percentage = 12 },
                    new PlanPricingInventorySourceDto{ Id = 5, Percentage = 13 },
                    new PlanPricingInventorySourceDto{ Id = 6, Percentage = 14 },
                    new PlanPricingInventorySourceDto{ Id = 7, Percentage = 15 },
                    new PlanPricingInventorySourceDto{ Id = 10, Percentage = 16 },
                    new PlanPricingInventorySourceDto{ Id = 11, Percentage = 17 },
                    new PlanPricingInventorySourceDto{ Id = 12, Percentage = 8 },
                },
                InventorySourceTypePercentages = new List<PlanPricingInventorySourceTypeDto>
                {
                    new PlanPricingInventorySourceTypeDto { Id = 5, Percentage = 3 }
                }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = 17,
                    VersionId = 77,
                    CoverageGoalPercent = 80,
                    AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                                    new PlanAvailableMarketDto
                                    {
                                        Id = 5,
                                        MarketCode = 101,
                                        ShareOfVoicePercent = 12
                                    },
                                    new PlanAvailableMarketDto
                                    {
                                        Id = 6,
                                        MarketCode = 102,
                                        ShareOfVoicePercent = 13
                                    }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                                    new PlanDaypartDto
                                    {
                                        DaypartCodeId = 15,
                                        WeightingGoalPercent = 60
                                    },
                                    new PlanDaypartDto
                                    {
                                        DaypartCodeId = 16,
                                        WeightingGoalPercent = 40
                                    }
                    },
                    PricingParameters = _GetPlanPricingParametersDto(),
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob { Status = BackgroundJobProcessingStatus.Succeeded });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetNotUserBasedLockObjectForKey(It.IsAny<string>()))
                .Returns(new object());

            _PlanRepositoryMock
                .Setup(x => x.AddPlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Returns(jobId);

            var passedParameters = new List<object>();
            _PlanRepositoryMock
                .Setup(x => x.UpdateJobHangfireId(It.IsAny<int>(), It.IsAny<string>()))
                .Callback<int, string>((pricingJobId, hangFireJobId) => passedParameters.Add(new { pricingJobId, hangFireJobId }));

            _BackgroundJobClientMock
                .Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
                .Callback<Job, IState>((job, state) => passedParameters.Add(new { job, state }))
                .Returns("hangfire job 35");

            var service = _GetService();

            // Act
            service.QueuePricingJob(parameters, now, user);

            // Assert
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(WaitHandle), "Handle");

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters, settings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCurrentPricingExecution_NoJobs()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns((PlanPricingJob)null);

            var service = _GetService();

            // Act
            var result = service.GetCurrentPricingExecution(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetCurrentPricingExecution_ShowsPricingErrorMessage()
        {
            // Arrange
            const int planId = 6;
            const string expectedMessage = "SomeErrorMessage";

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Failed,
                    DiagnosticResult = string.Empty,
                    ErrorMessage = expectedMessage
                });

            var service = _GetService();

            // Act
            var exception = Assert.Throws<Exception>(() => service.GetCurrentPricingExecution(planId));

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void GetCurrentPricingExecution_ShowsGenericErrorMessage()
        {
            // Arrange
            const int planId = 6;
            const string expectedMessage = "Error encountered while running Pricing Model, please contact a system administrator for help";

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Failed,
                    DiagnosticResult = "SomeDiagnosticResult",
                    ErrorMessage = "SomeErrorMessage"
                });

            var service = _GetService();

            // Act
            var exception = Assert.Throws<Exception>(() => service.GetCurrentPricingExecution(planId));

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCurrentPricingExecution_Queued()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Queued,
                    Id = 6,
                    HangfireJobId = "HangfireJobId 9",
                    PlanVersionId = 4,
                    Queued = new DateTime(2020, 2, 4, 11, 12, 13)
                });

            var service = _GetService();

            // Act
            var result = service.GetCurrentPricingExecution(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCurrentPricingExecution_Processing()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Processing,
                    Id = 6,
                    HangfireJobId = "HangfireJobId 9",
                    PlanVersionId = 4,
                    Queued = new DateTime(2020, 2, 4, 11, 12, 13)
                });

            var service = _GetService();

            // Act
            var result = service.GetCurrentPricingExecution(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCurrentPricingExecution_Canceled()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Canceled,
                    Id = 6,
                    HangfireJobId = "HangfireJobId 9",
                    PlanVersionId = 4,
                    Queued = new DateTime(2020, 2, 4, 11, 12, 13)
                });

            var service = _GetService();

            // Act
            var result = service.GetCurrentPricingExecution(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCurrentPricingExecution_Succeeded_GoalFulfilledByProprietary()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Id = 6,
                    HangfireJobId = "HangfireJobId 9",
                    PlanVersionId = 4,
                    Queued = new DateTime(2020, 2, 4, 11, 12, 13),
                    Completed = new DateTime(2020, 2, 4, 11, 13, 13),
                    DiagnosticResult = "DiagnosticResult"
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPricingResults(planId))
                .Returns(new CurrentPricingExecutionResultDto
                {
                    OptimalCpm = 5,
                    GoalFulfilledByProprietary = true,
                    PlanVersionId = 11,
                    JobId = 12
                });

            var service = _GetService();

            // Act
            var result = service.GetCurrentPricingExecution(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms_Succeeded()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPricingProgramsResult(planId))
                .Returns(new PricingProgramsResultDto
                {
                    Programs = new List<PlanPricingProgramProgramDto>
                    {
                        new PlanPricingProgramProgramDto
                        {
                            Id = 7,
                            ProgramName = "1+1",
                            Genre = "Comedy",
                            MarketCount = 6,
                            StationCount = 13,
                            AvgCpm = 6m,
                            AvgImpressions = 111000,
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3
                        }
                    },
                    Totals = new PricingProgramsResultTotalsDto
                    {
                        MarketCount = 6,
                        StationCount = 13,
                        AvgCpm = 6m,
                        AvgImpressions = 111000,
                        ImpressionsPercentage = 100,
                        Budget = 1131,
                        Spots = 3,
                        Impressions = 200000
                    }
                });

            // Act
            var result = service.GetPrograms(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetPrograms_JobStatusFailed()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Failed,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_JobStatusCanceled()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Canceled,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_JobStatusProcessing()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Processing,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_JobStatusQueued()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Queued,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_JobNull()
        {
            // Arrange
            const int planId = 6;

            var service = _GetService();

            // Act
            var result = service.GetPrograms(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_GetPricingProgramsResultReturnsNull()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms(planId);

            // Assert
            Assert.IsNull(result);
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCurrentPricingExecution_Succeeded_WithPricingResults()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Id = 6,
                    HangfireJobId = "HangfireJobId 9",
                    PlanVersionId = 4,
                    Queued = new DateTime(2020, 2, 4, 11, 12, 13),
                    Completed = new DateTime(2020, 2, 4, 11, 13, 13),
                    DiagnosticResult = "DiagnosticResult"
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPricingResults(planId))
                .Returns(new CurrentPricingExecutionResultDto
                {
                    OptimalCpm = 5,
                    GoalFulfilledByProprietary = true,
                    PlanVersionId = 11,
                    JobId = 12
                });

            var service = _GetService();

            // Act
            var result = service.GetCurrentPricingExecution(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void CanNotCancelJob_WhenItHasFailedStatus()
        {
            // Arrange
            const int planId = 6;
            const string expectedMessage = "Error encountered while running Pricing Model, please contact a system administrator for help";

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Failed
                });

            var service = _GetService();

            // Act
            var exception = Assert.Throws<Exception>(() => service.CancelCurrentPricingExecution(planId));

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void CanNotCancelJob_WhenItIsNotRunning()
        {
            // Arrange
            const int planId = 6;
            const string expectedMessage = "Error encountered while canceling Pricing Model, process is not running";

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded
                });

            var service = _GetService();

            // Act
            var exception = Assert.Throws<Exception>(() => service.CancelCurrentPricingExecution(planId));

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CancelsPricingJob()
        {
            // Arrange
            const int planId = 6;
            const string hangfireJobId = "#w2e3r4";

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Processing,
                    HangfireJobId = hangfireJobId,
                    Id = 7,
                    PlanVersionId = 11,
                    Queued = new DateTime(2020, 2, 4, 15, 31, 27)
                });

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate => jobUpdates.Add(jobUpdate));

            var hanfgireJobUpdates = new List<object>();
            _BackgroundJobClientMock
                .Setup(x => x.ChangeState(It.IsAny<string>(), It.IsAny<IState>(), It.IsAny<string>()))
                .Callback<string, IState, string>((jobId, state, expectedState) => hanfgireJobUpdates.Add(new { jobId, state, expectedState }));

            var service = _GetService();
            service.UT_CurrentDateTime = new DateTime(2020, 2, 4, 15, 32, 52);

            // Act
            var cancelCurrentPricingExecutionResult = service.CancelCurrentPricingExecution(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(new
            {
                cancelCurrentPricingExecutionResult,
                jobUpdates,
                hanfgireJobUpdates
            }));
        }

        /// <summary>
        /// Scenario : Enqueued without a HangfireJobId.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CancelsPricingJob_WhenQueuedWithoutHangfireId()
        {
            // Arrange
            const int planId = 6;
            const string hangfireJobId = null;

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Queued,
                    HangfireJobId = hangfireJobId,
                    Id = 7,
                    PlanVersionId = 11,
                    Queued = new DateTime(2020, 2, 4, 15, 31, 27)
                });

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate => jobUpdates.Add(jobUpdate));

            var hanfgireJobUpdates = new List<object>();
            _BackgroundJobClientMock
                .Setup(x => x.ChangeState(It.IsAny<string>(), It.IsAny<IState>(), It.IsAny<string>()))
                .Callback<string, IState, string>((jobId, state, expectedState) =>
                {
                    // mimicing hangfire service behaviour
                    if (string.IsNullOrEmpty(jobId))
                    {
                        throw new Exception("Null Job Id Exception");
                    }

                    hanfgireJobUpdates.Add(new {jobId, state, expectedState});
                });

            var service = _GetService();
            service.UT_CurrentDateTime = new DateTime(2020, 2,4,15,32, 52);

            // Act
            var cancelCurrentPricingExecutionResult = service.CancelCurrentPricingExecution(planId);

            // Assert
            // it shouldn't even try to delete the hangfire job.
            // it should update the job always
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(new
            {
                cancelCurrentPricingExecutionResult,
                jobUpdates,
                hanfgireJobUpdates
            }));
        }

        /// <summary>
        /// Scenario : When deleting the hangfire job it throws an exception.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CancelsPricingJob_WhenHangfireCancelThrowsException()
        {
            // Arrange
            const int planId = 6;
            const string hangfireJobId = "#w2e3r4";

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Queued,
                    HangfireJobId = hangfireJobId,
                    Id = 7,
                    PlanVersionId = 11,
                    Queued = new DateTime(2020, 2, 4, 15, 31, 27)
                });

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate => jobUpdates.Add(jobUpdate));

            var hanfgireJobUpdates = new List<object>();
            _BackgroundJobClientMock
                .Setup(x => x.ChangeState(It.IsAny<string>(), It.IsAny<IState>(), It.IsAny<string>()))
                .Callback<string, IState, string>((jobId, state, expectedState) =>
                {
                    hanfgireJobUpdates.Add(new { jobId, state, expectedState });
                    throw new Exception("Throwing a test exception.");
                });

            var service = _GetService();
            service.UT_CurrentDateTime = new DateTime(2020, 2, 4, 15, 32, 52);

            // Act
            var cancelCurrentPricingExecutionResult = service.CancelCurrentPricingExecution(planId);

            // Assert
            // it shouldn't even try to delete the hangfire job.
            // it should update the job always
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(new
            {
                cancelCurrentPricingExecutionResult,
                jobUpdates,
                hanfgireJobUpdates
            }));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunPricingJobTop100MarketsTest()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = PricingMarketGroupEnum.Top100;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(_GetInventoryProgram())
                .Returns(_GetInventoryProgram())
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop100MarketCoverages())
                .Returns(_GetTop100Markets());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(_GetWeeklyBreakDownGroup());

            var requests = new List<PlanPricingApiRequestDto>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Callback<PlanPricingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunPricingJobTop50MarketsTest()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = PricingMarketGroupEnum.Top50;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(_GetInventoryProgram())
                .Returns(_GetInventoryProgram())
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop50MarketCoverages())
                .Returns(_GetTop50Markets());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(_GetWeeklyBreakDownGroup());

            var requests = new List<PlanPricingApiRequestDto>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Callback<PlanPricingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunPricingJobTop25MarketsTest()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = PricingMarketGroupEnum.Top25;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(_GetInventoryProgram())
                .Returns(_GetInventoryProgram())
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop25MarketCoverages())
                .Returns(_GetTop25Markets());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(_GetWeeklyBreakDownGroup());

            var requests = new List<PlanPricingApiRequestDto>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Callback<PlanPricingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunPricingJobAllMarketsTest()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = PricingMarketGroupEnum.All;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(_GetInventoryProgram())
                .Returns(_GetInventoryProgram())
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(_GetWeeklyBreakDownGroup());

            var requests = new List<PlanPricingApiRequestDto>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Callback<PlanPricingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        private List<WeeklyBreakdownByWeek> _GetWeeklyBreakDownGroup()
        {
            return new List<WeeklyBreakdownByWeek>
                {
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 150,
                        Budget = 15,
                        MediaWeekId = 100
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 250,
                        Budget = 15m,
                        MediaWeekId = 101
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 100,
                        Budget = 15m,
                        MediaWeekId = 102
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 0,
                        Budget = 15m,
                        MediaWeekId = 103
                    }
                };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunPricingJobNoMarketsTest()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = PricingMarketGroupEnum.None;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .SetupSequence(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>()))
                .Returns(_GetInventoryProgram())
                .Returns(_GetInventoryProgram())
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()))
                .Returns(_GetWeeklyBreakDownGroup());

            var requests = new List<PlanPricingApiRequestDto>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResult(It.IsAny<PlanPricingApiRequestDto>()))
                .Callback<PlanPricingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            service.RunPricingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        private List<PlanPricingInventoryProgram> _GetInventoryProgram()
        {
            return new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    }
                };
        }

        private List<PlanPricingInventoryProgram> _GetMultipleInventoryPrograms()
        {
            return new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wabc",
                            MarketCode = 100,
                        },
                        ProjectedImpressions = 1500,
                        SpotCost = 60,
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 0,
                        ProjectedImpressions = 0,
                        SpotCost = 50,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 4,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        SpotCost = 0,
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<ManifestDaypart>
                        {
                            new ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<Program>
                                {
                                    new Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<ManifestWeek>
                        {
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    }
                };
        }

        private PlanDto _GetPlan()
        {
            return new PlanDto
            {
                CoverageGoalPercent = 80,
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                        new PlanAvailableMarketDto
                        {
                            Id = 5,
                            MarketCode = 101,
                            ShareOfVoicePercent = 12
                        },
                        new PlanAvailableMarketDto
                        {
                            Id = 6,
                            MarketCode = 102,
                            ShareOfVoicePercent = 13
                        }
                    },
                Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40
                        }
                    },
                PricingParameters = _GetPlanPricingParametersDto(),
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15,
                            MediaWeekId = 100
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 250,
                            WeeklyBudget = 15m,
                            MediaWeekId = 101
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 100,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 0,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    }
            };
        }
    }
}
