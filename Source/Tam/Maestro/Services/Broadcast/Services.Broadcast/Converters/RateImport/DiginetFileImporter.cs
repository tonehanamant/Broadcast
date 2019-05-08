using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Services;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using static Services.Broadcast.Entities.ProprietaryInventory.ProprietaryInventoryFile;
using static Services.Broadcast.Entities.ProprietaryInventory.ProprietaryInventoryFile.ProprietaryInventoryDataLine;

namespace Services.Broadcast.Converters.RateImport
{
    public class DiginetFileImporter : ProprietaryFileImporterBase
    {
        private const string EFFECTIVE_DATE_CELL = "B4";
        private const string END_DATE_CELL = "B5";
        private const string NTI_TO_NSI_INCREASE_CELL = "B6";
        private const string DEFAULT_DAYPART_CODE = "DIGI";

        private readonly IDaypartCache _DaypartCache;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;

        public DiginetFileImporter(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine,
            IDaypartCache daypartCache,
            IImpressionAdjustmentEngine impressionAdjustmentEngine) : base(
                broadcastDataRepositoryFactory,
                broadcastAudiencesCache,
                inventoryDaypartParsingEngine,
                mediaMonthAndWeekAggregateCache,
                stationProcessingEngine,
                spotLengthEngine)
        {
            _DaypartCache = daypartCache;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
        }

        public override void LoadAndValidateDataLines(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            const int firstColumnIndex = 1;
            const int firstDataLineRowIndex = 12;
            var rowIndex = firstDataLineRowIndex;
            var columnIndex = firstColumnIndex;
            var lastRowIndex = worksheet.Dimension.End.Row;
            var audiences = _ReadAudiences(worksheet, out var audienceProblems);
            
            if (audienceProblems.Any())
            {
                proprietaryFile.ValidationProblems.AddRange(audienceProblems);
                return;
            }
            else if (!audiences.Where(x => x != null).Any(x => x.Code.Equals(BroadcastConstants.HOUSEHOLD_CODE, StringComparison.OrdinalIgnoreCase)))
            {
                proprietaryFile.ValidationProblems.Add("File must contain data for House Holds(HH)");
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
                }
                else
                {
                    proprietaryFile.DataLines.Add(line);
                }
                
                columnIndex = firstColumnIndex;
                rowIndex++;
            }
        }

        private ProprietaryInventoryDataLine _ReadAndValidateDataLine(
            ExcelWorksheet worksheet, 
            int rowIndex, 
            int columnIndex, 
            List<BroadcastAudience> audiences,
            out List<string> problems)
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
                problems.Add($"Line {rowIndex} contains an empty daypart cell");
            }
            else if (!DaypartParsingEngine.TryParse(daypartText, out var dayparts))
            {
                problems.Add($"Line {rowIndex} contains an invalid daypart(s): {daypartText}");
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
                problems.Add($"Line {rowIndex} contains an empty rate cell");
            }
            else if (!decimal.TryParse(spotCostText, out var spotCost))
            {
                problems.Add($"Line {rowIndex} contains an invalid rate value: {spotCostText}");
            }
            else if (spotCost < 0)
            {
                problems.Add($"Line {rowIndex} contains a negative rate value: {spotCost}");
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
                problems.Add($"Line {rowIndex} contains an empty {cellName} cell in column {columnIndex}");
            }
            else if (!double.TryParse(cellText, out var cellValue))
            {
                problems.Add($"Line {rowIndex} contains an invalid {cellName} value in column {columnIndex}: {cellText}");
            }
            else if (cellValue < 0)
            {
                problems.Add($"Line {rowIndex} contains a negative {cellName} value in column {columnIndex}: {cellValue}");
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

            proprietaryFile.InventoryManifests = proprietaryFile.DataLines
                .Select(x => new StationInventoryManifest
                {
                    EffectiveDate = fileHeader.EffectiveDate,
                    EndDate = fileHeader.EndDate,
                    InventorySourceId = proprietaryFile.InventorySource.Id,
                    InventoryFileId = proprietaryFile.Id,
                    SpotLengthId = defaultSpotLengthId,
                    DaypartCode = fileHeader.DaypartCode,
                    ManifestDayparts = x.Dayparts.Select(d => new StationInventoryManifestDaypart { Daypart = d }).ToList(),
                    ManifestWeeks = GetManifestWeeksInRange(fileHeader.EffectiveDate, fileHeader.EndDate, defaultSpotsNumberPerWeek),
                    ManifestAudiences = x.Audiences.Select(a => new StationInventoryManifestAudience
                    {
                        Audience = a.Audience,
                        IsReference = true,
                        Impressions = _ImpressionAdjustmentEngine.ConvertNtiImpressionsToNsi(a.Impressions.Value * 1000, ntiToNsiIncreaseInDecimals),
                        Rating = a.Rating
                    }).ToList(),
                    ManifestRates = new List<StationInventoryManifestRate>
                    {
                        new StationInventoryManifestRate
                        {
                            SpotLengthId = defaultSpotLengthId,
                            SpotCost = x.SpotCost.Value
                        }
                    }
                }).ToList();
        }

        protected override void LoadAndValidateHeaderData(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            var header = new ProprietaryInventoryHeader { DaypartCode = DEFAULT_DAYPART_CODE };
            var validationProblems = new List<string>();

            _ValidateAndSetEffectiveAndEndDates(worksheet, validationProblems, header);
            _ValidateAndSetNTIToNSIIncrease(worksheet, validationProblems, header);

            proprietaryFile.Header = header;
            proprietaryFile.ValidationProblems.AddRange(validationProblems);
        }

        private void _ValidateAndSetEffectiveAndEndDates(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
        {
            var effectiveDateText = worksheet.Cells[EFFECTIVE_DATE_CELL].GetTextValue();
            var endDateText = worksheet.Cells[END_DATE_CELL].GetTextValue();
            var validDate = true;

            if (string.IsNullOrWhiteSpace(effectiveDateText))
            {
                validationProblems.Add($"Effective date is missing");
                validDate = false;
            }
            else
            {
                effectiveDateText = effectiveDateText.Split(' ')[0]; //split is removing time section
            }

            if (string.IsNullOrWhiteSpace(endDateText))
            {
                validationProblems.Add($"End date is missing");
                validDate = false;
            }
            else
            {
                endDateText = endDateText.Split(' ')[0];
            }

            if (!validDate) return;

            if (!DateTime.TryParseExact(effectiveDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime effectiveDate))
            {
                validationProblems.Add($"Effective date is not in the correct format ({(string.Join(", ", DATE_FORMATS))})");
                validDate = false;
            }

            if (!DateTime.TryParseExact(endDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                validationProblems.Add($"End date is not in the correct format ({(string.Join(", ", DATE_FORMATS))})");
                validDate = false;
            }

            if (!validDate) return;

            if (endDate <= effectiveDate)
            {
                validationProblems.Add($"End date ({endDateText}) should be greater than effective date ({effectiveDateText})");
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
            var ntiToNsiIncreaseText = worksheet.Cells[NTI_TO_NSI_INCREASE_CELL].GetTextValue();

            if (string.IsNullOrWhiteSpace(ntiToNsiIncreaseText))
            {
                validationProblems.Add("NTI to NSI Increase is missing");
                return;
            }

            ntiToNsiIncreaseText = ntiToNsiIncreaseText.Replace("%", string.Empty);

            if (!decimal.TryParse(ntiToNsiIncreaseText, out var ntiToNsiIncrease))
            {
                validationProblems.Add("Invalid NTI to NSI increase is specified");
            }
            else
            {
                header.NtiToNsiIncrease = ntiToNsiIncrease;
            }
        }

        private List<BroadcastAudience> _ReadAudiences(ExcelWorksheet worksheet, out List<string> validationProblems)
        {
            validationProblems = new List<string>();
            const int audienceRowIndex = 11;
            var result = new List<BroadcastAudience>();
            var currentAudienceColumnIndex = 3;
            var ratingRegex = new Regex(@"(?<Audience>[a-z0-9\s-\[\]]+)\sRtg.*", RegexOptions.IgnoreCase);
            var impressionsRegex = new Regex(@"(?<Audience>[a-z0-9\s-\[\]]+)\sImps\s*\(000\).*", RegexOptions.IgnoreCase);

            while (true)
            {
                if (_ShouldStopReadingAudiences(worksheet, audienceRowIndex, currentAudienceColumnIndex))
                    break;

                var ratingColumnIndex = currentAudienceColumnIndex;
                var impressionsColumnIndex = currentAudienceColumnIndex + 1;

                var ratingText = worksheet.Cells[audienceRowIndex, ratingColumnIndex].GetStringValue();
                var impressionsText = worksheet.Cells[audienceRowIndex, impressionsColumnIndex].GetStringValue();

                var invalidAudience = false;

                if (string.IsNullOrWhiteSpace(ratingText))
                {
                    validationProblems.Add($"Rating header is expected. Row: {audienceRowIndex}, column: {ratingColumnIndex}");
                    invalidAudience = true;
                }

                if (string.IsNullOrWhiteSpace(impressionsText))
                {
                    validationProblems.Add($"Impressions header is expected. Row: {audienceRowIndex}, column: {impressionsColumnIndex}");
                    invalidAudience = true;
                }

                if (invalidAudience)
                    break;

                var ratingMatch = ratingRegex.Match(ratingText);
                var impressionsMatch = impressionsRegex.Match(impressionsText);

                if (!ratingMatch.Success)
                {
                    validationProblems.Add($"Rating header is incorrect: {ratingText}. Row: {audienceRowIndex}, column: {ratingColumnIndex}. Correct format: '[DEMO] Rtg'");
                    invalidAudience = true;
                }

                if (!impressionsMatch.Success)
                {
                    validationProblems.Add($"Impressions header is incorrect: {impressionsText}. Row: {audienceRowIndex}, column: {impressionsColumnIndex}. Correct format: '[DEMO] Imps (000)'");
                    invalidAudience = true;
                }

                if (invalidAudience)
                    break;

                var ratingAudience = ratingMatch.Groups["Audience"].Value.RemoveWhiteSpaces();
                var impressionsAudience = impressionsMatch.Groups["Audience"].Value.RemoveWhiteSpaces();

                if (ratingAudience != impressionsAudience)
                {
                    validationProblems.Add($"Audience '{impressionsAudience}' from cell(row: {audienceRowIndex}, column: {impressionsColumnIndex}) should be the same as audience '{ratingAudience}' from cell(row: {audienceRowIndex}, column: {ratingColumnIndex})");
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
                        validationProblems.Add($"Unknown audience is specified: {ratingAudience}. Row: {audienceRowIndex}, column: {ratingColumnIndex}");
                        break;
                    }

                    if (result.Where(x => x != null).Any(x => x.Code == audience.Code))
                    {
                        validationProblems.Add($"Data for audience '{audience.Code}' have been already read. Please specify unique audiences. Row: {audienceRowIndex}, column: {ratingColumnIndex}");
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
    }
}
