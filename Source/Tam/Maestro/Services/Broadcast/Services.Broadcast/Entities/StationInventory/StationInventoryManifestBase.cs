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
        public List<StationInventoryManifestDaypart> ManifestDayparts { get; set; }

        public int? FileId { get; set; }
        public int InventorySourceId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<StationInventoryManifestAudience> ManifestAudiences { get; set; }
        public List<StationInventoryManifestAudience> ManifestAudiencesReferences { get; set; }

        public List<StationInventoryManifestRate> ManifestRates { get; set; }

        public StationInventoryManifestBase()
        {
            ManifestDayparts = new List<StationInventoryManifestDaypart>();
            ManifestAudiences = new List<StationInventoryManifestAudience>();
            ManifestAudiencesReferences = new List<StationInventoryManifestAudience>();
            ManifestRates = new List<StationInventoryManifestRate>();
        }
        // todo: this date needs to be populated
        public DateTime? EndDate { get; set; }
    }
}
