using Services.Broadcast.Entities.DTO;

namespace Services.Broadcast.Entities
{
    public class OpenMarketPricingGuide
    {
        public decimal? CpmMin { get; set; }
        public decimal? CpmMax { get; set; }
        public int? UnitCapPerStation { get; set; }
        public OpenMarketCpmTarget? OpenMarketCpmTarget { get; set; }

        public PricingGuideOpenMarketDistributionReport DistributionReport { get; set; }
    }
}
