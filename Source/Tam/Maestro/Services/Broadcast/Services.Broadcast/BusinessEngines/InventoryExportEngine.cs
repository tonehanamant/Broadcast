using Common.Services.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Inventory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        /// Generates the export file.
        /// </summary>
        InventoryExportGenerationResult GenerateExportFile(List<InventoryExportLineDetail> lineDetails, List<int> weekIds,
            List<DisplayBroadcastStation> stations, List<MarketCoverage> markets,
            Dictionary<int, DisplayDaypart> dayparts, List<DateTime> weekStartDates);
    }

    /// <summary>
    /// Operations for generating the inventory export file.
    /// </summary>
    public class InventoryExportEngine : BroadcastBaseClass, IInventoryExportEngine
    {
        /// <summary>
        /// The types of columns used in this export
        /// </summary>
        public enum ColumnTypeEnum
        {
            Text,
            Integer,
            Money
        }

        /// <summary>
        /// Local column descriptor
        /// </summary>
        public class ColumnDescriptor
        {
            public int ColumnIndex { get; set; }
            public string Name { get; set; }
            public ColumnTypeEnum ColumnType { get; set; }
            public double Width { get; set; } = 10.38;
            public bool IsValueCentered { get; set; } 
            public bool IsHeaderBold { get; set; }
        }

        /// <summary>
        /// The base column headers
        /// </summary>
        private readonly List<ColumnDescriptor> _BaseColumnHeaders = new List<ColumnDescriptor>
        {
            new ColumnDescriptor {ColumnIndex = 1, Name = "Rank", ColumnType = ColumnTypeEnum.Integer, Width = 7 },
            new ColumnDescriptor {ColumnIndex = 2, Name = "Market", ColumnType = ColumnTypeEnum.Text, Width = 27.43},
            new ColumnDescriptor {ColumnIndex = 3, Name = "Station", ColumnType = ColumnTypeEnum.Text, Width = 8.29},
            new ColumnDescriptor {ColumnIndex = 4, Name = "Timeslot", ColumnType = ColumnTypeEnum.Text, Width = 20.57},
            new ColumnDescriptor {ColumnIndex = 5, Name = "Program name", ColumnType = ColumnTypeEnum.Text, Width = 37},
            new ColumnDescriptor {ColumnIndex = 6, Name = ":30 Rate", ColumnType = ColumnTypeEnum.Money, IsValueCentered = true},
            new ColumnDescriptor {ColumnIndex = 7, Name = "HH Imp(000)", ColumnType = ColumnTypeEnum.Integer, IsValueCentered = true},
            new ColumnDescriptor {ColumnIndex = 8, Name = ":30 CPM", ColumnType = ColumnTypeEnum.Money, IsValueCentered = true},
        };

        /// <inheritdoc />
        public List<InventoryExportLineDetail> Calculate(List<InventoryExportDto> items)
        {
            var result = new List<InventoryExportLineDetail>();

            var stationDaypartGroups = items.Where(s => s.StationId.HasValue).GroupBy(s => new {StationId = s.StationId.Value, DaypartId = s.DaypartId});
            foreach (var stationDaypartGroup in stationDaypartGroups)
            {
                var itemWeeks = new List<InventoryExportLineWeekDetail>();

                var weekGroups = stationDaypartGroup.GroupBy(g => g.MediaWeekId).ToList();
                foreach (var weekItems in weekGroups)
                {
                    // Average these in case of multiple program names
                    var weekAvgSpotCost = weekItems.Average(s => s.SpotCost);
                    var weekAvgHhImpressions = weekItems.Average(s => s.Impressions ?? 0);
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
                var programNames = stationDaypartGroup.Select(s => s.ProgramName).Distinct().ToList();

                var lineItem = new InventoryExportLineDetail
                {
                    StationId = stationDaypartGroup.Key.StationId,
                    DaypartId = stationDaypartGroup.Key.DaypartId,
                    ProgramNames = programNames,
                    AvgSpotCost = avgSpotCost,
                    AvgHhImpressions = avgHhImpressions,
                    AvgCpm = avgCpm,
                    Weeks = itemWeeks
                };

                result.Add(lineItem);
            }

            return result;
        }

        /// <inheritdoc />
        public InventoryExportGenerationResult GenerateExportFile(List<InventoryExportLineDetail> lineDetails, List<int> weekIds,
            List<DisplayBroadcastStation> stations, List<MarketCoverage> markets, 
            Dictionary<int, DisplayDaypart> dayparts, List<DateTime> weekStartDates)
        {
            var columnDescriptors = _GetInventoryWorksheetColumnDescriptors(weekStartDates);
            var lines = _TransformToExportLines(lineDetails, weekIds, stations, markets, dayparts);

            var excelPackage = new ExcelPackage();
            var excelInventoryTab = excelPackage.Workbook.Worksheets.Add("Inventory");

            const int headerRowIndex = 1;
            var rowIndex = headerRowIndex;
            // add the header
            for (var i = 0; i < columnDescriptors.Count; i++)
            {
                excelInventoryTab.Cells[rowIndex, (i+1)].Value = columnDescriptors[i].Name;
                excelInventoryTab.Column((i + 1)).Width = columnDescriptors[i].Width;
                excelInventoryTab.Cells[rowIndex, (i + 1)].Style.Font.Bold = columnDescriptors[i].IsHeaderBold;
                excelInventoryTab.Column((i + 1)).Style.HorizontalAlignment = columnDescriptors[i].IsValueCentered ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Right;
                excelInventoryTab.Column((i + 1)).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            }

            // add the lines
            lines.ForEach(line =>
            {
                rowIndex++;
                for (var i = 0; i < columnDescriptors.Count; i++)
                {
                    switch (columnDescriptors[i].ColumnType)
                    {
                        case ColumnTypeEnum.Integer:
                            excelInventoryTab.Cells[rowIndex, (i + 1)].Style.Numberformat.Format = "###,###,##0";
                            break;
                        case ColumnTypeEnum.Money:
                            excelInventoryTab.Cells[rowIndex, (i + 1)].Style.Numberformat.Format = "$###,###,##0";
                            break;
                    }

                    excelInventoryTab.Cells[rowIndex, (i + 1)].Value = line[i];
                }
            });
            var result = new InventoryExportGenerationResult
            {
                InventoryTabLineCount = lines.Count,
                ExportExcelPackage = excelPackage
            };
            return result;
        }

        protected List<ColumnDescriptor> _GetInventoryWorksheetColumnDescriptors(List<DateTime> weekStartDates)
        {
            var headers = _BaseColumnHeaders;
            var columnIndex = headers.Max(h => h.ColumnIndex) + 1;
            
            headers.AddRange(weekStartDates.Select(w => 
                new ColumnDescriptor {
                    ColumnIndex = columnIndex++,
                    Name = w.ToString("MM/dd"),
                    ColumnType = ColumnTypeEnum.Money,
                    IsHeaderBold = true,
                    IsValueCentered = true
                }));
            return headers;
        }

        protected List<List<object>> _TransformToExportLines(List<InventoryExportLineDetail> lineDetails, List<int> weekIds,
            List<DisplayBroadcastStation> stations, List<MarketCoverage> markets, Dictionary<int, DisplayDaypart> dayparts)
        {
            var lineStrings = new ConcurrentBag<List<object>>();

            Parallel.ForEach(lineDetails, (line) =>
                {
                    // first or default to allow handling of missing station.
                    var station = stations.FirstOrDefault(s => s.Id.Equals(line.StationId) && s.MarketCode.HasValue);
                    var market = station == null ? null : markets.FirstOrDefault(m => m.MarketCode.Equals(station.MarketCode));
                    var daypart = dayparts.ContainsKey(line.DaypartId) ? dayparts[line.DaypartId] : null;

                    var lineColumnValues = _TransformToLine(line, weekIds, station, market, daypart);
                    lineStrings.Add(lineColumnValues);
                }
            );

            var rankListIndex = _BaseColumnHeaders.First(s => s.Name.Equals("Rank")).ColumnIndex - 1; // 0 indexed
            var marketNameListIndex = _BaseColumnHeaders.First(s => s.Name.Equals("Market")).ColumnIndex - 1;  // 0 indexed
            var stationCallsignsListIndex = _BaseColumnHeaders.First(s => s.Name.Equals("Station")).ColumnIndex - 1;  // 0 indexed
            var daypartListIndex = _BaseColumnHeaders.First(s => s.Name.Equals("Timeslot")).ColumnIndex - 1;  // 0 indexed

            var orderedLines = lineStrings.
                OrderBy(s => s[rankListIndex]).
                ThenBy(s => s[marketNameListIndex]).
                ThenBy(s => s[stationCallsignsListIndex]).
                ThenBy(s => s[daypartListIndex]).
                ToList();

            return orderedLines;
        }

        private List<object> _TransformToLine(InventoryExportLineDetail lineDetailDetail, List<int> weekIds, DisplayBroadcastStation station,
            MarketCoverage market, DisplayDaypart daypart)
        {
            const string unknownIndicator = "UNKNOWN";
            var marketRank = market?.Rank ?? -1;
            var marketName = market?.Market ?? unknownIndicator;
            var callLetters = string.IsNullOrEmpty(station?.LegacyCallLetters) ? unknownIndicator : station.LegacyCallLetters;
            var daypartText = daypart == null ? unknownIndicator : daypart.Preview;
            var programNames = string.Join("/", lineDetailDetail.ProgramNames);

            var formattedRate = lineDetailDetail.AvgSpotCost;
            var formattedImpressions = Convert.ToInt32(Math.Round(lineDetailDetail.AvgHhImpressions / 1000));
            var formattedCpm = lineDetailDetail.AvgCpm;

            var lineColumnValues = new List<object> { marketRank, marketName, callLetters, daypartText, programNames, formattedRate, formattedImpressions, formattedCpm };
            weekIds.ForEach(weekId =>
            {
                var foundWeek = lineDetailDetail.Weeks.FirstOrDefault(s => s.MediaWeekId.Equals(weekId));
                var formattedWeekRate = foundWeek?.SpotCost ?? 0;
                lineColumnValues.Add(formattedWeekRate);
            });

            return lineColumnValues;
        }
    }
}