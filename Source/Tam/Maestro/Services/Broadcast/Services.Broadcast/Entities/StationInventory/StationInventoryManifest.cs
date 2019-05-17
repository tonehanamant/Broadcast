using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryManifest
    {
        public int? Id { get; set; }
        public string DaypartCode { get; set; }
        public int SpotLengthId { get; set; }
        public int? SpotsPerWeek { get; set; }
        public int? SpotsPerDay { get; set; }
        public int? InventoryFileId { get; set; }
        public int InventorySourceId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Comment { get; set; }
        public DisplayBroadcastStation Station { get; set; }
        public List<StationInventoryManifestDaypart> ManifestDayparts { get; set; } = new List<StationInventoryManifestDaypart>();
        public List<StationInventoryManifestAudience> ManifestAudiences { get; set; } = new List<StationInventoryManifestAudience>();
        public List<StationInventoryManifestAudience> ManifestAudiencesReferences { get; set; } = new List<StationInventoryManifestAudience>();
        public List<StationInventoryManifestRate> ManifestRates { get; set; } = new List<StationInventoryManifestRate>();
        public List<StationInventoryManifestWeek> ManifestWeeks { get; set; } = new List<StationInventoryManifestWeek>();
        public List<StationImpressions> ProjectedStationImpressions { get; set; } = new List<StationImpressions>();
    }
}
