namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryManifestDto
    {
        public int? StationId { get; set; }
        public short? MarketCode { get; set; }
        public string DaypartCode { get; set; }
        public int? FileId { get; set; }
        public int ManifestId { get; set; }
    }
}