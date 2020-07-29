using System;
using Services.Broadcast.Clients;

namespace Services.Broadcast.Helpers
{
    /// <summary>
    /// A helper for getting feature toggle states.
    /// </summary>
    public interface IFeatureToggleHelper
    {
        /// <summary>
        /// Queries to see if the given toggle is on or off.
        /// </summary>
        /// <param name="toggleKey">The key of the toggle.</param>
        /// <param name="username">The username of the logged in user.</param>
        /// <returns>True if the toggle is on.  False if the toggle is off.</returns>
        bool IsToggleEnabled(string toggleKey, string username);
    }

    /// <inheritdoc cref="IFeatureToggleHelper" />
    public class FeatureToggleHelper : BroadcastBaseClass, IFeatureToggleHelper
    {
        private readonly ILaunchDarklyClient _LaunchDarklyClient;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FeatureToggleHelper(ILaunchDarklyClient launchDarklyClient)
        {
            _LaunchDarklyClient = launchDarklyClient;
        }

        /// <inheritdoc />
        public bool IsToggleEnabled(string toggleKey, string username)
        {
            try
            {
                var toggleValue = _LaunchDarklyClient.IsToggleEnabled(toggleKey, username);
                return toggleValue;
            }
            catch (Exception ex)
            {
                _LogError($"Error resolving toggle '{toggleKey}'.", ex);

                // return as disabled so that the code doesn't just stop.
                return false;
            }
        }
    }
}