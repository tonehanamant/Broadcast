using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Inventory;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices.Inventory
{
    [TestFixture]
    public class InventoryExportServiceTests
    {
        private readonly IGenreRepository _GenreRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
        private readonly IInventoryRepository _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        private readonly IInventoryExportJobRepository _InventoryExportJobRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryExportJobRepository>();

        private readonly IInventoryExportService _InventoryExportService;
        private readonly InMemoryFileServiceStubb _FileService;

        private readonly int openMarket_InventorySourceId = 1;

        public InventoryExportServiceTests()
        {
            _FileService = new InMemoryFileServiceStubb();

            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(_FileService);

            _InventoryExportService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryExportService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetOpenMarketExportGenreTypesTest()
        {
            using (new TransactionScopeWrapper())
            {
                var genreTypes =
                    _InventoryExportService.GetOpenMarketExportGenreTypes();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(genreTypes));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetOpenMarketExportInventoryQuartersTest()
        {
            using (new TransactionScopeWrapper())
            {
                var quarters = _InventoryExportService.GetOpenMarketExportInventoryQuarters(openMarket_InventorySourceId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarters));
            }
        }

        [Test]
        public void GenerateExportForOpenMarketAndDownloadIt()
        {
            const string testUser = "TestUser";
            var testRequest = new InventoryExportRequestDto
            {
                Genre = InventoryExportGenreTypeEnum.News,
                Quarter = new QuarterDetailDto
                {
                    Year = 2018,
                    Quarter = 4,
                    StartDate = new DateTime(2018, 03, 25),
                    EndDate = new DateTime(2018, 12, 30)
                }
            };

            var jobId = -1;
            InventoryExportJobDto job = null;

            var programs = _GetProgramsForTest();

            _FileService.CreatedFileStreams.Clear();
            string generatedFileContent;
            using (new TransactionScopeWrapper())
            {
                // setup the test data
                using (var transCreateData = new TransactionScopeWrapper())
                {
                    _InventoryRepository.CreateInventoryPrograms(programs, DateTime.Now);
                    transCreateData.Complete();
                }
                // generate the file
                jobId = _InventoryExportService.GenerateExportForOpenMarket(testRequest, testUser);
                job = _InventoryExportJobRepository.GetJob(jobId);

                // download the file 
                var downloadedFileResult = _InventoryExportService.DownloadOpenMarketExportFile(jobId);
                using (var reader = new StreamReader(downloadedFileResult.Item2))
                {
                    generatedFileContent = reader.ReadToEnd();
                }
            }

            // Verify the job is as expected
            Assert.AreEqual(BackgroundJobProcessingStatus.Succeeded, job.Status);

            // folderPath and fileName are validated in unit tests.
            // here we will only validate the file content.
            // since that is Excel nonsense we will just verify the length.
            Assert.AreEqual(3540, generatedFileContent.Length);
        }

        /// <summary>
        /// Gets the programs data we will insert for our test.
        /// </summary>
        private List<StationInventoryManifestDaypartProgram> _GetProgramsForTest()
        {
            var daypartIds = new[] { 576682, 576683, 576683, 576683, 576683, 576683, 576684, 576685, 576685, 576685, 576685, 576685 };

            var genres = _GenreRepository.GetAllMaestroGenres().ToList();
            var notNewsGenreId = genres
                .Where(g => g.Display.ToUpper().Equals("NEWS") == false)
                .Select(g => g.Id)
                .ToArray();
            var newsGenreIds = genres
                .Where(g => g.Display.ToUpper().Equals("NEWS"))
                .Select(g => g.Id)
                .ToArray();

            var startDate = new DateTime(2018, 10, 01);
            var endDate = new DateTime(2018, 10, 31);
            var rng = new Random();

            var programs = daypartIds.Select(i =>
                new StationInventoryManifestDaypartProgram
                {
                    StationInventoryManifestDaypartId = i,
                    ProgramName = i % 2 == 0 ? $"News Program {i}" : $"Non News Program {i}",
                    ShowType = "SER",
                    SourceGenreId = 25,
                    GenreSourceId = 2,
                    MaestroGenreId = i % 2 == 0 ? newsGenreIds[rng.Next(0, newsGenreIds.Length)] : notNewsGenreId[rng.Next(0, notNewsGenreId.Length)], // news : not-news
                    StartDate = startDate,
                    EndDate = endDate,
                    StartTime = 1200,
                    EndTime = 1220,
                    CreatedDate = startDate
                }
            ).ToList();
            return programs;
        }
    }
}