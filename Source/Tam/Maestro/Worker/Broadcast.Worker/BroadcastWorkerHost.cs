using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Broadcast.Worker
{
    public class BroadcastWorkerHost
    {
        static void Main(string[] args)
        {
            HostFactory.Run(config =>
            {
                config.Service<BroadcastWorkerService>(service =>
                {
                    service.ConstructUsing(s => new BroadcastWorkerService());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                config.RunAsLocalSystem();
                config.SetDescription("Broadcast Worker Service");
                config.SetDisplayName("BroadcastWorkerService");

            });
        }
    }
}
