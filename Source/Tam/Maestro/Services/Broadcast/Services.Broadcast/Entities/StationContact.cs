using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class StationContact
    {

        public enum StationContactType
        {
            Station = 1,
            Rep = 2,
            Traffic = 3
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public StationContactType Type { get; set; }
        public int StationCode { get; set; }
        public int StationId { get; set; }
        public string InventorySourceString { get; set; }
        public DateTime ModifiedDate { get; set; }

        private sealed class StationContactEqualityComparer : IEqualityComparer<StationContact>
        {
            public bool Equals(StationContact x, StationContact y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null) return false;
                if (y is null) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase) 
                    && string.Equals(x.Company, y.Company, StringComparison.InvariantCultureIgnoreCase) && x.Type == y.Type && x.StationCode == y.StationCode;
            }

            public int GetHashCode(StationContact obj)
            {
                unchecked
                {
                    int hashCode = (obj.Name != null ? obj.Name.ToUpperInvariant().GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Company != null ? obj.Company.ToUpperInvariant().GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int) obj.Type;
                    hashCode = (hashCode * 397) ^ obj.StationCode;
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<StationContact> StationContactComparer { get; } = new StationContactEqualityComparer();
    }
}
