using System;

namespace Services.Broadcast.Entities
{
    public enum TrackingStatus
    {
        UnTracked = 0,
        InSpec = 1,
        OutOfSpec = 2,
        OfficialOutOfSpec = 3
    }

    public class DetectionTrackingDetail
    {
        public int Id { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string Affiliate { get; set; }
        public int SpotLength { get; set; }
        public int? SpotLengthId { get; set; }
        public string Program { get; set; }
        public string Isci { get; set; }
        public DateTime NsiDate { get; set; }
        public int AirTime { get; set; }
        public double? Impressions { get; set; }
        public decimal? Cost { get; set; } //only populated if there's a match
        public int EstimateId { get; set; }

        public int? ScheduleDetailWeekId { get; set; }
        public TrackingStatus Status { get; set; }
        public bool MatchStation { get; set; }
        public bool MatchProgram { get; set; }
        public bool MatchAirtime { get; set; }
        public bool MatchIsci { get; set; }
        public bool MatchSpotLength { get; set; }

        public bool LinkedToBlock { get; set; }
        public bool LinkedToLeadin { get; set; }
        public bool HasLeadInScheduleMatches { get; set; }

        public DateTime RunTime { get { return NsiDate.AddSeconds(AirTime); } }

        public bool NeedProgramMatch
        {
            get { return !MatchProgram && !LinkedToBlock && !LinkedToBlock; }
        }
    }
}
