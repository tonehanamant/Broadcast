using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventoryQuartersDto
    {
        public List<QuarterDetailDto> Quarters { get; set; }
        public QuarterDetailDto DefaultQuarter { get; set; }
    }
}
