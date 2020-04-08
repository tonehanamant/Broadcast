using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResultBaseDto
    {
        public PlanPricingTotalsDto Totals { get; set; } = new PlanPricingTotalsDto();
        public List<PlanPricingProgramDto> Programs { get; set; } = new List<PlanPricingProgramDto>();
        public decimal OptimalCpm { get; set; }
        public bool GoalFulfilledByProprietary { get; set; }
    }

    public class GetPlanPricingResultDto : PlanPricingResultBaseDto
    {
        public string Notes { get; set; }
    }

    public class PlanPricingProgramDto
    {
        public int Id { get; set; }
        public string ProgramName { get; set; }
        public string Genre { get; set; }
        public int MarketCount { get; set; }
        public int StationCount { get; set; }
        public decimal AvgCpm { get; set; }
        public double AvgImpressions { get; set; }
        public double PercentageOfBuy { get; set; }
    }

    public class PlanPricingTotalsDto
    {
        public int MarketCount { get; set; }
        public int StationCount { get; set; }
        public decimal AvgCpm { get; set; }
        public double AvgImpressions { get; set; }
    }
}