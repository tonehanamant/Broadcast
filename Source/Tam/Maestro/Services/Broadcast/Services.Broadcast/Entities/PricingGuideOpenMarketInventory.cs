using Services.Broadcast.Entities.DTO.PricingGuide;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PricingGuideOpenMarketInventory : ProposalDetailInventoryBase
    {
        public OpenMarketCriterion Criteria { get; set; }
        public List<PricingGuideMarketDto> Markets { get; set; } = new List<PricingGuideMarketDto>();
        public List<PricingGuideMarketTotalsDto> AllMarkets { get; set; } = new List<PricingGuideMarketTotalsDto>();
        public double? MarketCoverage { get; set; }
    }
}
