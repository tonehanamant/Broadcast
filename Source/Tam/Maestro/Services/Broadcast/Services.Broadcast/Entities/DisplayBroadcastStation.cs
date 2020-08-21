using System;

namespace Services.Broadcast.Entities
{
    public class DisplayBroadcastStation
    {
        public enum StationFilter
        {
            WithTodaysData,
            WithoutTodaysData
        }

        public int Id { get; set; }
        public int? Code { get; set; }
        public string CallLetters { get; set; }
        public string LegacyCallLetters { get; set; }
        public string OriginMarket { get; set; }
        public int? MarketCode { get; set; }
        public string Affiliation { get; set; }
        public string RateDataThrough { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? ManifestMaxEndDate { get; set; }
        public string OwnershipGroupName { get; set; }
        public string SalesGroupName { get; set; }
        public bool IsTrueInd { get; set; }

        public override string ToString()
        {
            return string.Format("lcl={0};mc={1}; DisplayBroadcastStation", LegacyCallLetters, MarketCode);
        }
    }
}
