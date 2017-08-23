namespace Services.Broadcast.Entities
{
    public class ProposalMediaWeek
    {
        public int MediaWeekId { get; set; }
        public string Week { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string QuarterText { get; set; }
        public int QuarterId { get; set; }
        public byte Quarter { get; set; }
        public bool IsHiatus { get; set; }
    }
}
