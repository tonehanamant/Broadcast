using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryQuarterFilterDto
    {
        public List<InventorySummaryQuarter> Quarters { get; set; }
        public QuarterDetailDto DefaultQuarter { get; set; }
    }
}
