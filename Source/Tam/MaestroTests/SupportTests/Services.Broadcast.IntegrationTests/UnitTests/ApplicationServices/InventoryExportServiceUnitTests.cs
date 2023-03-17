
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Services.Broadcast.ApplicationServices.Inventory;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Clients;
using Tam.Maestro.Common.DataLayer;
using Services.Broadcast.Entities.InventorySummary;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    /// <summary>
    /// Test the InventoryExportService
    /// </summary>
    public class InventoryExportServiceUnitTests
    {
        private const string TEST_USERNAME = "testUser";
        private const string TEST_TEMPLATE_PATH = @".\Files\Excel templates";

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
            List<int> genreIds = new List<int> { 34 };
            var testCurrentTimestamp = new DateTime(2020, 05, 06, 14, 32, 18);

            // instantiate our mocks.
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var inventoryExportEngine = new Mock<IInventoryExportEngine>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            var daypartCache = new Mock<IDaypartCache>();
            var marketService = new Mock<IMarketService>();

            var inventoryRepository = new Mock<IInventoryRepository>();
            var inventoryExportRepository = new Mock<IInventoryExportRepository>();
            var inventoryExportJobRepository = new Mock<IInventoryExportJobRepository>();
            var genreRepository = new Mock<IGenreRepository>();
            var stationRepository = new Mock<IStationRepository>();
            var audienceRepository = new Mock<IBroadcastAudienceRepository>();
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            configurationSettingsHelper.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");
            var inventoryManagementClient = new Mock<IInventoryManagementApiClient>();

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
                .Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns(new MediaMonth(1, 2020, 12, "NotSure", new DateTime(2020, 12, 1), new DateTime(2020, 12, 31)));

            mediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(testMediaWeeks);

            var inventory = new List<InventoryExportDto>
            {
                _GetInventoryExportDto(1,1,849, 1, "One", "One"),
                _GetInventoryExportDto(2,2,850, 2, "Two", "Two"),
            };
            var getInventoryForExportOpenMarketCalls = new List<Tuple<List<int>, List<int>, List<int>>>();
            inventoryExportRepository.Setup(s =>
                    s.GetInventoryForExportOpenMarket(It.IsAny<List<int>>(), It.IsAny<List<int>>(),
                        It.IsAny<List<int>>()))
                .Callback<List<int>, List<int>, List<int>>((s, g, w) =>
                    getInventoryForExportOpenMarketCalls.Add(new Tuple<List<int>, List<int>, List<int>>(s, g, w)))
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

            object[][] headers = { new object[] { "01/01", "02/02" } };
            inventoryExportEngine.Setup(s => s.GetInventoryTableWeeklyColumnHeaders(It.IsAny<List<DateTime>>()))
                .Returns(headers);

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

            const int testDataRowCount = 10;
            var tableData = _GetTestInventoryTableData(testDataRowCount);
            inventoryExportEngine.Setup(s => s.GetInventoryTableData(It.IsAny<List<InventoryExportLineDetail>>(),
                    It.IsAny<List<DisplayBroadcastStation>>(), It.IsAny<List<MarketCoverage>>(),
                    It.IsAny<List<int>>(), It.IsAny<Dictionary<int, DisplayDaypart>>(),
                    It.IsAny<List<LookupDto>>(), It.IsAny<List<LookupDto>>()))
                .Returns(tableData);

            inventoryExportEngine.Setup(s => s.GetInventoryExportFileName(It.IsAny<InventoryExportGenreTypeEnum>(), It.IsAny<QuarterDetailDto>()))
                .Returns("TestFileName.xlsx");

            var fileServiceCreateFilesCalled = new List<Tuple<string, string, Stream>>();

            var updatedJobs = new List<InventoryExportJobDto>();
            inventoryExportJobRepository.Setup(s => s.UpdateJob(It.IsAny<InventoryExportJobDto>()))
                .Callback<InventoryExportJobDto>((j) => updatedJobs.Add(j));

            audienceRepository.Setup(s => s.GetAudienceDtosById(It.IsAny<List<int>>()))
                .Returns(new List<LookupDto>());

            var nsiPostingBooksService = new Mock<INsiPostingBookService>();
            var shareBookId = 5;
            nsiPostingBooksService.Setup(s => s.GetLatestNsiPostingBookForMonthContainingDate(It.IsAny<DateTime>()))
                .Returns(shareBookId);

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
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IBroadcastAudienceRepository>())
                .Returns(audienceRepository.Object);

            var dateTimeEngine = new Mock<IDateTimeEngine>();
            dateTimeEngine.Setup(s => s.GetCurrentMoment())
                .Returns(testCurrentTimestamp);

            var sharedFolderService = new Mock<ISharedFolderService>();
            var savedSharedFolderFiles = new List<SharedFolderFile>();
            var savedSharedFolderFileId = new Guid("F2ABCE13-CA61-4323-8D87-D347E9F102DD");
            sharedFolderService.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Callback<SharedFolderFile>((f) => savedSharedFolderFiles.Add(f))
                .Returns(savedSharedFolderFileId);

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportService(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                sharedFolderService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object,
                nsiPostingBooksService.Object,
                dateTimeEngine.Object,
                featureToggle.Object, configurationSettingsHelper.Object, inventoryManagementClient.Object);

            // *** ACT ***/
            var result = service.GenerateExportForOpenMarket(request, TEST_USERNAME, TEST_TEMPLATE_PATH);

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
            Assert.AreEqual(1, getInventoryForExportOpenMarketCalls.Count);
            Assert.AreEqual(spotLengthId, getInventoryForExportOpenMarketCalls[0].Item1[0]); // spotlengthid
            Assert.AreEqual(34, getInventoryForExportOpenMarketCalls[0].Item2[0]); // genre id should be news only
            // verify the calculated call
            Assert.AreEqual(1, calculateCalledCount);

            // verify the saved job
            //updatedJobs
            Assert.AreEqual(1, updatedJobs.Count);
            Assert.AreEqual(BackgroundJobProcessingStatus.Succeeded, updatedJobs[0].Status);
            Assert.IsNotNull(updatedJobs[0].CompletedAt);
            Assert.AreEqual(savedSharedFolderFileId, updatedJobs[0].SharedFolderFileId);

            // verify the saved file
            Assert.AreEqual(1, savedSharedFolderFiles.Count);
            Assert.IsTrue(savedSharedFolderFiles[0].FolderPath.EndsWith(@"\InventoryExports"));
            Assert.AreEqual("TestFileName.xlsx", savedSharedFolderFiles[0].FileNameWithExtension);
            Assert.IsNotNull(savedSharedFolderFiles[0].FileContent);
            Assert.AreEqual(SharedFolderFileUsage.InventoryExport, savedSharedFolderFiles[0].FileUsage);

            Assert.AreEqual(0, fileServiceCreateFilesCalled.Count);
           
        }

        [Test]
        public void GenerateExportForOpenMarket_Unenriched()
        {
            /*** Arrange ***/
            var request = new InventoryExportRequestDto
            {
                Genre = InventoryExportGenreTypeEnum.NotEnriched,
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
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            var daypartCache = new Mock<IDaypartCache>();
            var marketService = new Mock<IMarketService>();
            var nsiPostingBooksService = new Mock<INsiPostingBookService>();

            var inventoryRepository = new Mock<IInventoryRepository>();
            var inventoryExportRepository = new Mock<IInventoryExportRepository>();
            var inventoryExportJobRepository = new Mock<IInventoryExportJobRepository>();
            var genreRepository = new Mock<IGenreRepository>();
            var stationRepository = new Mock<IStationRepository>();
            var audienceRepository = new Mock<IBroadcastAudienceRepository>();
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            configurationSettingsHelper.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");
            var inventoryManagementClient = new Mock<IInventoryManagementApiClient>();

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
                .Setup(s => s.GetMediaMonthById(It.IsAny<int>()))
                .Returns(new MediaMonth(1, 2020, 12, "NotSure", new DateTime(2020, 12, 1), new DateTime(2020, 12, 31)));

            mediaMonthAndWeekAggregateCache
                .Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(testMediaWeeks);

            var inventory = new List<InventoryExportDto>
            {
                _GetInventoryExportDto(1,1,849, 1, "One", "One"),
                _GetInventoryExportDto(2,2,850, 2, "Two", "Two"),
            };
            var getInventoryForExportOpenMarketCalls = new List<Tuple<List<int>, List<int>, List<int>>>();
            inventoryExportRepository.Setup(s =>
                    s.GetInventoryForExportOpenMarket(It.IsAny<List<int>>(), It.IsAny<List<int>>(),
                        It.IsAny<List<int>>()))
                .Callback<List<int>, List<int>, List<int>>((s, g, w) =>
                    getInventoryForExportOpenMarketCalls.Add(new Tuple<List<int>, List<int>, List<int>>(s, g, w)))
                .Returns(inventory);

            var getInventoryForExportOpenMarketNotEnriched = new List<Tuple<List<int>, List<int>>>();
            inventoryExportRepository.Setup(s =>
                    s.GetInventoryForExportOpenMarketNotEnriched(It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .Callback<List<int>, List<int>>((s, w) =>
                    getInventoryForExportOpenMarketNotEnriched.Add(new Tuple<List<int>, List<int>>(s, w)))
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

            object[][] headers = { new object[] { "01/01", "02/02" } };
            inventoryExportEngine.Setup(s => s.GetInventoryTableWeeklyColumnHeaders(It.IsAny<List<DateTime>>()))
                .Returns(headers);

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

            const int testDataRowCount = 10;
            var tableData = _GetTestInventoryTableData(testDataRowCount);
            inventoryExportEngine.Setup(s => s.GetInventoryTableData(It.IsAny<List<InventoryExportLineDetail>>(),
                    It.IsAny<List<DisplayBroadcastStation>>(), It.IsAny<List<MarketCoverage>>(),
                    It.IsAny<List<int>>(), It.IsAny<Dictionary<int, DisplayDaypart>>(),
                    It.IsAny<List<LookupDto>>(), It.IsAny<List<LookupDto>>()))
                .Returns(tableData);

            inventoryExportEngine.Setup(s => s.GetInventoryExportFileName(It.IsAny<InventoryExportGenreTypeEnum>(), It.IsAny<QuarterDetailDto>()))
                .Returns("TestFileName.xlsx");

            var fileServiceCreateFilesCalled = new List<Tuple<string, string, Stream>>();

            var updatedJobs = new List<InventoryExportJobDto>();
            inventoryExportJobRepository.Setup(s => s.UpdateJob(It.IsAny<InventoryExportJobDto>()))
                .Callback<InventoryExportJobDto>((j) => updatedJobs.Add(j));

            audienceRepository.Setup(s => s.GetAudienceDtosById(It.IsAny<List<int>>()))
                .Returns(new List<LookupDto>());

            var shareBookId = 5;
            nsiPostingBooksService.Setup(s => s.GetLatestNsiPostingBookForMonthContainingDate(It.IsAny<DateTime>()))
                .Returns(shareBookId);

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
            broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IBroadcastAudienceRepository>())
                .Returns(audienceRepository.Object);

            var sharedFolderService = new Mock<ISharedFolderService>();
            var savedSharedFolderFiles = new List<SharedFolderFile>();
            var savedSharedFolderFileId = new Guid("F2ABCE13-CA61-4323-8D87-D347E9F102DD");
            sharedFolderService.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Callback<SharedFolderFile>((f) => savedSharedFolderFiles.Add(f))
                .Returns(savedSharedFolderFileId);

            var dateTimeEngine = new Mock<IDateTimeEngine>();
            dateTimeEngine.Setup(s => s.GetCurrentMoment())
                .Returns(testCurrentTimestamp);

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportService(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                sharedFolderService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object,
                nsiPostingBooksService.Object,
                dateTimeEngine.Object,
                featureToggle.Object, configurationSettingsHelper.Object, inventoryManagementClient.Object);

            // *** ACT ***/
            var result = service.GenerateExportForOpenMarket(request, TEST_USERNAME, TEST_TEMPLATE_PATH);

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
            Assert.AreEqual(0, getInventoryForExportOpenMarketCalls.Count);
            Assert.AreEqual(1, getInventoryForExportOpenMarketNotEnriched.Count);
            Assert.AreEqual(spotLengthId, getInventoryForExportOpenMarketNotEnriched[0].Item1[0]); // spotlengthid

            // verify the calculated call
            Assert.AreEqual(1, calculateCalledCount);
            // verify the saved job
            //updatedJobs
            Assert.AreEqual(1, updatedJobs.Count);
            Assert.AreEqual(BackgroundJobProcessingStatus.Succeeded, updatedJobs[0].Status);
            Assert.IsNotNull(updatedJobs[0].CompletedAt);
            Assert.AreEqual(savedSharedFolderFileId, updatedJobs[0].SharedFolderFileId);

            // verify the saved file
            Assert.AreEqual(1, savedSharedFolderFiles.Count);
            Assert.IsTrue(savedSharedFolderFiles[0].FolderPath.EndsWith(@"\InventoryExports"));
            Assert.AreEqual("TestFileName.xlsx", savedSharedFolderFiles[0].FileNameWithExtension);
            Assert.IsNotNull(savedSharedFolderFiles[0].FileContent);
            Assert.AreEqual(SharedFolderFileUsage.InventoryExport, savedSharedFolderFiles[0].FileUsage);

            Assert.AreEqual(0, fileServiceCreateFilesCalled.Count);
        }

        private InventoryExportDto _GetInventoryExportDto(int inventoryId, int mediaWeekId, int stationId, int daypartId, string programNameSeed, string inventoryProgramNameSeed)
        {
            const int hhAudienceId = 31;
            var programName = string.IsNullOrWhiteSpace(programNameSeed) ? null : $"ProgramName{programNameSeed}";
            var inventoryProgramName = string.IsNullOrWhiteSpace(inventoryProgramNameSeed) ? null : $"InventoryProgramName{inventoryProgramNameSeed}";

            var item = new InventoryExportDto()
            {
                InventoryId = inventoryId,
                MediaWeekId = mediaWeekId,
                StationId = stationId,
                DaypartId = daypartId,
                HhImpressionsProjected = 10000,
                SpotCost = 20,
                ProgramName = programName,
                InventoryProgramName = inventoryProgramName,
                ProvidedAudiences = new List<InventoryExportAudienceDto>
                {
                    new InventoryExportAudienceDto {AudienceId = hhAudienceId, Impressions = 20000}
                }
            };

            return item;
        }

        private object[][] _GetTestInventoryTableData(int count)
        {
            const int columnCount = 10;
            var rows = new List<object[]>();

            for (var i = 0; i < count; i++)
            {
                var columns = Enumerable.Range(1, columnCount).ToList().Select(s => (object)$"Column{s}").ToArray();
                rows.Add(columns);
            }

            return rows.ToArray();
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
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            configurationSettingsHelper.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");
            var inventoryManagementClient = new Mock<IInventoryManagementApiClient>();

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
            inventoryExportRepository.Setup(s =>
                    s.GetInventoryForExportOpenMarket(It.IsAny<List<int>>(), It.IsAny<List<int>>(),
                        It.IsAny<List<int>>()))
                .Callback<List<int>, List<int>, List<int>>((s, g, w) =>
                    getInventoryForExportOpenMarketCalls.Add(new Tuple<List<int>, List<int>, List<int>>(s, g, w)))
                .Returns(inventory);

            var updatedJobs = new List<InventoryExportJobDto>();
            inventoryExportJobRepository.Setup(s => s.UpdateJob(It.IsAny<InventoryExportJobDto>()))
                .Callback<InventoryExportJobDto>((j) => updatedJobs.Add(j));

            var nsiPostingBooksService = new Mock<INsiPostingBookService>();
            var shareBookId = 5;
            nsiPostingBooksService.Setup(s => s.GetLatestNsiPostingBookForMonthContainingDate(It.IsAny<DateTime>()))
                .Returns(shareBookId);

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

            var sharedFolderService = new Mock<ISharedFolderService>();
            var dateTimeEngine = new Mock<IDateTimeEngine>();
            dateTimeEngine.Setup(s => s.GetCurrentMoment())
                .Returns(testCurrentTimestamp);

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportService(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                sharedFolderService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object,
                nsiPostingBooksService.Object,
                dateTimeEngine.Object,
                featureToggle.Object, configurationSettingsHelper.Object, inventoryManagementClient.Object);

            // *** ACT ***/
            var caught = Assert.Throws<InvalidOperationException>(() => service.GenerateExportForOpenMarket(request, TEST_USERNAME, TEST_TEMPLATE_PATH));

            // *** Assert ***/
            Assert.AreEqual(1, getInventoryForExportOpenMarketCalls.Count);
            Assert.IsNotNull(caught);
            Assert.AreEqual("No 'News' inventory found to export for Q2 2020.", caught.Message);
            Assert.AreEqual(1, updatedJobs.Count);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedJobs[0]));
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
            bool existInSharedFolderService = true;
            var inventoryRepository = new Mock<IInventoryRepository>();
            var inventoryExportRepository = new Mock<IInventoryExportRepository>();
            var inventoryExportJobRepository = new Mock<IInventoryExportJobRepository>();
            var genreRepository = new Mock<IGenreRepository>();
            var stationRepository = new Mock<IStationRepository>();
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            configurationSettingsHelper.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");
            var inventoryManagementClient = new Mock<IInventoryManagementApiClient>();

            var savedFileGuid = existInSharedFolderService
                ? new Guid("4FAED53D-759A-4088-9A33-DE2C9107CCC5")
                : (Guid?)null;

            // load our mocks with our test data.
            inventoryExportJobRepository.Setup(s => s.GetJob(It.IsAny<int>()))
                .Returns(new InventoryExportJobDto { FileName = "TestFileName.xlsx", SharedFolderFileId = savedFileGuid });

            var getFileStreamCalls = new List<Tuple<string, string>>();
            fileService.Setup(s => s.GetFileStream(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((s, sf) => getFileStreamCalls.Add(new Tuple<string, string>(s, sf)))
                .Returns(new MemoryStream());

            var nsiPostingBooksService = new Mock<INsiPostingBookService>();
            var shareBookId = 5;
            nsiPostingBooksService.Setup(s => s.GetLatestNsiPostingBookForMonthContainingDate(It.IsAny<DateTime>()))
                .Returns(shareBookId);

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

            var sharedFolderService = new Mock<ISharedFolderService>();
            var dateTimeEngine = new Mock<IDateTimeEngine>();

            sharedFolderService.Setup(s => s.GetFile(It.IsAny<Guid>()))
                .Returns(new SharedFolderFile { FileName = "TestFileName", FileExtension = ".xlsx", FileContent = new MemoryStream() });

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportService(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                sharedFolderService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object,
                nsiPostingBooksService.Object,
                dateTimeEngine.Object,
                featureToggle.Object,
                configurationSettingsHelper.Object, inventoryManagementClient.Object);

            // *** ACT ***/
            var result = service.DownloadOpenMarketExportFile(1);

            // *** ASSERT ***/
            Assert.IsNotNull(result);
            Assert.AreEqual("TestFileName.xlsx", result.Item1);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Item3);

            inventoryExportJobRepository.Verify(s => s.GetJob(It.IsAny<int>()), Times.Once);

            var sharedFolderServiceGetFileTimesCalled = existInSharedFolderService ? Times.Once() : Times.Never();
            sharedFolderService.Verify(s => s.GetFile(It.IsAny<Guid>()), sharedFolderServiceGetFileTimesCalled);

            var shouldHaveCheckedFileService = !existInSharedFolderService;
            var fileServiceGetFileStreamTimesCalled = shouldHaveCheckedFileService ? Times.Once() : Times.Never();
            fileService.Verify(s => s.GetFileStream(It.IsAny<string>(), It.IsAny<string>()), fileServiceGetFileStreamTimesCalled);

            // verify correct path.
            if (shouldHaveCheckedFileService)
            {
                Assert.IsTrue(getFileStreamCalls[0].Item1.EndsWith(@"\InventoryExports"));
                Assert.AreEqual("TestFileName.xlsx", getFileStreamCalls[0].Item2);
            }
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



        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetOpenMarketExportInventoryQuartersTest_WhenfeatureToggleOn()
        {
            // instantiate our mocks.
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var inventoryExportEngine = new Mock<IInventoryExportEngine>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            var daypartCache = new Mock<IDaypartCache>();
            var marketService = new Mock<IMarketService>();

            var inventoryRepository = new Mock<IInventoryRepository>();
            var inventoryExportRepository = new Mock<IInventoryExportRepository>();
            var inventoryExportJobRepository = new Mock<IInventoryExportJobRepository>();
            var genreRepository = new Mock<IGenreRepository>();
            var stationRepository = new Mock<IStationRepository>();
            var audienceRepository = new Mock<IBroadcastAudienceRepository>();
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            configurationSettingsHelper.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");
            var inventoryManagementClient = new Mock<IInventoryManagementApiClient>();
            var nsiPostingBooksService = new Mock<INsiPostingBookService>();
            var sharedFolderService = new Mock<ISharedFolderService>();
            var dateTimeEngine = new Mock<IDateTimeEngine>();

            //Adding mock data to required repos
            featureToggle.Setup(s =>
                    s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION))
                .Returns(true);
            inventoryManagementClient.Setup(s => s.GetOpenMarketExportInventoryQuarters(It.IsAny<int>())).Returns(getQurters);

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportService(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                sharedFolderService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object,
                nsiPostingBooksService.Object,
                dateTimeEngine.Object,
                featureToggle.Object, configurationSettingsHelper.Object, inventoryManagementClient.Object);
            using (new TransactionScopeWrapper())
            {
                var quarters = service.GetOpenMarketExportInventoryQuarters(7);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarters));
            }
        }

        private InventoryQuartersDto getQurters()
        {
            var quarters = new InventoryQuartersDto
            {
                DefaultQuarter = new QuarterDetailDto
                {
                    EndDate = new DateTime(2021, 01, 04),
                    StartDate = new DateTime(2022, 01, 04),
                    Quarter = 4,
                    Year = 2022
                }
            };
            return quarters;
        }

        private List<LookupDto> _getGenres()
        {
            return new List<LookupDto>
            {
                new LookupDto
                {
                    Id = 1,
                    Display = "Enrich"
                }
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Getgeneres_WhenInventoryMicroservicefeatureToggleOn()
        {
            // instantiate our mocks.
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var inventoryExportEngine = new Mock<IInventoryExportEngine>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            var daypartCache = new Mock<IDaypartCache>();
            var marketService = new Mock<IMarketService>();

            var inventoryRepository = new Mock<IInventoryRepository>();
            var inventoryExportRepository = new Mock<IInventoryExportRepository>();
            var inventoryExportJobRepository = new Mock<IInventoryExportJobRepository>();
            var genreRepository = new Mock<IGenreRepository>();
            var stationRepository = new Mock<IStationRepository>();
            var audienceRepository = new Mock<IBroadcastAudienceRepository>();
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            configurationSettingsHelper.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");
            var inventoryManagementClient = new Mock<IInventoryManagementApiClient>();
            var nsiPostingBooksService = new Mock<INsiPostingBookService>();
            var sharedFolderService = new Mock<ISharedFolderService>();
            var dateTimeEngine = new Mock<IDateTimeEngine>();

            //Adding mock data to required repos
            featureToggle.Setup(s =>
                    s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION))
                .Returns(true);
            inventoryManagementClient.Setup(s => s.GetInventoryGenreTypes()).Returns(_getGenres());

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportService(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                sharedFolderService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object,
                nsiPostingBooksService.Object,
                dateTimeEngine.Object,
                featureToggle.Object, configurationSettingsHelper.Object, inventoryManagementClient.Object);
            using (new TransactionScopeWrapper())
            {
                var GenreTypes = service.GetOpenMarketExportGenreTypes();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(GenreTypes));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenrateExportForOpenMarket_WhenInventoryMicroservicefeatureToggleOn()
        {
            // instantiate our mocks.
            var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var inventoryExportEngine = new Mock<IInventoryExportEngine>();
            var spotLengthEngine = new Mock<ISpotLengthEngine>();
            var daypartCache = new Mock<IDaypartCache>();
            var marketService = new Mock<IMarketService>();

            var inventoryRepository = new Mock<IInventoryRepository>();
            var inventoryExportRepository = new Mock<IInventoryExportRepository>();
            var inventoryExportJobRepository = new Mock<IInventoryExportJobRepository>();
            var genreRepository = new Mock<IGenreRepository>();
            var stationRepository = new Mock<IStationRepository>();
            var audienceRepository = new Mock<IBroadcastAudienceRepository>();
            var featureToggle = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            configurationSettingsHelper.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");
            var inventoryManagementClient = new Mock<IInventoryManagementApiClient>();
            var nsiPostingBooksService = new Mock<INsiPostingBookService>();
            var sharedFolderService = new Mock<ISharedFolderService>();
            var dateTimeEngine = new Mock<IDateTimeEngine>();

            //Adding mock data to required repos
            featureToggle.Setup(s =>
                    s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION))
                .Returns(true);

            var inventoryRequest = new InventoryExportRequestDto
            {
                Genre = InventoryExportGenreTypeEnum.NotEnriched,
                Quarter = new QuarterDetailDto
                {
                    EndDate = new DateTime(2021, 01, 04),
                    StartDate = new DateTime(2022, 01, 04),
                    Quarter = 4,
                    Year = 2022
                }
            };
            int expectedResult = 123;
            string userName = "TestUSer";
            string templatePath = "~/test";
            inventoryManagementClient.Setup(s => s.GenerateExportForOpenMarket(It.IsAny<InventoryExportRequestDto>())).Returns(expectedResult);

            // instantiate our test server with all our setup mocks.
            var service = new InventoryExportService(broadcastDataRepositoryFactory.Object,
                quarterCalculationEngine.Object,
                mediaMonthAndWeekAggregateCache.Object,
                inventoryExportEngine.Object,
                sharedFolderService.Object,
                spotLengthEngine.Object,
                daypartCache.Object,
                marketService.Object,
                nsiPostingBooksService.Object,
                dateTimeEngine.Object,
                featureToggle.Object, configurationSettingsHelper.Object, inventoryManagementClient.Object);
            using (new TransactionScopeWrapper())
            {
                var result = service.GenerateExportForOpenMarket(inventoryRequest, userName, templatePath);

                //assertion
                Assert.NotNull(result);
                Assert.AreEqual(expectedResult, result);
            }
        }
    }
}