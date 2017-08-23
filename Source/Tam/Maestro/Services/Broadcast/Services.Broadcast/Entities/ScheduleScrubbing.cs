using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ScheduleScrubbing
    {
        public int EstimateId { get; set; }
        public int PostingBookId { get; set; }
        public List<int> OfficiallyOutOfSpecIds { get; set; }
    }
}
