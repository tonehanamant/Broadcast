using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class BuyingInventoryGetRequestParametersDto
    {
        public decimal? MinCpm { get; set; }

        public decimal? MaxCpm { get; set; }

        public double? InflationFactor { get; set; }

        public List<int> InventorySourceIds { get; set; } = new List<int>();

        public double Margin { get; set; }

        public MarketGroupEnum MarketGroup { get; set; }
    }
}
