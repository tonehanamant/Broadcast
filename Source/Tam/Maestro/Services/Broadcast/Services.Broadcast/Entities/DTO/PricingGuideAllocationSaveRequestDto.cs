using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class PricingGuideAllocationSaveRequestDto
    {
        public int ProposalDetailId { get; set; }
        public OpenMarketPricingGuideGridDisplayFilterDto DisplayFilter { get; set; } = new OpenMarketPricingGuideGridDisplayFilterDto();
        public OpenMarketPricingGuideGridFilterDto Filter { get; set; } = new OpenMarketPricingGuideGridFilterDto();
        public List<PricingGuideOpenMarketInventory.PricingGuideMarket> Markets { get; set; } = new List<PricingGuideOpenMarketInventory.PricingGuideMarket>();
    }
}
