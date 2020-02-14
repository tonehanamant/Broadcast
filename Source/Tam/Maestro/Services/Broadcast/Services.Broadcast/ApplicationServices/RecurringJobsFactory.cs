using Hangfire;
using Services.Broadcast.ApplicationServices.Plan;
using System;

namespace Services.Broadcast.ApplicationServices
{
    public class RecurringJobsFactory
    {
        private const string RECURRING_JOBS_USERNAME = "RecurringJobsUser";

        private readonly IRecurringJobManager _RecurringJobManager;
        private readonly IPlanService _PlanService;
        private readonly IInventoryProgramsProcessingService _InventoryProgramsProcessingService;

        public RecurringJobsFactory(
            IRecurringJobManager recurringJobManager,
            IPlanService planService,
            IInventoryProgramsProcessingService inventoryProgramsProcessingService)
        {
            _RecurringJobManager = recurringJobManager;
            _PlanService = planService;
            _InventoryProgramsProcessingService = inventoryProgramsProcessingService;
        }

        public void AddOrUpdateRecurringJobs()
        {
            const int CRON_MIDNIGHT_HOUR = 0;
            _RecurringJobManager.AddOrUpdate(
                "plan-automatic-status-transition",
                () => _PlanService.AutomaticStatusTransitionsJobEntryPoint(),
                Cron.Daily(CRON_MIDNIGHT_HOUR),
                TimeZoneInfo.Local, 
                queue: "planstatustransition");

            _RecurringJobManager.AddOrUpdate(
                "inventory-programs-processing-for-weeks",
                () => _InventoryProgramsProcessingService.QueueProcessInventoryProgramsBySourceForWeeksFromNow(RECURRING_JOBS_USERNAME),
                Cron.Daily(CRON_MIDNIGHT_HOUR),
                TimeZoneInfo.Local,
                queue: "inventoryprogramsprocessing");
        }
    }
}
