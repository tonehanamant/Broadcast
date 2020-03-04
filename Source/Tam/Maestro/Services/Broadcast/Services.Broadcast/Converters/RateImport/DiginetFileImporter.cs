using Common.Services;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.ProprietaryInventory.ProprietaryInventoryFile;
using static Services.Broadcast.Entities.ProprietaryInventory.ProprietaryInventoryFile.ProprietaryInventoryDataLine;

namespace Services.Broadcast.Converters.RateImport
{
    public class DiginetFileImporter : ProprietaryFileImporterBase
    {
        private enum StationColumnIndex
        {
            CallSign = 1,
            StationName = 2,
            Affiliate = 3,
            TimeZone = 4
        }

        private readonly FileCell INVENTORY_SOURCE_CELL = new FileCell {ColumnLetter = "B", RowIndex = 3};
        private readonly FileCell EFFECTIVE_DATE_CELL = new FileCell { ColumnLetter = "B", RowIndex = 4 };
        private readonly FileCell END_DATE_CELL = new FileCell { ColumnLetter = "B", RowIndex = 5 };
        private readonly FileCell NTI_TO_NSI_INCREASE_CELL = new FileCell { ColumnLetter = "B", RowIndex = 6 };

        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;

        private int _ErrorColumnIndex = 0;
        private const int audienceRowIndex = 11;

        public string InventorySourceName { get; set; } = string.Empty;
        public List<string> FileStationsCallsigns { get; private set; } = new List<string>();

        public override bool HasSecondWorksheet { get; } = true;

        public DiginetFileImporter(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine,
            IImpressionAdjustmentEngine impressionAdjustmentEngine,
            IFileService fileService,
            IStationMappingService stationMappingService) : base(
                broadcastDataRepositoryFactory,
                broadcastAudiencesCache,
                inventoryDaypartParsingEngine,
                mediaMonthAndWeekAggregateCache,
                stationProcessingEngine,
                spotLengthEngine,
                fileService,
                stationMappingService)
        {
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
        }

        public override void LoadAndValidateHeaderData(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            var header = new ProprietaryInventoryHeader();
            var validationProblems = new List<string>();

            _ValidateAndSetInventorySourceName(worksheet, validationProblems);
            _ValidateAndSetEffectiveAndEndDates(worksheet, validationProblems, header);
            _ValidateAndSetNTIToNSIIncrease(worksheet, validationProblems, header);

            proprietaryFile.Header = header;
            proprietaryFile.ValidationProblems.AddRange(validationProblems);
        }

        private void _ValidateAndSetEffectiveAndEndDates(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
        {
            var effectiveDateText = worksheet.Cells[EFFECTIVE_DATE_CELL.ToString()].GetTextValue();
            var endDateText = worksheet.Cells[END_DATE_CELL.ToString()].GetTextValue();
            var validDate = true;

            if (string.IsNullOrWhiteSpace(effectiveDateText))
            {
                var errorMessage = "Effective date is missing";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{EFFECTIVE_DATE_CELL.RowIndex}"].Value = errorMessage;
                validDate = false;
            }
            else
            {
                effectiveDateText = effectiveDateText.Split(' ')[0]; //split is removing time section
            }

            if (string.IsNullOrWhiteSpace(endDateText))
            {
                var errorMessage = "End date is missing";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{END_DATE_CELL.RowIndex}"].Value = errorMessage;
                validDate = false;
            }
            else
            {
                endDateText = endDateText.Split(' ')[0];
            }

            if (!validDate) return;

            if (!DateTime.TryParseExact(effectiveDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime effectiveDate))
            {
                var errorMessage = $"Effective date is not in the correct format ({(string.Join(", ", DATE_FORMATS))})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{EFFECTIVE_DATE_CELL.RowIndex}"].Value = errorMessage;
                validDate = false;
            }

            if (!DateTime.TryParseExact(endDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                var errorMessage = $"End date is not in the correct format ({(string.Join(", ", DATE_FORMATS))})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{END_DATE_CELL.RowIndex}"].Value = errorMessage;
                validDate = false;
            }

            if (!validDate) return;

            if (endDate <= effectiveDate)
            {
                var errorMessage = $"End date ({endDateText}) should be greater than effective date ({effectiveDateText})";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{END_DATE_CELL.RowIndex}"].Value = errorMessage;
                validDate = false;
            }

            if (validDate)
            {
                header.EffectiveDate = effectiveDate;
                header.EndDate = endDate;
            }
        }

        private void _ValidateAndSetNTIToNSIIncrease(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
        {
            var ntiToNsiIncreaseText = worksheet.Cells[NTI_TO_NSI_INCREASE_CELL.ToString()].GetTextValue();
            if (string.IsNullOrWhiteSpace(ntiToNsiIncreaseText))
            {
                var errorMessage = "NTI to NSI Increase is missing";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{NTI_TO_NSI_INCREASE_CELL.RowIndex}"].Value = errorMessage;
                return;
            }

            ntiToNsiIncreaseText = ntiToNsiIncreaseText.Replace("%", string.Empty);

            if (!decimal.TryParse(ntiToNsiIncreaseText, out var ntiToNsiIncrease))
            {
                var errorMessage = "Invalid NTI to NSI increase is specified";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{NTI_TO_NSI_INCREASE_CELL.RowIndex}"].Value = errorMessage;
            }
            else
            {
                header.NtiToNsiIncrease = ntiToNsiIncrease;
            }
        }

        private void _ValidateAndSetInventorySourceName(ExcelWorksheet worksheet, List<string> validationProblems)
        {
            var inventorySourceName = worksheet.Cells[3, 2].GetStringValue();
            if (string.IsNullOrWhiteSpace(inventorySourceName))
            {
                var errorMessage = "Inventory Source value is empty.";
                validationProblems.Add(errorMessage);
                worksheet.Cells[$"{HEADER_ERROR_COLUMN}{INVENTORY_SOURCE_CELL.RowIndex}"].Value = errorMessage;
                return;
            }
            InventorySourceName = inventorySourceName;
        }

        public override void LoadAndValidateSecondWorksheet(ExcelWorksheet worksheet,
            ProprietaryInventoryFile proprietaryFile)
        {
            const string stationsWorksheetName = "Diginet Stations";
            if (worksheet.Name.Equals(stationsWorksheetName) == false)
            {
                proprietaryFile.ValidationProblems.Add($"File '{proprietaryFile.FileName}' is a {proprietaryFile.InventorySource.Name} template that requires a second tab named '{stationsWorksheetName}'.");
                return;
            }

            // skip the header on line 1
            const int startingRowNumber = 2;
            var rowIndex = startingRowNumber;
            var lastRowIndex = worksheet.Dimension.End.Row;
            var fileStationsCallsigns = new List<string>();

            // ingest all
            // Call Sign | Station Name | Affiliate | TZ
            while (rowIndex <= lastRowIndex)
            {
                var stationAffiliate = worksheet.Cells[rowIndex, (int) StationColumnIndex.Affiliate].GetStringValue();
                if (InventorySourceName.Equals(stationAffiliate, StringComparison.OrdinalIgnoreCase) == false)
                {
                    rowIndex++;
                    continue;
                }

                fileStationsCallsigns.Add(worksheet.Cells[rowIndex, (int)StationColumnIndex.CallSign].GetStringValue());
                rowIndex++;
            }

            // store them in a list member var
            if (fileStationsCallsigns.Any() == false)
            {
                proprietaryFile.ValidationProblems.Add($"In file '{proprietaryFile.FileName}' the '{stationsWorksheetName}' tab didn't produce station information.");
                return;
            }

            FileStationsCallsigns = fileStationsCallsigns;
        }

        public override void LoadAndValidateDataLines(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            const int firstColumnIndex = 1;
            const int firstDataLineRowIndex = 12;
            const int audienceRowIndex = 11;
            var rowIndex = firstDataLineRowIndex;
            var columnIndex = firstColumnIndex;
            var lastRowIndex = worksheet.Dimension.End.Row;

            //the errors column is the second one after all the data columns (that's why I am doing +1 on the last column)
            _ErrorColumnIndex = _GetLastColumnWithValue(worksheet, audienceRowIndex) + 1;

            var audiences = _ReadAudiences(worksheet, out var audienceProblems);
            
            if (audienceProblems.Any())
            {
                proprietaryFile.ValidationProblems.AddRange(audienceProblems);
                worksheet.Cells[audienceRowIndex, _ErrorColumnIndex].Value = string.Join(FILE_MULTIPLE_ERRORS_SEPARATOR, audienceProblems);
                return;
            }

            if (!audiences.Where(x => x != null).Any(x => x.Code.Equals(BroadcastConstants.HOUSEHOLD_CODE, StringComparison.OrdinalIgnoreCase)))
            {
                var errorMessage = "File must contain data for HouseHolds(HH)";
                proprietaryFile.ValidationProblems.Add(errorMessage);
                worksheet.Cells[audienceRowIndex, _ErrorColumnIndex].Value = errorMessage;
                return;
            }

            while (rowIndex <= lastRowIndex)
            {
                if (_IsLineEmpty(worksheet, rowIndex, columnIndex, audiences))
                {
                    rowIndex++;
                    continue;
                }

                var line = _ReadAndValidateDataLine(worksheet, rowIndex, columnIndex, audiences, out var lineProblems);

                if (lineProblems.Any())
                {
                    proprietaryFile.ValidationProblems.AddRange(lineProblems);
                    worksheet.Cells[rowIndex, _ErrorColumnIndex].Value = string.Join(FILE_MULTIPLE_ERRORS_SEPARATOR, lineProblems);
                }
                else
                {
                    FileStationsCallsigns.ForEach(FileCallsign => proprietaryFile.DataLines.Add(new ProprietaryInventoryDataLine
                    {
                        Station = FileCallsign,
                        Comment = line.Comment,
                        Program = line.Program,
                        Impressions = line.Impressions,
                        CPM = line.CPM,
                        SpotCost = line.SpotCost,
                        Dayparts = line.Dayparts,
                        Units = line.Units,
                        Weeks = line.Weeks,
                        Audiences = line.Audiences,
                        RowIndex = line.RowIndex
                    }));
                }
                
                columnIndex = firstColumnIndex;
                rowIndex++;
            }
        }

        private ProprietaryInventoryDataLine _ReadAndValidateDataLine(
            ExcelWorksheet worksheet
            , int rowIndex
            , int columnIndex
            , List<BroadcastAudience> audiences
            ,out List<string> problems)
        {
            problems = new List<string>();
            var line = new ProprietaryInventoryDataLine();

            _ValidateAndSetDayparts(worksheet, rowIndex, columnIndex++, line, problems);
            _ValidateAndSetSpotCost(worksheet, rowIndex, columnIndex++, line, problems);

            foreach (var audience in audiences)
            {
                if (audience == null)
                {
                    columnIndex += 2;
                    continue;
                }

                var lineAudience = new LineAudience { Audience = audience.ToDisplayAudience() };

                var ratingCellColumnIndex = columnIndex++;
                var impressionsCellColumnIndex = columnIndex++;

                var ratingCellText = worksheet.Cells[rowIndex, ratingCellColumnIndex].GetStringValue();
                var impressionsCellText = worksheet.Cells[rowIndex, impressionsCellColumnIndex].GetStringValue();

                // Skip if both cells are empty. See PRI-8905
                if (string.IsNullOrEmpty(ratingCellText) && string.IsNullOrEmpty(impressionsCellText))
                {
                    continue;
                }

                lineAudience.Rating = _ParseAndValidateCellValue(ratingCellText, rowIndex, ratingCellColumnIndex, problems, "rating");
                lineAudience.Impressions = _ParseAndValidateCellValue(impressionsCellText, rowIndex, impressionsCellColumnIndex, problems, "impressions");
                
                if (lineAudience.Rating.HasValue && lineAudience.Impressions.HasValue)
                {
                    line.Audiences.Add(lineAudience);
                }
            }
            return line;
        }

        private void _ValidateAndSetDayparts(ExcelWorksheet worksheet, int rowIndex, int columnIndex, ProprietaryInventoryDataLine line, List<string> problems)
        {
            var daypartText = worksheet.Cells[rowIndex, columnIndex].GetStringValue();

            if (string.IsNullOrEmpty(daypartText))
            {
                problems.Add($"Line {rowIndex} contains an empty daypart cell in column {columnIndex.GetColumnAdress()}");
            }
            else if (!DaypartParsingEngine.TryParse(daypartText, out var dayparts))
            {
                problems.Add($"Line {rowIndex} contains an invalid daypart(s): {daypartText} in column {columnIndex.GetColumnAdress()}");
            }
            else
            {
                line.Dayparts = dayparts;
            }
        }

        private void _ValidateAndSetSpotCost(ExcelWorksheet worksheet, int rowIndex, int columnIndex, ProprietaryInventoryDataLine line, List<string> problems)
        {
            var spotCostText = worksheet.Cells[rowIndex, columnIndex].GetStringValue();

            if (string.IsNullOrEmpty(spotCostText))
            {
                problems.Add($"Line {rowIndex} contains an empty rate cell in column {columnIndex.GetColumnAdress()}");
            }
            else if (!decimal.TryParse(spotCostText, out var spotCost))
            {
                problems.Add($"Line {rowIndex} contains an invalid rate value: ({spotCostText}) in column {columnIndex.GetColumnAdress()}");
            }
            else if (spotCost < 0)
            {
                problems.Add($"Line {rowIndex} contains a negative rate value: ({spotCost}) in column {columnIndex.GetColumnAdress()}");
            }
            else
            {
                line.SpotCost = spotCost;
            }
        }

        private double? _ParseAndValidateCellValue(string cellText, int rowIndex, int columnIndex, List<string> problems, string cellName)
        {
            if (string.IsNullOrEmpty(cellText))
            {
                problems.Add($"Line {rowIndex} contains an empty ({cellName}) cell in column {columnIndex.GetColumnAdress()}");
            }
            else if (!double.TryParse(cellText, out var cellValue))
            {
                problems.Add($"Line {rowIndex} contains an invalid ({cellName}) value in column {columnIndex.GetColumnAdress()}: {cellText}");
            }
            else if (cellValue < 0)
            {
                problems.Add($"Line {rowIndex} contains a negative ({cellName}) value in column {columnIndex.GetColumnAdress()}: {cellValue}");
            }
            else
            {
                return cellValue;
            }

            return null;
        }

        private bool _IsLineEmpty(ExcelWorksheet worksheet, int rowIndex, int columnIndex, List<BroadcastAudience> audiences)
        {
            // Headers sequence: Daypart -> Rate [-> [Demo] Rtg -> [Demo] Imps]
            // Rtg and Imps columns are repeated for each audience
            var cellsToCheck = 2 + audiences.Count * 2;

            for (var i = 0; i < cellsToCheck; i++)
            {
                if (!string.IsNullOrWhiteSpace(worksheet.Cells[rowIndex, columnIndex + i].GetStringValue()))
                {
                    return false;
                }
            }

            return true;
        }

        public override void PopulateManifests(ProprietaryInventoryFile proprietaryFile, List<DisplayBroadcastStation> stations)
        {
            const int defaultSpotLength = 30;
            const int defaultSpotsNumberPerWeek = 1;
            var fileHeader = proprietaryFile.Header;
            var ntiToNsiIncreaseInDecimals = (double)(fileHeader.NtiToNsiIncrease.Value / 100);
            var defaultSpotLengthId = SpotLengthEngine.GetSpotLengthIdByValue(defaultSpotLength);
            var allDaypartCodes = _DaypartDefaultRepository.GetAllActiveDaypartDefaults();

            proprietaryFile.InventoryManifests = proprietaryFile.DataLines.Select(x =>
                new StationInventoryManifest
                {
                    InventorySourceId = proprietaryFile.InventorySource.Id,
                    InventoryFileId = proprietaryFile.Id,
                    SpotLengthId = defaultSpotLengthId,
                    ManifestDayparts = x.Dayparts.Select(d => new StationInventoryManifestDaypart
                    {
                        Daypart = d,
                        DaypartDefault = _MapToDaypartCode(d, allDaypartCodes)
                    }).ToList(),
                    ManifestWeeks = GetManifestWeeksInRange(fileHeader.EffectiveDate, fileHeader.EndDate,
                        defaultSpotsNumberPerWeek),
                    ManifestAudiences = x.Audiences.Select(a => new StationInventoryManifestAudience
                    {
                        Audience = a.Audience,
                        IsReference = true,
                        Impressions = _ImpressionAdjustmentEngine.ConvertNtiImpressionsToNsi(a.Impressions.Value * 1000,
                            ntiToNsiIncreaseInDecimals),
                        Rating = a.Rating
                    }).ToList(),
                    ManifestRates = new List<StationInventoryManifestRate>
                    {
                        new StationInventoryManifestRate
                        {
                            SpotLengthId = defaultSpotLengthId,
                            SpotCost = x.SpotCost.Value
                        }
                    },
                    Station = _StationMappingService.GetStationByCallLetters(x.Station)
                }
            ).ToList();
        }

        private DaypartDefaultDto _MapToDaypartCode(DisplayDaypart displayDaypart, List<DaypartDefaultDto> allDaypartDefaults)
        {
            var startHour = int.Parse(displayDaypart.StartHourFullPercision);
            var endHour = int.Parse(displayDaypart.EndHourFullPercision);

            var daypartCode = _IsBetween(startHour, 2, 5) ? "OVN" :
                              _IsBetween(startHour, 6, 8) ? "EM" :
                              
                              // for program that overlap 3p-4p
                              _IsBetween(startHour, 9, 14) ? "DAY" :
                              startHour == 15 && endHour == 16 ? "DAY" :
                              startHour == 15 && _IsAfter(endHour, 16) ? "EF" :
                              startHour == 15 && _IsBefore(endHour, 16) ? "EF" :
                              _IsBetween(startHour, 16, 17) ? "EF" :
                              
                              _IsBetween(startHour, 18, 19) ? "PA" :
                              _IsBetween(startHour, 20, 22) ? "PT" :
                              startHour == 23 ? "LF" :
                              _IsBetween(startHour, 0, 1) ? "LF" : string.Empty;

            return allDaypartDefaults.SingleOrDefault(dc => dc.Code == daypartCode);
        }

        private bool _IsBetween(int hour, int start, int end)
        {
            return hour >= start && hour <= end;
        }

        private bool _IsBefore(int hour, int beforeHour)
        {
            return hour < beforeHour;
        }

        private bool _IsAfter(int hour, int afterHour)
        {
            return hour > afterHour;
        }
                
        private List<BroadcastAudience> _ReadAudiences(ExcelWorksheet worksheet, out List<string> validationProblems)
        {
            validationProblems = new List<string>();            
            var result = new List<BroadcastAudience>();
            var currentAudienceColumnIndex = 3;
            var ratingRegex = new Regex(@"(?<Audience>[a-z0-9\s-+\[\]]+)\sRtg.{0,}", RegexOptions.IgnoreCase);
            var impressionsRegex = new Regex(@"(?<Audience>[a-z0-9\s-+\[\]]+)\sImps\s*\(000\).{0,}", RegexOptions.IgnoreCase);

            while (currentAudienceColumnIndex < _ErrorColumnIndex - 1)
            {                
                var ratingColumnIndex = currentAudienceColumnIndex;
                var impressionsColumnIndex = currentAudienceColumnIndex + 1;

                var ratingText = worksheet.Cells[audienceRowIndex, ratingColumnIndex].GetStringValue();
                var impressionsText = worksheet.Cells[audienceRowIndex, impressionsColumnIndex].GetStringValue();

                var invalidAudience = false;

                if (string.IsNullOrWhiteSpace(ratingText))
                {
                    validationProblems.Add($"Rating header is expected. Row: {audienceRowIndex}, column: {ratingColumnIndex.GetColumnAdress()}");
                    invalidAudience = true;
                }

                if (string.IsNullOrWhiteSpace(impressionsText))
                {
                    validationProblems.Add($"Impressions header is expected. Row: {audienceRowIndex}, column: {impressionsColumnIndex.GetColumnAdress()}");
                    invalidAudience = true;
                }

                if (invalidAudience)
                    break;      

                var ratingMatch = ratingRegex.Match(ratingText);
                var impressionsMatch = impressionsRegex.Match(impressionsText);

                if (!ratingMatch.Success)
                {
                    validationProblems.Add($"Rating header is incorrect: {ratingText}. Row: {audienceRowIndex}, column: {ratingColumnIndex.GetColumnAdress()}. Correct format: '[DEMO] Rtg'");
                    invalidAudience = true;
                }

                if (!impressionsMatch.Success)
                {
                    validationProblems.Add($"Impressions header is incorrect: {impressionsText}. Row: {audienceRowIndex}, column: {impressionsColumnIndex.GetColumnAdress()}. Correct format: '[DEMO] Imps (000)'");
                    invalidAudience = true;
                }

                if (invalidAudience)
                    break;

                var ratingAudience = ratingMatch.Groups["Audience"].Value.RemoveWhiteSpaces();
                var impressionsAudience = impressionsMatch.Groups["Audience"].Value.RemoveWhiteSpaces();

                if (ratingAudience != impressionsAudience)
                {
                    validationProblems.Add($"Audience '{impressionsAudience}' from cell(row: {audienceRowIndex}, column: {impressionsColumnIndex.GetColumnAdress()}) should be the same as audience '{ratingAudience}' from cell(row: {audienceRowIndex}, column: {ratingColumnIndex.GetColumnAdress()})");
                    break;
                }

                if (ratingAudience.Equals("[DEMO]", StringComparison.OrdinalIgnoreCase))
                {
                    // null is used later to skip [DEMO] columns
                    result.Add(null);
                }
                else
                {
                    var audience = AudienceCache.GetBroadcastAudienceByCode(ratingAudience);

                    if (audience == null)
                    {
                        validationProblems.Add($"Unknown audience is specified: {ratingAudience}. Row: {audienceRowIndex}, column: {ratingColumnIndex.GetColumnAdress()}");
                        break;
                    }

                    if (result.Where(x => x != null).Any(x => x.Code == audience.Code))
                    {
                        validationProblems.Add($"Data for audience '{audience.Code}' have been already read. Please specify unique audiences. Row: {audienceRowIndex}, column: {ratingColumnIndex.GetColumnAdress()}");
                        break;
                    }

                    result.Add(audience);
                }

                currentAudienceColumnIndex = currentAudienceColumnIndex + 2;
            }

            return result;
        }

        private bool _ShouldStopReadingAudiences(ExcelWorksheet worksheet, int rowIndex, int columnIndex)
        {
            // stop reading if current cell and next 3 cells are empty
            for (var i = 0; i < 4; i++)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(worksheet.Cells[rowIndex, columnIndex + i].GetStringValue()))
                    {
                        return false;
                    }
                }
                catch
                {
                    // end of file is reached
                    return true;
                }
            }

            return true;  
        }

        private int _GetLastColumnWithValue(ExcelWorksheet worksheet, int rowindex)
        {
            int currentColumnIndex = 1;
            while (currentColumnIndex <= worksheet.Dimension.Columns)
            {
                if (_ShouldStopReadingAudiences(worksheet, rowindex, currentColumnIndex))
                {
                    return currentColumnIndex;
                }
                else
                {
                    currentColumnIndex++;
                }
            }
            return currentColumnIndex;
        }
    }
}
