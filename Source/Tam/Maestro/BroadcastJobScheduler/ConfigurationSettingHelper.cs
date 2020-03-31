using System;
using System.Configuration;
using System.Linq;

namespace BroadcastJobScheduler
{
    /// <summary>
    /// Helps you get your appSetting driven configuration settings.
    /// </summary>
    public static class ConfigurationSettingHelper
    {
        /// <summary>
        /// Gets the configuration setting.  Falls back to the given default value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSettingsKey">The application settings key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
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
                // falls back to the default.
            }

            return result;
        }
    }
}