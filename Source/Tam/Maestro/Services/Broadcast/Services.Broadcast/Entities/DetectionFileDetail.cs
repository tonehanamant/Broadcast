using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class DetectionFileDetail : TrackerFileDetail
    {        
        public int? ScheduleDetailWeekId { get; set; }
        public bool MatchStation { get; set; }
        public bool MatchProgram { get; set; }
        public bool MatchAirtime { get; set; }
        public bool MatchIsci { get; set; }
        public int Status { get; set; }
        public DateTime NsiDate { get; set; }
        public DateTime NtiDate { get; set; }
        public bool HasLeadInScheduleMatches { get; set; }
        public bool LinkedToBlock { get; set; }
        public bool LinkedToLeadin { get; set; }
        public bool MatchSpotLength { get; set; }
        
        public List<DetectionPostDetail> DetectionPostDetails { get; set; } = new List<DetectionPostDetail>();
        
        public bool Equals(DetectionFileDetail other)
        {
            return string.Equals(Advertiser, other.Advertiser)
                && DateAired.Equals(other.DateAired)
                && EstimateId == other.EstimateId
                && string.Equals(Isci, other.Isci)
                && Station == other.Station
                && TimeAired == other.TimeAired;
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Advertiser != null ? Advertiser.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DateAired.GetHashCode();
                hashCode = (hashCode * 397) ^ EstimateId;
                hashCode = (hashCode * 397) ^ (Isci != null ? Isci.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Station.GetHashCode();
                hashCode = (hashCode * 397) ^ TimeAired;
                return hashCode;
            }
        }
    }
}
