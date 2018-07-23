using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class BvsFileDetail
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string Affiliate { get; set; }
        public DateTime DateAired{ get; set; }
        public int TimeAired { get; set; }
        public string ProgramName { get; set; }
        public int SpotLength { get; set; }
        public int? SpotLengthId { get; set; }
        public string Isci { get; set; }
        public int EstimateId { get; set; }
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
        public string Advertiser { get; set; }

        public List<BvsPostDetail> BvsPostDetails { get; set; } = new List<BvsPostDetail>();


        public bool Equals(BvsFileDetail other)
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
