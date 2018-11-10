using Services.Broadcast.Entities.DTO.PricingGuide;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class ProposalDetailPricingGuideDto 
    {
        public int ProposalId { get; set; }
        public int ProposalDetailId { get; set; }

        public double? AdjustmentRate { get; set; }
        public double? AdjustmentMargin { get; set; }
        public double? GoalImpression { get; set; }
        public decimal? GoalBudget { get; set; }
        public double? AdjustmentInflation { get; set; }

        public List<ProprietaryPricingDto> ProprietaryPricing { get; set; } = new List<ProprietaryPricingDto>();
        public OpenMarketPricingGuideDto OpenMarketPricing { get; set; } = new OpenMarketPricingGuideDto();
    }
}
