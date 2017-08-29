using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Entities
{
    public class StationProgramFlightWeekAudience
    {
        public DisplayAudience Audience { get; set; }
        public double? Impressions { get; set; }
        public double? Rating { get; set; }

        public decimal? Cpm15 { get; set; }
        public decimal? Cpm30 { get; set; }
        public decimal? Cpm60 { get; set; }
        public decimal? Cpm90 { get; set; }
        public decimal? Cpm120 { get; set; }

        public class AudienceImpressionsRatingEqualityComparer : IEqualityComparer<StationProgramFlightWeekAudience>
        {
            public bool Equals(StationProgramFlightWeekAudience x, StationProgramFlightWeekAudience y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Audience.Id.Equals(y.Audience.Id) && x.Impressions.Equals(y.Impressions) &&
                       x.Rating.Equals(y.Rating) && x.Cpm15.Equals(y.Cpm15) && x.Cpm30.Equals(y.Cpm30) &&
                       x.Cpm60.Equals(y.Cpm60) && x.Cpm90.Equals(y.Cpm90) && x.Cpm120.Equals(y.Cpm120);
            }

            public int GetHashCode(StationProgramFlightWeekAudience obj)
            {
                unchecked
                {
                    int hashCode = (obj.Audience != null ? obj.Audience.Id.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ obj.Impressions.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Rating.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Cpm15.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Cpm30.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Cpm60.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Cpm90.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Cpm120.GetHashCode();
                    return hashCode;
                }
            }
        }

        private static readonly IEqualityComparer<StationProgramFlightWeekAudience> AudienceImpressionsRatingComparerInstance = new AudienceImpressionsRatingEqualityComparer();

        public static IEqualityComparer<StationProgramFlightWeekAudience> AudienceImpressionsRatingComparer
        {
            get { return AudienceImpressionsRatingComparerInstance; }
        }
    }

}
