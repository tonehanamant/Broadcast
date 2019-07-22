using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    public class ScxGenerationServiceTests
    {
        private readonly IScxGenerationService _ScxGenerationService;
        private readonly IScxGenerationJobRepository _ScxGenerationJobRepository;
        private readonly InMemoryFileServiceStubb _FileService;
        private readonly JsonSerializerSettings _JsonSettings;

        public ScxGenerationServiceTests()
        {
            _FileService = new InMemoryFileServiceStubb();
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(_FileService);
            _ScxGenerationService = IntegrationTestApplicationServiceFactory.GetApplicationService<IScxGenerationService>();
            _ScxGenerationJobRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IScxGenerationJobRepository>();
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ScxGenerationJob), "Id");
            _JsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScxGenerationQueueJobTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    DaypartCodeId = 1,
                    InventorySourceId = 1,
                    StartDate = new DateTime(2019, 3, 1),
                    EndDate = new DateTime(2019, 3, 15),
                    UnitNames = new List<string> { "EM 1 " }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var job = _ScxGenerationJobRepository.GetJobById(jobId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(job, _JsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScxGenerationProcessJobTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    DaypartCodeId = 1,
                    InventorySourceId = 7,
                    UnitNames = new List<string> { "ExpiresGroupsTest" }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var job = _ScxGenerationJobRepository.GetJobById(jobId);

                _ScxGenerationService.ProcessScxGenerationJob(job, new DateTime(2019, 7, 11));

                var jobAfterProcessing = _ScxGenerationJobRepository.GetJobById(jobId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobAfterProcessing, _JsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScxGenerationProcessJobFileResultTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    DaypartCodeId = 1,
                    InventorySourceId = 7,
                    UnitNames = new List<string> { "ExpiresGroupsTest" }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var job = _ScxGenerationJobRepository.GetJobById(jobId);

                _ScxGenerationService.ProcessScxGenerationJob(job, new DateTime(2019, 7, 11));

                var jobAfterProcessing = _ScxGenerationJobRepository.GetJobById(jobId);

                var paths = _FileService.Paths;
                var streams = _FileService.Streams;

                Assert.NotNull(paths.Count > 0 );
                Assert.NotNull(streams.Count > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScxGenerationGetQueuedJobsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    DaypartCodeId = 1,
                    InventorySourceId = 7,
                    UnitNames = new List<string> { "ExpiresGroupsTest" }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var jobs = _ScxGenerationService.GetQueuedJobs(5);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobs, _JsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScxGenerationGetHistoryTest()
        {
            const int inventorySourceId = 7;
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    DaypartCodeId = 1,
                    InventorySourceId = inventorySourceId,
                    UnitNames = new List<string> {"ExpiresGroupsTest"}
                };
                var currentDate = new DateTime(2019,04,17,12,30,23);
                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", currentDate);
                var job = _ScxGenerationJobRepository.GetJobById(jobId);
                _ScxGenerationService.ProcessScxGenerationJob(job, currentDate);

                var history = _ScxGenerationService.GetScxFileGenerationHistory(inventorySourceId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(history, _JsonSettings));
            }
        }
    }
}
