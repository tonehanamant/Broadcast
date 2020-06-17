using System.Collections.Generic;

namespace Services.Broadcast.Entities.Vpvh
{
    public class VpvhRequest
    {
        public int StandardDaypartId { get; set; }

        public List<int> AudienceIds { get; set; }
    }
}
