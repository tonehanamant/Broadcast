using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Services.Clients;
using static Services.Broadcast.Entities.StationContact;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class InventoryServiceUnitTests
    {
        private Mock<IDataRepositoryFactory> _BroadcastDataRepositoryFactoryMock;
        private Mock<IInventoryFileValidator> _InventoryFileValidatorMock;
        private Mock<IDaypartCache> _DaypartCacheMock;
        private Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock;
        private Mock<ISMSClient> _SmsClientMock;
        private Mock<IInventoryManagementApiClient> _InventoryManagementApiClientMock;
        private Mock<IProprietarySpotCostCalculationEngine> _ProprietarySpotCostCalculationEngineMock;
        private Mock<IStationInventoryGroupService> _StationInventoryGroupServiceMock;
        private Mock<IBroadcastAudiencesCache> _AudiencesCacheMock;
        private Mock<IRatingForecastService> _RatingForecastServiceMock;
        private Mock<INsiPostingBookService> _NsiPostingBookServiceMock;
        private Mock<ILockingEngine> _LockingEngineMock;
        private Mock<IStationProcessingEngine> _StationProcessingEngineMock;
        private Mock<IImpressionsService> _ImpressionsServiceMock;
        private Mock<IOpenMarketFileImporter> _OpenMarketFileImporterMock;
        private Mock<IFileService> _FileServiceMock;
        private Mock<IInventoryRatingsProcessingService> _InventoryRatingsServiceMock;
        private Mock<IInventoryProgramsProcessingService> _InventoryProgramsProcessingServiceMock;
        private Mock<IInventoryRepository> _InventoryRepositoryMock;
        private Mock<ISpotLengthRepository> _SpotLengthRepository;
        private Mock<IInventoryFileRepository> _InventoryFileRepositoryMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private Mock<ISharedFolderService> _SharedFolderService;
        private Mock<IDateTimeEngine> _DateTimeEngine;
        private Mock<IStationRepository> _StationRepositoryMock;
        private InventoryService _InventoryService;

        [SetUp]
        public void Setup()
        {
            _BroadcastDataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _InventoryFileValidatorMock = new Mock<IInventoryFileValidator>();
            _DaypartCacheMock = new Mock<IDaypartCache>();
            _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            _SmsClientMock = new Mock<ISMSClient>();
            _InventoryManagementApiClientMock = new Mock<IInventoryManagementApiClient>();
            _ProprietarySpotCostCalculationEngineMock = new Mock<IProprietarySpotCostCalculationEngine>();
            _StationInventoryGroupServiceMock = new Mock<IStationInventoryGroupService>();
            _AudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
            _RatingForecastServiceMock = new Mock<IRatingForecastService>();
            _NsiPostingBookServiceMock = new Mock<INsiPostingBookService>();
            _LockingEngineMock = new Mock<ILockingEngine>();
            _StationProcessingEngineMock = new Mock<IStationProcessingEngine>();
            _ImpressionsServiceMock = new Mock<IImpressionsService>();
            _OpenMarketFileImporterMock = new Mock<IOpenMarketFileImporter>();
            _FileServiceMock = new Mock<IFileService>();
            _InventoryRatingsServiceMock = new Mock<IInventoryRatingsProcessingService>();
            _InventoryProgramsProcessingServiceMock = new Mock<IInventoryProgramsProcessingService>();
            _InventoryRepositoryMock = new Mock<IInventoryRepository>();
            _SpotLengthRepository = new Mock<ISpotLengthRepository>();
            _InventoryFileRepositoryMock = new Mock<IInventoryFileRepository>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _SharedFolderService = new Mock<ISharedFolderService>();
            _DateTimeEngine = new Mock<IDateTimeEngine>();
            _StationRepositoryMock = new Mock<IStationRepository>();
            _SpotLengthRepository
                .Setup(x => x.GetSpotLengthIdsByDuration())
                .Returns(new Dictionary<int, int>());

            _SpotLengthRepository
                .Setup(x => x.GetSpotLengthIdsAndCostMultipliers(true))
                .Returns(new Dictionary<int, decimal>());

            _BroadcastDataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepositoryMock.Object);

            _BroadcastDataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
                .Returns(_SpotLengthRepository.Object);

            _BroadcastDataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryFileRepository>())
                .Returns(_InventoryFileRepositoryMock.Object);

            _BroadcastDataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IStationRepository>())
                .Returns(_StationRepositoryMock.Object);

            _InventoryService = new InventoryService(
                _BroadcastDataRepositoryFactoryMock.Object,
                _InventoryFileValidatorMock.Object,
                _DaypartCacheMock.Object,
                _QuarterCalculationEngineMock.Object,
                _SmsClientMock.Object,               
                _ProprietarySpotCostCalculationEngineMock.Object,
                _StationInventoryGroupServiceMock.Object,
                _AudiencesCacheMock.Object,
                _RatingForecastServiceMock.Object,
                _NsiPostingBookServiceMock.Object,
                _LockingEngineMock.Object,
                _StationProcessingEngineMock.Object,
                _ImpressionsServiceMock.Object,
                _OpenMarketFileImporterMock.Object,
                _FileServiceMock.Object,
                _SharedFolderService.Object,
                _InventoryRatingsServiceMock.Object,
                _InventoryProgramsProcessingServiceMock.Object,
                _DateTimeEngine.Object,
                _InventoryManagementApiClientMock.Object,
                _FeatureToggleMock.Object,
                _ConfigurationSettingsHelperMock.Object);
        }

        [Test]
        [TestCase(FileStatusEnum.Failed, 0, "Validation Error")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Failed, "Processing Error")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Succeeded, "Succeeded")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Queued, "Processing")]
        [TestCase(FileStatusEnum.Loaded, BackgroundJobProcessingStatus.Processing, "Processing")]
        public void GetInventoryUploadHistory_ReturnsCorrectStatuses(
            FileStatusEnum fileLoadStatus,
            BackgroundJobProcessingStatus ratingProcessingJobStatus,
            string expectedStatus)
        {
            _QuarterCalculationEngineMock
                .Setup(x => x.GetQuarterDateRange(It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns(new DateRange(new DateTime(2019, 1, 1), new DateTime(2019, 4, 1)));

            _InventoryRepositoryMock
                .Setup(x => x.GetInventoryUploadHistoryForInventorySource(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .Returns(new List<InventoryUploadHistory>
                {
                    new InventoryUploadHistory
                    {
                        FileLoadStatus = fileLoadStatus,
                        RatingProcessingJobStatus = ratingProcessingJobStatus
                    }
                });

            var result = _InventoryService.GetInventoryUploadHistory(
                inventorySourceId: 1,
                quarter: null,
                year: null);

            Assert.AreEqual(expectedStatus, result.First().Status);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DownloadErrorFile(bool existInSharedFolderService)
        {
            // Arrange
            const int fileId = 23;
            const string fileName = "InventoryErrorFile.xlsx";
            var expectedFileName = fileName;
            var errorFileSharedFolderFileId = existInSharedFolderService
                ? new Guid("5CB9C249-21EE-4F93-9602-19E31013EA05")
                : (Guid?)null;

            var savedFileContent = new MemoryStream();

            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");

            _InventoryFileRepositoryMock.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Returns(new InventoryFile
                {
                    Id = fileId,
                    FileName = fileName,
                    ErrorFileSharedFolderFileId = errorFileSharedFolderFileId
                });

            _SharedFolderService.Setup(s => s.GetFile(It.IsAny<Guid>()))
                .Returns(new SharedFolderFile
                {
                    FileContent = savedFileContent,
                    FileNameWithExtension = expectedFileName
                });

            _FileServiceMock.Setup(s => s.Exists(It.IsAny<string>()))
                .Returns(true);

            _FileServiceMock.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Returns(savedFileContent);

            // Act
            var result = _InventoryService.DownloadErrorFile(fileId);

            // Assert
            // validate result is correct
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedFileName, result.Item1); // filename
            Assert.IsNotNull(result.Item2); // stream
            Assert.AreEqual(@"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Item3); // mimeType

            // validate correct locations were called
            var timesCallSharedFileService = existInSharedFolderService
                ? Times.Once()
                : Times.Never();
            _SharedFolderService.Verify(s => s.GetFile(It.IsAny<Guid>()), timesCallSharedFileService);

            var shouldCheckFileService = !existInSharedFolderService;
            var timesCallFileServiceExists = shouldCheckFileService ? Times.Once() : Times.Never();
            _FileServiceMock.Verify(s => s.Exists(It.IsAny<string>()), timesCallFileServiceExists);

            var timesCallFileServiceGetFileStream = shouldCheckFileService ? Times.Once() : Times.Never();
            _FileServiceMock.Verify(s => s.GetFileStream(It.IsAny<string>()), timesCallFileServiceGetFileStream);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DownloadErrorFile_XmlFile(bool existInSharedFolderService)
        {
            // Arrange
            const int fileId = 23;
            const string fileName = "InventoryErrorFile.xml";
            var expectedFileName = $"{fileName}.txt";
            var errorFileSharedFolderFileId = existInSharedFolderService
                ? new Guid("5CB9C249-21EE-4F93-9602-19E31013EA05")
                : (Guid?)null;

            var savedFileContent = new MemoryStream();

            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");

            _InventoryFileRepositoryMock.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Returns(new InventoryFile
                {
                    Id = fileId,
                    FileName = fileName,
                    ErrorFileSharedFolderFileId = errorFileSharedFolderFileId
                });

            _SharedFolderService.Setup(s => s.GetFile(It.IsAny<Guid>()))
                .Returns(new SharedFolderFile
                {
                    FileContent = savedFileContent,
                    FileNameWithExtension = expectedFileName
                });

            _FileServiceMock.Setup(s => s.Exists(It.IsAny<string>()))
                .Returns(true);

            _FileServiceMock.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Returns(savedFileContent);

            // Act
            var result = _InventoryService.DownloadErrorFile(fileId);

            // Assert
            // validate result is correct
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedFileName, result.Item1); // filename
            Assert.IsNotNull(result.Item2); // stream
            Assert.AreEqual(@"text/plain", result.Item3); // mimeType

            // validate correct locations were called
            var timesCallSharedFileService = existInSharedFolderService
                ? Times.Once()
                : Times.Never();
            _SharedFolderService.Verify(s => s.GetFile(It.IsAny<Guid>()), timesCallSharedFileService);
            
            var shouldCheckFileService = !existInSharedFolderService;
            var timesCallFileServiceExists = shouldCheckFileService ? Times.Once() : Times.Never();
            _FileServiceMock.Verify(s => s.Exists(It.IsAny<string>()), timesCallFileServiceExists);

            var timesCallFileServiceGetFileStream = shouldCheckFileService ? Times.Once() : Times.Never();
            _FileServiceMock.Verify(s => s.GetFileStream(It.IsAny<string>()), timesCallFileServiceGetFileStream);
        }

        [Test]
        public void DownloadErrorFile_IdNotFound()
        {
            const int fileId = 23;
            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");

            var getInventoryFileByIdCalledCount = 0;
            _InventoryFileRepositoryMock.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Callback(() => getInventoryFileByIdCalledCount++)
                .Throws(new Exception("Test message for id not found."));

            var caught = Assert.Throws<InvalidOperationException>(() => _InventoryService.DownloadErrorFile(fileId));

            Assert.AreEqual($"File record for id '{fileId}' not found.", caught.Message);
            Assert.AreEqual(1, getInventoryFileByIdCalledCount);
        }

        [Test]
        public void DownloadErrorFile_FileNotFound()
        {
            // Arrange
            const int fileId = 23;
            const string fileName = "InventoryErrorFile.xlsx";
            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");

            var getInventoryFileByIdCalledCount = 0;
            _InventoryFileRepositoryMock.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Callback(() => getInventoryFileByIdCalledCount++)
                .Returns(new InventoryFile
                {
                    Id = fileId,
                    FileName = fileName,
                });

            var fileExistsCalledCount = 0;
            _FileServiceMock.Setup(s => s.Exists(It.IsAny<string>()))
                .Callback(() => fileExistsCalledCount++)
                .Returns(false);

            var getFileStreamCalled = 0;
            _FileServiceMock.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Callback(() => getFileStreamCalled++)
                .Returns(new MemoryStream());

            // Act
            var caught = Assert.Throws<FileNotFoundException>(() => _InventoryService.DownloadErrorFile(fileId));

            // Assert
            Assert.AreEqual($"File '{fileName}' with id '{fileId}' not found.", caught.Message);
            Assert.AreEqual(1, fileExistsCalledCount);
            Assert.AreEqual(0, getFileStreamCalled);
        }

        [Test]
        public void DownloadErrorFiles()
        {
            // Arrange
            var fileIds = new List<int> { 27, 28, 52 };
            var expectedArchiveFileName = $"InventoryErrorFiles_01242020123007.zip";

            var testDateTimeNow = new DateTime(2020, 01, 24, 12, 30, 07);
            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");

            _InventoryFileRepositoryMock.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Returns<int>((fileId) => new InventoryFile
                {
                    Id = fileId,
                    FileName = "InventoryErrorFile.xlsx",
                });

            _FileServiceMock.Setup(s => s.Exists(It.IsAny<string>()))
                .Returns(true);

            _FileServiceMock.Setup(s => s.CreateZipArchive(It.IsAny<Dictionary<string, string>>()))
                .Returns(new MemoryStream());

            _DateTimeEngine.Setup(s => s.GetCurrentMoment())
                .Returns(testDateTimeNow);

            _InventoryFileRepositoryMock.Setup(s => s.GetErrorFileSharedFolderFileIds(It.IsAny<List<int>>()))
                .Returns(new List<Guid>
                {
                    new Guid("3EFA6136-7C6B-4ED3-8F0E-52C58CFE5B38"),
                    new Guid("45E64F65-B807-4178-B81E-2A4AE2B33B38"), 
                    new Guid("74D8DEA4-6734-44E3-9056-A1D00FEFEA3C"),
                });

            _SharedFolderService.Setup(s => s.CreateZipArchive(It.IsAny<List<Guid>>()))
                .Returns(new MemoryStream());

            // Act
            var result = _InventoryService.DownloadErrorFiles(fileIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedArchiveFileName, result.Item1);
            Assert.IsNotNull(result.Item2); // stream
            _InventoryFileRepositoryMock.Verify(s => s.GetErrorFileSharedFolderFileIds(It.IsAny<List<int>>()), Times.Once());
            _SharedFolderService.Verify(s => s.CreateZipArchive(It.IsAny<List<Guid>>()), Times.Once());

            _InventoryFileRepositoryMock.Verify(s => s.GetInventoryFileById(It.IsAny<int>()), Times.Never);
            _FileServiceMock.Verify(s => s.Exists(It.IsAny<string>()), Times.Never);
            _FileServiceMock.Verify(s => s.CreateZipArchive(It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [Test]
        public void SaveUploadedInventoryFileToFileStore()
        {
            // Arrange
            const string username = "TestUser";
            const int inventoryFileId = 23;
            var testSavedSharedFolderFileId = new Guid("5CB9C249-21EE-4F93-9602-19E31013EA05");
            var request = new InventoryFileSaveRequest
            {
                FileName = "TestFileName.xml",
                StreamData = new MemoryStream()
            };
            var testDateTimeNow = new DateTime(2021, 10,29, 12, 1, 35);

            _DateTimeEngine.Setup(s => s.GetCurrentMoment())
                .Returns(testDateTimeNow);
            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");

            var savedSharedFolderFiles = new List<SharedFolderFile>();
            _SharedFolderService.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Callback<SharedFolderFile>((f) => savedSharedFolderFiles.Add(f))
                .Returns(testSavedSharedFolderFileId);

            var saveUploadedFileIdCalls = new List<Tuple<int, Guid>>();
            _InventoryFileRepositoryMock.Setup(s => s.SaveUploadedFileId(It.IsAny<int>(), It.IsAny<Guid>()))
                .Callback<int, Guid>((i,g) => saveUploadedFileIdCalls.Add(new Tuple<int, Guid>(i,g)));

            // Act
            _InventoryService._SaveUploadedInventoryFileToFileStore(request, inventoryFileId, username);

            // Assert
            // verify the request mapped to the shared folder correctly
            _SharedFolderService.Verify(s => s.SaveFile(It.IsAny<SharedFolderFile>()), Times.Once);
            Assert.AreEqual(request.FileName, savedSharedFolderFiles[0].FileNameWithExtension);

            // verify the job saved with the shared id
            _InventoryFileRepositoryMock.Verify(s => s.SaveUploadedFileId(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
            Assert.AreEqual(inventoryFileId, saveUploadedFileIdCalls[0].Item1);
            Assert.AreEqual(testSavedSharedFolderFileId, saveUploadedFileIdCalls[0].Item2);
        }

        [Test]
        public void WriteErrorFileToDisk()
        {
            // Arrange
            const string username = "TestUser";
            var testDateTimeNow = new DateTime(2021, 10, 29, 12, 1, 35);
            const int inventoryFileId = 23;
            const string fileName = "TestFileName.xml";
            const string errorFileName = "TestFileName.xml.txt";
            var validationErrors = new List<string> {"Error One", "Error Two", "Error Three"};
            var testSavedSharedFolderFileId = new Guid("5CB9C249-21EE-4F93-9602-19E31013EA05");

            _DateTimeEngine.Setup(s => s.GetCurrentMoment())
                .Returns(testDateTimeNow);
            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns(@"C:\Temp");

            var savedSharedFolderFiles = new List<SharedFolderFile>();
            _SharedFolderService.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Callback<SharedFolderFile>((f) => savedSharedFolderFiles.Add(f))
                .Returns(testSavedSharedFolderFileId);

            var saveErrorFileIdCalls = new List<Tuple<int, Guid>>();
            _InventoryFileRepositoryMock.Setup(s => s.SaveErrorFileId(It.IsAny<int>(), It.IsAny<Guid>()))
                .Callback<int, Guid>((i, g) => saveErrorFileIdCalls.Add(new Tuple<int, Guid>(i, g)));

            // Act
            _InventoryService._WriteErrorFileToDisk(inventoryFileId, fileName, validationErrors, username);

            // Assert
            // verify the request mapped to the shared folder correctly
            _SharedFolderService.Verify(s => s.SaveFile(It.IsAny<SharedFolderFile>()), Times.Once);
            Assert.AreEqual(errorFileName, savedSharedFolderFiles[0].FileNameWithExtension);
            Assert.IsTrue(savedSharedFolderFiles[0].FileContent.Length > 0);

            // verify the job saved with the shared id
            _InventoryFileRepositoryMock.Verify(s => s.SaveErrorFileId(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
            Assert.AreEqual(inventoryFileId, saveErrorFileIdCalls[0].Item1);
            Assert.AreEqual(testSavedSharedFolderFileId, saveErrorFileIdCalls[0].Item2);
        }
    }
}
