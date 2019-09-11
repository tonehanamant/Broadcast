using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests.Wpf;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
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
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();

            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object, 
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object);

            Assert.IsNotNull(tc);
        }

        #endregion // #region Constructor

        #region GetScxFileGenerationHistory

        [Test]
        public void GetScxFileGenerationHistoryHappyPath()
        {
            var sourceId = 7;
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var dropFolder = "thisFolder";
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetailDto>
            {
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw(),
            };
            scxGenerationJobRepository.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((s) => getHistoryCalls.Add(s))
                .Returns(getHistoryReturn);
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder
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
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var dropFolder = "thisFolder";
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetailDto>
            {
                new ScxFileGenerationDetailDto(),
                new ScxFileGenerationDetailDto(),
                new ScxFileGenerationDetailDto()
            };
            scxGenerationJobRepository.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((s) =>
                {
                    getHistoryCalls.Add(s);
                    throw new Exception("Exception from GetScxFileGenerationHistory.");
                })
                .Returns(getHistoryReturn);

            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder
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

        [Test]
        public void GetScxFileGenerationHistoryWithNoData()
        {
            var sourceId = 7;
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var dropFolder = "thisFolder";
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetailDto>();
            scxGenerationJobRepository.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((s) => getHistoryCalls.Add(s))
                .Returns(getHistoryReturn);
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder
            };

            var result = tc.GetScxFileGenerationHistory(sourceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, getHistoryCalls.Count);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetScxFileGenerationHistoryWithData()
        {
            var sourceId = 7;
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var dropFolder = "thisFolder";
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetailDto>
            {
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw()
            };
            scxGenerationJobRepository.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((s) => getHistoryCalls.Add(s))
                .Returns(getHistoryReturn);
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder
            };

            var result = tc.GetScxFileGenerationHistory(sourceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, getHistoryCalls.Count);
            Assert.AreEqual(3, getHistoryReturn.Count);
        }

        #endregion // #region GetScxFileGenerationHistory

        #region DownloadGeneratedScxFile

        [Test]
        public void DownloadGeneratedScxFile()
        {
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var dropFolder = "thisFolder";
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();

            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder,
            };
            var getScxFileNameCallCount = 0;
            scxGenerationJobRepository.Setup(s => s.GetScxFileName(It.IsAny<int>()))
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
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var dropFolder = "thisFolder";
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder
            };
            var getScxFileNameCallCount = 0;
            scxGenerationJobRepository.Setup(s => s.GetScxFileName(It.IsAny<int>()))
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

        #region ProcessScxGenerationJob
        [Test]
        public void ProcessScxGenerationJobUpdatesStatus()
        {
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var dropFolder = "thisFolder";
            var updateJobCallCount = 0;
            var testJob = new Mock<ScxGenerationJob>();
            testJob.Object.Status = BackgroundJobProcessingStatus.Queued;
            scxGenerationJobRepository.Setup(x => x.UpdateJob(testJob.Object))
                .Callback(() => updateJobCallCount++);
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder
            };
            var testFiles = new Mock<List<InventoryScxFile>>();
            proprietaryInventoryService.Setup(x => x.GenerateScxFiles(testJob.Object.InventoryScxDownloadRequest))
                .Returns(testFiles.Object);

            tc.ProcessScxGenerationJob(testJob.Object, new DateTime(2019, 8, 23));

            Assert.AreEqual(2, updateJobCallCount);
            Assert.AreEqual(BackgroundJobProcessingStatus.Succeeded, testJob.Object.Status);
        }
        #endregion

        #region Ensure ProcessScxGenerationJob Enqueued
        [Test]
        public void ProcessScxGenerationJobEnqueued()
        {
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var dropFolder = "thisFolder";
            var updateJobCallCount = 0;
            var testJob = new Mock<ScxGenerationJob>();
            testJob.Object.Status = BackgroundJobProcessingStatus.Queued;
            scxGenerationJobRepository.Setup(x => x.UpdateJob(testJob.Object))
                .Callback(() => updateJobCallCount++);
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder
            };
            var testFiles = new Mock<List<InventoryScxFile>>();
            proprietaryInventoryService.Setup(x => x.GenerateScxFiles(testJob.Object.InventoryScxDownloadRequest))
                .Returns(testFiles.Object);

            var inventoryScxDownloadRequest = new Mock<InventoryScxDownloadRequest>();

            tc.QueueScxGenerationJob(inventoryScxDownloadRequest.Object, "UnitTestUser", DateTime.Now);

            backgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "ProcessScxGenerationJob"),
                It.IsAny<EnqueuedState>()));
        }
        #endregion

        #region Transformation

        [Test]
        public void TransformFromDtoToEntityOfRawHappyPath()
        {
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var quartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var dropFolder = "thisFolder";
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, quartersCalculationEngine.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder
            };

            const int processingStatusRaw = 1;
            var startDate = new DateTime();
            var endDate = new DateTime();
            var dto = GetPopulatedDetail(startDate, endDate, processingStatusRaw);
            var getAllQuartersBetweenDatesResults = new List<QuarterDetailDto>
            {
                new QuarterDetailDto {Quarter = 1, Year = 2019}
            };
            var getAllQuartersBetweenDatesCalls = new List<Tuple<DateTime, DateTime>>();

            quartersCalculationEngine.Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((s, e) => getAllQuartersBetweenDatesCalls.Add(new Tuple<DateTime, DateTime>(s, e)))
                .Returns(getAllQuartersBetweenDatesResults);

            var result = tc.UT_TransformFromDtoToEntity(dto);

            Assert.IsNotNull(result);
            AssertBaseTransform(dto, result);
            AssertCalculatedFields(result, BackgroundJobProcessingStatus.Queued, getAllQuartersBetweenDatesResults.Count);
            Assert.AreEqual(1, getAllQuartersBetweenDatesCalls.Count);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item1, startDate);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item2, endDate);
        }

        [Test]
        public void FinalizeTransformationOfRawWithMultipleQuarters()
        {
            var scxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(scxGenerationJobRepository.Object);
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var calculator = new Mock<IQuarterCalculationEngine>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            var dropFolder = "thisFolder";
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object, fileService.Object, calculator.Object,
                backgroundJobClient.Object)
            {
                DropFolderPath = dropFolder
            };
            const int processingStatusRaw = 1;
            var startDate = new DateTime();
            var endDate = new DateTime();
            var dto = GetPopulatedDetail(startDate, endDate, processingStatusRaw);
            var getAllQuartersBetweenDatesResults = new List<QuarterDetailDto>
            {
                new QuarterDetailDto {Quarter = 1, Year = 2019},
                new QuarterDetailDto {Quarter = 2, Year = 2019},
                new QuarterDetailDto {Quarter = 3, Year = 2019},
                new QuarterDetailDto {Quarter = 4, Year = 2019},
                new QuarterDetailDto {Quarter = 1, Year = 2018},
                new QuarterDetailDto {Quarter = 2, Year = 2018},
                new QuarterDetailDto {Quarter = 3, Year = 2018},
                new QuarterDetailDto {Quarter = 4, Year = 2018},
                new QuarterDetailDto {Quarter = 4, Year = 2017}
            };
            var getAllQuartersBetweenDatesCalls = new List<Tuple<DateTime, DateTime>>();
            calculator.Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((s, e) => getAllQuartersBetweenDatesCalls.Add(new Tuple<DateTime, DateTime>(s, e)))
                .Returns(getAllQuartersBetweenDatesResults);

            var result = tc.UT_TransformFromDtoToEntity(dto);

            Assert.IsNotNull(result);
            AssertBaseTransform(dto, result);
            AssertCalculatedFields(result, BackgroundJobProcessingStatus.Queued, getAllQuartersBetweenDatesResults.Count);
            Assert.AreEqual(1, getAllQuartersBetweenDatesCalls.Count);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item1, startDate);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item2, endDate);
        }

        private ScxFileGenerationDetailDto GetPopulatedDetailRaw()
        {
            var detail = new ScxFileGenerationDetailDto
            {
                GenerationRequestDateTime = new DateTime(2017, 10, 17, 19, 30, 3),
                GenerationRequestedByUsername = "SomeGuy",
                FileId = 12,
                UnitName = "IUnit",
                DaypartCode = "EMN",
                StartDateTime = new DateTime(2017, 10, 17, 19, 30, 3),
                EndDateTime = new DateTime(2017, 11, 17, 19, 30, 3),
                ProcessingStatusId = 1
            };
            return detail;
        }

        private ScxFileGenerationDetailDto GetPopulatedDetail(DateTime startDate, DateTime endDate, int processingStatusId)
        {
            var raw = new ScxFileGenerationDetailDto
            {
                GenerationRequestDateTime = new DateTime(2019, 10, 17, 12, 23, 33),
                GenerationRequestedByUsername = "TestUser",
                FileId = 12,
                UnitName = "U1",
                DaypartCode = "EMN",
                ProcessingStatusId = processingStatusId,
                StartDateTime = startDate,
                EndDateTime = endDate
            };
            return raw;
        }

        private void AssertBaseTransform(ScxFileGenerationDetailDto dto, ScxFileGenerationDetail entity)
        {
            Assert.AreEqual(dto.GenerationRequestDateTime, entity.GenerationRequestDateTime);
            Assert.AreEqual(dto.GenerationRequestedByUsername, entity.GenerationRequestedByUsername);
            Assert.AreEqual(dto.UnitName, entity.UnitName);
            Assert.AreEqual(dto.FileId, entity.FileId);
            Assert.AreEqual(dto.DaypartCode, entity.DaypartCode);
        }

        private void AssertCalculatedFields(ScxFileGenerationDetail entity, BackgroundJobProcessingStatus expectedStatus, int expectedQuartersCount)
        {
            Assert.AreEqual(expectedStatus, entity.ProcessingStatus);
            Assert.AreEqual(expectedQuartersCount, entity.QuarterDetails.Count);
        }

        #endregion // #region Transformation
    }
}