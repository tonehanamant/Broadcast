using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Services.Broadcast.Entities.DTO;
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
        private Mock<IEnvironmentService> _EnvironmentService = new Mock<IEnvironmentService>();

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
                    s.CreateInventoryPrograms(It.IsAny<List<StationInventoryManifestDaypartProgram>>(),
                        It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((s, d) => savedCalls.Add(s));

            _InventoryRepo.Setup(s => s.GetStationInventoryManifestsByIds(It.IsAny<List<int>>()))
                .Returns(new List<StationInventoryManifest>());

            _InventoryRepo.Setup(s => s.GetManuallyMappedPrograms(It.IsAny<List<int>>()))
                .Returns(new List<int> { 123456 });

            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()));

            var fileServiceCreateCalls = new List<Tuple<string, Stream, long, long>>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s,a) => fileServiceCreateCalls.Add(new Tuple<string,Stream, long, long>(s,a, a.Length, a.Position)));

            var fileServiceMoveCalls = new List<Tuple<string, string>>();
            _FileService.Setup(s => s.Move(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((f,t) => fileServiceMoveCalls.Add(new Tuple<string, string>(f,t)))
                .Returns("moved");

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Exported 4 lines from the file."));
            Assert.IsTrue(result.Contains("Extracted and saved 4 program records."));
            Assert.IsTrue(result.Contains(@"File 'Results_ProgramGuideInventoryExportFile_20200307_172832.csv' moved from to 'testSettingBroadcastSharedDirectoryPath\ProgramGuide\ResultProcessing\DEV\Completed'."));

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
            Assert.AreEqual(2, savedCalls[0][0].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][0].MaestroGenreId);
            Assert.AreEqual("2019-03-20", savedCalls[0][0].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-04-20", savedCalls[0][0].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(39600, savedCalls[0][0].StartTime);
            Assert.AreEqual(46799, savedCalls[0][0].EndTime);
            
            Assert.AreEqual(194937, savedCalls[0][1].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Bang Theory", savedCalls[0][1].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][1].ShowType);
            Assert.AreEqual(2, savedCalls[0][1].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][1].MaestroGenreId);
            Assert.AreEqual("2019-03-06", savedCalls[0][1].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-04-24", savedCalls[0][1].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(43200, savedCalls[0][1].StartTime);
            Assert.AreEqual(50399, savedCalls[0][1].EndTime);

            Assert.AreEqual(194938, savedCalls[0][2].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Brother", savedCalls[0][2].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][2].ShowType);
            Assert.AreEqual(2, savedCalls[0][2].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][2].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][2].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-06-18", savedCalls[0][2].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(39600, savedCalls[0][2].StartTime);
            Assert.AreEqual(48599, savedCalls[0][2].EndTime);

            Assert.AreEqual(194940, savedCalls[0][3].StationInventoryManifestDaypartId);
            Assert.AreEqual("Super Troopers", savedCalls[0][3].ProgramName);
            Assert.AreEqual("Movie", savedCalls[0][3].ShowType);
            Assert.AreEqual(2, savedCalls[0][3].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][3].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][3].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-05-01", savedCalls[0][3].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(50400, savedCalls[0][3].StartTime);
            Assert.AreEqual(68399, savedCalls[0][3].EndTime);

            // validate we moved the file
            // created in progress
            Assert.AreEqual(1, fileServiceCreateCalls.Count);
            Assert.IsTrue(fileServiceCreateCalls[0].Item1.Contains("InProgress"));
            // length > 0 means there is content to save.
            Assert.IsTrue(fileServiceCreateCalls[0].Item3 > 0);
            // position 0 means we are at beginning of the content to save
            Assert.AreEqual(0, fileServiceCreateCalls[0].Item4);
            // moved to Completed
            Assert.AreEqual(1, fileServiceMoveCalls.Count);
            Assert.IsTrue(fileServiceMoveCalls[0].Item1.Contains("InProgress"));
            Assert.IsTrue(fileServiceMoveCalls[0].Item2.Contains("Completed"));
        }

        [Test]
        public void ImportResultsFile_SkipsMappedProgams()
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
                    s.CreateInventoryPrograms(It.IsAny<List<StationInventoryManifestDaypartProgram>>(),
                        It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((s, d) => savedCalls.Add(s));

            _InventoryRepo.Setup(s => s.GetStationInventoryManifestsByIds(It.IsAny<List<int>>()))
                .Returns(new List<StationInventoryManifest>());

            _InventoryRepo.Setup(s => s.GetManuallyMappedPrograms(It.IsAny<List<int>>()))
                .Returns(new List<int>{ 194936 });

            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()));

            var fileServiceCreateCalls = new List<Tuple<string, Stream, long, long>>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s, a) => fileServiceCreateCalls.Add(new Tuple<string, Stream, long, long>(s, a, a.Length, a.Position)));

            var fileServiceMoveCalls = new List<Tuple<string, string>>();
            _FileService.Setup(s => s.Move(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((f, t) => fileServiceMoveCalls.Add(new Tuple<string, string>(f, t)))
                .Returns("moved");

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Exported 4 lines from the file."));
            // Skipped one.
            Assert.IsTrue(result.Contains("Extracted and saved 3 program records."));
            Assert.IsTrue(result.Contains(@"File 'Results_ProgramGuideInventoryExportFile_20200307_172832.csv' moved from to 'testSettingBroadcastSharedDirectoryPath\ProgramGuide\ResultProcessing\DEV\Completed'."));

            // validate we deleted
            Assert.AreEqual(3, deleteCalls.Count);
            Assert.AreEqual(1, deleteCalls[0].Item1.Count);
            Assert.IsTrue(deleteCalls[0].Item1.Contains(192899));
            Assert.AreEqual(1, deleteCalls[1].Item1.Count);
            Assert.IsTrue(deleteCalls[1].Item1.Contains(192900));
            Assert.AreEqual(1, deleteCalls[2].Item1.Count);
            Assert.IsTrue(deleteCalls[2].Item1.Contains(192902));

            // validate we saved
            Assert.AreEqual(1, savedCalls.Count);
            Assert.AreEqual(3, savedCalls[0].Count);

            //Assert.AreEqual(194936, savedCalls[0][0].StationInventoryManifestDaypartId);
            //Assert.AreEqual("Tacoma FD", savedCalls[0][0].ProgramName);
            //Assert.AreEqual("Series", savedCalls[0][0].ShowType);
            //Assert.AreEqual(2, savedCalls[0][0].ProgramSourceId);
            //Assert.AreEqual(2, savedCalls[0][0].MaestroGenreId);
            //Assert.AreEqual("2019-03-20", savedCalls[0][0].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            //Assert.AreEqual("2019-04-20", savedCalls[0][0].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            //Assert.AreEqual(39600, savedCalls[0][0].StartTime);
            //Assert.AreEqual(46799, savedCalls[0][0].EndTime);

            Assert.AreEqual(194937, savedCalls[0][0].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Bang Theory", savedCalls[0][0].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][0].ShowType);
            Assert.AreEqual(2, savedCalls[0][0].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][0].MaestroGenreId);
            Assert.AreEqual("2019-03-06", savedCalls[0][0].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-04-24", savedCalls[0][0].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(43200, savedCalls[0][0].StartTime);
            Assert.AreEqual(50399, savedCalls[0][0].EndTime);

            Assert.AreEqual(194938, savedCalls[0][1].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Brother", savedCalls[0][1].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][1].ShowType);
            Assert.AreEqual(2, savedCalls[0][1].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][1].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][1].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-06-18", savedCalls[0][1].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(39600, savedCalls[0][1].StartTime);
            Assert.AreEqual(48599, savedCalls[0][1].EndTime);

            Assert.AreEqual(194940, savedCalls[0][2].StationInventoryManifestDaypartId);
            Assert.AreEqual("Super Troopers", savedCalls[0][2].ProgramName);
            Assert.AreEqual("Movie", savedCalls[0][2].ShowType);
            Assert.AreEqual(2, savedCalls[0][2].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][2].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][2].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-05-01", savedCalls[0][2].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(50400, savedCalls[0][2].StartTime);
            Assert.AreEqual(68399, savedCalls[0][2].EndTime);

            // validate we moved the file
            // created in progress
            Assert.AreEqual(1, fileServiceCreateCalls.Count);
            Assert.IsTrue(fileServiceCreateCalls[0].Item1.Contains("InProgress"));
            // length > 0 means there is content to save.
            Assert.IsTrue(fileServiceCreateCalls[0].Item3 > 0);
            // position 0 means we are at beginning of the content to save
            Assert.AreEqual(0, fileServiceCreateCalls[0].Item4);
            // moved to Completed
            Assert.AreEqual(1, fileServiceMoveCalls.Count);
            Assert.IsTrue(fileServiceMoveCalls[0].Item1.Contains("InProgress"));
            Assert.IsTrue(fileServiceMoveCalls[0].Item2.Contains("Completed"));
        }

        [Test]
        public void ImportResultsFile_CompressDateRange()
        {
            /*** Arrange ***/
            const string fileName = @"Results_ProgramGuideInventory_CompressDateRangeTest.csv";

            const string filesDirectoryPath = @".\Files\InventoryProgramsResultsImport";
            var filePath = Path.Combine(filesDirectoryPath, fileName);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var deleteCalls = new List<Tuple<List<int>, DateTime, DateTime>>();
            _InventoryRepo.Setup(s =>
                    s.DeleteInventoryPrograms(It.IsAny<List<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<List<int>, DateTime, DateTime>((ids, s, e) => deleteCalls.Add(new Tuple<List<int>, DateTime, DateTime>(ids, s, e)));

            var savedCalls = new List<List<StationInventoryManifestDaypartProgram>>();
            _InventoryRepo.Setup(s =>
                    s.CreateInventoryPrograms(It.IsAny<List<StationInventoryManifestDaypartProgram>>(),
                        It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((s, d) => savedCalls.Add(s));

            _InventoryRepo.Setup(s => s.GetStationInventoryManifestsByIds(It.IsAny<List<int>>()))
                .Returns(new List<StationInventoryManifest>());

            _InventoryRepo.Setup(s => s.GetManuallyMappedPrograms(It.IsAny<List<int>>()))
                .Returns(new List<int> { 123456 });

            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()));

            var fileServiceCreateCalls = new List<Tuple<string, Stream, long, long>>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s, a) => fileServiceCreateCalls.Add(new Tuple<string, Stream, long, long>(s, a, a.Length, a.Position)));

            var fileServiceMoveCalls = new List<Tuple<string, string>>();
            _FileService.Setup(s => s.Move(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((f, t) => fileServiceMoveCalls.Add(new Tuple<string, string>(f, t)))
                .Returns("moved");

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Exported 130 lines from the file."));
            Assert.IsTrue(result.Contains("Grouped batch of 130 to 8 program groups."));
            Assert.IsTrue(result.Contains("Extracted and saved 8 program records."));

            // validate we saved the compressed dates correctly.
            Assert.AreEqual(1, savedCalls.Count);
            Assert.AreEqual(8, savedCalls[0].Count);

            Assert.AreEqual(124714, savedCalls[0][0].StationInventoryManifestDaypartId);
            Assert.AreEqual("Extra", savedCalls[0][0].ProgramName);
            Assert.AreEqual("SER", savedCalls[0][0].ShowType);
            Assert.AreEqual("2020-01-04", savedCalls[0][0].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2020-03-29", savedCalls[0][0].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(84900, savedCalls[0][0].StartTime);
            Assert.AreEqual(2100, savedCalls[0][0].EndTime);

            Assert.AreEqual(125644, savedCalls[0][1].StationInventoryManifestDaypartId);
            Assert.AreEqual("Late Night with Seth Meyers", savedCalls[0][1].ProgramName);
            Assert.AreEqual("SER", savedCalls[0][1].ShowType);
            Assert.AreEqual("2020-01-03", savedCalls[0][1].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2020-03-28", savedCalls[0][1].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(85020, savedCalls[0][1].StartTime);
            Assert.AreEqual(2220, savedCalls[0][1].EndTime);

            Assert.AreEqual(128196, savedCalls[0][2].StationInventoryManifestDaypartId);
            Assert.AreEqual("NBC3 News at 7pm", savedCalls[0][2].ProgramName);
            Assert.AreEqual("NEWS", savedCalls[0][2].ShowType);
            Assert.AreEqual("2019-12-30", savedCalls[0][2].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2020-03-27", savedCalls[0][2].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(68400, savedCalls[0][2].StartTime);
            Assert.AreEqual(70200, savedCalls[0][2].EndTime);

            Assert.AreEqual(129747, savedCalls[0][3].StationInventoryManifestDaypartId);
            Assert.AreEqual("Fox43 News Featured", savedCalls[0][3].ProgramName);
            Assert.AreEqual("NEWS", savedCalls[0][3].ShowType);
            Assert.AreEqual("2020-01-04", savedCalls[0][3].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2020-03-28", savedCalls[0][3].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(28800, savedCalls[0][3].StartTime);
            Assert.AreEqual(32400, savedCalls[0][3].EndTime);

            Assert.AreEqual(135307, savedCalls[0][4].StationInventoryManifestDaypartId);
            Assert.AreEqual("America This Week", savedCalls[0][4].ProgramName);
            Assert.AreEqual("NEWS", savedCalls[0][4].ShowType);
            Assert.AreEqual("2020-01-18", savedCalls[0][4].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2020-03-29", savedCalls[0][4].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(84900, savedCalls[0][4].StartTime);
            Assert.AreEqual(2100, savedCalls[0][4].EndTime);

            Assert.AreEqual(135329, savedCalls[0][5].StationInventoryManifestDaypartId);
            Assert.AreEqual("America This Week", savedCalls[0][5].ProgramName);
            Assert.AreEqual("NEWS", savedCalls[0][5].ShowType);
            Assert.AreEqual("2020-01-04", savedCalls[0][5].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2020-01-12", savedCalls[0][5].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(84900, savedCalls[0][5].StartTime);
            Assert.AreEqual(2100, savedCalls[0][5].EndTime);

            Assert.AreEqual(138288, savedCalls[0][6].StationInventoryManifestDaypartId);
            Assert.AreEqual("Wipeout", savedCalls[0][6].ProgramName);
            Assert.AreEqual("SER", savedCalls[0][6].ShowType);
            Assert.AreEqual("2020-01-04", savedCalls[0][6].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2020-01-19", savedCalls[0][6].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(84600, savedCalls[0][6].StartTime);
            Assert.AreEqual(1800, savedCalls[0][6].EndTime);

            Assert.AreEqual(138288, savedCalls[0][7].StationInventoryManifestDaypartId);
            Assert.AreEqual("Wipeout", savedCalls[0][7].ProgramName);
            Assert.AreEqual("SER", savedCalls[0][7].ShowType);
            Assert.AreEqual("2020-01-25", savedCalls[0][7].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2020-03-29", savedCalls[0][7].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(84900, savedCalls[0][7].StartTime);
            Assert.AreEqual(2100, savedCalls[0][7].EndTime);
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
                    s.CreateInventoryPrograms(It.IsAny<List<StationInventoryManifestDaypartProgram>>(),
                        It.IsAny<DateTime>()))
                .Callback<List<StationInventoryManifestDaypartProgram>, DateTime>((s, d) => savedCalls.Add(s));

            _InventoryRepo.Setup(s => s.GetStationInventoryManifestsByIds(It.IsAny<List<int>>()))
                .Returns(new List<StationInventoryManifest>());

            _InventoryRepo.Setup(s => s.GetManuallyMappedPrograms(It.IsAny<List<int>>()))
                .Returns(new List<int> { 123456 });

            _FileService.Setup(s => s.CreateDirectory(It.IsAny<string>()));

            var fileServiceCreateCalls = new List<Tuple<string, Stream, long, long>>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s, a) => fileServiceCreateCalls.Add(new Tuple<string, Stream, long, long>(s, a, a.Length, a.Position)));

            var fileServiceMoveCalls = new List<Tuple<string, string>>();
            _FileService.Setup(s => s.Move(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((f, t) => fileServiceMoveCalls.Add(new Tuple<string, string>(f, t)))
                .Returns("Completed.");

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Exported 4 lines from the file."));
            Assert.IsTrue(result.Contains("Extracted and saved 4 program records."));
            Assert.IsTrue(result.Contains(@"File 'Results_ProgramGuideInventoryExportFile_DaypartQuotes.csv' moved from to 'testSettingBroadcastSharedDirectoryPath\ProgramGuide\ResultProcessing\DEV\Completed'."));

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
            Assert.AreEqual(2, savedCalls[0][0].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][0].MaestroGenreId);
            Assert.AreEqual("2019-03-20", savedCalls[0][0].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-04-20", savedCalls[0][0].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(39600, savedCalls[0][0].StartTime);
            Assert.AreEqual(46799, savedCalls[0][0].EndTime);

            Assert.AreEqual(194937, savedCalls[0][1].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Bang Theory", savedCalls[0][1].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][1].ShowType);
            Assert.AreEqual(2, savedCalls[0][1].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][1].MaestroGenreId);
            Assert.AreEqual("2019-03-06", savedCalls[0][1].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-04-24", savedCalls[0][1].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(43200, savedCalls[0][1].StartTime);
            Assert.AreEqual(50399, savedCalls[0][1].EndTime);

            Assert.AreEqual(194938, savedCalls[0][2].StationInventoryManifestDaypartId);
            Assert.AreEqual("Big Brother", savedCalls[0][2].ProgramName);
            Assert.AreEqual("Series", savedCalls[0][2].ShowType);
            Assert.AreEqual(2, savedCalls[0][2].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][2].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][2].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-06-18", savedCalls[0][2].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(39600, savedCalls[0][2].StartTime);
            Assert.AreEqual(48599, savedCalls[0][2].EndTime);

            Assert.AreEqual(194940, savedCalls[0][3].StationInventoryManifestDaypartId);
            Assert.AreEqual("Super Troopers", savedCalls[0][3].ProgramName);
            Assert.AreEqual("Movie", savedCalls[0][3].ShowType);
            Assert.AreEqual(2, savedCalls[0][3].ProgramSourceId);
            Assert.AreEqual(2, savedCalls[0][3].MaestroGenreId);
            Assert.AreEqual("2019-04-01", savedCalls[0][3].StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual("2019-05-01", savedCalls[0][3].EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(50400, savedCalls[0][3].StartTime);
            Assert.AreEqual(68399, savedCalls[0][3].EndTime);

            // validate we moved the file
            // created in in progress
            Assert.AreEqual(1, fileServiceCreateCalls.Count);
            Assert.IsTrue(fileServiceCreateCalls[0].Item1.Contains("InProgress"));
            // length > 0 means there is content to save.
            Assert.IsTrue(fileServiceCreateCalls[0].Item3 > 0);
            // position 0 means we are at beginning of the content to save
            Assert.AreEqual(0, fileServiceCreateCalls[0].Item4);
            // moved to Completed
            Assert.AreEqual(1, fileServiceMoveCalls.Count);
            Assert.IsTrue(fileServiceMoveCalls[0].Item1.Contains("InProgress"));
            Assert.IsTrue(fileServiceMoveCalls[0].Item2.Contains("Completed"));
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
            var fileServiceCreateCalls = new List<Tuple<string, Stream, long, long>>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s, a) => fileServiceCreateCalls.Add(new Tuple<string, Stream, long, long>(s, a, a.Length, a.Position)));

            var fileServiceMoveCalls = new List<Tuple<string, string>>();
            _FileService.Setup(s => s.Move(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((f, t) => fileServiceMoveCalls.Add(new Tuple<string, string>(f, t)))
                .Returns("Completed.");

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsTrue(result.Contains("Parsing attempt failed"));
            //Assert.IsTrue(result.Contains("Could not find required column inventory_id"));
            //Assert.IsTrue(result.Contains("Could not find required column inventory_week_id"));

            // validate we saved the failed file in correct place
            // created in in progress
            Assert.AreEqual(1, fileServiceCreateCalls.Count);
            Assert.IsTrue(fileServiceCreateCalls[0].Item1.Contains("InProgress"));
            // length > 0 means there is content to save.
            Assert.IsTrue(fileServiceCreateCalls[0].Item3 > 0);
            // position 0 means we are at beginning of the content to save
            Assert.AreEqual(0, fileServiceCreateCalls[0].Item4);
            // moved to failed
            Assert.AreEqual(1, fileServiceMoveCalls.Count);
            Assert.IsTrue(fileServiceMoveCalls[0].Item1.Contains("InProgress"));
            Assert.IsTrue(fileServiceMoveCalls[0].Item2.Contains("Failed"));
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
            var fileServiceCreateCalls = new List<Tuple<string, Stream, long, long>>();
            _FileService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()))
                .Callback<string, Stream>((s, a) => fileServiceCreateCalls.Add(new Tuple<string, Stream, long, long>(s, a, a.Length, a.Position)));

            var fileServiceMoveCalls = new List<Tuple<string, string>>();
            _FileService.Setup(s => s.Move(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((f, t) => fileServiceMoveCalls.Add(new Tuple<string, string>(f, t)))
                .Returns("Completed.");

            var engine = _GetInventoryProgramsProcessingEngine();

            /*** Act ***/
            var result = engine.ImportInventoryProgramResults(fileStream, fileName);

            /*** Assert ***/
            Assert.IsTrue(result.Contains("Error caught :"));
            // validate we saved the failed file in correct place
            // created in In Progress
            Assert.AreEqual(1, fileServiceCreateCalls.Count);
            Assert.IsTrue(fileServiceCreateCalls[0].Item1.Contains("InProgress"));
            // length > 0 means there is content to save.
            Assert.IsTrue(fileServiceCreateCalls[0].Item3 > 0);
            // position 0 means we are at beginning of the content to save
            Assert.AreEqual(0, fileServiceCreateCalls[0].Item4);
            // moved to failed
            Assert.AreEqual(1, fileServiceMoveCalls.Count);
            Assert.IsTrue(fileServiceMoveCalls[0].Item1.Contains("InProgress"));
            Assert.IsTrue(fileServiceMoveCalls[0].Item2.Contains("Failed"));
        }

        [Test]
        public void ImportInventoryProgramResultsFromDirectory_DirectoryNotExist()
        {
            /*** Arrange ***/
            const int dayOffset = -1;

            // setup so that directory doesn't exist.
            _FileService.Setup(s => s.DirectoryExists(It.IsAny<string>()))
                .Returns(false);
            
            var engine = _GetInventoryProgramsProcessingEngine();
            // simulate the actual import for our test.
            engine.UT_ShouldPerformImportInventoryProgramResults = false;

            /*** ACT **/
            var caught = Assert.Throws<DirectoryNotFoundException>(() => engine.ImportInventoryProgramResultsFromDirectory(dayOffset));

            /*** Assert ***/
            Assert.IsNotNull(caught);
            // verify we never actually called the import method.
            Assert.AreEqual(0, engine.UT_PerformImportInventoryProgramResultsCalls.Count); 
        }

        [Test]
        public void ImportInventoryProgramResultsFromDirectory_NoFilesToProcess()
        {
            /*** Arrange ***/
            const int dayOffset = -1;

            _FileService.Setup(s => s.DirectoryExists(It.IsAny<string>()))
                .Returns(true);
            // setup so that files don't exist
            _FileService.Setup(s => s.GetFiles(It.IsAny<string>()))
                .Returns(new List<string>());

            var engine = _GetInventoryProgramsProcessingEngine();
            // simulate the actual import for our test.
            engine.UT_ShouldPerformImportInventoryProgramResults = false;

            /*** ACT **/
            var caught = Assert.Throws<FileNotFoundException>(() => engine.ImportInventoryProgramResultsFromDirectory(dayOffset));

            /*** Assert ***/
            Assert.IsNotNull(caught);
            // verify we never actually called the import method.
            Assert.AreEqual(0, engine.UT_PerformImportInventoryProgramResultsCalls.Count);
        }

        [Test]
        public void ImportInventoryProgramResultsFromDirectory_FileProcessingError()
        {
            /*** Arrange ***/
            const int dayOffset = -1;

            _FileService.Setup(s => s.DirectoryExists(It.IsAny<string>()))
                .Returns(true);
            // setup so that files don't exist
            _FileService.Setup(s => s.GetFiles(It.IsAny<string>()))
                .Returns(new List<string>
                {
                    "fileOne.csv",
                    "fileTwo.csv",
                    "fileThree.csv"
                });

            _FileService.SetupSequence(s => s.GetFileStream(It.IsAny<string>()))
                .Returns(new MemoryStream())
                .Throws<Exception>()
                .Returns(new MemoryStream());

            var engine = _GetInventoryProgramsProcessingEngine();
            // simulate the actual import for our test.
            engine.UT_ShouldPerformImportInventoryProgramResults = false;

            /*** ACT **/
            engine.ImportInventoryProgramResultsFromDirectory(dayOffset);

            /*** Assert ***/
            // verify we called even though we hit an error.
            Assert.AreEqual(2, engine.UT_PerformImportInventoryProgramResultsCalls.Count);
            var processedFileNames = engine.UT_PerformImportInventoryProgramResultsCalls.Select(s => s.Item2).ToList();
            Assert.IsTrue(processedFileNames.Contains("fileOne.csv"));
            Assert.IsFalse(processedFileNames.Contains("fileTwo.csv"));
            Assert.IsTrue(processedFileNames.Contains("fileThree.csv"));
        }

        [Test]
        public void ImportInventoryProgramResultsFromDirectory()
        {
            /*** Arrange ***/
            const int dayOffset = -1;

            _FileService.Setup(s => s.DirectoryExists(It.IsAny<string>()))
                .Returns(true);
            // setup so that files don't exist
            _FileService.Setup(s => s.GetFiles(It.IsAny<string>()))
                .Returns(new List<string>
                {
                    "fileOne.csv",
                    "fileTwo.csv",
                    "fileThree.csv"
                });

            _FileService.SetupSequence(s => s.GetFileStream(It.IsAny<string>()))
                .Returns(new MemoryStream())
                .Returns(new MemoryStream())
                .Returns(new MemoryStream());

            var engine = _GetInventoryProgramsProcessingEngine();
            // simulate the actual import for our test.
            engine.UT_ShouldPerformImportInventoryProgramResults = false;

            /*** ACT **/
            engine.ImportInventoryProgramResultsFromDirectory(dayOffset);

            /*** Assert ***/
            // verify we called for each
            Assert.AreEqual(3, engine.UT_PerformImportInventoryProgramResultsCalls.Count);
            var processedFileNames = engine.UT_PerformImportInventoryProgramResultsCalls.Select(s => s.Item2).ToList();
            Assert.IsTrue(processedFileNames.Contains("fileOne.csv"));
            Assert.IsTrue(processedFileNames.Contains("fileTwo.csv"));
            Assert.IsTrue(processedFileNames.Contains("fileThree.csv"));
        }

        [Test]
        [TestCase(-1, "20200513", "05/14/2020")]
        [TestCase(0, "20200514", "05/14/2020")]
        public void GetProgramGuideExportWorkingDirectoryPath(int dayOffset, string expectedDateFileString, string currentDateString)
        {
            /*** Arrange ***/
            var engine = _GetInventoryProgramsProcessingEngine();
            engine.UT_CurrentDateTime = DateTime.Parse(currentDateString);
            var expectedResult = $@"testSettingBroadcastSharedDirectoryPath\ProgramGuide\ExportsToProcess\{expectedDateFileString}\DEV\Processed";

            /*** ACT **/
            var result = engine.UT_GetProgramGuideInterfaceProcessedDirectoryPath(dayOffset);

            /*** Assert ***/
            Assert.AreEqual(expectedResult, result);
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
                    ProgramSourceId = 2,
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
                .Setup(x => x.GetSourceGenreByName(It.IsAny<string>(), It.IsAny<ProgramSourceEnum>()))
                .Returns<string, ProgramSourceEnum>((p1, p2) => new LookupDto
                {
                    Id = 1,
                    Display = $"{p2.ToString()} Genre"
                });

            _GenreCacheMock
                .Setup(x => x.GetMaestroGenreBySourceGenre(It.IsAny<LookupDto>(), It.IsAny<ProgramSourceEnum>()))
                .Returns(new LookupDto
                {
                    Id = 2,
                    Display = "Maestro Genre"
                });

            _EnvironmentService.Setup(s => s.GetEnvironmentInfo())
                .Returns(new EnvironmentDto
                {
                    DisplayBuyingLink = true,
                    DisplayCampaignLink = true,
                    AllowMultipleCreativeLengths = false,
                    Environment = "DEV"
                });

            var engine = new InventoryProgramsByFileProcessorTestClass(
                dataRepoFactory.Object,
                _ProgramGuidClient.Object,
                _StationMappingService.Object,
                _GenreCacheMock.Object,
                _FileService.Object,
                _EmailerService.Object,
                _EnvironmentService.Object);
            return engine;
        }
    }
}