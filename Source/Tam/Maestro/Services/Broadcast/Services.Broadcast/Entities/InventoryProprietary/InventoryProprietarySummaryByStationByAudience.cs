namespace Services.Broadcast.Entities.InventoryProprietary
{
    public class InventoryProprietarySummaryByStationByAudience
    {
        public int AudienceId { get; set; }

        public short MarketCode { get; set; }

        public int StationId { get; set; }

        public double Impressions { get; set; }
    }
}
