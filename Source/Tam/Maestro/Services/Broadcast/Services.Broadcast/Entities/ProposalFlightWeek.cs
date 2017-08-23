using System;

namespace Services.Broadcast.Entities
{
    public class ProposalFlightWeek
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsHiatus { get; set; }
        public int MediaWeekId { get; set; }
    }
}