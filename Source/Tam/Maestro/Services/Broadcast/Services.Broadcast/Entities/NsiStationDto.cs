using System;

namespace Services.Broadcast.Entities
{
    public class NsiStationDto
    {
        public int MediaMonthId { get; set; }
        public int DistributorCode { get; set; }
        public int MarketCode { get; set; }
        public int MarketOfOriginCode { get; set; }
        public string CallLetters { get; set; }
        public string LegacyCallLetters { get; set; }
        public DateTime StartDatetimeOfSurvey { get; set; }
        public DateTime EndDatetimeOfSurvey { get; set; }
        public bool ParentPlusIndicator { get; set; }
        public string CableLongName { get; set; }
        public string BroadcastChannelNumber { get; set; }
        public string DistributionSourceType { get; set; }
        public string PrimaryAffiliation { get; set; }
        public string SecondaryAffiliation { get; set; }
        public string TertiaryAffiliation { get; set; }
        public string DistributorGroup { get; set; }
        public bool ParentIndicator { get; set; }
        public bool SatelliteIndicatior { get; set; }
        public string StationTypeCode { get; set; }
        public string ReportabilityStatus { get; set; }
    }
}
