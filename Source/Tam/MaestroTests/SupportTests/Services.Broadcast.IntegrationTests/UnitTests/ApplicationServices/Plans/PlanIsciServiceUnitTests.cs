﻿using ApprovalTests;
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
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
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

            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaMonthById);

            _FeatureToggleMock.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PLAN_ISCI_BY_WEEK))
                .Returns(true);

            _PlanIsciService = new PlanIsciService(
                _DataRepositoryFactoryMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _PlanService.Object,
                _CampaignService.Object,
                _StandardDaypartService.Object,
                _SpotLengthEngine.Object,
                _DateTimeEngineMock.Object, 
                _AabEngineMock.Object,
                _FeatureToggleMock.Object, 
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
        [TestCase(true)]
        [TestCase(false)]
        public void GetAvailableIscis_DatesPerToggle(bool toggleEnabled)
        {
            // Arrange
            _FeatureToggleMock.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PLAN_ISCI_BY_WEEK))
                .Returns(toggleEnabled);

            IsciSearchDto isciSearch = new IsciSearchDto
            {
                MediaMonth = new MediaMonthDto { Id = 479, Month = 5, Year = 2021 },
                WeekStartDate = new DateTime(2021,11,01),
                WeekEndDate = new DateTime(2021, 11, 7),
                UnmappedOnly = false
            };

            var passedDateRange = new DateRange();
            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime,DateTime>((start,end) => passedDateRange = new DateRange(start, end))
                .Returns(_GetAvailableIscis());

            // Act
            _PlanIsciService.GetAvailableIscis(isciSearch);

            // Assert
            const string DateFormat = "yyyy-MM-dd";
            var expectedStartDateTime = "2021-11-01";
            var expectedendDateTime = "2021-11-07";

            if (!toggleEnabled)
            {
                expectedStartDateTime = "2021-07-26";
                expectedendDateTime = "2021-08-29";
            }
            Assert.AreEqual(expectedStartDateTime, passedDateRange.Start.Value.ToString(DateFormat));
            Assert.AreEqual(expectedendDateTime, passedDateRange.End.Value.ToString(DateFormat));
            _PlanIsciRepositoryMock.Verify(x => x.GetAvailableIscis(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetAvailableIsciPlans_DatesPerToggle(bool toggleEnabled)
        {
            // Arrange
            _FeatureToggleMock.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PLAN_ISCI_BY_WEEK))
                .Returns(toggleEnabled);
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

            if (!toggleEnabled)
            {
                expectedStartDateTime = "2021-07-26";
                expectedendDateTime = "2021-08-29";
            }
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
        public void GetAvailableIscisNullProduct()
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
                .Returns(
                          new List<IsciAdvertiserDto>()
                            {
                                new IsciAdvertiserDto()
                                {
                                AdvertiserName = "O'Keeffes",
                                Id = 1,
                                SpotLengthDuration = 21,
                                ProductName = null,
                                Isci = "OKWF1701H"
                                },
                                 new IsciAdvertiserDto()
                                {
                                AdvertiserName = "O'Keeffes1",
                                Id = 2,
                                SpotLengthDuration = 22,
                                ProductName = null,
                                Isci = "OKWL1702H"
                             }
                 });

            // Act
            var result = _PlanIsciService.GetAvailableIscis(isciSearch);

            // Assert
            Assert.IsNull(result[0].Iscis[0].ProductName);
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
                                ProductName = null,
                                Isci = "OKWF1701H",
                                PlanIsci=1
                                },
                                 new IsciAdvertiserDto()
                                {
                                AdvertiserName = "O'Keeffes1",
                                Id = 2,
                                SpotLengthDuration = 22,
                                ProductName = null,
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
                ProductName = "Product1",
                Isci = "OKWF1701H",
                PlanIsci=1
                },
                 new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes",
                Id = 2,
                SpotLengthDuration = 22,
                ProductName = "Product2",
                Isci = "OKWL1702H",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "ATLT0063000HU",
                Id = 3,
                SpotLengthDuration = 23,
                ProductName = "Product3",
                Isci = "OKWF1701H",
                PlanIsci=1
                },
                   new IsciAdvertiserDto()
                {
                AdvertiserName = "Invisaling (Adult)",
                Id = 4,
                SpotLengthDuration = 24,
                ProductName = "Product4",
                Isci = "CLDC6513000H",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Colgate EM",
                Id = 5,
                SpotLengthDuration = 25,
                ProductName = "Product5",
                Isci = "CUSA1813000H",
                PlanIsci=1
                },
               new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes6",
                Id = 6,
                SpotLengthDuration = 26,
                ProductName = "Product6",
                Isci = "OKWF1701H",
                PlanIsci=1
                },
               new IsciAdvertiserDto()
                {
                AdvertiserName = "USAA",
                Id = 7,
                SpotLengthDuration = 27,
                ProductName = "Product7",
                Isci = "OKWL1702H",
                PlanIsci=1
                },
               new IsciAdvertiserDto()
                {
                AdvertiserName = "Nature's Bounty",
                Id = 8,
                SpotLengthDuration = 28,
                ProductName = null,
                Isci = "ATLT0063000HU",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Colgate EM9",
                Id = 9,
                SpotLengthDuration = 29,
                ProductName = null,
                Isci = "CLDC6513000H",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Colgate EM10",
                Id = 10,
                SpotLengthDuration = 30,
                ProductName = null,
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
                        ProductName = "Sample - 2Q09",
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
                        ProductName = "1-800-Contacts",
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
                        ProductName = "1-800-Contacts",
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
                        ProductName = "Sample - 2Q09",
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
                        ProductName = "1-800-Contacts",
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
                        ProductName = "1-800-Contacts",
                        Iscis = new List<string>()
                    }
                });

            _AabEngineMock
                .Setup(x => x.GetAdvertisers())
                .Returns(_AgencyAdvertiserBrandApiClientStub.GetAdvertisers());

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
                        ProductName = "Sample - 2Q09",
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
                        ProductName = "1-800-Contacts",
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
                        ProductName = "1-800-Contacts",
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
        public void SaveIsciMappings_IsciProductMappings()
        {
            //Arrange
            var createdBy = "Test User";

            var isciProductList = new List<IsciProductMappingDto>
            {
                new IsciProductMappingDto()
                {
                    ProductName = "NewProduct1",
                    Isci = "NewIsci"
                },
                new IsciProductMappingDto()
                {
                    ProductName = "ListDuplicateProduct1",
                    Isci = "ListDuplicateIsci"
                },
                new IsciProductMappingDto()
                {
                    ProductName = "ListDuplicateProduct2",
                    Isci = "ListDuplicateIsci"
                },
                new IsciProductMappingDto()
                {
                    ProductName = "DbDupProduct1",
                    Isci = "DbDuplicateIsci"
                }
            };

            var getIsciProductMappingsCalls = new List<string>();
            _PlanIsciRepositoryMock.Setup(s => s.GetIsciProductMappings(It.IsAny<List<string>>()))
                .Callback<List<string>>((a) => getIsciProductMappingsCalls = a)
                .Returns(new List<IsciProductMappingDto>
                {
                    new IsciProductMappingDto()
                    {
                        ProductName = "DbDuplicateProduct2",
                        Isci = "DbDuplicateIsci"
                    }
                });

            var savedProductMappings = new List<IsciProductMappingDto>();
            _PlanIsciRepositoryMock.Setup(s =>
                    s.SaveIsciProductMappings(It.IsAny<List<IsciProductMappingDto>>(), It.IsAny<string>(),
                        It.IsAny<DateTime>()))
                .Callback<List<IsciProductMappingDto>, string, DateTime>((a, b, c) => savedProductMappings = a)
                .Returns(1);

            var saveRequest = new IsciPlanMappingsSaveRequestDto()
            {
                IsciProductMappings = isciProductList
            };

            //Act
            var result = _PlanIsciService.SaveIsciMappings(saveRequest, createdBy);

            //Assert
            Assert.IsTrue(result);

            _PlanIsciRepositoryMock.Verify(s =>
                s.GetIsciProductMappings(It.IsAny<List<string>>()), Times.Once);

            _PlanIsciRepositoryMock.Verify(s =>
                s.SaveIsciProductMappings(It.IsAny<List<IsciProductMappingDto>>(), It.IsAny<string>(),
                    It.IsAny<DateTime>()), Times.Once);

            var toVerify = new
            {
                getIsciProductMappingsCalls,
                savedProductMappings
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void SaveIsciMappings_Delete()
        {
            //Arrange
            var createdBy = "Test User";
            var toDelete = new List<int> {1, 2, 5};

            var saveRequest = new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappingsDeleted = toDelete
            };

            var deletedPlanIsciIds = new List<int>();
            _PlanIsciRepositoryMock.Setup(s =>
                    s.DeleteIsciPlanMappings(It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<List<int>, string, DateTime>((a, b, c) => deletedPlanIsciIds = a)
                .Returns(2);
            
            //Act
            var result = _PlanIsciService.SaveIsciMappings(saveRequest, createdBy);

            //Assert
            Assert.IsTrue(result);
            _PlanIsciRepositoryMock.Verify(s =>
                s.DeleteIsciPlanMappings(It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            Assert.AreEqual(toDelete, deletedPlanIsciIds);
        }

        [Test]
        public void SaveIsciMappings_ModifyFlight()
        {
            var createdBy = "Test User";
            var modified = new List<IsciPlanModifiedMappingDto>
            {
                new IsciPlanModifiedMappingDto{ PlanIsciMappingId = 1, FlightStartDate = new DateTime(2021,11,24), FlightEndDate = new DateTime(2021,12,15)},
                new IsciPlanModifiedMappingDto{ PlanIsciMappingId = 2, FlightStartDate = new DateTime(2021,11,26), FlightEndDate = new DateTime(2021,12,12)}
            };
            var saveRequest = new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappingsModified = modified
            };
            var saved = new List<IsciPlanModifiedMappingDto>();
            _PlanIsciRepositoryMock.Setup(s => s.UpdateIsciPlanMappings(It.IsAny<List<IsciPlanModifiedMappingDto>>()))
                .Callback<List<IsciPlanModifiedMappingDto>>((a) => saved = a)
                .Returns(2);

            //Act
            var result = _PlanIsciService.SaveIsciMappings(saveRequest, createdBy);

            //Assert
            Assert.IsTrue(result);
            _PlanIsciRepositoryMock.Verify(s => s.UpdateIsciPlanMappings(It.IsAny<List<IsciPlanModifiedMappingDto>>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saved));
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
            _ReelIsciRepository.Verify(s => s.GetReelIscis(It.IsAny<List<string>>()), Times.Once);
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
                ProductName = "Product1",
                Isci = "OKWF1701H",
                PlanIsci=1
                },
                 new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes",
                Id = 2,
                SpotLengthDuration = 22,
                ProductName = "Product1",
                Isci = "OKWF1701H",
                PlanIsci=1
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Reckitt Benckiser",
                Id = 3,
                SpotLengthDuration = 23,
                ProductName = "Product3",
                Isci = "OKWF1701H",
                PlanIsci=1
                },
                   new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes",
                Id = 4,
                SpotLengthDuration = 24,
                ProductName = "Product4",
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
                        FlightEndDate = DateTime.Parse("11/10/2021")
                    },
                    new PlanIsciDto
                    {
                        Id = 2,
                        PlanId = planId,
                        Isci = "Isci456",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021")
                    },
                    new PlanIsciDto
                    {
                        Id = 3,
                        PlanId = planId,
                        Isci = "Isci789",
                        FlightStartDate = DateTime.Parse("11/1/2021"),
                        FlightEndDate = DateTime.Parse("11/10/2021")
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
    }
}