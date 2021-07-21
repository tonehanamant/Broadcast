using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class InventoryProgramsProcessingServiceTestClass : InventoryProgramsProcessingService
    {
        public InventoryProgramsProcessingServiceTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBackgroundJobClient backgroundJobClient,
            IEmailerService emailerService,
            IInventoryProgramsProcessorFactory inventoryProgramsProcessorFactory,
            IInventoryProgramsRepairEngine inventoryProgramsRepairEngine, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(broadcastDataRepositoryFactory, backgroundJobClient, emailerService, 
            inventoryProgramsProcessorFactory, inventoryProgramsRepairEngine,featureToggleHelper,configurationSettingsHelper)
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

        public List<int> UT_DoEnqueueProcessInventoryProgramsBySourceJobUnprocessedIds { get; set; } = new List<int>();

        protected override void _DoEnqueueProcessInventoryProgramsBySourceJobUnprocessed(int jobId)
        {
            UT_DoEnqueueProcessInventoryProgramsBySourceJobUnprocessedIds.Add(jobId);
        }

        public string[] UT_GetToEmails { get; set; } = new string[]{ "DefaultConfiguredToEmail" };

        protected override string[] _GetProcessingBySourceResultReportToEmails()
        {
            return UT_GetToEmails;
        }
    }
}