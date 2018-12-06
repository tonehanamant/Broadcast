namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class OpenMarketPricingGuideDto
    {
        public decimal? CpmMin { get; set; }
        public decimal? CpmMax { get; set; }
        public int? UnitCapPerStation { get; set; }
        public OpenMarketCpmTarget? OpenMarketCpmTarget { get; set; }
    }
}
