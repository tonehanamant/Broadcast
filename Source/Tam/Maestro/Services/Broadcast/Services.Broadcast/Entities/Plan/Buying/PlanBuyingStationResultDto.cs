using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingStationResultDto
    {
        public int Id { get; set; }
        public int? BuyingJobId { get; set; }
        public int? PlanVersionId { get; set; }
        public PlanBuyingProgramTotalsDto Totals { get; set; } = new PlanBuyingProgramTotalsDto();
        public List<PlanBuyingStationDto> Details { get; set; } = new List<PlanBuyingStationDto>();
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
    }

    public class PlanBuyingStationDto
    {
        public string Station { get; set; }
        public string Market { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public double ImpressionsPercentage { get; set; }
    }
}