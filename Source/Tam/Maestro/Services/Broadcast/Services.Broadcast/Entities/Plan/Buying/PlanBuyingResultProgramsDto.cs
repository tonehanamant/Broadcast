using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingResultProgramsDto
    {
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
        public PlanBuyingProgramTotalsDto Totals { get; set; } = new PlanBuyingProgramTotalsDto();
        public List<PlanBuyingProgramProgramDto> Details { get; set; } = new List<PlanBuyingProgramProgramDto>();
    }

    public class PlanBuyingProgramProgramDto
    {
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