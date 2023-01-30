using BroadcastLogging;
using Cadent.Library.Models.Standard.Common.Logging;
using Common.Logging;
using Services.Broadcast.Clients;
using Services.Broadcast.Extensions;
using System;
using System.Runtime.CompilerServices;

namespace Services.Broadcast.Helpers
{
    /// <summary>
    /// A helper for getting feature toggle states.
    /// </summary>
    public interface IFeatureToggleHelper
    {
        /// <summary>
        /// Queries to see if the given toggle is on or off.
        /// The username ties into group/user/role managed toggles.
        /// </summary>
        /// <param name="toggleKey">The key of the toggle.</param>
        /// <param name="username">The username of the logged in user.  Should be an email.</param>
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
        /// <param name="username">The username to authenticate.  Should be an email.</param>
        /// <returns>An authenticated token for the FE to use when getting the toggle values directly from the Launch Darkly application.</returns>
        string Authenticate(string username);
    }

    /// <inheritdoc cref="IFeatureToggleHelper" />
    public class FeatureToggleHelper :IFeatureToggleHelper
    {
        private readonly ILaunchDarklyClient _LaunchDarklyClient;
        private readonly ILog _Log;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FeatureToggleHelper(ILaunchDarklyClient launchDarklyClient)
        {
            _LaunchDarklyClient = launchDarklyClient;
            _Log = LogManager.GetLogger(GetType());
        }

        /// <inheritdoc />
        public bool IsToggleEnabled(string toggleKey, string username)
        {
            try
            {
                _ValidateUsername(username);
                var toggleValue = _LaunchDarklyClient.IsToggleEnabled(toggleKey, username);
                return toggleValue;
            }
            catch (Exception ex)
            {
                _LogError($"Error resolving toggle '{toggleKey}' for user '{username}'.", ex);

                // return as disabled so that the code doesn't just stop.
                return false;
            }
        }

        /// <inheritdoc />
        public bool IsToggleEnabledUserAnonymous(string toggleKey)
        {
            try
            {
                var toggleValue = _LaunchDarklyClient.IsToggleEnabledUserAnonymous(toggleKey);
                return toggleValue;
            }
            catch (Exception ex)
            {
                _LogError($"Error resolving toggle '{toggleKey}' for anonymous user.", ex);

                // return as disabled so that the code doesn't just stop.
                return false;
            }
        }

        /// <inheritdoc />
        public string Authenticate(string username)
        {
            try
            {
                _ValidateUsername(username);
                var clientHash = _LaunchDarklyClient.Authenticate(username);
                return clientHash;
            }
            catch (Exception ex)
            {
                _LogError($"Error authenticating user '{username}'.", ex);
                throw new InvalidOperationException($"Error authenticating user '{username}'.", ex);
            }
        }

        private void _ValidateUsername(string username)
        {
            // Cadent wants the username to be an email address.
            var emailFormatErrorMessage = $"Username '{username}' is not in the expected email address format.";
            if (!username.IsEmailAddressFormat())
            {
                throw new InvalidOperationException(emailFormatErrorMessage);
            }
        }
        protected virtual void _LogError(string message, Exception ex = null, [CallerMemberName] string memberName = "")
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
    }
}