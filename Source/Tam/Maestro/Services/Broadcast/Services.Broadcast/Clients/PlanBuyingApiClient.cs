using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Helpers;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public interface IPlanBuyingApiClient
    {
        Task<PlanBuyingApiSpotsResponseDto_v3> GetBuyingSpotsResultAsync(PlanBuyingApiRequestDto_v3 request);
    }

    public class PlanBuyingApiClient : IPlanBuyingApiClient
    {
        private const int ASYNC_API_TIMEOUT_MILLISECONDS = 900000;

        private readonly Lazy<string> _PlanPricingAllocationsEfficiencyModelUrl;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly HttpClient _HttpClient;
        private readonly Lazy<bool> _AreQueuedPricingRequestsEnabled;

        public PlanBuyingApiClient(IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient, IFeatureToggleHelper featureToggleHelper)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _PlanPricingAllocationsEfficiencyModelUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelUrl);
            _HttpClient = httpClient;
            _AreQueuedPricingRequestsEnabled = new Lazy<bool>(() => featureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_QUEUED_PRICINGAPICLIENT_REQUESTS));
        }

        public async Task<PlanBuyingApiSpotsResponseDto_v3> GetBuyingSpotsResultAsync(PlanBuyingApiRequestDto_v3 request)
        {
            if (_AreQueuedPricingRequestsEnabled.Value)
            {
                var client = new PlanBuyingJobQueueApiClient(_ConfigurationSettingsHelper, _HttpClient);
                var queuedResult = await client.GetBuyingSpotsResultAsync(request);
                return queuedResult;
            }

            var url = $"{_PlanPricingAllocationsEfficiencyModelUrl.Value}";
            var result = await _PostAsync<PlanBuyingApiSpotsResponseDto_v3>(url, request);
            return result;
        }

        protected virtual async Task<T> _PostAsync<T>(string url, object data)
        {
            T output;

            try
            {
                var serviceResponse = await _HttpClient.PostAsJsonAsync(url, data, new CancellationTokenSource(ASYNC_API_TIMEOUT_MILLISECONDS).Token);

                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    if (serviceResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new InvalidOperationException($"The end point responded with code 404 NotFound.  Url '{url}'");
                    }

                    try
                    {
                        output = await serviceResponse.Content.ReadAsAsync<T>();
                    }
                    catch
                    {
                        throw new Exception($"Error connecting to Buying API for post data. : {serviceResponse}");
                    }

                    return output;
                }

                output = await serviceResponse.Content.ReadAsAsync<T>();
            }
            catch (Exception e)
            {
                throw new Exception("Error calling the Buying API for post data during post-get.", e);
            }

            return output;
        }
        private string _GetPlanPricingAllocationsEfficiencyModelUrl()
        {
            return _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PlanPricingAllocationsEfficiencyModelUrl);
        }
        private string _GetOpenMarketSpotsAllocationUrl()
        {
            return _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PlanPricingAllocationsUrl);
        }
    }
}