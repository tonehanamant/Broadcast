using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingResultBaseDto
    {
        public PlanBuyingProgramTotalsDto Totals { get; set; } = new PlanBuyingProgramTotalsDto();
        public List<PlanBuyingProgramDto> Programs { get; set; } = new List<PlanBuyingProgramDto>();
        public decimal OptimalCpm { get; set; }
        public int? JobId { get; set; }
        public int PlanVersionId { get; set; }
        public bool GoalFulfilledByProprietary { get; set; }
    }

    public class PlanBuyingResultDto : PlanBuyingResultBaseDto
    {
        public string Notes { get; set; }
    }

    public class PlanBuyingProgramDto
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
    }

    public class PlanBuyingProgramTotalsDto
    {
        public int MarketCount { get; set; }
        public double MarketCoveragePercent { get; set; }
        public int StationCount { get; set; }
        public decimal AvgCpm { get; set; }
        public double AvgImpressions { get; set; }
        public double Impressions { get; set; }
        public decimal Budget { get; set; }
        public int Spots { get; set; }
        public decimal ImpressionsPercentage { get; set; } = 100;
    }
}