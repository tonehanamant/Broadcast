using BroadcastJobScheduler.JobQueueMonitors;
using Hangfire;
using Microsoft.Practices.Unity;
using Services.Broadcast.ApplicationServices;
using System;
using System.ServiceProcess;
using Tam.Maestro.Common.Utilities.Logging;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace BroadcastJobScheduler.Service
{
    /// <summary>
    /// Broadcast job scheduler
    /// </summary>
    public partial class JobSchedulerService : ServiceBase
    {
        private IJobsServiceHost _JobsServiceHost;

        /// <summary>
        /// Ctor
        /// </summary>
        public JobSchedulerService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// on scheduler start.
        /// </summary>
        /// <param name="args">start up arguments</param>
        protected override void OnStart(string[] args)
        {
            Startup();
        }

        /// <summary>
        /// on scheduler stop.
        /// </summary>
        protected override void OnStop()
        {
            _JobsServiceHost.Stop();
            LogHelper.Logger.Info("Broadcast JobScheduler Service stopped");
        }

        /// <summary>
        /// Starts the process.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Startup()
        {
            LogHelper.Logger.Info("Broadcast JobScheduler Service started");
            try
            {
                BroadcastApplicationServiceFactory.Instance.RegisterType<IRecurringJobManager, RecurringJobManager>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<IQuickRunQueueMonitors, QuickRunQueueMonitors>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<ILongRunQueueMonitors, LongRunQueueMonitors>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<IInventoryAggregationQueueMonitor, InventoryAggregationQueueMonitor>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<IWebServiceJobsServicesHost, WebServiceJobsServicesHost>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<IWindowsServiceJobsServiceHost, WindowsServiceJobsServiceHost>();

                var jobServiceHostsGlobalConfigurator = new JobServiceHostsGlobalConfigurator();
                var applicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                jobServiceHostsGlobalConfigurator.Configure(applicationName, BroadcastApplicationServiceFactory.Instance);

                _JobsServiceHost = BroadcastApplicationServiceFactory.Instance.Resolve<IWindowsServiceJobsServiceHost>();
                _JobsServiceHost.Start();
            }
            catch (Exception e)
            {
                LogHelper.Logger.Error("An error occurred in Broadcast JobScheduler Service", e);
            }
        }
    }
}
