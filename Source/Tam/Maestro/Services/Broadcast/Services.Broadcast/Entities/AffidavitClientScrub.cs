using System;

namespace Services.Broadcast.Entities
{
    public class AffidavitClientScrub
    {
        public int Id { get; set; }
        public long AffidavitFileDetailId { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }
        public bool MatchProgram { get; set; }
        public bool MatchGenre { get; set; }
        public bool MatchMarket { get; set; }
        public bool MatchTime { get; set; }
        public AffidavitClientScrubStatus Status { get; set; }
        public string Comment { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool LeadIn { get; set; }
    }
}
