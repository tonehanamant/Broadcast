using BroadcastJobScheduler.JobQueueMonitors;
using BroadcastLogging;
using Hangfire;
using log4net;
using Services.Broadcast.ApplicationServices;
using System;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using Unity;

namespace BroadcastJobScheduler.Service
{
    /// <summary>
    /// Broadcast job scheduler
    /// </summary>
    public partial class JobSchedulerService : ServiceBase
    {
        private IJobsServiceHost _JobsServiceHost;
        private readonly ILog _Log;

        /// <summary>
        /// Ctor
        /// </summary>
        public JobSchedulerService()
        {
            InitializeComponent();
            _Log = LogManager.GetLogger(GetType());
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
            _LogInfo("Broadcast JobScheduler Service stopped");
        }

        /// <summary>
        /// Starts the process.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Startup()
        {
            _LogInfo("Broadcast JobScheduler Service started");
            try
            {
                BroadcastApplicationServiceFactory.Instance.RegisterType<IRecurringJobManager, RecurringJobManager>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<IQuickRunQueueMonitors, QuickRunQueueMonitors>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<ILongRunQueueMonitors, LongRunQueueMonitors>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<IInventoryAggregationQueueMonitor, InventoryAggregationQueueMonitor>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<IWebServiceJobsServicesHost, WebServiceJobsServicesHost>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<IWindowsServiceJobsServiceHost, WindowsServiceJobsServiceHost>();
                BroadcastApplicationServiceFactory.Instance.RegisterType<IJobServiceHostsGlobalConfigurator, JobServiceHostsGlobalConfigurator>();

                var jobServiceHostsGlobalConfigurator = BroadcastApplicationServiceFactory.Instance.Resolve<IJobServiceHostsGlobalConfigurator>();
                var applicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                jobServiceHostsGlobalConfigurator.Configure(applicationName, BroadcastApplicationServiceFactory.Instance);

                _JobsServiceHost = BroadcastApplicationServiceFactory.Instance.Resolve<IWindowsServiceJobsServiceHost>();
                _JobsServiceHost.Start();
            }
            catch (Exception e)
            {
                _LogError("An error occurred in Broadcast JobScheduler Service", e);
            }
        }

        private void _LogInfo(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Info(logMessage.ToJson());
        }

        private void _LogError(string message, Exception ex = null, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
        }
    }
}
