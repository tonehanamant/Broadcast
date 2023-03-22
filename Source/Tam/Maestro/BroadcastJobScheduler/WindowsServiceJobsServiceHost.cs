using BroadcastJobScheduler.JobQueueMonitors;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.ApplicationServices.SpotExceptions;
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

        private readonly IStationService _StationService;
        //private readonly IReelIsciIngestService _IsciIngestService;
        private readonly IPlanService _IPlanService;
        private readonly ISpotExceptionsServiceV2 _ISpotExceptionsServiceV2;

        public WindowsServiceJobsServiceHost(IRecurringJobManager recurringJobManager,
            ILongRunQueueMonitors longRunQueueMonitors,
            IInventoryAggregationQueueMonitor inventoryAggregationQueueMonitor,
            IStationService stationService,
            IReelIsciIngestService isciIngestService,
            IPlanService planService,
            ISpotExceptionsServiceV2 spotExceptionsServiceV2)
            : base(recurringJobManager)
        {
            _LongRunQueueMonitors = longRunQueueMonitors;
            _InventoryAggregationQueueMonitor = inventoryAggregationQueueMonitor;

            _StationService = stationService;
           // _IsciIngestService = isciIngestService;
            _IPlanService = planService;
            _ISpotExceptionsServiceV2 = spotExceptionsServiceV2;
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

            //_RecurringJobManager.AddOrUpdate(
            //   "reel-isci-ingest",
            //   () => _IsciIngestService.PerformReelIsciIngest(RECURRING_JOBS_USERNAME),
            //   Cron.Daily(_ReelIsciiUpdateJobRunHour()),
            //   TimeZoneInfo.Local,
            //   queue: "reelisciingest");

            _RecurringJobManager.AddOrUpdate(
                "plan-automatic-status-transition-v2",
                () => _IPlanService.AutomaticStatusTransitionsJobEntryPointV2(),
                Cron.Daily(_GetPlanAutomaticStatusTransitionJobRunHour()),
                TimeZoneInfo.Local,
                queue: "planstatustransitionv2");

            _RecurringJobManager.AddOrUpdate(
                "spot-exception-ingest-run",
                () => _ISpotExceptionsServiceV2.SpotExceptionIngestRun(_GetSpotExceptionIngestJobRunHour()),
                Cron.Daily(_GetSpotExceptionIngestJobRunHour()),
                TimeZoneInfo.Local,
                queue: "spotexceptioningestrun");
        }

        private int _StationsUpdateJobRunHour()
        {
            return AppSettingHelper.GetConfigSetting("StationImportJobRunHour", 0);
        }
        //private int _ReelIsciiUpdateJobRunHour()
        //{
        //    return AppSettingHelper.GetConfigSetting("ReelIsciiImportJobRunHour", 1);
        //}
        private int _GetPlanAutomaticStatusTransitionJobRunHour()
        {
            return AppSettingHelper.GetConfigSetting("PlanAutomaticStatusTransitionJobRunHour", 0);
        }

        private int _GetSpotExceptionIngestJobRunHour()
        {
            return AppSettingHelper.GetConfigSetting("SpotExceptionIngestJobRunHours", 0);
        }
    }
}