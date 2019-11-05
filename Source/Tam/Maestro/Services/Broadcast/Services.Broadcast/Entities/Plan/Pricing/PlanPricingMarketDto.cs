namespace Services.Broadcast.Entities.PlanPricing
{
    public class PlanPricingMarketDto
    {
        public int MarketId { get; set; }
        public string MarketName { get; set; }
        public double? MarketShareOfVoice { get; set; }
        public int MarketSegment { get; set; }
    }
}
