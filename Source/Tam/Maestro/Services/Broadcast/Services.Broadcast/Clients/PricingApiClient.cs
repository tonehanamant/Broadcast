using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Services.Broadcast.Clients
{
    public interface IPricingApiClient
    {
        PlanPricingApiResponsetDto GetPricingCalculationResult(PlanPricingApiRequestDto request);

        PlanPricingApiResponsetDto GetPricingSpotsResult(PlanPricingApiRequestDto request);
    }

    public class MockedResultsPricingApiClient : IPricingApiClient
    {
        public PlanPricingApiResponsetDto GetPricingCalculationResult(PlanPricingApiRequestDto request)
        {
            return new PlanPricingApiResponsetDto
            {
                RequestId = Guid.NewGuid().ToString(),
                Results = new PlanPricingApiResultDto
                {
                    // Mocked.
                    OptimalCpm = 13.3m
                }
            };
        }

        public PlanPricingApiResponsetDto GetPricingSpotsResult(PlanPricingApiRequestDto request)
        {
            var spots = new List<PlanPricingApiResultSpotDto>();

            foreach (var spot in request.Spots)
            {
                spots.Add(new PlanPricingApiResultSpotDto
                {
                    Id = spot.Id,
                    MediaWeekId = spot.MediaWeekId,
                    Cost = spot.Cost,
                    Impressions = spot.Impressions,
                    DaypartId = spot.DaypartId,
                    Spots = 3
                });
            }

            return new PlanPricingApiResponsetDto
            {
                RequestId = Guid.NewGuid().ToString(),
                Results = new PlanPricingApiResultDto
                {
                    // Mocked.
                    Spots = spots
                }
            };
        }
    }

    public class PricingApiClient : IPricingApiClient
    {
        private readonly string _BaseUrl;

        public PricingApiClient()
        {
            _BaseUrl = @"https://datascience-dev.cadent.tv/broadcast-floor-pricing/v1/optimization";
        }          

        public PlanPricingApiResponsetDto GetPricingCalculationResult(PlanPricingApiRequestDto request)
        {
            var url = $"{_BaseUrl}";
            return _Post<PlanPricingApiResponsetDto>(url, request);
        }

        public PlanPricingApiResponsetDto GetPricingSpotsResult(PlanPricingApiRequestDto request)
        {
            throw new NotImplementedException();
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