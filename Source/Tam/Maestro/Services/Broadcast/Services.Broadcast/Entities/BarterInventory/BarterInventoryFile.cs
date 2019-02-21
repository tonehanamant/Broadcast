using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.BarterInventory
{
    public class BarterInventoryFile : InventoryFileBase
    {
        public BarterInventoryHeader Header { get; set; } 
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> ValidationProblems { get; set; } = new List<string>();
    }
}
