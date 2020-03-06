using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingAllocationResult
    {
        public string RequestId { get; set; }

        public decimal OptimalCpm { get; set; }

        public List<PlanPricingAllocatedSpot> Spots { get; set; }
    }
}
