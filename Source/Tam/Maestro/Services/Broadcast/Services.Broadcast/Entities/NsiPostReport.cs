using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class NsiPostReport
    {
        public bool Equivalized { get; set; }
        public bool WithOvernightImpressions { get; set; } = false;
        public List<NsiPostReportQuarterSummaryTable> QuarterTables { get; set; } = new List<NsiPostReportQuarterSummaryTable>();
        public List<NsiPostReportQuarterTab> QuarterTabs { get; set; } = new List<NsiPostReportQuarterTab>();
        public List<LookupDto> ProposalAudiences { get; set; }
        public int ProposalId { get; set; }
        public string Advertiser { get; set; }
        public string GuaranteedDemo { get; set; }
        public string Daypart { get; set; }
        public List<string> FlightDates { get; set; }
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
            public Dictionary<int, double> AudienceImpressions { get; set; } = new Dictionary<int, double>();
            public decimal ProposalWeekTotalCost { get; set; }
            public decimal ProposalWeekCost { get; set; }
            public decimal ProposalWeekCPM { get; set; }
            public double ProposalWeekTotalImpressionsGoal { get; set; }
            public double ProposalWeekImpressionsGoal { get; set; }
            public int ProposalWeekUnits { get; set; }
            public int ProposalWeekId { get; set; }
        }

        public class NsiPostReportQuarterSummaryTableRow
        {
            public string Contract { get; set; }
            public DateTime WeekStartDate { get; set; }
            public int Spots { get; set; }
            public int SpotLength { get; set; }
            public decimal ProposalWeekTotalCost { get; set; }
            public decimal ProposalWeekCost { get; set; }
            public decimal ProposalWeekCPM { get; set; }
            public int HHRating { get; set; }
            public double ProposalWeekTotalImpressionsGoal { get; set; }
            public double ProposalWeekImpressionsGoal { get; set; }
            public double ActualImpressions { get; set; }
            public double DeliveredImpressionsPercentage { get; set; }
        }

        public NsiPostReport(int proposalId, List<InSpecAffidavitFileDetail> inSpecAffidavitFileDetails,
                            LookupDto advertiser, List<LookupDto> proposalAudiences,
                            Dictionary<int, List<int>> audienceMappings,
                            Dictionary<int, int> spotLengthMappings,
                            Dictionary<int, double> spotLengthMultipliers,
                            Dictionary<DateTime, MediaWeek> mediaWeekMappings,
                            Dictionary<string, DisplayBroadcastStation> stationMappings,
                            Dictionary<int, Dictionary<int, int>> nsiMarketRankings, string guaranteedDemo, int guaranteedDemoId,
                            List<Tuple<DateTime, DateTime>> flights, bool withOvernightImpressions, bool equivalized)
        {
            Advertiser = advertiser.Display;
            ProposalId = proposalId;
            WithOvernightImpressions = withOvernightImpressions;
            Equivalized = equivalized;
            GuaranteedDemo = guaranteedDemo;
            ProposalAudiences = proposalAudiences;

            var quartersGroup = inSpecAffidavitFileDetails.GroupBy(d => new { d.Year, d.Quarter }).OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Quarter);

            foreach (var group in quartersGroup)
            {
                var tab = new NsiPostReportQuarterTab()
                {
                    TabName = String.Format("Spot Detail {0}Q{1}", group.Key.Quarter, group.Key.Year.ToString().Substring(2)),
                    Title = String.Format("{0} {1}Q{2} Post Spot Detail", advertiser, group.Key.Quarter, group.Key.Year.ToString().Substring(2)),
                    TabRows = group.Select(r =>
                    {
                        var audienceImpressions = ProposalAudiences
                        .ToDictionary(proposalAudience => proposalAudience.Id, proposalAudience => r.AudienceImpressions
                            .Where(i => audienceMappings.Where(m => m.Key == proposalAudience.Id).SelectMany(m => m.Value).Contains(i.Key))
                            .Select(i => i.Value).Sum());
                        if (WithOvernightImpressions)
                        {
                            _ApplyOvernightImpressions(audienceImpressions, r.OvernightImpressions);
                        }
                        if (Equivalized)
                        {
                            _EquivalizeImpressions(spotLengthMultipliers[spotLengthMappings[r.SpotLengthId]], ref audienceImpressions);
                        }
                        return new NsiPostReportQuarterTabRow()
                        {
                            Rank = stationMappings.TryGetValue(r.Station, out DisplayBroadcastStation currentStation) && r.ProposalDetailPostingBookId.HasValue ? nsiMarketRankings[r.ProposalDetailPostingBookId.Value][currentStation.MarketCode] : 0,
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
                            ProposalWeekTotalCost = r.ProposalWeekTotalCost,
                            ProposalWeekCost = r.ProposalWeekCost,
                            ProposalWeekTotalImpressionsGoal = r.ProposalWeekTotalImpressionsGoal,
                            ProposalWeekImpressionsGoal = r.ProposalWeekImpressionsGoal,
                            ProposalWeekUnits = r.Units,
                            ProposalWeekCPM = r.ProposalWeekCPM,
                            ProposalWeekId = r.ProposalWeekId
                        };
                    }).ToList()
                };

                QuarterTabs.Add(tab);

                QuarterTables.Add(
                    new NsiPostReportQuarterSummaryTable()
                    {
                        TableName = String.Format("{0}Q'{1}", group.Key.Quarter, group.Key.Year.ToString().Substring(2)),
                        TableRows = tab.TabRows.GroupBy(x => new
                        {
                            x.DaypartName,
                            x.SpotLength,
                            x.WeekStart
                        }).OrderBy(x => x.Key.WeekStart).ThenBy(x => x.Key.SpotLength).Select(x =>
                             {
                                 var items = x.ToList();
                                 var row = new NsiPostReportQuarterSummaryTableRow
                                 {
                                     Contract = x.Key.DaypartName,
                                     SpotLength = x.Key.SpotLength,
                                     WeekStartDate = x.Key.WeekStart,
                                     Spots = items.GroupBy(y => new { y.ProposalWeekId, y.ProposalWeekUnits }).Select(y => y.Key.ProposalWeekUnits).Sum(),
                                     ActualImpressions = items.Select(y => y.AudienceImpressions.Where(w => w.Key == guaranteedDemoId).Sum(w => w.Value)).Sum(),
                                     ProposalWeekTotalCost = items.GroupBy(y => new { y.ProposalWeekId, y.ProposalWeekTotalCost }).Select(y => y.Key.ProposalWeekTotalCost).Sum(),
                                     ProposalWeekTotalImpressionsGoal = items.GroupBy(y => new { y.ProposalWeekId, y.ProposalWeekTotalImpressionsGoal }).Select(y => y.Key.ProposalWeekTotalImpressionsGoal).Sum()
                                 };
                                 row.DeliveredImpressionsPercentage = row.ActualImpressions / row.ProposalWeekTotalImpressionsGoal;
                                 row.ProposalWeekCost = row.ProposalWeekTotalCost / row.Spots;
                                 row.ProposalWeekImpressionsGoal = row.ProposalWeekTotalImpressionsGoal / row.Spots;
                                 row.ProposalWeekCPM = row.ProposalWeekCost / (decimal)row.ProposalWeekImpressionsGoal * 1000;
                                 return row;
                             }).ToList()
                    });
            }
            FlightDates = _GetFormattedFlights(flights, QuarterTables, Equivalized);
            SpotLengthsDisplay = string.Join(" & ", QuarterTabs.SelectMany(x => x.TabRows.Select(y => y.SpotLength)).Distinct().OrderBy(x => x).Select(x => $":{x}s").ToList());
            if (Equivalized)
            {
                SpotLengthsDisplay += " (Equivalized)";
            }
        }

        private void _EquivalizeImpressions(double spotLengthMutiplier, ref Dictionary<int, double> audienceImpressions)
        {
            foreach (var key in audienceImpressions.Keys.ToArray())
            {
                audienceImpressions[key] = audienceImpressions[key] * spotLengthMutiplier;
            }
        }

        private void _ApplyOvernightImpressions(Dictionary<int, double> audienceImpressions, Dictionary<int, double> overnightImpressions)
        {
            if (audienceImpressions.Any())
            {
                foreach (var audienceKey in overnightImpressions.Keys)
                {
                    if (audienceImpressions.Keys.Contains(audienceKey))
                    {
                        audienceImpressions[audienceKey] = overnightImpressions[audienceKey];
                    }
                }
            }
            else
            {
                audienceImpressions = overnightImpressions;
            }
        }

        private List<string> _GetFormattedFlights(List<Tuple<DateTime, DateTime>> flightDates, List<NsiPostReportQuarterSummaryTable> quarterTables, bool isEquivalized)
        {
            if (isEquivalized)
            {
                List<string> flights = new List<string>();
                if (quarterTables.Count() > 1)
                {
                    flights.Add($@"{quarterTables.Select(x => x.TableName).First()}-{quarterTables.Select(x => x.TableName).Last()} - {quarterTables.SelectMany(x => x.TableRows.Select(y => y.WeekStartDate)).Distinct().Count()} weeks");
                }
                quarterTables.ForEach(x =>
                {
                    var distinctWeeks = x.TableRows.Select(y => y.WeekStartDate.ToString(@"M\/d")).Distinct().ToList();
                    flights.Add($@"{x.TableName}: {distinctWeeks.Count()} {(distinctWeeks.Count() > 1 ? "weeks" : "week")} - {string.Join(", ", distinctWeeks)}");
                });
                return flights;
            }
            else
            {
                return new List<string>()
                {
                    string.Join(" & ", flightDates.Select(x => $"{x.Item1.ToString(@"M\/d\/yyyy")}-{x.Item2.ToString(@"M\/d\/yyyy")}").ToList())
                };
            }
        }
    }
}
