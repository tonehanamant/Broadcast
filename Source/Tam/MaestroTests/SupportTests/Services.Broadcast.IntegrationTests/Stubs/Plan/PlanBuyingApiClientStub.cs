using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.Buying;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;

namespace Services.Broadcast.IntegrationTests.Stubs.Plan
{
    public class PlanBuyingApiClientStub : IPlanBuyingApiClient
    {
        public PlanBuyingApiRequestDto LastSentRequest;

        public PlanBuyingApiSpotsResponseDto GetBuyingSpotsResult(PlanBuyingApiRequestDto request)
        {
            LastSentRequest = request;

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
            var results = new List<PlanBuyingApiSpotsResultDto_v3>();

            foreach (var spot in request.Spots)
            {
                var result = new PlanBuyingApiSpotsResultDto_v3
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

            return new PlanBuyingApiSpotsResponseDto_v3
            {
                RequestId = "djj4j4399fmmf1m212",
            };
        }
    }
}
