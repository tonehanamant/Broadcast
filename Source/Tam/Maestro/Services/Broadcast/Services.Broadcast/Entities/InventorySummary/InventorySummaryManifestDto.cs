using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryManifestDto
    {
        public int? StationId { get; set; }
        public short? MarketCode { get; set; }
        public List<int> DaypartCodeIds { get; set; }
        public string UnitName { get; set; }
        public int? FileId { get; set; }
        public int ManifestId { get; set; }
    }
}