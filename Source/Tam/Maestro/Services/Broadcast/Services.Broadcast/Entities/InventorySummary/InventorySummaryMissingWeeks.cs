using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventoryGapDetail
    {
        public QuarterDetailDto Quarter { get; set; }
        public bool AllQuarterMissing { get; set; }
        public List<DateRange> DateGaps { get; set; }
    }
}
