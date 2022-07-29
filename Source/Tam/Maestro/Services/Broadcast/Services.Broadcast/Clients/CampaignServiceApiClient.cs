using Newtonsoft.Json;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System;
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
        /// <summary>
        /// Post a message that indicates the campaign changes
        /// </summary>        
        Task<UnifiedCampaignResponseDto> NotifyCampaignAsync(string unifiedCampaignId);
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
            _LogInfo("Attempting to call the CampaignServiceApi to publish a message for Fluidity.");

            const string coreApiVersion = "api/v1";                        
            var requestUri = $"{coreApiVersion}/BroadcastPlans/PublishMessage";

            var requestSerialized = new PostDSPDTO
            {
                PlanId = planId,
                PlanVersionId = planVersionId
            };
            var content = new StringContent(JsonConvert.SerializeObject(requestSerialized), Encoding.UTF8, "application/json");

            var httpClient = await _GetSecureHttpClientAsync();
            // not sure why we need the .GetAwaiter().GetResult() but we do.
            var apiResult = httpClient.PostAsync(requestUri, content).GetAwaiter().GetResult();
            if (apiResult.IsSuccessStatusCode)
            {
                _LogInfo("Successfully called the CampaignServiceApi to publish a message for Fluidity.");
            }
        }

        /// </inheritDoc>
        public async Task<UnifiedCampaignResponseDto> NotifyCampaignAsync(string unifiedCampaignId)
        {
            try
            {
                const string coreApiVersion = "api/v1";
                var requestUri = $"{coreApiVersion}/BroadcastUnifiedCampaign/PublishUpstreamMessage";

                var requestSerialized = new UnifiedCampaignRequestDto
                {
                    UnifiedCampaignId = unifiedCampaignId
                };
                
                var content = new StringContent(JsonConvert.SerializeObject(requestSerialized), Encoding.UTF8, "application/json");

                var httpClient = await _GetSecureHttpClientAsync();
                var apiResult =  httpClient.PostAsync(requestUri, content).GetAwaiter().GetResult();
                if (apiResult.IsSuccessStatusCode)
                {
                    _LogInfo("Successfully Called the api For publish broadcast message");
                }
                var result = apiResult.Content.ReadAsAsync<UnifiedCampaignResponseDto>();
                UnifiedCampaignResponseDto unifiedCampaignResponse = new UnifiedCampaignResponseDto()
                {
                    Success = result.Result.Success,
                    Message = result.Result.Success == true ?  result.Result.Message : "Failed to publish the Unified Campaign."
                };
                return unifiedCampaignResponse;
            }
            catch (Exception ex)
            {
                _LogInfo("Failed to publish the Unified Campaign. Error: " + ex.Message.ToString());
                throw new InvalidOperationException(String.Format("Failed to publish the Unified Campaign. Error: :{0}", ex.Message.ToString()));                
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
