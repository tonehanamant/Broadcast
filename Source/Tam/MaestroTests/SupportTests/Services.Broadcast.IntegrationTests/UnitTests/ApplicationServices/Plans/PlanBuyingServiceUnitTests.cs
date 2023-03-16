using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
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
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Plan.Buying.PlanBuyingInventoryProgram;
using static Services.Broadcast.Entities.Plan.CommonPricingEntities.BasePlanInventoryProgram;
using static Services.Broadcast.Entities.Plan.CommonPricingEntities.BasePlanInventoryProgram.ManifestDaypart;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Plans
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PlanBuyingServiceUnitTests
    {
        private readonly DateTime _CurrentDate = new DateTime(2017, 10, 17, 7, 30, 23);
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
        private Mock<IAabEngine> _AabEngine;
        private Mock<IAudienceService> _AudienceServiceMock;
        private Mock<IDaypartCache> _DaypartCacheMock;
        private Mock<IPlanValidator> _PlanValidator;

        private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
        private Mock<IConfigurationSettingsHelper> _IConfigurationSettingsHelperMock;

        protected PlanBuyingService _GetService(bool useTrueIndependentStations = false,
             bool isPostingTypeToggleEnabled = false)
        {
            _LaunchDarklyClientStub = new LaunchDarklyClientStub();
            _LaunchDarklyClientStub.FeatureToggles.Add(FeatureToggles.USE_TRUE_INDEPENDENT_STATIONS, useTrueIndependentStations);
            _LaunchDarklyClientStub.FeatureToggles.Add(FeatureToggles.PRICING_MODEL_BARTER_INVENTORY, false);
            _LaunchDarklyClientStub.FeatureToggles.Add(FeatureToggles.PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY, false);

            _IConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder)).Returns("c:\\TempFolder");

            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);

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
                featureToggleHelper,
                _AabEngine.Object,
                _AudienceServiceMock.Object,
                _DaypartCacheMock.Object,
                _PlanValidator.Object,
                _IConfigurationSettingsHelperMock.Object                
            );
        }

        [SetUp]
        public void SetUp()
        {
            _SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();
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
            _AabEngine = new Mock<IAabEngine>();
            _AudienceServiceMock = new Mock<IAudienceService>();
            _DaypartCacheMock = new Mock<IDaypartCache>();
            _IConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _PlanValidator = new Mock<IPlanValidator>();

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(MarketsTestData.GetLatestMarketCoverages());

            _SpotLengthEngineMock.Setup(s => s.GetDeliveryMultipliers())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);
            _SpotLengthEngineMock.Setup(s => s.GetCostMultipliers(true))
                .Returns(SpotLengthTestData.GetCostMultipliersBySpotLengthId(applyInventoryPremium: true));

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
                .Setup(x => x.SaveBuyingRequest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SpotAllocationModelMode>()));

            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaWeekById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaWeek);

            _DataRepositoryFactoryMock
               .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
               .Returns(_SpotLengthRepositoryMock.Object);

            _DataRepositoryFactoryMock
               .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
               .Returns(_SpotLengthRepositoryMock.Object);
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
        [Ignore("SDE - BP-2260 : This fails on the build server as-if it's running the old PlanBuyingServiceCode.  The approval file is fine.  Setting this to ignore to allow checking of BP-2260.")]
        public async void RunBuyingJobWithProprietaryInventory()
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
                .Setup(x => x.GetBuyingSpotsResultAsync(It.IsAny<PlanBuyingApiRequestDto_v3>()))
                .Returns(
                    Task.FromResult(new PlanBuyingApiSpotsResponseDto_v3
                    {
                        RequestId = "q1w2e3r4",
                        Results = new List<PlanBuyingApiSpotsResultDto_v3>()
                    }))
                .Callback<PlanBuyingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunBuyingJobAsync(parameters, jobId, CancellationToken.None);

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
                                SpotLengthId = 1,
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
                                SpotLengthId = 1,
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
                            WeightingGoalPercent = 60,
                            DaypartTypeId = DaypartTypeEnum.News
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 2,
                            WeightingGoalPercent = 40,
                            DaypartTypeId = DaypartTypeEnum.News
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
                    },
                ImpressionsPerUnit = 1
            };
        }

        private PlanBuyingParametersDto _GetPlanBuyingParametersDto(PostingTypeEnum postingType = PostingTypeEnum.NSI)
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
                UnitCapsType = UnitCapEnum.Per30Min,
                MarketGroup = MarketGroupEnum.All,
                PostingType = postingType
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async void SavesBuyingApiResults_v3()
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
                .Returns(new PlanBuyingJob
                {
                    Id = jobId
                });

            var plan = _GetPlan();
            plan.Equivalized = true;
            plan.PostingType = PostingTypeEnum.NSI;
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
                .Setup(x => x.GetBuyingSpotsResultAsync(It.IsAny<PlanBuyingApiRequestDto_v3>()))
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

                    return Task.FromResult(new PlanBuyingApiSpotsResponseDto_v3
                    {
                        RequestId = "djj4j4399fmmf1m212",
                        Results = results
                    });
                });

            _SpotLengthEngineMock
                .Setup(x => x.GetDeliveryMultiplierBySpotLengthId(It.IsAny<int>()))
                .Returns<int>(id => id == 1 ? 1 : 0.5);

            var passedParameters = new List<PlanBuyingAllocationResult>();
            _PlanBuyingRepositoryMock
                .Setup(x => x.SaveBuyingApiResults(It.IsAny<PlanBuyingAllocationResult>()))
                .Callback<PlanBuyingAllocationResult>(p => passedParameters.Add(p));

            _PlanBuyingProgramEngine.Setup(s => s.Calculate(
                 It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                 It.IsAny<bool>()))
             .Returns(new PlanBuyingResultBaseDto
             {
                 GoalFulfilledByProprietary = false,
                 JobId = 1,
                 OptimalCpm = 0.1682953311617806731813246000m,
                 PlanVersionId = 77,
                 Programs = new List<PlanBuyingProgramDto>
                 {
                        new PlanBuyingProgramDto
                        {
                            AvgCpm = 62.5000m,
                            AvgImpressions = 1000,
                            Budget = 187.5m,
                            Genre = "Sport",
                            Id = 0,
                            Impressions = 3000,
                            MarketCount = 1,
                            PercentageOfBuy = 40,
                            ProgramName = "seinfeld_2",
                            SpotCount = 3,
                            StationCount = 1
                        },
                        new PlanBuyingProgramDto
                        {
                            AvgCpm = 50.00m,
                            AvgImpressions = 1500,
                            Budget = 225m,
                            Genre = "News",
                            Id = 0,
                            Impressions = 4500,
                            MarketCount = 1,
                            PercentageOfBuy = 60,
                            ProgramName = "seinfeld",
                            SpotCount = 3,
                            StationCount = 1
                        }
                 },
                 Totals = new PlanBuyingProgramTotalsDto
                 {
                     AvgCpm = 55.000m,
                     AvgImpressions = 1250,
                     Budget = 412.5m,
                     Impressions = 7500,
                     ImpressionsPercentage = 0,
                     MarketCount = 2,
                     SpotCount = 6,
                     StationCount = 2
                 }
             });

            _PlanBuyingProgramEngine.Setup(s => s.CalculateProgramStations(
                   It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                   It.IsAny<bool>()))
               .Returns(new PlanBuyingResultBaseDto());

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.Calculate(
                    It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                    It.IsAny<PlanBuyingParametersDto>()))
                .Returns(new PlanBuyingBandsDto());

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.CalculateBandInventoryStation(
                    It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>()))
                .Returns(new PlanBuyingBandInventoryStationsDto());

            _PlanBuyingStationEngineMock.Setup(s =>
                    s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                        It.IsAny<PlanBuyingParametersDto>()))
                .Returns(new PlanBuyingStationResultDto());

            _PlanBuyingMarketResultsEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>(),
                    It.IsAny<PlanDto>(), It.IsAny<List<MarketCoverage>>()))
                .Returns(new PlanBuyingResultMarketsDto());

            _PlanBuyingOwnershipGroupEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>()
                   ))
                .Returns(new PlanBuyingResultOwnershipGroupDto());

            _PlanBuyingRepFirmEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>()
                   ))
                .Returns(new PlanBuyingResultRepFirmDto());

            var service = _GetService(false, allowMultipleCreativeLengths);

            // Act
            await service.RunBuyingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        [TestCase(SpotAllocationModelMode.Floor)]
        public void GetBuyingModelWeeks_Test_WithFloor_Returns1Cpm(SpotAllocationModelMode allocationMode)
        {
            //arrange
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            var buyingParameters = _GetPlanBuyingParametersDto();

            plan.BuyingParameters = buyingParameters;
            var proprietaryData = new ProprietaryInventoryData();
            var skippedWeekIds = new List<int>();

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
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

            var service = _GetService();

            //act
            var result = service._GetBuyingModelWeeks_v3(plan, buyingParameters, proprietaryData,
                out  skippedWeekIds, allocationMode);

            //assert
            Assert.IsTrue(result.All(x => x.CpmGoal == 1));
        }

        [Test]
        [TestCase(SpotAllocationModelMode.Efficiency)]
        public void GetBuyingModelWeeks_Test_WithEfficency_ReturnsCpm(SpotAllocationModelMode allocationMode)
        {
            //arrange
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            var buyingParameters = _GetPlanBuyingParametersDto();

            plan.BuyingParameters = buyingParameters;
            var proprietaryData = new ProprietaryInventoryData();
            var skippedWeekIds = new List<int>();

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
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

            var service = _GetService();

            //act
            var result = service._GetBuyingModelWeeks_v3(plan, buyingParameters, proprietaryData,
                out skippedWeekIds, allocationMode);

            //assert
            Assert.IsTrue(result.All(x => x.CpmGoal != 1));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async void SavesBuyingAggregateResults_WhenRunningBuyingJob()
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
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanBuyingRepositoryMock
               .Setup(x => x.GetPlanBuyingJob(It.IsAny<int>()))
               .Returns(new PlanBuyingJob
               {
                   Id = jobId
               });
            _PlanRepositoryMock
               .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
               .Returns(new PlanDto
               {
                   VersionId = 77,
                   CoverageGoalPercent = 80,
                   PostingType = PostingTypeEnum.NSI,
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
                   FlightStartDate = new DateTime(2005, 11, 21),
                   FlightEndDate = new DateTime(2005, 12, 18),
                   FlightDays = new List<int>(),
                   TargetRatingPoints = 1000,
                   Dayparts = new List<PlanDaypartDto>
                   {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60,
                            DaypartTypeId = DaypartTypeEnum.News
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40,
                            DaypartTypeId = DaypartTypeEnum.News
                        }
                   },
                   BuyingParameters = _GetPlanBuyingParametersDto(),
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
                   },
                   ImpressionsPerUnit = 1
               });

            _WeeklyBreakdownEngineMock
               .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
               .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
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

            _PlanBuyingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanBuyingJobDiagnostic>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        PostingType = PostingTypeEnum.NSI,
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
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        PostingType = PostingTypeEnum.NSI,
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
                                InventoryMediaWeekId = 90
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        PostingType = PostingTypeEnum.NSI,
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
                        PostingType = PostingTypeEnum.NSI,
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
                });

            _MarketCoverageRepositoryMock
              .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
              .Returns(_GetLatestMarketCoverages());

            _BuyingApiClientMock
             .Setup(x => x.GetBuyingSpotsResultAsync(It.IsAny<PlanBuyingApiRequestDto_v3>()))
             .Returns(Task.FromResult(new PlanBuyingApiSpotsResponseDto_v3
                     {
                         RequestId = "#q1w2e3",
                         Results = new List<PlanBuyingApiSpotsResultDto_v3>
                         {
                             new PlanBuyingApiSpotsResultDto_v3
                             {
                                 ManifestId = 1,
                                 MediaWeekId = 100,
                                 Frequencies = new List<SpotFrequencyResponse>
                                 {
                                     new SpotFrequencyResponse
                                     {
                                         SpotLengthId = 1,
                                         Frequency = 1
                                     }
                                 }
                             },
                             new PlanBuyingApiSpotsResultDto_v3
                             {
                                 ManifestId = 1,
                                 MediaWeekId = 101,
                                 Frequencies = new List<SpotFrequencyResponse>
                                 {
                                     new SpotFrequencyResponse
                                     {
                                         SpotLengthId = 1,
                                         Frequency = 2
                                     }
                                 }
                             },
                             new PlanBuyingApiSpotsResultDto_v3
                             {
                                 ManifestId = 2,
                                 MediaWeekId = 100,
                                 Frequencies = new List<SpotFrequencyResponse>
                                 {
                                     new SpotFrequencyResponse
                                     {
                                         SpotLengthId = 1,
                                         Frequency = 3
                                     }
                                 }
                             }
                         }
                     }
                 )
             );

            _MediaMonthAndWeekAggregateCacheMock
              .Setup(x => x.GetMediaWeekById(It.IsAny<int>()))
              .Returns<int>(weekId => new MediaWeek
              {
                  Id = weekId
              });

            var passedParameters = new List<PlanBuyingResultBaseDto>();
            var planVersionBuyingResultId = 100;
            _PlanBuyingRepositoryMock
                .Setup(x => x.SaveBuyingAggregateResults(It.IsAny<PlanBuyingResultBaseDto>()))
                .Callback<PlanBuyingResultBaseDto>(p =>
                {
                    passedParameters.Add(p);
                    planVersionBuyingResultId++;
                })
                .Returns<PlanBuyingResultBaseDto>(c => planVersionBuyingResultId);

            var planVersionBuyingResultIds = new List<int>();
            _PlanBuyingRepositoryMock
                .Setup(x => x.SavePlanBuyingResultSpots(It.IsAny<int>(), It.IsAny<PlanBuyingResultBaseDto>()))
                .Callback<int, PlanBuyingResultBaseDto>((p1, p2) =>
                {
                    planVersionBuyingResultIds.Add(p1);
                });

            _PlanBuyingProgramEngine.Setup(s => s.Calculate(
                   It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                   It.IsAny<bool>()))
               .Returns(new PlanBuyingResultBaseDto
               {
                   GoalFulfilledByProprietary = false,
                   JobId = 1,
                   OptimalCpm = 0.1682953311617806731813246000m,
                   PlanVersionId = 77,
                   Programs = new List<PlanBuyingProgramDto>
                   {
                        new PlanBuyingProgramDto
                        {
                            AvgCpm = 62.5000m,
                            AvgImpressions = 1000,
                            Budget = 187.5m,
                            Genre = "Sport",
                            Id = 0,
                            Impressions = 3000,
                            MarketCount = 1,
                            PercentageOfBuy = 40,
                            ProgramName = "seinfeld_2",
                            SpotCount = 3,
                            StationCount = 1
                        },
                        new PlanBuyingProgramDto
                        {
                            AvgCpm = 50.00m,
                            AvgImpressions = 1500,
                            Budget = 225m,
                            Genre = "News",
                            Id = 0,
                            Impressions = 4500,
                            MarketCount = 1,
                            PercentageOfBuy = 60,
                            ProgramName = "seinfeld",
                            SpotCount = 3,
                            StationCount = 1
                        }
                   },
                   Totals = new PlanBuyingProgramTotalsDto
                   {
                       AvgCpm = 55.000m,
                       AvgImpressions = 1250,
                       Budget = 412.5m,
                       Impressions = 7500,
                       ImpressionsPercentage = 0,
                       MarketCount = 2,
                       SpotCount = 6,
                       StationCount = 2
                   }
               });

            _PlanBuyingProgramEngine.Setup(s => s.CalculateProgramStations(
                   It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                   It.IsAny<bool>()))
               .Returns(new PlanBuyingResultBaseDto());

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.Calculate(
                    It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                    It.IsAny<PlanBuyingParametersDto>()))
                .Returns(new PlanBuyingBandsDto());

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.CalculateBandInventoryStation(
                    It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>()))
                .Returns(new PlanBuyingBandInventoryStationsDto());

            _PlanBuyingStationEngineMock.Setup(s =>
                    s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                        It.IsAny<PlanBuyingParametersDto>()))
                .Returns(new PlanBuyingStationResultDto());

            _PlanBuyingMarketResultsEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>(),
                    It.IsAny<PlanDto>(), It.IsAny<List<MarketCoverage>>()))
                .Returns(new PlanBuyingResultMarketsDto());

            _PlanBuyingOwnershipGroupEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>()
                   ))
                .Returns(new PlanBuyingResultOwnershipGroupDto());

            _PlanBuyingRepFirmEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>()
                   ))
                .Returns(new PlanBuyingResultRepFirmDto());

            var service = _GetService();
            // Act
            await service.RunBuyingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Assert.IsFalse(planVersionBuyingResultIds.Any(x => x <= 100));
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
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

            var plan = new PlanDto { CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 }, new CreativeLength { SpotLengthId = 2 } } };
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
        public async void RunBuyingJobWithProprietaryInventory_15Only()
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
                .Setup(x => x.GetBuyingSpotsResultAsync(It.IsAny<PlanBuyingApiRequestDto_v3>()))
                .Returns(Task.FromResult(new PlanBuyingApiSpotsResponseDto_v3
                {
                    RequestId = "q1w2e3r4",
                    Results = new List<PlanBuyingApiSpotsResultDto_v3>()
                }))
                .Callback<PlanBuyingApiRequestDto>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunBuyingJobAsync(parameters, jobId, CancellationToken.None);

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
                Assert.Throws<CadentException>(() => service._ValidatePlanBuyingScxExportRequest(request));
            }
        }

        [Test]
        [TestCase(SpotAllocationModelMode.Efficiency, "PlanBuying_Test Plan Name_20201030_121523_E")]
        [TestCase(SpotAllocationModelMode.Floor, "PlanBuying_Test Plan Name_20201030_121523_F")]
        public void ExportPlanBuyingScx(SpotAllocationModelMode spotAllocationModelMode, string expectedFileName)
        {
            const string username = "testUser";
            const string planName = "Test Plan Name";
            const string testStreamContent = "<xml>TestContent<xml/>";
            var request = new PlanBuyingScxExportRequest { PlanId = 21, UnallocatedCpmThreshold = 12 };

            var currentDateTime = new DateTime(2020, 10, 30, 12, 15, 23);
            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            _PlanBuyingScxDataPrep.Setup(s => s.GetScxData(It.IsAny<PlanBuyingScxExportRequest>(), It.IsAny<DateTime>(),
                It.IsAny<SpotAllocationModelMode>(),
                It.IsAny<PostingTypeEnum>()))
                .Returns<PlanBuyingScxExportRequest, DateTime, SpotAllocationModelMode, PostingTypeEnum>((a, b, c, d) => new PlanScxData { PlanName = planName, Generated = b });
            _PlanBuyingScxDataConverter.Setup(s => s.ConvertData(It.IsAny<PlanScxData>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<PlanScxData, SpotAllocationModelMode>((d, e) => new PlanBuyingScxFile
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

        [Test]
        public void ThrowsException_ProgramLineup_WhenNoPlansSelected()
        {
            // Arrange
            var expectedMessage = $"Choose at least one plan";
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int>()
            };

            var tc = _GetService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_ProgramLineup_WhenNoBuyingRunsDone()
        {
            // Arrange
            const string expectedMessage = "There are no completed buying runs for the chosen plan. Please run buying";
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { 1 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Dayparts = _GetPlanDayparts()
                });

            _BroadcastLockingManagerApplicationServiceMock
                 .Setup(x => x.GetLockObject(It.IsAny<string>()))
                 .Returns(new LockResponse
                 {
                     Success = true
                 });

            var tc = _GetService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed, "The latest buying run was failed. Please run buying again or contact the support")]
        [TestCase(BackgroundJobProcessingStatus.Queued, "There is a buying run in progress right now. Please wait until it is completed")]
        [TestCase(BackgroundJobProcessingStatus.Processing, "There is a buying run in progress right now. Please wait until it is completed")]
        public void ThrowsException_ProgramLineup_BuyingJobIsNotAcceptable(
            BackgroundJobProcessingStatus jobStatus,
            string expectedMessage)
        {
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { 1 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Dayparts = _GetPlanDayparts()
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Status = jobStatus
                });

            _BroadcastLockingManagerApplicationServiceMock
                 .Setup(x => x.GetLockObject(It.IsAny<string>()))
                 .Returns(new LockResponse
                 {
                     Success = true
                 });

            var tc = _GetService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ReturnsData_ProgramLineupReport_45UnEquivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            Guid advertiserId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 8, Weight = 50 } },
                    Equivalized = false,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    BuyingParameters = new PlanBuyingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForBuyingProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _GetService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetLatestBuyingJob(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetPlanBuyingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum?>(), It.IsAny<SpotAllocationModelMode?>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ProgramLineup_MultipleCreativeLengths()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            Guid advertiserId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId },

            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> {
                        new CreativeLength { SpotLengthId = 1, Weight = 50 },
                        new CreativeLength { SpotLengthId = 2, Weight = 20 },
                        new CreativeLength { SpotLengthId = 3, Weight = 30 }
                    },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    BuyingParameters = new PlanBuyingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForBuyingProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _GetService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetLatestBuyingJob(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetPlanBuyingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum?>(), It.IsAny<SpotAllocationModelMode?>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ProgramLineup_MultipleCreativeLengths_UnEquivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> {
                        new CreativeLength { SpotLengthId = 1, Weight = 50 },
                        new CreativeLength { SpotLengthId = 2, Weight = 20 },
                        new CreativeLength { SpotLengthId = 3, Weight = 30 }
                    },
                    Equivalized = false,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    BuyingParameters = new PlanBuyingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForBuyingProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = new Guid("8987F27B-AE14-4D7F-99C4-81AA7505EDE2"),
                    AdvertiserMasterId = new Guid("F1D648C8-8152-4932-8C4F-EC8B29445889")
                });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _GetService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ReturnsData_ProgramLineup_45Equivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            Guid advertiserId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 8, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    BuyingParameters = new PlanBuyingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanBuyingRepositoryMock
               .Setup(x => x.GetProprietaryInventoryForBuyingProgramLineup(It.IsAny<int>()))
               .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _GetService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetLatestBuyingJob(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetPlanBuyingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum?>(), It.IsAny<SpotAllocationModelMode?>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ReturnsData_ProgramLineup_30Equivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            Guid advertiserId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    BuyingParameters = new PlanBuyingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanBuyingRepositoryMock
              .Setup(x => x.GetProprietaryInventoryForBuyingProgramLineup(It.IsAny<int>()))
              .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _BroadcastLockingManagerApplicationServiceMock
                 .Setup(x => x.GetLockObject(It.IsAny<string>()))
                 .Returns(new LockResponse
                 {
                     Success = true
                 });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _GetService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetLatestBuyingJob(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetPlanBuyingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum?>(), It.IsAny<SpotAllocationModelMode?>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ProgramLineup_RollupStationSpecificNewsPrograms()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            Guid advertiserId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 30001,
                            StartTimeSeconds = 57600,
                            EndTimeSeconds = 68399
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 10001,
                            StartTimeSeconds = 14400,
                            EndTimeSeconds = 35999
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 20001,
                            StartTimeSeconds = 39600,
                            EndTimeSeconds = 46799
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 40001,
                            StartTimeSeconds = 72000,
                            EndTimeSeconds = 299
                        }
                    },
                    BuyingParameters = new PlanBuyingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanBuyingRepositoryMock
              .Setup(x => x.GetProprietaryInventoryForBuyingProgramLineup(It.IsAny<int>()))
              .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupForRollupTestData();

            var tc = _GetService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetLatestBuyingJob(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetPlanBuyingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum?>(), It.IsAny<SpotAllocationModelMode?>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 4001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<PlanDaypartDto> _GetPlanDayparts()
        {
            return new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartCodeId = 10001,
                    StartTimeSeconds = 50,
                    EndTimeSeconds = 150
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 20001,
                    StartTimeSeconds = 230,
                    EndTimeSeconds = 280
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 30002,
                    StartTimeSeconds = 350,
                    EndTimeSeconds = 450
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 40001,
                    StartTimeSeconds = 100,
                    EndTimeSeconds = 200
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 50001,
                    StartTimeSeconds = 100,
                    EndTimeSeconds = 200
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 60001,
                    StartTimeSeconds = 200,
                    EndTimeSeconds = 299
                }
            };
        }

        private List<ProgramLineupProprietaryInventory> _GetProprietaryLineupData()
        {
            return new List<ProgramLineupProprietaryInventory>
            {
                new ProgramLineupProprietaryInventory{
                    Genre = "Comedy",
                    ProgramName = "The big bang theory",
                    DaypartId = 19,
                    InventoryProprietaryDaypartProgramId  = 1,
                    MarketCode = 100,
                    SpotLengthId = 1,
                    Station = new MarketCoverageByStation.Station{
                        Affiliation = "A101 Affiliation",
                        Id = 101,
                        LegacyCallLetters = "WNBC"
                    },
                    ImpressionsPerWeek = 19999
                }
            };
        }

        private DisplayDaypart _GetDisplayDaypart()
        {
            return new DisplayDaypart
            {
                Code = "PRA",
                Name = "Prime Access",
                Monday = true,
                Thursday = true,
                Sunday = true,
                StartTime = 120,
                EndTime = 45600,
                StartAMPM = "6PM-8PM"
            };
        }

        private void _SetupBaseProgramLineupTestData()
        {
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime(2020, 02, 12)
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetPlanBuyingAllocatedSpotsByPlanId(It.IsAny<int>(), It.IsAny<PostingTypeEnum?>(), It.IsAny<SpotAllocationModelMode?>()))
                .Returns(_GetPlanBuyingAllocatedSpots());

            _InventoryRepositoryMock
                .Setup(x => x.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetStationInventoryManifests());

            _AabEngine
                .Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(new AgencyDto
                {
                    Name = "1st Image Marketing"
                });

            _AabEngine
                .Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(new AdvertiserDto
                {
                    Name = "PetMed Express, Inc"
                });

            _AudienceServiceMock
                .Setup(x => x.GetAudienceById(It.IsAny<int>()))
                .Returns(new PlanAudienceDisplay
                {
                    Code = "A18-20"
                });

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());

            _SpotLengthRepositoryMock.Setup(s => s.GetDeliveryMultipliersBySpotLengthId())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoveragesWithStations())
                .Returns(_GetMarketCoverages());

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, BasePlanInventoryProgram.ManifestDaypart.Program>
                {
                    {
                        1001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "Movie",
                            Name = "Joker"
                        }
                    },
                    {
                        2001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "Late News"
                        }
                    },
                    {
                        3001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "Movie",
                            Name = "1917"
                        }
                    }
                });
        }

        private void _SetupBaseProgramLineupForRollupTestData()
        {
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime(2020, 02, 12)
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetPlanBuyingAllocatedSpotsByPlanId(It.IsAny<int>(), It.IsAny<PostingTypeEnum?>(), It.IsAny<SpotAllocationModelMode?>()))
                .Returns(_GetPlanBuyingAllocatedSpotsForRollup());

            _InventoryRepositoryMock
                .Setup(x => x.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetStationInventoryManifestsForRollup());

            _AabEngine
                .Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(new AgencyDto
                {
                    Name = "1st Image Marketing"
                });

            _AabEngine
                .Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(new AdvertiserDto
                {
                    Name = "PetMed Express, Inc"
                });

            _AudienceServiceMock
               .Setup(x => x.GetAudienceById(It.IsAny<int>()))
               .Returns(new PlanAudienceDisplay
               {
                   Code = "A18-20"
               });

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());

            _SpotLengthRepositoryMock.Setup(s => s.GetDeliveryMultipliersBySpotLengthId())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoveragesWithStations())
                .Returns(_GetMarketCoverages());

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, BasePlanInventoryProgram.ManifestDaypart.Program>
                {
                    {
                        1001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 1001 Morning NEWS"
                        }
                    },
                    {
                        1002,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 1002 Morning NEWS"
                        }
                    },
                    {
                        2001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 2001 Midday NEWS"
                        }
                    },
                     {
                        2002,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 2002 Midday NEWS"
                        }
                    },
                     {
                        3001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 3001 Evening NEWS"
                        }
                    },
                     {
                        3002,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 3002 Evening NEWS"
                        }
                    },
                     {
                        4001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 4001 Late NEWS"
                        }
                    },
                     {
                        4002,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 4002 Late NEWS"
                        }
                    },
                });
        }

        private List<PlanBuyingAllocatedSpot> _GetPlanBuyingAllocatedSpots()
        {
            return new List<PlanBuyingAllocatedSpot>
            {
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 10,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 10001,
                        Code = "EMN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 20001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 20
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 30002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 2,
                            Impressions = 10,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 2,
                            Impressions = 10,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 2,
                            Impressions = 10,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 40001,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 50,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 50001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 60,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 60001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 70,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 60001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                }
            };
        }

        private List<PlanBuyingAllocatedSpot> _GetPlanBuyingAllocatedSpotsForRollup()
        {
            return new List<PlanBuyingAllocatedSpot>
            {
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 10,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 10001,
                        Code = "EMN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 10,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 10002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 20001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 20
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 20002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 20
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 30001,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 30002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 40001,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },new PlanBuyingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 40002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                }
            };
        }

        private List<StationInventoryManifest> _GetStationInventoryManifestsForRollup()
        {
            return new List<StationInventoryManifest>
                {
                    new StationInventoryManifest
                    {
                        Id = 10,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 100,
                            LegacyCallLetters = "KSTP",
                            Affiliation = "ABC"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 1001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 10001,
                                    StartTime = 14400,
                                    EndTime = 35999,
                                    Monday = true,
                                    Tuesday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 20,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 200,
                            LegacyCallLetters = "WTTV",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 2001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 20001,
                                    StartTime = 39600,
                                    EndTime = 46799,
                                    Wednesday = true,
                                    Thursday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 30,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 300,
                            LegacyCallLetters = "KELO",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 3001,
                                ProgramName = "The Shawshank Redemption",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 30001,
                                    StartTime = 57600,
                                    EndTime = 68399,
                                    Friday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 40,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 300,
                            LegacyCallLetters = "KELOW",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 4001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 40001,
                                    StartTime = 72000,
                                    EndTime = 299,
                                    Friday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                };
        }

        private List<StationInventoryManifest> _GetStationInventoryManifests()
        {
            return new List<StationInventoryManifest>
                {
                    new StationInventoryManifest
                    {
                        Id = 10,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 100,
                            LegacyCallLetters = "KSTP",
                            Affiliation = "ABC"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 1001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 10001,
                                    StartTime = 100,
                                    EndTime = 199,
                                    Monday = true,
                                    Tuesday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 20,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 200,
                            LegacyCallLetters = "WTTV",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 2001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 20001,
                                    StartTime = 200,
                                    EndTime = 299,
                                    Wednesday = true,
                                    Thursday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 30,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 300,
                            LegacyCallLetters = "KELO",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 3001,
                                ProgramName = "The Shawshank Redemption",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 30001,
                                    StartTime = 300,
                                    EndTime = 399,
                                    Friday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 40,
                        Station = null
                    },
                    new StationInventoryManifest
                    {
                        Id = 50,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = null
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 60,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 200,
                            LegacyCallLetters = "WTTV",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 6001,
                                ProgramName = "Fallback program",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 60001,
                                    StartTime = 200,
                                    EndTime = 299,
                                    Saturday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 70,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 200,
                            LegacyCallLetters = "WTTV",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 6001,
                                ProgramName = "Fallback program",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 60001,
                                    StartTime = 200,
                                    EndTime = 299,
                                    Saturday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                };
        }

        private MarketCoverageByStation _GetMarketCoverages()
        {
            return new MarketCoverageByStation
            {
                Markets = new List<MarketCoverageByStation.Market>
                {
                    new MarketCoverageByStation.Market
                    {
                        MarketCode = 100,
                        Rank = 2,
                        MarketName = "Binghamton",
                        Stations = new List<MarketCoverageByStation.Station>
                        {
                            new MarketCoverageByStation.Station
                            {
                                Id = 101,
                                LegacyCallLetters = "A101"
                            }
                        }
                    },
                    new MarketCoverageByStation.Market
                    {
                        MarketCode = 200,
                        Rank = 1,
                        MarketName = "New York",
                        Stations = new List<MarketCoverageByStation.Station>
                        {
                            new MarketCoverageByStation.Station
                            {
                                Id = 201,
                                LegacyCallLetters = "B201"
                            }
                        }
                    },
                    new MarketCoverageByStation.Market
                    {
                        MarketCode = 300,
                        Rank = 3,
                        MarketName = "Macon",
                        Stations = new List<MarketCoverageByStation.Station>
                        {
                            new MarketCoverageByStation.Station
                            {
                                Id = 301,
                                LegacyCallLetters = "C301"
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public void GetPlanBuyingGoals_BuyingParameters_Exist_NSI()
        {
            // Arrange
            const int planId = 1197;
            PostingTypeEnum postingType = PostingTypeEnum.NSI;

            _PlanRepositoryMock
                    .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                    .Returns(new PlanDto()
                    {
                        Id = planId,
                        PostingType = postingType,
                        BuyingParameters = _GetPlanBuyingParametersDto()
                    });

            var tc = _GetService();

            // Act
            var result = tc.GetPlanBuyingGoals(planId, postingType);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()), Times.Never);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetPlanBuyingGoals_BuyingParameters_Exist_NTI()
        {
            // Arrange
            const int planId = 1197;
            PostingTypeEnum postingType = PostingTypeEnum.NTI;

            _PlanRepositoryMock
                    .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                    .Returns(new PlanDto()
                    {
                        Id = planId,
                        PostingType = postingType,
                        BuyingParameters = _GetPlanBuyingParametersDto(postingType)
                    });

            var tc = _GetService();

            // Act
            var result = tc.GetPlanBuyingGoals(planId, postingType);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()), Times.Never);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetPlanBuyingGoals_BuyingParameters_Exist_PlanPostingTypeDifferent()
        {
            // Arrange
            const int planId = 1197;

            PostingTypeEnum postingType = PostingTypeEnum.NSI;
            PostingTypeEnum planPostingType = PostingTypeEnum.NTI;

            _PlanRepositoryMock
                    .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                    .Returns(new PlanDto()
                    {
                        Id = planId,
                        PostingType = planPostingType,
                        Dayparts = _GetPlanDayparts(),
                        BuyingParameters = _GetPlanBuyingParametersDto(planPostingType)
                    });

            _PlanRepositoryMock
                    .Setup(x => x.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()))
                    .Returns(2);

            var tc = _GetService();

            // Act
            var result = tc.GetPlanBuyingGoals(planId, postingType);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()), Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetPlanBuyingGoals_BuyingParameters_Exist_BuyingPostingTypeDifferentThanPlans()
        {
            // Arrange
            const int planId = 1197;

            PostingTypeEnum postingType = PostingTypeEnum.NSI;
            PostingTypeEnum planPostingType = PostingTypeEnum.NTI;

            _PlanRepositoryMock
                    .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                    .Returns(new PlanDto()
                    {
                        Id = planId,
                        PostingType = planPostingType,
                        Dayparts = _GetPlanDayparts(),
                        BuyingParameters = _GetPlanBuyingParametersDto(postingType)
                    });

            _PlanRepositoryMock
                    .Setup(x => x.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()))
                    .Returns(2);

            var tc = _GetService();

            // Act
            var result = tc.GetPlanBuyingGoals(planId, postingType);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()), Times.Never);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetPlanBuyingGoals_BuyingParameters_DontExist()
        {
            // Arrange
            const int planId = 1197;
            PostingTypeEnum postingType = PostingTypeEnum.NSI;

            _PlanRepositoryMock
                    .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                    .Returns(new PlanDto()
                    {
                        Id = planId,
                        Budget = 2000,
                        TargetCPM = 10,
                        TargetCPP = 10,
                        Currency = PlanCurrenciesEnum.Impressions,
                        TargetImpressions = 60000,
                        TargetRatingPoints = 1000,
                        VersionId = 77,
                        PostingType = PostingTypeEnum.NSI,
                        Dayparts = _GetPlanDayparts()
                    });

            var tc = _GetService();

            // Act
            var result = tc.GetPlanBuyingGoals(planId, postingType);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()), Times.Never);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetPlanBuyingGoals_BuyingParameters_DontExist_PlanPostingTypeDifferent()
        {
            // Arrange
            const int planId = 1197;
            PostingTypeEnum postingType = PostingTypeEnum.NSI;

            _PlanRepositoryMock
                    .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                    .Returns(new PlanDto()
                    {
                        Id = planId,
                        Budget = 2000,
                        TargetCPM = 10,
                        TargetCPP = 10,
                        Currency = PlanCurrenciesEnum.Impressions,
                        TargetImpressions = 60000,
                        TargetRatingPoints = 1000,
                        VersionId = 77,
                        PostingType = PostingTypeEnum.NTI,
                        Dayparts = _GetPlanDayparts()
                    });

            var tc = _GetService();

            // Act
            var result = tc.GetPlanBuyingGoals(planId, postingType);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetNsiToNtiConversionRate(It.IsAny<List<PlanDaypartDto>>()), Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllCurrentBuyingExecutions_v2_ForPlanId()
        {
            // Arrange
            const int planId = 128;
            const int JobID = 506;
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(planId))
                .Returns(new PlanBuyingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Id = JobID,
                    HangfireJobId = "9895",
                    PlanVersionId = 659,
                    Queued = new DateTime(2021, 3, 24, 11, 12, 13),
                    Completed = new DateTime(2021, 3, 24, 11, 13, 13),
                    ErrorMessage = null,
                    DiagnosticResult = "DiagnosticResult"
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingResultsByJobId(JobID, PostingTypeEnum.NSI))
                .Returns(_GetCurrentBuyingExecutionsResults());

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetGoalCpm(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PostingTypeEnum>())).Returns(6.75M);

            // TODO SDE : this should be reworked for these to be true, as they are in production
            var service = _GetService(false,true);

            // Act
            var result = service.GetCurrentBuyingExecution_v2(planId, PostingTypeEnum.NSI);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAllCurrentBuyingExecutions_v2_WhenJobIsRunning()
        {
            // Arrange
            const int planId = 128;
            const int JobID = 506;
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(planId))
                .Returns(new PlanBuyingJob
                {
                    Status = BackgroundJobProcessingStatus.Queued,
                    Id = JobID,
                    HangfireJobId = "9895",
                    PlanVersionId = 659,
                    Queued = new DateTime(2021, 3, 24, 11, 12, 13),
                    Completed = null,
                    ErrorMessage = null,
                    DiagnosticResult = null
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingResultsByJobId(JobID, PostingTypeEnum.NSI))
                .Returns(_GetCurrentBuyingExecutionsResults());

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetGoalCpm(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PostingTypeEnum>())).Returns(6.75M);

            var service = _GetService();

            // Act
            var result = service.GetCurrentBuyingExecution_v2(planId, PostingTypeEnum.NSI);

            // Assert
            Assert.AreEqual(true, result.IsBuyingModelRunning);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void _MapBuyingRawInventory_SingleFrequency()
        {
            // Arrange
            var allocationResult = new List<PlanBuyingAllocationResult>
            {
                new PlanBuyingAllocationResult
                {
                    BuyingCpm = 4.6841m,
                    JobId = 1,
                    BuyingVersion = "4",
                    PlanVersionId = 1172,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    AllocatedSpots = new List<PlanBuyingAllocatedSpot>
                    {
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 1, StationInventoryManifestId = 11,
                            ContractMediaWeek = new MediaWeek { Id = 871 },
                            InventoryMediaWeek = new MediaWeek { Id = 871 },
                            StandardDaypart = new StandardDaypartDto{ Id = 10},
                            Impressions30sec = 100,
                            SpotFrequencies = new List<SpotFrequency>
                                {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                        }
                    },
                    UnallocatedSpots = new List<PlanBuyingAllocatedSpot>
                    {
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 1, StationInventoryManifestId = 11,
                            ContractMediaWeek = new MediaWeek { Id = 871 },
                            InventoryMediaWeek = new MediaWeek { Id = 871 },
                            StandardDaypart = new StandardDaypartDto{ Id = 15},
                            Impressions30sec = 200,
                            SpotFrequencies = new List<SpotFrequency>
                                {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                        }
                    }
                },
                new PlanBuyingAllocationResult
                {
                    BuyingCpm = 4.6841m,
                    JobId = 1,
                    BuyingVersion = "4",
                    PlanVersionId = 1172,
                    SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                    PostingType = PostingTypeEnum.NSI,
                    AllocatedSpots = new List<PlanBuyingAllocatedSpot>
                    {
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 1, StationInventoryManifestId = 11,
                            ContractMediaWeek = new MediaWeek { Id = 871 },
                            InventoryMediaWeek = new MediaWeek { Id = 871 },
                            StandardDaypart = new StandardDaypartDto{ Id = 1},
                            Impressions30sec = 300,
                            SpotFrequencies = new List<SpotFrequency>
                                {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                        }
                    },
                    UnallocatedSpots = new List<PlanBuyingAllocatedSpot>
                    {
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 1, StationInventoryManifestId = 11,
                            ContractMediaWeek = new MediaWeek { Id = 871 },
                            InventoryMediaWeek = new MediaWeek { Id = 871 },
                            StandardDaypart = new StandardDaypartDto{ Id = 1},
                            Impressions30sec = 400,
                            SpotFrequencies = new List<SpotFrequency>
                                {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                        }
                    }
                }
            };

            var inventory = new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        NsiToNtiImpressionConversionRate = .8
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 10,
                        NsiToNtiImpressionConversionRate = .75
                    }
                };

            // Act
            var service = _GetService();
            var result = service._MapBuyingRawInventory(allocationResult, inventory);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void _MapBuyingRawInventory_MultipleFrequencies()
        {
            // Arrange
            var allocationResult = new List<PlanBuyingAllocationResult>
            {
                new PlanBuyingAllocationResult
                {
                    BuyingCpm = 4.6841m,
                    JobId = 1,
                    BuyingVersion = "4",
                    PlanVersionId = 1172,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    AllocatedSpots = new List<PlanBuyingAllocatedSpot>
                    {
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 1, StationInventoryManifestId = 11,
                            ContractMediaWeek = new MediaWeek { Id = 871 },
                            InventoryMediaWeek = new MediaWeek { Id = 871 },
                            StandardDaypart = new StandardDaypartDto{ Id = 10},
                            Impressions30sec = 100,
                            SpotFrequencies = new List<SpotFrequency>
                            {
                                new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3},
                                new SpotFrequency {SpotLengthId = 2, SpotCost = 20, Impressions = 2400, Spots = 3}
                            }
                        }
                    },
                    UnallocatedSpots = new List<PlanBuyingAllocatedSpot>
                    {
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 1, StationInventoryManifestId = 11,
                            ContractMediaWeek = new MediaWeek { Id = 871 },
                            InventoryMediaWeek = new MediaWeek { Id = 871 },
                            StandardDaypart = new StandardDaypartDto{ Id = 15},
                            Impressions30sec = 200,
                            SpotFrequencies = new List<SpotFrequency>
                            {
                                new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3},
                                new SpotFrequency {SpotLengthId = 2, SpotCost = 20, Impressions = 2400, Spots = 3}
                            }
                        }
                    }
                },
                new PlanBuyingAllocationResult
                {
                    BuyingCpm = 4.6841m,
                    JobId = 1,
                    BuyingVersion = "4",
                    PlanVersionId = 1172,
                    SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                    PostingType = PostingTypeEnum.NSI,
                    AllocatedSpots = new List<PlanBuyingAllocatedSpot>
                    {
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 1, StationInventoryManifestId = 11,
                            ContractMediaWeek = new MediaWeek { Id = 871 },
                            InventoryMediaWeek = new MediaWeek { Id = 871 },
                            StandardDaypart = new StandardDaypartDto{ Id = 1},
                            Impressions30sec = 300,
                            SpotFrequencies = new List<SpotFrequency>
                                {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                        }
                    },
                    UnallocatedSpots = new List<PlanBuyingAllocatedSpot>
                    {
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 1, StationInventoryManifestId = 11,
                            ContractMediaWeek = new MediaWeek { Id = 871 },
                            InventoryMediaWeek = new MediaWeek { Id = 871 },
                            StandardDaypart = new StandardDaypartDto{ Id = 1},
                            Impressions30sec = 400,
                            SpotFrequencies = new List<SpotFrequency>
                                {new SpotFrequency {SpotLengthId = 1, SpotCost = 20, Impressions = 2400, Spots = 3}}
                        }
                    }
                }
            };

            var inventory = new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        NsiToNtiImpressionConversionRate = .8
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 10,
                        NsiToNtiImpressionConversionRate = .75
                    }
                };

            // Act
            var service = _GetService();
            var result = service._MapBuyingRawInventory(allocationResult, inventory);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ValidateBuyingExecutionResultTest()
        {
            int expectedResult = 2;
            var result = new CurrentBuyingExecutions
            {
                Job = new PlanBuyingJob
                {
                    Id = 1,
                    HangfireJobId = "11268",
                    PlanVersionId = 801,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Queued = new DateTime(2011, 05, 22),
                    Completed = new DateTime(2011, 05, 22),
                    ErrorMessage = null,
                    DiagnosticResult = "setting job status to processing"
                },
                Results = new List<CurrentBuyingExecutionResultDto>
                {
                                 new CurrentBuyingExecutionResultDto
                                    {
                                        //Id = 15,
                                        OptimalCpm=21,
                                        JobId=755,
                                        PlanVersionId=805,
                                        GoalFulfilledByProprietary=false,
                                        Notes="",
                                        HasResults=true,
                                        CpmPercentage=4,
                                        PostingType=PostingTypeEnum.NTI,
                                        SpotAllocationModelMode=SpotAllocationModelMode.Efficiency
                                    }
                }
            };

            var service = _GetService();
            // Act
            var results = service._ValidateBuyingExecutionResult(result, expectedResult);
            // Assert     
            Assert.AreEqual(results.IsBuyingModelRunning, true);
            Assert.AreEqual(1, result.Results.Count());
            Assert.IsFalse(result.Results[0].HasResults);
            Assert.IsNull(result.Results[0].JobId);
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Succeeded, 3, true)]
        [TestCase(BackgroundJobProcessingStatus.Succeeded, 5, true)]
        [TestCase(BackgroundJobProcessingStatus.Succeeded, 6, false)]
        [TestCase(BackgroundJobProcessingStatus.Failed, 3, true)]
        [TestCase(BackgroundJobProcessingStatus.Failed, 5, true)]
        [TestCase(BackgroundJobProcessingStatus.Failed, 6, false)]
        [TestCase(BackgroundJobProcessingStatus.Canceled, 3, true)]
        [TestCase(BackgroundJobProcessingStatus.Canceled, 5, true)]
        [TestCase(BackgroundJobProcessingStatus.Canceled, 6, false)]
        [TestCase(BackgroundJobProcessingStatus.Processing, 3, false)]
        [TestCase(BackgroundJobProcessingStatus.Queued, 3, false)]
        public void DidBuyingJobCompleteWithinThreshold(BackgroundJobProcessingStatus status, int completedMinutesAgo, bool expectedResult)
        {
            var currentDateTime = new DateTime(2021, 5, 14, 12, 30, 0);
            var completedMinutes = currentDateTime.Minute - completedMinutesAgo;

            DateTime? completedWhen = null;
            if (status != BackgroundJobProcessingStatus.Processing &&
                status != BackgroundJobProcessingStatus.Queued)
            {
                completedWhen = new DateTime(2021, 5, 14, 12, completedMinutes, 0);
            }

            var job = new PlanBuyingJob
            {
                Id = 1,
                Status = status,
                Completed = completedWhen
            };

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            var service = _GetService();

            var result = service._DidBuyingJobCompleteWithinThreshold(job, thresholdMinutes: 5);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(3, PostingTypeEnum.NTI)]
        public void FillInMissingBuyingResultsWithEmptyResults(int startingResultCount, PostingTypeEnum postingType)
        {
            // only the one posting type ever.


            // Arrange
            var candidateResults = new List<CurrentBuyingExecutionResultDto>();

            candidateResults.Add(new CurrentBuyingExecutionResultDto
            {
                PostingType = postingType,
                SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                OptimalCpm = 23
            });

            if (startingResultCount > 1)
            {
                candidateResults.Add(new CurrentBuyingExecutionResultDto
                {
                    PostingType = postingType,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    OptimalCpm = 23
                });

                candidateResults.Add(new CurrentBuyingExecutionResultDto
                {
                    PostingType = postingType,
                    SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                    OptimalCpm = 23
                });
            }

            var service = _GetService();

            // Act
            var results = service._FillInMissingBuyingResultsWithEmptyResults(candidateResults, postingType);

            // Assert

            Assert.IsTrue(results.Any(a => a.PostingType == postingType && a.SpotAllocationModelMode == SpotAllocationModelMode.Efficiency));
            Assert.IsTrue(results.Any(a => a.PostingType == postingType && a.SpotAllocationModelMode == SpotAllocationModelMode.Floor));
        }
        [Test]
        public void GetStations()
        {
            // Arrange
            int stationResultCount = 0;
            const int planId = 224;
            PostingTypeEnum postingType = PostingTypeEnum.NSI;

            _PlanBuyingRepositoryMock.Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                    .Returns(new PlanBuyingJob()
                    {
                        Id = 4,
                        HangfireJobId = "10980",
                        PlanVersionId = 879,
                        Status = BackgroundJobProcessingStatus.Succeeded,
                        Queued = new DateTime(2021, 08, 16),
                        Completed = new DateTime(2021, 08, 16),
                        ErrorMessage = null,
                        DiagnosticResult = null,
                    });
            _PlanBuyingRepositoryMock.Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                   .Returns(new PlanBuyingStationResultDto()
                   {
                       Id = 8,
                       BuyingJobId = 4,
                       PlanVersionId = 879,
                       PostingType = PostingTypeEnum.NSI,
                       SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                       Details = new List<PlanBuyingStationDto>
                       {
                            new PlanBuyingStationDto
                            {
                                Station ="WDCW" ,
                                Market = "Washington, Dc (Hagrstwn)",
                                Spots = 19,
                                Impressions = 114400,
                                Cpm = 6.8291M,
                                Budget =781.25M ,
                                ImpressionsPercentage =5.32935,
                                Affiliate = "CW",
                                RepFirm = null,
                                OwnerName = null,
                                LegacyCallLetters ="WDCW"
                            },
                             new PlanBuyingStationDto
                            {
                                Station ="WDCW" ,
                                Market = "Washington, Dc (Hagrstwn)",
                                Spots = 19,
                                Impressions = 114400,
                                Cpm = 6.8291M,
                                Budget =781.25M ,
                                ImpressionsPercentage =5.32935,
                                Affiliate = "CW",
                                RepFirm = "Katz",
                                OwnerName = "Lockwood Broadcasting Inc.",
                                LegacyCallLetters ="WDCW"
                            }
                       },
                       Totals = new PlanBuyingProgramTotalsDto
                       {
                           Budget = 17365,
                           AvgCpm = 8.0895M,
                           Impressions = 2146600,
                           SpotCount = 749,
                           StationCount = 108
                       },


                   });
            _PlanRepositoryMock
                    .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                    .Returns(new PlanDto()
                    {
                        Id = planId,
                        Budget = 2000,
                        TargetCPM = 10,
                        TargetCPP = 10,
                        Currency = PlanCurrenciesEnum.Impressions,
                        TargetImpressions = 60000,
                        TargetRatingPoints = 1000,
                        VersionId = 77,
                        PostingType = PostingTypeEnum.NSI,
                        Dayparts = _GetPlanDayparts()
                    });
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
               .Returns(new PlanBuyingStationResultDto()
               {
                   BuyingJobId = 24,
                   PlanVersionId = 95,
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   PostingType = PostingTypeEnum.NSI,
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       Budget = 3912,
                       AvgCpm = 2,
                       Impressions = 2041.65,
                       SpotCount = 300,
                       StationCount = 46
                   },
                   Details = new List<PlanBuyingStationDto>()
                   {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "HMTV",
                        OwnerName = "HMTV",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "Lockwood Broadcasting Inc.",
                        LegacyCallLetters = "HMTV"
                      }
                   }
               }).Callback<PlanBuyingStationResultDto>(element =>
               {
                   stationResultCount = element.Details.Count;
               });

            var tc = _GetService();

            // Act
            var result = tc.GetStations(planId, postingType);

            // Assert
            Assert.AreEqual(result.Details[0].LegacyCallLetters, result.Details[0].RepFirm);
            Assert.AreEqual(result.Details[0].LegacyCallLetters, result.Details[0].OwnerName);
            Assert.AreEqual("TEGNA", result.Details[1].RepFirm);
            Assert.AreEqual("Lockwood Broadcasting Inc.", result.Details[1].OwnerName);
        }
        private List<CurrentBuyingExecutionResultDto> _GetCurrentBuyingExecutionsResults()
        {
            return new List<CurrentBuyingExecutionResultDto>
                    {
                        new CurrentBuyingExecutionResultDto()
                        {
                            OptimalCpm = 11,
                            JobId = 12,
                            PlanVersionId = 659,
                            GoalFulfilledByProprietary = false,
                            Notes = "",
                            HasResults = true,
                            CpmPercentage = 63,
                            PostingType = PostingTypeEnum.NSI,
                            SpotAllocationModelMode = SpotAllocationModelMode.Floor
                        },
                        new CurrentBuyingExecutionResultDto()
                        {
                            OptimalCpm = 8,
                            JobId = 506,
                            PlanVersionId = 659,
                            GoalFulfilledByProprietary = false,
                            Notes = "",
                            HasResults = true,
                            CpmPercentage = 47,
                            PostingType = PostingTypeEnum.NSI,
                            SpotAllocationModelMode = SpotAllocationModelMode.Efficiency
                        },
                        new CurrentBuyingExecutionResultDto()
                        {
                            OptimalCpm = 20,
                            JobId = 506,
                            PlanVersionId = 659,
                            GoalFulfilledByProprietary = false,
                            Notes = "",
                            HasResults = true,
                            CpmPercentage = 112,
                            PostingType = PostingTypeEnum.NSI,
                            SpotAllocationModelMode = SpotAllocationModelMode.Efficiency
                        }
                };
        }

        [Test]       
        [TestCase(false, false, 1)]
        [TestCase(true, false, 2)]
        [TestCase(true, true, 3)]
        public void GetSupportedInventorySourceTypes(bool isPricingModelBarterInventoryEnabled, bool isPricingModelProprietaryOAndOInventoryEnabled, int expectedResult)
        {
            // Arrange
            var service = _GetService();

            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.PRICING_MODEL_BARTER_INVENTORY] = isPricingModelBarterInventoryEnabled;
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY] = isPricingModelProprietaryOAndOInventoryEnabled;

            // Act
            var results = service._GetSupportedInventorySourceTypes();

            // Assert            
            Assert.AreEqual(expectedResult, results.Count());
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetResultRepFirms()
        {
            int stationResultCount = 0;
            // Arrange
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingStationResultDto
                {
                    Details = new List<PlanBuyingStationDto>()
                {
                  new PlanBuyingStationDto()
                  {
                  RepFirm = "Sales group 3"
                  },
                  new PlanBuyingStationDto()
                  {
                  RepFirm = "Sales group 4"
                  }
                }
                });
            var service = _GetService();
            var plan = _GetPlan();
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
             .Returns(new PlanBuyingStationResultDto()
             {
                 BuyingJobId = 24,
                 PlanVersionId = 95,
                 SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                 PostingType = PostingTypeEnum.NSI,
                 Totals = new PlanBuyingProgramTotalsDto()
                 {
                     Budget = 3912,
                     AvgCpm = 2,
                     Impressions = 2041.65,
                     SpotCount = 300,
                     StationCount = 46
                 },
                 Details = new List<PlanBuyingStationDto>()
                 {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      }
                 }
             }).Callback<PlanBuyingStationResultDto>(element =>
             {
                 stationResultCount = element.Details.Count;
             });
            // Act
            var result = service.GetResultRepFirms(plan.Id, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetResultOwnershipGroups()
        {
            int stationResultCount = 0;
            // Arrange
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingStationResultDto
                {
                    Details = new List<PlanBuyingStationDto>()
                {
                  new PlanBuyingStationDto()
                  {
                    OwnerName = "Ownership group 3"
                  },
                  new PlanBuyingStationDto()
                  {
                    OwnerName = "NASY"
                  },
                  new PlanBuyingStationDto()
                  {
                    OwnerName = "NASY"
                  }
                }
                });
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
              .Returns(new PlanBuyingStationResultDto()
              {
                  BuyingJobId = 24,
                  PlanVersionId = 95,
                  SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                  PostingType = PostingTypeEnum.NSI,
                  Totals = new PlanBuyingProgramTotalsDto()
                  {
                      Budget = 3912,
                      AvgCpm = 2,
                      Impressions = 2041.65,
                      SpotCount = 300,
                      StationCount = 46
                  },
                  Details = new List<PlanBuyingStationDto>()
                  {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      }
                  }
              }).Callback<PlanBuyingStationResultDto>(element =>
              {
                  stationResultCount = element.Details.Count;
              });

            var service = _GetService();
            var Id = 1197;

            // Act
            var result = service.GetResultOwnershipGroups(Id, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async void SavesBuyingAggregateResults_WhenRunningBuyingJob_SaveProgramStations()
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
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanBuyingRepositoryMock
               .Setup(x => x.GetPlanBuyingJob(It.IsAny<int>()))
               .Returns(new PlanBuyingJob
               {
                   Id = jobId
               });
            _PlanRepositoryMock
               .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
               .Returns(new PlanDto
               {
                   VersionId = 77,
                   CoverageGoalPercent = 80,
                   PostingType = PostingTypeEnum.NSI,
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
                   FlightStartDate = new DateTime(2005, 11, 21),
                   FlightEndDate = new DateTime(2005, 12, 18),
                   FlightDays = new List<int>(),
                   TargetRatingPoints = 1000,
                   Dayparts = new List<PlanDaypartDto>
                   {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60,
                            DaypartTypeId = DaypartTypeEnum.News
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40,
                            DaypartTypeId = DaypartTypeEnum.News
                        }
                   },
                   BuyingParameters = _GetPlanBuyingParametersDto(),
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
                   },
                   ImpressionsPerUnit = 1
               });

            _WeeklyBreakdownEngineMock
               .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
               .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _PlanBuyingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanBuyingJobDiagnostic>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        PostingType = PostingTypeEnum.NSI,
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
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        PostingType = PostingTypeEnum.NSI,
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
                                InventoryMediaWeekId = 90
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        PostingType = PostingTypeEnum.NSI,
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
                        PostingType = PostingTypeEnum.NSI,
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
                });

            _MarketCoverageRepositoryMock
              .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
              .Returns(_GetLatestMarketCoverages());

            _BuyingApiClientMock
             .Setup(x => x.GetBuyingSpotsResultAsync(It.IsAny<PlanBuyingApiRequestDto_v3>()))
             .Returns(Task.FromResult(new PlanBuyingApiSpotsResponseDto_v3
             {
                 RequestId = "#q1w2e3",
                 Results = new List<PlanBuyingApiSpotsResultDto_v3>
                 {
                        new PlanBuyingApiSpotsResultDto_v3
                        {
                            ManifestId = 1,
                            MediaWeekId = 100,
                            Frequencies = new List<SpotFrequencyResponse>
                            {
                                new SpotFrequencyResponse
                                {
                                    SpotLengthId = 1,
                                    Frequency = 1
                                }
                            }
                        },
                        new PlanBuyingApiSpotsResultDto_v3
                        {
                            ManifestId = 1,
                            MediaWeekId = 101,
                            Frequencies = new List<SpotFrequencyResponse>
                            {
                                new SpotFrequencyResponse
                                {
                                    SpotLengthId = 1,
                                    Frequency = 2
                                }
                            }
                        },
                        new PlanBuyingApiSpotsResultDto_v3
                        {
                            ManifestId = 2,
                            MediaWeekId = 100,
                            Frequencies = new List<SpotFrequencyResponse>
                            {
                                new SpotFrequencyResponse
                                {
                                    SpotLengthId = 1,
                                    Frequency = 3
                                }
                            }
                        }
                 }
             }));

            _MediaMonthAndWeekAggregateCacheMock
              .Setup(x => x.GetMediaWeekById(It.IsAny<int>()))
              .Returns<int>(weekId => new MediaWeek
              {
                  Id = weekId
              });

            var planVersionBuyingResultId = 100;
            _PlanBuyingRepositoryMock
                .Setup(x => x.SaveBuyingAggregateResults(It.IsAny<PlanBuyingResultBaseDto>()))
                .Callback(() => planVersionBuyingResultId++)
                .Returns<PlanBuyingResultBaseDto>(c => planVersionBuyingResultId);

            var passedParameters = new List<PlanBuyingResultBaseDto>();
            var planVersionBuyingResultIds = new List<int>();
            _PlanBuyingRepositoryMock
                .Setup(x => x.SavePlanBuyingResultSpotStations(It.IsAny<int>(), It.IsAny<PlanBuyingResultBaseDto>()))
                .Callback<int, PlanBuyingResultBaseDto>((p1, p2) =>
                {
                    planVersionBuyingResultIds.Add(p1);
                    passedParameters.Add(p2);
                });

            _PlanBuyingProgramEngine.Setup(s => s.Calculate(
                   It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                   It.IsAny<bool>()))
               .Returns(new PlanBuyingResultBaseDto
               {
                   GoalFulfilledByProprietary = false,
                   JobId = 1,
                   OptimalCpm = 0.1682953311617806731813246000m,
                   PlanVersionId = 77,
                   Programs = new List<PlanBuyingProgramDto>
                   {
                        new PlanBuyingProgramDto
                        {
                            AvgCpm = 62.5000m,
                            AvgImpressions = 1000,
                            Budget = 187.5m,
                            Genre = "Sport",
                            Id = 0,
                            Impressions = 3000,
                            MarketCount = 1,
                            PercentageOfBuy = 40,
                            ProgramName = "seinfeld_2",
                            SpotCount = 3,
                            StationCount = 1
                        },
                        new PlanBuyingProgramDto
                        {
                            AvgCpm = 50.00m,
                            AvgImpressions = 1500,
                            Budget = 225m,
                            Genre = "News",
                            Id = 0,
                            Impressions = 4500,
                            MarketCount = 1,
                            PercentageOfBuy = 60,
                            ProgramName = "seinfeld",
                            SpotCount = 3,
                            StationCount = 1
                        }
                   },
                   Totals = new PlanBuyingProgramTotalsDto
                   {
                       AvgCpm = 55.000m,
                       AvgImpressions = 1250,
                       Budget = 412.5m,
                       Impressions = 7500,
                       ImpressionsPercentage = 0,
                       MarketCount = 2,
                       SpotCount = 6,
                       StationCount = 2
                   }
               });

            _PlanBuyingProgramEngine.Setup(s => s.CalculateProgramStations(
                   It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                   It.IsAny<bool>()))
               .Returns(new PlanBuyingResultBaseDto
               {
                   GoalFulfilledByProprietary = false,
                   JobId = 1,
                   OptimalCpm = 0.1682953311617806731813246000m,
                   PlanVersionId = 77,
                   Programs = new List<PlanBuyingProgramDto>
                   {
                        new PlanBuyingProgramDto
                        {
                            Budget = 187.5m,
                            Genre = "Sport",
                            Impressions = 3000,
                            ProgramName = "seinfeld_2",
                            SpotCount = 3,
                            Station = "WABC"
                        },
                        new PlanBuyingProgramDto
                        {
                            Budget = 225m,
                            Genre = "News",
                            Impressions = 4500,
                            ProgramName = "seinfeld",
                            SpotCount = 3,
                            Station = "EUVP"
                        }
                   }
               });

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.Calculate(
                    It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                    It.IsAny<PlanBuyingParametersDto>()))
                .Returns(new PlanBuyingBandsDto());

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.CalculateBandInventoryStation(
                    It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>()))
                .Returns(new PlanBuyingBandInventoryStationsDto());

            _PlanBuyingStationEngineMock.Setup(s =>
                    s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                        It.IsAny<PlanBuyingParametersDto>()))
                .Returns(new PlanBuyingStationResultDto());

            _PlanBuyingMarketResultsEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>(),
                    It.IsAny<PlanDto>(), It.IsAny<List<MarketCoverage>>()))
                .Returns(new PlanBuyingResultMarketsDto());

            _PlanBuyingOwnershipGroupEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>()
                   ))
                .Returns(new PlanBuyingResultOwnershipGroupDto());

            _PlanBuyingRepFirmEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>()
                   ))
                .Returns(new PlanBuyingResultRepFirmDto());

            var service = _GetService();
            // Act
            await service.RunBuyingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Assert.IsTrue(planVersionBuyingResultIds.Count(x => x <= 100) == 0);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStationsWithFilter()
        {
            // Arrange
            int stationResultCount = 0;
            int expectedCount = 2;
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingStationResultDto
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 3912,
                        AvgCpm = 2,
                        Impressions = 2041.65,
                        SpotCount = 300,
                        StationCount = 46
                    },
                    Details = new List<PlanBuyingStationDto>()
                    {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 8765,
                        Cpm = 9,
                        Impressions = 9876,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "Cox",
                        OwnerName = null,
                        LegacyCallLetters = "HMTV"
                      }
                    }
                });
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
               .Returns(new PlanBuyingStationResultDto()
               {
                   BuyingJobId = 24,
                   PlanVersionId = 95,
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   PostingType = PostingTypeEnum.NSI,
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       Budget = 3912,
                       AvgCpm = 2,
                       Impressions = 2041.65,
                       SpotCount = 300,
                       StationCount = 46
                   },
                   Details = new List<PlanBuyingStationDto>()
                   {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      }
                   }
               }).Callback<PlanBuyingStationResultDto>(element =>
               {
                   stationResultCount = element.Details.Count;
               });
            _PlanBuyingStationEngineMock.Setup(s => s.ConvertImpressionsToUserFormat(It.IsAny<PlanBuyingStationResultDto>()))
                .Callback<PlanBuyingStationResultDto>(element =>
                {
                    element.Totals.Impressions /= 1000;
                    foreach (var detail in element.Details)
                    {
                        detail.Impressions /= 1000;
                    }
                });
            var service = _GetService();
            var Id = 1197;
            var planBuyingFilter = new PlanBuyingFilterDto()
            {
                OwnerNames = new List<string> { "TEGNA", "Newsweb Corporation/Channel 3 TV Company LLC" },
                RepFirmNames = new List<string> { "TEGNA", "Direct" }
            };

            // Act
            var result = service.GetStations(Id, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, planBuyingFilter);

            // Assert
            Assert.AreEqual(expectedCount, stationResultCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBuyingOwnershipGroupsWithFilter()
        {
            // Arrange
            int stationResultCount = 0;
            int expectedCount = 2;
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingStationResultDto
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 3912,
                        AvgCpm = 2,
                        Impressions = 2041.65,
                        SpotCount = 300,
                        StationCount = 46
                    },
                    Details = new List<PlanBuyingStationDto>()
                    {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 8765,
                        Cpm = 9,
                        Impressions = 9876,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "Cox",
                        OwnerName = null,
                        LegacyCallLetters = "HMTV"
                      }
                    }
                });
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
               .Returns(new PlanBuyingStationResultDto()
               {
                   BuyingJobId = 24,
                   PlanVersionId = 95,
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   PostingType = PostingTypeEnum.NSI,
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       Budget = 3912,
                       AvgCpm = 2,
                       Impressions = 2041.65,
                       SpotCount = 300,
                       StationCount = 46
                   },
                   Details = new List<PlanBuyingStationDto>()
                   {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      }
                   }
               });
            _PlanBuyingOwnershipGroupEngine.Setup(s => s.CalculateAggregateOfOwnershipGroup(It.IsAny<PlanBuyingStationResultDto>()))
                .Returns(new PlanBuyingResultOwnershipGroupDto()
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 3912,
                        AvgCpm = 2,
                        Impressions = 2041.65,
                        SpotCount = 300,
                        StationCount = 46
                    },
                    Details = new List<PlanBuyingResultOwnershipGroupDetailsDto>()
                    {
                      new PlanBuyingResultOwnershipGroupDetailsDto()
                      {
                        Budget = 14201,
                        Cpm = 11,
                        Impressions = 13328,
                        ImpressionsPercentage = 100,
                        MarketCount = 2,
                        SpotCount = 8,
                        StationCount = 2,
                        OwnershipGroupName = "TEGNA"
                      }
                    }
                }).Callback<PlanBuyingStationResultDto>(element =>
                {
                    stationResultCount = element.Details.Count;
                });
            _PlanBuyingOwnershipGroupEngine.Setup(s => s.ConvertImpressionsToUserFormat(It.IsAny<PlanBuyingResultOwnershipGroupDto>()))
                .Callback<PlanBuyingResultOwnershipGroupDto>(element =>
                {
                    element.Totals.Impressions /= 1000;
                    foreach (var detail in element.Details)
                    {
                        detail.Impressions /= 1000;
                    }
                });
            var service = _GetService();
            var Id = 1197;
            var planBuyingFilter = new PlanBuyingFilterDto()
            {
                OwnerNames = new List<string> { "TEGNA", "Newsweb Corporation/Channel 3 TV Company LLC" },
                RepFirmNames = new List<string> { "TEGNA", "Direct" }
            };

            // Act
            var result = service.GetBuyingOwnershipGroups(Id, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, planBuyingFilter);

            // Assert
            Assert.AreEqual(expectedCount, stationResultCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBuyingRepFirmsWithFilter()
        {
            // Arrange
            int stationResultCount = 0;
            int expectedCount = 2;
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingStationResultDto
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 3912,
                        AvgCpm = 2,
                        Impressions = 2041.65,
                        SpotCount = 300,
                        StationCount = 46
                    },
                    Details = new List<PlanBuyingStationDto>()
                    {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 8765,
                        Cpm = 9,
                        Impressions = 9876,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "Cox",
                        OwnerName = null,
                        LegacyCallLetters = "HMTV"
                      }
                    }
                });
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
               .Returns(new PlanBuyingStationResultDto()
               {
                   BuyingJobId = 24,
                   PlanVersionId = 95,
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   PostingType = PostingTypeEnum.NSI,
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       Budget = 3912,
                       AvgCpm = 2,
                       Impressions = 2041.65,
                       SpotCount = 300,
                       StationCount = 46
                   },
                   Details = new List<PlanBuyingStationDto>()
                   {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      }
                   }
               });
            _PlanBuyingRepFirmEngine.Setup(s => s.CalculateAggregateOfRepFirm(It.IsAny<PlanBuyingStationResultDto>()))
                .Returns(new PlanBuyingResultRepFirmDto()
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 3912,
                        AvgCpm = 2,
                        Impressions = 2041.65,
                        SpotCount = 300,
                        StationCount = 46
                    },
                    Details = new List<PlanBuyingResultRepFirmDetailsDto>()
                    {
                        new PlanBuyingResultRepFirmDetailsDto()
                        {
                            Budget = 14201,
                            Cpm = 11,
                            Impressions = 13328,
                            ImpressionsPercentage = 100,
                            MarketCount = 2,
                            SpotCount = 8,
                            StationCount = 2,
                            RepFirmName = "TEGNA"
                        }
                    }
                }).Callback<PlanBuyingStationResultDto>(element =>
                {
                    stationResultCount = element.Details.Count;
                });
            _PlanBuyingRepFirmEngine.Setup(s => s.ConvertImpressionsToUserFormat(It.IsAny<PlanBuyingResultRepFirmDto>()))
                .Callback<PlanBuyingResultRepFirmDto>(element =>
                {
                    element.Totals.Impressions /= 1000;
                    foreach (var detail in element.Details)
                    {
                        detail.Impressions /= 1000;
                    }
                });
            var service = _GetService();
            var Id = 1197;
            var planBuyingFilter = new PlanBuyingFilterDto()
            {
                OwnerNames = new List<string> { "TEGNA", "Newsweb Corporation/Channel 3 TV Company LLC" },
                RepFirmNames = new List<string> { "TEGNA", "Direct" }
            };

            // Act
            var result = service.GetBuyingRepFirms(Id, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, planBuyingFilter);

            // Assert
            Assert.AreEqual(expectedCount, stationResultCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStationsWithoutFilter()
        {
            // Arrange
            int stationResultCount = 0;
            int expectedCount = 3;
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingStationResultDto
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 3912,
                        AvgCpm = 2,
                        Impressions = 2041.65,
                        SpotCount = 300,
                        StationCount = 46
                    },
                    Details = new List<PlanBuyingStationDto>()
                    {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 8765,
                        Cpm = 9,
                        Impressions = 9876,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "Cox",
                        OwnerName = "HMTV",
                        LegacyCallLetters = "HMTV"
                      }
                    }
                });
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
               .Returns(new PlanBuyingStationResultDto()
               {
                   BuyingJobId = 24,
                   PlanVersionId = 95,
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   PostingType = PostingTypeEnum.NSI,
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       Budget = 3912,
                       AvgCpm = 2,
                       Impressions = 2041.65,
                       SpotCount = 300,
                       StationCount = 46
                   },
                   Details = new List<PlanBuyingStationDto>()
                   {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 8765,
                        Cpm = 9,
                        Impressions = 9876,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "Cox",
                        OwnerName = "HMTV",
                        LegacyCallLetters = "HMTV"
                      }
                   }
               }).Callback<PlanBuyingStationResultDto>(element =>
               {
                   stationResultCount = element.Details.Count;
               });
            _PlanBuyingStationEngineMock.Setup(s => s.ConvertImpressionsToUserFormat(It.IsAny<PlanBuyingStationResultDto>()))
                .Callback<PlanBuyingStationResultDto>(element =>
                {
                    element.Totals.Impressions /= 1000;
                    foreach (var detail in element.Details)
                    {
                        detail.Impressions /= 1000;
                    }
                });
            var service = _GetService();
            var Id = 1197;
            var planBuyingFilter = new PlanBuyingFilterDto()
            { };

            // Act
            var result = service.GetStations(Id, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, planBuyingFilter);

            // Assert
            Assert.AreEqual(expectedCount, stationResultCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStationsWithFilterRepFirm()
        {
            // Arrange
            int stationResultCount = 0;
            int expectedCount = 2;
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingStationResultDto
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 3912,
                        AvgCpm = 2,
                        Impressions = 2041.65,
                        SpotCount = 300,
                        StationCount = 46
                    },
                    Details = new List<PlanBuyingStationDto>()
                    {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 8765,
                        Cpm = 9,
                        Impressions = 9876,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "Cox",
                        OwnerName = null,
                        LegacyCallLetters = "HMTV"
                      }
                    }
                });
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
               .Returns(new PlanBuyingStationResultDto()
               {
                   BuyingJobId = 24,
                   PlanVersionId = 95,
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   PostingType = PostingTypeEnum.NSI,
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       Budget = 3912,
                       AvgCpm = 2,
                       Impressions = 2041.65,
                       SpotCount = 300,
                       StationCount = 46
                   },
                   Details = new List<PlanBuyingStationDto>()
                   {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      }
                   }
               }).Callback<PlanBuyingStationResultDto>(element =>
               {
                   stationResultCount = element.Details.Count;
               });
            _PlanBuyingStationEngineMock.Setup(s => s.ConvertImpressionsToUserFormat(It.IsAny<PlanBuyingStationResultDto>()))
                .Callback<PlanBuyingStationResultDto>(element =>
                {
                    element.Totals.Impressions /= 1000;
                    foreach (var detail in element.Details)
                    {
                        detail.Impressions /= 1000;
                    }
                });
            var service = _GetService();
            var Id = 1197;
            var planBuyingFilter = new PlanBuyingFilterDto()
            {
                RepFirmNames = new List<string> { "TEGNA", "Direct" }
            };

            // Act
            var result = service.GetStations(Id, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, planBuyingFilter);

            // Assert
            Assert.AreEqual(expectedCount, stationResultCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStationsWithFilterOwnerNames()
        {
            // Arrange
            int stationResultCount = 0;
            int expectedCount = 2;
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingStationResultDto
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 3912,
                        AvgCpm = 2,
                        Impressions = 2041.65,
                        SpotCount = 300,
                        StationCount = 46
                    },
                    Details = new List<PlanBuyingStationDto>()
                    {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 8765,
                        Cpm = 9,
                        Impressions = 9876,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "Cox",
                        OwnerName = null,
                        LegacyCallLetters = "HMTV"
                      }
                    }
                });
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
               .Returns(new PlanBuyingStationResultDto()
               {
                   BuyingJobId = 24,
                   PlanVersionId = 95,
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   PostingType = PostingTypeEnum.NSI,
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       Budget = 3912,
                       AvgCpm = 2,
                       Impressions = 2041.65,
                       SpotCount = 300,
                       StationCount = 46
                   },
                   Details = new List<PlanBuyingStationDto>()
                   {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      }
                   }
               }).Callback<PlanBuyingStationResultDto>(element =>
               {
                   stationResultCount = element.Details.Count;
               });
            _PlanBuyingStationEngineMock.Setup(s => s.ConvertImpressionsToUserFormat(It.IsAny<PlanBuyingStationResultDto>()))
                .Callback<PlanBuyingStationResultDto>(element =>
                {
                    element.Totals.Impressions /= 1000;
                    foreach (var detail in element.Details)
                    {
                        detail.Impressions /= 1000;
                    }
                });
            var service = _GetService();
            var Id = 1197;
            var planBuyingFilter = new PlanBuyingFilterDto()
            {
                OwnerNames = new List<string> { "TEGNA", "Newsweb Corporation/Channel 3 TV Company LLC" }
            };

            // Act
            var result = service.GetStations(Id, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, planBuyingFilter);

            // Assert
            Assert.AreEqual(expectedCount, stationResultCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMarketsWithFilter()
        {
            // Arrange
            int stationResultCount = 0;
            int expectedCount = 2;
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingStationsResultByJobId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingStationResultDto
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 3912,
                        AvgCpm = 2,
                        Impressions = 2041.65,
                        SpotCount = 300,
                        StationCount = 46
                    },
                    Details = new List<PlanBuyingStationDto>()
                    {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 8765,
                        Cpm = 9,
                        Impressions = 9876,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "Cox",
                        OwnerName = null,
                        LegacyCallLetters = "HMTV"
                      }
                    }
                });
            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(MarketsTestData.GetLatestMarketCoverages());
            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());
            _PlanBuyingStationEngineMock.Setup(s => s.CalculateAggregateOfStations(It.IsAny<PlanBuyingStationResultDto>()))
               .Returns(new PlanBuyingStationResultDto()
               {
                   BuyingJobId = 24,
                   PlanVersionId = 95,
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   PostingType = PostingTypeEnum.NSI,
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       Budget = 3912,
                       AvgCpm = 2,
                       Impressions = 2041.65,
                       SpotCount = 300,
                       StationCount = 46
                   },
                   Details = new List<PlanBuyingStationDto>()
                   {
                      new PlanBuyingStationDto()
                      {
                        Budget = 3912,
                        Cpm = 2,
                        Impressions = 2045,
                        ImpressionsPercentage = 0,
                        Market = "Madison",
                        Spots = 2,
                        Station = "HMTV",
                        Affiliate = "CW",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      },
                      new PlanBuyingStationDto()
                      {
                        Budget = 5436,
                        Cpm = 2,
                        Impressions = 3452,
                        ImpressionsPercentage = 0,
                        Market = "Tulsa",
                        Spots = 6,
                        Station = "HMTV",
                        Affiliate = "KMYT",
                        RepFirm = "TEGNA",
                        OwnerName = "TEGNA",
                        LegacyCallLetters = "HMTV"
                      }
                   }
               });
            _PlanBuyingMarketResultsEngine.Setup(s => s.CalculateAggregatedResultOfMarket(It.IsAny<PlanBuyingStationResultDto>(), It.IsAny<List<MarketCoverage>>(), It.IsAny<PlanDto>()))
                .Returns(new PlanBuyingResultMarketsDto()
                {
                    BuyingJobId = 24,
                    PlanVersionId = 95,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        Budget = 14201,
                        AvgCpm = 11,
                        Impressions = 13328,
                        SpotCount = 8,
                        StationCount = 2,
                        MarketCoveragePercent = 3.986,
                        ImpressionsPercentage = 100,
                        MarketCount = 1
                    },
                    Details = new List<PlanBuyingResultMarketDetailsDto>()
                    {
                      new PlanBuyingResultMarketDetailsDto()
                      {
                        Budget = 14201,
                        Cpm = 11,
                        Impressions = 13328,
                        ImpressionsPercentage = 100,
                        MarketName = "Milwaukee",
                        SpotCount = 8,
                        StationCount = 2,
                        MarketCoveragePercent = 3.986
                      }
                    }
                }).Callback<PlanBuyingStationResultDto,List<MarketCoverage>,PlanDto>((planBuyingStationResult,marketCoverage,plan) =>
                {
                    stationResultCount = planBuyingStationResult.Details.Count;
                });
            _PlanBuyingMarketResultsEngine.Setup(s => s.ConvertImpressionsToUserFormat(It.IsAny<PlanBuyingResultMarketsDto>()))
                .Callback<PlanBuyingResultMarketsDto>(element =>
                {
                    element.Totals.Impressions /= 1000;
                    foreach (var detail in element.Details)
                    {
                        detail.Impressions /= 1000;
                    }
                });
            var service = _GetService();
            var planId = 1197;
            var planBuyingFilter = new PlanBuyingFilterDto()
            {
                OwnerNames = new List<string> { "TEGNA", "Newsweb Corporation/Channel 3 TV Company LLC" },
                RepFirmNames = new List<string> { "TEGNA", "Direct" }
            };

            // Act
            var result = service.GetMarkets(planId, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, planBuyingFilter);

            // Assert
            Assert.AreEqual(expectedCount, stationResultCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async void SavesBuyingAggregateResults_WhenRunningBuyingJob_SaveBandStations()
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
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanBuyingRepositoryMock
               .Setup(x => x.GetPlanBuyingJob(It.IsAny<int>()))
               .Returns(new PlanBuyingJob
               {
                   Id = jobId
               });
            _PlanRepositoryMock
               .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
               .Returns(new PlanDto
               {
                   VersionId = 77,
                   CoverageGoalPercent = 80,
                   PostingType = PostingTypeEnum.NSI,
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
                   FlightStartDate = new DateTime(2005, 11, 21),
                   FlightEndDate = new DateTime(2005, 12, 18),
                   FlightDays = new List<int>(),
                   TargetRatingPoints = 1000,
                   Dayparts = new List<PlanDaypartDto>
                   {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 15,
                            WeightingGoalPercent = 60,
                            DaypartTypeId = DaypartTypeEnum.News
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 16,
                            WeightingGoalPercent = 40,
                            DaypartTypeId = DaypartTypeEnum.News
                        }
                   },
                   BuyingParameters = _GetPlanBuyingParametersDto(),
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
                   },
                   ImpressionsPerUnit = 1
               });

            _WeeklyBreakdownEngineMock
               .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
               .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _PlanBuyingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanBuyingJobDiagnostic>()))
                .Returns(new List<PlanBuyingInventoryProgram>
                {
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 1,
                        StandardDaypartId = 15,
                        PostingType = PostingTypeEnum.NSI,
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
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 2,
                        StandardDaypartId = 16,
                        PostingType = PostingTypeEnum.NSI,
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
                                InventoryMediaWeekId = 90
                            }
                        }
                    },
                    new PlanBuyingInventoryProgram
                    {
                        ManifestId = 3,
                        StandardDaypartId = 15,
                        PostingType = PostingTypeEnum.NSI,
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
                        PostingType = PostingTypeEnum.NSI,
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
                });

            _MarketCoverageRepositoryMock
              .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
              .Returns(_GetLatestMarketCoverages());

            _BuyingApiClientMock
             .Setup(x => x.GetBuyingSpotsResultAsync(It.IsAny<PlanBuyingApiRequestDto_v3>()))
             .Returns(Task.FromResult(new PlanBuyingApiSpotsResponseDto_v3
             {
                 RequestId = "#q1w2e3",
                 Results = new List<PlanBuyingApiSpotsResultDto_v3>
                 {
                        new PlanBuyingApiSpotsResultDto_v3
                        {
                            ManifestId = 1,
                            MediaWeekId = 100,
                            Frequencies = new List<SpotFrequencyResponse>
                            {
                                new SpotFrequencyResponse
                                {
                                    SpotLengthId = 1,
                                    Frequency = 1
                                }
                            }
                        },
                        new PlanBuyingApiSpotsResultDto_v3
                        {
                            ManifestId = 1,
                            MediaWeekId = 101,
                            Frequencies = new List<SpotFrequencyResponse>
                            {
                                new SpotFrequencyResponse
                                {
                                    SpotLengthId = 1,
                                    Frequency = 2
                                }
                            }
                        },
                        new PlanBuyingApiSpotsResultDto_v3
                        {
                            ManifestId = 2,
                            MediaWeekId = 100,
                            Frequencies = new List<SpotFrequencyResponse>
                            {
                                new SpotFrequencyResponse
                                {
                                    SpotLengthId = 1,
                                    Frequency = 3
                                }
                            }
                        }
                 }
             }));

            _MediaMonthAndWeekAggregateCacheMock
              .Setup(x => x.GetMediaWeekById(It.IsAny<int>()))
              .Returns<int>(weekId => new MediaWeek
              {
                  Id = weekId
              });

            _PlanBuyingProgramEngine.Setup(s => s.Calculate(
                   It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                   It.IsAny<bool>()))
               .Returns(new PlanBuyingResultBaseDto());

            _PlanBuyingProgramEngine.Setup(s => s.CalculateProgramStations(
                   It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                   It.IsAny<bool>()))
               .Returns(new PlanBuyingResultBaseDto());

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.Calculate(
                    It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                    It.IsAny<PlanBuyingParametersDto>()))
                .Returns(new PlanBuyingBandsDto());

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.CalculateBandInventoryStation(
                    It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>()))
                .Returns(new PlanBuyingBandInventoryStationsDto
                {
                    PlanVersionId = 77,
                    BuyingJobId = jobId,
                    Details = new List<PlanBuyingBandStationDetailDto>()
                    {
                        new PlanBuyingBandStationDetailDto()
                        {
                            StationId = 1376,
                            PostingTypeConversionRate = .7,
                            Impressions = 1135000,
                            Cost = 6593.75m,
                            ManifestWeeksCount = 2,
                            PlanBuyingBandInventoryStationDayparts = new List<PlanBuyingBandInventoryStationDaypartDto>()
                            { 
                                new PlanBuyingBandInventoryStationDaypartDto()
                                { 
                                    ActiveDays = 5,
                                    Hours = 1
                                },
                                new PlanBuyingBandInventoryStationDaypartDto()
                                {
                                    ActiveDays = 3,
                                    Hours = 2
                                }
                            }
                        },
                        new PlanBuyingBandStationDetailDto()
                        {
                            StationId = 1377,
                            PostingTypeConversionRate = .7,
                            Impressions = 2045420,
                            Cost = 16250.00m,
                            ManifestWeeksCount = 1,
                            PlanBuyingBandInventoryStationDayparts = new List<PlanBuyingBandInventoryStationDaypartDto>()
                            {
                                new PlanBuyingBandInventoryStationDaypartDto()
                                {
                                    ActiveDays = 1,
                                    Hours = 4
                                }
                            }
                        }
                    }
                });

            _PlanBuyingStationEngineMock.Setup(s =>
                    s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(), It.IsAny<PlanBuyingAllocationResult>(),
                        It.IsAny<PlanBuyingParametersDto>()))
                .Returns(new PlanBuyingStationResultDto());

            _PlanBuyingMarketResultsEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>(),
                    It.IsAny<PlanDto>(), It.IsAny<List<MarketCoverage>>()))
                .Returns(new PlanBuyingResultMarketsDto());

            _PlanBuyingOwnershipGroupEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>()
                   ))
                .Returns(new PlanBuyingResultOwnershipGroupDto());

            _PlanBuyingRepFirmEngine.Setup(s => s.Calculate(It.IsAny<List<PlanBuyingInventoryProgram>>(),
                    It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>()
                   ))
                .Returns(new PlanBuyingResultRepFirmDto());

            var passedParameters = new List<PlanBuyingBandInventoryStationsDto>();
            _PlanBuyingRepositoryMock
                .Setup(x => x.SavePlanBuyingBandInventoryStations(It.IsAny<PlanBuyingBandInventoryStationsDto>()))
                .Callback<PlanBuyingBandInventoryStationsDto>(p => passedParameters.Add(p));

            var service = _GetService();
            // Act
            await service.RunBuyingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms_AggregatedProgramStations()
        {
            int count = 0;
            int expectedCount = 1;
            // Arrange
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime()
                });
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingProgramsResultByJobId_V2(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanBuyingResultProgramsDto
                {
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI,
                    Totals = new PlanBuyingProgramTotalsDto()
                    {
                        MarketCount = 825,
                        StationCount = 825,
                        AvgCpm = 11.6318M,
                        AvgImpressions = 1025.9014,
                        Budget = 223912.9087M,
                        SpotCount = 18764,
                        Impressions = 19250014.450819
                    },
                    Details = new List<PlanBuyingProgramProgramDto>()
                    {
                      new PlanBuyingProgramProgramDto()
                      {
                        ProgramName = "12 News",
                        Genre = "News",
                        RepFirm = "WJTV",
                        OwnerName = "WJTV",
                        LegacyCallLetters = "WJTV",
                        MarketCode = 318,
                        Station = "WJTV",
                        Impressions = 18868.80333923681,
                        ImpressionsPercentage = 0.09801592,
                        Budget = 304.6875M,
                        Spots = 25,
                        StationCount = 1,
                        MarketCount= 1
                      },
                      new PlanBuyingProgramProgramDto()
                      {
                        ProgramName = "14 NEWS SUNRISE",
                        Genre = "News",
                        RepFirm = "KHBS",
                        OwnerName = "KHBS",
                        LegacyCallLetters = "KHBS",
                        MarketCode = 318,
                        Station = "KHBS",
                        Impressions = 18868.80333923681,
                        ImpressionsPercentage = 0.09801592,
                        Budget = 304.6875M,
                        Spots = 25,
                        StationCount = 1,
                        MarketCount= 1
                      },
                      new PlanBuyingProgramProgramDto()
                      {
                        ProgramName = "14 NEWS SUNRISE",
                        Genre = "News",
                        RepFirm = "WPBN",
                        OwnerName = "WPBN",
                        LegacyCallLetters = "WPBN",
                        MarketCode = 318,
                        Station = "WPBN",
                        Impressions = 18868.80333923681,
                        ImpressionsPercentage = 0.09801592,
                        Budget = 304.6875M,
                        Spots = 25,
                        StationCount = 1,
                        MarketCount= 1
                      }
                    }
                });
            _PlanBuyingProgramEngine.Setup(s => s.GetAggregatedProgramStations(It.IsAny<PlanBuyingResultProgramsDto>()))
               .Returns(new PlanBuyingResultProgramsDto()
               {
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   PostingType = PostingTypeEnum.NSI,
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       MarketCount = 825,
                       StationCount = 825,
                       AvgCpm = 11.6318M,
                       AvgImpressions = 1025.9014,
                       Budget = 223912.9087M,
                       SpotCount = 18764,
                       Impressions = 19250014.450819
                   },
                   Details = new List<PlanBuyingProgramProgramDto>()
                    {                      
                      new PlanBuyingProgramProgramDto()
                      {
                        ProgramName = "14 NEWS SUNRISE",
                        Genre = "News",
                        RepFirm = "WPBN",
                        OwnerName = "WPBN",
                        LegacyCallLetters = "WPBN",
                        MarketCode = 318,
                        Station = "WPBN",
                        Impressions = 18868.80333923681,
                        ImpressionsPercentage = 0.09801592,
                        Budget = 304.6875M,
                        Spots = 25,
                        StationCount = 1,
                        MarketCount= 1
                      }
                   }
               }).Callback<PlanBuyingResultProgramsDto>(element =>
               {
                   count = element.Details.Count;
               });
            _PlanBuyingProgramEngine.Setup(s => s.ConvertImpressionsToUserFormat(It.IsAny<PlanBuyingResultProgramsDto>()))
                .Callback<PlanBuyingResultProgramsDto>(element =>
                {
                    element.Totals.Impressions /= 1000;
                    foreach (var detail in element.Details)
                    {
                        detail.Impressions /= 1000;
                    }
                }); ;
          
            var service = _GetService();
            var planId = 1197;
            var planBuyingFilter = new PlanBuyingFilterDto()
            {
                OwnerNames = new List<string> { "WPBN", "Newsweb Corporation/Channel 3 TV Company LLC" },
                RepFirmNames = new List<string> { "WPBN", "Direct" }
            };

            // Act           
            var result = service.GetPrograms(planId, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, planBuyingFilter);
            // Assert
            Assert.AreEqual(expectedCount, count);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBuyingBands_Verify_PlanBuyingFilter()
        {
            int filteredStationCount = 0, filteredAllocatedSpotCount = 0, expectedCount = 3;
            var plan = _GetPlan();

            // Arrange
            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob
                {
                    Id = 1,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = _CurrentDate
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetLatestParametersForPlanBuyingJob(It.IsAny<int>()))
                .Returns(_GetPlanBuyingParametersDto());

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetBuyingApiResultsByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>(), It.IsAny<PostingTypeEnum>()))
                .Returns(new PlanBuyingAllocationResult
                {
                    BuyingCpm = 4.6841m,
                    JobId = 1,
                    BuyingVersion = "4",
                    PlanVersionId = 1172,
                    AllocatedSpots = new List<PlanBuyingAllocatedSpot>
                    {
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 4340, 
                            SpotFrequencies = new List<SpotFrequency>
                            {
                                new SpotFrequency {SpotLengthId = 1, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 3, Impressions = 659.63},
                                new SpotFrequency {SpotLengthId = 2, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 1, Impressions = 659.63}
                            },
                            RepFirm = "WPBN",
                            OwnerName = null,
                            LegacyCallLetters = "WPBN"
                        },
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 4341,
                            SpotFrequencies = new List<SpotFrequency>
                            {
                                new SpotFrequency {SpotLengthId = 1, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 3, Impressions = 659.63},
                            },
                            RepFirm = null,
                            OwnerName = null,
                            LegacyCallLetters = "WPBN"
                        },
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 4342,
                            SpotFrequencies = new List<SpotFrequency>
                            {
                                new SpotFrequency {SpotLengthId = 1, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 3, Impressions = 659.63},
                            },
                            RepFirm = null,
                            OwnerName = "WPBN",
                            LegacyCallLetters = "WPBN"
                        },
                        new PlanBuyingAllocatedSpot
                        {
                            Id = 4343,
                            SpotFrequencies = new List<SpotFrequency>
                            {
                                new SpotFrequency {SpotLengthId = 1, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 3, Impressions = 659.63},
                            },
                            RepFirm = "KHBS",
                            OwnerName = "KHBS",
                            LegacyCallLetters = "KHBS"
                        }
                    },
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    PostingType = PostingTypeEnum.NSI
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetPlanBuyingBandInventoryStations(It.IsAny<int>()))
                .Returns(new PlanBuyingBandInventoryStationsDto
                {
                    Details = new List<PlanBuyingBandStationDetailDto>
                    {
                        new PlanBuyingBandStationDetailDto
                        {
                            StationId = 162,
                            RepFirm = "WPBN",
                            OwnerName = null,
                            LegacyCallLetters = "WPBN"
                        },
                        new PlanBuyingBandStationDetailDto
                        {
                            StationId = 164,
                            RepFirm = null,
                            OwnerName = null,
                            LegacyCallLetters = "WPBN"
                        },
                        new PlanBuyingBandStationDetailDto
                        {
                            StationId = 166,
                            RepFirm = null,
                            OwnerName = "WPBN",
                            LegacyCallLetters = "WPBN"
                        },
                        new PlanBuyingBandStationDetailDto
                        {
                            StationId = 168,
                            RepFirm = "KHBS",
                            OwnerName = "KHBS",
                            LegacyCallLetters = "KHBS"
                        }
                    }
                });

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.Calculate(It.IsAny<PlanBuyingBandInventoryStationsDto>(), It.IsAny<PlanBuyingAllocationResult>(), It.IsAny<PlanBuyingParametersDto>()))
               .Returns(new PlanBuyingBandsDto()
               {
                   PlanVersionId = 1172,
                   BuyingJobId = 1,
                   PostingType = PostingTypeEnum.NSI,
                   SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                   Details = new List<PlanBuyingBandDetailDto>
                   {
                       new PlanBuyingBandDetailDto
                       {
                           MinBand = null,
                           MaxBand = 0.36891m,
                           Spots = 0,
                           Impressions = 0,
                           Budget = 0,
                           Cpm = 0,
                           ImpressionsPercentage = 0,
                           AvailableInventoryPercent = 0
                       },
                       new PlanBuyingBandDetailDto
                       {
                           MinBand = 0.36891m,
                           MaxBand = 1.370237142857143m,
                           Spots = 0,
                           Impressions = 0,
                           Budget = 0,
                           Cpm = 0,
                           ImpressionsPercentage = 0,
                           AvailableInventoryPercent = 0
                       },
                       new PlanBuyingBandDetailDto
                       {
                            MinBand = 1.370237142857143m,
                            MaxBand = 2.371564285714286m,
                            Spots = 156,
                            Impressions = 496.6,
                            Budget = 975,
                            Cpm = 1.963350785340314m,
                            ImpressionsPercentage = 9.696889223877857,
                            AvailableInventoryPercent = 869.3977591036414
                       },
                       new PlanBuyingBandDetailDto
                       {
                           MinBand = 2.371564285714286m,
                           MaxBand = 3.3728914285714287m,
                           Spots = 369,
                           Impressions = 2666.8,
                           Budget = 8331.25m,
                           Cpm = 3.1240625468726564m,
                           ImpressionsPercentage = 52.073427672648954,
                           AvailableInventoryPercent = 5426.943426943428
                       },
                       new PlanBuyingBandDetailDto
                       {
                           MinBand = 3.3728914285714287m,
                           MaxBand = 4.374218571428571m,
                           Spots = 18,
                           Impressions = 600.1,
                           Budget = 2281.25m,
                           Cpm = 3.801449758373604m,
                           ImpressionsPercentage = 11.717888085479466,
                           AvailableInventoryPercent = 5.12069165602878
                       },
                       new PlanBuyingBandDetailDto
                       {
                           MinBand = 4.374218571428571m,
                           MaxBand = 5.375545714285714m,
                           Spots = 9,
                           Impressions = 900,
                           Budget = 4187.5m,
                           Cpm = 4.652777777777778m,
                           ImpressionsPercentage = 17.573903144361804,
                           AvailableInventoryPercent = 3.497190978490721
                       },
                       new PlanBuyingBandDetailDto
                       {
                           MinBand = 5.375545714285714m,
                           MaxBand = 6.376872857142857m,
                           Spots = 0,
                           Impressions = 0,
                           Budget = 0,
                           Cpm = 0,
                           ImpressionsPercentage = 0,
                           AvailableInventoryPercent = 0
                       },
                       new PlanBuyingBandDetailDto
                       {
                           MinBand = 6.376872857142857m,
                           MaxBand = 7.3782m,
                           Spots = 0,
                           Impressions = 0,
                           Budget = 0,
                           Cpm = 0,
                           ImpressionsPercentage = 0,
                           AvailableInventoryPercent = 0
                       },
                       new PlanBuyingBandDetailDto
                       {
                           MinBand = 7.3782m,
                           MaxBand = null,
                           Spots = 52,
                           Impressions = 457.73,
                           Budget = 5118.75m,
                           Cpm = 11.182902584493041m,
                           ImpressionsPercentage = 8.93789187363192,
                           AvailableInventoryPercent = 0.26264892141627816
                       }
                   },
                   Totals = new PlanBuyingProgramTotalsDto()
                   {
                       AvgCpm = 4.079830431361216M,
                       Budget = 20893.75M,
                       SpotCount = 604,
                       Impressions = 5121.23
                   }
               }).Callback<PlanBuyingBandInventoryStationsDto, PlanBuyingAllocationResult, PlanBuyingParametersDto>((planBuyingBandInventoryStation, planBuyingAllocationResult, planBuyingParameters) =>
               {
                   filteredStationCount = planBuyingBandInventoryStation.Details.Count;
                   filteredAllocatedSpotCount = planBuyingAllocationResult.AllocatedSpots.Count;
               });

            _PlanBuyingBandCalculationEngineMock.Setup(s => s.ConvertImpressionsToUserFormat(It.IsAny<PlanBuyingBandsDto>()))
                .Callback<PlanBuyingBandsDto>(element =>
                {
                    element.Totals.Impressions /= 1000;
                    foreach (var detail in element.Details)
                    {
                        detail.Impressions /= 1000;
                    }
                }); ;

            var service = _GetService();
            var planId = 1197;
            var planBuyingFilter = new PlanBuyingFilterDto()
            {
                OwnerNames = new List<string> { "WPBN", "Newsweb Corporation/Channel 3 TV Company LLC" },
                RepFirmNames = new List<string> { "WPBN", "Direct" }
            };

            // Act           
            var result = service.GetBuyingBands(planId, PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, planBuyingFilter);

            // Assert
            Assert.AreEqual(expectedCount, filteredStationCount);
            Assert.AreEqual(expectedCount, filteredAllocatedSpotCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async Task RunBuyingJobAsync_Verify_FilteredDaypart()
        {
            // Arrange
            const int jobId = 1;
            var parameters = _GetPlanBuyingParametersDto();
            var plan = _GetPlan();
            plan.Dayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.News,
                    DaypartCodeId = 2,
                    StartTimeSeconds = 0,
                    EndTimeSeconds = 2000,
                    WeightingGoalPercent = 28.0,
                    VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                    {
                        new PlanDaypartVpvhForAudienceDto
                        {
                            AudienceId = 31,
                            Vpvh = 0.5,
                            VpvhType = VpvhTypeEnum.FourBookAverage,
                            StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                        }
                    }
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.Sports,
                    DaypartCodeId = 24,
                    StartTimeSeconds = 1500,
                    EndTimeSeconds = 2788,
                    WeightingGoalPercent = 33.2,
                    VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                    {
                        new PlanDaypartVpvhForAudienceDto
                        {
                            AudienceId = 31,
                            Vpvh = 0.5,
                            VpvhType = VpvhTypeEnum.FourBookAverage,
                            StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                        }
                    }
                }
            };
            int expectedResult = 1;

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetPlanBuyingJob(It.IsAny<int>()))
                .Returns(new PlanBuyingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            var service = _GetService(false, false);

            // Act
            await service.RunBuyingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Assert.AreEqual(expectedResult, plan.Dayparts.Count);
        }

        [Test]
        public void ProgramLineupReport_WithToggleOn()
        {
            // Arrange
            bool isProgramLineupAllocationByAffiliateEnabled = true;
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            Guid advertiserId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 8, Weight = 50 } },
                    Equivalized = false,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    BuyingParameters = new PlanBuyingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanBuyingRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForBuyingProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _GetService();
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_PROGRAM_LINEUP_ALLOCATION_BY_AFFILIATE] = isProgramLineupAllocationByAffiliateEnabled;
            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetLatestBuyingJob(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanBuyingRepositoryMock.Verify(x => x.GetPlanBuyingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum?>(), It.IsAny<SpotAllocationModelMode?>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

    }

}