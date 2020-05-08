
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    /// <summary>
    /// Test the InventoryExportService
    /// </summary>
    public class InventoryExportServiceUnitTests
    {
        private const string TEST_USERNAME = "testUser";

        [Test]
        public void GenerateExportForOpenMarket()
        {
            /*** Arrange ***/
            var request = new InventoryExportRequestDto
            {
                Genre = InventoryExportGenreTypeEnum.News,
                Quarter = new QuarterDetailDto
                {
                    Quarter = 2,
                    Year = 2020,
                    StartDate = new DateTime(2020, 3, 30),
                    EndDate = new DateTime(2020, 6, 28)
                }
            };
            var testCurrentTimestamp = new DateTime(2020, 05, 06, 14, 32, 18);

            // instantiate our mocks.
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var inventoryExportEngine = new Mock<IInventoryExportEngine>();
            var fileService = new Mock<IFileService>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            var daypartCache = new Mock<IDaypartCache>();
            var marketService = new Mock<IMarketService>();

            var inventoryRepository = new Mock<IInventoryRepository>();
            var inventoryExportRepository = new Mock<IInventoryExportRepository>();
            var inventoryExportJobRepository = new Mock<IInventoryExportJobRepository>();
            var genreRepository = new Mock<IGenreRepository>();
            var stationRepository = new Mock<IStationRepository>();

            // load our mocks with our test data.
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "Open Market"
            };

            inventoryRepository.Setup(s => s.GetInventorySource(It.IsAny<int>()))
                .Returns(inventorySource);

            const int createdJobId = 1;
            var jobsCreated = new List<Tuple<int, BackgroundJobProcessingStatus, QuarterDetailDto, InventoryExportGenreTypeEnum>>();
            inventoryExportJobRepository.Setup(s => s.CreateJob(It.IsAny<InventoryExportJobDto>(), It.IsAny<string>()))
                .Callback<InventoryExportJobDto, string>((j, u) => jobsCreated.Add(
                    new Tuple<int, BackgroundJobProcessingStatus, QuarterDetailDto, InventoryExportGenreTypeEnum>(j.InventorySourceId, j.Status, j.Quarter, j.ExportGenreType)))
                .Returns(createdJobId);
            const int spotLengthId = 1;
            spotLengthEngine.Setup(s => s.GetSpotLengthIdByValue(It.IsAny<int>()))
                .Returns(spotLengthId);

            genreRepository.Setup(s => s.GetAllMaestroGenres())
                .Returns(_GetAllGenres());

            var testMediaWeeks = new List<MediaWeek>
            {
                new MediaWeek(849, 463, 1,  DateTime.Parse("2020-03-30T00:00:00"), DateTime.Parse("2020-04-05T00:00:00")),
                new MediaWeek(850, 463, 2,  DateTime.Parse("2020-04-06T00:00:00"), DateTime.Parse("2020-04-12T00:00:00")),
                new MediaWeek(851, 463, 3,  DateTime.Parse("2020-04-13T00:00:00"), DateTime.Parse("2020-04-19T00:00:00")),
                new MediaWeek(852, 463, 4,  DateTime.Parse("2020-04-20T00:00:00"), DateTime.Parse("2020-04-26T00:00:00")),
                new MediaWeek(853, 464, 1,  DateTime.Parse("2020-04-27T00:00:00"), DateTime.Parse("2020-05-03T00:00:00")),
                new MediaWeek(854, 464, 2,  DateTime.Parse("2020-05-04T00:00:00"), DateTime.Parse("2020-05-10T00:00:00")),
                new MediaWeek(855, 464, 3,  DateTime.Parse("2020-05-11T00:00:00"), DateTime.Parse("2020-05-17T00:00:00")),
                new MediaWeek(856, 464, 4,  DateTime.Parse("2020-05-18T00:00:00"), DateTime.Parse("2020-05-24T00:00:00")),
                new MediaWeek(857, 464, 5,  DateTime.Parse("2020-05-25T00:00:00"), DateTime.Parse("2020-05-31T00:00:00")),
                new MediaWeek(858, 465, 1,  DateTime.Parse("2020-06-01T00:00:00"), DateTime.Parse("2020-06-07T00:00:00")),
                new MediaWeek(859, 465, 2,  DateTime.Parse("2020-06-08T00:00:00"), DateTime.Parse("2020-06-14T00:00:00")),
                new MediaWeek(860, 465, 3,  DateTime.Parse("2020-06-15T00:00:00"), DateTime.Parse("2020-06-21T00:00:00")),
                new MediaWeek(861, 465, 4,  DateTime.Parse("2020-06-22T00:00:00"), DateTime.Parse("2020-06-28T00:00:00")),
            };
            mediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(testMediaWeeks);

            var inventory = new List<InventoryExportDto>
            {
                new InventoryExportDto{ InventoryId = 1, StationId = 1, MediaWeekId = 849, DaypartId = 1, Impressions = 1, SpotCost =1, ProgramName = "One"},
                new InventoryExportDto{ InventoryId = 2, StationId = 2, MediaWeekId = 850, DaypartId = 2, Impressions = 2, SpotCost =2, ProgramName = "Two"},
            };
            var betInventoryForExportOpenMarketCalls = new List<Tuple<List<int>, List<int>, List<int>>>();
            inventoryRepository.Setup(s =>
                    s.GetInventoryForExportOpenMarket(It.IsAny<List<int>>(), It.IsAny<List<int>>(),
                        It.IsAny<List<int>>()))
                .Callback<List<int>, List<int>, List<int>>((s, g, w) =>
                    betInventoryForExportOpenMarketCalls.Add(new Tuple<List<int>, List<int>, List<int>>(s, g, w)))
                .Returns(inventory);

            var calculateReturn = new List<InventoryExportLineDetail>
            {
                new InventoryExportLineDetail {StationId = 1},
                new InventoryExportLineDetail {StationId = 2}
            };
            var calculateCalledCount = 0;
            inventoryExportEngine.Setup(s => s.Calculate(It.IsAny<List<InventoryExportDto>>()))
                .Callback(() => calculateCalledCount++)
                .Returns(calculateReturn);

            var stations = new List<DisplayBroadcastStation>
            {
                new DisplayBroadcastStation{ Id = 1},
                new DisplayBroadcastStation{ Id = 2},
                new DisplayBroadcastStation{ Id = 3},
                new DisplayBroadcastStation{ Id = 4}
            };
            stationRepository.Setup(s => s.GetBroadcastStations())
                .Returns(stations);

            var markets = new List<MarketCoverage>();
            marketService.Setup(s => s.GetMarketsWithLatestCoverage())
                .Returns(markets);

            var daypartsDict = new Dictionary<int, DisplayDaypart>();
            daypartCache.Setup(s => s.GetDisplayDayparts(It.IsAny<List<int>>()))
                .Returns(daypartsDict);

            var generateExportFileResult = new InventoryExportGenerationResult
            {
                InventoryTabLineCount = 1,
                ExportExcelPackage = new ExcelPackage()
            };
            generateExportFileResult.ExportExcelPackage.Workbook.Worksheets.Add("Inventory");
            inventoryExportEngine.Setup(s => s.GenerateExportFile(It.IsAny<List<InventoryExportLineDetail>>(),
                    It.IsAny<List<int>>(), It.IsAny<List<DisplayBroadcastStation>>(), It.IsAny<List<MarketCoverage>>(),
                    It.IsAny<Dictionary<int, DisplayDaypart>>(), It.IsAny<List<DateTime>>()))
                .Returns(generateExportFileResult);

            var createFilesCalled = new List<Tuple<string, string, Stream>>();
            fileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, string, Stream>((sd, fn, st) => createFilesCalled.Add(new Tuple<string, string, Stream>(sd, fn, st)));

            var updatedJobs = new List<InventoryExportJobDto>();
            inventoryExportJobRepository.Setup(s => s.UpdateJob(It.IsAny<InventoryExportJobDto>()))
                .Callback<InventoryExportJobDto>((j) => updatedJobs.Add(j));

            // Register all the repo objects that are now setup.
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryRepository>())
                .Returns(inventoryRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryExportRepository>())
                .Returns(inventoryExportRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryExportJobRepository>())
                .Returns(inventoryExportJobRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IGenreRepository>())
                .Returns(genreRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IStationRepository>())
                .Returns(stationRepository.Object);

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportServiceUnitTestClass(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                fileService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object)
            {
                UT_DateTimeNow = testCurrentTimestamp
            };

            // *** ACT ***/
            var result = service.GenerateExportForOpenMarket(request, TEST_USERNAME);

            // *** Assert ***/
            Assert.AreEqual(1, result);
            // verify JobsCreated
            Assert.AreEqual(1, jobsCreated.Count);
            Assert.AreEqual(1, jobsCreated[0].Item1);
            Assert.AreEqual(BackgroundJobProcessingStatus.Processing, jobsCreated[0].Item2);
            Assert.AreEqual(2020, jobsCreated[0].Item3.Year);
            Assert.AreEqual(2, jobsCreated[0].Item3.Quarter);
            Assert.AreEqual(request.Genre, jobsCreated[0].Item4);
            // verify the get inventory call 
            Assert.AreEqual(1, betInventoryForExportOpenMarketCalls.Count);
            Assert.AreEqual(spotLengthId, betInventoryForExportOpenMarketCalls[0].Item1[0]); // spotlengthid
            Assert.AreEqual(34, betInventoryForExportOpenMarketCalls[0].Item2[0]); // genre id should be news only
            // verify the calculated call
            Assert.AreEqual(1, calculateCalledCount);
            // verify the saved job
            //updatedJobs
            Assert.AreEqual(1, updatedJobs.Count);
            Assert.AreEqual(BackgroundJobProcessingStatus.Succeeded, updatedJobs[0].Status);
            Assert.IsNotNull(updatedJobs[0].CompletedAt);

            // verify the saved file
            Assert.AreEqual(1, createFilesCalled.Count);
            Assert.AreEqual(@"BroadcastServiceSystemParameter.BroadcastSharedFolder\InventoryExports", createFilesCalled[0].Item1);
            Assert.AreEqual("InventoryExport_OpenMarket_20200506_143218.xlsx", createFilesCalled[0].Item2);
            Assert.IsNotNull(createFilesCalled[0].Item3);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateExportForOpenMarket_NoInventory()
        {
            /*** Arrange ***/
            var request = new InventoryExportRequestDto
            {
                Genre = InventoryExportGenreTypeEnum.News,
                Quarter = new QuarterDetailDto
                {
                    Quarter = 2,
                    Year = 2020,
                    StartDate = new DateTime(2020, 3, 30),
                    EndDate = new DateTime(2020, 6, 28)
                }
            };
            var testCurrentTimestamp = new DateTime(2020, 05, 06, 14, 32, 18);

            // instantiate our mocks.
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var inventoryExportEngine = new Mock<IInventoryExportEngine>();
            var fileService = new Mock<IFileService>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            var daypartCache = new Mock<IDaypartCache>();
            var marketService = new Mock<IMarketService>();

            var inventoryRepository = new Mock<IInventoryRepository>();
            var inventoryExportRepository = new Mock<IInventoryExportRepository>();
            var inventoryExportJobRepository = new Mock<IInventoryExportJobRepository>();
            var genreRepository = new Mock<IGenreRepository>();
            var stationRepository = new Mock<IStationRepository>();

            // load our mocks with our test data.
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "Open Market"
            };

            inventoryRepository.Setup(s => s.GetInventorySource(It.IsAny<int>()))
                .Returns(inventorySource);

            const int createdJobId = 1;
            var jobsCreated = new List<Tuple<InventoryExportJobDto, string>>();
            inventoryExportJobRepository.Setup(s => s.CreateJob(It.IsAny<InventoryExportJobDto>(), It.IsAny<string>()))
                .Callback<InventoryExportJobDto, string>((j, u) => jobsCreated.Add(new Tuple<InventoryExportJobDto, string>(j, u)))
                .Returns(createdJobId);
            const int spotLengthId = 1;
            spotLengthEngine.Setup(s => s.GetSpotLengthIdByValue(It.IsAny<int>()))
                .Returns(spotLengthId);

            genreRepository.Setup(s => s.GetAllMaestroGenres())
                .Returns(_GetAllGenres());

            var testMediaWeeks = new List<MediaWeek>
            {
                new MediaWeek(849, 463, 1,  DateTime.Parse("2020-03-30T00:00:00"), DateTime.Parse("2020-04-05T00:00:00")),
                new MediaWeek(850, 463, 2,  DateTime.Parse("2020-04-06T00:00:00"), DateTime.Parse("2020-04-12T00:00:00")),
                new MediaWeek(851, 463, 3,  DateTime.Parse("2020-04-13T00:00:00"), DateTime.Parse("2020-04-19T00:00:00")),
                new MediaWeek(852, 463, 4,  DateTime.Parse("2020-04-20T00:00:00"), DateTime.Parse("2020-04-26T00:00:00")),
                new MediaWeek(853, 464, 1,  DateTime.Parse("2020-04-27T00:00:00"), DateTime.Parse("2020-05-03T00:00:00")),
                new MediaWeek(854, 464, 2,  DateTime.Parse("2020-05-04T00:00:00"), DateTime.Parse("2020-05-10T00:00:00")),
                new MediaWeek(855, 464, 3,  DateTime.Parse("2020-05-11T00:00:00"), DateTime.Parse("2020-05-17T00:00:00")),
                new MediaWeek(856, 464, 4,  DateTime.Parse("2020-05-18T00:00:00"), DateTime.Parse("2020-05-24T00:00:00")),
                new MediaWeek(857, 464, 5,  DateTime.Parse("2020-05-25T00:00:00"), DateTime.Parse("2020-05-31T00:00:00")),
                new MediaWeek(858, 465, 1,  DateTime.Parse("2020-06-01T00:00:00"), DateTime.Parse("2020-06-07T00:00:00")),
                new MediaWeek(859, 465, 2,  DateTime.Parse("2020-06-08T00:00:00"), DateTime.Parse("2020-06-14T00:00:00")),
                new MediaWeek(860, 465, 3,  DateTime.Parse("2020-06-15T00:00:00"), DateTime.Parse("2020-06-21T00:00:00")),
                new MediaWeek(861, 465, 4,  DateTime.Parse("2020-06-22T00:00:00"), DateTime.Parse("2020-06-28T00:00:00")),
            };
            mediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(testMediaWeeks);

            var inventory = new List<InventoryExportDto>();
            var getInventoryForExportOpenMarketCalls = new List<Tuple<List<int>, List<int>, List<int>>>();
            inventoryRepository.Setup(s =>
                    s.GetInventoryForExportOpenMarket(It.IsAny<List<int>>(), It.IsAny<List<int>>(),
                        It.IsAny<List<int>>()))
                .Callback<List<int>, List<int>, List<int>>((s, g, w) =>
                    getInventoryForExportOpenMarketCalls.Add(new Tuple<List<int>, List<int>, List<int>>(s, g, w)))
                .Returns(inventory);

            var updatedJobs = new List<InventoryExportJobDto>();
            inventoryExportJobRepository.Setup(s => s.UpdateJob(It.IsAny<InventoryExportJobDto>()))
                .Callback<InventoryExportJobDto>((j) => updatedJobs.Add(j));

            // Register all the repo objects that are now setup.
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryRepository>())
                .Returns(inventoryRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryExportRepository>())
                .Returns(inventoryExportRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryExportJobRepository>())
                .Returns(inventoryExportJobRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IGenreRepository>())
                .Returns(genreRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IStationRepository>())
                .Returns(stationRepository.Object);

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportServiceUnitTestClass(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                fileService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object);

            service.UT_DateTimeNow = testCurrentTimestamp;

            // *** ACT ***/
            var caught = Assert.Throws<InvalidOperationException>(() => service.GenerateExportForOpenMarket(request, TEST_USERNAME));

            // *** Assert ***/
            Assert.AreEqual(1, getInventoryForExportOpenMarketCalls.Count);
            Assert.IsNotNull(caught);
            Assert.AreEqual("No inventory found to export for job 1.", caught.Message);
            Assert.AreEqual(1, updatedJobs.Count);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedJobs[0]));
        }

        [Test]
        [TestCase("Open Market", "OpenMarket")]
        [TestCase("MLB", "MLB")]
        [TestCase("ABC O&O", "ABCO&O")]
        [TestCase("20th Century Fox (Twentieth Century)", "20thCenturyFox(TwentiethCentury)")]
        public void GetInventoryFileName(string inventorySourceName, string expectedResultName)
        {
            var inventorySource = new InventorySource {Name = inventorySourceName };
            var testCurrentTimestamp = new DateTime(2020, 05, 06, 14, 32, 18);
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var service = new InventoryExportServiceUnitTestClass(
                broadcastDataRepositoryFactory.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null)
            {
                UT_DateTimeNow = testCurrentTimestamp
            };
            var expectedResult = $"InventoryExport_{expectedResultName}_20200506_143218.xlsx";

            var result = service.UT_GetInventoryFileName(inventorySource);
            
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void DownloadOpenMarketExportFile()
        {
            /*** Arrange ***/
            // instantiate our mocks.
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var inventoryExportEngine = new Mock<IInventoryExportEngine>();
            var fileService = new Mock<IFileService>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            var daypartCache = new Mock<IDaypartCache>();
            var marketService = new Mock<IMarketService>();

            var inventoryRepository = new Mock<IInventoryRepository>();
            var inventoryExportRepository = new Mock<IInventoryExportRepository>();
            var inventoryExportJobRepository = new Mock<IInventoryExportJobRepository>();
            var genreRepository = new Mock<IGenreRepository>();
            var stationRepository = new Mock<IStationRepository>();

            // load our mocks with our test data.
            inventoryExportJobRepository.Setup(s => s.GetJob(It.IsAny<int>()))
                .Returns(new InventoryExportJobDto {FileName = "TestFileName.xlsx" });

            var getFileStreamCalls = new List<Tuple<string,string>>();
            fileService.Setup(s => s.GetFileStream(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string,string>((s,sf) => getFileStreamCalls.Add(new Tuple<string, string>(s, sf)))
                .Returns(new MemoryStream());

            // Register all the repo objects that are now setup.
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryRepository>())
                .Returns(inventoryRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryExportRepository>())
                .Returns(inventoryExportRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IInventoryExportJobRepository>())
                .Returns(inventoryExportJobRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IGenreRepository>())
                .Returns(genreRepository.Object);
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IStationRepository>())
                .Returns(stationRepository.Object);

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportServiceUnitTestClass(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                fileService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object);

            // *** ACT ***/
            var result = service.DownloadOpenMarketExportFile(1);

            // *** ASSERT ***/
            Assert.AreEqual(1, getFileStreamCalls.Count);
            Assert.AreEqual(@"BroadcastServiceSystemParameter.BroadcastSharedFolder\InventoryExports", getFileStreamCalls[0].Item1);
            Assert.AreEqual("TestFileName.xlsx", getFileStreamCalls[0].Item2);
            Assert.IsNotNull(result);
            Assert.AreEqual("TestFileName.xlsx", result.Item1);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Item3);
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
    }
}