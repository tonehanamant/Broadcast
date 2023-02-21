using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using IsciPlanMappingDto = Services.Broadcast.Entities.Isci.IsciPlanMappingDto;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Plans
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PlanIsciServiceUnitTests
    {
        private PlanIsciService _PlanIsciService;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IPlanIsciRepository> _PlanIsciRepositoryMock;
        private Mock<IStandardDaypartRepository> _StandardDaypartRepositoryMock;
        private Mock<IAabEngine> _AabEngineMock;
        private AgencyAdvertiserBrandApiClientStub _AgencyAdvertiserBrandApiClientStub;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private Mock<IPlanService> _PlanService;
        private Mock<ICampaignService> _CampaignService;
        private Mock<IStandardDaypartService> _StandardDaypartService;
        private Mock<ISpotLengthEngine> _SpotLengthEngine;
        private Mock<IAudienceRepository> _AudienceRepository;
        private Mock<IReelIsciRepository> _ReelIsciRepository;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
        private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _PlanIsciRepositoryMock = new Mock<IPlanIsciRepository>();
            _SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();
            _StandardDaypartRepositoryMock = new Mock<IStandardDaypartRepository>();
            _AabEngineMock = new Mock<IAabEngine>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _AgencyAdvertiserBrandApiClientStub = new AgencyAdvertiserBrandApiClientStub();
            _PlanService = new Mock<IPlanService>();
            _StandardDaypartService = new Mock<IStandardDaypartService>();
            _SpotLengthEngine = new Mock<ISpotLengthEngine>();
            _AudienceRepository = new Mock<IAudienceRepository>();
            _ReelIsciRepository = new Mock<IReelIsciRepository>();
            _CampaignService = new Mock<ICampaignService>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanIsciRepository>())
                .Returns(_PlanIsciRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IAudienceRepository>())
                .Returns(_AudienceRepository.Object);
            _DataRepositoryFactoryMock
              .Setup(x => x.GetDataRepository<IStandardDaypartRepository>())
              .Returns(_StandardDaypartRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IReelIsciRepository>())
                .Returns(_ReelIsciRepository.Object);

            _DataRepositoryFactoryMock
              .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
              .Returns(_SpotLengthRepositoryMock.Object);

            _SpotLengthEngine.Setup(s => s.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns<int>(SpotLengthTestData.GetSpotLengthValueById);

            _AudienceRepository.Setup(s => s.GetAudiencesByIds(It.IsAny<List<int>>()))
                .Returns<List<int>>(AudienceTestData.GetAudiencesByIds);

            _StandardDaypartService.Setup(s => s.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData);

            _LaunchDarklyClientStub = new LaunchDarklyClientStub();

            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);


            _PlanIsciService = new PlanIsciService(
                _DataRepositoryFactoryMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _PlanService.Object,
                _CampaignService.Object,
                _StandardDaypartService.Object,
                _SpotLengthEngine.Object,
                _DateTimeEngineMock.Object,
                _AabEngineMock.Object,
                featureToggleHelper,
                _ConfigurationSettingsHelperMock.Object);
        }

        [Test]
        public void GetMediaMonth()
        {
            // Arrange
            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2021, 01, 01));

            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthsBetweenDatesInclusive(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns<DateTime, DateTime>(MediaMonthAndWeekTestData.GetMediaMonthsIntersecting);

            // Act
            var result = _PlanIsciService.GetMediaMonths();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIscis_DatesPerToggle()
        {
            // Arrange
            IsciSearchDto isciSearch = new IsciSearchDto
            {
                MediaMonth = new MediaMonthDto { Id = 479, Month = 5, Year = 2021 },
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = false
            };

            var passedDateRange = new DateRange();
            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((start, end) => passedDateRange = new DateRange(start, end))
                .Returns(_GetAvailableIscis());

            // Act
            _PlanIsciService.GetAvailableIscis(isciSearch);

            // Assert
            const string DateFormat = "yyyy-MM-dd";
            var expectedStartDateTime = "2021-11-01";
            var expectedendDateTime = "2021-11-07";

            Assert.AreEqual(expectedStartDateTime, passedDateRange.Start.Value.ToString(DateFormat));
            Assert.AreEqual(expectedendDateTime, passedDateRange.End.Value.ToString(DateFormat));
            _PlanIsciRepositoryMock.Verify(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public void GetAvailableIsciPlans_DatesPerToggle()
        {
            // Arrange
            var isciPlanSearch = new IsciSearchDto
            {
                MediaMonth = new MediaMonthDto { Id = 479, Month = 5, Year = 2021 },
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = false
            };

            var passedDateRange = new DateRange();
            _PlanIsciRepositoryMock
                .Setup(s => s.GetAvailableIsciPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((start, end) => passedDateRange = new DateRange(start, end))
                .Returns(new List<IsciPlanDetailDto>());

            // Act
            var result = _PlanIsciService.GetAvailableIsciPlans(isciPlanSearch);

            // Assert
            const string DateFormat = "yyyy-MM-dd";
            var expectedStartDateTime = "2021-11-01";
            var expectedendDateTime = "2021-11-07";

            Assert.AreEqual(expectedStartDateTime, passedDateRange.Start.Value.ToString(DateFormat));
            Assert.AreEqual(expectedendDateTime, passedDateRange.End.Value.ToString(DateFormat));
            _PlanIsciRepositoryMock.Verify(x => x.GetAvailableIsciPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }


        [Test]
        public void GetAvailableIscis()
        {
            // Arrange
            IsciSearchDto isciSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = false
            };

            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetAvailableIscis());

            // Act
            var result = _PlanIsciService.GetAvailableIscis(isciSearch);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIscis_WithoutPlansOnly()
        {
            // Arrange
            IsciSearchDto isciSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = true,
            };

            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetAvailableIscis());

            // Act
            var result = _PlanIsciService.GetAvailableIscis(isciSearch);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        [Test]
        public void GetUnAvailableIscis()
        {
            // Arrange
            List<IsciAdvertiserDto> availableIscis = null;
            IsciSearchDto isciSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = false,

            };
            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(availableIscis);

            // Act
            var result = _PlanIsciService.GetAvailableIscis(isciSearch);

            // Assert
            Approvals.Equals(result.Count, 0);
        }

        [Test]
        public void GetAvailableIscis_NullPlanIsci()
        {
            // Arrange           
            IsciSearchDto isciSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = true,

            };

            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(
                          new List<IsciAdvertiserDto>()
                            {
                                new IsciAdvertiserDto()
                                {
                                AdvertiserName = "O'Keeffes",
                                Id = 1,
                                SpotLengthDuration = 21,
                                Isci = "OKWF1701H",
                                PlanIsci=1
                                },
                                 new IsciAdvertiserDto()
                                {
                                AdvertiserName = "O'Keeffes1",
                                Id = 2,
                                SpotLengthDuration = 22,
                                Isci = "OKWL1702H",
                                PlanIsci=1
                             }
                 });

            // Act
            var result = _PlanIsciService.GetAvailableIscis(isciSearch);

            // Assert
            Assert.AreEqual(result.Count, 0);
        }
        [Test]
        public void GetUnAvailableIscisThrowsException()
        {
            // Arrange           
            IsciSearchDto isciSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = false,

            };

            _PlanIsciRepositoryMock
               .Setup(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<Exception>(() => _PlanIsciService.GetAvailableIscis(isciSearch));
            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }
        private List<IsciAdvertiserDto> _GetAvailableIscis()
        {
            return new List<IsciAdvertiserDto>()
            {
                new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes",
                Id = 1,
                SpotLengthDuration = 21,
                Isci = "OKWF1701H",
                PlanIsci=1
                },
                 new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes",
                Id = 2,
                SpotLengthDuration = 22,
                Isci = "OKWL1702H",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "ATLT0063000HU",
                Id = 3,
                SpotLengthDuration = 23,
                Isci = "OKWF1701H",
                PlanIsci=1
                },
                   new IsciAdvertiserDto()
                {
                AdvertiserName = "Invisaling (Adult)",
                Id = 4,
                SpotLengthDuration = 24,
                Isci = "CLDC6513000H",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Colgate EM",
                Id = 5,
                SpotLengthDuration = 25,
                Isci = "CUSA1813000H",
                PlanIsci=1
                },
               new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes6",
                Id = 6,
                SpotLengthDuration = 26,
                Isci = "OKWF1701H",
                PlanIsci=1
                },
               new IsciAdvertiserDto()
                {
                AdvertiserName = "USAA",
                Id = 7,
                SpotLengthDuration = 27,
                Isci = "OKWL1702H",
                PlanIsci=1
                },
               new IsciAdvertiserDto()
                {
                AdvertiserName = "Nature's Bounty",
                Id = 8,
                SpotLengthDuration = 28,
                Isci = "ATLT0063000HU",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Colgate EM9",
                Id = 9,
                SpotLengthDuration = 29,
                Isci = "CLDC6513000H",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Colgate EM10",
                Id = 10,
                SpotLengthDuration = 30,
                Isci = "CUSA1813000H",
                PlanIsci=0
                }
            };
        }

        [Test]
        public void GetAvailableIsciPlans_Plan_DoesNotExist()
        {
            // Arrange
            var isciPlanSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = false
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetAvailableIsciPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<IsciPlanDetailDto>());

            // Act
            var result = _PlanIsciService.GetAvailableIsciPlans(isciPlanSearch);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetAvailableIsciPlans_PlansWithIsci_Exist()
        {
            // Arrange
            var isciPlanSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2022, 08, 01),
                WeekEndDate = new DateTime(2022, 08, 07),
                UnmappedOnly = false
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetAvailableIsciPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<IsciPlanDetailDto>()
                {
                    new IsciPlanDetailDto()
                    {
                        Id = 219,
                        Title = "Wellcare CBS Shows",
                        AdvertiserMasterId = new Guid("CFFFE6C6-0A33-44C5-8E12-FC1C0563591B"),
                        SpotLengthValues = new List<int>(){ 15, 30},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN", "PMN", "LN" },
                        FlightStartDate = new DateTime(2021,08,29),
                        FlightEndDate = new DateTime(2021, 08, 31),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            }
                        },
                        ProductMasterId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907"),
                        Iscis = new List<string>()
                        {
                            "OKWF1701H",
                            "OKWL1702H"
                        }
                    },
                    new IsciPlanDetailDto()
                    {
                        Id = 220,
                        Title = "Colgate Daytime Upfront",
                        AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                        SpotLengthValues = new List<int>(){ 15},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN" },
                        FlightStartDate = new DateTime(2021,08,20),
                        FlightEndDate = new DateTime(2021, 08, 28),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            }
                        },
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>()
                        {
                            "OKWF1703H",
                            "OKWL1704H"
                        }
                    },
                    new IsciPlanDetailDto()
                    {
                        Id = 221,
                        Title = "Colgate Early Morning Upfront",
                        AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                        SpotLengthValues = new List<int>(){ 30},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN" },
                        FlightStartDate = new DateTime(2021,07,15),
                        FlightEndDate = new DateTime(2021, 08, 22),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            }
                        },
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>()
                        {
                            "CLDC6513000H",
                            "CUSA1813000H"
                        }
                    }
                });
            _StandardDaypartRepositoryMock.Setup(x => x.GetDayIdsFromStandardDayparts(It.IsAny<List<int>>())).Returns(new List<int> { 1, 2 });

            _AabEngineMock
                .Setup(x => x.GetAdvertisers())
                .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertisers());

            _AabEngineMock
                .Setup(x => x.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()));

            // Act
            var result = _PlanIsciService.GetAvailableIsciPlans(isciPlanSearch);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIsciPlans_PlansWithoutIsci_Exist()
        {
            // Arrange
            var isciPlanSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2022, 08, 01),
                WeekEndDate = new DateTime(2022, 08, 07),
                UnmappedOnly = false
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetAvailableIsciPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<IsciPlanDetailDto>()
                {
                    new IsciPlanDetailDto()
                    {
                        Id = 219,
                        Title = "Wellcare CBS Shows",
                        AdvertiserMasterId = new Guid("CFFFE6C6-0A33-44C5-8E12-FC1C0563591B"),
                        SpotLengthValues = new List<int>(){ 15, 30},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN", "PMN", "LN" },
                        FlightStartDate = new DateTime(2021,08,29),
                        FlightEndDate = new DateTime(2021, 08, 31),
                        ProductMasterId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907"),
                        Iscis = new List<string>(),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            }
                        }
                    },
                    new IsciPlanDetailDto()
                    {
                        Id = 220,
                        Title = "Colgate Daytime Upfront",
                        AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                        SpotLengthValues = new List<int>(){ 15},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN" },
                        FlightStartDate = new DateTime(2021,08,20),
                        FlightEndDate = new DateTime(2021, 08, 28),
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>(),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            }
                        }
                    },
                    new IsciPlanDetailDto()
                    {
                        Id = 221,
                        Title = "Colgate Early Morning Upfront",
                        AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                        SpotLengthValues = new List<int>(){ 30},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN" },
                        FlightStartDate = new DateTime(2021,07,15),
                        FlightEndDate = new DateTime(2021, 08, 22),
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>(),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            }
                        }
                    }
                });
            _StandardDaypartRepositoryMock.Setup(x => x.GetDayIdsFromStandardDayparts(It.IsAny<List<int>>())).Returns(new List<int> { 1, 2 });
            _AabEngineMock
                .Setup(x => x.GetAdvertisers())
                .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertisers());

            _AabEngineMock
    .Setup(x => x.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()))
    .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()));

            // Act
            var result = _PlanIsciService.GetAvailableIsciPlans(isciPlanSearch);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIsciPlans_PlansWithoutIsci_UnmappedOnlyIsTrue()
        {
            // Arrange
            var isciPlanSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2022, 08, 01),
                WeekEndDate = new DateTime(2022, 08, 07),
                UnmappedOnly = true
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetAvailableIsciPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<IsciPlanDetailDto>()
                {
                    new IsciPlanDetailDto()
                    {
                        Id = 219,
                        Title = "Wellcare CBS Shows",
                        AdvertiserMasterId = new Guid("CFFFE6C6-0A33-44C5-8E12-FC1C0563591B"),
                        SpotLengthValues = new List<int>(){ 15, 30},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN", "PMN", "LN" },
                        FlightStartDate = new DateTime(2021,08,29),
                        FlightEndDate = new DateTime(2021, 08, 31),
                        ProductMasterId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907"),
                        Iscis = new List<string>(),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            }
                        }
                    },
                    new IsciPlanDetailDto()
                    {
                        Id = 220,
                        Title = "Colgate Daytime Upfront",
                        AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                        SpotLengthValues = new List<int>(){ 15},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN" },
                        FlightStartDate = new DateTime(2021,08,20),
                        FlightEndDate = new DateTime(2021, 08, 28),
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>(),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            }
                        }
                    },
                    new IsciPlanDetailDto()
                    {
                        Id = 221,
                        Title = "Colgate Early Morning Upfront",
                        AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                        SpotLengthValues = new List<int>(){ 30},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN" },
                        FlightStartDate = new DateTime(2021,07,15),
                        FlightEndDate = new DateTime(2021, 08, 22),
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>()
                        {
                            "CLDC6513000H",
                            "CUSA1813000H"
                        },
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2
                            }
                        }
                    }
                });
            _StandardDaypartRepositoryMock.Setup(x => x.GetDayIdsFromStandardDayparts(It.IsAny<List<int>>())).Returns(new List<int> { 1, 2 });
            _AabEngineMock
                .Setup(x => x.GetAdvertisers())
                .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertisers());

            _AabEngineMock
    .Setup(x => x.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()))
    .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()));

            // Act
            var result = _PlanIsciService.GetAvailableIsciPlans(isciPlanSearch);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }


        [Test]
        public void GetAvailableIsciPlans_FilterPlanNotInFlightWeek()
        {
            // Arrange
            var isciPlanSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2022, 08, 01),
                WeekEndDate = new DateTime(2022, 08, 07),
                UnmappedOnly = false
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetAvailableIsciPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<IsciPlanDetailDto>()
                {
                    new IsciPlanDetailDto()
                    {
                        Id = 219,
                        Title = "Wellcare CBS Shows",
                        AdvertiserMasterId = new Guid("CFFFE6C6-0A33-44C5-8E12-FC1C0563591B"),
                        SpotLengthValues = new List<int>(){ 15, 30},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN", "PMN", "LN" },
                        FlightStartDate = new DateTime(2022,06,27),
                        FlightEndDate = new DateTime(2022, 09, 25),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2022,08,1),
                            new DateTime(2022,08,2),
                            new DateTime(2022,08,3),
                            new DateTime(2022,08,4),
                            new DateTime(2022,08,5),
                            new DateTime(2022,08,6),
                            new DateTime(2022,08,7),
                            new DateTime(2022,08,8)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 12,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2,

                            }
                        },
                        ProductMasterId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907"),
                        Iscis = new List<string>()
                        {
                            "OKWF1701H",
                            "OKWL1702H"
                        }
                    },
                    new IsciPlanDetailDto()
                    {
                        Id = 220,
                        Title = "Colgate Daytime Upfront",
                        AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                        SpotLengthValues = new List<int>(){ 15},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN" },
                        FlightStartDate = new DateTime(2022,06,27),
                        FlightEndDate = new DateTime(2022, 09, 25),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2022,06,27),
                            new DateTime(2022,06,28)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2,

                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2,

                            }
                        },
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>()
                        {
                            "OKWF1703H",
                            "OKWL1704H"
                        }
                    },
                    new IsciPlanDetailDto()
                    {
                        Id = 221,
                        Title = "Colgate Early Morning Upfront",
                        AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                        SpotLengthValues = new List<int>(){ 30},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN" },
                        FlightStartDate = new DateTime(2022,06,27),
                        FlightEndDate = new DateTime(2022, 09, 25),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2022,07,20),
                            new DateTime(2022,07,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2,

                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2,

                            }
                        },
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>()
                        {
                            "CLDC6513000H",
                            "CUSA1813000H"
                        }
                    }
                });
            _StandardDaypartRepositoryMock.Setup(x => x.GetDayIdsFromStandardDayparts(It.IsAny<List<int>>())).Returns(new List<int> { 1, 2, });

            _AabEngineMock
                .Setup(x => x.GetAdvertisers())
                .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertisers());

            _AabEngineMock
                .Setup(x => x.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()));

            // Act
            var result = _PlanIsciService.GetAvailableIsciPlans(isciPlanSearch);

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetAvailableIsciPlans_ThrowsException()
        {
            // Arrange
            var isciPlanSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = false
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetAvailableIsciPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _PlanIsciService.GetAvailableIsciPlans(isciPlanSearch));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

        [Test]
        public void SaveIsciMappings_ModifyFlight()
        {
            var modifiedAt = DateTime.Now;
            var modifiedBy = "Test User";

            var modified = new List<IsciPlanModifiedMappingDto>
            {
                new IsciPlanModifiedMappingDto
                {
                    PlanIsciMappingId = 1,
                    FlightStartDate = new DateTime(2022,01,03),
                    FlightEndDate = new DateTime(2022,01,10)
                },
                new IsciPlanModifiedMappingDto
                {
                    PlanIsciMappingId = 1,
                    FlightStartDate = new DateTime(2022,01,03),
                    FlightEndDate = new DateTime(2022,01,16)
                },
                new IsciPlanModifiedMappingDto
                {
                    PlanIsciMappingId = 3,
                    FlightStartDate = new DateTime(2022,01,3),
                    FlightEndDate = new DateTime(2022,01,12)
                },
                new IsciPlanModifiedMappingDto
                {
                    PlanIsciMappingId = 3,
                    FlightStartDate = new DateTime(2022,01,03),
                    FlightEndDate = new DateTime(2022,01,09)
                }
            };

            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis())
                .Returns(new List<PlanIsciDto>
                {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,16)
                    },
                    new PlanIsciDto
                    {
                        Id = 2,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,16),
                        DeletedAt = new DateTime(2022,01,16),
                        DeletedBy = "Test User"
                    },
                    new PlanIsciDto
                    {
                        Id = 3,
                        PlanId = 2,
                        Isci = "myIsci2",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,13)
                    },
                    new PlanIsciDto
                    {
                        Id = 4,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,09)
                    }
                });

            _PlanIsciRepositoryMock
                .Setup(s => s.GetPlanIscisByMappingId(It.IsAny<int>()))
                .Returns(new List<PlanIsciDto>
                {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,16)
                    },
                    new PlanIsciDto
                    {
                        Id = 2,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,16),
                        DeletedAt = new DateTime(2022,01,16),
                        DeletedBy = "Test User"
                    },
                     new PlanIsciDto
                    {
                        Id = 4,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,09)
                    }
                });

            var saveRequest = new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappingsModified = modified
            };

            var isciPlanMappingsToUpdate = new List<PlanIsciDto>();
            _PlanIsciRepositoryMock.Setup(s => s.UpdateIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback<List<PlanIsciDto>, DateTime, string>((a, b, c) => isciPlanMappingsToUpdate = a)
                .Returns(2);

            //Act
            var result = _PlanIsciService.SaveIsciMappings(saveRequest, modifiedBy);

            //Assert
            Assert.IsTrue(result);
            _PlanIsciRepositoryMock.Verify(s => s.UpdateIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Exactly(1));
            Assert.AreEqual(12, isciPlanMappingsToUpdate.Count);
        }
        [Test]
        public void SaveIsciMappings_NewSimple()
        {
            var createdBy = "Test User";
            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci1",
                   SpotLengthId=1,
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03)

                           }
                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci2",
                   SpotLengthId=1,
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03)
                           }
                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci3",
                   SpotLengthId=1,
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03)
                           }
                   }
                },
            };

            _ReelIsciRepository.Setup(s => s.GetReelIscis(It.IsAny<List<string>>()))
                .Returns(new List<ReelIsciDto>
                {
                    new ReelIsciDto
                    {
                        Id = 1,
                        Isci = "MyIsci1",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,11,22),
                        ActiveEndDate = new DateTime(2021, 11, 28)
                    },
                    new ReelIsciDto
                    {
                        Id = 2,
                        Isci = "MyIsci2",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,10,15),
                        ActiveEndDate = new DateTime(2021, 11, 20)
                    },
                    new ReelIsciDto
                    {
                        Id = 3,
                        Isci = "MyIsci3",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,11,22),
                        ActiveEndDate = new DateTime(2021, 12, 15)
                    }
                });
            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns<int, int?>((a, b) => new PlanDto()
                {
                    Id = a,
                    FlightStartDate = new DateTime(2021, 11, 1),
                    FlightEndDate = new DateTime(2021, 11, 30)
                });
            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<List<int>>()))
                .Returns(new List<PlanIsciDto>());

            var saved = new List<PlanIsciDto>();
            _PlanIsciRepositoryMock.Setup(s =>
                    s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<List<PlanIsciDto>, string, DateTime>((a, b, c) => saved = a)
                .Returns(2);

            _PlanIsciRepositoryMock.Setup(s => s.GetDeletedPlanIscis(It.IsAny<List<int>>()))
                .Returns(new List<PlanIsciDto>());

            var saveRequest = new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappings = mappings
            };

            //Act
            var result = _PlanIsciService.SaveIsciMappings(saveRequest, createdBy);

            //Assert
            Assert.IsTrue(result);
            _PlanIsciRepositoryMock.Verify(s =>
                s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            _ReelIsciRepository.Verify(s => s.GetReelIscis(It.IsAny<List<string>>()), Times.Exactly(0));
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saved));
        }

        [Test]
        public void CopyIsciMappings_ModifyFlight()
        {
            var modifiedAt = DateTime.Now;
            var modifiedBy = "Test User";

            var modified = new List<IsciPlanModifiedMappingDto>
            {
                new IsciPlanModifiedMappingDto
                {
                    PlanIsciMappingId = 1,
                    FlightStartDate = new DateTime(2022,01,03),
                    FlightEndDate = new DateTime(2022,01,10)
                },
                new IsciPlanModifiedMappingDto
                {
                    PlanIsciMappingId = 1,
                    FlightStartDate = new DateTime(2022,01,03),
                    FlightEndDate = new DateTime(2022,01,16)
                },
                new IsciPlanModifiedMappingDto
                {
                    PlanIsciMappingId = 3,
                    FlightStartDate = new DateTime(2022,01,3),
                    FlightEndDate = new DateTime(2022,01,12)
                },
                new IsciPlanModifiedMappingDto
                {
                    PlanIsciMappingId = 3,
                    FlightStartDate = new DateTime(2022,01,03),
                    FlightEndDate = new DateTime(2022,01,09)
                }
            };

            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<int>()))
                .Returns(new List<PlanIsciDto>
                {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,16)
                    },
                    new PlanIsciDto
                    {
                        Id = 2,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,16),
                        DeletedAt = new DateTime(2022,01,16),
                        DeletedBy = "Test User"
                    },
                    new PlanIsciDto
                    {
                        Id = 3,
                        PlanId = 2,
                        Isci = "myIsci2",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,13)
                    },
                    new PlanIsciDto
                    {
                        Id = 4,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,09)
                    }
                });

            _PlanIsciRepositoryMock
                .Setup(s => s.GetPlanIscisByMappingId(It.IsAny<int>()))
                .Returns(new List<PlanIsciDto>
                {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,16)
                    },
                    new PlanIsciDto
                    {
                        Id = 2,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,16),
                        DeletedAt = new DateTime(2022,01,16),
                        DeletedBy = "Test User"
                    },
                     new PlanIsciDto
                    {
                        Id = 4,
                        PlanId = 2,
                        Isci = "myIsci1",
                        FlightStartDate = new DateTime(2022,01,03),
                        FlightEndDate = new DateTime(2022,01,09)
                    }
                });

            var saveRequest = new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappingsModified = modified
            };

            var isciPlanMappingsToUpdate = new List<PlanIsciDto>();
            _PlanIsciRepositoryMock.Setup(s => s.UpdateIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback<List<PlanIsciDto>, DateTime, string>((a, b, c) => isciPlanMappingsToUpdate = a)
                .Returns(2);

            //Act
            var result = _PlanIsciService.CopyIsciMappings(saveRequest, modifiedBy);

            //Assert
            Assert.IsTrue(result);
            _PlanIsciRepositoryMock.Verify(s => s.UpdateIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Exactly(1));
            Assert.AreEqual(12, isciPlanMappingsToUpdate.Count);
        }

        [Test]
        public void CopyIsciMappings_WithoutFlight()
        {
            var createdBy = "Test User";
            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci10",
                   SpotLengthId=1,
                   IsciPlanMappingFlights = new List<IsciPlanMappingFlightsDto>
                   {

                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci20",
                   SpotLengthId=1,
                   IsciPlanMappingFlights = new List<IsciPlanMappingFlightsDto>
                   {

                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci30",
                   SpotLengthId=1,
                   IsciPlanMappingFlights = new List<IsciPlanMappingFlightsDto>
                   {

                   }
                },
            };


            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns<int, int?>((a, b) => new PlanDto()
                {
                    Id = a,
                    FlightStartDate = DateTime.Now.AddDays(-10),
                    FlightEndDate = DateTime.Now.AddDays(30)
                });
            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<List<int>>()))
                .Returns(new List<PlanIsciDto>());

            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<int>())).
              Returns(new List<PlanIsciDto>
              {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 1,
                        SpotLengthId=1,
                        Isci = "MyIsci1",
                        FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                    },
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 1,
                        SpotLengthId=1,
                        Isci = "MyIsci2",
                        FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                    },
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 1,
                        SpotLengthId=1,
                        Isci = "MyIsci3",
                        FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                    }

              });


            var saved = new List<PlanIsciDto>();
            _PlanIsciRepositoryMock.Setup(s =>
                    s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<List<PlanIsciDto>, string, DateTime>((a, b, c) => saved = a)
                .Returns(2);

            _PlanIsciRepositoryMock.Setup(s => s.GetDeletedPlanIscis(It.IsAny<List<int>>()))
                .Returns(new List<PlanIsciDto>());

            var saveRequest = new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappings = mappings
            };

            //Act
            var result = _PlanIsciService.CopyIsciMappings(saveRequest, createdBy);

            //Assert
            Assert.IsTrue(result);
            _PlanIsciRepositoryMock.Verify(s =>
                s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            _ReelIsciRepository.Verify(s => s.GetReelIscis(It.IsAny<List<string>>()), Times.Exactly(0));
            foreach (var items in saved)
            {
                items.FlightStartDate = new DateTime(2022, 10, 28);
                items.FlightEndDate = new DateTime(2022, 11, 28);
            }
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saved));
        }

        [Test]
        public void CopyIsciMappings_PastDateFlight()
        {
            var createdBy = "Test User";
            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci10",
                   SpotLengthId=1,
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate = DateTime.Now.AddDays(-10),
                               FlightEndDate = DateTime.Now.AddDays(-1)
                           }
                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci20",
                   SpotLengthId=1,
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate = DateTime.Now.AddDays(-10),
                               FlightEndDate = DateTime.Now.AddDays(30)
                           }
                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci30",
                   SpotLengthId=1,
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                                FlightStartDate = DateTime.Now.AddDays(-10),
                               FlightEndDate = DateTime.Now.AddDays(-1)
                           }
                   }
                },
            };
            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<int>())).
            Returns(new List<PlanIsciDto>
            {
                    new PlanIsciDto
                    {
                         Id = 1,
                        PlanId = 1,
                        Isci = "MyIsci1",
                        FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                    },
                    new PlanIsciDto
                    {
                         Id = 1,
                        PlanId = 1,
                        Isci = "MyIsci2",
                        FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                    },
                    new PlanIsciDto
                    {
                         Id = 1,
                        PlanId = 1,
                        Isci = "MyIsci3",
                        FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                    }

            });
            _ReelIsciRepository.Setup(s => s.GetReelIscis(It.IsAny<List<string>>()))
                .Returns(new List<ReelIsciDto>
                {
                    new ReelIsciDto
                    {
                        Id = 1,
                        Isci = "MyIsci1",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,11,22),
                        ActiveEndDate = new DateTime(2021, 11, 28)
                    },
                    new ReelIsciDto
                    {
                        Id = 2,
                        Isci = "MyIsci2",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,10,15),
                        ActiveEndDate = new DateTime(2021, 11, 20)
                    },
                    new ReelIsciDto
                    {
                        Id = 3,
                        Isci = "MyIsci3",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,11,22),
                        ActiveEndDate = new DateTime(2021, 12, 15)
                    }
                });
            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns<int, int?>((a, b) => new PlanDto()
                {
                    Id = a,
                    FlightStartDate = new DateTime(2021, 11, 1),
                    FlightEndDate = new DateTime(2021, 11, 30)
                });
            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<List<int>>()))
                .Returns(new List<PlanIsciDto>());

            var saved = new List<PlanIsciDto>();
            _PlanIsciRepositoryMock.Setup(s =>
                    s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<List<PlanIsciDto>, string, DateTime>((a, b, c) => saved = a)
                .Returns(2);

            _PlanIsciRepositoryMock.Setup(s => s.GetDeletedPlanIscis(It.IsAny<List<int>>()))
                .Returns(new List<PlanIsciDto>());

            var saveRequest = new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappings = mappings
            };

            //Act
            var result = _PlanIsciService.CopyIsciMappings(saveRequest, createdBy);

            //Assert
            Assert.IsTrue(result);
            _PlanIsciRepositoryMock.Verify(s =>
                s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            _ReelIsciRepository.Verify(s => s.GetReelIscis(It.IsAny<List<string>>()), Times.Exactly(0));
            foreach (var items in saved)
            {
                items.FlightStartDate = new DateTime(2022, 10, 28);
                items.FlightEndDate = new DateTime(2022, 11, 28);
            }
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saved));

        }

        [Test]
        public void CopyIsciMappings_NewSimple()
        {
            var createdBy = "Test User";
            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci1",
                   SpotLengthId=1,
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)

                           }
                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci2",
                   SpotLengthId=1,
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                           }
                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci3",
                   SpotLengthId=1,
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                              FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                           }
                   }
                },
            };

            _ReelIsciRepository.Setup(s => s.GetReelIscis(It.IsAny<List<string>>()))
                .Returns(new List<ReelIsciDto>
                {
                    new ReelIsciDto
                    {
                        Id = 1,
                        Isci = "MyIsci1",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,11,22),
                        ActiveEndDate = new DateTime(2021, 11, 28)
                    },
                    new ReelIsciDto
                    {
                        Id = 2,
                        Isci = "MyIsci2",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,10,15),
                        ActiveEndDate = new DateTime(2021, 11, 20)
                    },
                    new ReelIsciDto
                    {
                        Id = 3,
                        Isci = "MyIsci3",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,11,22),
                        ActiveEndDate = new DateTime(2021, 12, 15)
                    }
                });
            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns<int, int?>((a, b) => new PlanDto()
                {
                    Id = a,
                    FlightStartDate = new DateTime(2021, 11, 1),
                    FlightEndDate = new DateTime(2021, 11, 30)
                });
            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<List<int>>()))
                .Returns(new List<PlanIsciDto>());
            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<int>())).
          Returns(new List<PlanIsciDto>
          {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 1,
                        SpotLengthId=1,
                        Isci = "MyIsci1",
                        FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                    },
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 1,
                        SpotLengthId=1,
                        Isci = "MyIsci2",
                        FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                    },
                    new PlanIsciDto
                    {
                        Id = 1,
                        SpotLengthId=1,
                        PlanId = 1,
                        Isci = "MyIsci3",
                        FlightStartDate = DateTime.Now.AddDays(-10),
                        FlightEndDate = DateTime.Now.AddDays(30)
                    }

          });
            var saved = new List<PlanIsciDto>();
            _PlanIsciRepositoryMock.Setup(s =>
                    s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<List<PlanIsciDto>, string, DateTime>((a, b, c) => saved = a)
                .Returns(2);

            _PlanIsciRepositoryMock.Setup(s => s.GetDeletedPlanIscis(It.IsAny<List<int>>()))
                .Returns(new List<PlanIsciDto>());

            var saveRequest = new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappings = mappings
            };

            //Act
            var result = _PlanIsciService.CopyIsciMappings(saveRequest, createdBy);

            //Assert
            Assert.IsTrue(result);
            _PlanIsciRepositoryMock.Verify(s =>
                s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            _ReelIsciRepository.Verify(s => s.GetReelIscis(It.IsAny<List<string>>()), Times.Exactly(0));
            foreach (var items in saved)
            {
                items.FlightStartDate = new DateTime(2022, 10, 28);
                items.FlightEndDate = new DateTime(2022, 11, 28);
            }
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saved));
        }
        [Test]
        public void GetPlanIsciMappingsDetails()
        {
            // Arrange
            const int planId = 23;
            const int campaignId = 32;


            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<int>()))
                .Returns(new List<PlanIsciDto>
                {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = planId,
                        Isci = "Isci123",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                        SpotLengthId= 1
                    },
                    new PlanIsciDto
                    {
                        Id = 2,
                        PlanId = planId,
                        Isci = "Isci456",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                        SpotLengthId = 2
                    },
                    new PlanIsciDto
                    {
                        Id = 3,
                        PlanId = planId,
                        Isci = "Isci789",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                        SpotLengthId=1
                    }
                });

            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    CampaignId = campaignId,
                    ProductMasterId = new Guid("A1CA207C-250E-4C4E-ACB0-A94200693344"),
                    Name = "TestPlanForGetPlanIsciMappingsDetails",
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto {DaypartCodeId = 1 },
                        new PlanDaypartDto {DaypartCodeId = 2 }
                    },
                    FlightStartDate = DateTime.Parse("11/1/2021"),
                    FlightEndDate = DateTime.Parse("11/10/2021"),
                    AudienceId = 13,
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength
                        {
                            SpotLengthId = 1,
                            Weight = 25
                        },
                        new CreativeLength
                        {
                            SpotLengthId = 3,
                            Weight = 75
                        }
                    }
                });

            _CampaignService.Setup(s => s.GetCampaignById(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    AdvertiserMasterId = new Guid("137B64C4-4887-4C8E-85E0-239F08609460")
                });

            _AabEngineMock.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(new AdvertiserDto() { Name = "Acme" });

            _AabEngineMock.Setup(s => s.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new ProductDto { Name = "KaBlam!!!" });

            // Act
            var result = _PlanIsciService.GetPlanIsciMappingsDetails(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase("11/1/2021", "11/10/2021", "11/01 - 11/10/2021")]
        [TestCase("11/1/2021", "11/10/2022", "11/01/2021 - 11/10/2022")]
        public void GetFlightString(string startDateString, string endDateString, string expectedFlightString)
        {
            // Arrange
            var startDate = DateTime.Parse(startDateString);
            var endDate = DateTime.Parse(endDateString);

            // Act
            var result = _PlanIsciService._GetFlightString(startDate, endDate);

            // Assert
            Assert.AreEqual(expectedFlightString, result);
        }

        [Test]
        public void GetPlanIsciMappingsDetails_DateFormat()
        {
            // Arrange
            const int planId = 23;
            const int campaignId = 32;


            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<int>()))
                .Returns(new List<PlanIsciDto>
                {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = planId,
                        Isci = "Isci123",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                        SpotLengthId= 1
                    },
                    new PlanIsciDto
                    {
                        Id = 2,
                        PlanId = planId,
                        Isci = "Isci456",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                        SpotLengthId = 2
                    },
                    new PlanIsciDto
                    {
                        Id = 3,
                        PlanId = planId,
                        Isci = "Isci789",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                        SpotLengthId=1
                    }
                });

            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    CampaignId = campaignId,
                    ProductMasterId = new Guid("A1CA207C-250E-4C4E-ACB0-A94200693344"),
                    Name = "TestPlanForGetPlanIsciMappingsDetails",
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto {DaypartCodeId = 1 },
                        new PlanDaypartDto {DaypartCodeId = 2 }
                    },
                    FlightStartDate = DateTime.Parse("11/1/2021"),
                    FlightEndDate = DateTime.Parse("11/10/2021"),
                    AudienceId = 13,
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength
                        {
                            SpotLengthId = 1,
                            Weight = 25
                        },
                        new CreativeLength
                        {
                            SpotLengthId = 3,
                            Weight = 75
                        }
                    }
                });

            _CampaignService.Setup(s => s.GetCampaignById(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    AdvertiserMasterId = new Guid("137B64C4-4887-4C8E-85E0-239F08609460")
                });

            _AabEngineMock.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(new AdvertiserDto() { Name = "Acme" });

            _AabEngineMock.Setup(s => s.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new ProductDto { Name = "KaBlam!!!" });

            // Act
            var result = _PlanIsciService.GetPlanIsciMappingsDetails(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableUnifiedIsciPlans_WithToggleOn()
        {
            // Arrange
            var isciPlanSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2022, 08, 01),
                WeekEndDate = new DateTime(2022, 08, 07),
                UnmappedOnly = true
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetAvailableIsciPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<IsciPlanDetailDto>()
                {
                    new IsciPlanDetailDto()
                    {
                        Id = 219,
                        Title = "Wellcare CBS Shows",
                        AdvertiserMasterId = new Guid("CFFFE6C6-0A33-44C5-8E12-FC1C0563591B"),
                        SpotLengthValues = new List<int>(){ 15, 30},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN", "PMN", "LN" },
                        FlightStartDate = new DateTime(2021,08,29),
                        FlightEndDate = new DateTime(2021, 08, 31),
                        ProductMasterId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907"),
                        Iscis = new List<string>(),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2,

                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2,

                            }
                        }
                    },
                    new IsciPlanDetailDto()
                    {
                        Id = 220,
                        Title = "Colgate Daytime Upfront",
                        AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                        SpotLengthValues = new List<int>(){ 15},
                        AudienceCode = "HH",
                        Dayparts = new List<string>(){ "EN" },
                        FlightStartDate = new DateTime(2021,08,20),
                        FlightEndDate = new DateTime(2021, 08, 28),
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>(),
                        UnifiedTacticLineId = "4CDA98D1-2F34-4B72-A4AD-72A012817E3C",
                        UnifiedCampaignLastSentAt = new DateTime(2021,07,15),
                        UnifiedCampaignLastReceivedAt = new DateTime(2021,07,15),
                        FlightDays = new List<int?> { 1, 2, 3, 4, 5, 6, 7 },
                        FlightHiatusDays = new List<DateTime>
                        {
                            new DateTime(2019,1,20),
                            new DateTime(2019,4,15)
                        },
                          PlanDayparts = new List<PlanDaypartDto>
                        {
                              new PlanDaypartDto
                            {
                                DaypartCodeId = 2,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2,

                            },
                            new PlanDaypartDto
                            {
                                DaypartCodeId = 11,
                                StartTimeSeconds = 1500,
                                EndTimeSeconds = 2788,
                                WeightingGoalPercent = 33.2,

                            }
                        }
                    }
                });
            _StandardDaypartRepositoryMock.Setup(x => x.GetDayIdsFromStandardDayparts(It.IsAny<List<int>>())).Returns(new List<int> { 1, 2, });
            _AabEngineMock
                .Setup(x => x.GetAdvertisers())
                .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertisers());

            _AabEngineMock
                .Setup(x => x.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertiserProduct(It.IsAny<Guid>(), It.IsAny<Guid>()));
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_UNIFIED_CAMPAIGN] = true;
            // Act
            var result = _PlanIsciService.GetAvailableIsciPlans(isciPlanSearch);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetMappedIscis_Exist()
        {
            // Arrange
            const int planId = 23;
            const int campaignId = 32;
            int expectedCount = 2;

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());
            _PlanIsciRepositoryMock.Setup(s => s.GetMappedIscis(It.IsAny<Guid>()))
                .Returns(new List<SearchPlan>
                {
                    new SearchPlan
                    {
                        Isci = "test12",
                        SpotLengths =new List<IsciSearchSpotLengthDto>
                        {
                            new IsciSearchSpotLengthDto
                            {
                                Id=1,
                                Length="30"
                            }
                        }
                    },
                    new SearchPlan
                    {
                        Isci = "test123",
                       SpotLengths =new List<IsciSearchSpotLengthDto>
                        {
                            new IsciSearchSpotLengthDto
                            {
                                Id=2,
                                Length="45"
                            }
                        }
                    }
                });
            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    CampaignId = campaignId,
                    ProductMasterId = new Guid("A1CA207C-250E-4C4E-ACB0-A94200693344"),
                    Name = "TestPlanForGetPlanIsciMappingsDetails",
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto {DaypartCodeId = 1 },
                        new PlanDaypartDto {DaypartCodeId = 2 }
                    },
                    FlightStartDate = DateTime.Parse("11/1/2021"),
                    FlightEndDate = DateTime.Parse("11/10/2021"),
                    AudienceId = 13,
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength
                        {
                            SpotLengthId = 1,
                            Weight = 25
                        },
                        new CreativeLength
                        {
                            SpotLengthId = 3,
                            Weight = 75
                        }
                    }
                });
            _CampaignService.Setup(s => s.GetCampaignById(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    AdvertiserMasterId = new Guid("137B64C4-4887-4C8E-85E0-239F08609460")
                });
            // Act
            var result = _PlanIsciService.GetMappedIscis(planId);
            // Assert
            Assert.AreEqual(result.Iscis.Count, expectedCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        [Test]
        public void GetMappedIscis_DoNotExist()
        {
            // Arrange
            const int planId = 23;
            const int campaignId = 32;
            int expectedCount = 0;
            var searchRequest = new SearchIsciRequestDto()
            {
                SourcePlanId = 467,
                SearchText = "tes"
            };

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());
            _PlanIsciRepositoryMock.Setup(s => s.GetMappedIscis(It.IsAny<Guid>()))
                .Returns(new List<SearchPlan> { });
            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    CampaignId = campaignId,
                    ProductMasterId = new Guid("A1CA207C-250E-4C4E-ACB0-A94200693344"),
                    Name = "TestPlanForGetPlanIsciMappingsDetails",
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto {DaypartCodeId = 1 },
                        new PlanDaypartDto {DaypartCodeId = 2 }
                    },
                    FlightStartDate = DateTime.Parse("11/1/2021"),
                    FlightEndDate = DateTime.Parse("11/10/2021"),
                    AudienceId = 13,
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength
                        {
                            SpotLengthId = 1,
                            Weight = 25
                        },
                        new CreativeLength
                        {
                            SpotLengthId = 3,
                            Weight = 75
                        }
                    }
                });
            _CampaignService.Setup(s => s.GetCampaignById(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    AdvertiserMasterId = new Guid("137B64C4-4887-4C8E-85E0-239F08609460")
                });
            // Act
            var result = _PlanIsciService.GetMappedIscis(planId);
            // Assert
            Assert.AreEqual(result.Iscis.Count, expectedCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetTargetIsciPlans_Exist()
        {
            // Arrange
            const int planId = 23;
            const int campaignId = 32;
            DateTime dateTime = DateTime.Today;
            int expectedCount = 1;

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());
            _PlanIsciRepositoryMock.Setup(s => s.GetTargetIsciPlans(It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns(new List<plan>
                {
                    new plan
                    {
                        id = 1,
                        campaign_id = campaignId,
                        name = "Abc",
                        latest_version_id = 1,
                        plan_versions = new List<plan_versions>
                        {
                            new plan_versions
                            {
                                id = 1,
                                plan_id=1,
                                flight_end_date =new DateTime(2024,08,29),
                                flight_start_date = new DateTime(2021,08,29),
                                plan_version_dayparts = new List<plan_version_dayparts>
                                {
                                    new plan_version_dayparts
                                    {
                                        standard_dayparts = new standard_dayparts
                                        {
                                            code = "SYN"
                                        }
                                    }
                                },
                                plan_version_creative_lengths = new List<plan_version_creative_lengths>
                                {
                                    new plan_version_creative_lengths
                                    {
                                        spot_length_id=1

                                    },
                                    new plan_version_creative_lengths
                                    {
                                         spot_length_id=1
                                    }
                                },
                                audience = new audience {code = "test"},
                                plan = new plan
                                {
                                    name = "test123",
                                    plan_iscis = new List<plan_iscis>
                                    {
                                        new plan_iscis
                                        {
                                            id=1,
                                            plan_id = 1,
                                            spot_length_id = 3
                                        }
                                     }
                                },
                                is_draft = false
                            }
                        },
                        plan_iscis = new List<plan_iscis>
                        {
                            new plan_iscis
                            {
                                id=1,
                                plan_id = 1,
                                spot_length_id = 3
                            }
                        }
                    },
                });
            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    CampaignId = campaignId,
                    ProductMasterId = new Guid("A1CA207C-250E-4C4E-ACB0-A94200693344"),
                    Name = "TestPlanForGetPlanIsciMappingsDetails",
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto {DaypartCodeId = 1 },
                        new PlanDaypartDto {DaypartCodeId = 2 }
                    },
                    FlightStartDate = DateTime.Parse("11/1/2021"),
                    FlightEndDate = dateTime,
                    AudienceId = 13,
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength
                        {
                            SpotLengthId = 1,
                            Weight = 25
                        },
                        new CreativeLength
                        {
                            SpotLengthId = 3,
                            Weight = 75
                        }
                    },
                });
            _CampaignService.Setup(s => s.GetCampaignById(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    AdvertiserMasterId = new Guid("137B64C4-4887-4C8E-85E0-239F08609460")
                });
            // Act
            var result = _PlanIsciService.GetTargetIsciPlans(planId);
            // Assert
            Assert.AreEqual(result.Plans.Count, expectedCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetTargetIsciPlansPastDate_DoNotExist()
        {
            // Arrange
            const int planId = 23;
            const int campaignId = 32;
            DateTime dateTime = DateTime.Today;
            int expectedCount = 0;

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());
            _PlanIsciRepositoryMock.Setup(s => s.GetTargetIsciPlans(It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns(new List<plan>
                {
                    new plan
                    {
                        id = 1,
                        campaign_id = campaignId,
                        name = "Abc",
                        plan_versions = new List<plan_versions>
                        {
                            new plan_versions
                            {
                                id = 1,
                                plan_id=1,
                                flight_end_date = new DateTime(2021,10,19),
                                flight_start_date = new DateTime(2021,08,29),
                                plan_version_dayparts = new List<plan_version_dayparts>
                                {
                                    new plan_version_dayparts
                                    {
                                        standard_dayparts = new standard_dayparts
                                        {
                                            code = "SYN"
                                        }
                                    }
                                }
                            }
                        },
                        plan_iscis = new List<plan_iscis>
                        {
                            new plan_iscis
                            {
                                id=1,
                                plan_id = 1,
                                spot_length_id = 3
                            }
                        }
                    },
                });
            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Id = planId,
                    CampaignId = campaignId,
                    ProductMasterId = new Guid("A1CA207C-250E-4C4E-ACB0-A94200693344"),
                    Name = "TestPlanForGetPlanIsciMappingsDetails",
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto {DaypartCodeId = 1 },
                        new PlanDaypartDto {DaypartCodeId = 2 }
                    },
                    FlightStartDate = DateTime.Parse("11/1/2021"),
                    FlightEndDate = new DateTime(2022, 08, 29),
                    AudienceId = 13,
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength
                        {
                            SpotLengthId = 1,
                            Weight = 25
                        },
                        new CreativeLength
                        {
                            SpotLengthId = 3,
                            Weight = 75
                        }
                    },
                });
            _CampaignService.Setup(s => s.GetCampaignById(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    AdvertiserMasterId = new Guid("137B64C4-4887-4C8E-85E0-239F08609460")
                });
            // Act
            var result = _PlanIsciService.GetTargetIsciPlans(planId);
            // Assert
            Assert.AreEqual(result.Plans.Count, expectedCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void CopyIsciMappings_OverlappingTime()
        {
            var copyRequest = _GetIsciSaveRequest();
            string createdBy = "Test User";
            DateTime dateAt = new DateTime(2023, 01, 15);
            // Arrange
            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2021, 01, 01));
            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns<int, int?>((a, b) => new PlanDto()
                {
                    Id = 1,
                    FlightStartDate = new DateTime(2023, 01, 01),
                    FlightEndDate = new DateTime(2023, 04, 30)
                });
            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<int>()))
               .Returns(new List<PlanIsciDto>
               {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 2,
                        Isci = "NDEN0114000H",
                        FlightStartDate = new DateTime(2023,01,03),
                        FlightEndDate = new DateTime(2023,03,10),
                        StartTime=300,
                        EndTime=21600
                    }
               });
            // Act
            var result = _PlanIsciService._HandleCopyIsciPlanMapping(copyRequest, createdBy, dateAt);
            // Assert
            Assert.AreEqual(0, result);
        }
        [Test]
        public void CopyIsciMappings_NoOverlappingTime()
        {
            var copyRequest = _GetIsciSaveRequest();
            string createdBy = "Test User";
            DateTime dateAt = new DateTime(2023, 01, 15);
            // Arrange
            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2021, 01, 01));
            _PlanService.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns<int, int?>((a, b) => new PlanDto()
                {
                    Id = 1,
                    FlightStartDate = new DateTime(2023, 01, 01),
                    FlightEndDate = new DateTime(2023, 04, 30)
                });
            _PlanIsciRepositoryMock.Setup(s => s.GetPlanIscis(It.IsAny<int>()))
               .Returns(new List<PlanIsciDto>
               {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 2,
                        Isci = "NDEN0114000H",
                        SpotLengthId=1,
                        FlightStartDate = new DateTime(2023,03,10),
                        FlightEndDate = new DateTime(2023,03,26),
                        StartTime=300,
                        EndTime=19800
                    }
               });
            _PlanIsciRepositoryMock.Setup(s => s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
               .Returns(1);
            // Act
            var result = _PlanIsciService._HandleCopyIsciPlanMapping(copyRequest, createdBy, dateAt);
            // Assert
            Assert.AreEqual(1, result);
        }
        private List<IsciPlanMappingDto> _GetIsciSaveRequest()
        {
            List<IsciPlanMappingDto>  IsciPlanMappings = new List<IsciPlanMappingDto>
                {
                    new IsciPlanMappingDto
                    {
                        PlanId=727,
                        Isci ="NDEN0114000H",
                        SpotLengthId =1,
                        IsciPlanMappingFlights=new List<IsciPlanMappingFlightsDto>
                        {
                            new IsciPlanMappingFlightsDto
                            {
                            FlightStartDate=new DateTime(2023, 01, 15),
                            FlightEndDate=new DateTime(2023, 03, 10),
                            StartTime =4000,
                            EndTime=20100
                            }
                        }
                    }

                };
            return IsciPlanMappings;
        }
    }
}