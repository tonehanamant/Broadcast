﻿using Services.Broadcast.Entities.StationInventory;
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

        public IEnumerable<StationInventoryManifest> GetAllManifests()
        {
            return InventoryGroups
                .SelectMany(g => g.Manifests)
                .Union(InventoryManifests);
        }

        public bool HasManifests() => HasGroupManifests || HasInventoryManifests;

        private bool HasGroupManifests => InventoryGroups != null && InventoryGroups.Count > 0 && InventoryGroups.SelectMany(g => g.Manifests).Any();

        private bool HasInventoryManifests => InventoryManifests != null && InventoryManifests.Count > 0;
    }
}
