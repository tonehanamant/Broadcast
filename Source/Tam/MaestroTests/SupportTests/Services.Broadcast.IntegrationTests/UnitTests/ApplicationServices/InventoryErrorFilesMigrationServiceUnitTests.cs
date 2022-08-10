using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class InventoryErrorFilesMigrationServiceUnitTests
    {
        const string TEST_USERNAME = "UnitTestUser";

        private InventoryErrorFilesMigrationService _Service;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISharedFolderService> _SharedFolderService;
        private Mock<IFileService> _FileService;

        private Mock<IInventoryFileRepository> _InventoryFileRepository;
        private Mock<IInventoryRepository> _InventoryRepository;

        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        [SetUp]
        public void Setup()
        {
            _InventoryFileRepository = new Mock<IInventoryFileRepository>();
            _InventoryRepository = new Mock<IInventoryRepository>();

            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            
            _DataRepositoryFactoryMock.Setup(s => s.GetDataRepository<IInventoryFileRepository>())
                .Returns(_InventoryFileRepository.Object);
            _DataRepositoryFactoryMock.Setup(s => s.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepository.Object);

            _SharedFolderService = new Mock<ISharedFolderService>();
            _FileService = new Mock<IFileService>();

            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _Service = new InventoryErrorFilesMigrationService(
                _DataRepositoryFactoryMock.Object,
                _SharedFolderService.Object,
                _FileService.Object,
                _DateTimeEngineMock.Object,
                _FeatureToggleMock.Object,
                _ConfigurationSettingsHelperMock.Object
                );

            // Defaults
            _FeatureToggleMock.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION))
                .Returns(true);
            _FeatureToggleMock.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_ATTACHMENT_MICRO_SERVICE))
                .Returns(true);
            _ConfigurationSettingsHelperMock.Setup(s => s.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder))
                .Returns("ConfiguredBaseFolderPath");

            _DateTimeEngineMock.Setup(s => s.GetCurrentMoment())
                .Returns(new DateTime(2017,10,17,7,25,32));
        }

        [Test]
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void Migrate_TogglesDisabled(bool consolidationEnabled, bool migrationEnabled)
        {
            // Arrange
            _FeatureToggleMock.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION))
                .Returns(consolidationEnabled);
            _FeatureToggleMock.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_ATTACHMENT_MICRO_SERVICE))
                .Returns(migrationEnabled);

            // Act
            var result = _Service.MigrateFilesToAttachmentService(TEST_USERNAME);

            // Assert
            // if this wasn't run then the rest wasn't run either.
            _InventoryRepository.Verify(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null), Times.Never);
            Assert.AreEqual(consolidationEnabled, result.ConsolidationEnabled);
            Assert.AreEqual(migrationEnabled, result.MigrationEnabled);
        }

        [Test]
        public void Migrate_NoHistory()
        {
            // Arrange
            _InventoryRepository.Setup(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null))
                .Returns(new List<InventoryUploadHistory>());

            // Act
            var result = _Service.MigrateFilesToAttachmentService(TEST_USERNAME);

            // Assert
            _InventoryRepository.Verify(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null), Times.Once);
            _InventoryFileRepository.Verify(s => s.GetInventoryFileById(It.IsAny<int>()), Times.Never);
            Assert.AreEqual(0, result.TotalFileHistoryCount);
            Assert.AreEqual(0, result.TotalErrorFileHistoryCount);
        }

        [Test]
        public void Migrate_NoErrorsInHistory()
        {
            // Arrange
            _InventoryRepository.Setup(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null))
                .Returns(new List<InventoryUploadHistory>
                {
                    new InventoryUploadHistory { FileId = 1, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 2, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 3, FileLoadStatus = FileStatusEnum.Loaded },
                });

            // Act
            var result = _Service.MigrateFilesToAttachmentService(TEST_USERNAME);

            // Assert
            _InventoryRepository.Verify(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null), Times.Once);
            _InventoryFileRepository.Verify(s => s.GetInventoryFileById(It.IsAny<int>()), Times.Never);
            Assert.AreEqual(3, result.TotalFileHistoryCount);
            Assert.AreEqual(0, result.TotalErrorFileHistoryCount);
        }

        [Test]
        public void Migrate_FileConsolidatedAndMigrated()
        {
            var fileName = "OpenMarketInventoryFileWithError.xml";
            var errorFileId = 666;
            var errorFileSharedFolderFileId = new Guid("EE4DC0DD-7AEC-4B46-95C2-E8F714C841AE");
            var attachmentId = new Guid("96B64E51-B37E-47A8-9ED7-E8C25932100C");

            var errorInventoryFile = new InventoryFile
            {
                Id = errorFileId,
                ErrorFileSharedFolderFileId = errorFileSharedFolderFileId,
                FileName = fileName,
                FileStatus = FileStatusEnum.Failed
            };
            var errorSharedFolderFile = new SharedFolderFile
            {
                AttachmentId = attachmentId,
                FileNameWithExtension = fileName
            };

            // Arrange
            _InventoryRepository.Setup(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null))
                .Returns(new List<InventoryUploadHistory>
                {
                    new InventoryUploadHistory { FileId = 1, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 2, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 3, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = errorFileId, FileLoadStatus = FileStatusEnum.Failed },
                });
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(errorFileId))
                .Returns(errorInventoryFile);
            _SharedFolderService.Setup(s => s.GetFileInfo(errorFileSharedFolderFileId))
                .Returns(errorSharedFolderFile);

            // Act
            var result = _Service.MigrateFilesToAttachmentService(TEST_USERNAME);

            // Assert
            _FileService.Verify(s => s.GetFileStream(It.IsAny<string>()), Times.Never);
            _SharedFolderService.Verify(s => s.SaveFile(It.IsAny<SharedFolderFile>()), Times.Never);
            _InventoryFileRepository.Verify(s => s.SaveErrorFileId(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);

            Assert.True(result.ConsolidationEnabled);
            Assert.True(result.MigrationEnabled);
            Assert.AreEqual(4, result.TotalFileHistoryCount);
            Assert.AreEqual(1, result.TotalErrorFileHistoryCount);
            Assert.AreEqual(1, result.AlreadyMigratedFileNames.Count);
        }

        [Test]
        public void Migrate_FileConsolidatedButNotMigrated()
        {
            var fileName = "OpenMarketInventoryFileWithError.xml";
            var errorFileId = 666;
            var errorFileSharedFolderFileId = new Guid("EE4DC0DD-7AEC-4B46-95C2-E8F714C841AE");
            Guid? attachmentId = null;
            var newErrorFileSharedFolderFileId = new Guid("13C3DB14-89E4-4E5B-AEDC-3E3E534F1A01");

            var errorInventoryFile = new InventoryFile
            {
                Id = errorFileId,
                ErrorFileSharedFolderFileId = errorFileSharedFolderFileId,
                FileName = fileName,
                FileStatus = FileStatusEnum.Failed
            };
            var errorSharedFolderFile = new SharedFolderFile
            {
                AttachmentId = attachmentId,
                FileNameWithExtension = fileName
            };

            // Arrange
            _InventoryRepository.Setup(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null))
                .Returns(new List<InventoryUploadHistory>
                {
                    new InventoryUploadHistory { FileId = 1, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 2, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 3, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = errorFileId, FileLoadStatus = FileStatusEnum.Failed },
                });
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(errorFileId))
                .Returns(errorInventoryFile);
            _SharedFolderService.Setup(s => s.GetFileInfo(errorFileSharedFolderFileId))
                .Returns(errorSharedFolderFile);
            _FileService.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Returns(new MemoryStream());
            _SharedFolderService.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Returns(newErrorFileSharedFolderFileId);
            _InventoryFileRepository.Setup(s => s.SaveErrorFileId(errorFileId, newErrorFileSharedFolderFileId));                

            // Act
            var result = _Service.MigrateFilesToAttachmentService(TEST_USERNAME);

            // Assert
            _SharedFolderService.Verify(s => s.SaveFile(It.IsAny<SharedFolderFile>()), Times.Once);
            _InventoryFileRepository.Verify(s => s.SaveErrorFileId(errorFileId, newErrorFileSharedFolderFileId), Times.Once);

            Assert.True(result.ConsolidationEnabled);
            Assert.True(result.MigrationEnabled);
            Assert.AreEqual(4, result.TotalFileHistoryCount);
            Assert.AreEqual(1, result.TotalErrorFileHistoryCount);
            Assert.AreEqual(0, result.AlreadyMigratedFileNames.Count);
            Assert.AreEqual(1, result.MigratedFileNames.Count);
        }

        [Test]
        public void Migrate_FileNotConsolidatedAndNotMigrated()
        {
            var fileName = "OpenMarketInventoryFileWithError.xml";
            var errorFileId = 666;
            Guid? errorFileSharedFolderFileId = null;
            var newErrorFileSharedFolderFileId = new Guid("13C3DB14-89E4-4E5B-AEDC-3E3E534F1A01");

            var errorInventoryFile = new InventoryFile
            {
                Id = errorFileId,
                ErrorFileSharedFolderFileId = errorFileSharedFolderFileId,
                FileName = fileName,
                FileStatus = FileStatusEnum.Failed
            };

            // Arrange
            _InventoryRepository.Setup(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null))
                .Returns(new List<InventoryUploadHistory>
                {
                    new InventoryUploadHistory { FileId = 1, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 2, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 3, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = errorFileId, FileLoadStatus = FileStatusEnum.Failed },
                });
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(errorFileId))
                .Returns(errorInventoryFile);

            _FileService.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Returns(new MemoryStream());
            _SharedFolderService.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Returns(newErrorFileSharedFolderFileId);
            _InventoryFileRepository.Setup(s => s.SaveErrorFileId(errorFileId, newErrorFileSharedFolderFileId));

            // Act
            var result = _Service.MigrateFilesToAttachmentService(TEST_USERNAME);

            // Assert
            _SharedFolderService.Verify(s => s.GetFileInfo(It.IsAny<Guid>()), Times.Never);
            _SharedFolderService.Verify(s => s.SaveFile(It.IsAny<SharedFolderFile>()), Times.Once);
            _InventoryFileRepository.Verify(s => s.SaveErrorFileId(errorFileId, newErrorFileSharedFolderFileId), Times.Once);

            Assert.True(result.ConsolidationEnabled);
            Assert.True(result.MigrationEnabled);
            Assert.AreEqual(4, result.TotalFileHistoryCount);
            Assert.AreEqual(1, result.TotalErrorFileHistoryCount);
            Assert.AreEqual(0, result.AlreadyMigratedFileNames.Count);
            Assert.AreEqual(1, result.MigratedFileNames.Count);
        }

        [Test]
        public void Migrate_ErrorGettingConsolidatedFileContent()
        {
            var fileName = "OpenMarketInventoryFileWithError.xml";
            var errorFileId = 666;
            var errorFileSharedFolderFileId = new Guid("EE4DC0DD-7AEC-4B46-95C2-E8F714C841AE");

            var errorInventoryFile = new InventoryFile
            {
                Id = errorFileId,
                ErrorFileSharedFolderFileId = errorFileSharedFolderFileId,
                FileName = fileName,
                FileStatus = FileStatusEnum.Failed
            };

            // Arrange
            _InventoryRepository.Setup(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null))
                .Returns(new List<InventoryUploadHistory>
                {
                    new InventoryUploadHistory { FileId = 1, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 2, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 3, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = errorFileId, FileLoadStatus = FileStatusEnum.Failed },
                });
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(errorFileId))
                .Returns(errorInventoryFile);
            _SharedFolderService.Setup(s => s.GetFileInfo(errorFileSharedFolderFileId))
                .Throws<Exception>();

            // Act
            var result = _Service.MigrateFilesToAttachmentService(TEST_USERNAME);

            // Assert
            _FileService.Verify(s => s.GetFileStream(It.IsAny<string>()), Times.Never);
            _SharedFolderService.Verify(s => s.SaveFile(It.IsAny<SharedFolderFile>()), Times.Never);
            _InventoryFileRepository.Verify(s => s.SaveErrorFileId(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);

            Assert.True(result.ConsolidationEnabled);
            Assert.True(result.MigrationEnabled);
            Assert.AreEqual(4, result.TotalFileHistoryCount);
            Assert.AreEqual(1, result.TotalErrorFileHistoryCount);
            Assert.AreEqual(1, result.FileNotFoundFileNames.Count);
        }

        [Test]
        public void Migrate_ErrorGettingNotConsolidatedFile()
        {
            var fileName = "OpenMarketInventoryFileWithError.xml";
            var errorFileId = 666;
            Guid? errorFileSharedFolderFileId = null;
            var newErrorFileSharedFolderFileId = new Guid("13C3DB14-89E4-4E5B-AEDC-3E3E534F1A01");

            var errorInventoryFile = new InventoryFile
            {
                Id = errorFileId,
                ErrorFileSharedFolderFileId = errorFileSharedFolderFileId,
                FileName = fileName,
                FileStatus = FileStatusEnum.Failed
            };

            // Arrange
            _InventoryRepository.Setup(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null))
                .Returns(new List<InventoryUploadHistory>
                {
                    new InventoryUploadHistory { FileId = 1, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 2, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 3, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = errorFileId, FileLoadStatus = FileStatusEnum.Failed },
                });
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(errorFileId))
                .Returns(errorInventoryFile);

            _FileService.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Throws<Exception>();

            // Act
            var result = _Service.MigrateFilesToAttachmentService(TEST_USERNAME);

            // Assert
            _SharedFolderService.Verify(s => s.GetFile(It.IsAny<Guid>()), Times.Never);
            _SharedFolderService.Verify(s => s.SaveFile(It.IsAny<SharedFolderFile>()), Times.Never);
            _InventoryFileRepository.Verify(s => s.SaveErrorFileId(errorFileId, newErrorFileSharedFolderFileId), Times.Never);

            Assert.True(result.ConsolidationEnabled);
            Assert.True(result.MigrationEnabled);
            Assert.AreEqual(4, result.TotalFileHistoryCount);
            Assert.AreEqual(1, result.TotalErrorFileHistoryCount);
            Assert.AreEqual(1, result.FileNotFoundFileNames.Count);
        }

        [Test]
        public void Migrate_ErrorSavingFile()
        {
            var fileName = "OpenMarketInventoryFileWithError.xml";
            var errorFileId = 666;
            Guid? errorFileSharedFolderFileId = null;
            var newErrorFileSharedFolderFileId = new Guid("13C3DB14-89E4-4E5B-AEDC-3E3E534F1A01");

            var errorInventoryFile = new InventoryFile
            {
                Id = errorFileId,
                ErrorFileSharedFolderFileId = errorFileSharedFolderFileId,
                FileName = fileName,
                FileStatus = FileStatusEnum.Failed
            };

            // Arrange
            _InventoryRepository.Setup(s => s.GetInventoryUploadHistoryForInventorySource((int)InventorySourceEnum.OpenMarket, null, null))
                .Returns(new List<InventoryUploadHistory>
                {
                    new InventoryUploadHistory { FileId = 1, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 2, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = 3, FileLoadStatus = FileStatusEnum.Loaded },
                    new InventoryUploadHistory { FileId = errorFileId, FileLoadStatus = FileStatusEnum.Failed },
                });
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(errorFileId))
                .Returns(errorInventoryFile);

            _FileService.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Returns(new MemoryStream());
            _SharedFolderService.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Throws<Exception>();

            // Act
            var result = _Service.MigrateFilesToAttachmentService(TEST_USERNAME);

            // Assert
            _SharedFolderService.Verify(s => s.GetFile(It.IsAny<Guid>()), Times.Never);
            _SharedFolderService.Verify(s => s.SaveFile(It.IsAny<SharedFolderFile>()), Times.Once);
            _InventoryFileRepository.Verify(s => s.SaveErrorFileId(errorFileId, newErrorFileSharedFolderFileId), Times.Never);

            Assert.True(result.ConsolidationEnabled);
            Assert.True(result.MigrationEnabled);
            Assert.AreEqual(4, result.TotalFileHistoryCount);
            Assert.AreEqual(1, result.TotalErrorFileHistoryCount);
            Assert.AreEqual(0, result.MigratedFileNames.Count);
            Assert.AreEqual(1, result.FailedToSaveToAttachmentService.Count);
        }
    }
}
