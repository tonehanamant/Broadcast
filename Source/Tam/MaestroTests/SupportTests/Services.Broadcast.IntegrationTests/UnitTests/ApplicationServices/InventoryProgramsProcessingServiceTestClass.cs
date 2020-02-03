using Common.Services.Repositories;
using Hangfire;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    public class InventoryProgramsProcessingServiceTestClass : InventoryProgramsProcessingService
    {
        public InventoryProgramsProcessingServiceTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBackgroundJobClient backgroundJobClient,
            IInventoryProgramsProcessingEngine inventoryProgramsProcessingEngine)
        : base(broadcastDataRepositoryFactory, backgroundJobClient, inventoryProgramsProcessingEngine)
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
    }
}