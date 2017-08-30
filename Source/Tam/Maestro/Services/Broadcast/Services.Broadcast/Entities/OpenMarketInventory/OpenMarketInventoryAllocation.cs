namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class OpenMarketInventoryAllocation
    {
        public int ProposalVersionDetailId { get; set; }
        public int StationProgramId { get; set; }
        public int StationProgramFlightId { get; set; }
        public int MediaWeekId { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }
        public int Spots { get; set; }
        public int Impressions { get; set; }
        public decimal SpotCost { get; set; }
    }
}
