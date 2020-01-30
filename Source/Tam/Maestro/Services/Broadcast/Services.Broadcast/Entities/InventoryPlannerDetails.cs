namespace Services.Broadcast.Entities
{
    public class InventoryPlannerDetails
    {
        public int InventoryTypeId { get; set; }
        public decimal? Budget { get; set; }
        public double? CPM { get; set; }
        public decimal? Impressions { get; set; }
        public double? SharePercent { get; set; }
        public string EstimateId { get; set; }
        public string Notes { get; set; }
    }
}
