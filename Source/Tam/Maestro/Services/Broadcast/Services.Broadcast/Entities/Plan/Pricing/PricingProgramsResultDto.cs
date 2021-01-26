using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PricingProgramsResultDto
    {
        public PricingProgramsResultDto()
        {
            Totals = new PricingProgramsResultTotalsDto();
            Programs = new List<PlanPricingProgramProgramDto>();
        }

        public PricingProgramsResultTotalsDto Totals { get; set; }
        public List<PlanPricingProgramProgramDto> Programs { get; set; }
        public PostingTypeEnum PostingType { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
    }

    public class PricingProgramsResultDto_v2
    {
        public PostingTypePlanPricingResultPrograms NsiResults { get; set; }
        public PostingTypePlanPricingResultPrograms NtiResults { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
    }

    public class PostingTypePlanPricingResultPrograms
    {
        public PricingProgramsResultTotalsDto Totals { get; set; }
        public List<PlanPricingProgramProgramDto> ProgramDetails { get; set; } = new List<PlanPricingProgramProgramDto>();
    }

    public class PricingProgramsResultTotalsDto
    {
        public int MarketCount { get; set; }
        public int Spots { get; set; }
        public int StationCount { get; set; }
        public double AvgImpressions { get; set; }
        public double Impressions { get; set; }
        public decimal AvgCpm { get; set; }
        public decimal Budget { get; set; }
        public double ImpressionsPercentage { get; set; }
    }

    public class PlanPricingProgramProgramDto
    {
        public int Id { get; set; }
        public string ProgramName { get; set; }
        public string Genre { get; set; }
        public int MarketCount { get; set; }
        public int Spots { get; set; }
        public int StationCount { get; set; }
        public double AvgImpressions { get; set; }
        public decimal AvgCpm { get; set; }
        public decimal Budget { get; set; }
        public double ImpressionsPercentage { get; set; }
        public double Impressions { get; set; }
    }
}