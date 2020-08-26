using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingResultOwnershipGroupDto
    {
        public int PlanVersionId { get; set; }
        public int? BuyingJobId { get; set; }
        public PlanBuyingProgramTotalsDto Totals { get; set; } = new PlanBuyingProgramTotalsDto();
        public List<PlanBuyingResultOwnershipGroupDetailsDto> Details { get; set; } = new List<PlanBuyingResultOwnershipGroupDetailsDto>();
    }

    public class PlanBuyingResultOwnershipGroupDetailsDto
    {
        public string OwnershipGroupName { get; set; }
        public int Markets { get; set; }
        public int Stations { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public double ImpressionsPercentage { get; set; }
    }
}
