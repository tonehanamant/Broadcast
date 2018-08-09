using System;

namespace Services.Broadcast.Entities
{
    public class AffidavitMatchingProposalWeek
    {
        public int ProposalVersionId { get; set; }
        public int ProposalVersionDetailId { get; set; }
        public int? ProposalVersionDetailPostingBookId { get; set; }
        public ProposalEnums.ProposalPlaybackType? ProposalVersionDetailPostingPlaybackType { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }
        public DateTime ProposalVersionDetailWeekStart { get; set; }
        public DateTime ProposalVersionDetailWeekEnd { get; set; }
        public int Spots { get; set; }
        public int ProposalVersionDetailDaypartId { get; set; }
        public string ClientIsci { get; set; }
        public string HouseIsci { get; set; }
        public string Brand { get; set; }
        public bool MarriedHouseIsci { get; set; }
        public bool IsLeadInMatch { get; set; }
        public bool DateMatch { get; set; }
        public bool TimeMatch { get; set; }
        public int SpotLengthId { get; set; }
    }
}
