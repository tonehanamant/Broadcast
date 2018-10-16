using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class ProposalDetailPostingData
    {
        public int ProposalVersionDetailId { get; set; }
        public int PostingBookId { get; set; }
        public ProposalEnums.ProposalPlaybackType? PostingPlaybackType { get; set; }
    }
}
