using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;

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
                    Spots = new List<PlanPricingApiResultSpotDto>
                    {
                        new PlanPricingApiResultSpotDto
                        {
                            Id = 1,
                            MediaWeekId = 200,
                            Cost = 1000,
                            Impressions = 1000,
                            Spots = 1
                        }
                    }
                }
            };
        }

        public PlanPricingApiResponsetDto GetPricingSpotsResult(PlanPricingApiRequestDto request)
        {
            return new PlanPricingApiResponsetDto
            {
                RequestId = "a3289ujvb3,s,aksa",
                Results = new PlanPricingApiResultDto
                {
                    Spots = new List<PlanPricingApiResultSpotDto>
                    {
                        new PlanPricingApiResultSpotDto
                        {
                            Id = 1,
                            MediaWeekId = 200,
                            Cost = 1000,
                            Impressions = 1000,
                            Spots = 1
                        }
                    }
                }
            };
        }
    }
}
