using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResultBaseDto
    {
        public PlanPricingProgramTotalsDto Totals { get; set; } = new PlanPricingProgramTotalsDto();
        public List<PlanPricingProgramDto> Programs { get; set; } = new List<PlanPricingProgramDto>();
        public decimal OptimalCpm { get; set; }
        public int? JobId { get; set; }
        public int PlanVersionId { get; set; }
        public bool GoalFulfilledByProprietary { get; set; }
        public PostingTypeEnum PostingType { get; internal set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; } = SpotAllocationModelMode.Quality;
    }

    public class PlanPricingResultDto : PlanPricingResultBaseDto
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
        public double Impressions { get; set; }
        public double PercentageOfBuy { get; set; }
        public int Spots { get; set; }
        public decimal Budget { get; set; }
        public bool IsProprietary { get; set; }      
    }

    public class PlanPricingProgramTotalsDto
    {
        public int MarketCount { get; set; }
        public int StationCount { get; set; }
        public decimal AvgCpm { get; set; }
        public double AvgImpressions { get; set; }
        public double Impressions { get; set; }
        public decimal Budget { get; set; }
        public int Spots { get; set; }
        public decimal ImpressionsPercentage { get; set; }
    }
}