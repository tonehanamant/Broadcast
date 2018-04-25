using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class NsiPostReport
    {
        public List<NsiPostReportQuarterSummaryTable> QuarterTables { get; set; } = new List<NsiPostReportQuarterSummaryTable>();
        public List<NsiPostReportQuarterTab> QuarterTabs { get; set; } = new List<NsiPostReportQuarterTab>();
        public List<LookupDto> ProposalAudiences { get; set; }
        public int ProposalId { get; set; }
        public string Advertiser { get; set; }
        public string GuaranteedDemo { get; set; }
        public string Daypart { get; set; }
        public List<Tuple<DateTime, DateTime>> FlightDates { get; set; }
        public string SpotLengthsDisplay { get; set; }

        public class NsiPostReportQuarterTab
        {
            public string TabName { get; set; }
            public string Title { get; set; }
            public List<NsiPostReportQuarterTabRow> TabRows { get; set; }
        }

        public class NsiPostReportQuarterSummaryTable
        {
            public string TableName { get; set; }
            public List<NsiPostReportQuarterSummaryTableRow> TableRows { get; set; }
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
            public decimal ProposalWeekCost { get; set; }
            public double ProposalWeekImpressionsGoal { get; set; }
            public int ProposalWeekUnits { get; set; }
        }

        public class NsiPostReportQuarterSummaryTableRow
        {
            public string Contract { get; set; }
            public DateTime WeekStartDate { get; set; }
            public int Spots { get; set; }
            public int SpotLength { get; set; }
            public decimal ProposalWeekCost { get; set; }
            public int HHRating { get; set; }
            public double ProposalWeekImpressionsGoal { get; set; }
            public double ActualImpressions { get; set; }
        }

        public NsiPostReport(int proposalId, List<InSpecAffidavitFileDetail> inSpecAffidavitFileDetails,
                            LookupDto advertiser, List<LookupDto> proposalAudiences,
                            Dictionary<int, List<int>> audienceMappings,
                            Dictionary<int, int> spotLengthMappings,
                            Dictionary<DateTime, MediaWeek> mediaWeekMappings,
                            Dictionary<string, DisplayBroadcastStation> stationMappings,
                            Dictionary<int, int> nsiMarketRankings, string guaranteedDemo, int guaranteedDemoId,
                            List<Tuple<DateTime, DateTime>> flightDates)
        {
            ProposalId = proposalId;
            ProposalAudiences = proposalAudiences;
            Advertiser = advertiser.Display;
            GuaranteedDemo = guaranteedDemo;
            FlightDates = flightDates;

            var quartersGroup = inSpecAffidavitFileDetails.GroupBy(d => new { d.Year, d.Quarter });

            foreach (var group in quartersGroup)
            {
                var tab = new NsiPostReportQuarterTab()
                {
                    TabName = String.Format("Spot Detail {0}Q{1}", group.Key.Quarter, group.Key.Year.ToString().Substring(2)),
                    Title = String.Format("{0} {1}Q{2} Post Spot Detail", advertiser, group.Key.Quarter, group.Key.Year.ToString().Substring(2)),
                    TabRows = group.Select(r =>
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
                            AudienceImpressions = audienceImpressions,
                            ProposalWeekCost = r.ProposalWeekCost,
                            ProposalWeekImpressionsGoal = r.ProposalWeekImpressionsGoal,
                            ProposalWeekUnits = r.Units
                        };
                    }).ToList()
                };

                QuarterTabs.Add(tab);

                var impressionsByDaypart = _GetImpressionsByDaypart(guaranteedDemoId, tab);

                QuarterTables.Add(
                    new NsiPostReportQuarterSummaryTable()
                    {
                        TableName = String.Format("{0}Q'{1}", group.Key.Quarter, group.Key.Year.ToString().Substring(2)),
                        TableRows = tab.TabRows.GroupBy(x => new
                        {
                            x.DaypartName,
                            x.SpotLength,
                            x.WeekStart,
                            x.ProposalWeekUnits,
                            x.ProposalWeekImpressionsGoal,
                            x.ProposalWeekCost
                        })
                            .Select(x =>
                            {
                                return new NsiPostReportQuarterSummaryTableRow
                                {
                                    Contract = x.Key.DaypartName,
                                    SpotLength = x.Key.SpotLength,
                                    WeekStartDate = x.Key.WeekStart,
                                    Spots = x.Key.ProposalWeekUnits,
                                    ActualImpressions = impressionsByDaypart.ContainsKey(x.Key.DaypartName) ? impressionsByDaypart[x.Key.DaypartName] : 0,
                                    ProposalWeekCost = x.Key.ProposalWeekCost,
                                    ProposalWeekImpressionsGoal = x.Key.ProposalWeekImpressionsGoal
                                };
                            }).ToList()
                    });
            }

            SpotLengthsDisplay = string.Join(",", QuarterTabs.SelectMany(x => x.TabRows.Select(y => y.SpotLength)).Distinct().OrderBy(x => x).Select(x => $":{x}s").ToList());
        }

        private Dictionary<string, double> _GetImpressionsByDaypart(int guaranteedDemoId, NsiPostReportQuarterTab tab)
        {
            var impressionsByDaypart = new Dictionary<string, double>();
            var tabRowsGroupedByDaypart = tab.TabRows.GroupBy(x => x.DaypartName);

            foreach (var tabRow in tabRowsGroupedByDaypart)
            {
                var impressionsSum = tabRow.Sum(x => x.AudienceImpressions.ContainsKey(guaranteedDemoId) ? x.AudienceImpressions[guaranteedDemoId] : 0);

                impressionsByDaypart.Add(tabRow.Key, impressionsSum);
            }

            return impressionsByDaypart;
        }
    }
}
