using System;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

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

            public string Comment { get; set; }

            public string Program { get; set; }

            public double? Impressions { get; set; }

            public decimal? CPM { get; set; }

            public List<DisplayDaypart> Dayparts { get; set; }

            public List<Unit> Units { get; set; } = new List<Unit>();

            public List<Week> Weeks { get; set; } = new List<Week>();

            public class Unit
            {
                public BarterInventoryUnit BarterInventoryUnit { get; set; }

                public int? Spots { get; set; }
            }

            public class Week
            {
                public int MediaWeekId { get; set; }

                public int? Spots { get; set; }
            }
        }
    }
}
