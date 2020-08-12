using Services.Broadcast.Entities.Plan;
using Services.Broadcast.ReportGenerators.CampaignExport;
using Services.Broadcast.ReportGenerators.Quote;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.QuoteReport
{
    public class QuoteReportData
    {
        private const string FILENAME_FORMAT = "PlanQuote_{0}_{1}.xlsx";
        private const string DATE_FORMAT_FILENAME = "MMddyyyy";
        private const string TIME_FORMAT_FILENAME = "HHmmss";

        private const int DEFAULT_SPOTS_ALLOCATED = 1;

        private const int COLUMN_INDEX_PROGRAM_NAME = 1;
        private const int COLUMN_INDEX_DAYPART_TEXT = 2;
        private const int COLUMN_INDEX_STATION_CALLSIGN = 3;
        private const int COLUMN_INDEX_MARKET_NAME = 4;

        public DateTime GeneratedTimeStamp { get; }
        public string ExportFileName { get; }
        public object[][] RateDetailsTableAudienceHeaders { get; }
        public object[][] RateDetailsTableData { get; }

        public QuoteReportData(QuoteRequestDto request,
            DateTime generatedTimeStamp,
            List<PlanAudienceDisplay> allAudiences,
            List<MarketCoverage> allMarkets,
            List<QuoteProgram> programs)
        {
            GeneratedTimeStamp = generatedTimeStamp;
            ExportFileName = _GetExportedFileName(generatedTimeStamp);

            var transformed = _TransformProgramToRateDetailLines(request, allMarkets, programs);
            var orderedAudiences = _GetOrderedAudiences(request, allAudiences);

            var rateDetailsTableAudienceHeaders = _GetRateDetailsTableAudienceHeaders(orderedAudiences);
            var rateDetailsTableData = _GetRateDetailsTableData(transformed, orderedAudiences);

            RateDetailsTableAudienceHeaders = rateDetailsTableAudienceHeaders;
            RateDetailsTableData = rateDetailsTableData;
        }

        internal static string _GetExportedFileName(DateTime generatedDateTime)
        {
            var result = string.Format(
                FILENAME_FORMAT,
                generatedDateTime.ToString(DATE_FORMAT_FILENAME),
                generatedDateTime.ToString(TIME_FORMAT_FILENAME));
            return result;
        }

        internal static List<QuoteReportRateDetailLine> _TransformProgramToRateDetailLines(QuoteRequestDto request,
            List<MarketCoverage> allMarkets, List<QuoteProgram> inventory)
        {
            var detailLines = new List<QuoteReportRateDetailLine>();

            foreach (var item in inventory)
            {
                var itemAudiences = _GetQuoteReportRateDetailLineAudience(request.AudienceId, item.DeliveryPerAudience);
                var itemMarket = allMarkets.Single(m => m.MarketCode == item.Station.MarketCode);

                foreach (var daypart in item.ManifestDayparts)
                {
                    foreach (var creativeLength in request.CreativeLengths)
                    {
                        // BP-1093 will bring in multiple spot lengths.
                        // for now there will be only one.
                        var itemRate = item.ManifestRates.Single(s => s.SpotLengthId == creativeLength.SpotLengthId);
                        var spotCost = itemRate.Cost;

                        var lineDetail = new QuoteReportRateDetailLine
                        {
                            DaypartName = daypart.Daypart.Preview,
                            ProgramName = daypart.PrimaryProgram.Name,
                            StationCallsign = item.Station.LegacyCallLetters,
                            MarketName = itemMarket.Market,
                            Affiliate = item.Station.Affiliation,
                            PlanAudiences = itemAudiences,
                            SpotsAllocated = DEFAULT_SPOTS_ALLOCATED,
                            SpotCost = spotCost
                        };
                        detailLines.Add(lineDetail);
                    }
                }
            }

            return detailLines;
        }

        private static List<QuoteReportRateDetailLineAudience> _GetQuoteReportRateDetailLineAudience(int primaryAudienceId,
            List<QuoteProgram.ImpressionsPerAudience> itemAudiences)
        {
            const int householdAudienceId = 31;
            var linePlanAudiences = new List<QuoteReportRateDetailLineAudience>();

            foreach (var audience in itemAudiences)
            {
                var linePlanAudience = new QuoteReportRateDetailLineAudience
                {
                    IsPrimaryDemo = audience.AudienceId == primaryAudienceId,
                    IsHouseHolds = audience.AudienceId == householdAudienceId,
                    AudienceId = audience.AudienceId,
                    CPM = audience.CPM,
                    Impressions = audience.Impressions
                };
                linePlanAudiences.Add(linePlanAudience);
            }

            return linePlanAudiences;
        }

        internal static List<PlanAudienceDisplay> _GetOrderedAudiences(QuoteRequestDto request, List<PlanAudienceDisplay> allAudiences)
        {
            var orderedAudiences = new List<PlanAudienceDisplay>();

            var primaryAudienceDetail = allAudiences.Single(a => a.Id == request.AudienceId);
            orderedAudiences.Add(primaryAudienceDetail);

            foreach (var secondaryAudience in request.SecondaryAudiences)
            {
                var secondaryAudienceDetail = allAudiences.Single(a => a.Id == secondaryAudience.AudienceId);
                orderedAudiences.Add(secondaryAudienceDetail);
            }

            return orderedAudiences;
        }

        internal static object[][] _GetRateDetailsTableAudienceHeaders(List<PlanAudienceDisplay> orderedAudiences)
        {
            var audienceHeaders = orderedAudiences.Select(a => (object)$"{a.Display} CPM").ToArray();
            var result = new[] { audienceHeaders };
            return result;
        }

        internal static object[][] _GetRateDetailsTableData(List<QuoteReportRateDetailLine> lineDetails,
            List<PlanAudienceDisplay> orderedAudiences)
        {
            var lines = new ConcurrentBag<object[]>();

            Parallel.ForEach(lineDetails, (lineDetail) =>
            {
                var lineColumnVales = _GetRateDetailsTableDataLine(lineDetail, orderedAudiences);
                lines.Add(lineColumnVales);
            });

            var orderedLines = lines.
                OrderBy(s => s[COLUMN_INDEX_PROGRAM_NAME]).
                ThenBy(s => s[COLUMN_INDEX_DAYPART_TEXT]).
                ThenBy(s => s[COLUMN_INDEX_STATION_CALLSIGN]).
                ThenBy(s => s[COLUMN_INDEX_MARKET_NAME]).
                ToArray();

            return orderedLines;
        }

        private static object[] _GetRateDetailsTableDataLine(QuoteReportRateDetailLine lineDetail, List<PlanAudienceDisplay> orderedAudiences)
        {
            var lineColumnValues = new List<object>
            {
                lineDetail.ProgramName,
                lineDetail.DaypartName,
                lineDetail.StationCallsign,
                lineDetail.MarketName,
                lineDetail.Affiliate,
            };

            var hhCpm = lineDetail.PlanAudiences.Single(a => a.IsHouseHolds).CPM;
            lineColumnValues.Add(hhCpm);

            foreach (var orderedAudience in orderedAudiences)
            {
                var audienceCpm = lineDetail.PlanAudiences.Single(a => a.AudienceId == orderedAudience.Id).CPM;
                lineColumnValues.Add(audienceCpm);
            }

            lineColumnValues.Add(lineDetail.SpotsAllocated);

            // skip these 
            lineColumnValues.Add(ExportSharedLogic.EMPTY_CELL); // TotalCost.Formula
            lineColumnValues.Add(ExportSharedLogic.EMPTY_CELL); // TotalPrimaryDemoImpressions.Formula
            lineColumnValues.Add(ExportSharedLogic.EMPTY_CELL); // Blank

            // hidden
            lineColumnValues.Add(lineDetail.SpotCost);

            // hidden
            var primaryDemoImpressions = lineDetail.PlanAudiences.Single(a => a.IsPrimaryDemo).Impressions;
            lineColumnValues.Add(primaryDemoImpressions);

            return lineColumnValues.ToArray();
        }
    }
}
