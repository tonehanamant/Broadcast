using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class InventoryProgramsProcessingServiceUnitTests
    {
        private const string TEST_USERNAME = "TestUser";

        private readonly Mock<IInventoryRepository> _InventoryRepository = new Mock<IInventoryRepository>();
        private readonly Mock<IInventoryFileRepository> _InventoryFileRepository = new Mock<IInventoryFileRepository>();
        private readonly Mock<IInventoryProgramsByFileJobsRepository> _InventoryProgramsByFileJobsRepository = new Mock<IInventoryProgramsByFileJobsRepository>();
        private readonly Mock<IInventoryProgramsBySourceJobsRepository> _InventoryProgramsBySourceJobsRepository = new Mock<IInventoryProgramsBySourceJobsRepository>();

        private readonly Mock<IBackgroundJobClient> _BackgroundJobClient = new Mock<IBackgroundJobClient>();
        private readonly Mock<IInventoryProgramsProcessingEngine> _InventoryProgramsProcessingEngine = new Mock<IInventoryProgramsProcessingEngine>();
        private readonly Mock<IInventoryProgramsRepairEngine> _InventoryProgramsRepairEngine = new Mock<IInventoryProgramsRepairEngine>();

        private readonly Mock<IEmailerService> _EmailerService = new Mock<IEmailerService>();
        private readonly Mock<IFeatureToggleHelper> _FeatureToggleHelper = new Mock<IFeatureToggleHelper>();
        private readonly Mock<IConfigurationSettingsHelper> _IConfigurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();

        [Test]
        public void QueueProcessInventoryProgramsByFileJob()
        {
            const int testFileId = 1;
            const int testJobId = 1;

            var getInventoryFileByIdCalled = 0;
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Callback(() => getInventoryFileByIdCalled++)
                .Returns(new InventoryFile{Id = 1});

            var queueJobCalled = 0;
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.SaveEnqueuedJob(It.IsAny<InventoryProgramsByFileJob>()))
                .Callback(() => queueJobCalled++)
                .Returns(testJobId);

            var service = _GetService();

            var result = service.QueueProcessInventoryProgramsByFileJob(testFileId, TEST_USERNAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(testFileId, result.Job.InventoryFileId);
            Assert.AreEqual(testJobId, result.Job.Id);
            Assert.AreEqual(1, getInventoryFileByIdCalled);
            Assert.AreEqual(1, service.UT_EnqueueProcessInventoryProgramsByFileJobIds.Count);
            Assert.AreEqual(1, queueJobCalled);
        }

        [Test]
        public void QueueProcessInventoryProgramsByFileJobByName()
        {
            const string testFileName = "TestFileOne.txt";
            const int testFileId = 1;
            const int testJobId = 1;

            var getLatestInventoryFileIdByName = 0;
            _InventoryFileRepository.Setup(s => s.GetLatestInventoryFileIdByName(It.IsAny<string>()))
                .Callback(() => getLatestInventoryFileIdByName++)
                .Returns(testFileId);

            var getInventoryFileByIdCalled = 0;
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Callback(() => getInventoryFileByIdCalled++)
                .Returns(new InventoryFile { Id = 1 });

            var queueJobCalled = 0;
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.SaveEnqueuedJob(It.IsAny<InventoryProgramsByFileJob>()))
                .Callback(() => queueJobCalled++)
                .Returns(testJobId);

            var service = _GetService();

            var result = service.QueueProcessInventoryProgramsByFileJobByFileName(testFileName, TEST_USERNAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(testFileId, result.Job.InventoryFileId);
            Assert.AreEqual(testJobId, result.Job.Id);
            Assert.AreEqual(1, getLatestInventoryFileIdByName);
            Assert.AreEqual(1, getInventoryFileByIdCalled);
            Assert.AreEqual(1, service.UT_EnqueueProcessInventoryProgramsByFileJobIds.Count);
            Assert.AreEqual(1, queueJobCalled);
        }

        [Test]
        public void QueueProcessInventoryProgramsByFileJobByName_EmptyFileNAme()
        {
            const string testFileName = "";
            const int testFileId = 1;
            const int testJobId = 1;

            var getLatestInventoryFileIdByName = 0;
            _InventoryFileRepository.Setup(s => s.GetLatestInventoryFileIdByName(It.IsAny<string>()))
                .Callback(() => getLatestInventoryFileIdByName++)
                .Returns(testFileId);

            var getInventoryFileByIdCalled = 0;
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Callback(() => getInventoryFileByIdCalled++)
                .Returns(new InventoryFile { Id = 1 });

            var queueJobCalled = 0;
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.SaveEnqueuedJob(It.IsAny<InventoryProgramsByFileJob>()))
                .Callback(() => queueJobCalled++)
                .Returns(testJobId);

            var service = _GetService();

            var caught = Assert.Throws<InvalidOperationException>(() =>
                service.QueueProcessInventoryProgramsByFileJobByFileName(testFileName, TEST_USERNAME));

            Assert.IsTrue(caught.Message.Contains("A filename is required."));
            Assert.AreEqual(0, getLatestInventoryFileIdByName);
            Assert.AreEqual(0, getInventoryFileByIdCalled);
            Assert.AreEqual(0, service.UT_EnqueueProcessInventoryProgramsByFileJobIds.Count);
            Assert.AreEqual(0, queueJobCalled);
        }

        [Test]
        public void QueueProcessInventoryProgramsByFileJobByName_FileNameNotFound()
        {
            const string testFileName = "NameNotFound";
            const int testFileIdIndicatesNotFound = 0;
           
            var getLatestInventoryFileIdByName = 0;
            _InventoryFileRepository.Setup(s => s.GetLatestInventoryFileIdByName(It.IsAny<string>()))
                .Callback(() => getLatestInventoryFileIdByName++)
                .Returns(testFileIdIndicatesNotFound);

            var getInventoryFileByIdCalled = 0;
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Callback(() => getInventoryFileByIdCalled++);

            var queueJobCalled = 0;
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.SaveEnqueuedJob(It.IsAny<InventoryProgramsByFileJob>()))
                .Callback(() => queueJobCalled++);

            var service = _GetService();

            var caught = Assert.Throws<InvalidOperationException>(() =>
                service.QueueProcessInventoryProgramsByFileJobByFileName(testFileName, TEST_USERNAME));

            Assert.IsTrue(caught.Message.Contains("File 'NameNotFound' not found."));
            Assert.AreEqual(1, getLatestInventoryFileIdByName);
            Assert.AreEqual(0, getInventoryFileByIdCalled);
            Assert.AreEqual(0, service.UT_EnqueueProcessInventoryProgramsByFileJobIds.Count);
            Assert.AreEqual(0, queueJobCalled);
        }

        [Test]
        public void ReQueueProcessInventoryProgramsByFileJob()
        {
            const int testFileId = 1;
            const int oldJobId = 1;
            var oldJob = new InventoryProgramsByFileJob
            {
                Id = oldJobId,
                InventoryFileId = testFileId,
                QueuedBy = TEST_USERNAME
            };
            const int newJobId = 2;

            var getJobCalled = 0;
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.GetJob(oldJobId))
                .Callback(() => getJobCalled++)
                .Returns(oldJob);

            var queueJobs = new List<InventoryProgramsByFileJob>();
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.SaveEnqueuedJob(It.IsAny<InventoryProgramsByFileJob>()))
                .Callback<InventoryProgramsByFileJob>((job) => queueJobs.Add(job))
                .Returns(newJobId);

            var service = _GetService();

            var result = service.ReQueueProcessInventoryProgramsByFileJob(oldJobId, TEST_USERNAME);
            Assert.IsNotNull(result);
            Assert.AreEqual(testFileId, result.Job.InventoryFileId);
            Assert.AreEqual(newJobId, result.Job.Id);

            Assert.AreEqual(1, service.UT_EnqueueProcessInventoryProgramsByFileJobIds.Count);
            Assert.AreEqual(1, queueJobs.Count);
            Assert.AreEqual(1, getJobCalled);
        }

        [Test]
        public void ProcessInventoryProgramsByFileJob()
        {
            const int jobId = 13;

            var processInventoryProgramsByFileJobCalled = new List<int>();
            _InventoryProgramsProcessingEngine.Setup(s => s.ProcessInventoryJob(It.IsAny<int>()))
                .Callback<int>((j) => processInventoryProgramsByFileJobCalled.Add(j))
                .Returns(new InventoryProgramsProcessingJobByFileDiagnostics(null));

            var service = _GetService();

            var result = service.ProcessInventoryProgramsByFileJob(jobId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, processInventoryProgramsByFileJobCalled.Count);
            Assert.AreEqual(jobId, processInventoryProgramsByFileJobCalled.First());
        }

        [Test]
        public void QueueProcessInventoryProgramsBySourceJob()
        {
            const int testSourceId = 12;
            var startDate = new DateTime(2020, 01, 01);
            var endDate = new DateTime(2020, 01, 07);

            const int testJobId = 1;

            var queueJobCalled = 0;
            _InventoryProgramsBySourceJobsRepository.Setup(s => s.SaveEnqueuedJob(It.IsAny<InventoryProgramsBySourceJob>()))
                .Callback(() => queueJobCalled++)
                .Returns(testJobId);

            var service = _GetService();

            var result = service.QueueProcessInventoryProgramsBySourceJob(testSourceId, startDate, endDate, TEST_USERNAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Jobs.Count);
            Assert.AreEqual(testJobId, result.Jobs.First().Id);
            Assert.AreEqual(testSourceId, result.Jobs.First().InventorySourceId);
            Assert.AreEqual(startDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), result.Jobs.First().StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(endDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), result.Jobs.First().EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(1, service.UT_EnqueueProcessInventoryProgramsBySourceJobIds.Count);
            Assert.AreEqual(1, queueJobCalled);
        }

        [Test]
        public void ReQueueProcessInventoryProgramsBySourceJob()
        {
            const int testSourceId = 12;
            var startDate = new DateTime(2020, 01, 01);
            var endDate = new DateTime(2020, 01, 07);

            const int oldJobId = 1;
            var oldJob = new InventoryProgramsBySourceJob()
            {
                Id = oldJobId,
                InventorySourceId = testSourceId,
                StartDate = startDate,
                EndDate = endDate,
                QueuedBy = TEST_USERNAME
            };

            const int newJobId = 2;

            var queueJobCalled = 0;
            _InventoryProgramsBySourceJobsRepository.Setup(s => s.SaveEnqueuedJob(It.IsAny<InventoryProgramsBySourceJob>()))
                .Callback(() => queueJobCalled++)
                .Returns(newJobId);
            var getJobCalled = 0;

            _InventoryProgramsBySourceJobsRepository
                .Setup(s => s.GetJob(It.IsAny<int>()))
                .Callback(() => getJobCalled++)
                .Returns(oldJob);

            var service = _GetService();

            var result = service.ReQueueProcessInventoryProgramsBySourceJob(oldJobId, TEST_USERNAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Jobs.Count);
            Assert.AreEqual(newJobId, result.Jobs.First().Id);
            Assert.AreEqual(testSourceId, result.Jobs.First().InventorySourceId);
            Assert.AreEqual(startDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), result.Jobs.First().StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(endDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), result.Jobs.First().EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(1, service.UT_EnqueueProcessInventoryProgramsBySourceJobIds.Count);
            Assert.AreEqual(1, queueJobCalled);
            Assert.AreEqual(1, getJobCalled);
        }         

        [Test]
        public void QueueProcessInventoryProgramsBySourceForWeeks()
        {
            var testDateNow = new DateTime(2020, 01, 01);
            var expectedStartDate = testDateNow.AddDays(-1 * 7 * 2); // two weeks back
            var expectedEndDate = testDateNow.AddDays(7 * 3); // three weeks forward
            var inventorySources = new List<InventorySource>
            {
                new InventorySource {Id = 1, Name = "SourceOne", IsActive = true, InventoryType = InventorySourceTypeEnum.Barter},
                new InventorySource {Id = 2, Name = "SourceTwo", IsActive = true, InventoryType = InventorySourceTypeEnum.OpenMarket}
            };

            var queuedJobs = new List<InventoryProgramsBySourceJob>();
            _InventoryProgramsBySourceJobsRepository.Setup(s => s.SaveEnqueuedJob(It.IsAny<InventoryProgramsBySourceJob>()))
                .Callback<InventoryProgramsBySourceJob>((job=> queuedJobs.Add(job)))
                .Returns(-1);

            _InventoryProgramsBySourceJobsRepository.Setup(s => s.GetJob(It.IsAny<int>()))
                .Returns(new InventoryProgramsBySourceJob());

            var getInventorySourcesCalled = 0;
            _InventoryRepository.Setup(s => s.GetInventorySources())
                .Callback(() => getInventorySourcesCalled++)
                .Returns(inventorySources);

            var service = _GetService();
            service.UT_DateTimeNow = testDateNow;

            var result = service.QueueProcessInventoryProgramsBySourceForWeeksFromDate(testDateNow, TEST_USERNAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Jobs.Count);
            Assert.AreEqual(1, getInventorySourcesCalled);
            Assert.AreEqual(2, queuedJobs.Count);
            // all source ids are represented
            var allQueuedSourceIDs = queuedJobs.Select(s => s.InventorySourceId).ToList();
            inventorySources.ForEach(r => Assert.AreEqual(1, allQueuedSourceIDs.Count(s => s == r.Id)));
            // all have same start \ End dates
            queuedJobs.ForEach(j => Assert.AreEqual(expectedStartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), j.StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)));
            queuedJobs.ForEach(j => Assert.AreEqual(expectedEndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), j.EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)));
        }

        [Test]
        public void QueueProcessInventoryProgramsBySourceJobUnprocessed()
        {
            const int testSourceId = 12;
            var startDate = new DateTime(2020, 01, 01);
            var endDate = new DateTime(2020, 01, 07);

            const int testJobId = 1;

            var queueJobCalled = 0;
            _InventoryProgramsBySourceJobsRepository.Setup(s => s.SaveEnqueuedJob(It.IsAny<InventoryProgramsBySourceJob>()))
                .Callback(() => queueJobCalled++)
                .Returns(testJobId);

            var service = _GetService();

            var result = service.QueueProcessInventoryProgramsBySourceJobUnprocessed(testSourceId, startDate, endDate, TEST_USERNAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Jobs.Count);
            Assert.AreEqual(testJobId, result.Jobs.First().Id);
            Assert.AreEqual(testSourceId, result.Jobs.First().InventorySourceId);
            Assert.AreEqual(startDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), result.Jobs.First().StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(endDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD), result.Jobs.First().EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD));
            Assert.AreEqual(1, service.UT_DoEnqueueProcessInventoryProgramsBySourceJobUnprocessedIds.Count);
            Assert.AreEqual(1, queueJobCalled);
        }

        [Test]
        public void ProcessInventoryProgramsBySourceJobUnprocessed()
        {
            const int jobId = 14;
            
            var processInventoryProgramsBySourceJobCalled = new List<int>();
            _InventoryProgramsProcessingEngine.Setup(s => s.ProcessInventoryJob(It.IsAny<int>()))
                .Callback<int>((j) => processInventoryProgramsBySourceJobCalled.Add(j))
                .Returns(new InventoryProgramsProcessingJobBySourceDiagnostics(null));

            var service = _GetService();

            var result = service.ProcessInventoryProgramsBySourceJobUnprocessed(jobId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, processInventoryProgramsBySourceJobCalled.Count);
            Assert.AreEqual(jobId, processInventoryProgramsBySourceJobCalled.First());
        }

        private class MonitorEmailProperties
        {
            public bool pIsHtmlBody { get; set; }
            public string pBody { get; set; }
            public string pSubject { get; set; }
            public MailPriority pPriority { get; set; }
            public string from { get; set; }
            public string[] pTos { get; set; }
        }

        private Mock<IDataRepositoryFactory> _GetDataRepositoryFactory()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();

            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryRepository>()).Returns(_InventoryRepository.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryFileRepository>()).Returns(_InventoryFileRepository.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryProgramsByFileJobsRepository>()).Returns(_InventoryProgramsByFileJobsRepository.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryProgramsBySourceJobsRepository>()).Returns(_InventoryProgramsBySourceJobsRepository.Object);

            return dataRepoFactory;
        }

        private Mock<IInventoryProgramsProcessorFactory> _GetInventoryProgramsProcessorFactory()
        {
            var engineFactory = new Mock<IInventoryProgramsProcessorFactory>();
            engineFactory.Setup(s => s.GetInventoryProgramsProcessingEngine(It.IsAny<InventoryProgramsProcessorType>()))
                .Returns(_InventoryProgramsProcessingEngine.Object);
            return engineFactory;
        }

        private InventoryProgramsProcessingServiceTestClass _GetService()
        {
            var dataRepoFactory = _GetDataRepositoryFactory();

            var engineFactory = _GetInventoryProgramsProcessorFactory();

            var service = new InventoryProgramsProcessingServiceTestClass(
                dataRepoFactory.Object,
                _BackgroundJobClient.Object,
                _EmailerService.Object,
                engineFactory.Object,
                _InventoryProgramsRepairEngine.Object, _FeatureToggleHelper.Object, _IConfigurationSettingsHelper.Object);

            return service;
        }
    }
}