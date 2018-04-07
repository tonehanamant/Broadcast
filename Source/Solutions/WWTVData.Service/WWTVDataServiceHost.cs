using System;
using System.Collections.Generic;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Services;
using Topshelf;
using Topshelf.Hosts;

namespace WWTVData.Service
{
    public class WWTVDataServiceHost
    {
        public const string _serviceName = "_WWTVData.Service";

        private static void Warmup()
        {
            // this is lame, but will hydrate the SMS Client otherwise an error will be thrown
            (new BroadcastApplicationServiceFactory()).GetApplicationService<IProposalService>().GetInitialProposalData(DateTime.Now);
        }

        public static void Main(string[] args)
        {
            Warmup();

            List<ScheduledServiceMethod> servicesToRun = new List<ScheduledServiceMethod>()
            {
                new WWTVDataFile(),
                new WWTVErrorFiles(),
                new WWTVDownloadFromWWTV()
            }; 

            var rc = HostFactory.Run(x =>
            {
                x.Service<ScheduledWindowsServiceMethodRunner>(s =>
                {
                    s.ConstructUsing(name => new ScheduledWindowsServiceMethodRunner(servicesToRun));
                    s.WhenStarted((runner, control)  =>
                    {
                        runner.IsConsole = control is ConsoleRunHost;
                        runner.Start();
                        return true;
                    });
                    s.WhenStopped(tc => tc.Stop());
                });

                x.SetDescription("_WWTV Data Service");
                x.SetDisplayName(_serviceName);
                x.SetServiceName(_serviceName);
            });
            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode()); 
            Environment.ExitCode = exitCode;
        }
    }
}
