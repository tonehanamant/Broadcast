namespace Services.Broadcast.Entities
{
    public class StationProgramFlightDto
    {
        public int StationProgramFlightId { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public string CreatedBy { get; set; }
    }
}