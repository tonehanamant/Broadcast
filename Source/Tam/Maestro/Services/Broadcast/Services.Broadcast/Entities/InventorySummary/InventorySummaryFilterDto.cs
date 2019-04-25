namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryFilterDto
    {
        public int? InventorySourceId { get; set; }
        public InventorySummaryQuarter Quarter { get; set; }
    }
}
