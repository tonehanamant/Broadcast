using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingScxExportResponse
    {
        public DateTime Generated { get; set; }

        public int PlanId { get; set; }

        public int PlanVersionId { get; set; }

        public int PlanBuyingJobId { get; set; }

        public decimal PlanTargetCpm { get; set; }

        public double? AppliedMargin { get; set; }

        public int? UnallocatedCpmThreshold { get; set; }

        public List<PlanBuyingAllocatedSpot> Allocated { get; set; }

        public List<PlanBuyingAllocatedSpot> Unallocated { get; set; }
    }
}