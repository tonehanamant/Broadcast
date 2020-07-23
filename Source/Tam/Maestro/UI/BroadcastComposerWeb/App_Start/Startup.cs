using BroadcastLogging;
using log4net;
using Owin;
using System.Runtime.CompilerServices;

namespace BroadcastComposerWeb
{
    /// <summary>
    /// Owin start up
    /// </summary>
    public partial class Startup
    {
        private ILog _Log;

        /// <summary>
        /// Owin configuration
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            _Log = LogManager.GetLogger(typeof(Startup));

            app.MapSignalR();

            ConfigureHangfire(app);
        }

        private void _LogWarning(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Warn(logMessage.ToJson());
        }
    }
}