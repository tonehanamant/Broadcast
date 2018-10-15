namespace Services.Broadcast.Entities
{
    public class OpenMarketPricing
    {
        public decimal? CpmMin { get; set; }
        public decimal? CpmMax { get; set; }
        public int? UnitCapPerStation { get; set; }
        public OpenMarketCpmTarget? OpenMarketCpmTarget { get; set; }
    }
}
