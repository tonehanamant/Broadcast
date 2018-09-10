using System;

namespace Services.Broadcast.Entities
{
    public struct TrackerFileDetailKey<T> : IEquatable<T> where T : TrackerFileDetail
    {
        private string Advertiser;
        private DateTime DateAired;
        private readonly int EstimateId;
        private string Isci;
        private int? Station;
        private string station;
        private readonly int TimeAired;

        public TrackerFileDetailKey(T file)
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

        public bool Equals(T other)
        {
            throw new NotImplementedException();
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