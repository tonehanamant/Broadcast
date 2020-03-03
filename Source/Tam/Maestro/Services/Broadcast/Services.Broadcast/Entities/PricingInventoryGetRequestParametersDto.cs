using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PricingInventoryGetRequestParametersDto
    {
        public decimal? MinCpm { get; set; }

        public decimal? MaxCpm { get; set; }

        public double? InflationFactor { get; set; }

        public List<int> InventorySourceIds { get; set; } = new List<int>();
    }
}
