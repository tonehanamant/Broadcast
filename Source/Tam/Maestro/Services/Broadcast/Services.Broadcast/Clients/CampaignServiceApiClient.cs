using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    /// <summary>
    /// Operations for interacting with the CampaignServiceApi
    /// </summary>
    public interface ICampaignServiceApiClient
    {
        /// <summary>
        /// Posts a message that a Plan is a Fluidity Plan.
        /// </summary>
        Task NotifyFluidityPlanAsync(int planId, int planVersionId);
    }

    /// <summary>
    /// A client for the CampaignServiceApi.
    /// </summary>
    public class CampaignServiceApiClient : CadentSecuredClientBase, ICampaignServiceApiClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignServiceApiClient"/> class.
        /// </summary>
        public CampaignServiceApiClient(IApiTokenManager apiTokenManager,
                IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(apiTokenManager, featureToggleHelper, configurationSettingsHelper)
        { 
        }

        /// </inheritDoc>
        public async Task NotifyFluidityPlanAsync(int planId, int planVersionId)
        {
            const string coreApiVersion = "api/v1";                        
            var requestUri = $"{coreApiVersion}/BroadcastPlans/PublishBroadcastMessage";

            var requestSerialized = new PostDSPDTO
            {
                PlanId = planId,
                PlanVersionId = planVersionId
            };
            var content = new StringContent(JsonConvert.SerializeObject(requestSerialized), Encoding.UTF8, "application/json");

            var httpClient = await _GetSecureHttpClientAsync();
            var apiResult = await httpClient.PostAsync(requestUri, content);
            if (apiResult.IsSuccessStatusCode)
            {
                _LogInfo("Successfully Called the api For post the DSP");
            }
        }

        private async Task<HttpClient> _GetSecureHttpClientAsync()
        {
            var apiBaseUrl = _GetCampaignServiceApiBaseUrl();
            var applicationId = _GetCampaignServiceApiApplicationId();
            var appName = _GetCampaignServiceApiAppName();

            var client = await _GetSecureHttpClientAsync(apiBaseUrl, applicationId, appName);
            return client;
        }

        private string _GetCampaignServiceApiBaseUrl()
        {
            var apiBaseUrl = _ConfigurationSettingsHelper.GetConfigValue<string>(CampaignServiceApiConfigKeys.ApiBaseUrl);
            return apiBaseUrl;
        }

        private string _GetCampaignServiceApiApplicationId()
        {
            var applicationId = _ConfigurationSettingsHelper.GetConfigValue<string>(CampaignServiceApiConfigKeys.ApplicationId);
            return applicationId;
        }

        private string _GetCampaignServiceApiAppName()
        {
            var appName = _ConfigurationSettingsHelper.GetConfigValue<string>(CampaignServiceApiConfigKeys.AppName);
            return appName;
        }
    }
}
