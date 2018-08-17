using System;

namespace Services.Broadcast.Entities
{
    public struct BvsFileDetailKey : IEquatable<BvsFileDetailKey>
    {
        private string Advertiser;
        private DateTime DateAired;
        private int EstimateId;
        private string Isci;
        private int? Station;
        private string station;
        private int TimeAired;

        public BvsFileDetailKey(BvsFileDetail file)
        {
            station = file.Station;
            DateAired = file.DateAired;
            TimeAired = file.TimeAired;
            Isci = file.Isci;
            Station = file.SpotLengthId;
            EstimateId = file.EstimateId;
            Advertiser = file.Advertiser;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(BvsFileDetailKey other)
        {
            return string.Equals(Advertiser, other.Advertiser) 
                && DateAired.Equals(other.DateAired) 
                && EstimateId == other.EstimateId 
                && string.Equals(Isci, other.Isci) 
                && Station == other.Station 
                && string.Equals(station, other.station)
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
                hashCode = (hashCode * 397) ^ (station != null ? station.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TimeAired;
                return hashCode;
            }
        }
    }
}