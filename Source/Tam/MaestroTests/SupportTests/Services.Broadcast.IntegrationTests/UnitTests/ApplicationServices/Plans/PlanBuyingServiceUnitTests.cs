using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
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
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities.Scx;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.BusinessEngines.PlanBuyingInventoryEngine;
using static Services.Broadcast.Entities.Plan.Buying.PlanBuyingInventoryProgram;
using static Services.Broadcast.Entities.Plan.CommonPricingEntities.BasePlanInventoryProgram;
using static Services.Broadcast.Entities.Plan.CommonPricingEntities.BasePlanInventoryProgram.ManifestDaypart;

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
        private Mock<IStandardDaypartRepository> _StandardDaypartRepositoryMock;
        private Mock<ICampaignRepository> _CampaignRepositoryMock;
        private Mock<IWeeklyBreakdownEngine> _WeeklyBreakdownEngineMock;
        private Mock<IPlanBuyingBandCalculationEngine> _PlanBuyingBandCalculationEngineMock;
        private Mock<IPlanBuyingStationEngine> _PlanBuyingStationEngineMock;
        private Mock<IPlanBuyingProgramEngine> _PlanBuyingProgramEngine;
        private Mock<IPlanBuyingMarketResultsEngine> _PlanBuyingMarketResultsEngine;
        private Mock<IPlanBuyingRequestLogClient> _PlanBuyingRequestLogClient;
        private Mock<IPlanBuyingOwnershipGroupEngine> _PlanBuyingOwnershipGroupEngine;
        private Mock<IPlanBuyingRepFirmEngine> _PlanBuyingRepFirmEngine;
        private Mock<IInventoryProprietarySummaryRepository> _InventoryProprietarySummaryRepositoryMock;
        private Mock<IBroadcastAudienceRepository> _BroadcastAudienceRepositoryMock;
        private Mock<IPlanBuyingRepository> _PlanBuyingRepositoryMock;
        private IAsyncTaskHelper _AsyncTaskHelper;
        private Mock<ISharedFolderService> _SharedFolderService;
        private Mock<IPlanBuyingScxDataPrep> _PlanBuyingScxDataPrep;
        private Mock<IPlanBuyingScxDataConverter> _PlanBuyingScxDataConverter;

        protected PlanBuyingService _GetService(bool useTrueIndependentStations = false, bool allowMultipleCreativeLengths = false)
        {
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            launchDarklyClientStub.FeatureToggles.Add(FeatureToggles.USE_TRUE_INDEPENDENT_STATIONS, useTrueIndependentStations);
            launchDarklyClientStub.FeatureToggles.Add(FeatureToggles.ALLOW_MULTIPLE_CREATIVE_LENGTHS, allowMultipleCreativeLengths);
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);

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
                _PlanBuyingRepFirmEngine.Object,
                _AsyncTaskHelper,
                _SharedFolderService.Object,
                _PlanBuyingScxDataPrep.Object,
                _PlanBuyingScxDataConverter.Object,
                featureToggleHelper
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
            _StandardDaypartRepositoryMock = new Mock<IStandardDaypartRepository>();
            _CampaignRepositoryMock = new Mock<ICampaignRepository>();
            _WeeklyBreakdownEngineMock = new Mock<IWeeklyBreakdownEngine>();
            _PlanBuyingBandCalculationEngineMock = new Mock<IPlanBuyingBandCalculationEngine>();
            _PlanBuyingStationEngineMock = new Mock<IPlanBuyingStationEngine>();
            _PlanBuyingProgramEngine = new Mock<IPlanBuyingProgramEngine>();
            _PlanBuyingMarketResultsEngine = new Mock<IPlanBuyingMarketResultsEngine>();
            _PlanBuyingRequestLogClient = new Mock<IPlanBuyingRequestLogClient>();
            _PlanBuyingOwnershipGroupEngine = new Mock<IPlanBuyingOwnershipGroupEngine>();
            _PlanBuyingRepFirmEngine = new Mock<IPlanBuyingRepFirmEngine>();
            _InventoryProprietarySummaryRepositoryMock = new Mock<IInventoryProprietarySummaryRepository>();
            _BroadcastAudienceRepositoryMock = new Mock<IBroadcastAudienceRepository>();
            _PlanBuyingRepositoryMock = new Mock<IPlanBuyingRepository>();
            _AsyncTaskHelper = new AsyncTaskHelperStub();
            _SharedFolderService = new Mock<ISharedFolderService>();
            _PlanBuyingScxDataPrep = new Mock<IPlanBuyingScxDataPrep>();
            _PlanBuyingScxDataConverter = new Mock<IPlanBuyingScxDataConverter>();

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(MarketsTestData.GetLatestMarketCoverages());

            _SpotLengthEngineMock.Setup(s => s.GetDeliveryMultipliers())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);
            _SpotLengthEngineMock.Setup(s => s.GetCostMultipliers())
                .Returns(SpotLengthTestData.GetCostMultipliersBySpotLengthId);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanBuyingRepository>())
                .Returns(_PlanBuyingRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryProprietarySummaryRepository>())
                .Returns(_InventoryProprietarySummaryRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IBroadcastAudienceRepository>())
                .Returns(_BroadcastAudienceRepositoryMock.Object);

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 2, 4, 15, 31, 27));

            _StandardDaypartRepositoryMock
                .Setup(x => x.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData());

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
                .Setup(x => x.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_StandardDaypartRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IMarketRepository>())
                .Returns(_MarketRepositoryMock.Object);

            _PlanBuyingRequestLogClient
                .Setup(x => x.SaveBuyingRequest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PlanBuyingApiRequestDto>(), It.IsAny<string>()));

            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaWeekById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaWeek);

            var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
            SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanBuyingDefaults()
        {
            // Arrange
            var service = _GetService();

            // Act
            var result = service.GetPlanBuyingDefaults();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBuyingModelSpots_V3_FilterOutZeroCostSpots()
        {
            // Arrange
            var inventory = _GetInventory();
            inventory[0].ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
            {
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 1, Cost = 50 },
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 2, Cost = 25 },
            };
            inventory[1].ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
            {
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 1, Cost = 0 },
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 2, Cost = 50 },
            };
            inventory[2].ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
            {
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 1, Cost = 25 },
                new BasePlanInventoryProgram.ManifestRate { SpotLengthId = 2, Cost = 0 },
            };

            var service = _GetService();
            var groupedInventory = PlanBuyingService._GroupInventory(inventory);
            var skippedWeekIds = new List<int>();

            // Act
            var result = service._GetBuyingModelSpots_v3(groupedInventory, skippedWeekIds);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingModelSpots_HandleDupWeekInventory()
        {
            // Arrange
            var inventory = new List<PlanBuyingInventoryProgram>
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 50
                            }
                        },
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
                                InventoryMediaWeekId = 100
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                                InventoryMediaWeekId = 101
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                                InventoryMediaWeekId = 102
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 103,
                                InventoryMediaWeekId = 103
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 104,
                                InventoryMediaWeekId = 104
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 105,
                                InventoryMediaWeekId = 105
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 100
                            }
                        },
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
                                ContractMediaWeekId = 102,
                                InventoryMediaWeekId = 102
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 103,
                                InventoryMediaWeekId = 103
                            }
                        }
                    }
                };
            var skippedWeekIds = new List<int>();
            var groupedInventory = PlanBuyingService._GroupInventory(inventory);
            var service = _GetService();

            // Act
            var result = service._GetBuyingModelSpots(groupedInventory, skippedWeekIds);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingModelSpots_V3_HandleDupWeekInventory()
        {
            // Arrange
            var inventory = new List<PlanBuyingInventoryProgram>
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 50
                            }
                        },
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
                                InventoryMediaWeekId = 100
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 101,
                                InventoryMediaWeekId = 101
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 102,
                                InventoryMediaWeekId = 102
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 103,
                                InventoryMediaWeekId = 103
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 104,
                                InventoryMediaWeekId = 104
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 105,
                                InventoryMediaWeekId = 105
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 15,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1000,
                        ProjectedImpressions = 1100,
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 100
                            }
                        },
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
                                ContractMediaWeekId = 102,
                                InventoryMediaWeekId = 102
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 103,
                                InventoryMediaWeekId = 103
                            }
                        }
                    }
                };
            var skippedWeekIds = new List<int>();
            var groupedInventory = PlanBuyingService._GroupInventory(inventory);
            var service = _GetService();

            // Act
            var result = service._GetBuyingModelSpots_v3(groupedInventory, skippedWeekIds);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunBuyingJobWithProprietaryInventory()
        {
            // Arrange
            const int jobId = 1;

            _InventoryProprietarySummaryRepositoryMock
                .Setup(x => x.GetInventoryProprietarySummariesByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetInventoryProprietaryQuarterSummary(false));

            _BroadcastAudienceRepositoryMock
                .Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(new List<audience_audiences>
                {
                    new audience_audiences { rating_audience_id = 1 },
                    new audience_audiences { rating_audience_id = 2 }
                });

            var parameters = _GetPlanBuyingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top100;
            parameters.Margin = 10;
            parameters.JobId = jobId;
            parameters.ProprietaryInventory = new List<InventoryProprietarySummary>
            {
                new InventoryProprietarySummary { Id = 1 },
                new InventoryProprietarySummary { Id = 2 }
            };

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetPlanBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanBuyingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanBuyingJobDiagnostic>()))
                .Returns(_GetMultipleInventoryPrograms());            

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop100MarketCoverages())
                .Returns(MarketsTestData.GetTop100Markets());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakDownGroup());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Returns(_GetWeeklyBreakDownWeeks());

            var requests = new List<PlanBuyingApiRequestDto>();
            _BuyingApiClientMock
                .Setup(x => x.GetBuyingSpotsResult(It.IsAny<PlanBuyingApiRequestDto>()))
                .Returns(new PlanBuyingApiSpotsResponseDto
                {
                    RequestId = "q1w2e3r4",
                    Results = new List<PlanBuyingApiSpotsResultDto>()
                })
                .Callback<PlanBuyingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            service.RunBuyingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        private WeeklyBreakdownResponseDto _GetWeeklyBreakDownWeeks()
        {
            return new WeeklyBreakdownResponseDto
            {
                Weeks = new List<WeeklyBreakdownWeek>
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

        private List<PlanBuyingInventoryProgram> _GetMultipleInventoryPrograms()
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                Cost = 50
                            }
                        },
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                Cost = 60
                            }
                        },
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
                    new PlanBuyingInventoryProgram
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                Cost = 50
                            }
                        },
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
                    new PlanBuyingInventoryProgram
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                Cost = 0
                            }
                        },
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
                Id = 1197,
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
                            DaypartCodeId = 1,
                            WeightingGoalPercent = 60
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 2,
                            WeightingGoalPercent = 40
                        }
                    },
                BuyingParameters = _GetPlanBuyingParametersDto(),
                FlightStartDate = new DateTime(2005, 11, 21),
                FlightEndDate = new DateTime(2005, 12, 18),
                FlightDays = new List<int>(),
                TargetRatingPoints = 1000,
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

        private PlanBuyingParametersDto _GetPlanBuyingParametersDto()
        {
            return new PlanBuyingParametersDto
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
                MarketGroup = MarketGroupEnum.All
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesBuyingApiResults_v3()
        {
            // Arrange
            var allowMultipleCreativeLengths = true;
            const int jobId = 1;

            _InventoryProprietarySummaryRepositoryMock
                .Setup(x => x.GetInventoryProprietarySummariesByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetInventoryProprietaryQuarterSummary(false));

            _BroadcastAudienceRepositoryMock
                .Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(new List<audience_audiences>
                {
                    new audience_audiences { rating_audience_id = 1 },
                    new audience_audiences { rating_audience_id = 2 }
                });

            var parameters = _GetPlanBuyingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.None;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetPlanBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob());

            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 1,
                        Weight = 50
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 2,
                        Weight = 50
                    }
                };
            plan.BuyingParameters.UnitCapsType = UnitCapEnum.Per30Min;
            plan.BuyingParameters.UnitCaps = 3;
            plan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    // SpotLengthId = 1, DaypartCodeId = 1
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 40,
                        WeeklyBudget = 2.5m,
                        MediaWeekId = 100,
                        SpotLengthId = 1,
                        DaypartCodeId = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 100,
                        WeeklyBudget = 2.5m,
                        MediaWeekId = 101,
                        SpotLengthId = 1,
                        DaypartCodeId = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 25,
                        WeeklyBudget = 2.5m,
                        MediaWeekId = 102,
                        SpotLengthId = 1,
                        DaypartCodeId = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 0,
                        WeeklyBudget = 2.5m,
                        MediaWeekId = 103,
                        SpotLengthId = 1,
                        DaypartCodeId = 1
                    },

                    // SpotLengthId = 2, DaypartCodeId = 1
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 35,
                        WeeklyBudget = 5m,
                        MediaWeekId = 100,
                        SpotLengthId = 2,
                        DaypartCodeId = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 25,
                        WeeklyBudget = 5m,
                        MediaWeekId = 101,
                        SpotLengthId = 2,
                        DaypartCodeId = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 25,
                        WeeklyBudget = 5m,
                        MediaWeekId = 102,
                        SpotLengthId = 2,
                        DaypartCodeId = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 0,
                        WeeklyBudget = 5m,
                        MediaWeekId = 103,
                        SpotLengthId = 2,
                        DaypartCodeId = 1
                    },

                    // SpotLengthId = 1, DaypartCodeId = 2
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 40,
                        WeeklyBudget = 2.5m,
                        MediaWeekId = 100,
                        SpotLengthId = 1,
                        DaypartCodeId = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 100,
                        WeeklyBudget = 2.5m,
                        MediaWeekId = 101,
                        SpotLengthId = 1,
                        DaypartCodeId = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 25,
                        WeeklyBudget = 2.5m,
                        MediaWeekId = 102,
                        SpotLengthId = 1,
                        DaypartCodeId = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 0,
                        WeeklyBudget = 2.5m,
                        MediaWeekId = 103,
                        SpotLengthId = 1,
                        DaypartCodeId = 2
                    },

                    // SpotLengthId = 2, DaypartCodeId = 2
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 35,
                        WeeklyBudget = 5m,
                        MediaWeekId = 100,
                        SpotLengthId = 2,
                        DaypartCodeId = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 25,
                        WeeklyBudget = 5m,
                        MediaWeekId = 101,
                        SpotLengthId = 2,
                        DaypartCodeId = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 25,
                        WeeklyBudget = 5m,
                        MediaWeekId = 102,
                        SpotLengthId = 2,
                        DaypartCodeId = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 0,
                        WeeklyBudget = 5m,
                        MediaWeekId = 103,
                        SpotLengthId = 2,
                        DaypartCodeId = 2
                    }
                };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            _PlanBuyingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanBuyingJobDiagnostic>()))
                .Returns(_GetMultipleInventoryPrograms_v3());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakDownGroup());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            var requests = new List<PlanBuyingApiRequestDto_v3>();
            _BuyingApiClientMock
                .Setup(x => x.GetBuyingSpotsResult(It.IsAny<PlanBuyingApiRequestDto_v3>()))
                .Returns<PlanBuyingApiRequestDto_v3>((request) =>
                {
                    var results = new List<PlanBuyingApiSpotsResultDto_v3>();

                    foreach (var spot in request.Spots)
                    {
                        var result = new PlanBuyingApiSpotsResultDto_v3
                        {
                            ManifestId = spot.Id,
                            MediaWeekId = spot.MediaWeekId,
                            Frequencies = spot.SpotCost
                                .Select(x => new SpotFrequencyResponse
                                {
                                    SpotLengthId = x.SpotLengthId,
                                    Frequency = 1
                                })
                                .ToList()
                        };

                        results.Add(result);
                    }

                    return new PlanBuyingApiSpotsResponseDto_v3
                    {
                        RequestId = "djj4j4399fmmf1m212",
                        Results = results
                    };
                });

            _SpotLengthEngineMock
                .Setup(x => x.GetDeliveryMultiplierBySpotLengthId(It.IsAny<int>()))
                .Returns<int>(id => id == 1 ? 1 : 0.5);

            var passedParameters = new List<PlanBuyingAllocationResult>();
            _PlanBuyingRepositoryMock
                .Setup(x => x.SaveBuyingApiResults(It.IsAny<PlanBuyingAllocationResult>()))
                .Callback<PlanBuyingAllocationResult>(p => passedParameters.Add(p));

            var service = _GetService(false, allowMultipleCreativeLengths);

            // Act
            service.RunBuyingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBuyingModelSpots_VerifyMappings_V2()
        {
            // Arrange
            var inventory = _GetInventory();
            // for v2 must remove the multi spot length. 
            // stick with SpotLength 1
            inventory.ForEach(s => s.ManifestRates = s.ManifestRates.Where(r => r.SpotLengthId == 1).ToList());

            var skippedWeekIds = new List<int> { 821 };
            var service = _GetService();
            var groupedInventory = PlanBuyingService._GroupInventory(inventory);

            // Act
            var result = service._GetBuyingModelSpots(groupedInventory, skippedWeekIds);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ResultSeparatesAllocatedAndUnallocatedV2()
        {
            var inventory = _GetInventory();
            // for v2 must remove the multi spot length. 
            // stick with SpotLength 1
            inventory.ForEach(s => s.ManifestRates = s.ManifestRates.Where(r => r.SpotLengthId == 1).ToList());

            var parameters = new PlanBuyingParametersDto { Margin = 20 };
            var service = _GetService();
            var skippedWeeks = new List<int>();

            // these service steps are performed leading up to this step.
            var groupedInventory = PlanBuyingService._GroupInventory(inventory);
            var spotsAndMappings = service._GetBuyingModelSpots(groupedInventory, skippedWeeks);
            var buyingApiRequest = new PlanBuyingApiRequestDto { Spots = spotsAndMappings.Spots };
            var apiSpotsResults = new PlanBuyingApiSpotsResponseDto
            {
                Results = new List<PlanBuyingApiSpotsResultDto>
                {
                    new PlanBuyingApiSpotsResultDto
                    {
                        ManifestId = spotsAndMappings.Spots[0].Id, MediaWeekId = spotsAndMappings.Spots[0].MediaWeekId, Frequency = 5
                    },
                    new PlanBuyingApiSpotsResultDto
                    {
                        ManifestId = spotsAndMappings.Spots[2].Id, MediaWeekId = spotsAndMappings.Spots[2].MediaWeekId, Frequency = 2
                    },
                    new PlanBuyingApiSpotsResultDto
                    {
                        ManifestId = spotsAndMappings.Spots[3].Id, MediaWeekId = spotsAndMappings.Spots[3].MediaWeekId, Frequency = 3
                    }
                }
            };

            // put this in
            var results = service._MapToResultSpotsV2(apiSpotsResults, buyingApiRequest, inventory, parameters, spotsAndMappings.Mappings);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        private List<PlanBuyingInventoryProgram> _GetInventory()
        {
            var inventory = new List<PlanBuyingInventoryProgram>
            {
                new PlanBuyingInventoryProgram
                {
                    ManifestId = 1,
                    ProvidedImpressions = 1000,
                    ProjectedImpressions = 1100,
                    StandardDaypartId = 12,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 5,
                        LegacyCallLetters = "wnbc",
                        MarketCode = 101,
                    },
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 50
                        },
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 2,
                            Cost = 25
                        }
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
                            InventoryMediaWeekId = 720,
                            Spots = 1,
                            ContractMediaWeekId = 820,
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 721,
                            Spots = 2,
                            ContractMediaWeekId = 821,
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    ManifestId = 2,
                    ProvidedImpressions = 1000,
                    ProjectedImpressions = 1100,
                    StandardDaypartId = 12,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 5,
                        LegacyCallLetters = "wnbc",
                        MarketCode = 101,
                    },
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 50
                        },
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 2,
                            Cost = 25
                        }
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
                            InventoryMediaWeekId = 722,
                            Spots = 1,
                            ContractMediaWeekId = 822,
                        }
                    }
                },
                new PlanBuyingInventoryProgram
                {
                    ManifestId = 3,
                    ProvidedImpressions = 1000,
                    ProjectedImpressions = 1100,
                    StandardDaypartId = 12,
                    Station = new DisplayBroadcastStation
                    {
                        Id = 4,
                        LegacyCallLetters = "wcbs",
                        MarketCode = 101,
                    },
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 50
                        },
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            SpotLengthId = 2,
                            Cost = 25
                        }
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
                            InventoryMediaWeekId = 720,
                            Spots = 1,
                            ContractMediaWeekId = 820,
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 721,
                            Spots = 1,
                            ContractMediaWeekId = 821,
                        },
                        new PlanBuyingInventoryProgram.ManifestWeek
                        {
                            InventoryMediaWeekId = 722,
                            Spots = 1,
                            ContractMediaWeekId = 822,
                        }
                    }
                }
            };

            return inventory;
        }

        /// <summary>
        /// Verify that sthe result.Mappings is reported correctly for the result.Spots.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBuyingModelSpots_VerifyMappings_V3()
        {
            // Arrange
            var inventory = _GetInventory();
            var skippedWeekIds = new List<int> { 821 };
            var service = _GetService();
            var groupedInventory = PlanBuyingService._GroupInventory(inventory);

            // Act
            var result = service._GetBuyingModelSpots_v3(groupedInventory, skippedWeekIds);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ResultSeparatesAllocatedAndUnallocatedV3()
        {
            // Arrange
            var inventory = _GetInventory();

            var plan = new PlanDto {CreativeLengths = new List<CreativeLength> {new CreativeLength { SpotLengthId = 1 }, new CreativeLength {SpotLengthId = 2 }}};
            var parameters = new PlanBuyingParametersDto { Margin = 20 };
            var service = _GetService();
            var skippedWeeks = new List<int>();

            // these service steps are performed leading up to this step.
            var groupedInventory = PlanBuyingService._GroupInventory(inventory);
            var spotsAndMappings = service._GetBuyingModelSpots_v3(groupedInventory, skippedWeeks);
            var buyingApiRequest = new PlanBuyingApiRequestDto_v3 { Spots = spotsAndMappings.Spots };
            var apiSpotsResults = new PlanBuyingApiSpotsResponseDto_v3
            {
                Results = new List<PlanBuyingApiSpotsResultDto_v3>
                {
                    new PlanBuyingApiSpotsResultDto_v3
                    {
                        ManifestId = spotsAndMappings.Spots[0].Id, MediaWeekId = spotsAndMappings.Spots[0].MediaWeekId, Frequencies = new List<SpotFrequencyResponse>
                        {
                            new SpotFrequencyResponse {SpotLengthId = 1, Frequency = 0 },
                            new SpotFrequencyResponse {SpotLengthId = 2, Frequency = 3 }
                        }
                    },
                    new PlanBuyingApiSpotsResultDto_v3
                    {
                        ManifestId = spotsAndMappings.Spots[2].Id, MediaWeekId = spotsAndMappings.Spots[2].MediaWeekId, Frequencies = new List<SpotFrequencyResponse>
                        {
                            new SpotFrequencyResponse {SpotLengthId = 1, Frequency = 2 },
                            new SpotFrequencyResponse {SpotLengthId = 2, Frequency = 4 }
                        }
                    },
                    new PlanBuyingApiSpotsResultDto_v3
                    {
                        ManifestId = spotsAndMappings.Spots[3].Id, MediaWeekId = spotsAndMappings.Spots[3].MediaWeekId, Frequencies = new List<SpotFrequencyResponse>
                        {
                            new SpotFrequencyResponse {SpotLengthId = 1, Frequency = 1 },
                            new SpotFrequencyResponse {SpotLengthId = 2, Frequency = 3 }
                        }
                    }
                }
            };
            
            // Act
            var results = service._MapToResultSpotsV3(apiSpotsResults, buyingApiRequest, inventory, parameters, plan, spotsAndMappings.Mappings);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        private List<WeeklyBreakdownWeek> _GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart()
        {
            return new List<WeeklyBreakdownWeek>
            {
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 150,
                    WeeklyBudget = 15,
                    MediaWeekId = 100,
                    SpotLengthId = 1,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 250,
                    WeeklyBudget = 15m,
                    MediaWeekId = 101,
                    SpotLengthId = 1,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 100,
                    WeeklyBudget = 15m,
                    MediaWeekId = 102,
                    SpotLengthId = 1,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 0,
                    WeeklyBudget = 15m,
                    MediaWeekId = 103,
                    SpotLengthId = 1,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 150,
                    WeeklyBudget = 15,
                    MediaWeekId = 100,
                    SpotLengthId = 1,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 250,
                    WeeklyBudget = 15m,
                    MediaWeekId = 101,
                    SpotLengthId = 1,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 100,
                    WeeklyBudget = 15m,
                    MediaWeekId = 102,
                    SpotLengthId = 1,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 0,
                    WeeklyBudget = 15m,
                    MediaWeekId = 103,
                    SpotLengthId = 1,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 150,
                    WeeklyBudget = 15,
                    MediaWeekId = 100,
                    SpotLengthId = 2,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 250,
                    WeeklyBudget = 15m,
                    MediaWeekId = 101,
                    SpotLengthId = 2,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 100,
                    WeeklyBudget = 15m,
                    MediaWeekId = 102,
                    SpotLengthId = 2,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 0,
                    WeeklyBudget = 15m,
                    MediaWeekId = 103,
                    SpotLengthId = 2,
                    DaypartCodeId = 1
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 150,
                    WeeklyBudget = 15,
                    MediaWeekId = 100,
                    SpotLengthId = 2,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 250,
                    WeeklyBudget = 15m,
                    MediaWeekId = 101,
                    SpotLengthId = 2,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 100,
                    WeeklyBudget = 15m,
                    MediaWeekId = 102,
                    SpotLengthId = 2,
                    DaypartCodeId = 2
                },
                new WeeklyBreakdownWeek
                {
                    WeeklyImpressions = 0,
                    WeeklyBudget = 15m,
                    MediaWeekId = 103,
                    SpotLengthId = 2,
                    DaypartCodeId = 2
                }
            };
        }

        private List<PlanBuyingInventoryProgram> _GetMultipleInventoryPrograms_v3()
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 50
                            },
                            new ManifestRate
                            {
                                SpotLengthId = 2,
                                Cost = 50
                            }
                        },
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
                                ContractMediaWeekId = 821,
                                InventoryMediaWeekId = 721
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 50
                            },
                            new ManifestRate
                            {
                                SpotLengthId = 2,
                                Cost = 50
                            }
                        },
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
                                ContractMediaWeekId = 821,
                                InventoryMediaWeekId = 721,
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 50
                            },
                            new ManifestRate
                            {
                                SpotLengthId = 2,
                                Cost = 50
                            }
                        },
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
                                ContractMediaWeekId = 823,
                                InventoryMediaWeekId = 723,
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
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
                        ManifestRates = new List<ManifestRate>
                        {
                            new ManifestRate
                            {
                                SpotLengthId = 1,
                                Cost = 10
                            },
                            new ManifestRate
                            {
                                SpotLengthId = 2,
                                Cost = 20
                            }
                        },
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
                                Spots = 2,
                                ContractMediaWeekId = 822,
                                InventoryMediaWeekId = 722,
                            }
                        }
                    }
                };
        }

        [Ignore("SDE - BP-2358 : This fails on the build server as-if it's running the old PlanBuyingServiceCode.  The approval file is fine.  Setting this to ignore to allow checking of BP-2358.")]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RunBuyingJobWithProprietaryInventory_15Only()
        {
            // Arrange
            const int jobId = 1;

            _SpotLengthEngineMock
                .Setup(x => x.GetSpotLengthIdByValue(It.IsAny<int>()))
                .Returns<int>(x => x == 30 ? 1 : 2);

            _InventoryProprietarySummaryRepositoryMock
                .Setup(x => x.GetInventoryProprietarySummariesByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetInventoryProprietaryQuarterSummary(false));

            _BroadcastAudienceRepositoryMock
                .Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(new List<audience_audiences>
                {
                    new audience_audiences { rating_audience_id = 1 },
                    new audience_audiences { rating_audience_id = 2 }
                });

            var parameters = _GetPlanBuyingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top100;
            parameters.Margin = 10;
            parameters.JobId = jobId;
            parameters.ProprietaryInventory = new List<InventoryProprietarySummary>
            {
                new InventoryProprietarySummary { Id = 1 },
                new InventoryProprietarySummary { Id = 2 }
            };

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetPlanBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob());

            var plan = _GetPlan();
            plan.CreativeLengths[0].SpotLengthId = 2;

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            _PlanBuyingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanBuyingJobDiagnostic>()))
                .Returns(_GetMultipleInventoryPrograms());            

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop100MarketCoverages())
                .Returns(MarketsTestData.GetTop100Markets());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakDownGroup());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Returns(_GetWeeklyBreakDownWeeks());

            var requests = new List<PlanBuyingApiRequestDto>();
            _BuyingApiClientMock
                .Setup(x => x.GetBuyingSpotsResult(It.IsAny<PlanBuyingApiRequestDto>()))
                .Returns(new PlanBuyingApiSpotsResponseDto
                {
                    RequestId = "q1w2e3r4",
                    Results = new List<PlanBuyingApiSpotsResultDto>()
                })
                .Callback<PlanBuyingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            service.RunBuyingJob(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [TestCase(null, true)]
        [TestCase(50, true)]
        [TestCase(0, false)]
        [TestCase(101, false)]
        public void ValidatePlanBuyingScxExportRequest(int? unallocatedCpmThreshold, bool expectSuccess)
        {
            var request = new PlanBuyingScxExportRequest { UnallocatedCpmThreshold = unallocatedCpmThreshold };
            var service = _GetService();

            if (expectSuccess)
            {
                Assert.DoesNotThrow(() => service._ValidatePlanBuyingScxExportRequest(request));
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => service._ValidatePlanBuyingScxExportRequest(request));
            }
        }

        [Test]
        [TestCase( SpotAllocationModelMode.Quality, "PlanBuying_Test Plan Name_20201030_121523_Q")]
        [TestCase( SpotAllocationModelMode.Efficiency, "PlanBuying_Test Plan Name_20201030_121523_E")]
        [TestCase( SpotAllocationModelMode.Floor, "PlanBuying_Test Plan Name_20201030_121523_F")]
        public void ExportPlanBuyingScx(SpotAllocationModelMode spotAllocationModelMode, string expectedFileName)
        {
            const string username = "testUser";
            const string planName = "Test Plan Name";
            const string testStreamContent = "<xml>TestContent<xml/>";
            var request = new PlanBuyingScxExportRequest { PlanId = 21, UnallocatedCpmThreshold = 12 };
            
            var currentDateTime = new DateTime(2020, 10, 30, 12, 15, 23);
            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);
            
            _PlanBuyingScxDataPrep.Setup(s => s.GetScxData(It.IsAny<PlanBuyingScxExportRequest>(), It.IsAny<DateTime>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<PlanBuyingScxExportRequest, DateTime, SpotAllocationModelMode>((a,b,c) => new PlanScxData {PlanName = planName, Generated = b});
            _PlanBuyingScxDataConverter.Setup(s => s.ConvertData(It.IsAny<PlanScxData>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<PlanScxData, SpotAllocationModelMode>((d,e) => new PlanBuyingScxFile
                {
                    PlanName = d.PlanName,
                  
                    GeneratedTimeStamp = d.Generated,
                    ScxStream = new MemoryStream(Encoding.UTF8.GetBytes(testStreamContent))
                }); 

            var savedSharedFiles = new List<SharedFolderFile>();
            var testGuid = Guid.NewGuid();
            _SharedFolderService.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Callback<SharedFolderFile>((f) => savedSharedFiles.Add(f))
                .Returns(testGuid);

            var service = _GetService();

            // Act
            var savedFileGuid = service.ExportPlanBuyingScx(request, username, spotAllocationModelMode);
           
            //// Assert           
            var savedSharedFile = savedSharedFiles[0];
            Assert.IsTrue(savedSharedFile.FolderPath.EndsWith(@"\PlanBuyingScx"));

            Assert.AreEqual(1, savedSharedFiles.Count);
            Assert.AreEqual(expectedFileName, savedSharedFile.FileName);
          
           
        }

        private List<InventoryProprietaryQuarterSummaryDto> _GetInventoryProprietaryQuarterSummary(bool highProprietaryNumbers)
        {
            return new List<InventoryProprietaryQuarterSummaryDto>
            {
                new InventoryProprietaryQuarterSummaryDto
                {
                    UnitCost = 100,                    
                    SummaryByStationByAudience = new List<InventoryProprietarySummaryByStationByAudience>
                    {
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 1,
                            MarketCode = 100,
                            StationId = 5237,
                            Impressions = highProprietaryNumbers ? 30000000 : 300000
                        },
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 1,
                            MarketCode = 101,
                            StationId = 1467,
                            Impressions = highProprietaryNumbers ? 30000000 : 300000
                        },
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 1,
                            MarketCode = 302,
                            StationId = 3537,
                            Impressions = highProprietaryNumbers ? 40000000 : 400000
                        },
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 2,
                            MarketCode = 101,
                            StationId = 5014,
                            Impressions = highProprietaryNumbers ? 100000000 : 1000000
                        },
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 3,
                            MarketCode = 101,
                            StationId = 5015,
                            Impressions = highProprietaryNumbers ? 1000000000 : 10000000
                        }
                    }
                },
                new InventoryProprietaryQuarterSummaryDto
                {
                    UnitCost = 100,                   
                    SummaryByStationByAudience = new List<InventoryProprietarySummaryByStationByAudience>
                    {
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 1,
                            MarketCode = 101,
                            StationId = 5016,
                            Impressions = highProprietaryNumbers ? 100000000 : 1000000
                        }
                    }
                }
            };
        }
    }
}