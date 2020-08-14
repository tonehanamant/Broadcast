namespace Services.Broadcast.Entities.Buying
{
    public class PlanBuyingMarketDto
    {
        public int MarketId { get; set; }
        public string MarketName { get; set; }
        public double? MarketShareOfVoice { get; set; }
        public int MarketSegment { get; set; }
    }
}
