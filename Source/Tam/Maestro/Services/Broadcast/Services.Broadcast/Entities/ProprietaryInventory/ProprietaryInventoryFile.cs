using System;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities.ProprietaryInventory
{
    public class ProprietaryInventoryFile : InventoryFileBase
    {
        public ProprietaryInventoryHeader Header { get; set; } 
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> ValidationProblems { get; set; } = new List<string>();

        public List<ProprietaryInventoryDataLine> DataLines { get; set; } = new List<ProprietaryInventoryDataLine>();

        public class ProprietaryInventoryDataLine
        {
            public string Station { get; set; }

            public string Comment { get; set; }

            public string Program { get; set; }

            public double? Impressions { get; set; }

            public decimal? CPM { get; set; }

            public decimal? SpotCost { get; set; }

            public List<DisplayDaypart> Dayparts { get; set; }

            public List<Unit> Units { get; set; } = new List<Unit>();

            public List<Week> Weeks { get; set; } = new List<Week>();

            public List<LineAudience> Audiences { get; set; } = new List<LineAudience>();

            public int RowIndex { get; set; }

            public class Unit
            {
                public ProprietaryInventoryUnit ProprietaryInventoryUnit { get; set; }

                public int? Spots { get; set; }
            }

            public class Week
            {
                public int MediaWeekId { get; set; }

                public int? Spots { get; set; }
            }

            public class LineAudience
            {
                public DisplayAudience Audience { get; set; }
                public double? Impressions { get; set; }
                public double? Rating { get; set; }
                public decimal? Cpm { get; set; }
                public double? Vpvh { get; set; }
            }
        }
    }
}

