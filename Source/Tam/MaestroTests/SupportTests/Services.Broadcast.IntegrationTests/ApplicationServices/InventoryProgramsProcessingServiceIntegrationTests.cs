using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.IntegrationTests.Stubs;
using System;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryProgramsProcessingServiceIntegrationTests
    {
        private IInventoryProgramsProcessingService _InventoryProgramsProcessingService;
        private const string TEST_USERNAME = "TestUser";

        [TestFixtureSetUp]
        public void SetUp()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IProgramGuideApiClient, ProgramGuideApiClientStub>();
            _InventoryProgramsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
        }

        [Test]
        public void QueueInventoryScheduleMergeJob_FileNotExist()
        {
            const int invalidFileId = -27;
            var caught = Assert.Throws<Exception>(() => _InventoryProgramsProcessingService.QueueProcessInventoryProgramsByFileJob(invalidFileId, TEST_USERNAME));
            Assert.AreEqual($"Could not find existing file with id={invalidFileId}", caught.Message);
        }

        [Test]
        public void ProcessInventoryProgramsJob_JobNotExist()
        {
            const int invalidJobId = -27;
            var caught = Assert.Throws<Exception>(() => _InventoryProgramsProcessingService.ProcessInventoryProgramsByFileJob(invalidJobId));
            Assert.AreEqual($"Job with id '{invalidJobId}' not found.", caught.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void QueueProcessInventoryProgramsByFileJob()
        {
            var fileId = 233317;
            using (new TransactionScopeWrapper())
            {
                var queueResult = _InventoryProgramsProcessingService.QueueProcessInventoryProgramsByFileJob(fileId, TEST_USERNAME);
                var result = _InventoryProgramsProcessingService.ProcessInventoryProgramsByFileJob(queueResult.Job.Id);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void QueueProcessInventoryProgramsBySourceJob()
        {
            var sourceId = 1;
            var startDate = new DateTime(2018, 09, 20);
            var endDate = new DateTime(2018, 09, 26);

            using (new TransactionScopeWrapper())
            {
                var queueResult = _InventoryProgramsProcessingService.QueueProcessInventoryProgramsBySourceJob(sourceId, startDate, endDate, TEST_USERNAME);
                var result = _InventoryProgramsProcessingService.ProcessInventoryProgramsBySourceJob(queueResult.Jobs.First().Id);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(InventoryProgramsProcessingJobDiagnostics), "JobId");
            jsonResolver.Ignore(typeof(InventoryProgramsProcessingJobDiagnostics), "StopWatchDict");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
    }
}