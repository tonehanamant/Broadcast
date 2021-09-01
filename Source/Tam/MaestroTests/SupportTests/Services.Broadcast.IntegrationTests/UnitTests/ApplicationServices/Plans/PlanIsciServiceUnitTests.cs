using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Tam.Maestro.Data.Entities;

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

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanIsciRepository>())
                .Returns(_PlanIsciRepositoryMock.Object);

            _AgencyAdvertiserBrandApiClientStub = new AgencyAdvertiserBrandApiClientStub();

            _PlanIsciService = new PlanIsciService(_DataRepositoryFactoryMock.Object, _MediaMonthAndWeekAggregateCacheMock.Object, _DateTimeEngineMock.Object, _AabEngineMock.Object, _FeatureToggleMock.Object,_ConfigurationSettingsHelperMock.Object);
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
        public void GetAvailableIscis()
        {
            // Arrange
            IsciSearchDto isciSearch = new IsciSearchDto
            {
                MediaMonth = new MediaMonthDto { Id = 479, Month = 5, Year = 2021 },
                UnmappedOnly = false,

            };
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns(
                new MediaMonth { Id = 479, StartDate = new DateTime(2021, 01, 01), EndDate = new DateTime(2024, 08, 08) }
                );

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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 5, Year = 2021 },
                UnmappedOnly = true,

            };
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns(
                new MediaMonth { Id = 479, StartDate = new DateTime(2021, 01, 01), EndDate = new DateTime(2024, 08, 08) }
                );

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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 5, Year = 2021 },
                UnmappedOnly = false,

            };
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
              .Returns(
              new MediaMonth { Id = 479, StartDate = new DateTime(2021, 01, 01), EndDate = new DateTime(2024, 08, 08) }
              );
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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 5, Year = 2021 },
                UnmappedOnly = false,

            };
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
              .Returns(
              new MediaMonth { Id = 479, StartDate = new DateTime(2021, 01, 01), EndDate = new DateTime(2024, 08, 08) }
              );
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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 5, Year = 2021 },
                UnmappedOnly = true,

            };
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
              .Returns(
              new MediaMonth { Id = 479, StartDate = new DateTime(2021, 01, 01), EndDate = new DateTime(2024, 08, 08) }
              );
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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 5, Year = 2021 },
                UnmappedOnly = false,

            };
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
             .Returns(
             new MediaMonth { Id = 479, StartDate = new DateTime(2021, 01, 01), EndDate = new DateTime(2024, 08, 08) }
             );
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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 8, Year = 2021 },
                UnmappedOnly = false
            };

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaMonthById);

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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 8, Year = 2021 },
                UnmappedOnly = false
            };

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaMonthById);

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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 8, Year = 2021 },
                UnmappedOnly = false
            };

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaMonthById);

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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 8, Year = 2021 },
                UnmappedOnly = true
            };

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaMonthById);

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
                MediaMonth = new MediaMonthDto { Id = 479, Month = 8, Year = 2021 },
                UnmappedOnly = false
            };

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaMonthById);

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
        public void SaveIsciProductMappings()
        {
            //Arrange
            string createdBy = "Test User";
            DateTime createdAt = DateTime.Now;
            int result = 0;
            bool actualResult = true;
            bool expectedData = false;
            var isciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE67VR14"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7049,
                                Isci= "AE67VR14"
                            }
                        };
            var isciProductList = new List<IsciProductMappingDto>
                        {
                            new IsciProductMappingDto()
                            {
                                ProductName = "Femoston",
                                Isci= "AE67VR14"
                            },
                            new IsciProductMappingDto()
                            {
                                ProductName = "Femoston",
                                Isci= "AE67VR14"
                            }
                        };
            var isciPlanProductMapping = new IsciPlanProductMappingDto()
            {
                IsciPlanMappings = isciPlanList,
                IsciProductMappings = isciProductList
            };
            _PlanIsciRepositoryMock
                .Setup(s => s.GetPlanIscis())
                .Returns(isciPlanList);
            _PlanIsciRepositoryMock
               .Setup(s => s.SaveIsciPlanMappings(It.IsAny<List<IsciPlanMappingDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
               .Callback(() =>
               {
                   result = 2;
               })
               .Returns(result);

            _PlanIsciRepositoryMock.Setup(s => s.GetIsciProductMappings(It.IsAny<List<string>>()))
                .Returns(new List<IsciProductMappingDto>());

            //Act
            actualResult = _PlanIsciService.SaveIsciMappings(isciPlanProductMapping, createdBy);

            //Assert
            Assert.AreEqual(expectedData, actualResult);
        }

        [Test]
        public void SaveIsciPlanMappings()
        {
            //Arrange
            string createdBy = "Test User";
            DateTime createdAt = DateTime.Now;
            int result = 0;
            bool actualResult = true;
            bool expectedData = false;
            var expectedIsciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE77VR16"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7059,
                                Isci= "AE87VR14"
                            }
                        };
            var isciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE77VR14"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7049,
                                Isci= "AE87VR14"
                            }
                        };
            var isciProductList = new List<IsciProductMappingDto>
                        {
                            new IsciProductMappingDto()
                            {
                                ProductName = "Femoston",
                                Isci= "AE67VR14"
                            },
                            new IsciProductMappingDto()
                            {
                                ProductName = "Femoston",
                                Isci= "AE67VR14"
                            }
                        };
            var isciPlanProductMapping = new IsciPlanProductMappingDto()
            {
                IsciPlanMappings = isciPlanList,
                IsciProductMappings = isciProductList
            };
            _PlanIsciRepositoryMock
                .Setup(s => s.GetPlanIscis())
                .Returns(expectedIsciPlanList);
            _PlanIsciRepositoryMock
            .Setup(s => s.SaveIsciPlanMappings(It.IsAny<List<IsciPlanMappingDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback(() =>
            {
                result = 2;
            })
            .Returns(result);

            _PlanIsciRepositoryMock.Setup(s => s.GetIsciProductMappings(It.IsAny<List<string>>()))
                .Returns(new List<IsciProductMappingDto>());

            //Act
            actualResult = _PlanIsciService.SaveIsciMappings(isciPlanProductMapping, createdBy);

            //Assert
            Assert.AreEqual(expectedData, actualResult);
        }

        [Test]
        public void SaveIsciPlanMappings_FilterDuplicatesInDb()
        {
            //Arrange
            string createdBy = "Test User";
            DateTime createdAt = DateTime.Now;
            var existingMappings = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE77VR14"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7059,
                                Isci= "AE87VR15"
                            }
                        };
            var isciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE77VR14"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7049,
                                Isci= "AE87VR16"
                            }
                        };
            var isciPlanProductMapping = new IsciPlanProductMappingDto()
            {
                IsciPlanMappings = isciPlanList
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetPlanIscis())
                .Returns(existingMappings);

            var savedPlanMappings = new List<List<IsciPlanMappingDto>>();
            _PlanIsciRepositoryMock
                .Setup(s => s.SaveIsciPlanMappings(It.IsAny<List<IsciPlanMappingDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback<List<IsciPlanMappingDto>, string, DateTime>((pms, un, dt) => savedPlanMappings.Add(pms))
            .Returns(2);

            _PlanIsciRepositoryMock.Setup(s => s.GetIsciProductMappings(It.IsAny<List<string>>()))
                .Returns(new List<IsciProductMappingDto>());

            //Act
            _PlanIsciService.SaveIsciMappings(isciPlanProductMapping, createdBy);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedPlanMappings));
        }

        [Test]
        public void SaveIsciPlanMappings_FilterDuplicatesInGivenList()
        {
            //Arrange
            var createdBy = "Test User";
            var createdAt = DateTime.Now;
            var existingMappings = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE77VR16"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7059,
                                Isci= "AE87VR15"
                            }
                        };
            var isciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE77VR14"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7059,
                                Isci= "AE87VR27"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE77VR14"
                            }
                        };
            var isciPlanProductMapping = new IsciPlanProductMappingDto()
            {
                IsciPlanMappings = isciPlanList
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetPlanIscis())
                .Returns(existingMappings);

            var savedPlanMappings = new List<List<IsciPlanMappingDto>>();
            _PlanIsciRepositoryMock
                .Setup(s => s.SaveIsciPlanMappings(It.IsAny<List<IsciPlanMappingDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback<List<IsciPlanMappingDto>, string, DateTime>((pms, un, dt) => savedPlanMappings.Add(pms))
            .Returns(2);

            _PlanIsciRepositoryMock.Setup(s => s.GetIsciProductMappings(It.IsAny<List<string>>()))
                .Returns(new List<IsciProductMappingDto>());

            //Act
            _PlanIsciService.SaveIsciMappings(isciPlanProductMapping, createdBy);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedPlanMappings));
        }

        [Test]
        public void SaveIsciPlanMappings_DuplicateProgramMappingsInGivenList()
        {
            //Arrange
            var createdBy = "Test User";
            var createdAt = DateTime.Now;
            var existingMappings = new List<IsciPlanMappingDto>();
            var existingProductMappings = new List<IsciProductMappingDto>();

            var isciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 123,
                                Isci= "Isci-273"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 789,
                                Isci= "Isci-789"
                            }
                        };
            var isciProductList = new List<IsciProductMappingDto>
            {
                new IsciProductMappingDto
                {
                    Isci = "Isci-123",
                    ProductName = "Product-273"
                },
                new IsciProductMappingDto
                {
                    Isci = "Isci-789",
                    ProductName = "Product-789"
                },
                new IsciProductMappingDto
                {
                    Isci = "Isci-123",
                    ProductName = "Product-273"
                }
            };

            var isciPlanProductMapping = new IsciPlanProductMappingDto()
            {
                IsciPlanMappings = isciPlanList,
                IsciProductMappings = isciProductList
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetPlanIscis())
                .Returns(existingMappings);

            var savedIsciProductMappings = new List<List<IsciProductMappingDto>>();
            _PlanIsciRepositoryMock.Setup(s => 
                    s.SaveIsciProductMappings(It.IsAny<List<IsciProductMappingDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<List<IsciProductMappingDto>, string, DateTime>((m, c, d) => savedIsciProductMappings.Add(m))
                .Returns(2);

            var getIsciProductMappingsParams = new List<List<string>>();
            _PlanIsciRepositoryMock.Setup(s => s.GetIsciProductMappings(It.IsAny<List<string>>()))
                .Callback<List<string>>((l) => getIsciProductMappingsParams.Add(l))
                .Returns(existingProductMappings);
            
            //Act
            _PlanIsciService.SaveIsciMappings(isciPlanProductMapping, createdBy);

            //Assert
            var toVerify = new
            {
                getIsciProductMappingsParams = getIsciProductMappingsParams,
                savedIsciProductMappings = savedIsciProductMappings
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void SaveIsciPlanMappings_DuplicateProgramMappingsInDb()
        {
            //Arrange
            var createdBy = "Test User";
            var createdAt = DateTime.Now;
            var existingMappings = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 123,
                                Isci= "Isci-123"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 456,
                                Isci= "Isci-456"
                            }
                        };
            var existingProductMappings = new List<IsciProductMappingDto>
                {
                    new IsciProductMappingDto
                    {
                        Isci = "Isci-123",
                        ProductName = "Product-123"
                    }
                };

            var isciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 123,
                                Isci= "Isci-273"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 789,
                                Isci= "Isci-789"
                            }
                        };
            var isciProductList = new List<IsciProductMappingDto>
            {
                new IsciProductMappingDto
                {
                    Isci = "Isci-123",
                    ProductName = "Product-273"
                },
                new IsciProductMappingDto
                {
                    Isci = "Isci-789",
                    ProductName = "Product-789"
                }
            };

            var isciPlanProductMapping = new IsciPlanProductMappingDto()
            {
                IsciPlanMappings = isciPlanList,
                IsciProductMappings = isciProductList
            };

            _PlanIsciRepositoryMock
                .Setup(s => s.GetPlanIscis())
                .Returns(existingMappings);

            var savedIsciProductMappings = new List<List<IsciProductMappingDto>>();
            _PlanIsciRepositoryMock.Setup(s =>
                    s.SaveIsciProductMappings(It.IsAny<List<IsciProductMappingDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<List<IsciProductMappingDto>, string, DateTime>((m, c, d) => savedIsciProductMappings.Add(m))
                .Returns(2);

            var getIsciProductMappingsParams = new List<List<string>>();
            _PlanIsciRepositoryMock.Setup(s => s.GetIsciProductMappings(It.IsAny<List<string>>()))
                .Callback<List<string>>((l) => getIsciProductMappingsParams.Add(l))
                .Returns(existingProductMappings);

            //Act
            _PlanIsciService.SaveIsciMappings(isciPlanProductMapping, createdBy);

            //Assert
            var toVerify = new
            {
                getIsciProductMappingsParams = getIsciProductMappingsParams,
                savedIsciProductMappings = savedIsciProductMappings
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void DeletedIsciPlanMappings()
        {
            //Arrange
            string createdBy = "Test User";
            DateTime createdAt = DateTime.Now;
            int result = 0;
            bool actualResult = true;
            bool expectedData = false;
            var expectedIsciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE77VR16"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7059,
                                Isci= "AE87VR14"
                            }
                        };
            var isciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE77VR14"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7049,
                                Isci= "AE87VR14"
                            }
                        };
            var isciProductList = new List<IsciProductMappingDto>
                        {
                            new IsciProductMappingDto()
                            {
                                ProductName = "Femoston",
                                Isci= "AE67VR14"
                            },
                            new IsciProductMappingDto()
                            {
                                ProductName = "Femoston",
                                Isci= "AE67VR14"
                            }
                        };
            var isciPlanProductMapping = new IsciPlanProductMappingDto()
            {
                IsciPlanMappings = isciPlanList,
                IsciProductMappings = isciProductList,
                IsciPlanMappingsDeleted = isciPlanList
            };
            _PlanIsciRepositoryMock
                .Setup(s => s.GetPlanIscis())
                .Returns(expectedIsciPlanList);
            _PlanIsciRepositoryMock
            .Setup(s => s.SaveIsciPlanMappings(It.IsAny<List<IsciPlanMappingDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback(() =>
            {
                result = 2;
            })
            .Returns(result);

            _PlanIsciRepositoryMock.Setup(s => s.GetIsciProductMappings(It.IsAny<List<string>>()))
                .Returns(new List<IsciProductMappingDto>());

            //Act
            actualResult = _PlanIsciService.SaveIsciMappings(isciPlanProductMapping, createdBy);

            //Assert
            Assert.AreEqual(expectedData, actualResult);
        }

        [Test]
        public void SaveIsciPlanMappings_ThrowsException()
        {
            // Arrange
            _PlanIsciRepositoryMock
                    .Setup(s => s.SaveIsciProductMappings(It.IsAny<List<IsciProductMappingDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                    .Callback(() =>
                    {
                        throw new Exception("Throwing a test exception.");
                    });
            var isciPlanList = new List<IsciPlanMappingDto>
                        {
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7009,
                                Isci= "AE67VR14"
                            },
                            new IsciPlanMappingDto()
                            {
                                PlanId = 7049,
                                Isci= "AE67VR14"
                            }
                        };
            var isciProductList = new List<IsciProductMappingDto>
                        {
                            new IsciProductMappingDto()
                            {
                                ProductName = "Femoston",
                                Isci= "AE67VR14"
                            },
                            new IsciProductMappingDto()
                            {
                                ProductName = "Femoston",
                                Isci= "AE67VR14"
                            }
                        };
            var isciPlanProductMapping = new IsciPlanProductMappingDto()
            {
                IsciPlanMappings = isciPlanList,
                IsciProductMappings = isciProductList
            };

            _PlanIsciRepositoryMock.Setup(s => s.GetIsciProductMappings(It.IsAny<List<string>>()))
                .Returns(new List<IsciProductMappingDto>());

            // Act
            var result = Assert.Throws<Exception>(() => _PlanIsciService.SaveIsciMappings(isciPlanProductMapping, It.IsAny<string>()));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }
    }
}
