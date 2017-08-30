namespace Services.Broadcast.Entities
{
    public class OpenMarketInventoryAllocationSnapshotDto
    {
        public int StationProgramFlightId { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }
        public int Spots { get; set; }
        public string CreatedBy { get; set; }
        public string Isci { get; set; }
        public int Impressions { get; set; }
        public decimal SpotCost { get; set; }
        public int SpotLengthId { get; set; }
        public short StationCode { get; set; }
        public int StationProgramId { get; set; }
    }
}
