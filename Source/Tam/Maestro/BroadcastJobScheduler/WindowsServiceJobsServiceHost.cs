using System;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using BroadcastJobScheduler.JobQueueMonitors;

namespace BroadcastJobScheduler
{
    public interface IWindowsServiceJobsServiceHost : IJobsServiceHost
    {
    }

    public class WindowsServiceJobsServiceHost : JobsServiceHostBase, IWindowsServiceJobsServiceHost
    {
        private readonly ILongRunQueueMonitors _LongRunQueueMonitors;
        private readonly IInventoryAggregationQueueMonitor _InventoryAggregationQueueMonitor;

        private readonly IInventoryProgramsProcessingService _InventoryProgramsProcessingService;
        private readonly IStationService _StationService;

        public WindowsServiceJobsServiceHost(IRecurringJobManager recurringJobManager,
            ILongRunQueueMonitors longRunQueueMonitors,
            IInventoryAggregationQueueMonitor inventoryAggregationQueueMonitor,
            IInventoryProgramsProcessingService inventoryProgramsProcessingService,
            IStationService stationService)
            : base(recurringJobManager)
        {
            _LongRunQueueMonitors = longRunQueueMonitors;
            _InventoryAggregationQueueMonitor = inventoryAggregationQueueMonitor;

            _InventoryProgramsProcessingService = inventoryProgramsProcessingService;
            _StationService = stationService;
        }

        protected override void OnStart()
        {
            _StartLongRunRecurringJobs();
            _LongRunQueueMonitors.Start();
            _InventoryAggregationQueueMonitor.Start();
        }

        protected override void OnStop()
        {
            _LongRunQueueMonitors.Stop();
            _InventoryAggregationQueueMonitor.Stop();
        }

        private void _StartLongRunRecurringJobs()
        {
            _RecurringJobManager.AddOrUpdate(
                "inventory-programs-processing-for-weeks",
                () => _InventoryProgramsProcessingService.QueueProcessInventoryProgramsBySourceForWeeksFromNow(RECURRING_JOBS_USERNAME),
                Cron.Daily(_GetInventoryProgramsProcessingForWeeksJobRunHour()),
                TimeZoneInfo.Local,
                queue: "inventoryprogramsprocessing");

            _RecurringJobManager.AddOrUpdate(
                "stations-update",
                () => _StationService.ImportStationsFromForecastDatabaseJobEntryPoint(RECURRING_JOBS_USERNAME),
                Cron.Daily(_StationsUpdateJobRunHour()),
                TimeZoneInfo.Local,
                queue: "stationsupdate");
        }

        private int _GetInventoryProgramsProcessingForWeeksJobRunHour()
        {
            return ConfigurationSettingHelper.GetConfigSetting("InventoryProgramsProcessingForWeeksJobRunHour", 0);
        }

        private int _StationsUpdateJobRunHour()
        {
            return ConfigurationSettingHelper.GetConfigSetting("StationImportJobRunHour", 0);
        }
    }
}