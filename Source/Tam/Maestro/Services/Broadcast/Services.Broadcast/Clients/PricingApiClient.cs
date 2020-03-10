using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IPricingApiClient
    {
        PlanPricingApiSpotsResponseDto GetPricingSpotsResult(PlanPricingApiRequestDto request);
    }

    public class PricingApiClient : IPricingApiClient
    {
        private readonly string _OpenMarketSpotsAllocationUrl;

        public PricingApiClient()
        {
            _OpenMarketSpotsAllocationUrl = BroadcastServiceSystemParameter.PlanPricingAllocationsUrl;
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

    public class PricingApiMockClient : IPricingApiClient
    {
        public PlanPricingApiSpotsResponseDto GetPricingSpotsResult(PlanPricingApiRequestDto request)
        {
            var results = new List<PlanPricingApiSpotsResultDto>();

            var spotsGroupedByWeekId = request.Spots.GroupBy(x => x.MediaWeekId);

            foreach (var spot in spotsGroupedByWeekId)
            {
                var result = new PlanPricingApiSpotsResultDto
                {
                    MediaWeekId = spot.Key,
                    AllocatedManifestIds = spot.Select(y => y.Id).ToList()
                };

                results.Add(result);
            }

            return new PlanPricingApiSpotsResponseDto
            {
                RequestId = "djj4j4399fmmf1m212",
                Results = results
            };
        }
    }
}