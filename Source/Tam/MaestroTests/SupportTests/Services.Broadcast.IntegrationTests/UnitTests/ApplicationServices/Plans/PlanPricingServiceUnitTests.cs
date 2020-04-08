using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Hangfire;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Plan;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram.ManifestDaypart;
using Services.Broadcast.Entities;
using static Services.Broadcast.BusinessEngines.PlanPricingInventoryEngine;
using Common.Services;
using System.Threading;
using Services.Broadcast.Entities.StationInventory;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.Entities;

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
                .Setup(x => x.GetDataRepository<IMarketRepository>())
                .Returns(_MarketRepositoryMock.Object);
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
        public void AggregateResultsTest()
        {
            var inventory = _GetPlanPricingInventoryPrograms();

            var apiResponse = new PlanPricingAllocationResult
            {
                PricingCpm = 5.78m,
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Id = 1,
                        StandardDaypart = new DaypartDefaultDto { Id = 1 },
                        Spots = 2,
                        SpotCost = 200,
                        Impressions = 10000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 2,
                        StandardDaypart = new DaypartDefaultDto { Id = 2 },
                        Spots = 4,
                        SpotCost = 300,
                        Impressions = 50000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 3,
                        StandardDaypart = new DaypartDefaultDto { Id = 3 },
                        Spots = 3,
                        SpotCost = 500,
                        Impressions = 20000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 4,
                        StandardDaypart = new DaypartDefaultDto { Id = 4 },
                        Spots = 1,
                        SpotCost = 100,
                        Impressions = 30000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 5,
                        StandardDaypart = new DaypartDefaultDto { Id = 5 },
                        Spots = 3,
                        SpotCost = 300,
                        Impressions = 10000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 6,
                        StandardDaypart = new DaypartDefaultDto { Id = 6 },
                        Spots = 2,
                        SpotCost = 400,
                        Impressions = 50000,
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 7,
                        StandardDaypart = new DaypartDefaultDto { Id = 7 },
                        Spots = 1,
                        SpotCost = 250,
                        Impressions = 20000,
                    }
                }
            };

            var service = _GetService();

            var result = service.AggregateResults(inventory, apiResponse);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AggregateResultsWhenProprietaryGoalsMeetPlanGoalsTest()
        {
            var inventory = _GetPlanPricingInventoryPrograms();

            var apiResponse = new PlanPricingAllocationResult
            {
                PricingCpm = 5.78m,
                Spots = new List<PlanPricingAllocatedSpot>()
            };

            var service = _GetService();

            var result = service.AggregateResults(inventory, apiResponse, goalsFulfilledByProprietaryInventory: true);

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingModelSpots()
        {
            // Arrange
            var inventory = _GetInventory();
            var plan = _GetPlan();

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(null))
                .Returns(_GetLatestMarketCoverages());

            var service = _GetService();
            
            // Act
            var result = service._GetPricingModelSpots(inventory, new List<int>());

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
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

        private List<PlanPricingInventoryProgram> _GetInventory()
        {
            return new List<PlanPricingInventoryProgram>
            {
                // should keep.  All good.
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "wnbc",
                        MarketCode = 101,
                    },
                    ProvidedImpressions = 12,
                    ProjectedImpressions = 13,
                    SpotCost = 1,
                    StandardDaypartId = 1,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 1,
                                Monday = true,
                                Friday = true,
                                StartTime = 36000,
                                EndTime = 43199
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
                        new ManifestWeek { ContractMediaWeekId = 1 },
                        new ManifestWeek { ContractMediaWeekId = 2 }
                    }
                },
                // should keep.  All good.
                new PlanPricingInventoryProgram
                {
                    ManifestId = 2,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 2,
                        LegacyCallLetters = "wabc",
                        MarketCode = 101,
                    },
                    ProvidedImpressions = 12,
                    ProjectedImpressions = 13,
                    SpotCost = 1,
                    StandardDaypartId = 1,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 2,
                                Monday = true,
                                Friday = true,
                                Sunday = true,
                                StartTime = 36000,
                                EndTime = 43199
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
                        new ManifestWeek { ContractMediaWeekId = 1 },
                        new ManifestWeek { ContractMediaWeekId = 2 }
                    }
                },
                // should keep.  ProvidedImpressions = null but ProjectedImpressions > 0
                new PlanPricingInventoryProgram
                {
                    ManifestId = 3,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 3,
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
                    },
                    ProvidedImpressions = null,
                    ProjectedImpressions = 13,
                    SpotCost = 1,
                    StandardDaypartId = 1,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 3,
                                Monday = true,
                                Friday = true,
                                StartTime = 36000,
                                EndTime = 43699
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
                        new ManifestWeek { ContractMediaWeekId = 1 },
                        new ManifestWeek { ContractMediaWeekId = 2 }
                    }
                },
                // should filter due to ProvidedImpressions = null && ProjectedImpressions = 0
                new PlanPricingInventoryProgram
                {
                    ManifestId = 4,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 4,
                        LegacyCallLetters = "kabc",
                        MarketCode = 302,
                    },
                    ProvidedImpressions = null,
                    ProjectedImpressions = 0,
                    SpotCost = 1,
                    StandardDaypartId = 1,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 4,
                                Monday = true,
                                Friday = true,
                                StartTime = 36000,
                                EndTime = 43199
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
                        new ManifestWeek { ContractMediaWeekId = 1 },
                        new ManifestWeek { ContractMediaWeekId = 2 }
                    }
                },
                // should filter due to ProvidedImpressions = 0
                new PlanPricingInventoryProgram
                {
                    ManifestId = 5,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 1,
                        LegacyCallLetters = "wnbc",
                        MarketCode = 101,
                    },
                    ProvidedImpressions = 0,
                    ProjectedImpressions = 13,
                    SpotCost = 1,
                    StandardDaypartId = 1,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 5,
                                Monday = true,
                                Friday = true,
                                StartTime = 36000,
                                EndTime = 43199
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
                        new ManifestWeek { ContractMediaWeekId = 1 },
                        new ManifestWeek { ContractMediaWeekId = 2 }
                    }
                },
                // should filter due to ProvidedImpressions = null and ProjectedImpressions = 0
                new PlanPricingInventoryProgram
                {
                    ManifestId = 6,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 2,
                        LegacyCallLetters = "wabc",
                        MarketCode = 101,
                    },
                    ProvidedImpressions = null,
                    ProjectedImpressions = 0,
                    SpotCost = 1,
                    StandardDaypartId = 1,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 6,
                                Monday = true,
                                Friday = true,
                                StartTime = 36000,
                                EndTime = 43199
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
                        new ManifestWeek { ContractMediaWeekId = 1 },
                        new ManifestWeek { ContractMediaWeekId = 2 }
                    }
                },
                // should filter due to SpotCost = 0
                new PlanPricingInventoryProgram
                {
                    ManifestId = 7,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 3,
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
                    },
                    ProvidedImpressions = 12,
                    ProjectedImpressions = 13,
                    SpotCost = 0,
                    StandardDaypartId = 1,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 7,
                                Monday = true,
                                Friday = true,
                                StartTime = 36000,
                                EndTime = 43199
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
                        new ManifestWeek { ContractMediaWeekId = 1 },
                        new ManifestWeek { ContractMediaWeekId = 2 }
                    }
                }
            };
        }

        [Test]
        [TestCase(-1.1, false)]
        [TestCase(-0.1, false)]
        [TestCase(0.0, false)]
        [TestCase(0.1, true)]
        [TestCase(1.1, true)]
        public void AreImpressionsValidForPricingModelInput(decimal spotCost, bool expectedResult)
        {
            var service = _GetService();

            var result = service._AreImpressionsValidForPricingModelInput(spotCost);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void AreImpressionsValidForPricingModelInput_WhenNull()
        {
            var service = _GetService();

            var result = service._AreImpressionsValidForPricingModelInput((decimal?) null);

            Assert.IsFalse(result);
        }

        [Test]
        [TestCase(-1.1, false)]
        [TestCase(-0.1, false)]
        [TestCase(0.0, false)]
        [TestCase(0.1, true)]
        [TestCase(1.1, true)]
        public void IsSpotCostValidForPricingModelInput(double impressions, bool expectedResult)
        {
            var service = _GetService();

            var result = service._IsSpotCostValidForPricingModelInput(impressions);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void IsSpotCostValidForPricingModelInput_WhenNull()
        {
            var service = _GetService();

            var result = service._IsSpotCostValidForPricingModelInput((double?) null);

            Assert.IsFalse(result);
        }

        [Test]
        [TestCase(-1.1, false)]
        [TestCase(-0.1, false)]
        [TestCase(0.0, false)]
        [TestCase(0.1, true)]
        [TestCase(1.1, true)]
        public void AreWeeklyImpressionsValidForPricingModelInput(double impressions, bool expectedResult)
        {
            var service = _GetService();

            var result = service._AreWeeklyImpressionsValidForPricingModelInput(impressions);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void AreWeeklyImpressionsValidForPricingModelInput_WhenNull()
        {
            var service = _GetService();

            var result = service._AreWeeklyImpressionsValidForPricingModelInput(null);

            Assert.IsFalse(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingModelWeeks()
        {
            // Arrange
            var usedDayparts = new List<DisplayDaypart>();
            var proprietaryEstimates = _GetProprietaryEstimates();
            var plan = _GetPlan();
            var parameters = new ProgramInventoryOptionalParametersDto();

            var service = _GetService();

            // Act
            var weeks = service._GetPricingModelWeeks(plan, proprietaryEstimates, parameters, out List<int> skippedWeeksIds);

            // Assert
            Assert.AreEqual(3, skippedWeeksIds.Count());
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(weeks));
        }
        
        [Test]
        [TestCase(UnitCapEnum.Per30Min, "hour", 0.5)]
        [TestCase(UnitCapEnum.PerHour, "hour", 1)]
        [TestCase(UnitCapEnum.PerDay, "day", 1)]
        [TestCase(UnitCapEnum.PerWeek, "week", 1)]
        public void GetPricingModelWeeks_CapSpotTypes(
            UnitCapEnum unitCapType,
            string expectedFrequencyCapUnit,
            double expectedFrequencyCapTime)
        {
            // Arrange
            var usedDayparts = new List<DisplayDaypart>();
            var proprietaryEstimates = _GetProprietaryEstimates();
            var plan = _GetPlan();
            var parameters = new ProgramInventoryOptionalParametersDto();

            plan.PricingParameters.UnitCapsType = unitCapType;

            _DaypartCacheMock
                .Setup(x => x.GetIdByDaypart(It.IsAny<DisplayDaypart>()))
                .Returns<DisplayDaypart>(x => x.StartTime + x.EndTime)
                .Callback<DisplayDaypart>(x => usedDayparts.Add(x));

            var service = _GetService();

            // Act
            var weeks = service._GetPricingModelWeeks(plan, proprietaryEstimates, parameters, out List<int> skippedWeeksIds);

            // Assert
            var firstWeek = weeks.First();

            Assert.AreEqual(expectedFrequencyCapUnit, firstWeek.FrequencyCapUnit);
            Assert.AreEqual(expectedFrequencyCapTime, firstWeek.FrequencyCapTime);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ThrowsException_WhenWrongUnitCapType_IsPassed()
        {
            // Arrange
            const string expectedMessage = "Unsupported unit cap type was discovered";

            var proprietaryEstimates = _GetProprietaryEstimates();
            var plan = _GetPlan();
            var parameters = new ProgramInventoryOptionalParametersDto();

            plan.PricingParameters.UnitCapsType = UnitCapEnum.PerMonth;

            var service = _GetService();

            // Act
            var exception = Assert.Throws<ApplicationException>(() => 
                service._GetPricingModelWeeks(plan, proprietaryEstimates, parameters, out List<int> skippedWeeksIds));

            // Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingWeeksWithMargin()
        {
            // Arrange
            var usedDayparts = new List<DisplayDaypart>();
            var proprietaryEstimates = _GetProprietaryEstimates();
            var plan = _GetPlan();
            var parameters = new ProgramInventoryOptionalParametersDto
            {
                Margin = 20
            };

            var service = _GetService();

            // Act
            var weeks = service._GetPricingModelWeeks(plan, proprietaryEstimates, parameters, out List<int> skippedWeeksIds);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(weeks));
        }

        private PlanDto _GetPlan()
        {
            return new PlanDto
            {
                CoverageGoalPercent = 80,
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    // should be filtered out.
                    new WeeklyBreakdownWeek
                    {
                        MediaWeekId = 1,
                        WeeklyImpressions = 0,
                        WeeklyBudget = 200000
                    },
                    // should be filtered out because there is proprietary inventory to cover all these impressions
                    new WeeklyBreakdownWeek
                    {
                        MediaWeekId = 3,
                        WeeklyImpressions = 300,
                        WeeklyBudget = 999
                    },
                    // should be filtered out because there is proprietary inventory to cover the cost
                    new WeeklyBreakdownWeek
                    {
                        MediaWeekId = 4,
                        WeeklyImpressions = 300,
                        WeeklyBudget = 999
                    },
                    // should stay
                    new WeeklyBreakdownWeek
                    {
                        MediaWeekId = 2,
                        WeeklyImpressions = 2000,
                        WeeklyBudget = 200000
                    }
                },
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto
                    {
                        MarketCode = 101,
                        ShareOfVoicePercent = 11
                    },
                    new PlanAvailableMarketDto
                    {
                        MarketCode = 102,
                        ShareOfVoicePercent = null
                    },
                    new PlanAvailableMarketDto
                    {
                        MarketCode = 103,
                        ShareOfVoicePercent = 55
                    }
                },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        WeightingGoalPercent = 17,
                        StartTimeSeconds = 100,
                        EndTimeSeconds = 199,
                        DaypartCodeId = 1
                    },
                    new PlanDaypartDto
                    {
                        WeightingGoalPercent = 19,
                        StartTimeSeconds = 100,
                        EndTimeSeconds = 299,
                        DaypartCodeId = 2
                    }
                },
                PricingParameters = new PlanPricingParametersDto
                {
                    UnitCapsType = UnitCapEnum.Per30Min,
                    UnitCaps = 2
                },
                FlightDays = new List<int> { 1, 2, 3, 4, 6 }
            };
        }

        private List<PricingEstimate> _GetProprietaryEstimates()
        {
            return new List<PricingEstimate>
            {
                new PricingEstimate
                {
                    InventorySourceId = 5,
                    MediaWeekId = 1,
                    Impressions = 500,
                    Cost = 500
                },
                new PricingEstimate
                {
                    InventorySourceId = 6,
                    MediaWeekId = 2,
                    Impressions = 400,
                    Cost = 400
                },
                new PricingEstimate
                {
                    InventorySourceId = 6,
                    MediaWeekId = 4,
                    Impressions = 400,
                    Cost = 999
                },
                new PricingEstimate
                {
                    InventorySourceId = 7,
                    MediaWeekId = 3,
                    Impressions = 300,
                    Cost = 300
                }
            };
        }

        protected PlanPricingService _GetService()
        {
            return new PlanPricingService(
                _DataRepositoryFactoryMock.Object,
                _SpotLengthEngineMock.Object,
                _PricingApiClientMock.Object,
                _BackgroundJobClientMock.Object,
                _PlanPricingInventoryEngineMock.Object,
                _BroadcastLockingManagerApplicationServiceMock.Object,
                _DaypartCacheMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object);
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
                    PricingParameters = _GetPlanPricingParametersDto()
                });

            _InventoryRepositoryMock
                .Setup(x => x.GetInventorySources())
                .Returns(_GetInventorySources());

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
                    It.Is<IEnumerable<int>>(list => list.SequenceEqual(new List<int> { 3, 5, 7, 10, 11, 12 })),
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
    }
}
