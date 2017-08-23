using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class StationProgramFlightWeek
    {
        public int Id { get; set; }
        public DisplayMediaWeek FlightWeek { get; set; }
        public Boolean Active { get; set; }
        public decimal? Rate15s { get; set; }
        public decimal? Rate30s { get; set; }
        public decimal? Rate60s { get; set; }
        public decimal? Rate90s { get; set; }
        public decimal? Rate120s { get; set; }
        public int? Spots { get; set; }

        private List<StationProgramFlightWeekAudience> _stationProgramFlightWeekAudiences;

        public List<StationProgramFlightWeekAudience> Audiences
        {
            get
            {
                if (_stationProgramFlightWeekAudiences == null)
                {
                    _stationProgramFlightWeekAudiences = new List<StationProgramFlightWeekAudience>();
                }
                return _stationProgramFlightWeekAudiences;
            }
            set { _stationProgramFlightWeekAudiences = value; }
        }

        public class StationProgramFlightWeekEqualityComparer : IEqualityComparer<StationProgramFlightWeek>
        {
            public bool Equals(StationProgramFlightWeek x, StationProgramFlightWeek y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                if (x.FlightWeek == null & y.FlightWeek != null) return false;
                if (y.FlightWeek == null && x.FlightWeek != null) return false;
                return x.Audiences.Count == y.Audiences.Count && !x.Audiences.Except(y.Audiences, new StationProgramFlightWeekAudience.AudienceImpressionsRatingEqualityComparer()).Any() //Equals(x._stationProgramFlightWeekAudiences, y._stationProgramFlightWeekAudiences) 
                    && ((x.FlightWeek == null && y.FlightWeek == null) || (x.FlightWeek.Id.Equals(y.FlightWeek.Id))) 
                    && x.Active.Equals(y.Active) 
                    && x.Rate15s.Equals(y.Rate15s) 
                    && x.Rate30s.Equals(y.Rate30s) 
                    && x.Rate60s.Equals(y.Rate60s)
                    && x.Rate90s.Equals(y.Rate90s)
                    && x.Rate120s.Equals(y.Rate120s);
            }

            public int GetHashCode(StationProgramFlightWeek obj)
            {
                unchecked
                {
                    int hashCode = (obj.FlightWeek != null ? obj.FlightWeek.Id.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ obj.Active.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Rate15s.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Rate30s.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Rate60s.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Rate90s.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Rate120s.GetHashCode();
                    return hashCode;
                }
            }
        }

        private static readonly IEqualityComparer<StationProgramFlightWeek> StationProgramFlightWeekComparerInstance = new StationProgramFlightWeekEqualityComparer();

        public static IEqualityComparer<StationProgramFlightWeek> StationProgramFlightWeekComparer
        {
            get { return StationProgramFlightWeekComparerInstance; }
        }
    }
}
