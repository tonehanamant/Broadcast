namespace Services.Broadcast.Entities.InventoryProprietary
{
    public class InventoryProprietarySummaryByStationByAudience
    {
        public int AudienceId { get; set; }

        public short MarketCode { get; set; }

        public int StationId { get; set; }

        public double Impressions { get; set; }

        public int SpotsPerWeek { get; set; }

        public decimal CostPerWeek { get; set; }
    }
}
