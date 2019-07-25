using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventoryAggregationDto
    {
        public int InventorySourceId { get; set; }
        public InventorySourceTypeEnum InventorySourceType { get; set; }
    }
}
