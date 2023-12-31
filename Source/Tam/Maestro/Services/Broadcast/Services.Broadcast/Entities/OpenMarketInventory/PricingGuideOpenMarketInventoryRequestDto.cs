﻿using Services.Broadcast.Entities.DTO;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PricingGuideOpenMarketInventoryRequestDto
    {
        public int ProposalId { get; set; }
        public int ProposalDetailId { get; set; }
        public OpenMarketPricingGuide OpenMarketPricing { get; set; } = new OpenMarketPricingGuide();
        public bool? HasPricingGuideChanges { get; set; }
        public decimal? BudgetGoal { get; set; }
        public double? ImpressionGoal { get; set; }
        public List<ProprietaryPricingDto> ProprietaryPricing { get; set; }
    }
}