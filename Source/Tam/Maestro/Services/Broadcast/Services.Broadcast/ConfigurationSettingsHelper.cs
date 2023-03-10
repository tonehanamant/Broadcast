using BroadcastLogging;
using Cadent.Library.Models.Standard.Common.Logging;
using Common.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Services.Broadcast
{
    public interface IConfigurationSettingsHelper
    {
        /// <summary>
        /// Get the cofig value based on provided key
        /// </summary>
        /// <param name="key">configuration key</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>configuration value against provided key </returns>
        T GetConfigValueWithDefault<T>(string key, T defaultValue);
        /// <summary>
        /// Get the cofig value based on provided key
        /// </summary>
        /// <param name="key">configuration key</param>
       /// <returns>configuration value against provided key </returns>
        T GetConfigValue<T>(string key);
    }

    public class ConfigurationSettingsHelper : IConfigurationSettingsHelper
    {
        private readonly ILog _Log;
        private readonly Lazy<Dictionary<object, object>> _ConfigDataDictionary;

        public ConfigurationSettingsHelper()
        {
            _Log = LogManager.GetLogger(GetType());
            _ConfigDataDictionary = new Lazy<Dictionary<object, object>>(() => _LoadConfigItems(BroadcastConstants.CONFIG_FILE_NAME));

        }

        public T GetConfigValueWithDefault<T>(string key, T defaultValue)
        {

            T result;
           
            object value;
            if (_ConfigDataDictionary.Value.ContainsKey(key))
            {
                value = _ConfigDataDictionary.Value[key];
                try
                {
                     result = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value.ToString());
                     
                }
                catch (Exception )
                {
                    result = defaultValue;
                    _LogWarning($"The key '{key}' doesn't contain any value hence default value is returned");
                                          
                }               
                
            }
            else
            {                  
                 result = defaultValue;
                _LogWarning($"The key '{key}' doesn't exist");
                
            }
            return result;
        }

        public T GetConfigValue<T>(string key)
        {
            T result;
          
            object value;
            if (_ConfigDataDictionary.Value.ContainsKey(key))
            {
                value = _ConfigDataDictionary.Value[key];
                try
                {
                   result = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value.ToString());
                    
                }
                catch (Exception e)
                {
                    _LogError($"Key '{key}' found but of incorrect type");
                    throw new InvalidOperationException ($"Key '{key}' found but of incorrect type : {e.Message}");
               }
                
            }
            else
            {
                _LogError($"The key '{key}' doesn't exist");
                throw new InvalidOperationException($"The key '{key}' doesn't exist");
            }
            return result;
        }

        internal Dictionary<object, object> _LoadConfigItems(string fileName)
        {
            Dictionary<object, object> configData = new Dictionary<object, object>();

            var directoryPath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            string filePath = System.IO.Path.Combine(directoryPath, fileName);

            using (StreamReader reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                JObject parantNode = JObject.Parse(json);
                foreach (var node in parantNode)
                {
                    if (node.Value.Any())
                    {
                        var data = parantNode.Value<JObject>(node.Key).Properties()
                          .ToDictionary(
                              k => k.Name,
                              v => v.Value.ToObject<object>());
                        foreach (var item in data)
                        {
                            configData.Add(String.Format("{0}:{1}", node.Key, item.Key), item.Value);
                        }

                    }
                    else
                    {
                        configData.Add(node.Key, node.Value.ToObject<object>());
                    }
                }
            }

            // Load from the file last to override settings.
            _LoadAppConfigFile(configData);

            return configData;
        }

        /// <summary>
        /// This is for local Dev and Debug.
        /// It's expected that sensitive or environment specific values are left empty in the Repo 
        /// and populated at Deployment time using TFS Pipeline Variables.
        /// 
        /// We use User Secrets to provide those values during development.
        /// The User Secrets override key values from the app.config for web.config with values provided by the user.
        /// </summary>
        private void _LoadAppConfigFile(Dictionary<object, object> configData)
        {
            foreach(string appSettingKey in ConfigurationManager.AppSettings.Keys)
            {
                var appSettingValue = ConfigurationManager.AppSettings[appSettingKey];
                configData[appSettingKey] = appSettingValue;
            }
        }

        private void _LogError(string message, Exception ex = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
            _ConsiderLogDebug(logMessage);
        }

        private void _ConsiderLogDebug(LogMessage logMessage)
        {
#if DEBUG
            _Log.Debug(logMessage.ToJson());
#endif
        }

        private void _LogWarning(string message, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Warn(logMessage.ToJson());
            _ConsiderLogDebug(logMessage);
        }
    }
}