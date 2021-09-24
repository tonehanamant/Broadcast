using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
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

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class InventoryServiceUnitTests
    {
        private readonly Mock<IDataRepositoryFactory> _BroadcastDataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
        private readonly Mock<IInventoryFileValidator> _InventoryFileValidatorMock = new Mock<IInventoryFileValidator>();
        private readonly Mock<IDaypartCache> _DaypartCacheMock = new Mock<IDaypartCache>();
        private readonly Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
        private readonly Mock<ISMSClient> _SmsClientMock = new Mock<ISMSClient>();
        private readonly Mock<IProprietarySpotCostCalculationEngine> _ProprietarySpotCostCalculationEngineMock = new Mock<IProprietarySpotCostCalculationEngine>();
        private readonly Mock<IStationInventoryGroupService> _StationInventoryGroupServiceMock = new Mock<IStationInventoryGroupService>();
        private readonly Mock<IBroadcastAudiencesCache> _AudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
        private readonly Mock<IRatingForecastService> _RatingForecastServiceMock = new Mock<IRatingForecastService>();
        private readonly Mock<INsiPostingBookService> _NsiPostingBookServiceMock = new Mock<INsiPostingBookService>();
        private readonly Mock<ILockingEngine> _LockingEngineMock = new Mock<ILockingEngine>();
        private readonly Mock<IStationProcessingEngine> _StationProcessingEngineMock = new Mock<IStationProcessingEngine>();
        private readonly Mock<IImpressionsService> _ImpressionsServiceMock = new Mock<IImpressionsService>();
        private readonly Mock<IOpenMarketFileImporter> _OpenMarketFileImporterMock = new Mock<IOpenMarketFileImporter>();
        private readonly Mock<IFileService> _FileServiceMock = new Mock<IFileService>();
        private readonly Mock<IInventoryRatingsProcessingService> _InventoryRatingsServiceMock = new Mock<IInventoryRatingsProcessingService>();
        private readonly Mock<IInventoryProgramsProcessingService> _InventoryProgramsProcessingServiceMock = new Mock<IInventoryProgramsProcessingService>();
        private readonly Mock<IInventoryRepository> _InventoryRepositoryMock = new Mock<IInventoryRepository>();
        private readonly Mock<ISpotLengthRepository> _SpotLengthRepository = new Mock<ISpotLengthRepository>();
        private readonly Mock<IInventoryFileRepository> _InventoryFileRepositoryMock = new Mock<IInventoryFileRepository>();
        private readonly Mock<IFeatureToggleHelper> _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
        private readonly Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

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

            var service = _GetInventoryService();

            var result = service.GetInventoryUploadHistory(
                inventorySourceId: 1,
                quarter: null,
                year: null);

            Assert.AreEqual(expectedStatus, result.First().Status);
        }

        [Test]
        public void DownloadErrorFile()
        {
            const int fileId = 23;
            const string fileName = "InventoryErrorFile.xlsx";

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
                .Returns(true);

            var getFileStreamCalled = 0;
            _FileServiceMock.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Callback(() => getFileStreamCalled++)
                .Returns(new MemoryStream());

            var service = _GetInventoryService();

            var result = service.DownloadErrorFile(fileId);

            Assert.IsNotNull(result);
            Assert.AreEqual(fileName, result.Item1);
            Assert.IsNotNull(result.Item2); // stream
            Assert.AreEqual(@"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Item3); // mimeType

            Assert.AreEqual(1, getInventoryFileByIdCalledCount);
            Assert.AreEqual(1, service.GetInventoryUploadErrorsFolderCalledCount);
            Assert.AreEqual(1, fileExistsCalledCount);
            Assert.AreEqual(1, getFileStreamCalled);
        }

        [Test]
        public void DownloadErrorFile_XmlFile()
        {
            const int fileId = 23;
            const string fileName = "InventoryErrorFile.xml";

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
                .Returns(true);

            var getFileStreamCalled = 0;
            _FileServiceMock.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Callback(() => getFileStreamCalled++)
                .Returns(new MemoryStream());

            var service = _GetInventoryService();

            var result = service.DownloadErrorFile(fileId);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item2); // stream
            Assert.AreEqual(@"text/plain", result.Item3); // mimeType

            Assert.AreEqual(1, getInventoryFileByIdCalledCount);
            Assert.AreEqual(1, service.GetInventoryUploadErrorsFolderCalledCount);
            Assert.AreEqual(1, fileExistsCalledCount);
            Assert.AreEqual(1, getFileStreamCalled);
        }

        [Test]
        public void DownloadErrorFile_IdNotFound()
        {
            const int fileId = 23;

            var getInventoryFileByIdCalledCount = 0;
            _InventoryFileRepositoryMock.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Callback(() => getInventoryFileByIdCalledCount++)
                .Throws(new Exception("Test message for id not found."));

            var fileExistsCalledCount = 0;
            _FileServiceMock.Setup(s => s.Exists(It.IsAny<string>()))
                .Callback(() => fileExistsCalledCount++)
                .Returns(true);

            var getFileStreamCalled = 0;
            _FileServiceMock.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Callback(() => getFileStreamCalled++)
                .Returns(new MemoryStream());

            var service = _GetInventoryService();

            var caught = Assert.Throws<InvalidOperationException>(() => service.DownloadErrorFile(fileId));

            Assert.AreEqual($"File record for id '{fileId}' not found.", caught.Message);
            Assert.AreEqual(1, getInventoryFileByIdCalledCount);
            Assert.AreEqual(0, service.GetInventoryUploadErrorsFolderCalledCount);
            Assert.AreEqual(0, fileExistsCalledCount);
            Assert.AreEqual(0, getFileStreamCalled);
        }

        [Test]
        public void DownloadErrorFile_FileNotFound()
        {
            const int fileId = 23;
            const string fileName = "InventoryErrorFile.xlsx";

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

            var service = _GetInventoryService();

            var caught = Assert.Throws<FileNotFoundException>(() => service.DownloadErrorFile(fileId));

            Assert.AreEqual($"File '{fileName}' with id '{fileId}' not found.", caught.Message);
            Assert.AreEqual(1, getInventoryFileByIdCalledCount);
            Assert.AreEqual(1, service.GetInventoryUploadErrorsFolderCalledCount);
            Assert.AreEqual(1, fileExistsCalledCount);
            Assert.AreEqual(0, getFileStreamCalled);
        }

        [Test]
        public void DownloadErrorFiles()
        {
            var fileIds = new List<int> { 27, 28, 52 };
            var expectedArchiveFileName = $"InventoryErrorFiles_01242020123007.zip";
            var testDateTimeNow = new DateTime(2020, 01, 24, 12, 30, 07);

            var getInventoryFileByIdCalledCount = 0;
            _InventoryFileRepositoryMock.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Callback(() => getInventoryFileByIdCalledCount++)
                .Returns<int>((fileId) =>  new InventoryFile
                {
                    Id = fileId,
                    FileName = "InventoryErrorFile.xlsx",
                });

            var fileExistsCalledCount = 0;
            _FileServiceMock.Setup(s => s.Exists(It.IsAny<string>()))
                .Callback(() => fileExistsCalledCount++)
                .Returns(true);

            var createZipArchiveCalled = 0;
            _FileServiceMock.Setup(s => s.CreateZipArchive(It.IsAny<Dictionary<string, string>>()))
                .Callback(() => createZipArchiveCalled++)
                .Returns(new MemoryStream());

            var service = _GetInventoryService();
            service.UTDateTimeNow = testDateTimeNow;

            var result = service.DownloadErrorFiles(fileIds);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedArchiveFileName, result.Item1);
            Assert.IsNotNull(result.Item2); // stream

            Assert.AreEqual(3, getInventoryFileByIdCalledCount);
            Assert.AreEqual(3, service.GetInventoryUploadErrorsFolderCalledCount);
            Assert.AreEqual(3, fileExistsCalledCount);
            Assert.AreEqual(1, createZipArchiveCalled);
        }

        private void _SetupInventoryServiceDependencies()
        {
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
        }

        private InventoryServiceUnitTestClass _GetInventoryService()
        {
            _SetupInventoryServiceDependencies();

            return new InventoryServiceUnitTestClass(
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
                _InventoryRatingsServiceMock.Object,
                _InventoryProgramsProcessingServiceMock.Object, _FeatureToggleMock.Object, _ConfigurationSettingsHelperMock.Object);
        }
    }
}
