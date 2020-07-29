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
        /// </summary>
        /// <param name="toggleKey">The key of the toggle.</param>
        /// <param name="username">The username of the logged in user.</param>
        /// <returns>True if the toggle is on.  False if the toggle is off.</returns>
        bool IsToggleEnabled(string toggleKey, string username);
    }

    /// <inheritdoc />
    public class LaunchDarklyClient : ILaunchDarklyClient
    {
        /// <inheritdoc />
        public bool IsToggleEnabled(string toggleKey, string username)
        {
            const bool isAnonymous = false;
            var sdkKey = _GetSdkKey();
            var ldHelper = new LaunchDarklyHelper(username, sdkKey, isAnonymous);

            var toggleValue = ldHelper.LdIsFeatureEnabled(toggleKey);

            return toggleValue;
        }

        private string _GetSdkKey()
        {
            var encryptedKey = BroadcastServiceSystemParameter.LaunchDarklySdkKey;
            var decryptedKey = EncryptionHelper.DecryptString(encryptedKey, EncryptionHelper.EncryptionKey);
            return decryptedKey;
        }
    }
}