using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class PricingApiClientStub : IPricingApiClient
    {
        public PlanPricingApiRequestDto LastSentRequest;

        public PlanPricingApiSpotsResponseDto GetPricingSpotsResult(PlanPricingApiRequestDto request)
        {
            LastSentRequest = request;

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
