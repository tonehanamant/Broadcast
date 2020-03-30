﻿using BroadcastComposerWeb.Filters;
using BroadcastJobScheduler;
using BroadcastJobScheduler.JobQueueMonitors;
using Hangfire;
using Microsoft.Practices.Unity;
using Owin;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.Utilities.Logging;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace BroadcastComposerWeb
{
    /// <summary>
    /// Owin start up
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configure Hangfire job scheduler.
        /// </summary>
        /// <param name="app"></param>
        public void ConfigureHangfire(IAppBuilder app)
        {
            // register our dependencies with the container
            BroadcastApplicationServiceFactory.Instance.RegisterType<IRecurringJobManager, RecurringJobManager>();
            BroadcastApplicationServiceFactory.Instance.RegisterType<IQuickRunQueueMonitors, QuickRunQueueMonitors>();
            BroadcastApplicationServiceFactory.Instance.RegisterType<ILongRunQueueMonitors, LongRunQueueMonitors>();
            BroadcastApplicationServiceFactory.Instance.RegisterType<IInventoryAggregationQueueMonitor, InventoryAggregationQueueMonitor>();
            BroadcastApplicationServiceFactory.Instance.RegisterType<IWebServiceJobsServicesHost, WebServiceJobsServicesHost>();
            BroadcastApplicationServiceFactory.Instance.RegisterType<IWindowsServiceJobsServiceHost, WindowsServiceJobsServiceHost>();

            // setup the client to accept background tasks and the services to use the same db as the client
            var jobServiceHostsGlobalConfigurator = new JobServiceHostsGlobalConfigurator();
            var applicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            jobServiceHostsGlobalConfigurator.Configure(applicationName, BroadcastApplicationServiceFactory.Instance);

            // start background job services
            var disabledForDev = _AreHangfireServicesEnabled() == false;
            if (disabledForDev)
            {
                LogHelper.Logger.Warn("Background services are disabled for dev.");
                return;
            }

            var webServiceJobHost = BroadcastApplicationServiceFactory.Instance.Resolve<IWebServiceJobsServicesHost>();
            webServiceJobHost.Start();

            if (_GetRunLongRunningJobsInWebServiceEnabled())
            {
                LogHelper.Logger.Warn("Windows Service background jobs are running in the Web Service per the feature toggle.");
                var windowsServiceJobHost = BroadcastApplicationServiceFactory.Instance.Resolve<IWindowsServiceJobsServiceHost>();
                windowsServiceJobHost.Start();
            }

            //Configure Hangfire Dashboard and Dashboard Authorization
            app.UseHangfireDashboard("/jobs", new DashboardOptions()
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
        }

        /// <summary>
        /// Are the services enabled at all?
        /// Allows Dev to disable for test.
        /// </summary>
        /// <remarks>
        ///     Set this to false while developing so that you don't monitor background jobs in the environment you are connected to.
        /// </remarks>
        /// <returns></returns>
        private bool _AreHangfireServicesEnabled()
        {
            return ConfigurationSettingHelper.GetConfigSetting("HangfireServicesEnabled", true);
        }

        private bool _GetRunLongRunningJobsInWebServiceEnabled()
        {
            return BroadcastServiceSystemParameter.RunLongRunningJobsInWebServiceEnabled;
        }
    }
}