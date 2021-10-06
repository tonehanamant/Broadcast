using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IPlanBuyingApiClient
    {
        PlanBuyingApiSpotsResponseDto_v3 GetBuyingSpotsResult(PlanBuyingApiRequestDto_v3 request);
    }

    public class PlanBuyingApiClient : IPlanBuyingApiClient
    {
        private readonly Lazy<string> _OpenMarketSpotsAllocationUrl;
        private readonly Lazy<string> _PlanPricingAllocationsEfficiencyModelUrl;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly Lazy<bool> _IsPipelineVariablesEnabled;
        private readonly HttpClient _HttpClient;

        public PlanBuyingApiClient(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper, HttpClient httpClient)
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));
            _OpenMarketSpotsAllocationUrl = new Lazy<string>(_GetOpenMarketSpotsAllocationUrl);
            _PlanPricingAllocationsEfficiencyModelUrl = new Lazy<string>(_GetPlanPricingAllocationsEfficiencyModelUrl);
            _HttpClient = httpClient;
        }

        public PlanBuyingApiSpotsResponseDto_v3 GetBuyingSpotsResult(PlanBuyingApiRequestDto_v3 request)
        {
            var url = $"{_PlanPricingAllocationsEfficiencyModelUrl.Value}";
            return _Post<PlanBuyingApiSpotsResponseDto_v3>(url, request);
        }

        protected virtual T _Post<T>(string url, object data)
        {
            T output;

            try
            {
                var serviceResponse = _HttpClient.PostAsJsonAsync(url, data).Result;

                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    try
                    {
                        output = serviceResponse.Content.ReadAsAsync<T>().Result;
                        return output;
                    }
                    catch
                    {
                        throw new Exception($"Error connecting to Buying API for post data. : {serviceResponse}");
                    }

                }
                output = serviceResponse.Content.ReadAsAsync<T>().Result;

            }
            catch (Exception e)
            {
                throw new Exception("Error calling the Buying API for post data during post-get.", e);
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

    public class BuyingApiMockClient : IPlanBuyingApiClient
    {
        public PlanBuyingApiSpotsResponseDto GetBuyingSpotsResult(PlanBuyingApiRequestDto request)
        {
            var results = new List<PlanBuyingApiSpotsResultDto>();

            foreach (var spot in request.Spots)
            {
                var result = new PlanBuyingApiSpotsResultDto
                {
                    ManifestId = spot.Id,
                    MediaWeekId = spot.MediaWeekId,
                    Frequency = 1
                };

                results.Add(result);
            }

            return new PlanBuyingApiSpotsResponseDto
            {
                RequestId = "djj4j4399fmmf1m212",
                Results = results
            };
        }

        public PlanBuyingApiSpotsResponseDto_v3 GetBuyingSpotsResult(PlanBuyingApiRequestDto_v3 request)
        {
            throw new NotImplementedException();
        }
    }
}