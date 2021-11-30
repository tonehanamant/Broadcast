using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Broadcast.Clients
{
    public interface IPricingApiClient
    {
        Task<PlanPricingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanPricingApiRequestDto_v3 request);
    }

    public class PricingApiClient : IPricingApiClient
    {
        private const int ASYNC_API_TIMEOUT_MILLISECONDS = 900000;

        private readonly Lazy<string> _PlanPricingAllocationsEfficiencyModelUrl;
        private readonly HttpClient _HttpClient;
        private readonly Lazy<bool> _AreQueuedPricingRequestsEnabled;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public PricingApiClient(IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient, IFeatureToggleHelper featureToggleHelper)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _PlanPricingAllocationsEfficiencyModelUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelUrl);
            _HttpClient = httpClient;
            _AreQueuedPricingRequestsEnabled = new Lazy<bool>(() => featureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_QUEUED_PRICINGAPICLIENT_REQUESTS));
        }

        public async Task<PlanPricingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanPricingApiRequestDto_v3 request)
        {
            if (_AreQueuedPricingRequestsEnabled.Value)
            {
                var client = new PricingJobQueueApiClient(_ConfigurationSettingsHelper, _HttpClient);
                return await client.GetPricingSpotsResultAsync(request);
            }

            var url = $"{_PlanPricingAllocationsEfficiencyModelUrl.Value}";
            return await _PostAsync<PlanPricingApiSpotsResponseDto_v3>(url, request);

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
                        throw new Exception($"Error connecting to Pricing API for post data. : {serviceResponse}");
                    }

                    return output;
                }

                output = await serviceResponse.Content.ReadAsAsync<T>();
            }
            catch (Exception e)
            {
                throw new Exception("Error calling the Pricing API for post data during post-get.", e);
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