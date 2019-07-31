using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Scx;
using Services.Broadcast.Entities.Scx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests.Wpf;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.UnitTests.ScxFileGeneration
{
    public class ScxGenerationServiceUnitTests
    {
        #region Constructor

        [Test]
        public void ConstructorTest()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();

            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object, 
                proprietaryInventoryService.Object,
                fileService.Object);

            Assert.IsNotNull(tc);
        }

        #endregion // #region Constructor

        #region GetScxFileGenerationHistory

        [Test]
        public void GetScxFileGenerationHistoryHappyPath()
        {
            var sourceId = 7;
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var dropFolder = "thisFolder";
            var historian = new Mock<IScxFileGenerationHistorian>();
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetail>
            {
                new ScxFileGenerationDetail(),
                new ScxFileGenerationDetail(),
                new ScxFileGenerationDetail()
            };
            historian.Setup(s => s.GetScxFileGenerationHistory(It.IsAny<int>()))
                .Callback<int>((s) => getHistoryCalls.Add(s))
                .Returns(getHistoryReturn);
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object,
                fileService.Object)
            {
                DropFolderPath = dropFolder,
                ScxFileGenerationHistorian = historian.Object
            };

            var result = tc.GetScxFileGenerationHistory(sourceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, getHistoryCalls.Count);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void GetScxFileGenerationHistoryWithException()
        {
            var sourceId = 7;
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var dropFolder = "thisFolder";
            var historian = new Mock<IScxFileGenerationHistorian>();
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetail>
            {
                new ScxFileGenerationDetail(),
                new ScxFileGenerationDetail(),
                new ScxFileGenerationDetail()
            };
            historian.Setup(s => s.GetScxFileGenerationHistory(It.IsAny<int>()))
                .Callback<int>((s) =>
                {
                    getHistoryCalls.Add(s);
                    throw new Exception("Exception from GetScxFileGenerationHistory.");
                })
                .Returns(getHistoryReturn);
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object,
                fileService.Object)
            {
                DropFolderPath = dropFolder,
                ScxFileGenerationHistorian = historian.Object
            };
            Exception caught = null;

            try
            {
                tc.GetScxFileGenerationHistory(sourceId);
            }
            catch (Exception e)
            {
                caught = e;
            }

            Assert.IsNotNull(caught);
            Assert.AreEqual(1, getHistoryCalls.Count);
        }

        #endregion // #region GetScxFileGenerationHistory

        #region DownloadGeneratedScxFile

        [Test]
        public void DownloadGeneratedScxFile()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var dropFolder = "thisFolder";
            var historian = new Mock<IScxFileGenerationHistorian>();
            var scxRepo = new Mock<IScxGenerationJobRepository>();
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object,
                fileService.Object)
            {
                DropFolderPath = dropFolder,
                ScxFileGenerationHistorian = historian.Object,
                ScxGenerationJobRepository = scxRepo.Object
            };
            var getScxFileNameCallCount = 0;
            scxRepo.Setup(s => s.GetScxFileName(It.IsAny<int>()))
                .Callback(() => getScxFileNameCallCount++)
                .Returns("fileTwo.txt");
            var getFilesCallCount = 0;
            var getFilesReturn = new List<string>
            {
                Path.Combine(dropFolder, "fileOne.txt"),
                Path.Combine(dropFolder, "fileTwo.txt"),
                Path.Combine(dropFolder, "fileThree.txt")
            };
            fileService.Setup(s => s.GetFiles(It.IsAny<string>()))
                .Callback(() => getFilesCallCount++)
                .Returns(getFilesReturn);
            var getFileStreamCallCount = 0;
            fileService.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Callback(() => getFileStreamCallCount++)
                .Returns(new MemoryStream());

            var result = tc.DownloadGeneratedScxFile(2);

            Assert.IsNotNull(result);
            Assert.AreEqual("fileTwo.txt", result.Item1);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual("text/plain", result.Item3);
            Assert.AreEqual(1, getFilesCallCount);
            Assert.AreEqual(1, getScxFileNameCallCount);
            Assert.AreEqual(1, getFileStreamCallCount);
        }

        [Test]
        public void DownloadGeneratedScxFileWithFileNotFound()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var dropFolder = "thisFolder";
            var historian = new Mock<IScxFileGenerationHistorian>();
            var scxRepo = new Mock<IScxGenerationJobRepository>();
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object,
                fileService.Object)
            {
                DropFolderPath = dropFolder,
                ScxFileGenerationHistorian = historian.Object,
                ScxGenerationJobRepository = scxRepo.Object
            };
            var getScxFileNameCallCount = 0;
            scxRepo.Setup(s => s.GetScxFileName(It.IsAny<int>()))
                .Callback(() => getScxFileNameCallCount++)
                .Returns("fileUnfound.txt");
            var getFilesCallCount = 0;
            var getFilesReturn = new List<string>
            {
                Path.Combine(dropFolder, "fileOne.txt"),
                Path.Combine(dropFolder, "fileTwo.txt"),
                Path.Combine(dropFolder, "fileThree.txt")
            };
            fileService.Setup(s => s.GetFiles(It.IsAny<string>()))
                .Callback(() => getFilesCallCount++)
                .Returns(getFilesReturn);
            var getFileStreamCallCount = 0;
            fileService.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Callback(() => getFileStreamCallCount++)
                .Returns(new MemoryStream());

            var caught = Assert.Throws<Exception>(() => tc.DownloadGeneratedScxFile(2));

            Assert.AreEqual("File not found!", caught.Message);
            Assert.AreEqual(1, getFilesCallCount);
            Assert.AreEqual(1, getScxFileNameCallCount);
            Assert.AreEqual(0, getFileStreamCallCount);
        }

        #endregion // #region DownloadGeneratedScxFile
    }
}