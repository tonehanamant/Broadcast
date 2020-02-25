using Hangfire;
using Services.Broadcast.ApplicationServices.Plan;
using System;
using System.Configuration;
using System.Linq;
using Tam.Maestro.Common.Utilities.Logging;

namespace Services.Broadcast.ApplicationServices
{
    public class RecurringJobsFactory
    {
        private const string RECURRING_JOBS_USERNAME = "RecurringJobsUser";

        private readonly IRecurringJobManager _RecurringJobManager;
        private readonly IPlanService _PlanService;
        private readonly IInventoryProgramsProcessingService _InventoryProgramsProcessingService;
        private readonly IStationService _StationService;

        public RecurringJobsFactory(
            IRecurringJobManager recurringJobManager,
            IPlanService planService,
            IInventoryProgramsProcessingService inventoryProgramsProcessingService,
            IStationService stationService)
        {
            _RecurringJobManager = recurringJobManager;
            _PlanService = planService;
            _InventoryProgramsProcessingService = inventoryProgramsProcessingService;
            _StationService = stationService;
        }

        public void AddOrUpdateRecurringJobs()
        {
            _RecurringJobManager.AddOrUpdate(
                "plan-automatic-status-transition",
                () => _PlanService.AutomaticStatusTransitionsJobEntryPoint(),
                Cron.Daily(_GetPlanAutomaticStatusTransitionJobRunHour()),
                TimeZoneInfo.Local, 
                queue: "planstatustransition");

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

        private int _GetPlanAutomaticStatusTransitionJobRunHour()
        {
            return _GetConfiguredInt("PlanAutomaticStatusTransitionJobRunHour", 0);
        }

        private int _GetInventoryProgramsProcessingForWeeksJobRunHour()
        {
            return _GetConfiguredInt("InventoryProgramsProcessingForWeeksJobRunHour", 0);
        }

        private int _StationsUpdateJobRunHour()
        {
            return _GetConfiguredInt("StationImportJobRunHour", 0);
        }

        private int _GetConfiguredInt(string appSettingKey, int defaultValue)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(appSettingKey) == false)
            {
                LogHelper.Logger.Warn($"{appSettingKey} not configured.  Using default : {defaultValue}");
                return defaultValue;
            }

            var raw = ConfigurationManager.AppSettings[appSettingKey];
            if (int.TryParse(raw, out int result))
            {
                return result;
            }

            LogHelper.Logger.Warn($"{appSettingKey} misconfigured as '{raw}'.  Using default : {defaultValue}");
            return defaultValue;
        }
    }
}
