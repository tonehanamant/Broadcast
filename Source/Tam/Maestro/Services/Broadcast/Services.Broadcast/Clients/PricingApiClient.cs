using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface IPricingApiClient
    {
        PlanPricingApiSpotsResponseDto GetPricingSpotsResult(PlanPricingApiRequestDto request);

        PlanPricingApiSpotsResponseDto_v3 GetPricingSpotsResult(PlanPricingApiRequestDto_v3 request);
    }

    public class PricingApiClient : IPricingApiClient
    {
        private const int ASYNC_API_TIMEOUT_SECONDS = 900;

        private readonly string _OpenMarketSpotsAllocationUrl;
        private readonly string _PlanPricingAllocationsEfficiencyModelUrl;

        public PricingApiClient()
        {
            _OpenMarketSpotsAllocationUrl = BroadcastServiceSystemParameter.PlanPricingAllocationsUrl;
            _PlanPricingAllocationsEfficiencyModelUrl = BroadcastServiceSystemParameter.PlanPricingAllocationsEfficiencyModelUrl;
        }          

        public PlanPricingApiSpotsResponseDto GetPricingSpotsResult(PlanPricingApiRequestDto request)
        {
            var url = $"{_OpenMarketSpotsAllocationUrl}";
            return _Post<PlanPricingApiSpotsResponseDto>(url, request);
        }

        public PlanPricingApiSpotsResponseDto_v3 GetPricingSpotsResult(PlanPricingApiRequestDto_v3 request)
        {
            var url = $"{_PlanPricingAllocationsEfficiencyModelUrl}";
            return _Post<PlanPricingApiSpotsResponseDto_v3>(url, request);
        }

        protected virtual T _Post<T>(string url, object data)
        {
            T output;

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, ASYNC_API_TIMEOUT_SECONDS);

                var rawServiceResponse = client.PostAsJsonAsync(url, data);
                rawServiceResponse.Wait();

                var serviceResponse = rawServiceResponse.Result;
                if (serviceResponse.IsSuccessStatusCode == false)
                {
                    if (serviceResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new InvalidOperationException($"The end point responded with code 404 NotFound.  Url '{url}'");
                    }

                    try
                    {
                        output = serviceResponse.Content.ReadAsAsync<T>().Result;                                                
                    }
                    catch
                    {
                        throw new Exception($"Error connecting to Pricing API for post data. : {serviceResponse}");
                    }
                    return output;
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

            return new PlanPricingApiSpotsResponseDto
            {
                RequestId = "djj4j4399fmmf1m212",
                Results = results
            };
        }

        public PlanPricingApiSpotsResponseDto_v3 GetPricingSpotsResult(PlanPricingApiRequestDto_v3 request)
        {
            throw new NotImplementedException();
        }
    }
}