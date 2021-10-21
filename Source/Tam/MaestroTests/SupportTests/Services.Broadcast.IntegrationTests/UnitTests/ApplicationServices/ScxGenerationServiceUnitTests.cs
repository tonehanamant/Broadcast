﻿using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class ScxGenerationServiceUnitTests
    {
        private Mock<IDataRepositoryFactory> _DataRepositoryFactory;
        private Mock<IProprietaryInventoryService> _ProprietaryInventoryService;
        private Mock<IFileService> _FileService;
        private Mock<IQuarterCalculationEngine> _QuartersCalculationEngine;
        private Mock<IBackgroundJobClient> _BackgroundJobClient;
        private Mock<IFeatureToggleHelper> _FeatureToggle;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelper;
        private Mock<ISharedFolderService> _SharedFolderService;

        private Mock<IScxGenerationJobRepository> _ScxGenerationJobRepository;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            _ProprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            _FileService = new Mock<IFileService>();
            _SharedFolderService = new Mock<ISharedFolderService>(); 
            _QuartersCalculationEngine = new Mock<IQuarterCalculationEngine>();
            _BackgroundJobClient = new Mock<IBackgroundJobClient>();
            _FeatureToggle = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();

            _ScxGenerationJobRepository = new Mock<IScxGenerationJobRepository>();

            _DataRepositoryFactory.Setup(s => s.GetDataRepository<IScxGenerationJobRepository>())
                .Returns(_ScxGenerationJobRepository.Object);
        }

        public ScxGenerationService _GetTestClass()
        {
            var tc = new ScxGenerationService(
                _DataRepositoryFactory.Object,
                _ProprietaryInventoryService.Object,
                _FileService.Object,
                _SharedFolderService.Object,
                _QuartersCalculationEngine.Object,
                _BackgroundJobClient.Object,
                _FeatureToggle.Object,
                _ConfigurationSettingsHelper.Object);

            return tc;
        }

        [Test]
        public void ConstructorTest()
        {
            var tc = _GetTestClass();

            Assert.IsNotNull(tc);
        }

        #region GetScxFileGenerationHistory

        [Test]
        public void GetScxFileGenerationHistoryHappyPath()
        {
            var sourceId = 7;
            
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetailDto>
            {
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw(),
            };
            _ScxGenerationJobRepository.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((s) => getHistoryCalls.Add(s))
                .Returns(getHistoryReturn);

            var tc = _GetTestClass();

            var result = tc.GetScxFileGenerationHistory(sourceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, getHistoryCalls.Count);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void GetScxFileGenerationHistoryWithException()
        {
            var sourceId = 7;
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetailDto>
            {
                new ScxFileGenerationDetailDto(),
                new ScxFileGenerationDetailDto(),
                new ScxFileGenerationDetailDto()
            };
            _ScxGenerationJobRepository.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((s) =>
                {
                    getHistoryCalls.Add(s);
                    throw new Exception("Exception from GetScxFileGenerationHistory.");
                })
                .Returns(getHistoryReturn);

            var tc = _GetTestClass();
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
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetailDto>();
            _ScxGenerationJobRepository.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((s) => getHistoryCalls.Add(s))
                .Returns(getHistoryReturn);
            var tc = _GetTestClass();

            var result = tc.GetScxFileGenerationHistory(sourceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, getHistoryCalls.Count);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetScxFileGenerationHistoryWithData()
        {
            var sourceId = 7;
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetailDto>
            {
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw()
            };
            _ScxGenerationJobRepository.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((s) => getHistoryCalls.Add(s))
                .Returns(getHistoryReturn);
            var tc = _GetTestClass();

            var result = tc.GetScxFileGenerationHistory(sourceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, getHistoryCalls.Count);
            Assert.AreEqual(3, getHistoryReturn.Count);
        }

        #endregion // #region GetScxFileGenerationHistory

        #region DownloadGeneratedScxFile

        [Test]
        [TestCase(true,  true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void DownloadGeneratedScxFileFromFileService(bool enableSharedFileServiceConsolidation, bool existInSharedFolderService)
        {
            // Arrange
            var savedFileGuid = existInSharedFolderService
                    ? new Guid("4FAED53D-759A-4088-9A33-DE2C9107CCC5") 
                    : (Guid?)null;

            _FeatureToggle.Setup(s =>
                    s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION))
                .Returns(enableSharedFileServiceConsolidation);

            _ScxGenerationJobRepository.Setup(s => s.GetScxFileName(It.IsAny<int>()))
                .Returns("fileTwo.txt");
            var dropFolder = "thisFolder";
            var getFilesReturn = new List<string>
            {
                Path.Combine(dropFolder, "fileOne.txt"),
                Path.Combine(dropFolder, "fileTwo.txt"),
                Path.Combine(dropFolder, "fileThree.txt")
            };
            _FileService.Setup(s => s.GetFiles(It.IsAny<string>()))
                .Returns(getFilesReturn);
            _FileService.Setup(s => s.GetFileStream(It.IsAny<string>()))
                .Returns(new MemoryStream());

            _ScxGenerationJobRepository.Setup(s => s.GetSharedFolderFileIdForFile(It.IsAny<int>()))
                .Returns(savedFileGuid);

            _SharedFolderService.Setup(s => s.GetFile(It.IsAny<Guid>()))
                .Returns(new SharedFolderFile { FileName = "fileTwo", FileExtension = ".txt", FileContent = new MemoryStream()});

            var tc = _GetTestClass();

            // Act
            var result = tc.DownloadGeneratedScxFile(2);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("fileTwo.txt", result.Item1);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual("text/plain", result.Item3);

            var shouldCheckSharedFolderService = enableSharedFileServiceConsolidation;
            if (shouldCheckSharedFolderService)
            {
                _ScxGenerationJobRepository.Verify(s => s.GetSharedFolderFileIdForFile(It.IsAny<int>()), Times.Once);
                if (existInSharedFolderService)
                {
                    _SharedFolderService.Verify(s => s.GetFile(It.IsAny<Guid>()), Times.Once);
                }
                else
                {
                    _SharedFolderService.Verify(s => s.GetFile(It.IsAny<Guid>()), Times.Never);
                }
            }

            var shouldHaveCheckedFileService = !enableSharedFileServiceConsolidation || !existInSharedFolderService;
            if (shouldHaveCheckedFileService)
            {
                _ScxGenerationJobRepository.Verify(s => s.GetScxFileName(It.IsAny<int>()), Times.Once);
                _FileService.Verify(s => s.GetFiles(It.IsAny<string>()), Times.Once);
                _FileService.Verify(s => s.GetFileStream(It.IsAny<string>()), Times.Once);
            }
            else
            {
                _FileService.Verify(s => s.GetFiles(It.IsAny<string>()), Times.Never);
                _FileService.Verify(s => s.GetFileStream(It.IsAny<string>()), Times.Never);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DownloadGeneratedScxFileWithFileNotFound(bool enableSharedFileServiceConsolidation)
        {
            // Arrange
            _FeatureToggle.Setup(s =>
                    s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION))
                .Returns(enableSharedFileServiceConsolidation);

            _ScxGenerationJobRepository.Setup(s => s.GetScxFileName(It.IsAny<int>()))
                .Returns("fileUnfound.txt");
            var getFilesCallCount = 0;
            var dropFolder = "thisFolder";
            var getFilesReturn = new List<string>
            {
                Path.Combine(dropFolder, "fileOne.txt"),
                Path.Combine(dropFolder, "fileTwo.txt"),
                Path.Combine(dropFolder, "fileThree.txt")
            };
            _FileService.Setup(s => s.GetFiles(It.IsAny<string>()))
                .Callback(() => getFilesCallCount++)
                .Returns(getFilesReturn);

            var tc = _GetTestClass();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.DownloadGeneratedScxFile(2));

            // Assert
            Assert.AreEqual("File not found.  Please regenerate.", caught.Message);
            
            if (enableSharedFileServiceConsolidation)
            {
                _ScxGenerationJobRepository.Verify(s => s.GetSharedFolderFileIdForFile(It.IsAny<int>()), Times.Once);
                _SharedFolderService.Verify(s => s.GetFile(It.IsAny<Guid>()), Times.Never);
            }

            _ScxGenerationJobRepository.Verify(s => s.GetScxFileName(It.IsAny<int>()), Times.Once);
            _FileService.Verify(s => s.GetFiles(It.IsAny<string>()), Times.Once);
            _FileService.Verify(s => s.GetFileStream(It.IsAny<string>()), Times.Never);
        }

        #endregion // #region DownloadGeneratedScxFile

        #region ProcessScxGenerationJob

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ProcessScxGenerationJobUpdatesStatus(bool enableSharedFileServiceConsolidation)
        {
            var savedFileGuid = new Guid("4FAED53D-759A-4088-9A33-DE2C9107CCC5");

            _FeatureToggle.Setup(s =>
                    s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION))
                .Returns(enableSharedFileServiceConsolidation);

            var updateJobCallCount = 0;
            var testJob = new ScxGenerationJob
            {
                Status = BackgroundJobProcessingStatus.Queued
            };

            _ScxGenerationJobRepository.Setup(x => x.UpdateJob(testJob))
                .Callback(() => updateJobCallCount++);

            var savedScxJobFiles = new List<List<InventoryScxFile>>();
            _ScxGenerationJobRepository.Setup(s =>
                    s.SaveScxJobFiles(It.IsAny<List<InventoryScxFile>>(), It.IsAny<ScxGenerationJob>()))
                .Callback<List<InventoryScxFile>, ScxGenerationJob>((l, j) => savedScxJobFiles.Add(l));

            var testScxFileStream = new MemoryStream();

            var writer = new StreamWriter(testScxFileStream);
            writer.Write("TestFileContent");
            writer.Flush();
            testScxFileStream.Seek(0, SeekOrigin.Begin);

            var testFiles = new List<InventoryScxFile>
            {
                new InventoryScxFile
                {
                    DaypartCodeId = 1,
                    InventorySource = new InventorySource { Id = 1, Name = "TestSource", IsActive = true, InventoryType = InventorySourceTypeEnum.ProprietaryOAndO },
                    StartDate = new DateTime(2021,10,01),
                    EndDate = new DateTime(2021,10,17),
                    UnitName = "TestUnit",
                    ScxStream = testScxFileStream
                }
            };

            _ProprietaryInventoryService.Setup(x => x.GenerateScxFiles(testJob.InventoryScxDownloadRequest))
                .Returns(testFiles);

            var savedSharedFolderFiles = new List<SharedFolderFile>();
            _SharedFolderService.Setup(s => s.SaveFile(It.IsAny<SharedFolderFile>()))
                .Callback<SharedFolderFile>((sf) => savedSharedFolderFiles.Add(sf))
                .Returns(savedFileGuid);

            var tc = _GetTestClass();

            tc.ProcessScxGenerationJob(testJob, new DateTime(2019, 8, 23));

            Assert.AreEqual(2, updateJobCallCount);
            Assert.AreEqual(BackgroundJobProcessingStatus.Succeeded, testJob.Status);
            Assert.AreEqual(1, savedSharedFolderFiles.Count);
            Assert.AreEqual(1, savedScxJobFiles.Count);
            Assert.IsTrue(savedScxJobFiles[0][0]?.SharedFolderFileId.HasValue ?? false);
            Assert.AreEqual(savedFileGuid, savedScxJobFiles[0][0]?.SharedFolderFileId.Value);

            if (enableSharedFileServiceConsolidation)
            {
                _FileService.Verify(s => s.CreateDirectory(It.IsAny<string>()), Times.Never);
                _FileService.Verify(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()), Times.Never);
            }
            else
            {
                _FileService.Verify(s => s.CreateDirectory(It.IsAny<string>()), Times.Once);
                _FileService.Verify(s => s.Create(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
            }
        }

        #endregion

        #region Ensure ProcessScxGenerationJob Enqueued
        [Test]
        public void ProcessScxGenerationJobEnqueued()
        {
            var testJob = new ScxGenerationJob
            {
                Status = BackgroundJobProcessingStatus.Queued
            };
            const int jobId = 5;
            _ScxGenerationJobRepository.Setup(x => x.AddJob(It.IsAny<ScxGenerationJob>())).Returns(jobId);
            var tc = _GetTestClass();
            var testFiles = new Mock<List<InventoryScxFile>>();
            _ProprietaryInventoryService.Setup(x => x.GenerateScxFiles(testJob.InventoryScxDownloadRequest))
                .Returns(testFiles.Object);

            var inventoryScxDownloadRequest = new Mock<InventoryScxDownloadRequest>();

            tc.QueueScxGenerationJob(inventoryScxDownloadRequest.Object, "UnitTestUser", DateTime.Now);

            _BackgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "ProcessScxGenerationJob" && 
                                  job.Args[0].ToString().Equals(jobId.ToString(), StringComparison.OrdinalIgnoreCase)),
                It.IsAny<EnqueuedState>()));
        }
        #endregion

        #region Transformation

        [Test]
        public void TransformFromDtoToEntityOfRawHappyPath()
        {
            const int processingStatusRaw = 1;
            var startDate = new DateTime();
            var endDate = new DateTime();
            var dto = GetPopulatedDetail(startDate, endDate, processingStatusRaw);
            var getAllQuartersBetweenDatesResults = new List<QuarterDetailDto>
            {
                new QuarterDetailDto {Quarter = 1, Year = 2019}
            };
            var getAllQuartersBetweenDatesCalls = new List<Tuple<DateTime, DateTime>>();

            _QuartersCalculationEngine.Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((s, e) => getAllQuartersBetweenDatesCalls.Add(new Tuple<DateTime, DateTime>(s, e)))
                .Returns(getAllQuartersBetweenDatesResults);

            var tc = _GetTestClass();

            var result = tc.TransformFromDtoToEntity(dto);

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
            _QuartersCalculationEngine.Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((s, e) => getAllQuartersBetweenDatesCalls.Add(new Tuple<DateTime, DateTime>(s, e)))
                .Returns(getAllQuartersBetweenDatesResults);

            var tc = _GetTestClass();

            var result = tc.TransformFromDtoToEntity(dto);

            Assert.IsNotNull(result);
            AssertBaseTransform(dto, result);
            AssertCalculatedFields(result, BackgroundJobProcessingStatus.Queued, getAllQuartersBetweenDatesResults.Count);
            Assert.AreEqual(1, getAllQuartersBetweenDatesCalls.Count);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item1, startDate);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item2, endDate);
        }

        #endregion // #region Transformation

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
    }
}