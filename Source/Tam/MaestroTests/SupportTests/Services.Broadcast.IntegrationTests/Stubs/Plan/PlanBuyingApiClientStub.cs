using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.Buying;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;

namespace Services.Broadcast.IntegrationTests.Stubs.Plan
{
    public class PlanBuyingApiClientStub : IPlanBuyingApiClient
    {
        public PlanBuyingApiRequestDto_v3 LastSentRequest;        

        public async Task<PlanBuyingApiSpotsResponseDto_v3> GetBuyingSpotsResultAsync(PlanBuyingApiRequestDto_v3 request)
        {
            LastSentRequest = request;

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

            return await Task.FromResult(new PlanBuyingApiSpotsResponseDto_v3
            {
                RequestId = "djj4j4399fmmf1m212",
                Results = results
            });
        }
    }
}
