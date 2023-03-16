using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Hangfire;
using Hangfire.States;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.PlanPricing;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.QuoteReport;
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
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Plan.CommonPricingEntities.BasePlanInventoryProgram;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram.ManifestDaypart;
using Job = Hangfire.Common.Job;

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
        private Mock<IMarketCoverageRepository> _MarketCoverageRepositoryMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private Mock<IStationProgramRepository> _StationProgramRepositoryMock;
        private Mock<IMarketRepository> _MarketRepositoryMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IStandardDaypartRepository> _StandardDaypartRepositoryMock;
        private Mock<ICampaignRepository> _CampaignRepositoryMock;
        private Mock<IWeeklyBreakdownEngine> _WeeklyBreakdownEngineMock;
        private Mock<IPlanPricingBandCalculationEngine> _PlanPricingBandCalculationEngineMock;
        private Mock<IPlanPricingStationCalculationEngine> _PlanPricingStationCalculationEngineMock;
        private Mock<IPlanPricingMarketResultsEngine> _PlanPricingMarketResultsEngine;
        private Mock<IPlanPricingProgramCalculationEngine> _PlanPricingProgramCalculationEngine;
        private Mock<IPricingRequestLogClient> _PricingRequestLogClient;
        private Mock<IPlanPricingScxDataPrep> _PlanPricingScxDataPrepMock;
        private Mock<IPlanPricingScxDataConverter> _PlanPricingScxDataConverterMock;
        private Mock<IPlanValidator> _PlanValidatorMock;
        private Mock<ISharedFolderService> _SharedFolderServiceMock;
        private Mock<IAudienceService> _AudienceServiceMock;
        private Mock<ICreativeLengthEngine> _CreativeLengthEngineMock;
        private Mock<IInventoryProprietarySummaryRepository> _InventoryProprietarySummaryRepositoryMock;
        private Mock<IBroadcastAudienceRepository> _BroadcastAudienceRepositoryMock;
        private AsyncTaskHelperStub _AsyncTaskHelperStub;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        protected PlanPricingService _GetService(bool useTrueIndependentStations = false,
                                                               bool isPostingTypeToggleEnabled = false)
        {
            _LaunchDarklyClientStub = new LaunchDarklyClientStub();
            _LaunchDarklyClientStub.FeatureToggles.Add(FeatureToggles.USE_TRUE_INDEPENDENT_STATIONS, useTrueIndependentStations);           
            _LaunchDarklyClientStub.FeatureToggles.Add(FeatureToggles.PRICING_MODEL_BARTER_INVENTORY, false);
            _LaunchDarklyClientStub.FeatureToggles.Add(FeatureToggles.PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY, false);

            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder)).Returns("c:\\TempFolder");

            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);

            return new PlanPricingService(
                _DataRepositoryFactoryMock.Object,
                _SpotLengthEngineMock.Object,
                _PricingApiClientMock.Object,
                _BackgroundJobClientMock.Object,
                _PlanPricingInventoryEngineMock.Object,
                _BroadcastLockingManagerApplicationServiceMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _DateTimeEngineMock.Object,
                _WeeklyBreakdownEngineMock.Object,
                _PlanPricingBandCalculationEngineMock.Object,
                _PlanPricingStationCalculationEngineMock.Object,
                _PlanPricingMarketResultsEngine.Object,
                _PlanPricingProgramCalculationEngine.Object,
                _PricingRequestLogClient.Object,
                _PlanPricingScxDataPrepMock.Object,
                _PlanPricingScxDataConverterMock.Object,
                _PlanValidatorMock.Object,
                _SharedFolderServiceMock.Object,
                _AudienceServiceMock.Object,
                _CreativeLengthEngineMock.Object,
                _AsyncTaskHelperStub,
                featureToggleHelper, _ConfigurationSettingsHelperMock.Object);
        }

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
            _MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _StationProgramRepositoryMock = new Mock<IStationProgramRepository>();
            _MarketRepositoryMock = new Mock<IMarketRepository>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _StandardDaypartRepositoryMock = new Mock<IStandardDaypartRepository>();
            _CampaignRepositoryMock = new Mock<ICampaignRepository>();
            _WeeklyBreakdownEngineMock = new Mock<IWeeklyBreakdownEngine>();
            _PlanPricingBandCalculationEngineMock = new Mock<IPlanPricingBandCalculationEngine>();
            _PlanPricingStationCalculationEngineMock = new Mock<IPlanPricingStationCalculationEngine>();
            _PlanPricingMarketResultsEngine = new Mock<IPlanPricingMarketResultsEngine>();
            _PricingRequestLogClient = new Mock<IPricingRequestLogClient>();
            _PlanPricingScxDataPrepMock = new Mock<IPlanPricingScxDataPrep>();
            _PlanPricingScxDataConverterMock = new Mock<IPlanPricingScxDataConverter>();
            _PlanValidatorMock = new Mock<IPlanValidator>();
            _SharedFolderServiceMock = new Mock<ISharedFolderService>();
            _AudienceServiceMock = new Mock<IAudienceService>();
            _CreativeLengthEngineMock = new Mock<ICreativeLengthEngine>();
            _InventoryProprietarySummaryRepositoryMock = new Mock<IInventoryProprietarySummaryRepository>();
            _BroadcastAudienceRepositoryMock = new Mock<IBroadcastAudienceRepository>();
            _PlanPricingProgramCalculationEngine = new Mock<IPlanPricingProgramCalculationEngine>();
            _AsyncTaskHelperStub = new AsyncTaskHelperStub();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 2, 4, 15, 31, 27));

            _StandardDaypartRepositoryMock
                .Setup(x => x.GetAllStandardDayparts())
                .Returns(_GetStandardDayparts());

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

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryProprietarySummaryRepository>())
                .Returns(_InventoryProprietarySummaryRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IBroadcastAudienceRepository>())
                .Returns(_BroadcastAudienceRepositoryMock.Object);

            _PricingRequestLogClient
                .Setup(x => x.SavePricingRequest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SpotAllocationModelMode>()));
        }

        private List<StandardDaypartDto> _GetStandardDayparts()
        {
            return new List<StandardDaypartDto>
            {
                new StandardDaypartDto { Id = 1, Code = "EMN", FullName = "Early Morning News"},
                new StandardDaypartDto { Id = 2, Code = "MDN", FullName = "Midday News"},
                new StandardDaypartDto { Id = 3, Code = "EN", FullName = "Evening News"},
                new StandardDaypartDto { Id = 4, Code = "LN", FullName = "Late News"},
                new StandardDaypartDto { Id = 5, Code = "ENLN", FullName = "Evening News/Late News"},
                new StandardDaypartDto { Id = 6, Code = "EF", FullName = "Early Fringe"},
                new StandardDaypartDto { Id = 7, Code = "PA", FullName = "Prime Access"},
                new StandardDaypartDto { Id = 8, Code = "PT", FullName = "Prime"},
                new StandardDaypartDto { Id = 9, Code = "LF", FullName = "Late Fringe"},
                new StandardDaypartDto { Id = 10, Code = "SYN", FullName = "Total Day Syndication"},
                new StandardDaypartDto { Id = 11, Code = "OVN", FullName = "Overnights"},
                new StandardDaypartDto { Id = 12, Code = "DAY", FullName = "Daytime"},
                new StandardDaypartDto { Id = 14, Code = "EM", FullName = "Early morning"},
                new StandardDaypartDto { Id = 15, Code = "AMN", FullName = "AM News"},
                new StandardDaypartDto { Id = 16, Code = "PMN", FullName = "PM News"},
                new StandardDaypartDto { Id = 17, Code = "TDN", FullName = "Total Day News"},
                new StandardDaypartDto { Id = 19, Code = "ROSS", FullName = "ROS Syndication"},
                new StandardDaypartDto { Id = 20, Code = "SPORTS", FullName = "ROS Sports"},
                new StandardDaypartDto { Id = 21, Code = "ROSP", FullName = "ROS Programming"},
                new StandardDaypartDto { Id = 22, Code = "TDNS", FullName = "Total Day News and Syndication"},
            };
        }

        [Test]
        public void UsesCurrentPlanVersion_WhenVersionIsNotSpecified_OnPricingResultsReportGeneration()
        {
            // Arrange
            const int planId = 1;

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    VersionNumber = 3,
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanVersionId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
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
            service.GetPricingResultsReportData(planId, SpotAllocationModelMode.Quality);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
        }

        [Test]
        public void UsesSpotAllocationModelModeAsEfficiencyWithPostingTypeAsNTI_OnPricingResultsReportGeneration()
        {
            // Arrange
            const int planId = 1;
            var planVersionNumber = 2;

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    PostingType = PostingTypeEnum.NTI,
                    VersionNumber = planVersionNumber,
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanVersionId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
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
            service.GetPricingResultsReportData(planId, SpotAllocationModelMode.Efficiency);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
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
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanVersionId(It.IsAny<int>(),It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                        Impressions30sec = 1000,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 10,
                                Spots = 1,
                                Impressions = 1000
                            }
                        },
                        StationInventoryManifestId = 1,
                        StandardDaypart = new StandardDaypartDto
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
                        Impressions30sec = 800,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 8,
                                Spots = 2,
                                Impressions = 800
                            }
                        },
                        StationInventoryManifestId = 1,
                        StandardDaypart = new StandardDaypartDto
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
                        Impressions30sec = 500,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 5,
                                Spots = 3,
                                Impressions = 500
                            }
                        },
                        StationInventoryManifestId = 2,
                        StandardDaypart = new StandardDaypartDto
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
                        Impressions30sec = 400,
                        SpotFrequencies = new List<SpotFrequency>
                        {
                            new SpotFrequency
                            {
                                SpotCost = 3,
                                Spots = 4,
                                Impressions = 400
                            }
                        },
                        StationInventoryManifestId = 3,
                        StandardDaypart = new StandardDaypartDto
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
                            MarketCode = 100,
                            Affiliation = "CBS"
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
            var result = service.GetPricingResultsReportData(planId, SpotAllocationModelMode.Quality);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(false, true, false, 11.1957)]
        [TestCase(false, true, true, 13.9946)]
        [TestCase(true, true, false, 21.5957)]
        [TestCase(true, true, true, 24.3351)]
        [TestCase(true, false, true, 500)] // Margin is not used without spots
        [TestCase(true, false, false, 500)]
        [TestCase(false, false, false, 0)]
        public void CalculatePricingCpm(bool hasProprietary, bool hasSpots, bool hasMargin, decimal expectedCpmResult)
        {
            // Arrange
            var margin = hasMargin ? 20 : (double?)null;

            var proprietaryData = hasProprietary ?
                new ProprietaryInventoryData
                {
                    ProprietarySummaries = new List<ProprietarySummary>
                    {
                        new ProprietarySummary
                        {
                            ProprietarySummaryByStations = new List<ProprietarySummaryByStation>
                            {
                                new ProprietarySummaryByStation
                                {
                                    TotalCostWithMargin = 5000,
                                    ProprietarySummaryByAudiences = new List<ProprietarySummaryByAudience>
                                    {
                                        new ProprietarySummaryByAudience
                                        {
                                            TotalImpressions = 10000
                                        }
                                    }
                                }
                            }
                        }
                    }
                } :
                new ProprietaryInventoryData();

            var spots = new List<PlanPricingAllocatedSpot>
            {
                new PlanPricingAllocatedSpot
                {
                    Id = 1,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            SpotCost = 200,
                            Spots = 2,
                            Impressions = 10000
                        }
                    },
                    Impressions30sec = 10000
                },
                new PlanPricingAllocatedSpot
                {
                    Id = 2,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            SpotCost = 300,
                            Spots = 4,
                            Impressions = 50000
                        }
                    },
                    Impressions30sec = 50000
                },
                new PlanPricingAllocatedSpot
                {
                    Id = 3,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            SpotCost = 500,
                            Spots = 3,
                            Impressions = 20000
                        }
                    },
                    Impressions30sec = 20000
                },
                new PlanPricingAllocatedSpot
                {
                    Id = 4,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            SpotCost = 100,
                            Spots = 1,
                            Impressions = 30000
                        }
                    },
                    Impressions30sec = 30000
                },
                new PlanPricingAllocatedSpot
                {
                    Id = 5,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            SpotCost = 300,
                            Spots = 3,
                            Impressions = 10000
                        }
                    },
                    Impressions30sec = 10000
                },
                new PlanPricingAllocatedSpot
                {
                    Id = 6,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            SpotCost = 400,
                            Spots = 2,
                            Impressions = 50000
                        }
                    },
                    Impressions30sec = 50000
                },
                new PlanPricingAllocatedSpot
                {
                    Id = 7,
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            SpotCost = 250,
                            Spots = 1,
                            Impressions = 20000
                        }
                    },
                    Impressions30sec = 20000
                }
            };

            // apply the margin as the engine would, prior to the CPM calculation
            spots.SelectMany(x => x.SpotFrequencies).ForEach(s => s.SpotCostWithMargin = GeneralMath.CalculateCostWithMargin(s.SpotCost, margin));

            var allocationResult = new PlanPricingAllocationResult
            {
                Spots = hasSpots ? spots : new List<PlanPricingAllocatedSpot>()
            };

            // Act
            var service = _GetService();

            var result = service._CalculatePricingCpm(allocationResult.Spots, proprietaryData);
            // round to 4 decimal points for the compare which is all that's needed for the FE usage
            var roundedResult = decimal.Round(result, 4, MidpointRounding.AwayFromZero);

            // Assert
            Assert.AreEqual(expectedCpmResult, roundedResult);
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        public void CanNotQueuePricingJobWhenThereIsOneActive(BackgroundJobProcessingStatus status)
        {
            const string expectedMessage = "The pricing model is already running for the plan";

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(new PlanPricingJob { Status = status });

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetNotUserBasedLockObjectForKey(It.IsAny<string>()))
                .Returns(new object());

            var service = _GetService();

            var exception = Assert.Throws<CadentException>(async () => service.QueuePricingJob(
                new PlanPricingParametersDto() { PlanId = 1 }
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
        public async Task ThrowsException_WhenWrongUnitCapType_IsPassed_WhenRunningPricing()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

            var planParameters = _GetPlanPricingParametersDto();
            planParameters.UnitCapsType = UnitCapEnum.PerMonth;

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
                    PricingParameters = planParameters,
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
                        }
                    },
                    ImpressionsPerUnit = 1
                });

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

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
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
                    }
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

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
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Assert.AreEqual(2, jobUpdates.Count);
            Assert.IsNull(jobUpdates[0].ErrorMessage);
            Assert.IsTrue(jobUpdates[1].ErrorMessage.StartsWith("Error attempting to run the pricing model."));

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "ErrorMessage");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates, settings));
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
                UnitCapsType = UnitCapEnum.Per30Min,
                MarketGroup = MarketGroupEnum.All,
                PostingType = PostingTypeEnum.NSI,
                FluidityPercentage = 40
            };
        }

        private List<PlanPricingInventoryProgram> _GetPlanPricingInventoryPrograms()
        {
            return new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    PostingType = PostingTypeEnum.NSI,
                    Station = new DisplayBroadcastStation
                    {
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
                    PostingType = PostingTypeEnum.NSI,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "wabc",
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
                    PostingType = PostingTypeEnum.NSI,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
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
                    PostingType = PostingTypeEnum.NSI,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kabc",
                        MarketCode = 302,
                    },
                    ProvidedImpressions = 3000,
                    ProjectedImpressions = 2900,
                    ManifestRates = new List<ManifestRate>
                    {
                        new ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 130
                        }
                    },
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
                    PostingType = PostingTypeEnum.NSI,
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
                    PostingType = PostingTypeEnum.NSI,
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
                    PostingType = PostingTypeEnum.NSI,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
                    },
                    ProvidedImpressions = null,
                    ProjectedImpressions = 700,
                    ManifestRates = new List<ManifestRate>
                    {
                        new ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 40
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
                    PostingType = PostingTypeEnum.NSI,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
                    },
                    ProvidedImpressions = null,
                    ProjectedImpressions = 700,
                    ManifestRates = new List<ManifestRate>
                    {
                        new ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 40
                        }
                    },
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
                    PostingType = PostingTypeEnum.NSI,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "kpdx",
                        MarketCode = 100,
                    },
                    ProvidedImpressions = null,
                    ProjectedImpressions = 700,
                    ManifestRates = new List<ManifestRate>
                    {
                        new ManifestRate
                        {
                            SpotLengthId = 1,
                            Cost = 40
                        }
                    },
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
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetPlanPricingInventoryPrograms());

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate => jobUpdates.Add(jobUpdate));

            var service = _GetService();

            // Act
            var task = Task.Run(() => service.RunPricingJobAsync(parameters, jobId, cancellationTokenSource.Token));
            cancellationTokenSource.Cancel();
            task.GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, jobUpdates.Count);
            Assert.IsNullOrEmpty(jobUpdates[0].ErrorMessage);
            Assert.IsTrue(jobUpdates[1].ErrorMessage.StartsWith("Running the pricing model was canceled."));

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "DiagnosticResult");
            jsonResolver.Ignore(typeof(PlanPricingJob), "ErrorMessage");
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task PassesOnlyChosenInventorySourceTypes_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;


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
                    .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                    .Callback<PlanDto, ProgramInventoryOptionalParametersDto, IEnumerable<int>, PlanPricingJobDiagnostic, Guid>((p1, p2, p3, p4, P5) =>
                    {
                        passedInventorySourceIds.Add(p3);
                    })
                    .Returns(_GetPlanPricingInventoryPrograms());

                var service = _GetService();

                _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.PRICING_MODEL_BARTER_INVENTORY] = true;
                _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY] = false;
                // Act
                await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

                // Assert
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedInventorySourceIds));
            }
            finally
            {
                _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.PRICING_MODEL_BARTER_INVENTORY] = true;
                _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY] = true;
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task NoInventoryFound_WhenRunningPricingJob()
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
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
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
             await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "ErrorMessage");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates, settings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task GoalsFulfilledByProprietaryInventory_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
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
                    AudienceId = 5,
                    CoverageGoalPercent = 80,
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    AvailableMarkets = new List<PlanAvailableMarketDto>(),
                    PricingParameters = _GetPlanPricingParametersDto(),
                    FlightStartDate = new DateTime(2005, 11, 21),
                    FlightEndDate = new DateTime(2005, 12, 18),
                    FlightDays = new List<int>(),
                    TargetRatingPoints = 1000,
                    PostingType = PostingTypeEnum.NSI,
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
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 1,
                        PostingType = PostingTypeEnum.NSI,
                        Station = new DisplayBroadcastStation
                        {
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
                        ManifestId = 1,
                        PostingType = PostingTypeEnum.NTI,
                        Station = new DisplayBroadcastStation
                        {
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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _InventoryProprietarySummaryRepositoryMock
                .Setup(x => x.GetInventoryProprietarySummariesByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetInventoryProprietaryQuarterSummary(true));

            _BroadcastAudienceRepositoryMock
                .Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(new List<audience_audiences>
                {
                    new audience_audiences { rating_audience_id = 1 },
                    new audience_audiences { rating_audience_id = 2 }
                });

            _PlanPricingProgramCalculationEngine.Setup(s => s.CalculateProgramResults(
                    It.IsAny<List<PlanPricingInventoryProgram>>(), It.IsAny<PlanPricingAllocationResult>(),
                    It.IsAny<bool>(), It.IsAny<ProprietaryInventoryData>(), It.IsAny<PostingTypeEnum>()))
                .Returns(new PlanPricingResultBaseDto
                {
                    GoalFulfilledByProprietary = true,
                    JobId = 1,
                    OptimalCpm = 0.0006666666666666666666667000m,
                });

            _PlanPricingBandCalculationEngineMock.Setup(s => s.CalculatePricingBands(
                    It.IsAny<List<PlanPricingInventoryProgram>>(), It.IsAny<PlanPricingAllocationResult>(),
                    It.IsAny<PlanPricingParametersDto>(), It.IsAny<ProprietaryInventoryData>(),
                    It.IsAny<PostingTypeEnum>()))
                .Returns(new PlanPricingBand());

            _PlanPricingStationCalculationEngineMock.Setup(s =>
                    s.Calculate(It.IsAny<List<PlanPricingInventoryProgram>>(), It.IsAny<PlanPricingAllocationResult>(),
                        It.IsAny<PlanPricingParametersDto>(), It.IsAny<ProprietaryInventoryData>(),
                        It.IsAny<PostingTypeEnum>()))
                .Returns(new PlanPricingStationResult());

            _PlanPricingMarketResultsEngine.Setup(s => s.Calculate(It.IsAny<List<PlanPricingInventoryProgram>>(),
                    It.IsAny<PlanPricingAllocationResult>(),
                    It.IsAny<PlanDto>(), It.IsAny<List<MarketCoverage>>(), It.IsAny<ProprietaryInventoryData>(),
                    It.IsAny<PostingTypeEnum>()))
                .Returns(new PlanPricingResultMarkets());

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedAggregationResults));
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
                            Impressions = highProprietaryNumbers ? 30000000 : 100000,
                            CostPerWeek = 50,
                            SpotsPerWeek = 2,
                            StationId = 100
                        },
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 1,
                            MarketCode = 101,
                            Impressions = highProprietaryNumbers ? 30000000 : 100000,
                            CostPerWeek = 50,
                            SpotsPerWeek = 2,
                            StationId = 101
                        },
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 1,
                            MarketCode = 302,
                            Impressions = highProprietaryNumbers ? 40000000 : 100000,
                            CostPerWeek = 50,
                            SpotsPerWeek = 2,
                            StationId = 302
                        },
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 2,
                            MarketCode = 101,
                            Impressions = highProprietaryNumbers ? 100000000 : 1000000,
                            CostPerWeek = 50,
                            SpotsPerWeek = 2,
                            StationId = 101
                        },
                        new InventoryProprietarySummaryByStationByAudience
                        {
                            AudienceId = 3,
                            MarketCode = 101,
                            Impressions = highProprietaryNumbers ? 1000000000 : 10000000,
                            CostPerWeek = 50,
                            SpotsPerWeek = 2,
                            StationId = 101
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
                            Impressions = highProprietaryNumbers ? 100000000 : 1000000,
                            CostPerWeek = 50,
                            SpotsPerWeek = 2,
                            StationId = 101
                        }
                    }
                }
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task SendsRequestToDSAPI_WhenRunningPricingJob()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

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
                    PricingParameters = _GetPlanPricingParametersDto(),
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
                });

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
               .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
               .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
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

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task PricingModelRetunedErrors()
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
                    PricingParameters = _GetPlanPricingParametersDto(),
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
                });

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

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(
                Task.FromResult(
                    new PlanPricingApiSpotsResponseDto_v3
                    {
                        RequestId = "AAAA",
                        Error = new PlanPricingApiSpotsErrorDto_v3
                        {
                            Messages = new List<string>
                            {
                                "Message 1",
                                "Error 2"
                            }
                        }
                    }));

            var jobUpdates = new List<PlanPricingJob>();
            _PlanRepositoryMock
                .Setup(x => x.UpdatePlanPricingJob(It.IsAny<PlanPricingJob>()))
                .Callback<PlanPricingJob>(jobUpdate =>
                {
                    // to skip row number 
                    jobUpdate.DiagnosticResult = jobUpdate.DiagnosticResult?.Substring(0, 60);
                    jobUpdates.Add(jobUpdate);
                });

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

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Assert.AreEqual(2, jobUpdates.Count);
            Assert.IsNull(jobUpdates[0].ErrorMessage);
            Assert.IsTrue(jobUpdates[1].ErrorMessage.StartsWith("Pricing Model Mode ('Quality') Request Id 'AAAA' returned the following error:"));

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "ErrorMessage");
            jsonResolver.Ignore(typeof(PlanPricingJob), "DiagnosticResult");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates, settings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task PricingModelRetuned_UnknownSpot()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

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
                    FlightStartDate = new DateTime(2005, 11, 21),
                    FlightEndDate = new DateTime(2005, 12, 18),
                    FlightDays = new List<int>(),
                    TargetRatingPoints = 1000,
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
                    },
                    ImpressionsPerUnit = 1
                });

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

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(Task.FromResult(
                new PlanPricingApiSpotsResponseDto_v3
                {
                    Results = new List<PlanPricingApiSpotsResultDto_v3>
                    {
                        new PlanPricingApiSpotsResultDto_v3
                        {
                            ManifestId = 1000,
                            MediaWeekId = 1000,
                            Frequencies = new List<SpotFrequencyResponse>
                            {
                                new SpotFrequencyResponse
                                {
                                    SpotLengthId = 1,
                                    Frequency = 5
                                }
                            }
                        }
                    }
                }));

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
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "ErrorMessage");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates, settings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task PricingModelRetuned_NoSpots()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.Margin = 20;
            parameters.JobId = jobId;

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
                    PricingParameters = _GetPlanPricingParametersDto(),
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
                });

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

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(Task.FromResult(
                new PlanPricingApiSpotsResponseDto_v3
                {
                    RequestId = "#q1w2e3",
                    Results = new List<PlanPricingApiSpotsResultDto_v3>()
                }));

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
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Assert.AreEqual(2, jobUpdates.Count);
            Assert.IsNull(jobUpdates[0].ErrorMessage);
            Assert.IsTrue(jobUpdates[1].ErrorMessage.StartsWith("Error attempting to run the pricing model."));

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "ErrorMessage");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobUpdates, settings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task SavesPricingApiResults_WhenRunningPricingJob()
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
                    FlightStartDate = new DateTime(2005, 11, 21),
                    FlightEndDate = new DateTime(2005, 12, 18),
                    FlightDays = new List<int>(),
                    TargetRatingPoints = 1000,
                    PostingType = PostingTypeEnum.NSI,
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
                    },
                    ImpressionsPerUnit = 1
                });

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

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
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
                    new PlanPricingInventoryProgram
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
                    new PlanPricingInventoryProgram
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

            _WeeklyBreakdownEngineMock
               .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
               .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(Task.FromResult(
                new PlanPricingApiSpotsResponseDto_v3
                {
                    RequestId = "#q1w2e3",
                    Results = new List<PlanPricingApiSpotsResultDto_v3>
                    {
                        new PlanPricingApiSpotsResultDto_v3
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
                        new PlanPricingApiSpotsResultDto_v3
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
                        new PlanPricingApiSpotsResultDto_v3
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

            var passedParameters = new List<PlanPricingAllocationResult>();
            _PlanRepositoryMock
                .Setup(x => x.SavePricingApiResults(It.IsAny<PlanPricingAllocationResult>()))
                .Callback<PlanPricingAllocationResult>(p => passedParameters.Add(p));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task SavesPricingAggregateResults_WhenRunningPricingJob()
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

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
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
                        HouseholdProjectedImpressions = 1100,
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
                    new PlanPricingInventoryProgram
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
                        HouseholdProjectedImpressions = 1500,
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
                    new PlanPricingInventoryProgram
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
                    new PlanPricingInventoryProgram
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

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(Task.FromResult(
                    new PlanPricingApiSpotsResponseDto_v3
                    {
                        RequestId = "#q1w2e3",
                        Results = new List<PlanPricingApiSpotsResultDto_v3>
                    {
                        new PlanPricingApiSpotsResultDto_v3
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
                        new PlanPricingApiSpotsResultDto_v3
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
                        new PlanPricingApiSpotsResultDto_v3
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

            var passedParameters = new List<PlanPricingResultBaseDto>();
            _PlanRepositoryMock
                .Setup(x => x.SavePricingAggregateResults(It.IsAny<PlanPricingResultBaseDto>()))
                .Callback<PlanPricingResultBaseDto>(p => passedParameters.Add(p));

            _PlanPricingProgramCalculationEngine.Setup(s => s.CalculateProgramResults(
                    It.IsAny<List<PlanPricingInventoryProgram>>(), It.IsAny<PlanPricingAllocationResult>(),
                    It.IsAny<bool>(), It.IsAny<ProprietaryInventoryData>(), It.IsAny<PostingTypeEnum>()))
                .Returns(new PlanPricingResultBaseDto
                {
                    GoalFulfilledByProprietary = false,
                    JobId = 1,
                    OptimalCpm = 0.1682953311617806731813246000m,
                    PlanVersionId = 77,
                    Programs = new List<PlanPricingProgramDto>
                    {
                        new PlanPricingProgramDto
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
                            Spots = 3,
                            StationCount = 1
                        },
                        new PlanPricingProgramDto
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
                            Spots = 3,
                            StationCount = 1
                        }
                    },
                    Totals = new PlanPricingProgramTotalsDto
                    {
                        AvgCpm = 55.000m,
                        AvgImpressions = 1250,
                        Budget = 412.5m,
                        Impressions = 7500,
                        ImpressionsPercentage = 0,
                        MarketCount = 2,
                        Spots = 6,
                        StationCount = 2
                    }
                });

            _PlanPricingBandCalculationEngineMock.Setup(s => s.CalculatePricingBands(
                    It.IsAny<List<PlanPricingInventoryProgram>>(), It.IsAny<PlanPricingAllocationResult>(),
                    It.IsAny<PlanPricingParametersDto>(), It.IsAny<ProprietaryInventoryData>(),
                    It.IsAny<PostingTypeEnum>()))
                .Returns(new PlanPricingBand());

            _PlanPricingStationCalculationEngineMock.Setup(s =>
                    s.Calculate(It.IsAny<List<PlanPricingInventoryProgram>>(), It.IsAny<PlanPricingAllocationResult>(),
                        It.IsAny<PlanPricingParametersDto>(), It.IsAny<ProprietaryInventoryData>(),
                        It.IsAny<PostingTypeEnum>()))
                .Returns(new PlanPricingStationResult());

            _PlanPricingMarketResultsEngine.Setup(s => s.Calculate(It.IsAny<List<PlanPricingInventoryProgram>>(),
                    It.IsAny<PlanPricingAllocationResult>(),
                    It.IsAny<PlanDto>(), It.IsAny<List<MarketCoverage>>(), It.IsAny<ProprietaryInventoryData>(),
                    It.IsAny<PostingTypeEnum>()))
                .Returns(new PlanPricingResultMarkets());

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        public void IsPricingModelRunning_ReturnsFalse_WhenNoJobsFound()
        {
            // Arrange
            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
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
        public void IsPricingModelRunningForJob_ReturnsFalse_WhenNoJobsFound()
        {
            // Arrange
            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns((PlanPricingJob)null);

            var service = _GetService();

            // Act
            var result = service.IsPricingModelRunningForJob(5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Queued, true)]
        [TestCase(BackgroundJobProcessingStatus.Processing, true)]
        [TestCase(BackgroundJobProcessingStatus.Succeeded, false)]
        [TestCase(BackgroundJobProcessingStatus.Failed, false)]
        [TestCase(BackgroundJobProcessingStatus.Canceled, false)]
        public void IsPricingModelRunningForJob_ChecksJobStatus(BackgroundJobProcessingStatus status, bool expectedResult)
        {
            // Arrange
            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = status
                });

            var service = _GetService();

            // Act
            var actualResult = service.IsPricingModelRunningForJob(5);

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
        public async Task AddsNewPricingJob_WhenQueuesPricing()
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
                Margin = 14               
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
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
        public async Task QueuePricingJob_WhenVerionIdMismatchWithDb()
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
                PlanVersionId=78

            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = 17,
                    VersionId = 77,
                    CoverageGoalPercent = 80,
                    IsDraft=false,
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
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
            CadentException caught = null;
            // Act
            try
            {
                service.QueuePricingJob(parameters, now, user);
            }
            catch (CadentException ex)
            {
                caught = ex;
            }
            // Assert
            Assert.IsTrue(caught.Message.Contains($"The current plan that you are viewing has been updated. Please close the plan and reopen in order to view the most current information"));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task SavesPricingParameters_WhenQueuesPricing()
        {
            // Arrange
            const string user = "test user";
            const int jobId = 5;

            var now = new DateTime(2019, 10, 23);

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
                MarketGroup = MarketGroupEnum.Top100,
                FluidityPercentage = 30
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
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
        [TestCase(null, false)]
        [TestCase(12.0, false)]
        [TestCase(.01, false)]
        [TestCase(100.0, false)]
        [TestCase(112.0, true)]
        [TestCase(.001, true)]
        public async Task SavesPricingParameters_ValidateMargin(double? testMargin, bool expectError)
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
                Margin = testMargin,
                MarketGroup = MarketGroupEnum.Top100
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
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

            Exception caught = null;

            // Act
            try
            {
                service.QueuePricingJob(parameters, now, user);
            }
            catch (Exception e)
            {
                caught = e;
            }

            // Assert
            if (expectError)
            {
                Assert.IsNotNull(caught);
            }
            else
            {
                Assert.IsNull(caught);
            }
        }

        [Test]
        [TestCase(null, false)]
        [TestCase(12.0, false)]
        [TestCase(.01, false)]
        [TestCase(100.0, false)]
        [TestCase(112.0, true)]
        [TestCase(.001, true)]
        public async Task QueuePricingWithoutPlan_ValidateMargin(double? testMargin, bool expectError)
        {
            // Arrange
            const string user = "test user";

            var now = new DateTime(2019, 10, 23);

            var parameters = _GetPricingParametersWithoutPlanDto();
            parameters.Margin = testMargin;

            var service = _GetService();

            Exception caught = null;

            // Act
            try
            {
                service.QueuePricingJob(parameters, now, user);
            }
            catch (Exception e)
            {
                caught = e;
            }

            // Assert
            if (expectError)
            {
                Assert.IsNotNull(caught);
            }
            else
            {
                Assert.IsNull(caught);
            }
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Queued, true)]
        [TestCase(BackgroundJobProcessingStatus.Processing, true)]
        [TestCase(BackgroundJobProcessingStatus.Canceled, false)]
        [TestCase(BackgroundJobProcessingStatus.Failed, false)]
        [TestCase(BackgroundJobProcessingStatus.Succeeded, false)]
        public async Task QueuePricingWithoutPlan_TryRunPricingThatIsRunning(BackgroundJobProcessingStatus jobStatus, bool expectError)
        {
            // Arrange
            const string user = "test user";
            const int jobId = 5;

            var now = new DateTime(2019, 10, 23);

            var parameters = _GetPricingParametersWithoutPlanDto();
            parameters.JobId = jobId;

            _PlanRepositoryMock.Setup(r => r.GetPlanPricingJob(jobId)).Returns(new PlanPricingJob { Id = jobId, Status = jobStatus });

            var service = _GetService();

            Exception caught = null;

            // Act
            try
            {
                service.QueuePricingJob(parameters, now, user);
            }
            catch (Exception e)
            {
                caught = e;
            }

            // Assert
            if (expectError)
            {
                Assert.IsNotNull(caught);
            }
            else
            {
                Assert.IsNull(caught);
            }
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
                Currency = PlanCurrenciesEnum.Impressions,
                CPP = 1.1m,
                DeliveryRatingPoints = 1.3,
                Margin = 10,
                MarketGroup = MarketGroupEnum.Top100,
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
                CoverageGoalPercent = 80,
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task UpdatesCampaignLastModified_WhenQueuesPricing()
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
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
        public async Task QueuesHangfireJob_WhenQueuesPricing()
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
                MarketGroup = MarketGroupEnum.Top100,
                FluidityPercentage = 50
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
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
            // Verify these since they're excluded from the json due version increments
            var passedJob = ((dynamic)passedParameters[0]).job;
            Assert.AreEqual("Services.Broadcast.ApplicationServices.Plan.IPlanPricingService", passedJob.Type.FullName);
            Assert.AreEqual("RunPricingJobAsync", passedJob.Method.Name);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(WaitHandle), "Handle");
            jsonResolver.Ignore(typeof(Job), "Type");
            jsonResolver.Ignore(typeof(Job), "Method");
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Failed,
                    DiagnosticResult = string.Empty,
                    ErrorMessage = expectedMessage
                });

            var service = _GetService();

            // Act
            var exception = Assert.Throws<CadentException>(() => service.GetCurrentPricingExecution(planId));

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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Failed,
                    DiagnosticResult = "SomeDiagnosticResult",
                    ErrorMessage = "SomeErrorMessage"
                });

            var service = _GetService();

            // Act
            var exception = Assert.Throws<CadentException>(() => service.GetCurrentPricingExecution(planId));

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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingResultsByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new CurrentPricingExecutionResultDto
                {
                    Id = 15,
                    OptimalCpm = 5,
                    GoalFulfilledByProprietary = true,
                    PlanVersionId = 11,
                    JobId = 12,
                    CpmPercentage = 204,
                    SpotAllocationModelMode = mode
                    //,CalculatedVpvh = 4.3 // Commented as per requirement of BP-2513
                });

            _PlanRepositoryMock
                .Setup(x => x.GetGoalCpm(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<int?>())).Returns(6.75M);

            var service = _GetService();

            // Act
            var result = service.GetCurrentPricingExecution(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllCurrentPricingExecutions_ForPlanId()
        {
            // Arrange
            const int planId = 128;
            const int JobID = 506;
            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Id = JobID,
                    HangfireJobId = "9895",
                    PlanVersionId = 659,
                    Queued = new DateTime(2021, 3, 24, 11, 12, 13),
                    Completed = new DateTime(2021, 3, 24, 11, 13, 13),
                    DiagnosticResult = "DiagnosticResult"
                });

            _PlanRepositoryMock
                .Setup(x => x.GetAllPricingResultsByJobIds(JobID))
                .Returns(_GetCurrentPricingExecutionsResults());

            _PlanRepositoryMock
                .Setup(x => x.GetGoalCpm(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<int?>())).Returns(6.75M);

            // TODO SDE : this should be reworked for these to be true, as they are in production
            var service = _GetService(false,true);

            // Act
            var result = service.GetAllCurrentPricingExecutions(planId, null);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAllCurrentPricingExecutions_WhenJobIsRunning()
        {
            // Arrange
            const int planId = 128;
            const int JobID = 506;
            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
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

            _PlanRepositoryMock
                .Setup(x => x.GetAllPricingResultsByJobIds(JobID))
                .Returns(_GetCurrentPricingExecutionsResults());

            _PlanRepositoryMock
                .Setup(x => x.GetGoalCpm(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<int?>())).Returns(6.75M);

            var service = _GetService();

            // Act
            var result = service.GetAllCurrentPricingExecutions(planId, null);

            // Assert
            Assert.AreEqual(true, result.IsPricingModelRunning);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllCurrentPricingExecutions_ForPlanAndPlanVersionId()
        {
            // Arrange
            const int planId = 128;
            const int JobID = 506;
            const int PlanVersionId = 659;
            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForPlanVersion(PlanVersionId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Id = JobID,
                    HangfireJobId = "9895",
                    PlanVersionId = 659,
                    Queued = new DateTime(2021, 3, 24, 11, 12, 13),
                    Completed = new DateTime(2021, 3, 24, 11, 13, 13),
                    DiagnosticResult = "DiagnosticResult"
                });

            _PlanRepositoryMock
                .Setup(x => x.GetAllPricingResultsByJobIds(JobID))
                .Returns(_GetCurrentPricingExecutionsResults());

            _PlanRepositoryMock
                .Setup(x => x.GetGoalCpm(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<int?>())).Returns(6.75M);

            // TODO SDE : this should be reworked for these to be true, as they are in production
            var service = _GetService(false,true);

            // Act
            var result = service.GetAllCurrentPricingExecutions(planId, PlanVersionId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void CalculateCpmPercentageTest()
        {
            //Arrange
            var expected = 204M;
            var service = _GetService();

            // Act
            var actual = service.CalculateCpmPercentage(13.75M, 6.75M);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms_Succeeded()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPricingProgramsResultByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new PricingProgramsResultDto
                {
                    SpotAllocationModelMode = mode,
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
        public void GetPrograms_v2_Succeeded()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPricingProgramsResultByJobId_v2(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new PricingProgramsResultDto_v2
                {
                    SpotAllocationModelMode = mode,
                    NsiResults = new PostingTypePlanPricingResultPrograms
                    {
                        ProgramDetails = new List<PlanPricingProgramProgramDto>
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
                    },
                    NtiResults = new PostingTypePlanPricingResultPrograms
                    {
                        ProgramDetails = new List<PlanPricingProgramProgramDto>
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
                            Impressions = 180000,
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
                            AvgImpressions = 110000,
                            ImpressionsPercentage = 100,
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 180000
                        }
                    }
                });

            // Act
            var result = service.GetPrograms_v2(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetPrograms_v2_JobStatusFailed()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Failed,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_v2_JobStatusCanceled()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Canceled,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_v2_JobStatusProcessing()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Processing,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_v2_JobStatusQueued()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Queued,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_v2_JobNull()
        {
            // Arrange
            const int planId = 6;

            var service = _GetService();

            // Act
            var result = service.GetPrograms_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPrograms_v2_GetPricingProgramsResultReturnsNull()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetPrograms_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramsByJobId_Succeeded()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPricingProgramsResultByJobId(jobId, It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new PricingProgramsResultDto
                {
                    SpotAllocationModelMode = mode,
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
            var result = service.GetProgramsByJobId(jobId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetProgramsByJobId_JobStatusFailed()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Failed,
                });

            var service = _GetService();

            // Act
            var result = service.GetProgramsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetProgramsByJobId_JobStatusCanceled()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Canceled,
                });

            var service = _GetService();

            // Act
            var result = service.GetProgramsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetProgramsByJobId_JobStatusProcessing()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Processing,
                });

            var service = _GetService();

            // Act
            var result = service.GetProgramsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetProgramsByJobId_JobStatusQueued()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Queued,
                });

            var service = _GetService();

            // Act
            var result = service.GetProgramsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetProgramsByJobId_JobNull()
        {
            // Arrange
            const int jobId = 6;

            var service = _GetService();

            // Act
            var result = service.GetProgramsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetProgramsByJobId_GetPricingProgramsResultReturnsNull()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetProgramsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStationsByJobId_Succeeded()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPricingStationsResultByJobId(jobId, It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new PlanPricingStationResultDto
                {
                    SpotAllocationModelMode = mode,
                    JobId = jobId,
                    Stations = new List<PlanPricingStationDto>
                    {
                         new PlanPricingStationDto
                         {
                            Id = 7,
                            Cpm = 22,
                            Market = "NY",
                            Station = "ESPN",
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3
                         },
                         new PlanPricingStationDto
                         {
                            Id = 8,
                            Cpm = 11,
                            Market = "NY",
                            Station = "ESPN",
                            Impressions = 2000,
                            ImpressionsPercentage = 4,
                            Budget = 100,
                            Spots = 2
                         }
                    },
                    Totals = new PlanPricingStationTotalsDto
                    {
                        ImpressionsPercentage = 100,
                        Budget = 1131,
                        Spots = 3,
                        Impressions = 200000,
                        Cpm = 22,
                        Station = 1
                    }
                });

            // Act
            var result = service.GetStationsByJobId(jobId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed)]
        [TestCase(BackgroundJobProcessingStatus.Canceled)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        public void GetStationsByJobId_JobStatus(BackgroundJobProcessingStatus status)
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = status,
                });

            var service = _GetService();

            // Act
            var result = service.GetStationsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetStationsByJobId_JobNull()
        {
            // Arrange
            const int jobId = 6;

            var service = _GetService();

            // Act
            var result = service.GetStationsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetStationsByJobId_ReturnsNull()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetStationsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStations_Succeeded()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Id = 5,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPricingStationsResultByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new PlanPricingStationResultDto
                {
                    SpotAllocationModelMode = mode,
                    JobId = 5,
                    PlanVersionId = 3,
                    Stations = new List<PlanPricingStationDto>
                    {
                         new PlanPricingStationDto
                         {
                            Id = 7,
                            Cpm = 22,
                            Market = "NY",
                            Station = "ESPN",
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3
                         }
                    },
                    Totals = new PlanPricingStationTotalsDto
                    {
                        ImpressionsPercentage = 100,
                        Budget = 1131,
                        Spots = 3,
                        Impressions = 200000,
                        Cpm = 22,
                        Station = 1
                    }
                });

            // Act
            var result = service.GetStations(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed)]
        [TestCase(BackgroundJobProcessingStatus.Canceled)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        public void GetStations_JobStatus(BackgroundJobProcessingStatus status)
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = status,
                });

            var service = _GetService();

            // Act
            var result = service.GetStations(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetStations_JobNull()
        {
            // Arrange
            const int planId = 6;

            var service = _GetService();

            // Act
            var result = service.GetStations(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetStations_ReturnsNull()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetStations(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStations_v2_Succeeded()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Id = 5,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPricingStationsResultByJobId_v2(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new PlanPricingStationResultDto_v2
                {
                    SpotAllocationModelMode = mode,
                    JobId = 5,
                    PlanVersionId = 3,
                    NsiResults = new PostingTypePlanPricingResultStations
                    {
                        StationDetails = new List<PlanPricingStationDto>
                    {
                         new PlanPricingStationDto
                         {
                            Id = 7,
                            Cpm = 22,
                            Market = "NY",
                            Station = "ESPN",
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3
                         }
                    },
                        Totals = new PlanPricingStationTotalsDto
                        {
                            ImpressionsPercentage = 100,
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 200000,
                            Cpm = 22,
                            Station = 1
                        }
                    },
                    NtiResults = new PostingTypePlanPricingResultStations
                    {
                        StationDetails = new List<PlanPricingStationDto>
                        {
                        new PlanPricingStationDto
                        {
                            Id = 7,
                            Cpm = 25,
                            Market = "NY",
                            Station = "ESPN",
                            Impressions = 180000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3
                        }
                    },
                        Totals = new PlanPricingStationTotalsDto
                        {
                            ImpressionsPercentage = 100,
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 180000,
                            Cpm = 25,
                            Station = 1
                        }
                    }
                });

            // Act
            var result = service.GetStations_v2(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed)]
        [TestCase(BackgroundJobProcessingStatus.Canceled)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        public void GetStations_v2_JobStatus(BackgroundJobProcessingStatus status)
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = status,
                });

            var service = _GetService();

            // Act
            var result = service.GetStations_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetStations_v2_JobNull()
        {
            // Arrange
            const int planId = 6;

            var service = _GetService();

            // Act
            var result = service.GetStations_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetStations_v2_ReturnsNull()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetStations_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMarketsByJobId_Succeeded()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingResultMarketsByJobId(jobId, It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new PlanPricingResultMarketsDto
                {
                    SpotAllocationModelMode = mode,
                    MarketDetails = new List<PlanPricingResultMarketDetailsDto>
                    {
                         new PlanPricingResultMarketDetailsDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            MarketCoveragePercent = 70,
                            Rank = 2,
                            ShareOfVoiceGoalPercentage = 70,
                            Stations = 5,
                            MarketName = "Chicago"
                         }
                    },
                    PricingJobId = jobId,
                    PlanVersionId = 3,
                    Totals = new PlanPricingResultMarketsTotalsDto
                    {
                        Budget = 1131,
                        Spots = 3,
                        Impressions = 200000,
                        Cpm = 22,
                        Stations = 5,
                        CoveragePercent = 70,
                        Markets = 1
                    },
                });

            // Act
            var result = service.GetMarketsByJobId(jobId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed)]
        [TestCase(BackgroundJobProcessingStatus.Canceled)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        public void GetMarketsByJobId_JobStatus(BackgroundJobProcessingStatus status)
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = status,
                });

            var service = _GetService();

            // Act
            var result = service.GetMarketsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetMarketsByJobId_JobNull()
        {
            // Arrange
            const int jobId = 6;

            var service = _GetService();

            // Act
            var result = service.GetMarketsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetMarketsByJobId_ReturnsNull()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetMarketsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMarkets_Succeeded()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Id = 5,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingResultMarketsByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new PlanPricingResultMarketsDto
                {
                    SpotAllocationModelMode = mode,
                    MarketDetails = new List<PlanPricingResultMarketDetailsDto>
                    {
                         new PlanPricingResultMarketDetailsDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            MarketCoveragePercent = 70,
                            Rank = 2,
                            ShareOfVoiceGoalPercentage = 70,
                            Stations = 6,
                            MarketName = "Chicago"
                         },
                         new PlanPricingResultMarketDetailsDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            MarketCoveragePercent = 70,
                            Rank = 2,
                            ShareOfVoiceGoalPercentage = 70,
                            Stations = 1,
                            MarketName = "Chicago"
                         }
                    },
                    PricingJobId = 2,
                    PlanVersionId = 3,

                    Totals = new PlanPricingResultMarketsTotalsDto
                    {
                        Budget = 1131,
                        Spots = 3,
                        Impressions = 200000,
                        Cpm = 22,
                        Stations = 5,
                        CoveragePercent = 70,
                        Markets = 1
                    }
                });

            // Act
            var result = service.GetMarkets(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMarketsByJobId_v2_Succeeded()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingResultMarketsByJobId_v2(jobId, It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new PlanPricingResultMarketsDto_v2
                {
                    SpotAllocationModelMode = mode,
                    PricingJobId = jobId,
                    PlanVersionId = 3,
                    NsiResults = new PostingTypePlanPricingResultMarkets
                    {
                        MarketDetails = new List<PlanPricingResultMarketDetailsDto>
                    {
                         new PlanPricingResultMarketDetailsDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            MarketCoveragePercent = 70,
                            Rank = 2,
                            ShareOfVoiceGoalPercentage = 70,
                            Stations = 5,
                            MarketName = "Chicago"
                         }
                    },
                        Totals = new PlanPricingResultMarketsTotalsDto
                        {
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 200000,
                            Cpm = 22,
                            Stations = 5,
                            CoveragePercent = 70,
                            Markets = 1
                        }
                    },

                    NtiResults = new PostingTypePlanPricingResultMarkets
                    {
                        MarketDetails = new List<PlanPricingResultMarketDetailsDto>
                    {
                         new PlanPricingResultMarketDetailsDto
                         {
                            Cpm = 22,
                            Impressions = 180000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            MarketCoveragePercent = 70,
                            Rank = 2,
                            ShareOfVoiceGoalPercentage = 70,
                            Stations = 5,
                            MarketName = "Chicago"
                         }
                    },
                        Totals = new PlanPricingResultMarketsTotalsDto
                        {
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 180000,
                            Cpm = 25,
                            Stations = 5,
                            CoveragePercent = 70,
                            Markets = 1
                        }
                    }
                });

            // Act
            var result = service.GetMarketsByJobId_v2(jobId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed)]
        [TestCase(BackgroundJobProcessingStatus.Canceled)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        public void GetMarketsByJobId_v2_JobStatus(BackgroundJobProcessingStatus status)
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = status,
                });

            var service = _GetService();

            // Act
            var result = service.GetMarketsByJobId_v2(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetMarketsByJobId_v2_JobNull()
        {
            // Arrange
            const int jobId = 6;

            var service = _GetService();

            // Act
            var result = service.GetMarketsByJobId_v2(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetMarketsByJobId_v2_ReturnsNull()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetMarketsByJobId_v2(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMarkets_v2_Succeeded()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Id = 5,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingResultMarketsByJobId_v2(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanPricingResultMarketsDto_v2
                {
                    PricingJobId = 2,
                    PlanVersionId = 3,
                    NsiResults = new PostingTypePlanPricingResultMarkets
                    {
                        MarketDetails = new List<PlanPricingResultMarketDetailsDto>
                    {
                         new PlanPricingResultMarketDetailsDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            MarketCoveragePercent = 70,
                            Rank = 2,
                            ShareOfVoiceGoalPercentage = 70,
                            Stations = 5,
                            MarketName = "Chicago"
                         }
                    },
                        Totals = new PlanPricingResultMarketsTotalsDto
                        {
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 200000,
                            Cpm = 22,
                            Stations = 5,
                            CoveragePercent = 70,
                            Markets = 1
                        }
                    },

                    NtiResults = new PostingTypePlanPricingResultMarkets
                    {
                        MarketDetails = new List<PlanPricingResultMarketDetailsDto>
                    {
                         new PlanPricingResultMarketDetailsDto
                         {
                            Cpm = 22,
                            Impressions = 180000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            MarketCoveragePercent = 70,
                            Rank = 2,
                            ShareOfVoiceGoalPercentage = 70,
                            Stations = 5,
                            MarketName = "Chicago"
                         }
                    },
                        Totals = new PlanPricingResultMarketsTotalsDto
                        {
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 180000,
                            Cpm = 25,
                            Stations = 5,
                            CoveragePercent = 70,
                            Markets = 1
                        }
                    }
                });

            // Act
            var result = service.GetMarkets_v2(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed)]
        [TestCase(BackgroundJobProcessingStatus.Canceled)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        public void GetMarkets_JobStatus(BackgroundJobProcessingStatus status)
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = status,
                });

            var service = _GetService();

            // Act
            var result = service.GetMarkets(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetMarkets_JobNull()
        {
            // Arrange
            const int planId = 6;

            var service = _GetService();

            // Act
            var result = service.GetMarkets(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetMarkets_ReturnsNull()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetMarkets(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingBandsByJobId_Succeeded()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingBandByJobId(jobId, It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanPricingBandDto
                {
                    PlanVersionId = 3,
                    JobId = jobId,
                    Bands = new List<PlanPricingBandDetailDto>
                     {
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 32,
                            Budget = 1131,
                            Spots = 3,
                            AvailableInventoryPercent = 20,
                            MaxBand = 5,
                            MinBand = 1
                         },
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 10,
                            Impressions = 100000,
                            ImpressionsPercentage = 22,
                            Budget = 2131,
                            Spots = 4,
                            AvailableInventoryPercent = 10,
                            MaxBand = 5,
                            MinBand = 1
                         },
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 32,
                            Budget = 1131,
                            Spots = 3,
                            AvailableInventoryPercent = 20,
                            MaxBand = 5,
                            MinBand = null
                         },
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 10,
                            Impressions = 100000,
                            ImpressionsPercentage = 22,
                            Budget = 2131,
                            Spots = 4,
                            AvailableInventoryPercent = 10,
                            MaxBand = 5,
                            MinBand = null
                         }
                     },
                    Totals = new PlanPricingBandTotalsDto
                    {
                        Budget = 1131,
                        Spots = 3,
                        Impressions = 200000,
                        Cpm = 22,
                    }
                });

            // Act
            var result = service.GetPricingBandsByJobId(jobId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed)]
        [TestCase(BackgroundJobProcessingStatus.Canceled)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        public void GetPricingBandsByJobId_JobStatus(BackgroundJobProcessingStatus status)
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = status,
                });

            var service = _GetService();

            // Act
            var result = service.GetPricingBandsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPricingBandsByJobId_JobNull()
        {
            // Arrange
            const int jobId = 6;

            var service = _GetService();

            // Act
            var result = service.GetPricingBandsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPricingBandsByJobId_ReturnsNull()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetPricingBandsByJobId(jobId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingBands_Succeeded()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Id = 5,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingBandByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanPricingBandDto
                {
                    PlanVersionId = 3,
                    JobId = 5,
                    Bands = new List<PlanPricingBandDetailDto>
                     {
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            AvailableInventoryPercent = 70,
                            MaxBand = 5,
                            MinBand = 1,
                         }
                     },
                    Totals = new PlanPricingBandTotalsDto
                    {
                        Budget = 1131,
                        Spots = 3,
                        Impressions = 200000,
                        Cpm = 22,
                    }
                });

            // Act
            var result = service.GetPricingBands(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed)]
        [TestCase(BackgroundJobProcessingStatus.Canceled)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        public void GetPricingBands_JobStatus(BackgroundJobProcessingStatus status)
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = status,
                });

            var service = _GetService();

            // Act
            var result = service.GetPricingBands(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPricingBands_JobNull()
        {
            // Arrange
            const int planId = 6;

            var service = _GetService();

            // Act
            var result = service.GetPricingBands(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPricingBands_ReturnsNull()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetPricingBands(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingBandsByJobId_v2_Succeeded()
        {
            // Arrange
            const int jobId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(jobId))
                .Returns(new PlanPricingJob
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingBandByJobId_v2(jobId, It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanPricingBandDto_v2
                {
                    PlanVersionId = 3,
                    JobId = jobId,
                    NsiResults = new PostingTypePlanPricingResultBands
                    {
                        BandsDetails = new List<PlanPricingBandDetailDto>
                     {
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 32,
                            Budget = 1131,
                            Spots = 3,
                            AvailableInventoryPercent = 20,
                            MaxBand = 5,
                            MinBand = 1
                         },
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 10,
                            Impressions = 100000,
                            ImpressionsPercentage = 22,
                            Budget = 2131,
                            Spots = 4,
                            AvailableInventoryPercent = 10,
                            MaxBand = 5,
                            MinBand = 1
                         },
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 32,
                            Budget = 1131,
                            Spots = 3,
                            AvailableInventoryPercent = 20,
                            MaxBand = 5,
                            MinBand = null
                         },
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 10,
                            Impressions = 100000,
                            ImpressionsPercentage = 22,
                            Budget = 2131,
                            Spots = 4,
                            AvailableInventoryPercent = 10,
                            MaxBand = 5,
                            MinBand = null
                         }
                     },
                        Totals = new PlanPricingBandTotalsDto
                        {
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 200000,
                            Cpm = 22,
                        }
                    },
                    NtiResults = new PostingTypePlanPricingResultBands
                    {
                        BandsDetails = new List<PlanPricingBandDetailDto>
                     {
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 25,
                            Impressions = 180000,
                            ImpressionsPercentage = 32,
                            Budget = 1131,
                            Spots = 3,
                            AvailableInventoryPercent = 20,
                            MaxBand = 5,
                            MinBand = 1
                         },
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 10,
                            Impressions = 100000,
                            ImpressionsPercentage = 25,
                            Budget = 2131,
                            Spots = 4,
                            AvailableInventoryPercent = 10,
                            MaxBand = 5,
                            MinBand = 1
                         },
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 25,
                            Impressions = 180000,
                            ImpressionsPercentage = 32,
                            Budget = 1131,
                            Spots = 3,
                            AvailableInventoryPercent = 20,
                            MaxBand = 5,
                            MinBand = null
                         },
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 10,
                            Impressions = 100000,
                            ImpressionsPercentage = 25,
                            Budget = 2131,
                            Spots = 4,
                            AvailableInventoryPercent = 10,
                            MaxBand = 5,
                            MinBand = null
                         }
                     },
                        Totals = new PlanPricingBandTotalsDto
                        {
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 180000,
                            Cpm = 25,
                        }
                    }
                });

            // Act
            var result = service.GetPricingBandsByJobId(jobId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingBands_v2_Succeeded()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Id = 5,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingBandByJobId_v2(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(new PlanPricingBandDto_v2
                {
                    PlanVersionId = 3,
                    JobId = 5,
                    NsiResults = new PostingTypePlanPricingResultBands
                    {
                        BandsDetails = new List<PlanPricingBandDetailDto>
                     {
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 22,
                            Impressions = 200000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            AvailableInventoryPercent = 70,
                            MaxBand = 5,
                            MinBand = 1,
                         }
                     },
                        Totals = new PlanPricingBandTotalsDto
                        {
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 200000,
                            Cpm = 22,
                        }
                    },
                    NtiResults = new PostingTypePlanPricingResultBands
                    {
                        BandsDetails = new List<PlanPricingBandDetailDto>
                     {
                         new PlanPricingBandDetailDto
                         {
                            Cpm = 25,
                            Impressions = 180000,
                            ImpressionsPercentage = 96,
                            Budget = 1131,
                            Spots = 3,
                            AvailableInventoryPercent = 70,
                            MaxBand = 5,
                            MinBand = 1,
                         }
                     },
                        Totals = new PlanPricingBandTotalsDto
                        {
                            Budget = 1131,
                            Spots = 3,
                            Impressions = 180000,
                            Cpm = 25,
                        }
                    }
                });

            // Act
            var result = service.GetPricingBands_v2(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed)]
        [TestCase(BackgroundJobProcessingStatus.Canceled)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        public void GetPricingBands_v2_JobStatus(BackgroundJobProcessingStatus status)
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = status,
                });

            var service = _GetService();

            // Act
            var result = service.GetPricingBands_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPricingBands_v2_JobNull()
        {
            // Arrange
            const int planId = 6;

            var service = _GetService();

            // Act
            var result = service.GetPricingBands_v2(planId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetPricingBands_v2_ReturnsNull()
        {
            // Arrange
            const int planId = 6;

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                });

            var service = _GetService();

            // Act
            var result = service.GetPricingBands_v2(planId);

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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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
                .Setup(x => x.GetPricingResultsByJobId(It.IsAny<int>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<int, SpotAllocationModelMode>((id, mode) => new CurrentPricingExecutionResultDto
                {
                    Id = 15,
                    OptimalCpm = 5,
                    GoalFulfilledByProprietary = true,
                    PlanVersionId = 11,
                    JobId = 12,
                    SpotAllocationModelMode = mode
                    //,CalculatedVpvh = 12.3  // Commented as per requirement of BP-2513
                });
            _PlanRepositoryMock
                .Setup(x => x.GetGoalCpm(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<int?>()))
                .Returns(110.0M);
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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Failed
                });

            var service = _GetService();

            // Act
            var exception = Assert.Throws<CadentException>(() => service.CancelCurrentPricingExecution(planId));

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
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded
                });

            var service = _GetService();

            // Act
            var exception = Assert.Throws<CadentException>(() => service.CancelCurrentPricingExecution(planId));

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
            var currentDateTime = new DateTime(2020, 2, 4, 15, 32, 52);

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);
            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "Completed");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var service = _GetService();

            // Act
            var cancelCurrentPricingExecutionResult = service.CancelCurrentPricingExecution(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(new
            {
                cancelCurrentPricingExecutionResult,
                jobUpdates,
                hanfgireJobUpdates
            }, settings));
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
            var currentDateTime = new DateTime(2020, 2, 4, 15, 32, 52);

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);
            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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

                    hanfgireJobUpdates.Add(new { jobId, state, expectedState });
                });

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "Completed");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var service = _GetService();

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
            }, settings));
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
            var currentDateTime = new DateTime(2020, 2, 4, 15, 32, 52);

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);
            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(planId))
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

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanPricingJob), "Completed");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var service = _GetService();

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
            }, settings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobWithoutMargin()
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

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top100;
            parameters.Margin = null;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobWithMargin()
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

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top100;
            parameters.Margin = 10;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobWithProprietaryInventory()
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

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top100;
            parameters.Margin = 10;
            parameters.JobId = jobId;
            parameters.ProprietaryInventory = new List<InventoryProprietarySummary>
            {
                new InventoryProprietarySummary { Id = 1 },
                new InventoryProprietarySummary { Id = 2 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(Task.FromResult(
                    new PlanPricingApiSpotsResponseDto_v3
                    {
                        RequestId = "q1w2e3r4",
                        Results = new List<PlanPricingApiSpotsResultDto_v3>()
                    }))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobWithProprietaryInventory_15Only()
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

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top100;
            parameters.Margin = 10;
            parameters.JobId = jobId;
            parameters.ProprietaryInventory = new List<InventoryProprietarySummary>
            {
                new InventoryProprietarySummary { Id = 1 },
                new InventoryProprietarySummary { Id = 2 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            var plan = _GetPlan();
            plan.CreativeLengths[0].SpotLengthId = 2;

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(Task.FromResult(
                    new PlanPricingApiSpotsResponseDto_v3
                    {
                        RequestId = "q1w2e3r4",
                        Results = new List<PlanPricingApiSpotsResultDto_v3>()
                    }))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobTop100MarketsTest()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top100;
            parameters.Margin = 20;
            parameters.JobId = jobId;

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

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobTop50MarketsTest()
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

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top50;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop50MarketCoverages())
                .Returns(_GetTop50Markets());

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

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobTop25MarketsTest()
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

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top25;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop25MarketCoverages())
                .Returns(_GetTop25Markets());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
                .Returns(_GetWeeklyBreakDownWeeks());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakDownGroup());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobAllMarketsTest()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.All;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>()
                    , It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

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

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobTieredInventory()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top100;
            parameters.Margin = 10;
            parameters.JobId = jobId;

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

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlanTiered());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetTieredInventory());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop100MarketCoverages())
                .Returns(MarketsTestData.GetTop100Markets());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakDownGroupTiered());

            _WeeklyBreakdownEngineMock
               .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
               .Returns(_GetWeeklyBreakDownWeeksTiered());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobTieredInventoryWithAllocations()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.Top100;
            parameters.Margin = 10;
            parameters.JobId = jobId;

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

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlanTiered());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetTieredInventory());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestTop100MarketCoverages())
                .Returns(MarketsTestData.GetTop100Markets());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakDownGroupTiered());

            _WeeklyBreakdownEngineMock
               .Setup(x => x.CalculatePlanWeeklyGoalBreakdown(It.IsAny<WeeklyBreakdownRequest>()))
               .Returns(_GetWeeklyBreakDownWeeksTiered());

            _WeeklyBreakdownEngineMock
                .Setup(x => x.DistributeGoalsByWeeksAndSpotLengthsAndStandardDayparts(It.IsAny<PlanDto>(), It.IsAny<double?>(), It.IsAny<decimal?>()))
                .Returns(_GetWeeklyBreakDownWeeks_DistributedBySpotLengthAndDaypart());

            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(Task.FromResult(_GetTieredAllocations()));

            var allocations = new List<PlanPricingAllocationResult>();
            _PlanPricingBandCalculationEngineMock
                .Setup(x => x.CalculatePricingBands(
                    It.IsAny<List<PlanPricingInventoryProgram>>(),
                    It.IsAny<PlanPricingAllocationResult>(),
                    It.IsAny<PlanPricingParametersDto>(),
                    It.IsAny<ProprietaryInventoryData>(),
                     It.IsAny<PostingTypeEnum>()))
                .Callback<List<PlanPricingInventoryProgram>,
                          PlanPricingAllocationResult,
                          PlanPricingParametersDto,
                          ProprietaryInventoryData,
                          PostingTypeEnum>((inventory, allocation, pricingParameters, proprietaryInventoryData, postingType) => allocations.Add(allocation));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(allocations));
        }

        private List<CurrentPricingExecutionResultDto> _GetCurrentPricingExecutionsResults()
        {
            return new List<CurrentPricingExecutionResultDto>
                    {
                        new CurrentPricingExecutionResultDto()
                        {
                            Id = 15,
                            OptimalCpm = 11,
                            JobId = 12,
                            PlanVersionId = 659,
                            GoalFulfilledByProprietary = false,
                            Notes = "",
                            HasResults = true,
                            CpmPercentage = 63,
                            PostingType = PostingTypeEnum.NTI,
                            SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                            //CalculatedVpvh = 12.3, // Commented as per requirement of BP-2513
                            TotalImpressions = 1010680,
                            TotalBudget = 37220.625m
                        },
                        new CurrentPricingExecutionResultDto()
                        {
                            Id = 15,
                            OptimalCpm = 7,
                            JobId = 506,
                            PlanVersionId = 659,
                            GoalFulfilledByProprietary = false,
                            Notes = "",
                            HasResults = true,
                            CpmPercentage = 44,
                            PostingType = PostingTypeEnum.NSI,
                            SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                            //CalculatedVpvh = 4.6, // Commented as per requirement of BP-2513
                            TotalImpressions = 1010680,
                            TotalBudget = 37220.625m
                        },
                        new CurrentPricingExecutionResultDto()
                        {
                            Id = 15,
                            OptimalCpm = 12,
                            JobId = 506,
                            PlanVersionId = 659,
                            GoalFulfilledByProprietary = false,
                            Notes = "",
                            HasResults = true,
                            CpmPercentage = 67,
                            PostingType = PostingTypeEnum.NSI,
                            SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                            //CalculatedVpvh = 6.2, // Commented as per requirement of BP-2513
                            TotalImpressions = 1010680,
                            TotalBudget = 37220.625m
                        },
                        new CurrentPricingExecutionResultDto()
                        {
                            Id = 15,
                            OptimalCpm = 8,
                            JobId = 506,
                            PlanVersionId = 659,
                            GoalFulfilledByProprietary = false,
                            Notes = "",
                            HasResults = true,
                            CpmPercentage = 47,
                            PostingType = PostingTypeEnum.NTI,
                            SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                            //CalculatedVpvh = 126.3, // Commented as per requirement of BP-2513
                            TotalImpressions = 1010680,
                            TotalBudget = 37220.625m
                        },
                        new CurrentPricingExecutionResultDto()
                        {
                            Id = 15,
                            OptimalCpm = 28,
                            JobId = 506,
                            PlanVersionId = 659,
                            GoalFulfilledByProprietary = false,
                            Notes = "",
                            HasResults = true,
                            CpmPercentage = 160,
                            PostingType = PostingTypeEnum.NSI,
                            SpotAllocationModelMode = SpotAllocationModelMode.Quality,
                            //CalculatedVpvh = 3.1, // Commented as per requirement of BP-2513
                            TotalImpressions = 1010680,
                            TotalBudget = 37220.625m
                        },
                        new CurrentPricingExecutionResultDto()
                        {
                            Id = 15,
                            OptimalCpm = 20,
                            JobId = 506,
                            PlanVersionId = 659,
                            GoalFulfilledByProprietary = false,
                            Notes = "",
                            HasResults = true,
                            CpmPercentage = 112,
                            PostingType = PostingTypeEnum.NTI,
                            SpotAllocationModelMode = SpotAllocationModelMode.Quality,
                            //CalculatedVpvh = 8.2, // Commented as per requirement of BP-2513
                            TotalImpressions = 1010680,
                            TotalBudget = 37220.625m
                        }
                };
        }

        private PlanPricingApiSpotsResponseDto_v3 _GetTieredAllocations()
        {
            return new PlanPricingApiSpotsResponseDto_v3
            {
                RequestId = "AAA",
                Results = new List<PlanPricingApiSpotsResultDto_v3>
                {
                    new PlanPricingApiSpotsResultDto_v3
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
                    new PlanPricingApiSpotsResultDto_v3
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
                    new PlanPricingApiSpotsResultDto_v3
                    {
                        ManifestId = 1,
                        MediaWeekId = 102,
                        Frequencies = new List<SpotFrequencyResponse>
                        {
                            new SpotFrequencyResponse
                            {
                                SpotLengthId = 1,
                                Frequency = 3
                            }
                        }
                    },
                    new PlanPricingApiSpotsResultDto_v3
                    {
                        ManifestId = 1,
                        MediaWeekId = 103,
                        Frequencies = new List<SpotFrequencyResponse>
                        {
                            new SpotFrequencyResponse
                            {
                                SpotLengthId = 1,
                                Frequency = 4
                            }
                        }
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

        private List<WeeklyBreakdownByWeek> _GetWeeklyBreakDownGroupTiered()
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
                        Impressions = 150,
                        Budget = 15m,
                        MediaWeekId = 102
                    },
                    new WeeklyBreakdownByWeek
                    {
                        Impressions = 200,
                        Budget = 15m,
                        MediaWeekId = 103
                    }
                };
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

        private WeeklyBreakdownResponseDto _GetWeeklyBreakDownWeeksTiered()
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
                        WeeklyImpressions = 150,
                        WeeklyBudget = 15m,
                        MediaWeekId = 102
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeeklyImpressions = 200,
                        WeeklyBudget = 15m,
                        MediaWeekId = 103
                    }
                }
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingJobNoMarketsTest()
        {
            // Arrange
            const int jobId = 1;

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.None;
            parameters.Margin = 20;
            parameters.JobId = jobId;

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

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Callback<PlanPricingApiRequestDto_v3>(request => requests.Add(request));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(requests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingSavePricingRequest()
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

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.None;
            parameters.Margin = 20;
            parameters.JobId = jobId;
            parameters.BudgetCpmLever = BudgetCpmLeverEnum.Budget;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(_GetPlan());

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(Task.FromResult(new PlanPricingApiSpotsResponseDto_v3 { RequestId = "Request1", Results = new List<PlanPricingApiSpotsResultDto_v3>()}));

            var service = _GetService();

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(WaitHandle), "Handle");
            jsonResolver.Ignore(typeof(object), "UT_CurrentDateTime");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var passedParameters = _AsyncTaskHelperStub.TaskFireAndForgetActions.Select(s => s.Target).ToList();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters, settings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task RunPricingSavePricingRequest_v3()
        {
            // Arrange
            var isMultiCreativeLengthAllowed = true;
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

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.None;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

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

            parameters.UnitCapsType = UnitCapEnum.Per30Min;
            parameters.UnitCaps = 3;

            plan.PricingParameters.UnitCapsType = parameters.UnitCapsType;
            plan.PricingParameters.UnitCaps = parameters.UnitCaps;

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

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms_v3());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns(Task.FromResult(
                    new PlanPricingApiSpotsResponseDto_v3
                    {
                        Error = null,
                        RequestId = "qwedw121",
                        Results = new List<PlanPricingApiSpotsResultDto_v3>()
                    }));

            _SpotLengthEngineMock
                .Setup(x => x.GetDeliveryMultiplierBySpotLengthId(It.IsAny<int>()))
                .Returns<int>(id => id == 1 ? 1 : 0.5);

            var service = _GetService(false, isMultiCreativeLengthAllowed);

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(WaitHandle), "Handle");
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var passedParameters = _AsyncTaskHelperStub.TaskFireAndForgetActions.Select(s => s.Target).ToList();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters, settings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task SavesPricingApiResults_v3()
        {
            // Arrange
            var isMultiCreativeLengthAllowed = true;
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

            var parameters = _GetPlanPricingParametersDto();
            parameters.MarketGroup = MarketGroupEnum.None;
            parameters.Margin = 20;
            parameters.JobId = jobId;

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

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

            parameters.UnitCapsType = UnitCapEnum.Per30Min;
            parameters.UnitCaps = 3;

            plan.PricingParameters.UnitCapsType = parameters.UnitCapsType;
            plan.PricingParameters.UnitCaps = parameters.UnitCaps;

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

            _PlanPricingInventoryEngineMock
                .Setup(x => x.GetInventoryForPlan(It.IsAny<PlanDto>(), It.IsAny<ProgramInventoryOptionalParametersDto>(), It.IsAny<IEnumerable<int>>(), It.IsAny<PlanPricingJobDiagnostic>(), It.IsAny<Guid>()))
                .Returns(_GetMultipleInventoryPrograms_v3());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetLatestMarketCoverages());

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

            var requests = new List<PlanPricingApiRequestDto_v3>();
            _PricingApiClientMock
                .Setup(x => x.GetPricingSpotsResultAsync(It.IsAny<PlanPricingApiRequestDto_v3>()))
                .Returns<PlanPricingApiRequestDto_v3>((request) =>
                {
                    var results = new List<PlanPricingApiSpotsResultDto_v3>();

                    foreach (var spot in request.Spots)
                    {
                        var result = new PlanPricingApiSpotsResultDto_v3
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

                    return Task.FromResult(
                        new PlanPricingApiSpotsResponseDto_v3
                        {
                            RequestId = "djj4j4399fmmf1m212",
                            Results = results
                        });
                });

            _SpotLengthEngineMock
                .Setup(x => x.GetDeliveryMultiplierBySpotLengthId(It.IsAny<int>()))
                .Returns<int>(id => id == 1 ? 1 : 0.5);

            var passedParameters = new List<PlanPricingAllocationResult>();
            _PlanRepositoryMock
                .Setup(x => x.SavePricingApiResults(It.IsAny<PlanPricingAllocationResult>()))
                .Callback<PlanPricingAllocationResult>(p => passedParameters.Add(p));

            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaWeekById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaWeek);

            var service = _GetService(false, isMultiCreativeLengthAllowed);

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(passedParameters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingModelSpots_V3_FilterOutZeroCostSpots()
        {
            // Arrange
            var inventory = new List<PlanPricingInventoryProgram>
            {
                _GetInventoryProgram()[0],
                _GetInventoryProgram()[0],
                _GetInventoryProgram()[0],
                _GetInventoryProgram()[0]
            };
            inventory[0].ManifestRates = new List<ManifestRate>
            {
                new ManifestRate { SpotLengthId = 1, Cost = 50 },
                new ManifestRate { SpotLengthId = 2, Cost = 25 },
            };
            inventory[1].ManifestId = 2;
            inventory[1].Station = new DisplayBroadcastStation { Id = 6, LegacyCallLetters = "WCBS", MarketCode = 106, };
            inventory[1].ManifestRates = new List<ManifestRate>
            {
                new ManifestRate { SpotLengthId = 1, Cost = 0 },
                new ManifestRate { SpotLengthId = 2, Cost = 25 },
            };
            inventory[2].ManifestId = 3;
            inventory[2].Station = new DisplayBroadcastStation { Id = 7, LegacyCallLetters = "WWWW", MarketCode = 107, };
            inventory[2].ManifestRates = new List<ManifestRate>
            {
                new ManifestRate { SpotLengthId = 1, Cost = 50 },
                new ManifestRate { SpotLengthId = 2, Cost = 0 },
            };
            inventory[3].ManifestId = 4;
            inventory[3].Station = new DisplayBroadcastStation { Id = 8, LegacyCallLetters = "WYYY", MarketCode = 108, };
            inventory[3].ManifestRates = new List<ManifestRate>
            {
                new ManifestRate { SpotLengthId = 1, Cost = 0 },
                new ManifestRate { SpotLengthId = 2, Cost = 0 },
            };

            var groupedInventory = PlanPricingService._GroupInventory(inventory);

            var skippedWeekIds = new List<int>();

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(MarketsTestData.GetLatestMarketCoverages);

            var service = _GetService();

            // Act
            var result = service._GetPricingModelSpots_v3(groupedInventory, skippedWeekIds);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingModelSpots_V3_HandleDupWeekInventory()
        {
            // Arrange
            var inventory = new List<PlanPricingInventoryProgram>
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
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 103,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 104,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 105,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
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
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 103,
                            }
                        }
                    }
                };
            var skippedWeekIds = new List<int>();
            var groupedInventory = PlanPricingService._GroupInventory(inventory);

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(MarketsTestData.GetLatestMarketCoverages());

            var service = _GetService();

            // Act
            var result = service._GetPricingModelSpots_v3(groupedInventory, skippedWeekIds);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingModelSpots_HandleDupWeekInventory()
        {
            // Arrange
            var inventory = new List<PlanPricingInventoryProgram>
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
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 103,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 104,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 105,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
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
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 103,
                            }
                        }
                    }
                };
            var skippedWeekIds = new List<int>();
            var groupedInventory = PlanPricingService._GroupInventory(inventory);

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverages(It.IsAny<IEnumerable<int>>()))
                .Returns(MarketsTestData.GetLatestMarketCoverages());

            var service = _GetService();

            // Act
            var result = service._GetPricingModelSpots_v3(groupedInventory, skippedWeekIds);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(SpotAllocationModelMode.Floor)]
        public void _GetPricingModelWeeks_v3Test_WithEfficencyAndFloor_Returns1Cpm(SpotAllocationModelMode allocationMode)
        {
            //arrange
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            var pricingParameters = new PlanPricingParametersDto
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
                PostingType = PostingTypeEnum.NSI
            };

            plan.PricingParameters = pricingParameters;
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
            var result = service._GetPricingModelWeeks_v3(plan, pricingParameters, proprietaryData,
                out skippedWeekIds, allocationMode);

            //assert
            Assert.IsTrue(result.All(x => x.CpmGoal == 1));
        }

        [Test]
        public void _GetPricingModelWeeks_v3Test_WithFloor_ReturnsEmptyShareOfVoice()
        {
            //arrange
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            var pricingParameters = new PlanPricingParametersDto
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
                PostingType = PostingTypeEnum.NSI
            };

            plan.PricingParameters = pricingParameters;
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
            var result = service._GetPricingModelWeeks_v3(plan, pricingParameters, proprietaryData,
                out skippedWeekIds, SpotAllocationModelMode.Floor);

            //assert
            Assert.IsTrue(result.All(x => !x.ShareOfVoice.Any()));
        }

        [Test]
        public void _GetPricingModelWeeks_v3Test_WithQuality_ReturnsCalculatedCpmAndShareOfVoice()
        {
            //arrange
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            var pricingParameters = new PlanPricingParametersDto
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
                PostingType = PostingTypeEnum.NSI
            };

            plan.PricingParameters = pricingParameters;
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
            var result = service._GetPricingModelWeeks_v3(plan, pricingParameters, proprietaryData,
                out skippedWeekIds, SpotAllocationModelMode.Quality);

            //assert
            Assert.IsTrue(result.All(x => x.ShareOfVoice.Any()));
            Assert.IsTrue(result.All(x => x.CpmGoal != 1));
        }
        
        [Test]
        public void ValidatePricingExecutionResultTest()
        {

            int expectedResult = 3;
            var result = new CurrentPricingExecutions
            {
                Job = new PlanPricingJob
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
                Results = new List<CurrentPricingExecutionResultDto>
                {
                                 new CurrentPricingExecutionResultDto
                                    {
                                        Id = 15,
                                       OptimalCpm=21,JobId=755,
                                        PlanVersionId=805,
                                        GoalFulfilledByProprietary=false,
                                        Notes="",
                                        HasResults=true,
                                        CpmPercentage=4,
                                        PostingType=PostingTypeEnum.NTI,
                                        SpotAllocationModelMode=SpotAllocationModelMode.Quality,
                                        //CalculatedVpvh=0, // Commented as per requirement of BP-2513
                                        TotalBudget=19939,
                                        TotalImpressions=870597
                                    },
                                    new CurrentPricingExecutionResultDto
                                    {
                                        Id = 15,
                                       OptimalCpm=21,JobId=755,
                                        PlanVersionId=805,
                                        GoalFulfilledByProprietary=false,
                                        Notes="",
                                        HasResults=true,
                                        CpmPercentage=4,
                                        PostingType=PostingTypeEnum.NTI,
                                        SpotAllocationModelMode=SpotAllocationModelMode.Quality,
                                        //CalculatedVpvh=0, // Commented as per requirement of BP-2513
                                        TotalBudget=19939,
                                        TotalImpressions=870597
                                    }
                }
            };

            var service = _GetService();
            // Act
            var results = service.ValidatePricingExecutionResult(result, expectedResult);
            // Assert     
            Assert.AreEqual(results.IsPricingModelRunning, true);
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
        public void DidPricingJobCompleteWithinThreshold(BackgroundJobProcessingStatus status, int completedMinutesAgo, bool expectedResult)
        {
            var currentDateTime = new DateTime(2021, 5, 14, 12, 30, 0);
            var completedMinutes = currentDateTime.Minute - completedMinutesAgo;

            DateTime? completedWhen = null;
            if (status != BackgroundJobProcessingStatus.Processing &&
                status != BackgroundJobProcessingStatus.Queued)
            {
                completedWhen = new DateTime(2021, 5, 14, 12, completedMinutes, 0);
            }

            var job = new PlanPricingJob
            {
                Id = 1,
                Status = status,
                Completed = completedWhen
            };

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            var service = _GetService();

            var result = service._DidPricingJobCompleteWithinThreshold(job, thresholdMinutes: 5);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(3)]
        public void FillInMissingPricingResultsWithEmptyResults(int startingResultCount)
        {
            // Arrange
            var candidateResults = new List<CurrentPricingExecutionResultDto>();

            candidateResults.Add(new CurrentPricingExecutionResultDto
            {
                PostingType = PostingTypeEnum.NTI,
                SpotAllocationModelMode = SpotAllocationModelMode.Quality,
                OptimalCpm = 23
            });

            if (startingResultCount > 1)
            {
                candidateResults.Add(new CurrentPricingExecutionResultDto
                {
                    PostingType = PostingTypeEnum.NSI,
                    SpotAllocationModelMode = SpotAllocationModelMode.Quality,
                    OptimalCpm = 23
                });
            }

            if (startingResultCount == 6)
            {
                candidateResults.Add(new CurrentPricingExecutionResultDto
                {
                    PostingType = PostingTypeEnum.NSI,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    OptimalCpm = 23
                });

                candidateResults.Add(new CurrentPricingExecutionResultDto
                {
                    PostingType = PostingTypeEnum.NTI,
                    SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                    OptimalCpm = 23
                });

                candidateResults.Add(new CurrentPricingExecutionResultDto
                {
                    PostingType = PostingTypeEnum.NSI,
                    SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                    OptimalCpm = 23
                });

                candidateResults.Add(new CurrentPricingExecutionResultDto
                {
                    PostingType = PostingTypeEnum.NTI,
                    SpotAllocationModelMode = SpotAllocationModelMode.Floor,
                    OptimalCpm = 23
                });
            }

            var service = _GetService();

            // Act
            var results = service.FillInMissingPricingResultsWithEmptyResults(candidateResults);

            // Assert

            Assert.IsTrue(results.Any(a => a.PostingType == PostingTypeEnum.NSI && a.SpotAllocationModelMode == SpotAllocationModelMode.Quality));
            Assert.IsTrue(results.Any(a => a.PostingType == PostingTypeEnum.NTI && a.SpotAllocationModelMode == SpotAllocationModelMode.Quality));

            Assert.IsTrue(results.Any(a => a.PostingType == PostingTypeEnum.NSI && a.SpotAllocationModelMode == SpotAllocationModelMode.Efficiency));
            Assert.IsTrue(results.Any(a => a.PostingType == PostingTypeEnum.NTI && a.SpotAllocationModelMode == SpotAllocationModelMode.Efficiency));
            Assert.IsTrue(results.Any(a => a.PostingType == PostingTypeEnum.NSI && a.SpotAllocationModelMode == SpotAllocationModelMode.Floor));
            Assert.IsTrue(results.Any(a => a.PostingType == PostingTypeEnum.NTI && a.SpotAllocationModelMode == SpotAllocationModelMode.Floor));
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
                    }
                };
        }

        private List<PlanPricingInventoryProgram> _GetTieredInventory()
        {
            return new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
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
                        ProjectedImpressions = 1000,
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
                            Id = 1,
                            InventoryType = InventorySourceTypeEnum.OpenMarket,
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
                        PostingType = PostingTypeEnum.NSI,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 5,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProjectedImpressions = 1100,
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
                            Id = 1,
                            InventoryType = InventorySourceTypeEnum.OpenMarket
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
                                ContractMediaWeekId = 103,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
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
                            Id = 1,
                            InventoryType = InventorySourceTypeEnum.OpenMarket
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
                                ContractMediaWeekId = 104,
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 105,
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 106,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
                    {
                        ManifestId = 4,
                        StandardDaypartId = 15,
                        PostingType = PostingTypeEnum.NSI,
                        Station = new DisplayBroadcastStation
                        {
                            Id = 6,
                            LegacyCallLetters = "wnbc",
                            MarketCode = 101,
                        },
                        ProvidedImpressions = 1200,
                        ProjectedImpressions = 1200,
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
                            Id = 1,
                            InventoryType = InventorySourceTypeEnum.OpenMarket
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

        private List<PlanPricingInventoryProgram> _GetMultipleInventoryPrograms()
        {
            return new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
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
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
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
                    new PlanPricingInventoryProgram
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
                };
        }

        private List<PlanPricingInventoryProgram> _GetMultipleInventoryPrograms_v3()
        {
            return new List<PlanPricingInventoryProgram>
                {
                    new PlanPricingInventoryProgram
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
                                ContractMediaWeekId = 735,
                                InventoryMediaWeekId = 735
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 736,
                                InventoryMediaWeekId = 736
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 737,
                                InventoryMediaWeekId = 737
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
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
                                ContractMediaWeekId = 100,
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
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
                                ContractMediaWeekId = 735,
                                InventoryMediaWeekId = 735
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 736,
                                InventoryMediaWeekId = 736
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 737,
                                InventoryMediaWeekId = 737
                            }
                        }
                    },
                    new PlanPricingInventoryProgram
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
                                Spots = 1,
                                ContractMediaWeekId = 735,
                                InventoryMediaWeekId = 735
                            },
                            new ManifestWeek
                            {
                                Spots = 2,
                                ContractMediaWeekId = 736,
                                InventoryMediaWeekId = 736
                            },
                            new ManifestWeek
                            {
                                Spots = 1,
                                ContractMediaWeekId = 737,
                                InventoryMediaWeekId = 737
                            }
                        }
                    }
                };
        }

        [Test]
        public void GetCalculatedDaypartVPVG()
        {
            int PlanVersionPricingResultId = 2632;
            _PlanRepositoryMock
               .Setup(x => x.GetPricingResultsDayparts(It.IsAny<int>()))
               .Returns(new List<PlanPricingResultsDaypartDto>
               {
                    new PlanPricingResultsDaypartDto
                    {
                    Id = 1,
                    PlanVersionPricingResultId = 1,
                    CalculatedVpvh = 4,
                    StandardDaypartId = 1
                    },
                    new PlanPricingResultsDaypartDto
                    {
                    Id = 1,
                    PlanVersionPricingResultId = 1,
                    CalculatedVpvh = 5,
                    StandardDaypartId = 1
                    },
                     new PlanPricingResultsDaypartDto
                    {
                    Id = 1,
                    PlanVersionPricingResultId = 1,
                    CalculatedVpvh = 3,
                    StandardDaypartId = 1
                    }
                });
            var service = _GetService();
            // Act
            var avgVPVh = service.GetCalculatedDaypartVPVH(PlanVersionPricingResultId);
            // Assert     
            Assert.AreEqual(avgVPVh, 4);
        }

        private PlanDto _GetPlan()
        {
            return new PlanDto
            {
                Id = 1197,
                CoverageGoalPercent = 80,
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                PostingType = PostingTypeEnum.NSI,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                    {
                        new PlanAvailableMarketDto
                        {
                            Id = 5,
                            MarketCode = 101,
                            ShareOfVoicePercent = 12,
                            IsUserShareOfVoicePercent = true
                        },
                        new PlanAvailableMarketDto
                        {
                            Id = 6,
                            MarketCode = 102,
                            ShareOfVoicePercent = 13,
                            IsUserShareOfVoicePercent = true
                        }
                    },
                AvailableMarketsSovTotal = 25,
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
                PricingParameters = _GetPlanPricingParametersDto(),
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
                ProductId = 1,
                ProductMasterId = new Guid("AD71646F-2736-45DA-BF65-71120DA01221"),
                ImpressionsPerUnit = 1
            };
        }

        private PlanDto _GetPlanTiered()
        {
            return new PlanDto
            {
                CoverageGoalPercent = 80,
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                PostingType = PostingTypeEnum.NSI,
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
                PricingParameters = _GetPlanPricingParametersDto(),
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
                            WeeklyImpressions = 150,
                            WeeklyBudget = 15m,
                            MediaWeekId = 102
                        },
                        new WeeklyBreakdownWeek
                        {
                            WeeklyImpressions = 200,
                            WeeklyBudget = 15m,
                            MediaWeekId = 103
                        }
                    },
                ImpressionsPerUnit = 1
            };
        }

        [Test]      
        [TestCase(true, false, false, 1)]
        [TestCase(true, true, false, 2)]
        [TestCase(true, true, true, 3)]
        public void GetSupportedInventorySourceTypes(bool isPricingModelOpenMarketInventoryEnabled, bool isPricingModelBarterInventoryEnabled, bool isPricingModelProprietaryOAndOInventoryEnabled, int expectedResult)
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
        public async Task RunPricingJobAsync_Verify_FilteredDaypart()
        {
            // Arrange
            const int jobId = 1;
            var parameters = _GetPlanPricingParametersDto();
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

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob());

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);

            var service = _GetService(false, false);

            // Act
            await service.RunPricingJobAsync(parameters, jobId, CancellationToken.None);

            // Assert
            Assert.AreEqual(expectedResult, plan.Dayparts.Count);
        }

        [Test]
        [TestCase(SpotAllocationModelMode.Quality, "PlanPricing_Test Plan Name_20201030_121523_Q", PostingTypeEnum.NSI)]
        [TestCase(SpotAllocationModelMode.Efficiency, "PlanPricing_Test Plan Name_20201030_121523_E", PostingTypeEnum.NSI)]
        [TestCase(SpotAllocationModelMode.Floor, "PlanPricing_Test Plan Name_20201030_121523_F", PostingTypeEnum.NSI)]
        public void ExportPlanPricingScx(SpotAllocationModelMode spotAllocationModelMode, string expectedFileName, PostingTypeEnum postingType)
        {
            const string username = "testUser";
            const string planName = "Test Plan Name";
            const string testStreamContent = "<xml>TestContent<xml/>";
            int planId = 21;

            var currentDateTime = new DateTime(2020, 10, 30, 12, 15, 23);
            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);

            _PlanPricingScxDataPrepMock.Setup(s => s.GetScxData(It.IsAny<int>(), It.IsAny<DateTime>(),
                It.IsAny<SpotAllocationModelMode>(),
                It.IsAny<PostingTypeEnum>()))
                .Returns<int, DateTime, SpotAllocationModelMode, PostingTypeEnum>((a, b, c, d) => new PlanScxData { PlanName = planName, Generated = b });

            _PlanPricingScxDataConverterMock.Setup(s => s.ConvertData(It.IsAny<PlanScxData>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns<PlanScxData, SpotAllocationModelMode>((d, e) => new PlanPricingScxFile
                {
                    PlanName = d.PlanName,

                    GeneratedTimeStamp = d.Generated,
                    ScxStream = new MemoryStream(Encoding.UTF8.GetBytes(testStreamContent))
                });

            var savedSharedFiles = new List<SharedFolderFile>();
            var testGuid = Guid.NewGuid();
            _SharedFolderServiceMock.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Callback<SharedFolderFile>((f) => savedSharedFiles.Add(f))
                .Returns(testGuid);

            var service = _GetService();

            // Act
            var savedFileGuid = service.ExportPlanPricingScx(planId, username, spotAllocationModelMode, postingType);

            //// Assert           
            var savedSharedFile = savedSharedFiles[0];
            Assert.IsTrue(savedSharedFile.FolderPath.EndsWith(@"\PlanPricingScx"));

            Assert.AreEqual(1, savedSharedFiles.Count);
            Assert.AreEqual(expectedFileName, savedSharedFile.FileName);
        }

        [Test]
        public void GetQuoteReportData_WithoutInventory()
        {
            // Arrange
            var request = _GetQuoteRequest();
            var currentDateTime = new DateTime(2022,02,25,15,20,12);
            var programs = new List<QuoteProgram>();
            
            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(currentDateTime);
            _AudienceServiceMock.Setup(s => s.GetAudiences())
                .Returns(AudienceTestData.GetAudiences());
            _MarketCoverageRepositoryMock.Setup(s => s.GetMarketsWithLatestCoverage())
                .Returns(MarketsTestData.GetMarketsWithLatestCoverage());

            _PlanPricingInventoryEngineMock.Setup(s => s.GetInventoryForQuote(It.IsAny<QuoteRequestDto>(), It.IsAny<Guid>()))
                .Returns(programs);

            var service = _GetService(true, true);

            // Act
            var caught = Assert.Throws<CadentException>(() => service.GetQuoteReportData(request));

            // Assert
            const string expected_message = "There is no inventory available for the selected program restrictions in the requested quarter. " +
                "Previous quarters might have available inventory that will be utilized when pricing is executed.";
            Assert.AreEqual(expected_message, caught.Message);
        }

        private static QuoteRequestDto _GetQuoteRequest()
        {
            var request = new QuoteRequestDto
            {
                FlightStartDate = new DateTime(2018, 12, 17),
                FlightEndDate = new DateTime(2018, 12, 23),
                FlightHiatusDays = new List<DateTime>(),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 100 } },
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

        [Test]
        public void GetPricingResultsReportData_WithEfficiencyAsSpotAllocationModelMode()
        {
            // Arrange
            const int planId = 1;
            var planVersionNumber = 2;

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    VersionNumber = planVersionNumber,
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanVersionId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
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
            service.GetPricingResultsReportData(planId, SpotAllocationModelMode.Efficiency);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
        }

        [Test]
        public void GetPricingResultsReportData_WithFloorAsSpotAllocationModelMode()
        {
            // Arrange
            const int planId = 1;
            var planVersionNumber = 2;

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    VersionNumber = planVersionNumber,
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>()
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanVersionId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
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
            service.GetPricingResultsReportData(planId, SpotAllocationModelMode.Floor);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(planId, null), Times.Once);
        }

        [Test]
        [TestCase(SpotAllocationModelMode.Efficiency)]
        public void _GetPricingModelWeeks_v3Test_WithEfficency_ReturnsCpm(SpotAllocationModelMode allocationMode)
        {
            //arrange
            var plan = PlanTestDataHelper.GetPlanForAllocationModelRunMultiSpot();

            var pricingParameters = new PlanPricingParametersDto
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
                PostingType = PostingTypeEnum.NSI
            };

            plan.PricingParameters = pricingParameters;
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
            var result = service._GetPricingModelWeeks_v3(plan, pricingParameters, proprietaryData,
                out skippedWeekIds, allocationMode);

            //assert
            Assert.IsTrue(result.All(x => x.CpmGoal != 1));
        }
    }
}
