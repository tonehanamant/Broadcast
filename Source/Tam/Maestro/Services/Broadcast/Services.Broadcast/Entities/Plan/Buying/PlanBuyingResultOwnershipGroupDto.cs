using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingResultOwnershipGroupDto
    {
        public int PlanVersionId { get; set; }
        public int? BuyingJobId { get; set; }
        public PlanBuyingProgramTotalsDto Totals { get; set; } = new PlanBuyingProgramTotalsDto();
        public List<PlanBuyingResultOwnershipGroupDetailsDto> Details { get; set; } = new List<PlanBuyingResultOwnershipGroupDetailsDto>();
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
    }

    public class PlanBuyingResultOwnershipGroupDetailsDto
    {
        public string OwnershipGroupName { get; set; }
        public int MarketCount { get; set; }
        public int StationCount { get; set; }
        public int SpotCount { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public double ImpressionsPercentage { get; set; }
    }
}
