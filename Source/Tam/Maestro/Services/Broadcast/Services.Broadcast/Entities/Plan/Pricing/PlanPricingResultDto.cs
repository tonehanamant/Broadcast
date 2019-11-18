using Services.Broadcast.Entities.PlanPricing;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResultDto
    {
       public List<PlanPricingProgramDto> Programs { get; set; }
    }

    public class PlanPricingProgramDto
    {
        public string ProgramName { get; set; }
        public string Genre { get; set; }
        public int MarketCount { get; set; }
        public int StationCount { get; set; }
        public decimal AvgCpm { get; set; }
        public double AvgImpressions { get; set; }
        public double PercentageOfBuy { get; set; }
    }
}