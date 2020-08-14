namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingResultMarketsTotalsDto
    {
        public int Markets { get; set; }
        public double CoveragePercent { get; set; }
        public int Stations { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
    }
}