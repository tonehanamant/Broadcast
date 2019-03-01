using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.StationInventory
{
    public abstract class StationInventoryManifestBase
    {
        public int? Id { get; set; }
        public string DaypartCode { get; set; }
        public int SpotLengthId { get; set; }
        public int? SpotsPerWeek { get; set; }
        public int? SpotsPerDay { get; set; }
        public List<StationInventoryManifestDaypart> ManifestDayparts { get; set; } = new List<StationInventoryManifestDaypart>();

        public int? FileId { get; set; }
        public int InventorySourceId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<StationInventoryManifestAudience> ManifestAudiences { get; set; } = new List<StationInventoryManifestAudience>();
        public List<StationInventoryManifestAudience> ManifestAudiencesReferences { get; set; } = new List<StationInventoryManifestAudience>();

        public List<StationInventoryManifestRate> ManifestRates { get; set; } = new List<StationInventoryManifestRate>();

        public List<StationInventoryManifestWeek> ManifestWeeks { get; set; } = new List<StationInventoryManifestWeek>();

        // todo: this date needs to be populated
        public DateTime? EndDate { get; set; }
        public string Comment { get; set; }
    }
}
