﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Clients;
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
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStub>();
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void QueueProcessInventoryProgramsBySourceJob_AsGroupWithSuccess()
        {
            var sourceId = 1;
            var startDate = new DateTime(2018, 09, 20);
            var endDate = new DateTime(2018, 09, 26);
            var jobGroupId = new Guid("af26f2db-3078-4659-9025-7362e14bbd02");

            using (new TransactionScopeWrapper())
            {
                var queueResult = _InventoryProgramsProcessingService.QueueProcessInventoryProgramsBySourceJob(sourceId, startDate, endDate, TEST_USERNAME, jobGroupId);
                var jobId = queueResult.Jobs.First().Id;

                EmailerServiceStub.LastMailMessageGenerated = null;
                var result = _InventoryProgramsProcessingService.ProcessInventoryProgramsBySourceJob(jobId);

                var lastMessageSent = EmailerServiceStub.LastMailMessageGenerated;
                Assert.IsNull(lastMessageSent);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void QueueProcessInventoryProgramsBySourceJob_AsGroupWithError()
        {
            var sourceId = 1;
            var startDate = new DateTime(2018, 09, 20);
            var endDate = new DateTime(2018, 09, 26);
            var jobGroupId = new Guid("af26f2db-3078-4659-9025-7362e14bbd02");

            using (new TransactionScopeWrapper())
            {
                int jobId;

                var queueResult = _InventoryProgramsProcessingService.QueueProcessInventoryProgramsBySourceJob(sourceId, startDate, endDate, TEST_USERNAME, jobGroupId);
                jobId = queueResult.Jobs.First().Id;

                ProgramGuideApiClientStub.ThrownOnPostAndGet = new Exception("Test error message"); // set it on the shared resource.
                EmailerServiceStub.LastMailMessageGenerated = null;  // set it on the shared resource.

                var caught = Assert.Throws<Exception>(() =>
                    _InventoryProgramsProcessingService.ProcessInventoryProgramsBySourceJob(jobId));

                var lastMessageSent = EmailerServiceStub.LastMailMessageGenerated;
                ProgramGuideApiClientStub.ThrownOnPostAndGet = null;  // cleanup after ourselves.
                EmailerServiceStub.LastMailMessageGenerated = null;  // cleanup after ourselves.

                Assert.AreEqual("Test error message", caught.Message);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(lastMessageSent));
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