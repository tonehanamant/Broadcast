using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class SwapProposalDetailRequest
    {
        public List<int> AffidavitScrubbingIds { get; set; }
        public int ProposalDetailId { get; set; }
    }
}
