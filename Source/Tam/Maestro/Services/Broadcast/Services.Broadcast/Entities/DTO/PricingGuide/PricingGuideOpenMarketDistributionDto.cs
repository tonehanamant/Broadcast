using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class PricingGuideOpenMarketDistributionDto : BasePricingGuideDto
    {
        public OpenMarketPricingGuideGridDisplayFilterDto DisplayFilter { get; set; } = new OpenMarketPricingGuideGridDisplayFilterDto();

        public OpenMarketPricingGuideGridFilterDto Filter { get; set; } = new OpenMarketPricingGuideGridFilterDto();

        public List<PricingGuideMarketDto> Markets { get; set; } = new List<PricingGuideMarketDto>();

        public List<PricingGuideMarketTotalsDto> AllMarkets { get; set; } = new List<PricingGuideMarketTotalsDto>();

        public OpenMarketTotalsDto OpenMarketTotals { get; set; }

        public ProprietaryTotalsDto ProprietaryTotals { get; set; }

        public PricingTotalsDto PricingTotals { get; set; }

        public class OpenMarketTotalsDto
        {
            public decimal Cpm { get; set; }
            public decimal Cost { get; set; }
            public double Impressions { get; set; }
            public double Coverage { get; set; }
        }

        public class ProprietaryTotalsDto
        {
            public decimal Cpm { get; set; }
            public decimal Cost { get; set; }
            public double Impressions { get; set; }
        }

        public class PricingTotalsDto
        {
            public decimal Cpm { get; set; }
            public decimal Cost { get; set; }
            public double Impressions { get; set; }
            public double Coverage { get; set; }
        }
    }
}
