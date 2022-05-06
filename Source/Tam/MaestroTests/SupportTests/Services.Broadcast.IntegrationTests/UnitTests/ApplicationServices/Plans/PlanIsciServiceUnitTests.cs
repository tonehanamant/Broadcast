using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
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
using System.Linq;
using Tam.Maestro.Data.Entities;
using Unity;
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

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _PlanIsciRepositoryMock = new Mock<IPlanIsciRepository>();
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
                .Setup(x => x.GetDataRepository<IReelIsciRepository>())
                .Returns(_ReelIsciRepository.Object);

            _SpotLengthEngine.Setup(s => s.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns<int>(SpotLengthTestData.GetSpotLengthValueById);

            _AudienceRepository.Setup(s => s.GetAudiencesByIds(It.IsAny<List<int>>()))
                .Returns<List<int>>(AudienceTestData.GetAudiencesByIds);

            _StandardDaypartService.Setup(s => s.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData);

            _LaunchDarklyClientStub = new LaunchDarklyClientStub();            
            _LaunchDarklyClientStub.FeatureToggles.Add(FeatureToggles.Enable_ISCI_Mapping_Flight_Select_and_Mapping, false);

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
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
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
                        ProductMasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"),
                        Iscis = new List<string>()
                        {
                            "CLDC6513000H",
                            "CUSA1813000H"
                        }
                    }
                });

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
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
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
                        Iscis = new List<string>()
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
                        Iscis = new List<string>()
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
                    }
                });

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
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
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
                        Iscis = new List<string>()
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
                        Iscis = new List<string>()
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
                        }
                    }
                });

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
                .Setup(s => s.GetPlanIsciDuplicates(It.IsAny<List<IsciPlanModifiedMappingDto>>()))
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
            _PlanIsciRepositoryMock.Verify(s => s.UpdateIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Exactly(2));
            Assert.AreEqual(2, isciPlanMappingsToUpdate.Count);
        }
        [Test]
        public void SaveIsciMappings_ModifyFlight_ToggleOn()
        {
            var modifiedAt = DateTime.Now;
            var modifiedBy = "Test User";

             _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.Enable_ISCI_Mapping_Flight_Select_and_Mapping] = true;
            var modified = new List<IsciPlanEditMappingDto>
            {
                new IsciPlanEditMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci1",
                   IsciPlanMappingFlights =new List<IsciPlanMappingEditFlightsDto>
                   {
                           new IsciPlanMappingEditFlightsDto
                           {
                               MappingId=1,
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03),
                               SpotLengthId=1
                           }
                   }
                },
                new IsciPlanEditMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci2",
                   IsciPlanMappingFlights =new List<IsciPlanMappingEditFlightsDto>
                   {
                           new IsciPlanMappingEditFlightsDto
                           {
                               MappingId=2,
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03),
                               SpotLengthId=1
                           }
                   }
                },
                new IsciPlanEditMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci3",
                   IsciPlanMappingFlights =new List<IsciPlanMappingEditFlightsDto>
                   {
                           new IsciPlanMappingEditFlightsDto
                           {
                               MappingId=null,
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03),
                               SpotLengthId=1
                           }
                   }
                },
                 new IsciPlanEditMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci4",
                   IsciPlanMappingFlights =new List<IsciPlanMappingEditFlightsDto>
                   {
                           new IsciPlanMappingEditFlightsDto
                           {
                               MappingId=0,
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03),
                               SpotLengthId=1
                           }
                   }
                },
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
                .Setup(s => s.GetPlanIsciDuplicates(It.IsAny<List<IsciPlanModifiedMappingDto>>()))
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
                IsciPlanMappingsEdited = modified
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
            Assert.AreEqual(2, isciPlanMappingsToUpdate.Count);
        }
        [Test]
        public void SaveIsciMappings_NewSimple()
        {
            var createdBy = "Test User";

            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci1"},
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci2"},
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci3"},
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
                .Returns<int, int?>((a,b) => new PlanDto()
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
                IsciPlanMappings  = mappings
            };

            //Act
            var result = _PlanIsciService.SaveIsciMappings(saveRequest, createdBy);

            //Assert
            Assert.IsTrue(result);
            _PlanIsciRepositoryMock.Verify(s =>
                s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            _ReelIsciRepository.Verify(s => s.GetReelIscis(It.IsAny<List<string>>()), Times.Exactly(2));
            _PlanIsciRepositoryMock.Verify(s => s.GetDeletedPlanIscis(It.IsAny<List<int>>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saved));
        }

        [Test]
        public void SaveIsciMappings_NewSimple_ToggleOn()
        {
            var createdBy = "Test User";
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.Enable_ISCI_Mapping_Flight_Select_and_Mapping] = true;
            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto 
                { 
                   PlanId = 1,
                   Isci = "MyIsci1",
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03),
                               SpotLengthId=1
                           }
                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci2",
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03),
                               SpotLengthId=1
                           }
                   }
                },
                new IsciPlanMappingDto
                {
                   PlanId = 1,
                   Isci = "MyIsci3",
                   IsciPlanMappingFlights =new List<IsciPlanMappingFlightsDto>
                   {
                           new IsciPlanMappingFlightsDto
                           {
                               FlightStartDate=new DateTime(2022,06,27),
                               FlightEndDate=new DateTime(2022,07,03),
                               SpotLengthId=1
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
        public void SaveIsciMappings_NewWithTwoReels()
        {
            var createdBy = "Test User";

            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci1"},
            };

            _ReelIsciRepository.Setup(s => s.GetReelIscis(It.IsAny<List<string>>()))
                .Returns(new List<ReelIsciDto>
                {
                    new ReelIsciDto
                    {
                        Id = 1,
                        Isci = "MyIsci1",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,10,15),
                        ActiveEndDate = new DateTime(2021, 11, 10)
                    },
                    new ReelIsciDto
                    {
                        Id = 2,
                        Isci = "MyIsci1",
                        SpotLengthId = 1,
                        ActiveStartDate = new DateTime(2021,11,22),
                        ActiveEndDate = new DateTime(2021, 11, 28)
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
            _PlanIsciRepositoryMock.Setup(s => s.GetDeletedPlanIscis(It.IsAny<List<int>>()))
                .Returns(new List<PlanIsciDto>());

            var saved = new List<PlanIsciDto>();
            _PlanIsciRepositoryMock.Setup(s =>
                    s.SaveIsciPlanMappings(It.IsAny<List<PlanIsciDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<List<PlanIsciDto>, string, DateTime>((a, b, c) => saved = a)
                .Returns(2);

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
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saved));
        }

        [Test]
        public void SaveIsciMappings_WithListDuplicates()
        {
            var createdBy = "Test User";

            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci1"},
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci2"},
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci2"},
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
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saved));
        }

        [Test]
        public void SaveIsciMappings_WithDbDuplicatesWithOverlap()
        {
            var createdBy = "Test User";

            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci1"},
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci2"},
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
                .Returns(new List<PlanIsciDto>
                {
                    new PlanIsciDto
                    {
                        Id = 1,
                        PlanId = 1,
                        Isci = "MyIsci2",
                        FlightStartDate = new DateTime(2021,11,1),
                        FlightEndDate = new DateTime(2021, 11, 20)
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
            var result = _PlanIsciService.SaveIsciMappings(saveRequest, createdBy);

            //Assert
            Assert.IsTrue(result);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saved));
        }

        [Test]
        public void GetAvailableIscis_WithoutDuplication()
        {
            // Arrange
            IsciSearchDto isciSearch = new IsciSearchDto
            {
                WeekStartDate = new DateTime(2021, 11, 01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = false,

            };
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns(
                new MediaMonth { Id = 479, StartDate = new DateTime(2021, 01, 01), EndDate = new DateTime(2024, 08, 08) }
                );

            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetAvailableIscis_withDuplication());

            // Act
            var result = _PlanIsciService.GetAvailableIscis(isciSearch);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<IsciAdvertiserDto> _GetAvailableIscis_withDuplication()
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
                Isci = "OKWF1701H",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Reckitt Benckiser",
                Id = 3,
                SpotLengthDuration = 23,
                Isci = "OKWF1701H",
                PlanIsci=1
                },
                   new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes",
                Id = 4,
                SpotLengthDuration = 24,
                Isci = "CLDC6513000H",
                PlanIsci=1
                }
            };
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
                    },
                    new PlanIsciDto
                    {
                        Id = 2,
                        PlanId = planId,
                        Isci = "Isci456",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                    },
                    new PlanIsciDto
                    {
                        Id = 3,
                        PlanId = planId,
                        Isci = "Isci789",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                    }
                });

            _PlanIsciRepositoryMock.Setup(s => s.GetIsciSpotLengths(It.IsAny<List<string>>()))
                .Returns<List<string>>((iscis) => iscis
                    .Select(i => new IsciSpotLengthDto {Isci = i, SpotLengthId = 1})
                    .ToList());

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
                .Returns(new ProductDto {Name = "KaBlam!!!" });

            // Act
            var result = _PlanIsciService.GetPlanIsciMappingsDetails(planId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void SaveIsciMappings_NewWithAnUnDelete()
        {
            var createdBy = "Test User";

            var mappings = new List<IsciPlanMappingDto>
            {
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci1"},
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci2"},
                new IsciPlanMappingDto { PlanId = 1, Isci = "MyIsci3"},
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
                .Returns(new List<PlanIsciDto>
                {
                    new PlanIsciDto
                    {
                        Id = 5,
                        PlanId = 1,
                        Isci = "MyIsci3",
                        FlightStartDate = new DateTime(2021,11,22),
                        FlightEndDate = new DateTime(2021, 11, 30)
                    }
                });

            var unDeletedIds = new List<int>();
            _PlanIsciRepositoryMock.Setup(s => s.UnDeleteIsciPlanMappings(It.IsAny<List<int>>()))
                .Callback<List<int>>((l) => unDeletedIds.AddRange(l))
                .Returns<List<int>>((l) => l.Count());

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
            _ReelIsciRepository.Verify(s => s.GetReelIscis(It.IsAny<List<string>>()), Times.Exactly(2));
            _PlanIsciRepositoryMock.Verify(s => s.GetDeletedPlanIscis(It.IsAny<List<int>>()), Times.Once);
            _PlanIsciRepositoryMock.Verify(s => s.UnDeleteIsciPlanMappings(It.IsAny<List<int>>()), Times.Once);

            var toVerify = new
            {
                unDeletedIds,
                saved
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
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
        public void GetPlanIsciMappingsDetails_WithToggleOn()
        {
            // Arrange
            const int planId = 23;
            const int campaignId = 32;
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.Enable_ISCI_Mapping_Flight_Select_and_Mapping] = true;

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
                    },
                    new PlanIsciDto
                    {
                        Id = 2,
                        PlanId = planId,
                        Isci = "Isci456",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                    },
                    new PlanIsciDto
                    {
                        Id = 3,
                        PlanId = planId,
                        Isci = "Isci789",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                    },
                    new PlanIsciDto
                    {
                        Id = 4,
                        PlanId = planId,
                        Isci = "Isci123",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                    },
                    new PlanIsciDto
                    {
                        Id = 5,
                        PlanId = planId,
                        Isci = "Isci123",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021"),
                    }

                });

            _PlanIsciRepositoryMock.Setup(s => s.GetIsciSpotLengths(It.IsAny<List<string>>()))
                .Returns<List<string>>((iscis) => iscis
                    .Select(i => new IsciSpotLengthDto { Isci = i, SpotLengthId = 1 })
                    .ToList());

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
    }
}