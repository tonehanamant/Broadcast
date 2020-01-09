using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Plan
{
    public class VPVHRequest
    {
        public List<int> AudienceIds { get; set; }
        public int ShareBookId { get; set; }
        public int? HutBookId { get; set; }
    }
}
