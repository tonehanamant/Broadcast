using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

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

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _PlanIsciRepositoryMock = new Mock<IPlanIsciRepository>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanIsciRepository>())
                .Returns(_PlanIsciRepositoryMock.Object);
            _PlanIsciService = new PlanIsciService(_DataRepositoryFactoryMock.Object, _MediaMonthAndWeekAggregateCacheMock.Object, _DateTimeEngineMock.Object);
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
                MediaMonth = new MediaMonthDto { Month = 5, Year = 2021 },
                WithoutPlansOnly = true,

            };
            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<int>(), It.IsAny<int>()))
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
                MediaMonth = new MediaMonthDto { Month = 5, Year = 2021 },
                WithoutPlansOnly = true,

            };
            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<int>(), It.IsAny<int>()))
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
                MediaMonth = new MediaMonthDto { Month = 5, Year = 2021 },
                WithoutPlansOnly = true,

            };
            _PlanIsciRepositoryMock
                .Setup(x => x.GetAvailableIscis(It.IsAny<int>(), It.IsAny<int>()))
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
        public void GetUnAvailableIscisThrowsException()
        {
            // Arrange           
            IsciSearchDto isciSearch = new IsciSearchDto
            {
                MediaMonth = new MediaMonthDto { Month = 5, Year = 2021 },
                WithoutPlansOnly = true,

            };
            _PlanIsciRepositoryMock
               .Setup(x => x.GetAvailableIscis(It.IsAny<int>(), It.IsAny<int>()))
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
                Isci = "OKWF1701H"
                },
                 new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes",
                Id = 2,
                SpotLengthDuration = 22,
                ProductName = "Product2",
                Isci = "OKWL1702H"
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "ATLT0063000HU",
                Id = 3,
                SpotLengthDuration = 23,
                ProductName = "Product3",
                Isci = "OKWF1701H"
                },
                   new IsciAdvertiserDto()
                {
                AdvertiserName = "Invisaling (Adult)",
                Id = 4,
                SpotLengthDuration = 24,
                ProductName = "Product4",
                Isci = "CLDC6513000H"
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Colgate EM",
                Id = 5,
                SpotLengthDuration = 25,
                ProductName = "Product5",
                Isci = "CUSA1813000H"
                },
               new IsciAdvertiserDto()
                {
                AdvertiserName = "O'Keeffes",
                Id = 6,
                SpotLengthDuration = 26,
                ProductName = "Product6",
                Isci = "OKWF1701H"
                },
               new IsciAdvertiserDto()
                {
                AdvertiserName = "USAA",
                Id = 7,
                SpotLengthDuration = 27,
                ProductName = "Product7",
                Isci = "OKWL1702H"
                },
               new IsciAdvertiserDto()
                {
                AdvertiserName = "Nature's Bounty",
                Id = 8,
                SpotLengthDuration = 28,
                ProductName = null,
                Isci = "ATLT0063000HU"
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Colgate EM",
                Id = 9,
                SpotLengthDuration = 29,
                ProductName = null,
                Isci = "CLDC6513000H"
                },
                new IsciAdvertiserDto()
                {
                AdvertiserName = "Colgate EM",
                Id = 30,
                SpotLengthDuration = 30,
                ProductName = null,
                Isci = "CUSA1813000H"
                }
            };
        }
    }
}
