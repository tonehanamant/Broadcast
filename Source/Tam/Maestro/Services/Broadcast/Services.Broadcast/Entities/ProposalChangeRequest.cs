using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class ProposalChangeRequest
    {
        public int? Id { get; set; }
        public List<ProposalDetailDto> Details { get; set; }

        public ProposalChangeRequest()
        {
            Details = new List<ProposalDetailDto>();
        }
    }
}
