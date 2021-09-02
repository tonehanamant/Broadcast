using System;
using System.Collections.Generic;
using Services.Broadcast;

namespace PricingModelEndpointTester
{
    public class TestConfigurationSettingsHelper : IConfigurationSettingsHelper
    {
        private readonly Dictionary<string, object> _SettingsDict;

        public TestConfigurationSettingsHelper(Dictionary<string, object> settingsDict)
        {
            _SettingsDict = settingsDict;
        }

        public T GetConfigValueWithDefault<T>(string key, T defaultValue)
        {
            if (_SettingsDict.ContainsKey(key))
            {
                return (T)_SettingsDict[key];
            }
            return defaultValue;
        }

        public T GetConfigValue<T>(string key)
        {
            if (_SettingsDict.ContainsKey(key))
            {
                return (T)_SettingsDict[key];
            }
            throw new InvalidOperationException($"Key '{key}' not found.");
        }
    }
}