using Cadent.Library.Utilities.Standard.Common.Launch_Darkly;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    /// <summary>
    /// A client for interacting with the LaunchDarkly application.
    /// </summary>
    public interface ILaunchDarklyClient
    {
        /// <summary>
        /// Queries to see if the given toggle is on or off.
        /// The username ties into group/user/role managed toggles.
        /// </summary>
        /// <param name="toggleKey">The key of the toggle.</param>
        /// <param name="username">The username of the logged in user.</param>
        /// <returns>True if the toggle is on.  False if the toggle is off.</returns>
        bool IsToggleEnabled(string toggleKey, string username);

        /// <summary>
        /// Queries to see if the given toggle is on or off.
        /// The anonymous user is used.
        /// </summary>
        /// <param name="toggleKey">The key of the toggle.</param>
        /// <returns>True if the toggle is on.  False if the toggle is off.</returns>
        bool IsToggleEnabledUserAnonymous(string toggleKey);

        /// <summary>
        /// Authenticates the user against the Launch Darkly application.
        /// Returns an authenticated token for the FE to use when getting the toggle values directly from the Launch Darkly application.
        /// </summary>
        /// <param name="username">The username to authenticate.</param>
        /// <returns>An authenticated token for the FE to use when getting the toggle values directly from the Launch Darkly application.</returns>
        string Authenticate(string username);
    }

    /// <inheritdoc />
    public class LaunchDarklyClient : ILaunchDarklyClient
    {
        /// <inheritdoc />
        public bool IsToggleEnabled(string toggleKey, string username)
        {
            using (var ldHelper = _GetLaunchDarklyHelper(username, isAnonymous: false))
            {
                var toggleValue = ldHelper.LdIsFeatureEnabled(toggleKey);
                return toggleValue;
            }
        }

        /// <inheritdoc />
        public bool IsToggleEnabledUserAnonymous(string toggleKey)
        {
            // the anonymous username for the broadcast project.
            const string username = "broadcast_user";
            using (var ldHelper = _GetLaunchDarklyHelper(username, isAnonymous: true))
            {
                var toggleValue = ldHelper.LdIsFeatureEnabled(toggleKey);
                return toggleValue;
            }
        }

        /// <inheritdoc />
        public string Authenticate(string username)
        {
            using (var ldHelper = _GetLaunchDarklyHelper(username, isAnonymous: false))
            {
                var clientHash = ldHelper.LdClientHash;
                return clientHash;
            }
        }

        private LaunchDarklyHelper _GetLaunchDarklyHelper(string username, bool isAnonymous)
        {
            var sdkKey = _GetSdkKey();
            var ldHelper = new LaunchDarklyHelper(username, sdkKey, isAnonymous);
            return ldHelper;
        }

        private string _GetSdkKey()
        {
            var encryptedKey = BroadcastServiceSystemParameter.LaunchDarklySdkKey;
            var decryptedKey = EncryptionHelper.DecryptString(encryptedKey, EncryptionHelper.EncryptionKey);
            return decryptedKey;
        }
    }
}