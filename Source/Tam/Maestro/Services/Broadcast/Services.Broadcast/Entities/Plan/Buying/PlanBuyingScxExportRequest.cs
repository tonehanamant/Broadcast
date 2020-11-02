namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingScxExportRequest
    {
        public int PlanId { get; set; }

        public int? UnallocatedCpmThreshold { get; set; }
    }
}