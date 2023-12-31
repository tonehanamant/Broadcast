﻿using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using System;
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
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> ValidationProblems { get; set; } = new List<string>();
        public int? RowsProcessed { get; set; }

        public Guid? UploadedFileSharedFolderFileId { get; set; }
        
        public Guid? ErrorFileSharedFolderFileId { get; set; }

        /// <summary>
        /// The start date of the inventory uploaded from the file
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// The end date of the inventory uploaded from the file
        /// </summary>
        public DateTime? EndDate { get; set; }

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
