using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class PostReport
    {
        public bool Equivalized { get; set; }
        public bool WithOvernightImpressions { get; set; } = false;
        public bool IsNtiReport { get; set; } = false;
        public List<PostReportQuarterSummaryTable> QuarterTables { get; set; } = new List<PostReportQuarterSummaryTable>();
        public List<NsiPostReportQuarterTab> QuarterTabs { get; set; } = new List<NsiPostReportQuarterTab>();
        public List<LookupDto> ProposalAudiences { get; set; }
        public string GuaranteedDemo { get; set; }
        public string Daypart { get; set; }
        public List<string> FlightDates { get; set; }
        public string SpotLengthsDisplay { get; set; }
        public string ReportName { get; set; }
        public string Advertiser { get; set; }
        public List<string> PostingBooks { get; set; }
        public List<string> PlaybackTypes { get; set; }
        public string ProposalNotes { get; set; }
        public string PostType { get; set; }

        private const string SUMMARY_TABLE_NAME_FORMAT = "{0}Q'{1}";
        private const string SPOT_DETAIL_TAB_NAME_FORMAT = "Spot Detail {0}Q{1}";
        private const string SPOT_DETAIL_TAB_TITLE_FORMAT = "{0} {1}Q{2} Post Spot Detail";

        public class NsiPostReportQuarterTab
        {
            public string TabName { get; set; }
            public string Title { get; set; }
            public List<NsiPostReportQuarterTabRow> TabRows { get; set; }
        }

        public class PostReportQuarterSummaryTable
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
            public string Brand { get; set; }
            public string ProposalDetailPostingBook { get; set; }
            public string ProposalDetailPlaybackType { get; set; }
            public string DaypartName { get; set; }
            public Dictionary<int, double> AudienceImpressions { get; set; } = new Dictionary<int, double>();
            public Dictionary<int, double> NtiImpressions { get; set; } = new Dictionary<int, double>();
            public decimal ProposalWeekTotalCost { get; set; }
            public decimal ProposalWeekCost { get; set; }
            public decimal ProposalWeekCPM { get; set; }
            public double ProposalWeekTotalImpressionsGoal { get; set; }
            public double ProposalWeekImpressionsGoal { get; set; }
            public int ProposalWeekUnits { get; set; }
            public int ProposalWeekId { get; set; }
            public int ProposalDetailSpotLength { get; set; }
            public bool Adu { get; set; }
        }

        public class NsiPostReportQuarterSummaryTableRow
        {
            public string Contract { get; set; }
            public DateTime WeekStartDate { get; set; }
            public int Spots { get; set; }
            public int SpotLength { get; set; }
            public decimal? ProposalWeekTotalCost { get; set; }
            public decimal? ProposalWeekCost { get; set; }
            public decimal? ProposalWeekCPM { get; set; }
            public int HHRating { get; set; }
            public double? ProposalWeekTotalImpressionsGoal { get; set; }
            public double? ProposalWeekImpressionsGoal { get; set; }
            public double? ActualImpressions { get; set; }
            public double? DeliveredImpressionsPercentage { get; set; }
            public bool Adu { get; set; }
        }

        public PostReport(int proposalId, List<InSpecAffidavitFileDetail> inSpecAffidavitFileDetails,
                            string advertiser, List<LookupDto> proposalAudiences,
                            Dictionary<int, List<int>> audienceMappings,
                            Dictionary<int, int> spotLengthMappings,
                            Dictionary<DateTime, MediaWeek> mediaWeekMappings,
                            Dictionary<string, DisplayBroadcastStation> stationMappings,
                            Dictionary<int, Dictionary<int, int>> nsiMarketRankings,
                            string guaranteedDemo,
                            List<Tuple<DateTime, DateTime>> flights,
                            bool withOvernightImpressions,
                            IImpressionAdjustmentEngine impressionAdjustmentEngine,
                            List<MediaMonth> mediaMonths,
                            List<ProposalEnums.ProposalPlaybackType> playbackTypes,
                            ProposalDto proposal,
                            bool isNtiReport)
        {
            WithOvernightImpressions = withOvernightImpressions;
            GuaranteedDemo = guaranteedDemo;
            ProposalAudiences = proposalAudiences;
            ReportName = _GenerateReportName(proposal.ProposalName, inSpecAffidavitFileDetails, advertiser, withOvernightImpressions, isNtiReport);
            Advertiser = advertiser;
            PostingBooks = mediaMonths.Select(x => x.LongMonthNameAndYear).ToList();
            PlaybackTypes = playbackTypes.Select(x => EnumHelper.GetDescriptionAttribute(x)).ToList();
            ProposalNotes = proposal.Notes;
            Equivalized = proposal.Equivalized;
            IsNtiReport = isNtiReport;
            PostType = proposal.PostType.ToString();

            //map the data
            var quartersGroup = inSpecAffidavitFileDetails.GroupBy(d => new { d.Year, d.Quarter }).OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Quarter);
            foreach (var group in quartersGroup)
            {
                var tab = new NsiPostReportQuarterTab()
                {
                    TabName = string.Format(SPOT_DETAIL_TAB_NAME_FORMAT, group.Key.Quarter, group.Key.Year.ToString().Substring(2)),
                    Title = string.Format(SPOT_DETAIL_TAB_TITLE_FORMAT, advertiser, group.Key.Quarter, group.Key.Year.ToString().Substring(2)),
                    TabRows = group.Select(r =>
                    {
                        var nsiImpressions = ProposalAudiences
                        .ToDictionary(proposalAudience => proposalAudience.Id, proposalAudience => r.NsiImpressions
                            .Where(i => audienceMappings.Where(m => m.Key == proposalAudience.Id).SelectMany(m => m.Value).Contains(i.Key))
                            .Select(i => i.Value).Sum());

                        var ntiImpressions = ProposalAudiences
                                        .ToDictionary(proposalAudience => proposalAudience.Id, proposalAudience => r.NtiImpressions
                                            .Where(i => audienceMappings.Where(m => m.Key == proposalAudience.Id).SelectMany(m => m.Value).Contains(i.Key))
                                            .Select(i => i.Value).Sum());
                        _ApplyOvernightImpressions(nsiImpressions, r.OvernightImpressions);

                        _EquivalizeImpressions(impressionAdjustmentEngine, spotLengthMappings.Single(x => x.Value == r.ProposalDetailSpotLengthId).Key, ref nsiImpressions);
                        _EquivalizeImpressions(impressionAdjustmentEngine, spotLengthMappings.Single(x => x.Value == r.ProposalDetailSpotLengthId).Key, ref ntiImpressions);

                        return _MapNsiPostReportQuarterTabRow(advertiser, spotLengthMappings, mediaWeekMappings, r, nsiImpressions,
                                         ntiImpressions, stationMappings, nsiMarketRankings, mediaMonths);
                    }).ToList()
                };

                QuarterTabs.Add(tab);

                QuarterTables.Add(
                    new PostReportQuarterSummaryTable()
                    {
                        TableName = String.Format(SUMMARY_TABLE_NAME_FORMAT, group.Key.Quarter, group.Key.Year.ToString().Substring(2)),
                        TableRows = _LoadPostReportQuarterSummaryTableRows(tab.TabRows, proposal.GuaranteedDemoId, isNtiReport)
                    });
            }

            _ApplyAduAdjustments(QuarterTables);
            FlightDates = _GetFormattedFlights(flights, QuarterTables);
            SpotLengthsDisplay = _GetFormattedSpotLengths(QuarterTables);
        }

        private string _GetFormattedSpotLengths(List<PostReportQuarterSummaryTable> quarterTables)
        {
            string result = string.Join(" & ", quarterTables.SelectMany(x => x.TableRows.Select(y => y.SpotLength)).Distinct().OrderBy(x => x).Select(x => $":{x}s").ToList());
            if (Equivalized)
            {
                result += " (Equivalized)";
            }
            return result;
        }

        private static NsiPostReportQuarterTabRow _MapNsiPostReportQuarterTabRow(string advertiser,
                                    Dictionary<int, int> spotLengthMappings,
                                    Dictionary<DateTime, MediaWeek> mediaWeekMappings,
                                    InSpecAffidavitFileDetail inspecAffidavitDetailFile,
                                    Dictionary<int, double> audienceImpressions,
                                    Dictionary<int, double> ntiImpressions,
                                    Dictionary<string, DisplayBroadcastStation> stationMappings,
                                    Dictionary<int, Dictionary<int, int>> nsiMarketRankings,
                                    List<MediaMonth> mediaMonths)
        {
            var stationCallLetters = new StationProcessingEngine().StripStationSuffix(inspecAffidavitDetailFile.Station);
            var foundStation = stationMappings.TryGetValue(stationCallLetters, out DisplayBroadcastStation currentStation);
            var rank = _CalculateRank(nsiMarketRankings, inspecAffidavitDetailFile.ProposalDetailPostingBookId, foundStation, currentStation);

            return new NsiPostReportQuarterTabRow()
            {
                Rank = rank,
                Market = foundStation ? currentStation.OriginMarket : string.Empty,
                Station = foundStation ? currentStation.LegacyCallLetters : stationCallLetters,
                NetworkAffiliate = foundStation ? currentStation.Affiliation : string.Empty,
                WeekStart = mediaWeekMappings[inspecAffidavitDetailFile.AirDate].StartDate,
                ProgramName = inspecAffidavitDetailFile.ProgramName,
                Isci = inspecAffidavitDetailFile.Isci,
                TimeAired = inspecAffidavitDetailFile.AirTime,
                DateAired = inspecAffidavitDetailFile.AirDate,
                SpotLength = spotLengthMappings.Single(x => x.Value == inspecAffidavitDetailFile.SpotLengthId).Key,
                Advertiser = advertiser,
                DaypartName = inspecAffidavitDetailFile.Adu ? "ADU" : inspecAffidavitDetailFile.DaypartName,
                AudienceImpressions = audienceImpressions,
                NtiImpressions = ntiImpressions,
                ProposalWeekTotalCost = inspecAffidavitDetailFile.ProposalWeekTotalCost,
                ProposalWeekCost = inspecAffidavitDetailFile.ProposalWeekCost,
                ProposalWeekTotalImpressionsGoal = inspecAffidavitDetailFile.ProposalWeekTotalImpressionsGoal,
                ProposalWeekImpressionsGoal = inspecAffidavitDetailFile.ProposalWeekImpressionsGoal,
                ProposalWeekUnits = inspecAffidavitDetailFile.Units,
                ProposalWeekCPM = inspecAffidavitDetailFile.ProposalWeekCPM,
                ProposalWeekId = inspecAffidavitDetailFile.ProposalWeekId,
                ProposalDetailSpotLength = spotLengthMappings.Single(x => x.Value == inspecAffidavitDetailFile.ProposalDetailSpotLengthId).Key,
                Adu = inspecAffidavitDetailFile.Adu,
                Brand = inspecAffidavitDetailFile.Brand,
                ProposalDetailPostingBook = mediaMonths.Single(x => x.Id == inspecAffidavitDetailFile.ProposalDetailPostingBookId).GetCompactMonthNameAndYear(),
                ProposalDetailPlaybackType = EnumHelper.GetDescriptionAttribute(inspecAffidavitDetailFile.ProposalDetailPlaybackType)
            };
        }

        private List<NsiPostReportQuarterSummaryTableRow> _LoadPostReportQuarterSummaryTableRows(List<NsiPostReportQuarterTabRow> tabRows, int guaranteedDemoId, bool isNtiReport)
        {
            return tabRows.GroupBy(x => new
            {
                x.DaypartName,
                x.ProposalDetailSpotLength,
                x.WeekStart,
                x.Adu
            }).OrderBy(x => x.Key.WeekStart).ThenBy(x => x.Key.ProposalDetailSpotLength).ThenBy(x => x.Key.Adu).Select(x =>
            {
                var items = x.ToList();
                var row = new NsiPostReportQuarterSummaryTableRow
                {
                    Contract = x.Key.DaypartName,
                    SpotLength = x.Key.ProposalDetailSpotLength,
                    WeekStartDate = x.Key.WeekStart,
                    Spots = items.GroupBy(y => new { y.ProposalWeekId, y.ProposalWeekUnits }).Select(y => y.Key.ProposalWeekUnits).Sum(),
                    ActualImpressions = isNtiReport
                    ? items.Select(y => y.NtiImpressions.Where(w => w.Key == guaranteedDemoId).Sum(w => w.Value)).Sum()
                    : items.Select(y => y.AudienceImpressions.Where(w => w.Key == guaranteedDemoId).Sum(w => w.Value)).Sum(),
                    ProposalWeekTotalCost = items.GroupBy(y => new { y.ProposalWeekId, y.ProposalWeekTotalCost }).Select(y => y.Key.ProposalWeekTotalCost).Sum(),
                    ProposalWeekTotalImpressionsGoal = items.GroupBy(y => new { y.ProposalWeekId, y.ProposalWeekTotalImpressionsGoal }).Select(y => y.Key.ProposalWeekTotalImpressionsGoal).Sum()
                };
                row.DeliveredImpressionsPercentage = row.ActualImpressions / row.ProposalWeekTotalImpressionsGoal;
                row.ProposalWeekCost = row.ProposalWeekTotalCost / row.Spots;
                row.ProposalWeekImpressionsGoal = (row.ProposalWeekTotalImpressionsGoal / row.Spots);
                row.ProposalWeekCPM = row.ProposalWeekCost / (decimal)row.ProposalWeekImpressionsGoal * 1000;
                row.Adu = x.Key.Adu;
                return row;
            }).ToList();

        }

        private static int _CalculateRank(Dictionary<int, Dictionary<int, int>> nsiMarketRankings, int? proposalDetailPostingBookId, bool foundStation, DisplayBroadcastStation currentStation)
        {
            int rank = 0;
            if (foundStation && proposalDetailPostingBookId.HasValue)
            {
                var hasMarketRanksForPostingBook = nsiMarketRankings.TryGetValue(proposalDetailPostingBookId.Value, out Dictionary<int, int> marketRankForPostingBook);

                if (hasMarketRanksForPostingBook)
                {
                    marketRankForPostingBook.TryGetValue(currentStation.MarketCode, out rank);
                }
            }

            return rank;
        }

        private string _GenerateReportName(string proposalName, List<InSpecAffidavitFileDetail> inSpecAffidavitFileDetails, string advertiser, bool withOvernightImpressions, bool isNtiReport)
        {
            var quartersGroup = inSpecAffidavitFileDetails.GroupBy(d => new { d.Year, d.Quarter }).OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Quarter);

            if (quartersGroup.Any())
            {
                string firstQuarter = $"{quartersGroup.Select(x => x.Key.Quarter).First() }Q{ quartersGroup.Select(x => x.Key.Year.ToString().Substring(2)).First()}";
                string lastQuarter = $"{quartersGroup.Select(x => x.Key.Quarter).Last() }Q{ quartersGroup.Select(x => x.Key.Year.ToString().Substring(2)).Last()}";
                return String.Format("{0} - {1} - Cadent Network {2} Post Report{3}{4}.xlsx",
                    proposalName,
                    advertiser,
                    firstQuarter.Equals(lastQuarter) ? firstQuarter : $"{firstQuarter}-{lastQuarter}",
                    isNtiReport ? " - NTI" : string.Empty,
                    WithOvernightImpressions ? " with Overnights" : string.Empty);
            }
            else
            {
                return String.Format("{0} - {1} - Cadent Network Post Report{2}{3}.xlsx",
                    proposalName,
                    advertiser,
                    isNtiReport ? " - NTI" : string.Empty,
                    WithOvernightImpressions ? " with Overnights" : string.Empty);
            }
        }

        private void _ApplyAduAdjustments(List<PostReportQuarterSummaryTable> quarterTables)
        {
            foreach (var quarterTable in quarterTables)
            {
                foreach (var quarterRow in quarterTable.TableRows.Where(x => x.Adu == true))
                {
                    var weeks = quarterTable.TableRows.Where(x => x.WeekStartDate == quarterRow.WeekStartDate && x.SpotLength == quarterRow.SpotLength && !x.Adu);

                    quarterRow.ProposalWeekTotalImpressionsGoal = null;
                    quarterRow.ProposalWeekImpressionsGoal = null;
                    quarterRow.DeliveredImpressionsPercentage = null;
                    quarterRow.ProposalWeekCost = null;
                    quarterRow.ProposalWeekTotalCost = null;
                    quarterRow.ProposalWeekCPM = null;

                    foreach (var week in weeks)
                    {
                        if (week.ActualImpressions > week.ProposalWeekTotalImpressionsGoal)
                        {
                            var overflowingImpressions = week.ActualImpressions - week.ProposalWeekTotalImpressionsGoal;

                            week.ActualImpressions = week.ActualImpressions - overflowingImpressions;

                            week.DeliveredImpressionsPercentage = week.ActualImpressions / week.ProposalWeekTotalImpressionsGoal;

                            quarterRow.ActualImpressions += overflowingImpressions;
                        }
                    }
                }
            }
        }

        private void _EquivalizeImpressions(IImpressionAdjustmentEngine impressionAdjustmentEngine, int spotLength, ref Dictionary<int, double> audienceImpressions)
        {
            if (Equivalized)
            {
                foreach (var key in audienceImpressions.Keys.ToArray())
                {
                    audienceImpressions[key] = impressionAdjustmentEngine.AdjustImpression(audienceImpressions[key], true, spotLength);
                }
            }
        }

        private void _ApplyOvernightImpressions(Dictionary<int, double> audienceImpressions, Dictionary<int, double> overnightImpressions)
        {
            if (WithOvernightImpressions)
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
        }

        private List<string> _GetFormattedFlights(List<Tuple<DateTime, DateTime>> flightDates, List<PostReportQuarterSummaryTable> quarterTables)
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
    }
}
