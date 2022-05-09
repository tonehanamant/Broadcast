using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class ProgramInventoryOptionalParametersDto
    {
        public decimal? MinCPM { get; set; }

        public decimal? MaxCPM { get; set; }

        public double? InflationFactor { get; set; }

        public MarketGroupEnum MarketGroup { get; set; }
        public int? ShareBookId { get; set; }
        public int? HUTBookId { get; set; }
    }
}
