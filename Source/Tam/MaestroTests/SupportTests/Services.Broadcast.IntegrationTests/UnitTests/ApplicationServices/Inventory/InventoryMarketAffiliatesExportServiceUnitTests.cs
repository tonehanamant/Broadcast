using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Services.Broadcast.IntegrationTests.TestData;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Inventory;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventoryMarketAffiliates;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Tam.Maestro.Data.Entities;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Services.Broadcast.Repositories.Inventory;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Inventory
{
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class InventoryMarketAffiliatesExportServiceUnitTests
    {
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISharedFolderService> _SharedFolderServiceMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IGenreRepository> _GenreRepository;
        private Mock<IInventoryMarketAffiliatesExportRepository> _InventoryMarketAffiliatesExportRepository;
        private InventoryMarketAffiliatesExportService _InventoryMarketAffiliatesExportService;
        private Mock<IMarketCoverageRepository> _MarketCoverageRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SharedFolderServiceMock = new Mock<ISharedFolderService>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _GenreRepository = new Mock<IGenreRepository>();
            _InventoryMarketAffiliatesExportRepository = new Mock<IInventoryMarketAffiliatesExportRepository>();
            _MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IGenreRepository>())
                .Returns(_GenreRepository.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryMarketAffiliatesExportRepository>())
                .Returns(_InventoryMarketAffiliatesExportRepository.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IMarketCoverageRepository>())
                .Returns(_MarketCoverageRepositoryMock.Object);

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetMediaWeeks());

            _InventoryMarketAffiliatesExportService = new InventoryMarketAffiliatesExportService(
                _DataRepositoryFactoryMock.Object,
                _SharedFolderServiceMock.Object,
                _DateTimeEngineMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _FeatureToggleMock.Object,
                _ConfigurationSettingsHelperMock.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMarketAffiliatesReportData()
        {
            // Arrange
            List<int> genreIds = new List<int> { 9, 11, 12, 14, 15, 20, 25, 26, 33, 39, 40, 42, 44, 45, 51, 52, 53, 54, 55, 56 };
            var newsMarkets = _GetMarkets();
            var nonNewsMarkets = _GetMarkets();
            var request = new InventoryMarketAffiliatesRequest
            {
                InventorySourceId = 1,
                Quarter = new QuarterDetailDto
                {
                    Quarter = 2,
                    Year = 2022,
                    StartDate = new DateTime(2022, 3, 30),
                    EndDate = new DateTime(2022, 6, 28)
                }
            };

            _GenreRepository
                .Setup(s => s.GetAllMaestroGenres())
                .Returns(_GetAllGenres());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoverageFile())
                .Returns(MarketsTestData.GetLatestMarketCoverageFile());

            var isAggregateList = _GetIsAggregateList();

            _InventoryMarketAffiliatesExportRepository
                .Setup(x => x.GetMarketInvenoryList(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>()))
                .Returns(_GetMarketInventoryList());

            _InventoryMarketAffiliatesExportRepository
                .Setup(x => x.GetMarketAffiliateList(It.IsAny<List<string>>(), It.IsAny<int>()))
                .Returns(_GetMarketAffiliatesList());

            // Act
            var result = _InventoryMarketAffiliatesExportService.GetMarketAffiliatesReportData(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMarketAffiliates_News()
        {
            // Arrange
            int inventorySourceId = 1;
            int marketCoverageFileId = 1;
            List<string> affiliates = new List<string> { "ABC", "NBC", "FOX", "CBS", "CW" };
            List<int> mediaWeekIds = new List<int> { 951, 952 };
            List<int> newsGenreIds = new List<int> { 34 };
            var isAggregateList = _GetIsAggregateList();

            _InventoryMarketAffiliatesExportRepository
                .Setup(x => x.GetMarketInvenoryList(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>()))
                .Returns(_GetMarketInventoryList());

            _InventoryMarketAffiliatesExportRepository
                .Setup(x => x.GetMarketAffiliateList(It.IsAny<List<string>>(), It.IsAny<int>()))
                .Returns(_GetMarketAffiliatesList());

            // Act
            var result = _InventoryMarketAffiliatesExportService._GetMarketAffiliates(inventorySourceId, mediaWeekIds, affiliates, newsGenreIds, marketCoverageFileId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMarketAffiliates_NonNews()
        {
            // Arrange
            int inventorySourceId = 1;
            int marketCoverageFileId = 1;
            List<string> affiliates = new List<string> { "ABC", "NBC", "FOX", "CBS", "CW" };
            List<int> mediaWeekIds = new List<int> { 951, 952 };
            List<int> newsGenreIds = new List<int> { 9, 11, 12, 14, 15, 20, 25, 26, 33, 39, 40, 42, 44, 45, 51, 52, 53, 54, 55, 56 };

            var isAggregateList = _GetIsAggregateList();

            _InventoryMarketAffiliatesExportRepository
                .Setup(x => x.GetMarketInvenoryList(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>()))
                .Returns(_GetMarketInventoryList());

            _InventoryMarketAffiliatesExportRepository
                .Setup(x => x.GetMarketAffiliateList(It.IsAny<List<string>>(), It.IsAny<int>()))
                .Returns(_GetMarketAffiliatesList());

            // Act
            var result = _InventoryMarketAffiliatesExportService._GetMarketAffiliates(inventorySourceId, mediaWeekIds, affiliates, newsGenreIds, marketCoverageFileId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetIsAggregateList()
        {
            // Arrange
            var marketInventoryList = _GetMarketInventoryList();

            _InventoryMarketAffiliatesExportRepository
                .Setup(x => x.GetMarketInvenoryList(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<string>>(), It.IsAny<List<int>>()))
                .Returns(_GetMarketInventoryList());

            // Act
            var result = _InventoryMarketAffiliatesExportService._GetIsAggregateList(marketInventoryList);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<MarketInventoryDto> _GetMarketInventoryList()
        {
            var marketInventoryList = new List<MarketInventoryDto>
            {
                new MarketInventoryDto
                {
                    marketCode = 101,
                    affiliation = "ABC",
                    isInventory = "Yes"
                },
                new MarketInventoryDto
                {
                    marketCode = 101,
                    affiliation = "CBS",
                    isInventory = "Yes"
                },
                new MarketInventoryDto
                {
                    marketCode = 101,
                    affiliation = "CW",
                    isInventory = "Yes"
                },
                new MarketInventoryDto
                {
                    marketCode = 101,
                    affiliation = "FOX",
                    isInventory = "Yes"
                },
                new MarketInventoryDto
                {
                    marketCode = 101,
                    affiliation = "NBC",
                    isInventory = "Yes"
                },
                new MarketInventoryDto
                {
                    marketCode = 102,
                    affiliation = "ABC",
                    isInventory = "No"
                },
                new MarketInventoryDto
                {
                    marketCode = 102,
                    affiliation = "CBS",
                    isInventory = "No"
                },
                new MarketInventoryDto
                {
                    marketCode = 102,
                    affiliation = "CW",
                    isInventory = "No"
                },
                new MarketInventoryDto
                {
                    marketCode = 102,
                    affiliation = "FOX",
                    isInventory = "No"
                },
                new MarketInventoryDto
                {
                    marketCode = 102,
                    affiliation = "NBC",
                    isInventory = "No"
                },
                new MarketInventoryDto
                {
                    marketCode = 103,
                    affiliation = "ABC",
                    isInventory = "No"
                },
                new MarketInventoryDto
                {
                    marketCode = 103,
                    affiliation = "CBS",
                    isInventory = "No"
                },
                new MarketInventoryDto
                {
                    marketCode = 103,
                    affiliation = "CW",
                    isInventory = "Yes"
                },
                new MarketInventoryDto
                {
                    marketCode = 103,
                    affiliation = "FOX",
                    isInventory = "Yes"
                },
                new MarketInventoryDto
                {
                    marketCode = 103,
                    affiliation = "NBC",
                    isInventory = "Yes"
                }
            };
            return marketInventoryList;
        }

        private List<MarketAffiliatesDto> _GetMarketAffiliatesList()
        {
            var marketAffiliatesList = new List<MarketAffiliatesDto>
            {
                new MarketAffiliatesDto
                {
                    marketName = "New York",
                    marketCode = 101,
                    rank = 1,
                    affiliation = "ABC"
                },
                new MarketAffiliatesDto
                {
                    marketName = "New York",
                    marketCode = 101,
                    rank = 1,
                    affiliation = "CBS"
                },
                new MarketAffiliatesDto
                {
                    marketName = "New York",
                    marketCode = 101,
                    rank = 1,
                    affiliation = "CW"
                },
                new MarketAffiliatesDto
                {
                    marketName = "New York",
                    marketCode = 101,
                    rank = 1,
                    affiliation = "FOX"
                },
                new MarketAffiliatesDto
                {
                    marketName = "New York",
                    marketCode = 101,
                    rank = 1,
                    affiliation = "NBC"
                },
                new MarketAffiliatesDto
                {
                    marketName = "Binghamton",
                    marketCode = 102,
                    rank = 161,
                    affiliation = "ABC"
                },new MarketAffiliatesDto
                {
                    marketName = "Binghamton",
                    marketCode = 102,
                    rank = 161,
                    affiliation = "CBS"
                },
                new MarketAffiliatesDto
                {
                    marketName = "Binghamton",
                    marketCode = 102,
                    rank = 161,
                    affiliation = "CW"
                },
                new MarketAffiliatesDto
                {
                    marketName = "Binghamton",
                    marketCode = 102,
                    rank = 161,
                    affiliation = "FOX"
                },
                new MarketAffiliatesDto
                {
                    marketName = "Binghamton",
                    marketCode = 102,
                    rank = 161,
                    affiliation = "NBC"
                },
                new MarketAffiliatesDto
                {
                    marketName = "Macon",
                    marketCode = 103,
                    rank = 120,
                    affiliation = "ABC"
                },
                new MarketAffiliatesDto
                {
                    marketName = "Macon",
                    marketCode = 103,
                    rank = 120,
                    affiliation = "CBS"
                },
                new MarketAffiliatesDto
                {
                    marketName = "Macon",
                    marketCode = 103,
                    rank = 120,
                    affiliation = "CW"
                },
                new MarketAffiliatesDto
                {
                    marketName = "Macon",
                    marketCode = 103,
                    rank = 120,
                    affiliation = "FOX"
                },
                new MarketAffiliatesDto
                {
                    marketName = "Macon",
                    marketCode = 103,
                    rank = 120,
                    affiliation = "NBC"
                }
            };
            return marketAffiliatesList;
        }

        private List<IsAggregateDto> _GetIsAggregateList()
        {
            var isAggregateList = new List<IsAggregateDto>
            {
                new IsAggregateDto
                {
                    isAggregate = "Yes",
                    marketCode = 101
                },
                new IsAggregateDto
                {
                    isAggregate = "No",
                    marketCode = 102
                },
                new IsAggregateDto
                {
                    isAggregate = "No",
                    marketCode = 103
                }
            };
            return isAggregateList;
        }

        private List<InventoryMarketAffiliates> _GetMarkets()
        {
            var marketList = new List<InventoryMarketAffiliates>
            {
                new InventoryMarketAffiliates
                {
                    affiliates = "ABC",
                    aggregate = "Yes",
                    inventory = "Yes",
                    marketName = "New York",
                    marketRank = 1
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "CBS",
                    aggregate = "Yes",
                    inventory = "Yes",
                    marketName = "New York",
                    marketRank = 1
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "CW",
                    aggregate = "Yes",
                    inventory = "Yes",
                    marketName = "New York",
                    marketRank = 1
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "FOX",
                    aggregate = "Yes",
                    inventory = "Yes",
                    marketName = "New York",
                    marketRank = 1
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "NBC",
                    aggregate = "Yes",
                    inventory = "Yes",
                    marketName = "New York",
                    marketRank = 1
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "ABC",
                    aggregate = "No",
                    inventory = "No",
                    marketName = "Binghamton",
                    marketRank = 161
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "CBS",
                    aggregate = "No",
                    inventory = "No",
                    marketName = "Binghamton",
                    marketRank = 161
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "CW",
                    aggregate = "No",
                    inventory = "No",
                    marketName = "Binghamton",
                    marketRank = 161
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "FOX",
                    aggregate = "No",
                    inventory = "No",
                    marketName = "Binghamton",
                    marketRank = 161
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "NBC",
                    aggregate = "No",
                    inventory = "No",
                    marketName = "Binghamton",
                    marketRank = 161
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "ABC",
                    aggregate = "No",
                    inventory = "No",
                    marketName = "Macon",
                    marketRank = 120
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "CBS",
                    aggregate = "No",
                    inventory = "No",
                    marketName = "Macon",
                    marketRank = 120
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "CW",
                    aggregate = "No",
                    inventory = "Yes",
                    marketName = "Macon",
                    marketRank = 120
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "FOX",
                    aggregate = "No",
                    inventory = "Yes",
                    marketName = "Macon",
                    marketRank = 120
                },
                new InventoryMarketAffiliates
                {
                    affiliates = "NBC",
                    aggregate = "No",
                    inventory = "Yes",
                    marketName = "Macon",
                    marketRank = 120
                }
            };
            return marketList;
        }

        private List<LookupDto> _GetAllGenres()
        {
            var allGenres = new List<LookupDto>
            {
                new LookupDto { Display = "Comedy", Id = 9 },
                new LookupDto { Display = "Crime", Id = 11 },
                new LookupDto { Display = "Documentary", Id = 12 },
                new LookupDto { Display = "Drama", Id = 14 },
                new LookupDto { Display = "Entertainment", Id = 15 },
                new LookupDto { Display = "Game Show", Id = 20 },
                new LookupDto { Display = "Horror", Id = 25 },
                new LookupDto { Display = "Informational", Id = 26 },
                new LookupDto { Display = "Nature", Id = 33 },
                new LookupDto { Display = "News", Id = 34 },
                new LookupDto { Display = "Reality", Id = 39 },
                new LookupDto { Display = "Religious", Id = 40 },
                new LookupDto { Display = "Science Fiction", Id = 42 },
                new LookupDto { Display = "Sports/Sports Talk", Id = 44 },
                new LookupDto { Display = "Talk", Id = 45 },
                new LookupDto { Display = "Action/Adventure", Id = 51 },
                new LookupDto { Display = "Children", Id = 52 },
                new LookupDto { Display = "Educational", Id = 53 },
                new LookupDto { Display = "Lifestyle", Id = 54 },
                new LookupDto { Display = "Paid Program", Id = 55 },
                new LookupDto { Display = "Special", Id = 56 }
            };
            return allGenres;
        }

        private List<MediaWeek> _GetMediaWeeks()
        {
            var testMediaWeeks = new List<MediaWeek>
            {
                new MediaWeek(949, 486, 1,  DateTime.Parse("2022-02-28T00:00:00"), DateTime.Parse("2022-03-06T00:00:00")),
                new MediaWeek(950, 486, 2,  DateTime.Parse("2022-03-07T00:00:00"), DateTime.Parse("2022-03-13T00:00:00")),
                new MediaWeek(951, 486, 3,  DateTime.Parse("2022-03-14T00:00:00"), DateTime.Parse("2022-03-20T00:00:00")),
                new MediaWeek(952, 486, 4,  DateTime.Parse("2022-03-21T00:00:00"), DateTime.Parse("2022-03-27T00:00:00")),
                new MediaWeek(953, 487, 1,  DateTime.Parse("2022-03-28T00:00:00"), DateTime.Parse("2022-04-03T00:00:00")),
                new MediaWeek(954, 487, 2,  DateTime.Parse("2022-04-04T00:00:00"), DateTime.Parse("2022-04-10T00:00:00")),
                new MediaWeek(955, 487, 3,  DateTime.Parse("2022-04-11T00:00:00"), DateTime.Parse("2022-04-17T00:00:00")),
                new MediaWeek(956, 487, 4,  DateTime.Parse("2022-04-18T00:00:00"), DateTime.Parse("2022-04-24T00:00:00")),
                new MediaWeek(957, 488, 1,  DateTime.Parse("2022-04-25T00:00:00"), DateTime.Parse("2022-05-01T00:00:00")),
                new MediaWeek(958, 488, 2,  DateTime.Parse("2022-05-02T00:00:00"), DateTime.Parse("2022-05-08T00:00:00")),
                new MediaWeek(959, 488, 3,  DateTime.Parse("2022-05-09T00:00:00"), DateTime.Parse("2022-05-15T00:00:00")),
                new MediaWeek(960, 488, 4,  DateTime.Parse("2022-05-16T00:00:00"), DateTime.Parse("2022-05-22T00:00:00")),
                new MediaWeek(961, 488, 5,  DateTime.Parse("2022-05-23T00:00:00"), DateTime.Parse("2022-05-29T00:00:00")),
                new MediaWeek(962, 489, 1,  DateTime.Parse("2022-05-30T00:00:00"), DateTime.Parse("2022-06-05T00:00:00")),
                new MediaWeek(963, 489, 2,  DateTime.Parse("2022-06-06T00:00:00"), DateTime.Parse("2022-06-12T00:00:00")),
                new MediaWeek(964, 489, 3,  DateTime.Parse("2022-06-13T00:00:00"), DateTime.Parse("2022-06-19T00:00:00")),
                new MediaWeek(965, 489, 4,  DateTime.Parse("2022-06-20T00:00:00"), DateTime.Parse("2022-06-26T00:00:00")),
            };
            return testMediaWeeks;
        }
    }
}
