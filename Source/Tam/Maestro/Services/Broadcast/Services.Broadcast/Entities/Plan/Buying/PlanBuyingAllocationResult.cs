using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingAllocationResult
    {
        public string RequestId { get; set; }

        public decimal BuyingCpm { get; set; }

        public int? JobId { get; set; }

        public int PlanVersionId { get; set; }

        public string BuyingVersion { get; set; }

        public List<PlanBuyingAllocatedSpot> Spots { get; set; } = new List<PlanBuyingAllocatedSpot>();
    }
}
