using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Broadcast.Worker.Startup))]

namespace Broadcast.Worker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureHangfire(app);
        }
    }
}
