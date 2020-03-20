using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    [TestFixture]
    [Category("short_running")]
    public class InventoryProgramsResultImportUnitTests
    {
        private Mock<IInventoryRepository> _InventoryRepo = new Mock<IInventoryRepository>();
        private Mock<IInventoryFileRepository> _InventoryFileRepo = new Mock<IInventoryFileRepository>();
        private Mock<IInventoryProgramsByFileJobsRepository> _InventoryProgramsByFileJobsRepo = new Mock<IInventoryProgramsByFileJobsRepository>();
        private Mock<IProgramGuideApiClient> _ProgramGuidClient = new Mock<IProgramGuideApiClient>();
        private Mock<IStationMappingService> _StationMappingService = new Mock<IStationMappingService>();
        private Mock<IGenreCache> _GenreCacheMock = new Mock<IGenreCache>();

        private Mock<IFileService> _FileService = new Mock<IFileService>();
        private Mock<IEmailerService> _EmailerService = new Mock<IEmailerService>();

        [Test]
        public void ImportResultsFile()
        {
            /*** Arrange ***/
            const string fileName = @"Results_ProgramGuideInventoryExportFile_20200307_172832.csv";

            const string filesDirectoryPath = @".\Files\InventoryProgramsResultsImport";
            var filePath = Path.Combine(filesDirectoryPath, fileName);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var deleteCalls = new List<Tuple<List<int>, DateTime, DateTime>>();
            _InventoryRepo.Setup(s =>
                    s.DeleteInventoryPrograms(It.IsAny<List<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<List<int>, DateTime, DateTime>((ids, s, e) => deleteCalls.Add(new Tuple<List<int>, DateTime, DateTime>(ids, s, e)));

            var savedCalls = new List<List<StationInventoryManifestDaypartProgram>>();
            _InventoryRepo.Setup(s =>
                    s.UpdateInventoryPrograms(It.IsAny<List<StationInventoryManifestDaypartProgram>>(),
                        It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((s, d) => savedCalls.Add(s));

            _InventoryRepo.Setup(s => s.GetStationInventoryManifestsByIds(It.IsAny<List<int>>()))
                .Returns(new List<StationInventoryManifest>());

            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()));

            var fileServiceCreateCalls = new List<string>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s,a) => fileServiceCreateCalls.Add(s));

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Exported 4 lines from the file."));
            Assert.IsTrue(result.Contains("Extracted and saved 4 program records."));
            Assert.IsTrue(result.Contains(@"File archived to 'testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Completed\Results_ProgramGuideInventoryExportFile_20200307_172832.csv'."));

            // validate we deleted
            Assert.AreEqual(3, deleteCalls.Count);
            Assert.AreEqual(2, deleteCalls[0].Item1.Count);
            Assert.IsTrue(deleteCalls[0].Item1.Contains(192899));
            Assert.IsTrue(deleteCalls[0].Item1.Contains(192898));
            Assert.AreEqual(1, deleteCalls[1].Item1.Count);
            Assert.IsTrue(deleteCalls[1].Item1.Contains(192900));
            Assert.AreEqual(1, deleteCalls[2].Item1.Count);
            Assert.IsTrue(deleteCalls[2].Item1.Contains(192902));

            // validate we saved
            Assert.AreEqual(1, savedCalls.Count);
            Assert.AreEqual(4, savedCalls[0].Count);

            Assert.AreEqual(194936, savedCalls[0][0].StationInventoryManifestDaypartId);
            Assert.AreEqual("Tacoma FD", savedCalls[0][0].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][0].ShowType);
            Assert.AreEqual(2, savedCalls[0][0].GenreSourceId);
            Assert.AreEqual(2, savedCalls[0][0].MaestroGenreId);
            Assert.AreEqual("2019-03-20", savedCalls[0][0].StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-04-20", savedCalls[0][0].EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(39600, savedCalls[0][0].StartTime);
            Assert.AreEqual(46799, savedCalls[0][0].EndTime);
            
            Assert.AreEqual(194937, savedCalls[0][1].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Bang Theory", savedCalls[0][1].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][1].ShowType);
            Assert.AreEqual(2, savedCalls[0][1].GenreSourceId);
            Assert.AreEqual(2, savedCalls[0][1].MaestroGenreId);
            Assert.AreEqual("2019-03-06", savedCalls[0][1].StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-04-24", savedCalls[0][1].EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(43200, savedCalls[0][1].StartTime);
            Assert.AreEqual(50399, savedCalls[0][1].EndTime);

            Assert.AreEqual(194938, savedCalls[0][2].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Brother", savedCalls[0][2].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][2].ShowType);
            Assert.AreEqual(2, savedCalls[0][2].GenreSourceId);
            Assert.AreEqual(2, savedCalls[0][2].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][2].StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-06-18", savedCalls[0][2].EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(39600, savedCalls[0][2].StartTime);
            Assert.AreEqual(48599, savedCalls[0][2].EndTime);

            Assert.AreEqual(194940, savedCalls[0][3].StationInventoryManifestDaypartId);
            Assert.AreEqual("Super Troopers", savedCalls[0][3].ProgramName);
            Assert.AreEqual("Movie", savedCalls[0][3].ShowType);
            Assert.AreEqual(2, savedCalls[0][3].GenreSourceId);
            Assert.AreEqual(2, savedCalls[0][3].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][3].StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-05-01", savedCalls[0][3].EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(50400, savedCalls[0][3].StartTime);
            Assert.AreEqual(68399, savedCalls[0][3].EndTime);

            // validate we moved the file
            Assert.AreEqual(1, fileServiceCreateCalls.Count);
            Assert.IsTrue(fileServiceCreateCalls[0].Contains("Completed"));
        }

        [Test]
        public void ImportResultsFile_DaypartHasComma()
        {
            /*** Arrange ***/
            const string fileName = @"Results_ProgramGuideInventoryExportFile_DaypartQuotes.csv";

            const string filesDirectoryPath = @".\Files\InventoryProgramsResultsImport";
            var filePath = Path.Combine(filesDirectoryPath, fileName);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var deleteCalls = new List<Tuple<List<int>, DateTime, DateTime>>();
            _InventoryRepo.Setup(s =>
                    s.DeleteInventoryPrograms(It.IsAny<List<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<List<int>, DateTime, DateTime>((ids, s, e) => deleteCalls.Add(new Tuple<List<int>, DateTime, DateTime>(ids, s, e)));

            var savedCalls = new List<List<StationInventoryManifestDaypartProgram>>();
            _InventoryRepo.Setup(s =>
                    s.UpdateInventoryPrograms(It.IsAny<List<StationInventoryManifestDaypartProgram>>(),
                        It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((s, d) => savedCalls.Add(s));

            _InventoryRepo.Setup(s => s.GetStationInventoryManifestsByIds(It.IsAny<List<int>>()))
                .Returns(new List<StationInventoryManifest>());

            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()));

            var fileServiceCreateCalls = new List<string>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s, a) => fileServiceCreateCalls.Add(s));

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Exported 4 lines from the file."));
            Assert.IsTrue(result.Contains("Extracted and saved 4 program records."));
            Assert.IsTrue(result.Contains(@"File archived to 'testSettingBroadcastSharedDirectoryPath\ProgramGuideInterfaceDirectory\Completed\Results_ProgramGuideInventoryExportFile_DaypartQuotes.csv'."));

            // validate we deleted
            Assert.AreEqual(3, deleteCalls.Count);
            Assert.AreEqual(2, deleteCalls[0].Item1.Count);
            Assert.IsTrue(deleteCalls[0].Item1.Contains(192899));
            Assert.IsTrue(deleteCalls[0].Item1.Contains(192898));
            Assert.AreEqual(1, deleteCalls[1].Item1.Count);
            Assert.IsTrue(deleteCalls[1].Item1.Contains(192900));
            Assert.AreEqual(1, deleteCalls[2].Item1.Count);
            Assert.IsTrue(deleteCalls[2].Item1.Contains(192902));

            // validate we saved
            Assert.AreEqual(1, savedCalls.Count);
            Assert.AreEqual(4, savedCalls[0].Count);

            Assert.AreEqual(194936, savedCalls[0][0].StationInventoryManifestDaypartId);
            Assert.AreEqual("Tacoma FD", savedCalls[0][0].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][0].ShowType);
            Assert.AreEqual(2, savedCalls[0][0].GenreSourceId);
            Assert.AreEqual(2, savedCalls[0][0].MaestroGenreId);
            Assert.AreEqual("2019-03-20", savedCalls[0][0].StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-04-20", savedCalls[0][0].EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(39600, savedCalls[0][0].StartTime);
            Assert.AreEqual(46799, savedCalls[0][0].EndTime);

            Assert.AreEqual(194937, savedCalls[0][1].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Bang Theory", savedCalls[0][1].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][1].ShowType);
            Assert.AreEqual(2, savedCalls[0][1].GenreSourceId);
            Assert.AreEqual(2, savedCalls[0][1].MaestroGenreId);
            Assert.AreEqual("2019-03-06", savedCalls[0][1].StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-04-24", savedCalls[0][1].EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(43200, savedCalls[0][1].StartTime);
            Assert.AreEqual(50399, savedCalls[0][1].EndTime);

            Assert.AreEqual(194938, savedCalls[0][2].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Brother", savedCalls[0][2].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][2].ShowType);
            Assert.AreEqual(2, savedCalls[0][2].GenreSourceId);
            Assert.AreEqual(2, savedCalls[0][2].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][2].StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-06-18", savedCalls[0][2].EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(39600, savedCalls[0][2].StartTime);
            Assert.AreEqual(48599, savedCalls[0][2].EndTime);

            Assert.AreEqual(194940, savedCalls[0][3].StationInventoryManifestDaypartId);
            Assert.AreEqual("Super Troopers", savedCalls[0][3].ProgramName);
            Assert.AreEqual("Movie", savedCalls[0][3].ShowType);
            Assert.AreEqual(2, savedCalls[0][3].GenreSourceId);
            Assert.AreEqual(2, savedCalls[0][3].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][3].StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-05-01", savedCalls[0][3].EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(50400, savedCalls[0][3].StartTime);
            Assert.AreEqual(68399, savedCalls[0][3].EndTime);

            // validate we moved the file
            Assert.AreEqual(1, fileServiceCreateCalls.Count);
            Assert.IsTrue(fileServiceCreateCalls[0].Contains("Completed"));
        }

        [Test]
        public void ImportResultsFile_InvalidFile()
        {
            /*** Arrange ***/
            const string fileName = @"INVALID_Results_ProgramGuideInventoryExportFile.csv";

            const string filesDirectoryPath = @".\Files\InventoryProgramsResultsImport";
            var filePath = Path.Combine(filesDirectoryPath, fileName);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()));
            var fileServiceCreateCalls = new List<string>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s, a) => fileServiceCreateCalls.Add(s));

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsTrue(result.Contains("Parsing attempt failed"));
            //Assert.IsTrue(result.Contains("Could not find required column inventory_id"));
            //Assert.IsTrue(result.Contains("Could not find required column inventory_week_id"));

            // validate we moved the file
            Assert.AreEqual(1, fileServiceCreateCalls.Count);
            Assert.IsTrue(fileServiceCreateCalls[0].Contains("Failed"));
        }

        [Test]
        public void ImportResultsFile_WithError()
        {
            /*** Arrange ***/
            const string fileName = @"Results_ProgramGuideInventoryExportFile_20200307_172832.csv";

            const string filesDirectoryPath = @".\Files\InventoryProgramsResultsImport";
            var filePath = Path.Combine(filesDirectoryPath, fileName);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            _InventoryRepo.Setup(s =>
                    s.DeleteInventoryPrograms(It.IsAny<List<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new Exception("TestException from Delete"));

            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()));
            var fileServiceCreateCalls = new List<string>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s, a) => fileServiceCreateCalls.Add(s));

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsTrue(result.Contains("Error caught :"));
            // validate we moved the file
            Assert.AreEqual(1, fileServiceCreateCalls.Count);
            Assert.IsTrue(fileServiceCreateCalls[0].Contains("Failed"));
        }

        /// <remarks>Do this after you've setup all your data repository returns</remarks>
        private Mock<IDataRepositoryFactory> _GetDataRepositoryFactory()
        {
            _InventoryProgramsByFileJobsRepo.Setup(r => r.UpdateJobStatus(It.IsAny<int>(), It.IsAny<InventoryProgramsJobStatus>(), It.IsAny<string>()));

            _InventoryRepo
                .Setup(x => x.GetDaypartProgramsForInventoryDayparts(It.IsAny<List<int>>()))
                .Returns<List<int>>(x => x.Select(mdId => new StationInventoryManifestDaypartProgram
                {
                    Id = 1,
                    StationInventoryManifestDaypartId = mdId,
                    ProgramName = "ProgramName",
                    ShowType = "ShowType",
                    SourceGenreId = 1,
                    GenreSourceId = 2,
                    MaestroGenreId = 2,
                    StartTime = 100,
                    EndTime = 200
                }).ToList());

            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryRepository>()).Returns(_InventoryRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryFileRepository>()).Returns(_InventoryFileRepo.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryProgramsByFileJobsRepository>()).Returns(_InventoryProgramsByFileJobsRepo.Object);

            return dataRepoFactory;
        }

        /// <summary>
        /// Can use either the BySource or ByFile process.
        /// </summary>
        private InventoryProgramsByFileProcessorTestClass _GetInventoryProgramsProcessingEngine()
        {
            var dataRepoFactory = _GetDataRepositoryFactory();

            _GenreCacheMock
                .Setup(x => x.GetSourceGenreByName(It.IsAny<string>(), It.IsAny<GenreSourceEnum>()))
                .Returns<string, GenreSourceEnum>((p1, p2) => new LookupDto
                {
                    Id = 1,
                    Display = $"{p2.ToString()} Genre"
                });

            _GenreCacheMock
                .Setup(x => x.GetMaestroGenreBySourceGenre(It.IsAny<LookupDto>(), It.IsAny<GenreSourceEnum>()))
                .Returns(new LookupDto
                {
                    Id = 2,
                    Display = "Maestro Genre"
                });

            var engine = new InventoryProgramsByFileProcessorTestClass(
                dataRepoFactory.Object,
                _ProgramGuidClient.Object,
                _StationMappingService.Object,
                _GenreCacheMock.Object,
                _FileService.Object,
                _EmailerService.Object);
            return engine;
        }
    }
}