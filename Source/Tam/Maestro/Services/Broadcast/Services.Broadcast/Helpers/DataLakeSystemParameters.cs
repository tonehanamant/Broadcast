using Services.Broadcast.Helpers;
using System;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IDataLakeSystemParameters
    {
        string GetSharedFolder();
        string GetUserName();
        string GetPassword();
        string GetNotificationEmail();
    }

    public class DataLakeSystemParameters : IDataLakeSystemParameters
    {
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        internal static Lazy<bool> _IsPipelineVariablesEnabled;

        public DataLakeSystemParameters(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));

        }
        public string GetSharedFolder()
        {
            return _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.DataLake_SharedFolder) :BroadcastServiceSystemParameter.DataLake_SharedFolder;
        }

        public string GetUserName()
        {
            if (_IsPipelineVariablesEnabled.Value)
            {
                var dataLake_SharedFolder_UserName = _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.DataLakeSharedFolderUserName);
                return dataLake_SharedFolder_UserName;
            }
            else
            {
                return BroadcastServiceSystemParameter.DataLake_SharedFolder_UserName;
            }         
        }

        public string GetPassword()
        {
            if (_IsPipelineVariablesEnabled.Value)
            {
                var dataLake_SharedFolder_Password = _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.DataLakeSharedFolderPassword);
                return dataLake_SharedFolder_Password;
            }
            else
            {
                return BroadcastServiceSystemParameter.DataLake_SharedFolder_Password;
            }
           
        }
        public string GetNotificationEmail()
        {
            return _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.DataLake_NotificationEmail) :BroadcastServiceSystemParameter.DataLake_NotificationEmail;
        }
    }
}