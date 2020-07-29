using Services.Broadcast.Clients;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class LaunchDarklyClientStub : ILaunchDarklyClient
    {
        /// <summary>
        /// The toggles source.
        /// </summary>
        public Dictionary<string, bool> FeatureToggles { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// True to have operations throw an exception.
        /// </summary>
        public bool ThrowError { get; set; }

        /// <summary>
        /// Uses the property FeatureToggles as the sources of the toggles.
        /// False if the toggle is not found.
        /// </summary>
        /// <param name="toggleKey">The key of the toggle.</param>
        /// <param name="username">The username.  Not used.</param>
        /// <returns></returns>
        public bool IsToggleEnabled(string toggleKey, string username)
        {
            if (ThrowError)
            {
                throw new Exception("Throwing error per test configuration.");
            }

            if (FeatureToggles.TryGetValue(toggleKey, out var toggleValue))
            {
                return toggleValue;
            }
            return false;
        }
    }
}