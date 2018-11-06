using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class PricingGuideAllocationSaveRequestDto
    {
        public int ProposalDetailId { get; set; }
        public OpenMarketPricingGuideGridDisplayFilterDto DisplayFilter { get; set; } = new OpenMarketPricingGuideGridDisplayFilterDto();
        public OpenMarketPricingGuideGridFilterDto Filter { get; set; } = new OpenMarketPricingGuideGridFilterDto();
        public List<PricingGuideMarketDto> Markets { get; set; } = new List<PricingGuideMarketDto>();
    }
}
