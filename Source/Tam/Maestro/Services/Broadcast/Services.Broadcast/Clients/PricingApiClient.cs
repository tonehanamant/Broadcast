using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Net.Http;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IPricingApiClient
    {
        PlanPricingApiCpmResponseDto GetPricingCalculationResult(PlanPricingApiRequestDto request);
        PlanPricingApiSpotsResponseDto GetPricingSpotsResult(PlanPricingApiRequestDto request);
    }

    public class PricingApiClient : IPricingApiClient
    {
        private readonly string _FloorPricingUrl;
        private readonly string _OpenMarketSpotsAllocationUrl;

        public PricingApiClient()
        {
            _FloorPricingUrl = BroadcastServiceSystemParameter.PlanPricingFloorPricingUrl;
            _OpenMarketSpotsAllocationUrl = BroadcastServiceSystemParameter.PlanPricingAllocationsUrl;
        }          

        public PlanPricingApiCpmResponseDto GetPricingCalculationResult(PlanPricingApiRequestDto request)
        {
            var url = $"{_FloorPricingUrl}";
            return _Post<PlanPricingApiCpmResponseDto>(url, request);
        }

        public PlanPricingApiSpotsResponseDto GetPricingSpotsResult(PlanPricingApiRequestDto request)
        {
            var url = $"{_OpenMarketSpotsAllocationUrl}";
            return _Post<PlanPricingApiSpotsResponseDto>(url, request);
        }

        protected virtual T _Post<T>(string url, object data)
        {
            T output;
            using (var client = new HttpClient())
            {
                var serviceResponse = client.PostAsJsonAsync(url, data).Result;

                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    throw new Exception($"Error connecting to Pricing API for post data. : {serviceResponse}");
                }

                try
                {
                    output = serviceResponse.Content.ReadAsAsync<T>().Result;
                }
                catch (Exception e)
                {
                    throw new Exception("Error calling the Pricing API for post data during post-get.", e);
                }
            }

            return output;
        }
    }
}