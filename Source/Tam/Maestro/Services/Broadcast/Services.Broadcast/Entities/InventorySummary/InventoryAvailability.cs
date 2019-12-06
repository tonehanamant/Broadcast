using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventoryAvailability
    {
        public List<InventoryGapDetail> InventoryGaps { get; set; }
        public QuarterDto StartQuarter { get; set; }

        public QuarterDto EndQuarter { get; set; }
    }
}
