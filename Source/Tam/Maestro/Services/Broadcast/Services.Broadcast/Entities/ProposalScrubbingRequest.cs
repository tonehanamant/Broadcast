using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class ProposalScrubbingRequest
    {
        public ScrubbingStatus? ScrubbingStatusFilter { get; set; }
    }
}
