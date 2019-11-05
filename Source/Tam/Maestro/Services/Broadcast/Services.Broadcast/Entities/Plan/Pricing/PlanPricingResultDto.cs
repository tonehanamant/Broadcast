using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResultDto
    {
        public List<PricingModelWeekInputDto> Weeks { get; set; }
        public List<PricingModelSpotsDto> Spots { get; set; }
        public PlanPricingApiRequestParametersDto Parameters { get; set; }
        public PlanPricingApiResponsetDto Response { get; set; }

    }
}