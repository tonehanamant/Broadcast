namespace Services.Broadcast.Entities
{
    public class GroupedDisplayProposalMarketPrograms
    {
        public int MarketId { get; set; }
        public string MarketName { get; set; }
        public double MarketSubscribers { get; set; }
        public int Rank { get; set; }
        public ProposalTotalFields MarketTotals { get; set; }
    }
}
