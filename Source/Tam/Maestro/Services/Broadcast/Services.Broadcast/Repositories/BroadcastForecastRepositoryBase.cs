using BroadcastLogging;
using Common.Services.Repositories;
using log4net;
using System;
using System.Runtime.CompilerServices;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;

namespace Services.Broadcast.Repositories
{
    public class BroadcastForecastRepositoryBase : CoreRepositoryBase<QueryHintBroadcastForecastContext>
    {
        private readonly ILog _Log;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public BroadcastForecastRepositoryBase(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper, 
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper)
        {
            _Log = LogManager.GetLogger(GetType());
            _ConfigurationSettingsHelper = configurationSettingsHelper;
        }

        public string GetDbInfo()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return $"{context.Database.Connection.DataSource}|{context.Database.Connection.Database}";
                });
        }

        protected override string GetConnectionString()
        {
            string connectionString = _ConfigurationSettingsHelper.GetConfigValue<string>(ConnectionStringConfigKeys.CONNECTIONSTRINGS_BROADCAST_FORECAST);
            return connectionString;
        }

        protected virtual void _LogInfo(string message, string username = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName, username);
            _Log.Info(logMessage.ToJson());
        }

        protected virtual void _LogWarning(string message, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Warn(logMessage.ToJson());
        }

        protected virtual void _LogError(string message, Exception ex = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
        }

        protected virtual void _LogDebug(string message, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Debug(logMessage.ToJson());
        }
    }
}
