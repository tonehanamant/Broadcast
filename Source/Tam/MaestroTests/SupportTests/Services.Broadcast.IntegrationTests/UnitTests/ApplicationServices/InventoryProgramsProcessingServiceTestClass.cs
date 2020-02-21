using Common.Services.Repositories;
using Hangfire;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using System;
using System.Collections.Generic;
using Common.Services;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    public class InventoryProgramsProcessingServiceTestClass : InventoryProgramsProcessingService
    {
        public InventoryProgramsProcessingServiceTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBackgroundJobClient backgroundJobClient,
            IInventoryProgramsProcessingEngine inventoryProgramsProcessingEngine,
            IEmailerService emailerService)
        : base(broadcastDataRepositoryFactory, backgroundJobClient, inventoryProgramsProcessingEngine, emailerService)
        {
        }

        public DateTime UT_DateTimeNow { get; set; } = DateTime.Now;

        protected override DateTime _GetDateTimeNow()
        {
            return UT_DateTimeNow;
        }

        public List<int> UT_EnqueueProcessInventoryProgramsByFileJobIds { get; set; } = new List<int>();

        protected override void _DoEnqueueProcessInventoryProgramsByFileJob(int jobId)
        {
            UT_EnqueueProcessInventoryProgramsByFileJobIds.Add(jobId);
        }

        public List<int> UT_EnqueueProcessInventoryProgramsBySourceJobIds { get; set; } = new List<int>();

        protected override void _DoEnqueueProcessInventoryProgramsBySourceJob(int jobId)
        {
            UT_EnqueueProcessInventoryProgramsBySourceJobIds.Add(jobId);
        }

        public string[] UT_GetToEmails { get; set; } = new string[]{ "DefaultConfiguredToEmail" };

        protected override string[] _GetProcessingBySourceResultReportToEmails()
        {
            return UT_GetToEmails;
        }
    }
}