using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class PricingGuideOpenMarketInventoryRequestDto
    {
        public int ProposalId { get; set; }
        public int ProposalDetailId { get; set; }
        public decimal? BudgetGoal { get; set; }
        public double? ImpressionGoal { get; set; }
        public double? Margin { get; set; }
        public double? Inflation { get; set; }
        public double? ImpressionLoss { get; set; }
        public decimal OpenMarketShare { get; set; }
        public bool MaintainManuallyEditedSpots { get; set; }
        public OpenMarketPricingGuideDto OpenMarketPricing { get; set; } = new OpenMarketPricingGuideDto();
        public List<ProprietaryPricingDto> ProprietaryPricing { get; set; }
    }
}