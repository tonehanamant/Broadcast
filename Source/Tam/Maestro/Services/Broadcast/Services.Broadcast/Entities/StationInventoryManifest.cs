using System;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class StationInventoryManifest
    {
        public int? Id { get; set; }
        public DisplayBroadcastStation Station { get; set; }
        public string DaypartCode { get; set; }
        public int SpotLengthId { get; set; } 
        public int? SpotsPerWeek { get; set; }
        public int? SpotsPerDay { get; set; }
        public List<StationInventoryManifestDaypart> Dayparts { get; set; }

        public int? FileId { get; set; }
        public int InventorySourceId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<StationInventoryManifestAudience> ManifestAudiences { get; set; }

        public StationInventoryManifest()
        {
            Dayparts = new List<StationInventoryManifestDaypart>();
            ManifestAudiences = new List<StationInventoryManifestAudience>();
        }
        // todo: this date needs to be populated
        public DateTime? EndDate { get; set; }
    }
}
