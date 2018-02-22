using System;
using Topshelf;

namespace WWTVData.Service
{
    
    class WWTVDataServiceHost
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0] == "-console")
            {
                (new WWTV()).CheckWWTVFiles(DateTime.Now);
            }
            else
            {
                var rc = HostFactory.Run(x =>
                {
                    x.Service<WWTV>(s =>
                    {
                        s.ConstructUsing(name => new WWTV());
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());
                    });
                    x.RunAsLocalSystem();

                    x.SetDescription("_WWTV Data Service");
                    x.SetDisplayName("_WWTVData.Service");
                    x.SetServiceName("_WWTVData.Service");
                });
                var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  //11
                Environment.ExitCode = exitCode;
            }
        }
    }
}
