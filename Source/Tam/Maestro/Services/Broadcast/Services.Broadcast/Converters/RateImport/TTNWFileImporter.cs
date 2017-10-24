using OfficeOpenXml;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Repositories;
using Tam.Maestro.Services.ContractInterfaces.Common;
using System.Text.RegularExpressions;
using Common.Services.Repositories;

namespace Services.Broadcast.Converters.RateImport
{
    public class TTNWFileImporter : InventoryFileImporterBase
    {
        private Dictionary<string, int> standardColumnOffsets = new Dictionary<string, int>()
        {
            //Assuming the main columns always have the same sequence order. zero-based
            {"dma",1},
            {"market",2},
            {"station",3},
            {"daypart",6}
        };

        private readonly int _TTNWStandardSpotLength = 30;

        class SpreadsheetTableDescriptor
        {
            public int StartRow { get; set; }
            public int EndRow { get; set; }
            public int StartCol { get; set; }
            public Dictionary<string, int> RequiredColumns { get; set; }
            public Dictionary<string, int> SpotColumns { get; set; }
            public Dictionary<string, int> AudienceImpressionsColumns { get; set; }
        }

        class TTNWFileRecord
        {
            public string StationLetters { get; set; }
            public string DaypartsString { get; set; }
            public Dictionary<string, int> Spots { get; set; }
            public Dictionary<string, double> AudienceImpressions { get; set; }

        }

        public override InventoryFile.InventorySource InventoryFileSource
        {
            get { return InventoryFile.InventorySource.TTNW; }
        }
        public override InventorySource InventorySource { get; set; }

        public override void ExtractFileData(System.IO.Stream stream, InventoryFile inventoryFile, DateTime effectiveDate,List<InventoryFileProblem> fileProblems)
        {

            var spotLengthId = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds()[_TTNWStandardSpotLength];

            using (var excelPackage = new ExcelPackage(stream))
            {
                var sheet = excelPackage.Workbook.Worksheets.First();
                var dataTable = _getDataTableDescriptor(sheet);

                if (dataTable.SpotColumns.Count == 0)
                {
                    fileProblems.Add(new InventoryFileProblem(string.Format("No valid daypart codes (spot columns) found in file.")));
        }

                //Console.WriteLine("data at " + dataTable.StartRow + " and " + dataTable.StartCol + " ending at row " + dataTable.EndRow);
                var ttnwRecords = _GetTTNWRecords(sheet, dataTable, fileProblems);
                if (ttnwRecords.Count == 0)
                {
                    return;
                }

                var validStations = _GetValidStations(ttnwRecords.Select(r => r.StationLetters).Distinct().ToList());
                if (validStations == null || validStations.Count == 0)
                {
                    fileProblems.Add(new InventoryFileProblem("There are no known stations in the file"));
                    return;
                }

                var inventoryGroups = new Dictionary<string, StationInventoryGroup>();
                foreach (var ttnwRecord in ttnwRecords)
                {
                    DisplayBroadcastStation station;
                    if(!validStations.TryGetValue(ttnwRecord.StationLetters, out station))
                    {
                        fileProblems.Add(new InventoryFileProblem(string.Format("Invalid station: {0}", ttnwRecord.StationLetters)));
                    }

                    var dayparts = _ParseDayparts(ttnwRecord.DaypartsString, ttnwRecord.StationLetters, fileProblems);

                    var manifestAudiences = _ParseManifestAudiences(ttnwRecord.AudienceImpressions);

                    foreach (var daypartCodeSpots in ttnwRecord.Spots)
                    {

                        var slotNumber = 1;
                        var daypartCode = daypartCodeSpots.Key;
                        if (daypartCode.StartsWith("EM", StringComparison.InvariantCultureIgnoreCase))
                        {
                            slotNumber = int.Parse(daypartCode.Substring(daypartCode.Length - 2));
                            daypartCode = daypartCode.Substring(0, 2);
                        }

                        StationInventoryGroup inventoryGroup;
                        if (!inventoryGroups.TryGetValue(daypartCode + slotNumber, out inventoryGroup))
                        {
                            inventoryGroup = new StationInventoryGroup()
                            {
                                DaypartCode = daypartCode,
                                Name = daypartCode + slotNumber,
                                Manifests = new List<StationInventoryManifest>(),
                                SlotNumber = slotNumber,
                                InventorySource = this.InventorySource
                            };
                            inventoryGroups.Add(daypartCode + slotNumber, inventoryGroup);
                        }

                        inventoryGroup.Manifests.Add(new StationInventoryManifest()
                        {
                            Station = station,
                            DaypartCode = daypartCode,
                            SpotsPerWeek = daypartCodeSpots.Value,
                            SpotLengthId = spotLengthId,
                            Dayparts = dayparts,
                            ManifestAudiences = manifestAudiences,
                            EffectiveDate = effectiveDate,
                        });
                    }


                }
                inventoryFile.InventoryGroups.AddRange(inventoryGroups.Values);

            }
        }

        private Dictionary<string, DisplayBroadcastStation> _GetValidStations(List<string> stationNameList)
        {
            var stationsDictionary = new Dictionary<string, DisplayBroadcastStation>();
            foreach (var stationName in stationNameList)
            {
                var station = _ParseStationCallLetters(stationName);
                if (station != null)
                {
                    stationsDictionary.Add(stationName, station);
                }
            }
            return stationsDictionary;
        }

        private List<StationInventoryManifestAudience> _ParseManifestAudiences(Dictionary<string, double> audiencesWithImpressions)
        {
            var manifestAudiences = new List<StationInventoryManifestAudience>();

            var audienceRepo = _BroadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>();

            foreach (var audience in audiencesWithImpressions)
            {
                var audienceCode = audience.Key;
                if (audienceCode.Length == 5 && !audienceCode.Contains("-"))
                {
                    audienceCode = audienceCode.Substring(0, 3) + "-" + audienceCode.Substring(3);
                }
                var displayAudience = audienceRepo.GetDisplayAudienceByCode(audienceCode);
                manifestAudiences.Add(new StationInventoryManifestAudience()
                {
                    Audience = displayAudience,
                    Impressions = audience.Value * 1000
                });
            }

            return manifestAudiences;
        }

        private List<DisplayDaypart> _ParseDayparts(string daypartInput, string station, List<InventoryFileProblem> fileProblems)
        {
            List<DisplayDaypart> dayparts = new List<DisplayDaypart>();
            var daypartProblems = new List<InventoryFileProblem>();
            try
            {

                var daypartStrings = Regex.Split(daypartInput, "\\s?/\\s?").ToList();//split on slash with optional spaces arounds

                foreach (var originalDaypartString in daypartStrings.ToList())
                {
                    var daypartString = originalDaypartString;

                    if (originalDaypartString.Contains("+"))
                    {
                        daypartStrings.Remove(originalDaypartString);
                    }

                    if (originalDaypartString.Contains("SU-"))
                    {
                        daypartString = daypartString.Replace("SU-F", "M-F + SU");
                        daypartString = daypartString.Replace("SU-TH", "M-TH + SU");
                        daypartString = daypartString.Replace("SU-W", "M-W + SU");
                        daypartString = daypartString.Replace("SU-TU", "M-TU + SU");
                        daypartString = daypartString.Replace("SU-M", "M-M + SU");
                        daypartStrings.Remove(originalDaypartString);
                    }

                    if (daypartString.Contains("+"))
                    {
                        var splitDayparts = Regex.Split(daypartString, "\\s?\\+\\s?");
                        if (splitDayparts.Count() == 2)
                        {
                            var weekdays = splitDayparts[0];
                            var weekend = splitDayparts[1].Split(' ')[0];
                            var time = splitDayparts[1].Split(' ')[1];
                            daypartStrings.Add(weekdays + " " + time);
                            daypartStrings.Add(weekend + " " + time);
                        }
                        else if (splitDayparts.Count() > 2)
                        {
                            daypartStrings.AddRange(splitDayparts);
                        }
                        else
                        {
                            //try to parse original daypart string
                            daypartStrings.Add(originalDaypartString);
                        }
                        
                    }
                }

                daypartStrings.ForEach(dp =>
                {

                        var daypart = ParseStringToDaypart(dp, station);
                        if (daypart != null)
                        {
                            dayparts.Add(daypart);
                        }

                });

            }
            catch (Exception)
            {
                daypartProblems.Add(new InventoryFileProblem(string.Format("Invalid daypart for station: {0}", station)));
            }

            fileProblems.AddRange(daypartProblems);
            return dayparts;
        }

        private DisplayBroadcastStation _ParseStationCallLetters(string stationName)
        {
            // check if it is legacy or the call letters
            var foundStation = _GetDisplayBroadcastStation(stationName);

            if (foundStation == null)
            {
                var station = stationName.Replace("-TV", "").Trim();
                foundStation = _GetDisplayBroadcastStation(station);
            }

            return foundStation;
        }

        private DisplayBroadcastStation _GetDisplayBroadcastStation(string stationName)
        {
            var _stationRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            return _stationRepository.GetBroadcastStationByLegacyCallLetters(stationName) ??
                                _stationRepository.GetBroadcastStationByCallLetters(stationName);
        }

        private List<TTNWFileRecord> _GetTTNWRecords(ExcelWorksheet sheet, SpreadsheetTableDescriptor dataTable, List<InventoryFileProblem> fileProblems)
        {
            var recordList = new List<TTNWFileRecord>();
            for (int i = dataTable.StartRow; i <= dataTable.EndRow; i++)
            {
                if (string.IsNullOrEmpty(sheet.Cells[i, dataTable.StartCol + standardColumnOffsets["dma"]].Text))
                {
                    //skip if dma column is empty
                    continue;
                }

                var row = new TTNWFileRecord();
                row.StationLetters = sheet.Cells[i, dataTable.StartCol + standardColumnOffsets["station"]].Text;
                row.DaypartsString = sheet.Cells[i, dataTable.StartCol + standardColumnOffsets["daypart"]].Text;
                row.Spots = new Dictionary<string, int>();
                foreach (var spotColumn in dataTable.SpotColumns)
                {
                    var spotValue = sheet.Cells[i, spotColumn.Value].Text;
                    if (!string.IsNullOrEmpty(spotValue))
                    {
                        int numberofSpots;
                        if (int.TryParse(sheet.Cells[i, spotColumn.Value].Text, out numberofSpots))
                        {
                            row.Spots.Add(spotColumn.Key, numberofSpots);
                        }
                        else
                        {
                            fileProblems.Add(new InventoryFileProblem(string.Format("Invalid number of spots for station {0}", row.StationLetters)));
                        }
                         
                    }
                }

                row.AudienceImpressions = new Dictionary<string, double>();
                foreach (var impressionsColumn in dataTable.AudienceImpressionsColumns)
                {
                    row.AudienceImpressions.Add(impressionsColumn.Key, double.Parse(sheet.Cells[i, impressionsColumn.Value].Text));
                }
                recordList.Add(row);

            }
            return recordList;
        }

        private SpreadsheetTableDescriptor _getDataTableDescriptor(ExcelWorksheet sheet)
        {

            //TODO: Add these operations as methods of the table descriptor
            var tableDescriptor = _FindStartCellForDataTable(sheet);
            tableDescriptor = _FindEndCellForDataTable(sheet, tableDescriptor);
            tableDescriptor = _FindRequiredColumnsForDataTable(sheet, tableDescriptor);
            tableDescriptor = _FindSpotColumnsForDataTable(sheet, tableDescriptor);
            tableDescriptor = _FindImpressionsColumnsForDataTable(sheet, tableDescriptor);

            return tableDescriptor;
        }

        private SpreadsheetTableDescriptor _FindImpressionsColumnsForDataTable(ExcelWorksheet sheet, SpreadsheetTableDescriptor tableDescriptor)
        {
            tableDescriptor.AudienceImpressionsColumns = new Dictionary<string, int>();

            //TODO: make headerRow as part of the table descriptors
            var headerRow = tableDescriptor.StartRow - 3;

            //Find end of main data table
            int lastTableCol = tableDescriptor.StartCol;
            for (int i = tableDescriptor.StartCol; i <= sheet.Dimension.End.Column; i++)
            {
                if (string.IsNullOrEmpty(sheet.Cells[headerRow, i].Text))
                {
                    lastTableCol = i;
                    break;
                }

            }

            //find first column with audience impressions
            int impressionsFirstCol = 0;
            for (int i = lastTableCol + 1; i <= sheet.Dimension.End.Column; i++)
            {
                var columnName = sheet.Cells[headerRow, i].Text;
                if (!string.IsNullOrEmpty(columnName) && !sheet.Column(i).Hidden && columnName.Equals("HH", StringComparison.InvariantCultureIgnoreCase))
                {
                    impressionsFirstCol = i;
                    break;
                }
            }

            //No impressions found
            if (impressionsFirstCol == 0)
            {
                return tableDescriptor;
            }

            for (int i = impressionsFirstCol; i <= sheet.Dimension.End.Column; i++)
            {
                if (string.IsNullOrEmpty(sheet.Cells[headerRow, i].Text))
                {
                    break;
                }
                tableDescriptor.AudienceImpressionsColumns.Add(sheet.Cells[headerRow, i].Text.ToUpper(), i);
            }

            return tableDescriptor;
        }

        private SpreadsheetTableDescriptor _FindSpotColumnsForDataTable(ExcelWorksheet sheet, SpreadsheetTableDescriptor tableDescriptor)
        {
            var headerRow = tableDescriptor.StartRow - 3;
            var initialCol = tableDescriptor.RequiredColumns["daypart"] + 1; //first column after the required ones

            tableDescriptor.SpotColumns = new Dictionary<string, int>();
            for (int i = initialCol; !string.IsNullOrEmpty(sheet.Cells[headerRow, i].Text); i++)
            {
                var columnName = sheet.Cells[headerRow, i].Text;
                if (columnName.StartsWith("WKLY TV", StringComparison.InvariantCultureIgnoreCase))
                {
                    tableDescriptor.SpotColumns.Add("LN", i);
                    break;
                }
                if (columnName.StartsWith("TTN EN", StringComparison.InvariantCultureIgnoreCase))
                {
                    tableDescriptor.SpotColumns.Add("EN", i);
                    break;
                }
                if (columnName.StartsWith("EM0", StringComparison.InvariantCultureIgnoreCase))
                {
                    tableDescriptor.SpotColumns.Add(columnName.ToUpper(), i);
                }
            }

            return tableDescriptor;
        }

        private SpreadsheetTableDescriptor _FindRequiredColumnsForDataTable(ExcelWorksheet sheet, SpreadsheetTableDescriptor tableDescriptor)
        {
            tableDescriptor.RequiredColumns = new Dictionary<string, int>();
            foreach (var columnOffset in standardColumnOffsets)
            {
                tableDescriptor.RequiredColumns.Add(columnOffset.Key, tableDescriptor.StartCol + columnOffset.Value);
            }

            return tableDescriptor;
        }

        private static SpreadsheetTableDescriptor _FindStartCellForDataTable(ExcelWorksheet sheet)
        {
            int StationCellToStartRowOffset = 3;
            int StationCellToStartColOffset = -3;
            var tableDescriptor = new SpreadsheetTableDescriptor();

            var lastRow = sheet.Dimension.End.Row;
            var lastCol = sheet.Dimension.End.Column;

            //Find start cell
            for (int i = 1; i <= lastRow; i++)
            {
                for (int j = 1; j <= lastCol; j++)
                {
                    if (sheet.Cells[i, j].Text.Equals("station", StringComparison.InvariantCultureIgnoreCase))
                    {
                        tableDescriptor.StartRow = i + StationCellToStartRowOffset;
                        tableDescriptor.StartCol = j + StationCellToStartColOffset;
                        return tableDescriptor;
                    }
                }
            }

            throw new ApplicationException("Could not locate data table in TTNW file.");

        }

        private static SpreadsheetTableDescriptor _FindEndCellForDataTable(ExcelWorksheet sheet, SpreadsheetTableDescriptor tableDescriptor)
        {

            var lastRow = sheet.Dimension.End.Row;
            var lastCol = sheet.Dimension.End.Column;
            int marketColumnOffset = 2;

            //Find end row
            for (int i = tableDescriptor.StartRow; i <= lastRow; i++)
            {
                //Since market column is always populated, we look for the cell that has market empty
                tableDescriptor.EndRow = i;
                if (string.IsNullOrEmpty(sheet.Cells[i, tableDescriptor.StartCol + marketColumnOffset].Text.Trim()))
                {
                    break;
                }

            }

            //Find end column

            return tableDescriptor;

    }

}
}
