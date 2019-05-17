using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<StationInventoryManifest> GetAllManifests()
        {
            return InventoryGroups
                .SelectMany(g => g.Manifests)
                .Union(InventoryManifests);
        }
    }
}
