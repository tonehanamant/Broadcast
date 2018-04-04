using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class NsiPostReport
    {
        public List<NsiPostReportQuarterTab> QuarterTabs { get; set; }
        public List<LookupDto> ProposalAudiences { get; set; }
        public int ProposalId { get; set; }
        public string Advertiser { get; set; }
        public string GuaranteedDemo { get; set; }
        public string Daypart { get; set; }
        public List<Tuple<DateTime?, DateTime?>> FlightDates { get; set; }

        public class NsiPostReportQuarterTab
        {
            public string TabName { get; set; }
            public string Title { get; set; }
            public List<NsiPostReportQuarterTabRow> TabRows {get; set;}
        }

        public class NsiPostReportQuarterTabRow
        {
            public NsiPostReportQuarterTabRow()
            {
                AudienceImpressions = new Dictionary<int, double>();
            }

            public int Rank { get; set; }
            public string Market { get; set; }
            public string Station { get; set; }
            public string NetworkAffiliate { get; set; }
            public DateTime WeekStart { get; set; }

            public int TimeAired { get; set; }
            public DateTime DateAired { get; set; }
            public string ProgramName { get; set; }
            public int SpotLength { get; set; }
            public string Isci { get; set; }
            public string Advertiser { get; set; }
            public string DaypartName { get; set; }
            public Dictionary<int, double> AudienceImpressions { get; set; }
        }

        public NsiPostReport(int proposalId, List<InSpecAffidavitFileDetail> inSpecAffidavitFileDetails, 
                            LookupDto advertiser, List<LookupDto> proposalAudiences,
                            Dictionary<int, List<int>> audienceMappings, 
                            Dictionary<int, int> spotLengthMappings,
                            Dictionary<DateTime, MediaWeek> mediaWeekMappings,
                            Dictionary<string, DisplayBroadcastStation> stationMappings,
                            Dictionary<int, int> nsiMarketRankings, string guaranteedDemo,
                            List<Tuple<DateTime?, DateTime?>> flightDates)
        {
            ProposalId = proposalId;
            ProposalAudiences = proposalAudiences;
            QuarterTabs = new List<NsiPostReportQuarterTab>();
            Advertiser = advertiser.Display;
            GuaranteedDemo = guaranteedDemo;
            FlightDates = flightDates;

            var quarters = inSpecAffidavitFileDetails.GroupBy(d => new { d.Year, d.Quarter })
                .Select(d => new NsiPostReportQuarterTab()
                {
                    TabName = String.Format("Spot Detail {0}Q{1}", d.Key.Quarter, d.Key.Year.ToString().Substring(2)),
                    Title = String.Format("{0} {1}Q{2} Post Spot Detail", advertiser, d.Key.Quarter, d.Key.Year.ToString().Substring(2)),
                    TabRows = d.Select(r =>
                    {

                        var audienceImpressions = proposalAudiences
                        .ToDictionary(proposalAudience => proposalAudience.Id, proposalAudience => r.AudienceImpressions
                            .Where(i => audienceMappings.Where(m => m.Key == proposalAudience.Id).SelectMany(m => m.Value).Contains(i.Key))
                            .Select(i => i.Value).Sum());
                        return new NsiPostReportQuarterTabRow()
                        {
                            Rank = stationMappings.TryGetValue(r.Station, out DisplayBroadcastStation currentStation) ? nsiMarketRankings[currentStation.MarketCode] : 0,
                            Market = stationMappings.TryGetValue(r.Station, out currentStation) ? currentStation.OriginMarket : "",
                            Station = r.Station,
                            NetworkAffiliate = stationMappings.TryGetValue(r.Station, out currentStation) ? currentStation.Affiliation : "",
                            WeekStart = mediaWeekMappings[r.AirDate].StartDate,
                            ProgramName = r.ProgramName,
                            Isci = r.Isci,
                            TimeAired = r.AirTime,
                            DateAired = r.AirDate,
                            SpotLength = spotLengthMappings[r.SpotLengthId],
                            Advertiser = advertiser.Display,
                            DaypartName = r.DaypartName,
                            AudienceImpressions = audienceImpressions
                        };
                    }).ToList()

                }).ToList();
            QuarterTabs.AddRange(quarters);
        }
    }
}
