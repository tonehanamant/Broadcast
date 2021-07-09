using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    public class ConfigurationSettingsHelper : BroadcastBaseClass, IConfigurationSettingsHelper
    {
        private readonly Lazy<Dictionary<object, object>> _ConfigDataDictionary;

        public ConfigurationSettingsHelper()
        {

            _ConfigDataDictionary = new Lazy<Dictionary<object, object>>(() => _LoadConfigItems(BroadcastConstants.CONFIG_FILE_NAME));

        }
        public T GetConfigValueWithDefault<T>(string key, T defaultValue)
        {
            string defaultValueType = string.Empty;
            string typeDictionary = string.Empty;
            defaultValueType = defaultValue.GetType().Name;
            T result = default(T);
           
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
                    result = defaultValue;
                    _LogWarning($"The key '{key}' doesn't contain any value hence default value is returned");
                                          
                }               
                
            }
            else
            {
                    _LogWarning(string.Format("The key {0} doesn't contain any value hence default value is returned", key));
                result = defaultValue;
                _LogWarning($"The key '{key}' doesn't exist");
                
            }
            return result;
        }
        public T GetConfigValue<T>(string key)
        {
            T result = default(T);
          
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
                    throw new InvalidOperationException($"Key '{key}' found but of incorrect type : {e.Message}");

                }
                
            }
            else
            {
                _LogError($"The key '{key}' doesn't exist");
                throw new ApplicationException($"The key '{key}' doesn't exist");
            }
            return result;
        }
        private Dictionary<object, object> _LoadConfigItems(string fileName)
        {
            Dictionary<object, object> configData = new Dictionary<object, object>();

            var directoryPath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            string filePath = System.IO.Path.Combine(directoryPath, fileName);

            using (StreamReader reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                dynamic jsonResults = JsonConvert.DeserializeObject(json);
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

            return configData;
        }
    }

}