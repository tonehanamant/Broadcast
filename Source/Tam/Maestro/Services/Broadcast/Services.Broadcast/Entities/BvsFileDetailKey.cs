using EntityFrameworkMapping.Broadcast;
using System;

namespace Services.Broadcast.Entities
{
    public struct BvsFileDetailKey : IEquatable<BvsFileDetailKey>
    {
        private string advertiser;
        private DateTime date_aired;
        private int estimate_id;
        private string isci;
        private int? spot_length_id;
        private string station;
        private int time_aired;

        public BvsFileDetailKey(bvs_file_details file)
        {
            station = file.station;
            date_aired = file.date_aired;
            time_aired = file.time_aired;
            isci = file.isci;
            spot_length_id = file.spot_length_id;
            estimate_id = file.estimate_id;
            advertiser = file.advertiser;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(BvsFileDetailKey other)
        {
            return string.Equals(advertiser, other.advertiser) && date_aired.Equals(other.date_aired) && estimate_id == other.estimate_id && string.Equals(isci, other.isci) && spot_length_id == other.spot_length_id && string.Equals(station, other.station) && time_aired == other.time_aired;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (advertiser != null ? advertiser.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ date_aired.GetHashCode();
                hashCode = (hashCode * 397) ^ estimate_id;
                hashCode = (hashCode * 397) ^ (isci != null ? isci.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ spot_length_id.GetHashCode();
                hashCode = (hashCode * 397) ^ (station != null ? station.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ time_aired;
                return hashCode;
            }
        }
    }
}