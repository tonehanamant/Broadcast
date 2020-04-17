using System;
using System.Runtime.CompilerServices;
using BroadcastLogging;
using log4net;

namespace Services.Broadcast
{
    public abstract class BroadcastBaseClass
    {
        private readonly ILog _Log;

        protected BroadcastBaseClass()
        {
            _Log = LogManager.GetLogger(GetType());
        }

        protected virtual void _LogInfo(string message, string username = null, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName, username);
            _Log.Info(logMessage.ToJson());
        }

        protected virtual void _LogWarning(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Warn(logMessage.ToJson());
        }

        protected virtual void _LogError(string message, Exception ex = null, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
        }

        protected virtual void _LogDebug(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Debug(logMessage.ToJson());
        }
    }
}