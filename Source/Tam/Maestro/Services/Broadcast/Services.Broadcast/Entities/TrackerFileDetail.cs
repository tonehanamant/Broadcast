using System;

namespace Services.Broadcast.Entities
{
    public abstract class TrackerFileDetail
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string Affiliate { get; set; }
        public DateTime DateAired { get; set; }
        public int TimeAired { get; set; }
        public string ProgramName { get; set; }
        public int SpotLength { get; set; }
        public int? SpotLengthId { get; set; }
        public string Isci { get; set; }
        public int EstimateId { get; set; }
        public string Advertiser { get; set; }
    }
}
