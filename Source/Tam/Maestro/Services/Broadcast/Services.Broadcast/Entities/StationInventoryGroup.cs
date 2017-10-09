using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class StationInventoryGroup
    {
        public StationInventoryGroup()
        {
            Manifests = new List<StationInventoryManifest>();
        }
        public int? Id { get; set; }
        public string Name { get; set; }
        public string DaypartCode { get; set; }
        public int SlotNumber { get; set; }
        public List<StationInventoryManifest> Manifests { get; set; } 
    }
}
