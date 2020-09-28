using System;
using System.Runtime.CompilerServices;
using BroadcastLogging;
using log4net;
using Tam.Maestro.Services.Cable.SystemComponentParameters; //leave this for the if debug

namespace Services.Broadcast
{
    public abstract class BroadcastBaseClass
    {
        private readonly ILog _Log;

        protected BroadcastBaseClass()
        {
            _Log = LogManager.GetLogger(GetType());
        }

        #region Logging Methods

        protected virtual void _LogInfo(string message, string username = null, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName, username);
            _Log.Info(logMessage.ToJson());
        }

        protected virtual void _LogInfo(string message, Guid transactionId, string username = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, transactionId, GetType(), memberName, username);
            _Log.Info(logMessage.ToJson());
        }

        protected virtual void _LogWarning(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Warn(logMessage.ToJson());
        }

        protected virtual void _LogWarning(string message, Guid transactionId, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, transactionId, GetType(), memberName);
            _Log.Warn(logMessage.ToJson());
        }

        protected virtual void _LogError(string message, Exception ex = null, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
        }

        protected virtual void _LogError(string message, Guid transactionId, Exception ex = null, string username = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, transactionId, GetType(), memberName, username);
            _Log.Error(logMessage.ToJson(), ex);
        }

        protected virtual void _LogDebug(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Debug(logMessage.ToJson());
        }

        protected virtual void _LogDebug(string message, Guid transactionId, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, transactionId, GetType(), memberName);
            _Log.Debug(logMessage.ToJson());
        }

        #endregion // #region Logging Methods

        protected virtual string _GetBroadcastAppFolder()
        {
#if DEBUG
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
#else
            return BroadcastServiceSystemParameter.BroadcastAppFolder;
#endif
        }

        protected virtual DateTime _GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}