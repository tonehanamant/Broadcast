using System;

namespace Services.Broadcast.Entities
{
    public class SpotTrackerFileDetail : TrackerFileDetail
    {        
        public string Client { get; set; }
        public string ClientName { get; set; }
        public string ReleaseName { get; set; }
        public string Country { get; set; }
        public int? MarketCode { get; set; }
        public string StationName { get; set; }
        public string DayOfWeek { get; set; }
        public string Daypart { get; set; }
        public DateTime? EncodeDate { get; set; }
        public int? EncodeTime { get; set; }
        public string RelType { get; set; }
        public int? Identifier2 { get; set; }
        public int? Identifier3 { get; set; }
        public int? Sid { get; set; }
        public int? Discid { get; set; }


        public bool Equals(SpotTrackerFileDetail other)
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
