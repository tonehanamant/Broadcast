using System;

namespace Services.Broadcast.Entities
{
    public class BvsPostDetail
    {
        public int BvsDetailId { get; set; }
        public string Station { get; set; }
        public DateTime NsiDate { get; set; }
        public int TimeAired { get; set; }
    }
}
