using System;
using System.Configuration;
using System.Linq;
using Tam.Maestro.Common.Utilities.Logging;

namespace BroadcastJobScheduler
{
    /// <summary>
    /// Helps you get your appSetting driven configuration settings.
    /// </summary>
    public static class ConfigurationSettingHelper
    {
        public static T GetConfigSetting<T>(string appSettingsKey, T defaultValue = default(T))
        {
            var result = defaultValue;
            try
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains(appSettingsKey)
                    && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[appSettingsKey]))
                {
                    result = (T)Convert.ChangeType(ConfigurationManager.AppSettings[appSettingsKey], typeof(T));
                }
            }
            catch
            {
                LogHelper.Logger.Warn($"AppSettingsKey not found or invalid value: {appSettingsKey}. Using Default value of the type: {result}");
            }

            return result;
        }
    }
}