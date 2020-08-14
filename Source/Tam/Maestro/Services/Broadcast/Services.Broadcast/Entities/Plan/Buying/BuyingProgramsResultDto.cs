using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class BuyingProgramsResultDto
    {
        public BuyingProgramsResultTotalsDto Totals { get; set; } = new BuyingProgramsResultTotalsDto();
        public List<PlanBuyingProgramProgramDto> Programs { get; set; } = new List<PlanBuyingProgramProgramDto>();
    }

    public class BuyingProgramsResultTotalsDto
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

    public class PlanBuyingProgramProgramDto
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