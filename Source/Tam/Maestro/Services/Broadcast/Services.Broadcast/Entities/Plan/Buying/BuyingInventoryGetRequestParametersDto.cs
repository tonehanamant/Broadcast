using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class BuyingInventoryGetRequestParametersDto
    {
        public decimal? MinCpm { get; set; }

        public decimal? MaxCpm { get; set; }

        public double? InflationFactor { get; set; }

        public double Margin { get; set; }

        public MarketGroupEnum MarketGroup { get; set; }
    }
}
