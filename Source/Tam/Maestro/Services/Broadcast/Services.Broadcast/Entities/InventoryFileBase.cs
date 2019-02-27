using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class InventoryFileBase
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public FileStatusEnum FileStatus { get; set; }
        public string Hash { get; set; }
        public InventorySource InventorySource { get; set; }
        public string UniqueIdentifier { get; set; }

        public List<StationInventoryGroup> InventoryGroups { get; set; } = new List<StationInventoryGroup>();
        public List<StationInventoryManifest> InventoryManifests { get; set; } = new List<StationInventoryManifest>();
        public List<StationInventoryManifestStaging> InventoryManifestsStaging { get; set; } = new List<StationInventoryManifestStaging>();
    }
}
