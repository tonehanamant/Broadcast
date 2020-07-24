using BroadcastLogging;
using log4net;
using Services.Broadcast.ApplicationServices;
using System;
using System.ServiceProcess;
using System.Threading;
using Unity;

namespace BroadcastJobScheduler.Service
{
    /// <summary>
    /// Start up
    /// </summary>
    internal static class Program
    {
        private static ILog _Log;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // Initialize BroadcastApplicationServiceFactory UnityContainer instance so it's available
            var instance = BroadcastApplicationServiceFactory.Instance;
            SetupLogging();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

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

            _Log = LogManager.GetLogger("Program.Main");
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var message = $"Unhandled exception caught.";
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, sender.GetType(), "unknown", "Program.Main");
            _Log.Error(logMessage.ToJson(), (Exception)e.ExceptionObject);
        }
    }
}
