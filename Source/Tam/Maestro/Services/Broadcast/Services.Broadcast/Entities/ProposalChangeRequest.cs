using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalChangeRequest
    {
        public int? Id { get; set; }
        public List<ProposalDetailDto> Details { get; set; } = new List<ProposalDetailDto>();
    }
}
