using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Hangfire;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.PlanBuying;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Plans
{
    [TestFixture]
    public class PlanBuyingServiceUnitTests
    {
        private Mock<ISpotLengthEngine> _SpotLengthEngineMock;
        private Mock<IPlanBuyingApiClient> _BuyingApiClientMock;
        private Mock<IBackgroundJobClient> _BackgroundJobClientMock;
        private Mock<IPlanBuyingInventoryEngine> _PlanBuyingInventoryEngineMock;
        private Mock<IBroadcastLockingManagerApplicationService> _BroadcastLockingManagerApplicationServiceMock;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IPlanRepository> _PlanRepositoryMock;
        private Mock<IInventoryRepository> _InventoryRepositoryMock;
        private Mock<IMarketCoverageRepository> _MarketCoverageRepositoryMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private Mock<IStationProgramRepository> _StationProgramRepositoryMock;
        private Mock<IMarketRepository> _MarketRepositoryMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IDaypartDefaultRepository> _DaypartDefaultRepositoryMock;
        private Mock<ICampaignRepository> _CampaignRepositoryMock;
        private Mock<IWeeklyBreakdownEngine> _WeeklyBreakdownEngineMock;
        private Mock<IPlanBuyingBandCalculationEngine> _PlanBuyingBandCalculationEngineMock;
        private Mock<IPlanBuyingStationEngine> _PlanBuyingStationEngineMock;
        private Mock<IPlanBuyingProgramEngine> _PlanBuyingProgramEngine;
        private Mock<IPlanBuyingMarketResultsEngine> _PlanBuyingMarketResultsEngine;
        private Mock<IPlanBuyingRequestLogClient> _PlanBuyingRequestLogClient;
        private Mock<IPlanBuyingOwnershipGroupEngine> _PlanBuyingOwnershipGroupEngine;
        private Mock<IPlanBuyingRepFirmEngine> _PlanBuyingRepFirmEngine;

        protected PlanBuyingService _GetService()
        {
            return new PlanBuyingService(
                _DataRepositoryFactoryMock.Object,
                _SpotLengthEngineMock.Object,
                _BuyingApiClientMock.Object,
                _BackgroundJobClientMock.Object,
                _PlanBuyingInventoryEngineMock.Object,
                _BroadcastLockingManagerApplicationServiceMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _DateTimeEngineMock.Object,
                _WeeklyBreakdownEngineMock.Object,
                _PlanBuyingBandCalculationEngineMock.Object,
                _PlanBuyingStationEngineMock.Object,
                _PlanBuyingProgramEngine.Object,
                _PlanBuyingMarketResultsEngine.Object,
                _PlanBuyingRequestLogClient.Object,
                _PlanBuyingOwnershipGroupEngine.Object,
                _PlanBuyingRepFirmEngine.Object
            );
        }

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _BuyingApiClientMock = new Mock<IPlanBuyingApiClient>();
            _BackgroundJobClientMock = new Mock<IBackgroundJobClient>();
            _PlanBuyingInventoryEngineMock = new Mock<IPlanBuyingInventoryEngine>();
            _BroadcastLockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();
            _InventoryRepositoryMock = new Mock<IInventoryRepository>();
            _MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _StationProgramRepositoryMock = new Mock<IStationProgramRepository>();
            _MarketRepositoryMock = new Mock<IMarketRepository>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _DaypartDefaultRepositoryMock = new Mock<IDaypartDefaultRepository>();
            _CampaignRepositoryMock = new Mock<ICampaignRepository>();
            _WeeklyBreakdownEngineMock = new Mock<IWeeklyBreakdownEngine>();
            _PlanBuyingBandCalculationEngineMock = new Mock<IPlanBuyingBandCalculationEngine>();
            _PlanBuyingStationEngineMock = new Mock<IPlanBuyingStationEngine>();
            _PlanBuyingProgramEngine = new Mock<IPlanBuyingProgramEngine>();
            _PlanBuyingMarketResultsEngine = new Mock<IPlanBuyingMarketResultsEngine>();
            _PlanBuyingRequestLogClient = new Mock<IPlanBuyingRequestLogClient>();
            _PlanBuyingOwnershipGroupEngine = new Mock<IPlanBuyingOwnershipGroupEngine>();
            _PlanBuyingRepFirmEngine = new Mock<IPlanBuyingRepFirmEngine>();

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 2, 4, 15, 31, 27));

            _DaypartDefaultRepositoryMock
                .Setup(x => x.GetAllDaypartDefaults())
                .Returns(DaypartsTestData.GetAllDaypartDefaultsWithBaseData());

            _InventoryRepositoryMock
                .Setup(x => x.GetInventorySources())
                .Returns(InventoryTestData.GetInventorySources());

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

            _PlanBuyingRequestLogClient
                .Setup(x => x.SaveBuyingRequest(It.IsAny<int>(), It.IsAny<PlanBuyingApiRequestDto>()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBuyingModelSpots_V3_FilterOutZeroCostSpots()
        {
            // Arrange
            var inventory = new List<PlanBuyingInventoryProgram>
            {
                _GetInventoryProgram()[0],
                _GetInventoryProgram()[0],
                _GetInventoryProgram()[0],
                _GetInventoryProgram()[0]
            };
            inventory[0].ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
            {
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 1, Cost = 50 },
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 2, Cost = 25 },
            };
            inventory[1].ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
            {
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 1, Cost = 0 },
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 2, Cost = 25 },
            };
            inventory[2].ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
            {
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 1, Cost = 50 },
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 2, Cost = 0 },
            };
            inventory[3].ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
            {
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 1, Cost = 0 },
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 2, Cost = 0 },
            };

            var flattedProgramsWithDayparts = inventory
                .SelectMany(x => x.ManifestDayparts.Select(d => new PlanBuyingService.ProgramWithManifestDaypart
                {
                    Program = x,
                    ManifestDaypart = d
                })).ToList();

            var groupedInventory = flattedProgramsWithDayparts.GroupBy(x =>
                new PlanBuyingInventoryGroup
                {
                    StationId = x.Program.Station.Id,
                    DaypartId = x.ManifestDaypart.Daypart.Id,
                    PrimaryProgramName = x.ManifestDaypart.PrimaryProgram.Name
                }).ToList();

            var skippedWeekIds = new List<int>();

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            var service = _GetService();

            // Act
            var result = service._GetBuyingModelSpots_v3(groupedInventory, skippedWeekIds);

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

        private List<PlanBuyingInventoryProgram> _GetInventoryProgram()
        {
            return new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
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
                        ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                        {
                            new BasePlanInventoryProgram.ManifestRate
                            {
                                Cost = 50
                            }
                        },
                        InventorySource = new InventorySource
                        {
                            Id = 3,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                        {
                            new BasePlanInventoryProgram.ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Monday = true,
                                    StartTime = 18000, // 5am
                                    EndTime = 21599 // 6am
                                },
                                Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                                {
                                    new BasePlanInventoryProgram.ManifestDaypart.Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 100,
                            },
                            new PlanBuyingInventoryProgram.ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                            },
                            new PlanBuyingInventoryProgram.ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
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
                        ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                        {
                            new BasePlanInventoryProgram.ManifestRate
                            {
                                Cost = 60
                            }
                        },
                        InventorySource = new InventorySource
                        {
                            Id = 4,
                            InventoryType = InventorySourceTypeEnum.Barter
                        },
                        ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                        {
                            new BasePlanInventoryProgram.ManifestDaypart
                            {
                                Daypart = new DisplayDaypart
                                {
                                    Id = 1,
                                    Wednesday = true,
                                    Friday = true,
                                    StartTime = 64800, // 6pm
                                    EndTime = 71999 // 8pm
                                },
                                Programs = new List<BasePlanInventoryProgram.ManifestDaypart.Program>
                                {
                                    new BasePlanInventoryProgram.ManifestDaypart.Program
                                    {
                                        Name = "seinfeld",
                                        Genre = "News"
                                    }
                                },
                                PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                                {
                                    Name = "seinfeld",
                                    Genre = "News"
                                }
                            }
                        },
                        ManifestWeeks = new List<PlanBuyingInventoryProgram.ManifestWeek>
                        {
                            new PlanBuyingInventoryProgram.ManifestWeek
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