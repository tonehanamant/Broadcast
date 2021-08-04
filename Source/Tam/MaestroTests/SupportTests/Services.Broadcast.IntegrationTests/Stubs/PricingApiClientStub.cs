using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class PricingApiClientStub : IPricingApiClient
    {
        public PlanPricingApiRequestDto LastSentRequest;

        public async Task<PlanPricingApiSpotsResponseDto> GetPricingSpotsResultAsync(PlanPricingApiRequestDto request)
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

            return await Task.FromResult(
                new PlanPricingApiSpotsResponseDto
                {
                    RequestId = "djj4j4399fmmf1m212",
                    Results = results
                });
        }

        public async Task<PlanPricingApiSpotsResponseDto_v3> GetPricingSpotsResultAsync(PlanPricingApiRequestDto_v3 request)
        {
            var results = new List<PlanPricingApiSpotsResultDto_v3>();

            foreach (var spot in request.Spots)
            {
                var result = new PlanPricingApiSpotsResultDto_v3
                {
                    ManifestId = spot.Id,
                    MediaWeekId = spot.MediaWeekId,
                    Frequencies = spot.SpotCost
                        .Select(x => new SpotFrequencyResponse
                        {
                            SpotLengthId = x.SpotLengthId,
                            Frequency = 1
                        })
                        .ToList()
                };

                results.Add(result);
            }


            return await Task.FromResult(
                new PlanPricingApiSpotsResponseDto_v3
                {
                    RequestId = "djj4j4399fmmf1m212",
                    Results = results
                });
        }
    }
}
