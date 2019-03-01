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

        public List<BarterInventoryDataLine> DataLines { get; set; } = new List<BarterInventoryDataLine>();

        public class BarterInventoryDataLine
        {
            public string Station { get; set; }

            public string Daypart { get; set; }

            public string Comment { get; set; }

            public List<Unit> Units { get; set; } = new List<Unit>();

            public class Unit
            {
                public BarterInventoryUnit BarterInventoryUnit { get; set; }

                public int? Spots { get; set; }
            }
        }
    }
}
