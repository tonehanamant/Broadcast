using Hangfire;
using Services.Broadcast.ApplicationServices.Plan;
using System;
using BroadcastJobScheduler.JobQueueMonitors;

namespace BroadcastJobScheduler
{
    /// <summary>
    /// Manages jobs for the WebService.
    /// </summary>
    public interface IWebServiceJobsServicesHost : IJobsServiceHost
    {
    }

    /// <summary>
    /// Manages jobs for the WebService.
    /// </summary>
    public class WebServiceJobsServicesHost : JobsServiceHostBase, IWebServiceJobsServicesHost
    {
        private readonly IQuickRunQueueMonitors _QuickRunQueueMonitors;
        private readonly IPlanService _IPlanService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceJobsServicesHost"/> class.
        /// </summary>
        public WebServiceJobsServicesHost(IRecurringJobManager recurringJobManager,
            IQuickRunQueueMonitors quickRunQueueMonitors,
            IPlanService planService)
        : base (recurringJobManager)
        {
            _QuickRunQueueMonitors = quickRunQueueMonitors;

            _IPlanService = planService;
        }

        protected override void OnStart()
        {
            _StartQuickRunRecurringJobs();
            _QuickRunQueueMonitors.Start();
        }

        protected override void OnStop()
        {
            _QuickRunQueueMonitors.Stop();
        }

        private void _StartQuickRunRecurringJobs()
        {
            _RecurringJobManager.AddOrUpdate(
                "plan-automatic-status-transition",
                () => _IPlanService.AutomaticStatusTransitionsJobEntryPoint(),
                Cron.Daily(_GetPlanAutomaticStatusTransitionJobRunHour()),
                TimeZoneInfo.Local,
                queue: "planstatustransition");
        }

        private int _GetPlanAutomaticStatusTransitionJobRunHour()
        {
            return ConfigurationSettingHelper.GetConfigSetting("PlanAutomaticStatusTransitionJobRunHour", 0);
        }
    }
}