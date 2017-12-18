namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class OpenMarketInventoryAllocation
    {
        public int Id { get; set; }
        public int ProposalVersionDetailId { get; set; }
        public int ManifestId { get; set; }
        public int StationProgramFlightId { get; set; }
        public int MediaWeekId { get; set; }
        public int? ProposalVersionDetailQuarterWeekId { get; set; }
        public int Spots { get; set; }
        public double TotalImpressions { get; set; }
        public double UnitImpressions { get; set; }
        public decimal SpotCost { get; set; }
        public decimal Rate { get; set; }
    }
}
