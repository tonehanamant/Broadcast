using System.ServiceProcess;
using System.Threading;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.Utilities.Logging;

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
            log4net.Config.XmlConfigurator.Configure();

            LogHelper.Logger.Info("Broadcast background job scheduler starting."); 

            // Initialize BroadcastApplicationServiceFactory UnityContainer instance so it's available
            // on method OnActionExecuting for the ViewControllerBase.
            var instance = BroadcastApplicationServiceFactory.Instance;
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
    }
}
