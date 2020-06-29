using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResultMarketsDto
    {
        public int PlanVersionId { get; set; }
        public int? PricingJobId { get; set; }
        public PlanPricingResultMarketsTotalsDto MarketTotals { get; set; }
        public List<PlanPricingResultMarketDetailsDto> MarketDetails { get; set; }
    }
}