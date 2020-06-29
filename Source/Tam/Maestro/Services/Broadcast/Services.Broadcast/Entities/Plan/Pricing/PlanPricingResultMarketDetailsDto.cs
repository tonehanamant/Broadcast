namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResultMarketDetailsDto
    {
        public int Rank { get; set; }
        public double MarketCoveragePercent { get; set; }
        public int Stations { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public double? ShareOfVoiceGoalPercentage { get; set; } // what's difference between this and MarketCoveragePercent?
        public double ImpressionsPercentage { get; set; }
    }
}