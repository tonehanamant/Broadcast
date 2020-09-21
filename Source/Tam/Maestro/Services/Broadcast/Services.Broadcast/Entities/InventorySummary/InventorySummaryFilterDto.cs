using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryFilterDto
    {
        public int? InventorySourceId { get; set; }
        public QuarterDto Quarter { get; set; }
        public int? StandardDaypartId { get; set; }
        public InventorySourceTypeEnum? InventorySourceType { get; set; }
        public Dictionary<int, DateTime?> LatestInventoryUpdatesBySourceId { get; set; } = new Dictionary<int, DateTime?>();
    }
}
