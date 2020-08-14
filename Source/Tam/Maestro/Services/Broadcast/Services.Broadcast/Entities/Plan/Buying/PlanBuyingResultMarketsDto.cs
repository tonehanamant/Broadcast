using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingResultMarketsDto
    {
        public int PlanVersionId { get; set; }
        public int? BuyingJobId { get; set; }
        public PlanBuyingResultMarketsTotalsDto Totals { get; set; }
        public List<PlanBuyingResultMarketDetailsDto> MarketDetails { get; set; }
    }
}