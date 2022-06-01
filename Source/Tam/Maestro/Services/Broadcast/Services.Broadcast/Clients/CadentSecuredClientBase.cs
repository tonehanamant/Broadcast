using Services.Broadcast.Helpers;
using System.Net.Http;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    /// <summary>
    /// A base class provided operational support for UM authenticated clients.
    /// </summary>
    public class CadentSecuredClientBase : BroadcastBaseClass
    {
        private readonly IApiTokenManager _ApiTokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CadentSecuredClientBase"/> class.
        /// </summary>
        /// <param name="apiTokenManager">The API token manager.</param>
        /// <param name="featureToggleHelper">The feature toggle helper.</param>
        /// <param name="configurationSettingsHelper">The configuration settings helper.</param>
        protected CadentSecuredClientBase(IApiTokenManager apiTokenManager,
                IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _ApiTokenManager = apiTokenManager;
        }

        /// <summary>
        /// Gets the secure HTTP client with Cadent headers, etc.
        /// </summary>
        /// <returns></returns>
        protected async Task<HttpClient> _GetSecureHttpClientAsync(string apiBaseUrl, string applicationId, string appName)
        {
            var umUrl = _GetUmUrl();
            var accessToken = await _ApiTokenManager.GetOrRefreshTokenAsync(umUrl, appName, applicationId);

            var client = CadentServiceClientHelper.GetServiceHttpClient(apiBaseUrl, applicationId, accessToken);
            return client;
        }

        private string _GetUmUrl()
        {
            return _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.UmUrl);
        }
    }
}
