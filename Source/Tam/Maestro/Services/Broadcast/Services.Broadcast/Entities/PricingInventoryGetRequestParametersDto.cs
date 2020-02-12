namespace Services.Broadcast.Entities
{
    public class PricingInventoryGetRequestParametersDto
    {
        public decimal? MinCpm { get; set; }
        public decimal? MaxCpm { get; set; }
        public double? InflationFactor { get; set; }
    }
}