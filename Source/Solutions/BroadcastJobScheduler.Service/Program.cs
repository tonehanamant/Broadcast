using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Threading;
using BroadcastLogging;
using log4net;
using Services.Broadcast.ApplicationServices;
using Microsoft.Practices.Unity;

namespace BroadcastJobScheduler.Service
{
    /// <summary>
    /// Start up
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // Initialize BroadcastApplicationServiceFactory UnityContainer instance so it's available
            var instance = BroadcastApplicationServiceFactory.Instance;
            SetupLogging();
#if DEBUG
            var s = new JobSchedulerService();
            s.Startup();
            Thread.Sleep(Timeout.Infinite);
#else
            var servicesToRun = new ServiceBase[]
            {
                new JobSchedulerService()
            };
            ServiceBase.Run(servicesToRun);
#endif
        }

        private static void SetupLogging()
        {
            log4net.Config.XmlConfigurator.Configure();
            BroadcastApplicationServiceFactory.Instance.RegisterInstance<IBroadcastLoggingConfiguration>(new BroadcastJobSchedulerServiceLogConfig());
            BroadcastLogMessageHelper.Configuration = BroadcastApplicationServiceFactory.Instance.Resolve<IBroadcastLoggingConfiguration>();
        }
    }
}
