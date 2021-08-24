using Common.Services.Extensions;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.BusinessEngines
{
    /// <summary>
    /// Operations for generating the inventory export file.
    /// </summary>
    public interface IInventoryExportEngine
    {
        /// <summary>
        /// Perform the media math calculations on the given inventory.
        /// </summary>
        List<InventoryExportLineDetail> Calculate(List<InventoryExportDto> items);

        /// <summary>
        /// Get the formatted name of the file.
        /// </summary>
        string GetInventoryExportFileName(InventoryExportGenreTypeEnum genre, QuarterDetailDto quarter);

        /// <summary>
        /// Generates the value for when the export was generated.
        /// </summary>
        string GetExportGeneratedTimestamp(DateTime generatedTimestamp);

        /// <summary>
        /// Generates the inventory table column headers for the given audiences.
        /// </summary>
        object[][] GetInventoryTableAudienceColumnHeaders(List<LookupDto> audiences);

        /// <summary>
        /// Generates the inventory table column headers for the given media weeks.
        /// </summary>
        object[][] GetInventoryTableWeeklyColumnHeaders(List<DateTime> mediaWeekStartDates);

        /// <summary>
        /// Get the table data.
        /// </summary>
        object[][] GetInventoryTableData(List<InventoryExportLineDetail> inventoryExportLineDetails,
            List<DisplayBroadcastStation> stations, List<MarketCoverage> markets,
            List<int> mediaWeekIds, Dictionary<int, DisplayDaypart> dayparts,
            List<LookupDto> audiences, List<LookupDto> genres);
    }

    /// <summary>
    /// Operations for generating the inventory export file.
    /// </summary>
    public class InventoryExportEngine : BroadcastBaseClass, IInventoryExportEngine
    {
        private const int COLUMN_INDEX_RANK = 1;
        private const int COLUMN_INDEX_MARKET_NAME = 2;
        private const int COLUMN_INDEX_STATION_CALLSIGN = 3;
        private const int COLUMN_INDEX_DAYPART_TEXT = 5;

        public InventoryExportEngine(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {

        }

        /// <inheritdoc />
        public string GetInventoryExportFileName(InventoryExportGenreTypeEnum genre, QuarterDetailDto quarter)
        {
            var genreValue = genre.GetDescriptionAttribute().ToUpper();
            var fileName = $"Open Market inventory {genreValue} {quarter.Year} Q{quarter.Quarter}.xlsx";
            return fileName;
        }

        /// <inheritdoc />
        public string GetExportGeneratedTimestamp(DateTime generatedTimestamp)
        {
            const string format = "MM/dd/yyyy HH:mm:ss";
            var result = $"Generated : {generatedTimestamp.ToString(format)}";
            return result;
        }

        /// <inheritdoc />
        public List<InventoryExportLineDetail> Calculate(List<InventoryExportDto> items)
        {
            var result = new List<InventoryExportLineDetail>();

            var inventoryGroups = items
                .GroupBy(s => s.InventoryId)
                .ToList();

            foreach (var inventoryGroup in inventoryGroups)
            {
                var inventoryItem = inventoryGroup.First();
                var itemWeeks = new List<InventoryExportLineWeekDetail>();
                var weekGroups = inventoryGroup.GroupBy(g => g.MediaWeekId).ToList();
                foreach (var weekItems in weekGroups)
                {
                    // Average these in case of multiple program names
                    var weekAvgSpotCost = weekItems.Average(s => s.SpotCost);
                    var weekAvgHhImpressions = weekItems.Average(s => s.HhImpressionsProjected ?? 0);
                    var weekAvgCpm = weekAvgHhImpressions.Equals(0) ? 0 : (weekAvgSpotCost / (decimal)weekAvgHhImpressions) * 1000;

                    var weekItem = new InventoryExportLineWeekDetail
                    {
                        MediaWeekId = weekItems.Key,
                        SpotCost = weekAvgSpotCost,
                        HhImpressions = weekAvgHhImpressions,
                        Cpm = weekAvgCpm
                    };

                    itemWeeks.Add(weekItem);
                }

                // do this math off the gathered weeks so they are aligned.
                var avgSpotCost = itemWeeks.Average(w => w.SpotCost);
                var avgHhImpressions = itemWeeks.Average(w => w.HhImpressions);
                var avgCpm = avgHhImpressions.Equals(0) ? 0 : (avgSpotCost / (decimal)avgHhImpressions) * 1000;

                var lineItem = new InventoryExportLineDetail
                {
                    InventoryId = inventoryItem.InventoryId,
                    StationId = inventoryItem.StationId,
                    DaypartId = inventoryItem.DaypartId,
                    InventoryProgramName = inventoryItem.InventoryProgramName,
                    ProgramName = inventoryItem.ProgramName,
                    ProgramSource = inventoryItem.ProgramSource,
                    MaestroGenreId = inventoryItem.MaestroGenreId,
                    AvgSpotCost = avgSpotCost,
                    AvgHhImpressions = avgHhImpressions,
                    AvgCpm = avgCpm,
                    Weeks = itemWeeks,
                    ProvidedAudienceImpressions = inventoryItem.ProvidedAudiences
                };

                result.Add(lineItem);
            }
            return result;
        }

        /// <inheritdoc />
        public object[][] GetInventoryTableAudienceColumnHeaders(List<LookupDto> audiences)
        {
            var headers = audiences.Select(a => (object)($"{a.Display} Imp(000)")).ToArray();
            return new[] { headers };
        }

        /// <inheritdoc />
        public object[][] GetInventoryTableWeeklyColumnHeaders(List<DateTime> mediaWeekStartDates)
        {
            var headers = mediaWeekStartDates.Select(w => (object)w.ToString("MM/dd")).ToArray();
            return new[] {headers};
        }

        /// <inheritdoc />
        public object[][] GetInventoryTableData(List<InventoryExportLineDetail> inventoryExportLineDetails,
            List<DisplayBroadcastStation> stations, List<MarketCoverage> markets,
            List<int> mediaWeekIds, Dictionary<int, DisplayDaypart> dayparts,
            List<LookupDto> audiences, List<LookupDto> genres)
        {
            var lineStrings = new ConcurrentBag<object[]>();

            Parallel.ForEach(inventoryExportLineDetails, (line) =>
            {
                // first or default to allow handling of missing station.
                var station = stations.FirstOrDefault(s => s.Id.Equals(line.StationId) && s.MarketCode.HasValue);
                var market = station == null ? null : markets.FirstOrDefault(m => m.MarketCode.Equals(station.MarketCode));
                var daypart = dayparts.ContainsKey(line.DaypartId) ? dayparts[line.DaypartId] : null;

                var lineColumnValues = _TransformToLine(line, mediaWeekIds, station, market, daypart, audiences, genres);
                lineStrings.Add(lineColumnValues);
            });

            var orderedLines = lineStrings.
                OrderBy(s => s[COLUMN_INDEX_RANK]).
                ThenBy(s => s[COLUMN_INDEX_MARKET_NAME]).
                ThenBy(s => s[COLUMN_INDEX_STATION_CALLSIGN]).
                ThenBy(s => s[COLUMN_INDEX_DAYPART_TEXT]).
                ToArray();

            return orderedLines;
        }

        private object[] _TransformToLine(InventoryExportLineDetail lineDetail, List<int> weekIds, DisplayBroadcastStation station,
            MarketCoverage market, DisplayDaypart daypart, List<LookupDto> audiences, List<LookupDto> genres)
        {
            const string unknownIndicator = "UNKNOWN";
            var marketRank = market?.Rank ?? -1;
            var marketName = market?.Market ?? unknownIndicator;
            var callLetters = string.IsNullOrWhiteSpace(station?.LegacyCallLetters) ? unknownIndicator : station.LegacyCallLetters;
            var affiliate = string.IsNullOrWhiteSpace(station?.Affiliation) ? unknownIndicator : station.Affiliation;
            var daypartText = daypart == null ? unknownIndicator : daypart.Preview;
            
            var programSource = lineDetail.ProgramSource.HasValue
                ? lineDetail.ProgramSource == ProgramSourceEnum.Master ? "Dativa" : "Mapping"
                : "None";
            var genre = genres.FirstOrDefault(g => g.Id.Equals(lineDetail.MaestroGenreId))?.Display;

            var formattedRate = lineDetail.AvgSpotCost;
            var formattedImpressions = Convert.ToInt32(Math.Round(lineDetail.AvgHhImpressions / 1000));
            var formattedCpm = lineDetail.AvgCpm;

            var lineColumnValues = new List<object> { 
                lineDetail.InventoryId,
                marketRank, 
                marketName, 
                callLetters, 
                affiliate,
                daypartText,
                lineDetail.InventoryProgramName, 
                lineDetail.ProgramName,
                genre, 
                programSource, 
                formattedRate, 
                formattedImpressions,
                formattedCpm };

            foreach (var audience in audiences)
            {
                var audienceImpressions = lineDetail.ProvidedAudienceImpressions
                    .Where(s => s.AudienceId == audience.Id)
                    .Average(s => s.Impressions);

                if (audienceImpressions.HasValue)
                {
                    lineColumnValues.Add(Convert.ToInt32(Math.Round(audienceImpressions.Value / 1000)));
                }
                else
                {
                    lineColumnValues.Add(ExportSharedLogic.NO_VALUE_CELL);
                }
            }

            weekIds.ForEach(weekId =>
            {
                var foundWeek = lineDetail.Weeks.FirstOrDefault(s => s.MediaWeekId.Equals(weekId));
                if (foundWeek == null)
                {
                    lineColumnValues.Add(ExportSharedLogic.NO_VALUE_CELL);
                }
                else
                {
                    lineColumnValues.Add(foundWeek.SpotCost);
                }
            });

            return lineColumnValues.ToArray();
        }
    }
}