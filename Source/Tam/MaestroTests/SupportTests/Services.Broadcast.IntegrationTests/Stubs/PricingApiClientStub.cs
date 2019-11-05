using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.Pricing;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class PricingApiClientStub : IPricingApiClient
    {
        public PlanPricingApiResponsetDto GetPricingCalculationResult(PlanPricingApiRequestDto request)
        {
            return new PlanPricingApiResponsetDto
            {
                RequestId = "a3289ujvb3,s,aksa",
                Results = new PlanPricingApiResultDto
                {
                    MinimumCost = 50
                }
            };
        }
    }
}
