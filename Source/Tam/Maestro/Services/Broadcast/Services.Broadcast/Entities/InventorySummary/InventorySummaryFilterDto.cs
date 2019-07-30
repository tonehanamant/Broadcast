using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryFilterDto
    {
        public int? InventorySourceId { get; set; }
        public InventorySummaryQuarter Quarter { get; set; }
        public int? DaypartCodeId { get; set; }
        public InventorySourceTypeEnum? InventorySourceType { get; set; }
        public Dictionary<int, int> LatestInventorySourceFileIds {get; set;}
    }
}
