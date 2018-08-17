using System;

namespace Services.Broadcast.Entities
{
    public class BvsPostDetail
    {
        public int BvsFileDetailId { get; set; }
        public int AudienceId { get; set; }
        public double Delivery { get; set; }
        public int? AudienceRank { get; set; }
        public string Station { get; set; }
        public DateTime NsiDate { get; set; }
        public int TimeAired { get; set; }
    }
}
