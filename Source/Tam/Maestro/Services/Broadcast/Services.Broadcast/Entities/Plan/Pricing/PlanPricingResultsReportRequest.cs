using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResultsReportRequest
    {
        public int PlanId { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
        public PostingTypeEnum? PostingType { get; set; }
    }
}
