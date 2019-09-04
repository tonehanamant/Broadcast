using Owin;

namespace BroadcastComposerWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureHangfire(app);
        }
    }
}