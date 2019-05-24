using OfficeOpenXml;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Repositories;
using Tam.Maestro.Services.ContractInterfaces.Common;
using System.Text.RegularExpressions;
using Services.Broadcast.Entities.StationInventory;

namespace Services.Broadcast.Converters.RateImport
{
    public class TTWNFileImporter : InventoryFileImporterBase
    {
        private Dictionary<string, int> standardColumnOffsets = new Dictionary<string, int>()
        {
            //Assuming the main columns always have the same sequence order. zero-based
            {"dma",1},
            {"market",2},
            {"station",3},
            {"daypart",6}
        };

        private readonly int _TTWNStandardSpotLength = 30;

        class SpreadsheetTableDescriptor
        {
            public int StartRow { get; set; }
            public int EndRow { get; set; }
            public int StartCol { get; set; }
            public Dictionary<string, int> RequiredColumns { get; set; }
            public Dictionary<string, int> SpotColumns { get; set; }
            public Dictionary<string, int> AudienceImpressionsColumns { get; set; }
        }

        class TTWNFileRecord
        {
            public string StationLetters { get; set; }
            public string DaypartsString { get; set; }
            public Dictionary<string, int> Spots { get; set; }
            public Dictionary<string, double> AudienceImpressions { get; set; }

        }

        public override void ExtractFileData(System.IO.Stream stream, InventoryFile inventoryFile)
        {
            var spotLengthId = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds()[_TTWNStandardSpotLength];

            using (var excelPackage = new ExcelPackage(stream))
            {
                var sheet = excelPackage.Workbook.Worksheets.First();
                var dataTable = _getDataTableDescriptor(sheet);

                if (dataTable.SpotColumns.Count == 0)
                {
                    FileProblems.Add(new InventoryFileProblem(string.Format("No valid daypart codes (spot columns) found in file.")));
                }

                var ttwnRecords = _GetTTWNRecords(sheet, dataTable);

                if (ttwnRecords.Count == 0)
                {
                    return;
                }

                var validStations = FindStations(ttwnRecords.Select(r => r.StationLetters).Distinct().ToList());

                if (validStations == null || validStations.Count == 0)
                {
                    FileProblems.Add(new InventoryFileProblem("There are no known stations in the file"));
                    return;
                }

                var inventoryGroups = new Dictionary<string, StationInventoryGroup>();

                foreach (var ttwnRecord in ttwnRecords)
                {
                    DisplayBroadcastStation station;

                    if(!validStations.TryGetValue(ttwnRecord.StationLetters, out station))
                    {
                        FileProblems.Add(new InventoryFileProblem(string.Format("Invalid station: {0}", ttwnRecord.StationLetters)));
                    }

                    var dayparts = ParseDayparts(ttwnRecord.DaypartsString, ttwnRecord.StationLetters);

                    var manifestDayparts = dayparts.Select(
                        d => new StationInventoryManifestDaypart()
                        {
                            Daypart = d
                        }).ToList();

                    var manifestAudiences = _ParseManifestAudiences(ttwnRecord.AudienceImpressions);

                    foreach (var daypartCodeSpots in ttwnRecord.Spots)
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
                                InventorySource = this.InventorySource,
                            };

                            inventoryGroups.Add(daypartCode + slotNumber, inventoryGroup);
                        }

                        inventoryGroup.Manifests.Add(new StationInventoryManifest()
                        {
                            Station = station,
                            DaypartCode = daypartCode,
                            SpotsPerWeek = daypartCodeSpots.Value,
                            SpotLengthId = spotLengthId,
                            ManifestDayparts = manifestDayparts,
                            ManifestAudiencesReferences = manifestAudiences,
                        });
                    }
                }

                inventoryFile.InventoryGroups.AddRange(inventoryGroups.Values);
            }
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
                    Impressions = audience.Value * 1000,
                    IsReference = true
                });
            }

            return manifestAudiences;
        }

        private List<TTWNFileRecord> _GetTTWNRecords(ExcelWorksheet sheet, SpreadsheetTableDescriptor dataTable)
        {
            var recordList = new List<TTWNFileRecord>();
            
            for (int i = dataTable.StartRow; i <= dataTable.EndRow; i++)
            {
                if (string.IsNullOrEmpty(sheet.Cells[i, dataTable.StartCol + standardColumnOffsets["dma"]].Text))
                {
                    //skip if dma column is empty
                    continue;
                }

                var row = new TTWNFileRecord
                {
                    StationLetters = sheet.Cells[i, dataTable.StartCol + standardColumnOffsets["station"]].Text,
                    DaypartsString = sheet.Cells[i, dataTable.StartCol + standardColumnOffsets["daypart"]].Text,
                    Spots = new Dictionary<string, int>()
                };

                foreach (var spotColumn in dataTable.SpotColumns)
                {
                    var spotValue = sheet.Cells[i, spotColumn.Value].Text;

                    if (!string.IsNullOrEmpty(spotValue))
                    {
                        int numberofSpots;
                        var parseSucceeded = int.TryParse(sheet.Cells[i, spotColumn.Value].Text, out numberofSpots);

                        if (parseSucceeded && numberofSpots > 0)
                        {
                            row.Spots.Add(spotColumn.Key, numberofSpots);
                        }
                        else
                        {
                            FileProblems.Add(new InventoryFileProblem(string.Format("Invalid number of spots for station {0}", row.StationLetters)));
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

            // TODO: make headerRow as part of the table descriptors.
            var headerRow = tableDescriptor.StartRow - 3;

            // Find end of main data table.
            int lastTableCol = tableDescriptor.StartCol;

            for (int i = tableDescriptor.StartCol + 1; i <= sheet.Dimension.End.Column; i++)
            {
                if (string.IsNullOrEmpty(sheet.Cells[headerRow, i].Text))
                {
                    lastTableCol = i;
                    break;
                }

            }

            // Find first column with audience impressions.
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

            // Try again, but one row below. 
            // TTWN EN tables audience header data is one row below the other two formats.
            if (impressionsFirstCol == 0)
            {
                for (int i = lastTableCol + 1; i <= sheet.Dimension.End.Column; i++)
                {
                    var columnName = sheet.Cells[headerRow + 1, i].Text;

                    if (!string.IsNullOrEmpty(columnName) && !sheet.Column(i).Hidden && columnName.Equals("HH", StringComparison.InvariantCultureIgnoreCase))
                    {
                        impressionsFirstCol = i;
                        // Increment header row so the next loop is able to find the impressions data.
                        headerRow += 1;
                        break;
                    }
                }
            }

            // No impressions found.
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

            throw new ApplicationException("Could not locate data table in TTWN file.");

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
