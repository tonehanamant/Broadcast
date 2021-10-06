using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IPricingApiClient
    {
        Task<PlanPricingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanPricingApiRequestDto_v3 request);
    }

    public class PricingApiClient : IPricingApiClient
    {
        private const int ASYNC_API_TIMEOUT_MILLISECONDS = 900000;

        private readonly Lazy<string> _OpenMarketSpotsAllocationUrl;
        private readonly Lazy<string> _PlanPricingAllocationsEfficiencyModelUrl;
        private readonly HttpClient _HttpClient;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly Lazy<bool> _IsPipelineVariablesEnabled;

        public PricingApiClient(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));
            _OpenMarketSpotsAllocationUrl = new Lazy<string>(_GetOpenMarketSpotsAllocationUrl);
            _PlanPricingAllocationsEfficiencyModelUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelUrl);
            _HttpClient = httpClient;
        }

        public async Task<PlanPricingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanPricingApiRequestDto_v3 request)
        {
            var url = $"{_PlanPricingAllocationsEfficiencyModelUrl.Value}";
            return await _PostAsync<PlanPricingApiSpotsResponseDto_v3>(url, request);
        }

        protected async virtual Task<T> _PostAsync<T>(string url, object data)
        {
            T output;
            HttpResponseMessage serviceResponse;

            try
            {
                serviceResponse = await _HttpClient.PostAsJsonAsync(url, data, new CancellationTokenSource(ASYNC_API_TIMEOUT_MILLISECONDS).Token);

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
            return _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PlanPricingAllocationsEfficiencyModelUrl) : BroadcastServiceSystemParameter.PlanPricingAllocationsEfficiencyModelUrl;
        }
        private string _GetOpenMarketSpotsAllocationUrl()
        {
            return _IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.PlanPricingAllocationsUrl) : BroadcastServiceSystemParameter.PlanPricingAllocationsUrl;
        }
    }

    public class PricingApiMockClient : IPricingApiClient
    {
        public async Task<PlanPricingApiSpotsResponseDto> GetPricingSpotsResultAsync(PlanPricingApiRequestDto request)
        {
            var results = new List<PlanPricingApiSpotsResultDto>();

            foreach (var spot in request.Spots)
            {
                var result = new PlanPricingApiSpotsResultDto
                {
                    ManifestId = spot.Id,
                    MediaWeekId = spot.MediaWeekId,
                    Frequency = 1
                };

                results.Add(result);
            }

            return await Task.FromResult(
                new PlanPricingApiSpotsResponseDto
                {
                    RequestId = "djj4j4399fmmf1m212",
                    Results = results
                });
        }

        public Task<PlanPricingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanPricingApiRequestDto_v3 request)
        {
            throw new NotImplementedException();
        }
    }
}