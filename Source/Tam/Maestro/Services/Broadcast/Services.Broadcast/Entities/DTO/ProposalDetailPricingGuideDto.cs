using Newtonsoft.Json;
using Services.Broadcast.Entities.Enums;
using System;
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
        public List<ProprietaryPricingDto> ProprietaryPricing { get; set; } = new List<ProprietaryPricingDto>();
        public OpenMarketPricingGuide OpenMarketPricing { get; set; } = new OpenMarketPricingGuide();
    }
}
