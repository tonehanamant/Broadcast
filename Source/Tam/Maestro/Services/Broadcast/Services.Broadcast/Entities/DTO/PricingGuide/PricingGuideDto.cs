using Newtonsoft.Json;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class PricingGuideDto
    {
        public int ProposalId { get; set; }
        public int ProposalVersionId { get; set; }
        public int ProposalDetailId { get; set; }
        public decimal? BudgetGoal { get; set; }
        public double? ImpressionGoal { get; set; }
        public decimal OpenMarketShare { get; set; }
        public double? Margin { get; set; }
        public double? Inflation { get; set; }
        public double? ImpressionLoss { get; set; }
        public OpenMarketPricingGuideDto OpenMarketPricing { get; set; } = new OpenMarketPricingGuideDto();
        public List<ProprietaryPricingDto> ProprietaryPricing { get; set; }
        public Dictionary<int, string> InventorySourceEnum { get; set; } = Enum.GetValues(typeof(InventorySourceEnum))
                                                                                .Cast<InventorySourceEnum>()
                                                                                .ToDictionary(e => (int)e, e => e.ToString());


        public int DistributionId { get; set; }

        public OpenMarketPricingGuideGridDisplayFilterDto DisplayFilter { get; set; } = new OpenMarketPricingGuideGridDisplayFilterDto();
        public OpenMarketPricingGuideGridFilterDto Filter { get; set; } = new OpenMarketPricingGuideGridFilterDto();

        public List<PricingGuideMarketDto> Markets { get; set; } = new List<PricingGuideMarketDto>();
        public List<PricingGuideMarketTotalsDto> AllMarkets { get; set; } = new List<PricingGuideMarketTotalsDto>();
        public OpenMarketTotalsDto OpenMarketTotals { get; set; }
        public ProprietaryTotalsDto ProprietaryTotals { get; set; }
        public PricingTotalsDto PricingTotals { get; set; }
        public int MarketCoverageFileId { get; set; }
        public bool HasEditedManuallySpots => Markets.Any(x => x.HasEditedManuallySpots);

        [JsonIgnore]
        public bool MaintainManuallyEditedSpots { get; set; }
    }
}
