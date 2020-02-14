using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Html;
using Common.Services.Repositories;
using Hangfire;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    public class InventoryProgramsProcessingServiceUnitTests
    {
        private const string TEST_USERNAME = "TestUser";
        private const string TEST_DATE_FORMAT = "yyyy-MM-dd";

        private readonly Mock<IInventoryRepository> _InventoryRepository = new Mock<IInventoryRepository>();
        private readonly Mock<IInventoryFileRepository> _InventoryFileRepository = new Mock<IInventoryFileRepository>();
        private readonly Mock<IInventoryProgramsByFileJobsRepository> _InventoryProgramsByFileJobsRepository = new Mock<IInventoryProgramsByFileJobsRepository>();
        private readonly Mock<IInventoryProgramsBySourceJobsRepository> _InventoryProgramsBySourceJobsRepository = new Mock<IInventoryProgramsBySourceJobsRepository>();

        private readonly Mock<IBackgroundJobClient> _BackgroundJobClient = new Mock<IBackgroundJobClient>();
        private readonly Mock<IInventoryProgramsProcessingEngine> _InventoryProgramsProcessingEngine = new Mock<IInventoryProgramsProcessingEngine>();

        [Test]
        public void QueueProcessInventoryProgramsByFileJob()
        {
            const int testFileId = 1;
            const int testJobId = 1;
            var job = new InventoryProgramsByFileJob
            {
                Id = testJobId,
                InventoryFileId = testFileId,
                QueuedBy = TEST_USERNAME
            };

            var getInventoryFileByIdCalled = 0;
            _InventoryFileRepository.Setup(s => s.GetInventoryFileById(It.IsAny<int>()))
                .Callback(() => getInventoryFileByIdCalled++)
                .Returns(new InventoryFile{Id = 1});

            var queueJobCalled = 0;
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.QueueJob(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => queueJobCalled++)
                .Returns(job.Id);
            var getJobCalled = 0;
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.GetJob(It.IsAny<int>()))
                .Callback(() => getJobCalled++)
                .Returns(job);

            var service = _GetService();

            var result = service.QueueProcessInventoryProgramsByFileJob(testFileId, TEST_USERNAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(testFileId, result.Job.InventoryFileId);
            Assert.AreEqual(testJobId, result.Job.Id);
            Assert.AreEqual(1, getInventoryFileByIdCalled);
            Assert.AreEqual(1, service.UT_EnqueueProcessInventoryProgramsByFileJobIds.Count);
            Assert.AreEqual(1, queueJobCalled);
            Assert.AreEqual(1, getJobCalled);
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
            var newJob = new InventoryProgramsByFileJob
            {
                Id = newJobId,
                InventoryFileId = testFileId,
                QueuedBy = TEST_USERNAME
            };

            var getJobCalled = 0;
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.GetJob(oldJobId))
                .Callback(() => getJobCalled++)
                .Returns(oldJob);
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.GetJob(newJobId))
                .Callback(() => getJobCalled++)
                .Returns(newJob);

            var queueJobs = new List<int>();
            _InventoryProgramsByFileJobsRepository
                .Setup(s => s.QueueJob(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<int, string, DateTime>((fileId, u, q) => queueJobs.Add(fileId))
                .Returns(newJob.Id);

            var service = _GetService();

            var result = service.ReQueueProcessInventoryProgramsByFileJob(oldJobId, TEST_USERNAME);
            Assert.IsNotNull(result);
            Assert.AreEqual(testFileId, result.Job.InventoryFileId);
            Assert.AreEqual(newJobId, result.Job.Id);

            Assert.AreEqual(1, service.UT_EnqueueProcessInventoryProgramsByFileJobIds.Count);
            Assert.AreEqual(1, queueJobs.Count);
            Assert.AreEqual(2, getJobCalled);
        }

        [Test]
        public void ProcessInventoryProgramsByFileJob()
        {
            const int jobId = 13;

            var processInventoryProgramsByFileJobCalled = new List<int>();
            _InventoryProgramsProcessingEngine.Setup(s => s.ProcessInventoryProgramsByFileJob(It.IsAny<int>()))
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
            var job = new InventoryProgramsBySourceJob()
            {
                Id = testJobId,
                InventorySourceId = testSourceId,
                StartDate = startDate,
                EndDate = endDate,
                QueuedBy = TEST_USERNAME
            };

            var queueJobCalled = 0;
            _InventoryProgramsBySourceJobsRepository.Setup(s => s.QueueJob(It.IsAny<int>(),  
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => queueJobCalled++)
                .Returns(job.Id);
            var getJobCalled = 0;
            _InventoryProgramsBySourceJobsRepository
                .Setup(s => s.GetJob(It.IsAny<int>()))
                .Callback(() => getJobCalled++)
                .Returns(job);

            var service = _GetService();

            var result = service.QueueProcessInventoryProgramsBySourceJob(testSourceId, startDate, endDate, TEST_USERNAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Jobs.Count);
            Assert.AreEqual(testJobId, result.Jobs.First().Id);
            Assert.AreEqual(testSourceId, result.Jobs.First().InventorySourceId);
            Assert.AreEqual(startDate.ToString(TEST_DATE_FORMAT), result.Jobs.First().StartDate.ToString(TEST_DATE_FORMAT));
            Assert.AreEqual(endDate.ToString(TEST_DATE_FORMAT), result.Jobs.First().EndDate.ToString(TEST_DATE_FORMAT));
            Assert.AreEqual(1, service.UT_EnqueueProcessInventoryProgramsBySourceJobIds.Count);
            Assert.AreEqual(1, queueJobCalled);
            Assert.AreEqual(1, getJobCalled);
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
            var newJob = new InventoryProgramsBySourceJob()
            {
                Id = newJobId,
                InventorySourceId = testSourceId,
                StartDate = startDate,
                EndDate = endDate,
                QueuedBy = TEST_USERNAME
            };

            var queueJobCalled = 0;
            _InventoryProgramsBySourceJobsRepository.Setup(s => s.QueueJob(It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => queueJobCalled++)
                .Returns(newJob.Id);
            var getJobCalled = 0;

            _InventoryProgramsBySourceJobsRepository
                .Setup(s => s.GetJob(oldJobId))
                .Callback(() => getJobCalled++)
                .Returns(oldJob);
            _InventoryProgramsBySourceJobsRepository
                .Setup(s => s.GetJob(newJobId))
                .Callback(() => getJobCalled++)
                .Returns(newJob);

            var service = _GetService();

            var result = service.ReQueueProcessInventoryProgramsBySourceJob(oldJobId, TEST_USERNAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Jobs.Count);
            Assert.AreEqual(newJobId, result.Jobs.First().Id);
            Assert.AreEqual(testSourceId, result.Jobs.First().InventorySourceId);
            Assert.AreEqual(startDate.ToString(TEST_DATE_FORMAT), result.Jobs.First().StartDate.ToString(TEST_DATE_FORMAT));
            Assert.AreEqual(endDate.ToString(TEST_DATE_FORMAT), result.Jobs.First().EndDate.ToString(TEST_DATE_FORMAT));
            Assert.AreEqual(1, service.UT_EnqueueProcessInventoryProgramsBySourceJobIds.Count);
            Assert.AreEqual(1, queueJobCalled);
            Assert.AreEqual(2, getJobCalled);
        }

        [Test]
        public void ProcessInventoryProgramsBySourceJob()
        {
            const int jobId = 14;

            var processInventoryProgramsBySourceJobCalled = new List<int>();
            _InventoryProgramsProcessingEngine.Setup(s => s.ProcessInventoryProgramsBySourceJob(It.IsAny<int>()))
                .Callback<int>((j) => processInventoryProgramsBySourceJobCalled.Add(j))
                .Returns(new InventoryProgramsProcessingJobBySourceDiagnostics(null));

            var service = _GetService();

            var result = service.ProcessInventoryProgramsBySourceJob(jobId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, processInventoryProgramsBySourceJobCalled.Count);
            Assert.AreEqual(jobId, processInventoryProgramsBySourceJobCalled.First());
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
            _InventoryProgramsBySourceJobsRepository.Setup(s => s.QueueJob(It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<int, DateTime, DateTime, string, DateTime>((sourceId, start, end, user, queuedAt) => queuedJobs.Add(
                    new InventoryProgramsBySourceJob
                    {
                        InventorySourceId = sourceId,
                        StartDate = start,
                        EndDate = end,
                        QueuedBy = user,
                        QueuedAt = queuedAt
                    }))
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
            queuedJobs.ForEach(j => Assert.AreEqual(expectedStartDate.ToString(TEST_DATE_FORMAT), j.StartDate.ToString(TEST_DATE_FORMAT)));
            queuedJobs.ForEach(j => Assert.AreEqual(expectedEndDate.ToString(TEST_DATE_FORMAT), j.EndDate.ToString(TEST_DATE_FORMAT)));
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

        private InventoryProgramsProcessingServiceTestClass _GetService()
        {
            var dataRepoFactory = _GetDataRepositoryFactory();

            var service = new InventoryProgramsProcessingServiceTestClass(
                dataRepoFactory.Object,
                _BackgroundJobClient.Object,
                _InventoryProgramsProcessingEngine.Object);

            return service;
        }
    }
}