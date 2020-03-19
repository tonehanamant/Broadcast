using Owin;

namespace BroadcastComposerWeb
{
    /// <summary>
    /// Owin start up
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Owin configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="schedulerOptions"></param>
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            ConfigureHangfire(app);
        }
    }
}