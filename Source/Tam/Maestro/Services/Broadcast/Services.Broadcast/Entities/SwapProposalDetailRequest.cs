using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class SwapProposalDetailRequest
    {
        public List<int> ScrubbingIds { get; set; }
        public int ProposalDetailId { get; set; }
    }
}
