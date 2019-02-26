using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public class InventoryFile : InventoryFileBase
    {                    
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        private List<StationContact> _stationContacts; 

        public List<StationContact> StationContacts
        {
            get
            {
                if (_stationContacts == null)
                {
                    _stationContacts = new List<StationContact>();
                }
                return _stationContacts;
            }
            set { _stationContacts = value;  }
        }

        public List<StationInventoryGroup> InventoryGroups { get; set; } = new List<StationInventoryGroup>();
        public List<StationInventoryManifest> InventoryManifests { get; set; } = new List<StationInventoryManifest>();
        public List<StationInventoryManifestStaging> InventoryManifestsStaging { get; set; } = new List<StationInventoryManifestStaging>();

        public IEnumerable<StationInventoryManifest> GetAllManifests()
        {
            return InventoryGroups
                .SelectMany(g => g.Manifests)
                .Union(InventoryManifests);
        }

        public bool HasManifests() => HasGroupManifests || HasInventoryManifests || HasInventoryManifestsStaging;

        private bool HasGroupManifests => InventoryGroups != null && InventoryGroups.Count > 0 && InventoryGroups.SelectMany(g => g.Manifests).Any();

        private bool HasInventoryManifests => InventoryManifests != null && InventoryManifests.Count > 0;

        private bool HasInventoryManifestsStaging => InventoryManifestsStaging != null && InventoryManifestsStaging.Count > 0;
    }
}
