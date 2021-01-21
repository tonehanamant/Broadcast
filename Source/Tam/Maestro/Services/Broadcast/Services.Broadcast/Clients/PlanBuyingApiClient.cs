using Services.Broadcast.Entities.Plan.Buying;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IPlanBuyingApiClient
    {
        PlanBuyingApiSpotsResponseDto GetBuyingSpotsResult(PlanBuyingApiRequestDto request);

        PlanBuyingApiSpotsResponseDto_v3 GetBuyingSpotsResult(PlanBuyingApiRequestDto_v3 request);
    }

    public class PlanBuyingApiClient : IPlanBuyingApiClient
    {
        private readonly string _OpenMarketSpotsAllocationUrl;
        private readonly string _PlanPricingAllocationsEfficiencyModelUrl;

        public PlanBuyingApiClient()
        {
            _OpenMarketSpotsAllocationUrl = BroadcastServiceSystemParameter.PlanPricingAllocationsUrl;
            _PlanPricingAllocationsEfficiencyModelUrl = BroadcastServiceSystemParameter.PlanPricingAllocationsEfficiencyModelUrl;
        }          

        public PlanBuyingApiSpotsResponseDto GetBuyingSpotsResult(PlanBuyingApiRequestDto request)
        {
            var url = $"{_OpenMarketSpotsAllocationUrl}";
            return _Post<PlanBuyingApiSpotsResponseDto>(url, request);
        }

        public PlanBuyingApiSpotsResponseDto_v3 GetBuyingSpotsResult(PlanBuyingApiRequestDto_v3 request)
        {
            var url = $"{_PlanPricingAllocationsEfficiencyModelUrl}";
            return _Post<PlanBuyingApiSpotsResponseDto_v3>(url, request);
        }

        protected virtual T _Post<T>(string url, object data)
        {
            T output;
            using (var client = new HttpClient())
            {
                var serviceResponse = client.PostAsJsonAsync(url, data).Result;

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

                try
                {
                    output = serviceResponse.Content.ReadAsAsync<T>().Result;
                }
                catch (Exception e)
                {
                    throw new Exception("Error calling the Buying API for post data during post-get.", e);
                }
            }

            return output;
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