using System;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryManifestStaging : StationInventoryManifestBase
    {
        public StationInventoryManifestStaging()
        {
            ManifestId = Guid.NewGuid().ToString();
        }

        // ManifestId is needed for identification of manifests when getting them from the db. 
        // See station_inventory_manifest_staging structure
        public string ManifestId { get; set; }

        public string Station { get; set; }
    }
}
