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
