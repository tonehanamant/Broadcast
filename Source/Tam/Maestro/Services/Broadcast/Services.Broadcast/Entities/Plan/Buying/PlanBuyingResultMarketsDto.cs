using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingResultMarketsDto
    {
        public int PlanVersionId { get; set; }
        public int? BuyingJobId { get; set; }
        public PlanBuyingProgramTotalsDto Totals { get; set; } = new PlanBuyingProgramTotalsDto();
        public List<PlanBuyingResultMarketDetailsDto> Details { get; set; } = new List<PlanBuyingResultMarketDetailsDto>();
    }

    public class PlanBuyingResultMarketDetailsDto
    {
        public int Rank { get; set; }
        public double MarketCoveragePercent { get; set; }
        public int StationCount { get; set; }
        public int SpotCount { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public double? ShareOfVoiceGoalPercentage { get; set; }
        public double ImpressionsPercentage { get; set; }
        public string MarketName { get; set; }
    }
}