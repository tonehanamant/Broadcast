using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{

    public class PlanPricingAllocationResultDto
    {
        public PlanPricingAllocationResult NsiResults { get; set; }
        public PlanPricingAllocationResult NtiResults { get; set; }
    }

    public class PlanPricingAllocationResult
    {
        public PlanPricingAllocationResult()
        {
            Spots = new List<PlanPricingAllocatedSpot>();
        }

        public string RequestId { get; set; }

        public decimal PricingCpm { get; set; }

        public int? JobId { get; set; }

        public int PlanVersionId { get; set; }

        public string PricingVersion { get; set; }

        public PostingTypeEnum PostingType { get; set; }

        public List<PlanPricingAllocatedSpot> Spots { get; set; }

        public SpotAllocationModelMode SpotAllocationModelMode { get; set; } = SpotAllocationModelMode.Quality;
    }
}
