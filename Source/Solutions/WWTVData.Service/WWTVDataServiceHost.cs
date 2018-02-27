using System;
using System.ComponentModel;
using System.Reflection;
using System.ServiceModel;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.Logging;
using Tam.Maestro.Services.Clients;
using Topshelf;
using Unity;

namespace WWTVData.Service
{
    
    public class WWTVDataServiceHost
    {
        public const string _serviceName = "_WWTVData.Service";

        public static void Main(string[] args)
        {
            var _ApplicationServiceFactory = new BroadcastApplicationServiceFactory();
            _ApplicationServiceFactory.GetApplicationService<IProposalService>().GetInitialProposalData(DateTime.Now);

            if (args.Length >= 1 && args[0] == "-console")
            {
                (new WWTV(_serviceName)).CheckWWTVFiles(DateTime.Now);
            }
            else
            {
                var rc = HostFactory.Run(x =>
                {
                    x.Service<WWTV>(s =>
                    {
                        s.ConstructUsing(name => new WWTV(_serviceName));
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
