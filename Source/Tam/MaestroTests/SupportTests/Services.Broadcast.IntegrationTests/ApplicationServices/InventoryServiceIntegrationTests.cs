using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryServiceIntegrationTests
    {
        private readonly IInventoryService _InventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();
        private const string TEST_USERNAME = "TestUser";

        [Test]
        public void QueueInventoryScheduleMergeJob_FileNotExist()
        {
            const int invalidFileId = -27;
            var caught = Assert.Throws<Exception>(() => _InventoryService.QueueInventoryFileProgramEnrichmentJob(invalidFileId, TEST_USERNAME));
            Assert.AreEqual($"Could not find existing file with id={invalidFileId}", caught.Message);
        }

        [Test]
        public void ProcessInventoryProgramsJob_JobNotExist()
        {
            const int invalidJobId = -27;
            var caught = Assert.Throws<Exception>(() => _InventoryService.PerformInventoryFileProgramEnrichmentJob(invalidJobId));
            Assert.AreEqual($"Job with id '{invalidJobId}' not found.", caught.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryProgramsJob_OpenMarket_ManifestCount2_DaypartCount2_WeekCount1()
        {
            var fileId = 205127;
            using (new TransactionScopeWrapper())
            {
                var jobId = _InventoryService.QueueInventoryFileProgramEnrichmentJob(fileId, TEST_USERNAME);
                var result = _InventoryService.PerformInventoryFileProgramEnrichmentJob(jobId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryProgramsJob_OpenMarket_ManifestCount1_DaypartCount1_WeekCount6()
        {
            var fileId = 233346;
            using (new TransactionScopeWrapper())
            {
                var jobId = _InventoryService.QueueInventoryFileProgramEnrichmentJob(fileId, TEST_USERNAME);
                var result = _InventoryService.PerformInventoryFileProgramEnrichmentJob(jobId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryProgramsJob_OpenMarket_ManifestCount2_DaypartCount2_WeekCount7()
        {
            var fileId = 233317;
            using (new TransactionScopeWrapper())
            {
                var jobId = _InventoryService.QueueInventoryFileProgramEnrichmentJob(fileId, TEST_USERNAME);
                var result = _InventoryService.PerformInventoryFileProgramEnrichmentJob(jobId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryProgramsJob_ABCOO_ManifestCount2_DaypartCount2_WeeksCount2()
        {
            var fileId = 236654;
            using (new TransactionScopeWrapper())
            {
                var jobId = _InventoryService.QueueInventoryFileProgramEnrichmentJob(fileId, TEST_USERNAME);
                var result = _InventoryService.PerformInventoryFileProgramEnrichmentJob(jobId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessInventoryProgramsJob_TTWN_ManifestCount35_DaypartCount49_WeeksCount3()
        {
            var fileId = 251391;
            using (new TransactionScopeWrapper())
            {
                var jobId = _InventoryService.QueueInventoryFileProgramEnrichmentJob(fileId, TEST_USERNAME);
                var result = _InventoryService.PerformInventoryFileProgramEnrichmentJob(jobId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(InventoryFileProgramEnrichmentJobDiagnostics), "JobId");
            jsonResolver.Ignore(typeof(InventoryFileProgramEnrichmentJobDiagnostics), "StopWatchDict");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
    }
}