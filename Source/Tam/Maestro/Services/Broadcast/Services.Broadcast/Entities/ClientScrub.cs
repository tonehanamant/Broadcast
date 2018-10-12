using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ClientScrub
    {
        public int Id { get; set; }
        public long ScrubbingFileDetailId { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }
        public int ProposalVersionDetailId { get; set; }
        public bool MatchProgram { get; set; }
        public bool MatchGenre { get; set; }
        public bool MatchShowType { get; set; }
        public bool MatchMarket { get; set; }
        public bool MatchStation { get; set; }
        public bool MatchTime { get; set; }
        public bool MatchIsciDays { get; set; }
        public ScrubbingStatus Status { get; set; }
        public string Comment { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool LeadIn { get; set; }
        public bool MatchDate { get; set; }
        public string EffectiveProgramName { get; set; }
        public string EffectiveGenre { get; set; }
        public string EffectiveShowType { get; set; }
        public int? PostingBookId { get; set; }
        public ProposalEnums.ProposalPlaybackType? PostingPlaybackType { get; set; }
        public List<ScrubbingFileAudiences> ClientScrubAudiences { get; set; } = new List<ScrubbingFileAudiences>();
        public string EffectiveIsci { get; set; }
        public bool MatchIsci { get; set; }
        public string EffectiveClientIsci { get; set; }
    }
}
