using System;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class ManifestDetailDaypart : IEquatable<ManifestDetailDaypart>
    {
        public int Id { get; set; }
        public string LegacyCallLetters { get; set; }
        public DisplayDaypart DisplayDaypart { get; set; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(ManifestDetailDaypart other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && LegacyCallLetters == other.LegacyCallLetters && Equals(DisplayDaypart, other.DisplayDaypart);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ LegacyCallLetters.GetHashCode();
                hashCode = (hashCode * 397) ^ (DisplayDaypart != null ? DisplayDaypart.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Id {0}, call letters {1}, Daypart {2} : {3} - {4}", Id, LegacyCallLetters, DisplayDaypart,DisplayDaypart.StartTime, DisplayDaypart.EndTime);
        }
    }
}