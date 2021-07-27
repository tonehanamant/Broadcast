using BroadcastJobScheduler.JobQueueMonitors;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using System;

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
        private readonly IReelIsciIngestService _IsciIngestService;

        public WindowsServiceJobsServiceHost(IRecurringJobManager recurringJobManager,
            ILongRunQueueMonitors longRunQueueMonitors,
            IInventoryAggregationQueueMonitor inventoryAggregationQueueMonitor,
            IInventoryProgramsProcessingService inventoryProgramsProcessingService,
            IStationService stationService,
            IReelIsciIngestService isciIngestService)
            : base(recurringJobManager)
        {
            _LongRunQueueMonitors = longRunQueueMonitors;
            _InventoryAggregationQueueMonitor = inventoryAggregationQueueMonitor;

            _InventoryProgramsProcessingService = inventoryProgramsProcessingService;
            _StationService = stationService;
            _IsciIngestService = isciIngestService;
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
                "stations-update",
                () => _StationService.ImportStationsFromForecastDatabaseJobEntryPoint(RECURRING_JOBS_USERNAME),
                Cron.Daily(_StationsUpdateJobRunHour()),
                TimeZoneInfo.Local,
                queue: "stationsupdate");

            _RecurringJobManager.AddOrUpdate(
               "reel-isci-ingest",
               () => _IsciIngestService.PerformReelIsciIngest(RECURRING_JOBS_USERNAME),
               Cron.Daily(_ReelIsciiUpdateJobRunHour()),
               TimeZoneInfo.Local,
               queue: "reelisciingest");

            if (_GetEnableProgramEnrichmentJobs())
            {
                ScheduleProgramEnrichmentJobs();
            }
            else
            {
                _LogInfo($"Skipping scheduling the Program Enrichment jobs per the configuration.");
            }
        }

        /// <summary>
        /// These processes enriched the data using Dativa's ProgramGuide.
        /// That process has been replaced so these are disabled, but we'll keep them just in case.
        /// </summary>
        private void ScheduleProgramEnrichmentJobs()
        {
            _RecurringJobManager.AddOrUpdate(
                "inventory-programs-processing-for-weeks",
                () => _InventoryProgramsProcessingService.QueueProcessInventoryProgramsBySourceForWeeksFromNow(RECURRING_JOBS_USERNAME),
                Cron.Daily(_GetInventoryProgramsProcessingForWeeksJobRunHour()),
                TimeZoneInfo.Local,
                queue: "inventoryprogramsprocessing");

            _RecurringJobManager.AddOrUpdate(
                "process-program-enriched-inventory-files",
                () => _InventoryProgramsProcessingService.ProcessProgramEnrichedInventoryFiles(),
                Cron.Daily(_ProgramEnrichedInventoryFilesJobRunHour()),
                TimeZoneInfo.Local,
                queue: "processprogramenrichedinventoryfiles");
        }

        private bool _GetEnableProgramEnrichmentJobs()
        {
            return AppSettingHelper.GetConfigSetting("EnableProgramEnrichmentJobs", false);
        }

        private int _GetInventoryProgramsProcessingForWeeksJobRunHour()
        {
            return AppSettingHelper.GetConfigSetting("InventoryProgramsProcessingForWeeksJobRunHour", 0);
        }

        private int _StationsUpdateJobRunHour()
        {
            return AppSettingHelper.GetConfigSetting("StationImportJobRunHour", 0);
        }
        private int _ReelIsciiUpdateJobRunHour()
        {
            return AppSettingHelper.GetConfigSetting("ReelIsciiImportJobRunHour", 1);
        }

        private int _ProgramEnrichedInventoryFilesJobRunHour()
        {
            return AppSettingHelper.GetConfigSetting("ProgramEnrichedInventoryFilesJobRunHour", 12);
        }
    }
}