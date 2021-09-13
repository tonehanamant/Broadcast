using System;
using System.Runtime.CompilerServices;
using BroadcastLogging;
using Common.Services.Repositories;
using ConfigurationService.Client;
using log4net;
using Services.Broadcast.Helpers;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.Repositories
{
    public class BroadcastForecastRepositoryBase : CoreRepositoryBase<QueryHintBroadcastForecastContext>
    {
        private readonly ILog _Log;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly Lazy<bool> _IsPipelineVariablesEnabled;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public BroadcastForecastRepositoryBase(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient
            , IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(configurationWebApiClient, pBroadcastContextFactory, pTransactionHelper, TAMResource.BroadcastForecastConnectionString.ToString())
        {
            _Log = LogManager.GetLogger(GetType());
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));
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
            string connectionString = string.Empty;
            if (_IsPipelineVariablesEnabled.Value)
            {
                connectionString = _ConfigurationSettingsHelper.GetConfigValue<string>(ConnectionStringConfigKeys.CONNECTIONSTRINGS_BROADCAST_FORECAST);
            }
            else
            {
                connectionString = base.GetConnectionString();
            }
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
