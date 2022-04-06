using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingResultsReportRequest
    {
        public int PlanId { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
    }
}
