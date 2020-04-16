using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class PricingApiClientStub : IPricingApiClient
    {
        public PlanPricingApiCpmResponseDto GetPricingCalculationResult(PlanPricingApiRequestDto request)
        {
            return new PlanPricingApiCpmResponseDto
            {
                RequestId = "dwq2994mfm2m3m3,amd",
                Results = new PlanPricingApiCpmResultDto
                {
                    // Mocked.
                    MinimumCost = 13.3m
                }
            };
        }

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
    }
}
