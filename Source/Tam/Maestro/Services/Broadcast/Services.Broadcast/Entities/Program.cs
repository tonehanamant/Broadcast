using System;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class Program : IEquatable<Program>
    {
        public readonly short StationCode;
        public readonly DisplayDaypart DisplayDaypart;

        public Program(short stationCode, DisplayDaypart daypart)
        {
            StationCode = stationCode;
            DisplayDaypart = daypart;
        }

        public override string ToString()
        {
            return "Station: " + StationCode + " " + DisplayDaypart;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Program other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return StationCode == other.StationCode && Equals(DisplayDaypart, other.DisplayDaypart);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((StationCode != null ? StationCode.GetHashCode() : 0) * 397) ^ (DisplayDaypart != null ? DisplayDaypart.GetHashCode() : 0);
            }
        }
    }
}