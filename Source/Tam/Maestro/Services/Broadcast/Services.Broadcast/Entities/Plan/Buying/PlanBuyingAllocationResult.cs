using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingAllocationResult
    {
        public string RequestId { get; set; }

        public decimal BuyingCpm { get; set; }

        public int? JobId { get; set; }

        public int PlanVersionId { get; set; }

        public string BuyingVersion { get; set; }

        public PostingTypeEnum PostingType { get; set; }

        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }

        public List<PlanBuyingAllocatedSpot> AllocatedSpots { get; set; } = new List<PlanBuyingAllocatedSpot>();

        public List<PlanBuyingAllocatedSpot> UnallocatedSpots { get; set; } = new List<PlanBuyingAllocatedSpot>();
    }
}
