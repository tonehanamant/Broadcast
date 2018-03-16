using System;
using System.Collections.Generic;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Services;
using Topshelf;

namespace WWTVData.Service
{
    public class WWTVDataServiceHost
    {
        public const string _serviceName = "_WWTVData.Service";

        public static void Main(string[] args)
        {
            List<ScheduledServiceMethod> servicesToRun = new List<ScheduledServiceMethod>()
            {
                new WWTVDataFile()
            }; 

            if (args.Length >= 1 && args[0] == "-console")
            {
                servicesToRun.ForEach(s => s.RunService(DateTime.Now));
            }
            else
            {
                var rc = HostFactory.Run(x =>
                {
                    x.Service<ScheduledWindowsServiceMethodRunner>(s =>
                    {
                        s.ConstructUsing(name => new ScheduledWindowsServiceMethodRunner(_serviceName,servicesToRun));
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());
                    });
                    //x.RunAsLocalSystem();

                    x.SetDescription("_WWTV Data Service");
                    x.SetDisplayName(_serviceName);
                    x.SetServiceName(_serviceName);
                });
                var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode()); 
                Environment.ExitCode = exitCode;
            }
        }
    }
}
