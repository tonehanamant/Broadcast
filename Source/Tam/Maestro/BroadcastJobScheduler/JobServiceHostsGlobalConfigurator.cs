﻿using Hangfire;
using Hangfire.SqlServer;
using Services.Broadcast;
using System;
using Tam.Maestro.Common;
using Unity;

namespace BroadcastJobScheduler
{
    /// <summary>
    /// Globally configures the environment for the hosted job services.
    /// Run this once before starting any queue monitors or services.
    /// </summary>
    public interface IJobServiceHostsGlobalConfigurator
    {
        void Configure(string applicationName, UnityContainer container);
    }

    /// <summary>
    /// Globally configures the environment for the hosted job services.
    /// Run this once before starting any queue monitors or services.
    /// </summary>
    public class JobServiceHostsGlobalConfigurator : BroadcastJobSchedulerBaseClass, IJobServiceHostsGlobalConfigurator
    {
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public JobServiceHostsGlobalConfigurator( IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
        }

        public void Configure(string applicationName, UnityContainer container)
        {
            _LogInfo("Configuring the Hangfire global settings.");
            var pollingIntervalMilliseconds = _GetQueuePollingIntervalMilliseconds();
            var defaultRetryCount = _GetDefaultRetryCount();

            //Set Hangfire to use SQL Server for job persistence
            var connectionString = GetConnectionString(applicationName);
            var sqlStorageOptions = GetSqlServerStorageOptions(pollingIntervalMilliseconds);

            _LogDebug($"Sql-storage-options: {sqlStorageOptions.ToJson()}");

            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString, sqlStorageOptions);

            //Sets Hangfire to use the Ioc container
            GlobalConfiguration.Configuration.UseActivator(new HangfireJobActivator(container));

            // set the default retry count.  
            // override on a queue item with the AutomaticRetry attribute
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = defaultRetryCount });
        }

        /// <summary>
        /// Get Broadcast connection string from Component settings (System settings DB)
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        private string GetConnectionString(string applicationName)
        {
            var connectionString = _ConfigurationSettingsHelper.GetConfigValue<string>(ConnectionStringConfigKeys.CONNECTIONSTRINGS_BROADCAST);

            return ConnectionStringHelper.BuildConnectionString(connectionString, applicationName);
        }

        private static SqlServerStorageOptions GetSqlServerStorageOptions(double hangfirePollingInterval)
        {
            // these are recommended settings for Hangfire from version 1.7.0 on.
            var options = new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromMilliseconds(hangfirePollingInterval),
                UseRecommendedIsolationLevel = true,
                UsePageLocksOnDequeue = true,
                DisableGlobalLocks = true
            };

            return options;
        }

        protected virtual int _GetDefaultRetryCount()
        {
            return AppSettingHelper.GetConfigSetting("HangfireDefaultRetryCount", 2);
        }

        protected virtual int _GetQueuePollingIntervalMilliseconds()
        {
            return AppSettingHelper.GetConfigSetting("HangfirePollingInterval", 1000);
        }
    }
}