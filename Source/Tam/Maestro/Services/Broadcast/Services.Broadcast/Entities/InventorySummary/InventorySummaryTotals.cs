using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryTotals
    {
        public int TotalMarkets { get; set; }
        public int TotalStations { get; set; }
        public int TotalPrograms { get; set; }

        public double TotalHouseholdImpressions { get; set; }
    }
}
