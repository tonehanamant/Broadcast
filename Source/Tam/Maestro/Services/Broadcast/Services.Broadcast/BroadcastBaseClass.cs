using BroadcastLogging;
using Cadent.Library.Logging.Standard.Common.LoggingModels;
using log4net;
using Services.Broadcast.Helpers;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Services.Broadcast
{
    public abstract class BroadcastBaseClass
    {
        private readonly ILog _Log;
        protected readonly IFeatureToggleHelper _FeatureToggleHelper;
        protected readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        
        protected BroadcastBaseClass(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _Log = LogManager.GetLogger(GetType());
            _FeatureToggleHelper = featureToggleHelper;
            _ConfigurationSettingsHelper = configurationSettingsHelper;
        }

        #region Logging Methods

        protected virtual void _LogInfo(string message, string username = null, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName, username);
            _Log.Info(logMessage.ToJson());
            _ConsiderLogDebug(logMessage);
        }

        protected virtual void _LogInfo(string message, Guid transactionId, string username = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, transactionId, GetType(), memberName, username);
            _Log.Info(logMessage.ToJson());
            _ConsiderLogDebug(logMessage);
        }

        protected virtual void _LogWarning(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Warn(logMessage.ToJson());
            _ConsiderLogDebug(logMessage);
        }

        protected virtual void _LogWarning(string message, Guid transactionId, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, transactionId, GetType(), memberName);
            _Log.Warn(logMessage.ToJson());
            _ConsiderLogDebug(logMessage);
        }

        protected virtual void _LogError(string message, Exception ex = null, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
            _ConsiderLogDebug(logMessage);
        }

        protected virtual void _LogError(string message, Guid transactionId, Exception ex = null, string username = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, transactionId, GetType(), memberName, username);
            _Log.Error(logMessage.ToJson(), ex);
            _ConsiderLogDebug(logMessage);
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

        private void _ConsiderLogDebug(LogMessage logMessage)
        {
#if DEBUG
            _Log.Debug(logMessage.ToJson());
#endif
        }

#endregion // #region Logging Methods

        protected virtual string _GetBroadcastAppFolder()
        {
#if DEBUG
             return Path.GetTempPath();         
 #else
            var result = _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.BroadcastAppFolder);
            return result;
#endif
        }

        protected virtual DateTime _GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}