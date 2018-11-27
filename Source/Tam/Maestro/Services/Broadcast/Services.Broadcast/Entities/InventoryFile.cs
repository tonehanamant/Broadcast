using Newtonsoft.Json;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public class InventoryFile
    {
        public enum FileStatusEnum
        {
            Pending = 1,
            Loaded = 2,
            Failed = 3
        }

        public InventoryFile()
        {
            InventoryGroups = new List<StationInventoryGroup>();
            InventoryManifests = new List<StationInventoryManifest>();
            InventoryManifestsStaging = new List<StationInventoryManifestStaging>();
        }

        public int Id { get; set; }
        public string FileName { get; set; }
        public string UniqueIdentifier { get; set; }
        public FileStatusEnum FileStatus { get; set; }
        public string Hash { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? RatingBook { get; set; }
        public ProposalEnums.ProposalPlaybackType? PlaybackType { get; set; }
        public InventorySource InventorySource { get; set; }
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

        public List<StationInventoryGroup> InventoryGroups { get; set; }
        public List<StationInventoryManifest> InventoryManifests { get; set; } //Only used for OpenMarket
        public List<StationInventoryManifestStaging> InventoryManifestsStaging { get; set; } // Manifests for not found programs

        public IEnumerable<StationInventoryManifest> GetAllManifests()
        {
            return InventoryGroups
                .SelectMany(g => g.Manifests)
                .Union(InventoryManifests);
        }

        [JsonIgnore]
        public bool HasManifests => HasGroupManifests || HasInventoryManifests || HasInventoryManifestsStaging;

        private bool HasGroupManifests => InventoryGroups != null && InventoryGroups.Count > 0 && InventoryGroups.SelectMany(g => g.Manifests).Any();

        private bool HasInventoryManifests => InventoryManifests != null && InventoryManifests.Count > 0;

        private bool HasInventoryManifestsStaging => InventoryManifestsStaging != null && InventoryManifestsStaging.Count > 0;
    }
}
