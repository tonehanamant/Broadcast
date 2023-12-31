﻿using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryGroup
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int SlotNumber { get; set; }
        public InventorySource InventorySource { get; set; }
        public List<StationInventoryManifest> Manifests { get; set; } = new List<StationInventoryManifest>();
    }
}
