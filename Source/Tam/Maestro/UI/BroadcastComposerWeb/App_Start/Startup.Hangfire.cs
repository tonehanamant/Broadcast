﻿using BroadcastComposerWeb.Filters;
using BroadcastJobScheduler;
using BroadcastJobScheduler.JobQueueMonitors;
using Hangfire;
using Owin;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Helpers;
using System;
using System.Linq;
using System.Web.Configuration;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Unity;

namespace BroadcastComposerWeb
{
    /// <summary>
    /// Owin start up
    /// </summary>
    public partial class Startup
    {
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly Lazy<bool> _IsPipelineVariablesEnabled;
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
            BroadcastApplicationServiceFactory.Instance.RegisterType<IJobServiceHostsGlobalConfigurator, JobServiceHostsGlobalConfigurator>();

            // setup the client to accept background tasks and the services to use the same db as the client
            var jobServiceHostsGlobalConfigurator = BroadcastApplicationServiceFactory.Instance.Resolve<IJobServiceHostsGlobalConfigurator>();
            var applicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            jobServiceHostsGlobalConfigurator.Configure(applicationName, BroadcastApplicationServiceFactory.Instance);

            // start background job services
            var disabledForDev = _AreHangfireServicesEnabled() == false;
            if (disabledForDev)
            {
                _LogWarning("Background services are disabled for dev.");
                return;
            }

            var webServiceJobHost = BroadcastApplicationServiceFactory.Instance.Resolve<IWebServiceJobsServicesHost>();
            webServiceJobHost.Start();

            if (_GetRunLongRunningJobsInWebServiceEnabled())
            {
                _LogWarning("Windows Service background jobs are running in the Web Service per the feature toggle.");
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
            const string key = "HangfireServicesEnabled";
            if (WebConfigurationManager.AppSettings.AllKeys.Contains(key) &&
                bool.TryParse(WebConfigurationManager.AppSettings[key], out var result))
            {
                return result;
            }
            return true;
        }

        private bool _GetRunLongRunningJobsInWebServiceEnabled()
        {
            return _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.RUNLONGRUNNINGJOBSINWEBSERVICEENABLED_KEY, false) : BroadcastServiceSystemParameter.RunLongRunningJobsInWebServiceEnabled;
        }
    }
}