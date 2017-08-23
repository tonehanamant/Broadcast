using System;

namespace Services.Broadcast.Entities
{
    public class ProposalWeekDto
    {
        public int? Id { get; set; }
        public int MediaWeekId { get; set; }
        public string Week { get; set; }
        public bool IsHiatus { get; set; }
        public int Units { get; set; }
        public double Impressions { get; set; }
        public decimal Cost { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
    }
}
