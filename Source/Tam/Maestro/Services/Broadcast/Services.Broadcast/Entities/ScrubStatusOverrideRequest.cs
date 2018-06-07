using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Services.Broadcast.Entities
{
    public class ScrubStatusOverrideRequest
    {
        public int ProposalId { get; set; }
        public List<int> ScrubIds { get; set; }
        public ScrubbingStatus OverrideStatus { get; set; }
        public ScrubbingStatus? ReturnStatusFilter { get; set; }
    }

}
