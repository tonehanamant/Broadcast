using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.IO;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class SharedFolderServiceUnitTests
    {
        private readonly ISharedFolderService _SharedFolderService;
        private readonly Mock<IFileService> _FileServiceMock = new Mock<IFileService>();
        private readonly Mock<IDataRepositoryFactory> _DataRepositoryFactory = new Mock<IDataRepositoryFactory>();
        private readonly Mock<ISharedFolderFilesRepository> _SharedFolderFilesRepository = new Mock<ISharedFolderFilesRepository>();

        public SharedFolderServiceUnitTests()
        {
            _FileServiceMock
                .Setup(x => x.GetFileStream(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(It.IsAny<Stream>());

            _DataRepositoryFactory
                .Setup(x => x.GetDataRepository<ISharedFolderFilesRepository>())
                .Returns(_SharedFolderFilesRepository.Object);

            _SharedFolderService = new SharedFolderService(
                _FileServiceMock.Object,
                _DataRepositoryFactory.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAndRemoveFile_GetsDataFromDB()
        {
            var fileId = Guid.NewGuid();
            var now = new DateTime(2020, 1, 1);
            var user = "UnitTestsUser";

            _SharedFolderFilesRepository
                .Setup(x => x.GetFileById(It.IsAny<Guid>()))
                .Returns(new SharedFolderFile
                {
                    Id = fileId,
                    FolderPath = @"D:\\files",
                    FileName = "report",
                    FileExtension = ".xlsx",
                    FileMediaType = "media-type",
                    FileUsage = 0,
                    CreatedDate = now,
                    CreatedBy = user
                });


            var result = _SharedFolderService.GetAndRemoveFile(fileId);

            _SharedFolderFilesRepository.Verify(x => x.GetFileById(fileId), Times.Exactly(2));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAndRemoveFile_ThrowsException_WhenFileIsNotInDB()
        {
            var fileId = Guid.NewGuid();

            _SharedFolderFilesRepository
                .Setup(x => x.GetFileById(It.IsAny<Guid>()))
                .Returns((SharedFolderFile)null);

            Assert.Throws<Exception>(() => _SharedFolderService.GetAndRemoveFile(fileId), $"There is no file with id: {fileId}");
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAndRemoveFile_GetsFileContent()
        {
            var fileId = Guid.NewGuid();
            var now = new DateTime(2020, 1, 1);
            var user = "UnitTestsUser";

            _SharedFolderFilesRepository
                .Setup(x => x.GetFileById(It.IsAny<Guid>()))
                .Returns(new SharedFolderFile
                {
                    Id = fileId,
                    FolderPath = @"D:\\files",
                    FileName = "report",
                    FileExtension = ".xlsx",
                    FileMediaType = "media-type",
                    FileUsage = 0,
                    CreatedDate = now,
                    CreatedBy = user
                });


            var result = _SharedFolderService.GetAndRemoveFile(fileId);

            _FileServiceMock.Verify(x => x.GetFileStream(@"D:\\files", $"{fileId}.xlsx"), Times.Once);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [TestCase(SharedFolderFileUsage.CampaignExport)]
        public void GetAndRemoveFile_RemovesFileInCertainCases(SharedFolderFileUsage fileUsage)
        {
            var fileId = Guid.NewGuid();
            var now = new DateTime(2020, 1, 1);
            var user = "UnitTestsUser";

            _SharedFolderFilesRepository
                .Setup(x => x.GetFileById(It.IsAny<Guid>()))
                .Returns(new SharedFolderFile
                {
                    Id = fileId,
                    FolderPath = @"D:\\files",
                    FileName = "report",
                    FileExtension = ".xlsx",
                    FileMediaType = "media-type",
                    FileUsage = fileUsage,
                    CreatedDate = now,
                    CreatedBy = user
                });
            
            var result = _SharedFolderService.GetAndRemoveFile(fileId);

            _FileServiceMock.Verify(x => x.Delete($@"D:\\files\{fileId}.xlsx"), Times.Once);
            _SharedFolderFilesRepository.Verify(x => x.RemoveFile(fileId), Times.Once);
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            
            jsonResolver.Ignore(typeof(SharedFolderFile), "Id");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
    }
}
