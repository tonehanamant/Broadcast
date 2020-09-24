using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingResultMarkets
    {
        public int PlanVersionId { get; set; }
        public int? PricingJobId { get; set; }
        public PlanPricingResultMarketsTotals Totals { get; set; }
        public List<PlanPricingResultMarketDetails> MarketDetails { get; set; } = new List<PlanPricingResultMarketDetails>();
    }

    public class PlanPricingResultMarketsTotals
    {
        public int Markets { get; set; }
        public double CoveragePercent { get; set; }
        public int Stations { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
    }

    public class PlanPricingResultMarketDetails
    {
        public short MarketCode { get; set; }
        public int Rank { get; set; }
        public double MarketCoveragePercent { get; set; }
        public int Stations { get; set; }
        public int StationsPerMarket { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public double? ShareOfVoiceGoalPercentage { get; set; }
        public double ImpressionsPercentage { get; set; }
        public string MarketName { get; set; }
        public bool IsProprietary { get; set; }
    }
}
