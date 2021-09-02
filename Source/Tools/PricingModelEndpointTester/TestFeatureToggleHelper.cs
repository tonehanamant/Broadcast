using System;
using System.Collections.Generic;
using Services.Broadcast.Helpers;

namespace PricingModelEndpointTester
{
    public class TestFeatureToggleHelper : IFeatureToggleHelper
    {
        private readonly Dictionary<string, bool> _TogglesDict;

        public TestFeatureToggleHelper(Dictionary<string, bool> togglesDict)
        {
            _TogglesDict = togglesDict;
        }

        public bool IsToggleEnabled(string toggleKey, string username)
        {
            if (_TogglesDict.ContainsKey(toggleKey))
            {
                return _TogglesDict[toggleKey];
            }
            throw new InvalidOperationException($"Key '{toggleKey}' not found.");
        }

        public bool IsToggleEnabledUserAnonymous(string toggleKey)
        {
            if (_TogglesDict.ContainsKey(toggleKey))
            {
                return _TogglesDict[toggleKey];
            }
            throw new InvalidOperationException($"Key '{toggleKey}' not found.");
        }

        public string Authenticate(string username)
        {
            return string.Empty;
        }
    }
}