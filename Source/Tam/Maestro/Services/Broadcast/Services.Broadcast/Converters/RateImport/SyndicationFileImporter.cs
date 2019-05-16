using Common.Services;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
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
    public class SyndicationFileImporter : ProprietaryFileImporterBase
    {
        private const string EFFECTIVE_DATE_CELL = "B4";
        private const string END_DATE_CELL = "B5";
        private const string DAYPART_CODE_CELL = "B6";
        private const string NTI_TO_NSI_INCREASE_CELL = "B7";
        private const string SHARE_BOOK_CELL = "B8";
        private const string HUT_BOOK_CELL = "B9";
        private const string PLAYBACK_TYPE_CELL = "B10";

        private readonly IDaypartCache _DaypartCache;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;

        public SyndicationFileImporter(
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
            const int firstDataLineRowIndex = 16;
            const int emptyLinesLimitToStopProcessing = 5;
            var rowIndex = firstDataLineRowIndex;
            var columnIndex = firstColumnIndex;
            var audienceProblems = new List<string>();
            var audiences = new List<BroadcastAudience>();
            var householdAudience = _ReadHouseholdAudience(worksheet, audienceProblems);
            
            if (householdAudience != null)
                audiences.Add(householdAudience);

            audiences.AddRange(_ReadAudiences(worksheet, audienceProblems));

            if (audienceProblems.Any())
            {
                proprietaryFile.ValidationProblems.AddRange(audienceProblems);
                return;
            }
            else if (!audiences.Any(x => x.Code.Equals(BroadcastConstants.HOUSEHOLD_CODE, StringComparison.OrdinalIgnoreCase)))
            {
                proprietaryFile.ValidationProblems.Add("File must contain data for House Holds(HH)");
                return;
            }

            var daypartId = _DaypartCache.GetIdByDaypart(_GetDefaultDaypart());
            var daypartFoLines = _DaypartCache.GetDisplayDaypart(daypartId);
            var emptyLinesProcessedAfterLastDataLine = 0;

            while (true)
            {
                if (_IsLineEmpty(worksheet, rowIndex, columnIndex, audiences))
                {
                    emptyLinesProcessedAfterLastDataLine++;

                    if (emptyLinesProcessedAfterLastDataLine == emptyLinesLimitToStopProcessing)
                    {
                        break;
                    }
                    else
                    {
                        rowIndex++;
                        continue;
                    }
                }

                var line = _ReadAndValidateDataLine(worksheet, rowIndex, columnIndex, audiences, daypartFoLines, out var lineProblems);

                if (_IsDuplicateLine(line, proprietaryFile.DataLines, out int duplicateRowIndex))
                {
                    proprietaryFile.ValidationProblems.Add($"File contains a duplicate line on row {rowIndex} and {duplicateRowIndex}, program name '{line.Program}'");
                }

                if (lineProblems.Any())
                {
                    proprietaryFile.ValidationProblems.AddRange(lineProblems);
                }
                else
                {
                    proprietaryFile.DataLines.Add(line);
                }

                emptyLinesProcessedAfterLastDataLine = 0;
                columnIndex = firstColumnIndex;
                rowIndex++;
            }
        }

        private bool _IsDuplicateLine(ProprietaryInventoryDataLine line, List<ProprietaryInventoryDataLine> dataLines, out int duplicateRowIndex)
        {
            var duplicateLines = dataLines.Where(x => x.Program == line.Program &&
                                                      x.SpotCost == line.SpotCost);

            foreach(var duplicateLine in duplicateLines)
            {
                var duplicateLineAudiences = duplicateLine.Audiences.OrderBy(x => x.Audience.Id).ToList();
                var lineAudiences = line.Audiences.OrderBy(x => x.Audience.Id).ToList();

                if (duplicateLineAudiences.Count() != lineAudiences.Count())
                    continue;

                var allAudiencesEqual = true;

                for (var index = 0; index < duplicateLineAudiences.Count(); index++)
                {
                    if (duplicateLineAudiences[index].Audience.Id != lineAudiences[index].Audience.Id ||
                        duplicateLineAudiences[index].Cpm != lineAudiences[index].Cpm ||
                        duplicateLineAudiences[index].Impressions != lineAudiences[index].Impressions ||
                        duplicateLineAudiences[index].Rating != lineAudiences[index].Rating ||
                        duplicateLineAudiences[index].Vpvh != lineAudiences[index].Vpvh)
                    {
                        allAudiencesEqual = false;
                        break;
                    }
                }

                if (allAudiencesEqual)
                {
                    duplicateRowIndex = duplicateLine.RowIndex;
                    return true;
                }
            }

            duplicateRowIndex = 0;
            return false;
        }

        private ProprietaryInventoryDataLine _ReadAndValidateDataLine(
            ExcelWorksheet worksheet,
            int rowIndex,
            int columnIndex,
            List<BroadcastAudience> audiences,
            DisplayDaypart daypart,
            out List<string> problems)
        {
            problems = new List<string>();
            var line = new ProprietaryInventoryDataLine
            {
                RowIndex = rowIndex
            };

            _ValidateAndSetProgramName(worksheet, rowIndex, columnIndex++, line, problems);
            _ValidateAndSetSpotCost(worksheet, rowIndex, columnIndex++, line, problems);

            line.Dayparts = new List<DisplayDaypart>
            {
                daypart
            };

            foreach (var audience in audiences)
            {
                var lineAudience = new LineAudience { Audience = audience.ToDisplayAudience() };

                lineAudience.Rating = _GetAndValidateDoubleCellValue(worksheet, rowIndex, columnIndex++, problems, "rating");
                lineAudience.Impressions = _GetAndValidateDoubleCellValue(worksheet, rowIndex, columnIndex++, problems, "impressions");

                if (audience.Code != BroadcastConstants.HOUSEHOLD_CODE)
                {
                    lineAudience.Vpvh = _GetAndValidateDoubleCellValue(worksheet, rowIndex, columnIndex++, problems, "VPVH");
                }

                lineAudience.Cpm = _GetAndValidateDecimalCellValue(worksheet, rowIndex, columnIndex++, problems, "CPM");

                var hasVpvhOrHousehold = (lineAudience.Vpvh.HasValue || audience.Code == BroadcastConstants.HOUSEHOLD_CODE);

                if (lineAudience.Rating.HasValue && 
                    lineAudience.Impressions.HasValue &&
                    lineAudience.Cpm.HasValue &&
                    hasVpvhOrHousehold)
                {
                    line.Audiences.Add(lineAudience);
                }
            }

            return line;
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

        private void _ValidateAndSetProgramName(ExcelWorksheet worksheet, int rowIndex, int columnIndex, ProprietaryInventoryDataLine line, List<string> problems)
        {
            var programText = worksheet.Cells[rowIndex, columnIndex].GetStringValue();

            if (string.IsNullOrEmpty(programText))
            {
                problems.Add($"Line {rowIndex} contains an empty program cell");
            }
            else            
            {
                line.Program = programText;
            }
        }

        private double? _GetAndValidateDoubleCellValue(ExcelWorksheet worksheet, int rowIndex, int columnIndex, List<string> problems, string cellName)
        {
            var cellText = worksheet.Cells[rowIndex, columnIndex].GetStringValue();

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

        private decimal? _GetAndValidateDecimalCellValue(ExcelWorksheet worksheet, int rowIndex, int columnIndex, List<string> problems, string cellName)
        {
            var cellText = worksheet.Cells[rowIndex, columnIndex].GetStringValue();

            if (string.IsNullOrEmpty(cellText))
            {
                problems.Add($"Line {rowIndex} contains an empty {cellName} cell in column {columnIndex}");
            }
            else if (!decimal.TryParse(cellText, out var cellValue))
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
                    ManifestDayparts = x.Dayparts.Select(d => new StationInventoryManifestDaypart { ProgramName = x.Program, Daypart = d }).ToList(),
                    ManifestWeeks = GetManifestWeeksInRange(fileHeader.EffectiveDate, fileHeader.EndDate, defaultSpotsNumberPerWeek),
                    ManifestAudiences = x.Audiences.Select(a => new StationInventoryManifestAudience
                    {
                        Audience = a.Audience,
                        IsReference = true,
                        Impressions = _ImpressionAdjustmentEngine.ConvertNtiImpressionsToNsi(a.Impressions.Value * 1000, ntiToNsiIncreaseInDecimals),
                        Rating = a.Rating,
                        CPM = a.Cpm,
                        Vpvh = a.Vpvh
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
            var header = new ProprietaryInventoryHeader();
            var validationProblems = new List<string>();
            
            _ProcessEffectiveAndEndDates(worksheet, validationProblems, header);
            _ProcessDaypartCode(worksheet, validationProblems, header);
            _ProcessContractedDaypart(validationProblems, header);
            _ProcessNTIToNSIIncrease(worksheet, validationProblems, header);
            _ProcessShareBook(worksheet, validationProblems, header, out var shareBookParsedCorrectly, out var shareBook);
            _ProcessHutBook(worksheet, validationProblems, header, shareBookParsedCorrectly, shareBook);
            _ProcessPlaybackType(worksheet, validationProblems, header);

            proprietaryFile.Header = header;
            proprietaryFile.ValidationProblems.AddRange(validationProblems);
        }

        private BroadcastAudience _ReadHouseholdAudience(ExcelWorksheet worksheet, List<string> validationProblems)
        {
            const int audienceRowIndex = 15;
            var currentAudienceColumnIndex = 3;
            var ratingRegex = new Regex(@"Avg\s*(?<Audience>[a-z0-9\s-]+)\s*Rtg.*", RegexOptions.IgnoreCase);
            var impressionsRegex = new Regex(@"Avg\s*(?<Audience>[a-z0-9\s-]+)\s*Imps\s*\(000\).*", RegexOptions.IgnoreCase);
            var cpmRegex = new Regex(@"Avg\s*(?<Audience>[a-z0-9\s-]+)\s*CPM.*", RegexOptions.IgnoreCase);
            var ratingColumnIndex = currentAudienceColumnIndex;
            var impressionsColumnIndex = currentAudienceColumnIndex + 1;
            var cpmColumnIndex = impressionsColumnIndex + 1;
            var ratingText = worksheet.Cells[audienceRowIndex, ratingColumnIndex].GetStringValue();
            var impressionsText = worksheet.Cells[audienceRowIndex, impressionsColumnIndex].GetStringValue();
            var cpmText = worksheet.Cells[audienceRowIndex, cpmColumnIndex].GetStringValue();
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

            if (string.IsNullOrWhiteSpace(cpmText))
            {
                validationProblems.Add($"CPM header is expected. Row: {audienceRowIndex}, column: {cpmColumnIndex}");
                invalidAudience = true;
            }

            if (invalidAudience)
                return null;

            var ratingMatch = ratingRegex.Match(ratingText);
            var impressionsMatch = impressionsRegex.Match(impressionsText);
            var cpmMatch = cpmRegex.Match(cpmText);

            if (!ratingMatch.Success)
            {
                validationProblems.Add($"Rating header is incorrect: {ratingText}. Row: {audienceRowIndex}, column: {ratingColumnIndex}. Correct format: 'Avg HH Rtg'");
                invalidAudience = true; 
            }

            if (!impressionsMatch.Success)
            {
                validationProblems.Add($"Impressions header is incorrect: {impressionsText}. Row: {audienceRowIndex}, column: {impressionsColumnIndex}. Correct format: 'Avg HH Imps (000)'");
                invalidAudience = true;
            }

            if (!impressionsMatch.Success)
            {
                validationProblems.Add($"CPM header is incorrect: {impressionsText}. Row: {audienceRowIndex}, column: {impressionsColumnIndex}. Correct format: 'Avg HH CPM'");
                invalidAudience = true;
            }

            if (invalidAudience)
                return null;

            var ratingAudience = ratingMatch.Groups["Audience"].Value.RemoveWhiteSpaces();
            var impressionsAudience = impressionsMatch.Groups["Audience"].Value.RemoveWhiteSpaces();
            var cpmAudience = cpmMatch.Groups["Audience"].Value.RemoveWhiteSpaces();

            if (ratingAudience != impressionsAudience ||
                ratingAudience != cpmAudience)
            {
                validationProblems.Add($"Audience '{ratingAudience}' from cell(row: {audienceRowIndex}, column: {ratingAudience}) should be the same as the audience found in the next two columns (Impressions and CPM)");
                return null;
            }

            var audience = AudienceCache.GetBroadcastAudienceByCode(ratingAudience);

            if (audience == null)
            {
                validationProblems.Add($"Unknown audience is specified: {ratingAudience}. Row: {audienceRowIndex}, column: {ratingColumnIndex}");
            }
            
            return audience;
        }

        private List<BroadcastAudience> _ReadAudiences(ExcelWorksheet worksheet, List<string> validationProblems)
        {
            const int audienceRowIndex = 15;
            var result = new List<BroadcastAudience>();
            var currentAudienceColumnIndex = 6;
            var ratingRegex = new Regex(@"(?<Audience>[a-z0-9\s-]+)\sAvg\s*Rtg.*", RegexOptions.IgnoreCase);
            var impressionsRegex = new Regex(@"(?<Audience>[a-z0-9\s-]+)\s*Avg\s*Imps\s*\(000\).*", RegexOptions.IgnoreCase);
            var vpvhRegex = new Regex(@"(?<Audience>[a-z0-9\s-]+)\s*VPVH.*", RegexOptions.IgnoreCase);
            var cpmRegex = new Regex(@"(?<Audience>[a-z0-9\s-]+)\s*CPM.*", RegexOptions.IgnoreCase);

            while (true)
            {
                if (_ShouldStopReadingAudiences(worksheet, audienceRowIndex, currentAudienceColumnIndex))
                    break;

                var ratingColumnIndex = currentAudienceColumnIndex;
                var impressionsColumnIndex = ratingColumnIndex + 1;
                var vpvhColumnIndex = impressionsColumnIndex + 1;
                var cpmColumnIndex = vpvhColumnIndex + 1;

                var ratingText = worksheet.Cells[audienceRowIndex, ratingColumnIndex].GetStringValue();
                var impressionsText = worksheet.Cells[audienceRowIndex, impressionsColumnIndex].GetStringValue();
                var vpvhText = worksheet.Cells[audienceRowIndex, vpvhColumnIndex].GetStringValue();
                var cpmText = worksheet.Cells[audienceRowIndex, cpmColumnIndex].GetStringValue();

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

                if (string.IsNullOrWhiteSpace(vpvhText))
                {
                    validationProblems.Add($"VPVH header is expected. Row: {audienceRowIndex}, column: {vpvhColumnIndex}");
                    invalidAudience = true;
                }

                if (string.IsNullOrWhiteSpace(cpmText))
                {
                    validationProblems.Add($"CPM header is expected. Row: {audienceRowIndex}, column: {cpmColumnIndex}");
                    invalidAudience = true;
                }

                if (invalidAudience)
                    break;

                var ratingMatch = ratingRegex.Match(ratingText);
                var impressionsMatch = impressionsRegex.Match(impressionsText);
                var vpvhMatch = vpvhRegex.Match(vpvhText);
                var cpmMatch = cpmRegex.Match(cpmText);

                if (!ratingMatch.Success)
                {
                    validationProblems.Add($"Rating header is incorrect: {ratingText}. Row: {audienceRowIndex}, column: {ratingColumnIndex}. Correct format: '[DEMO] Avg Rtg'");
                    invalidAudience = true;
                }

                if (!impressionsMatch.Success)
                {
                    validationProblems.Add($"Impressions header is incorrect: {impressionsText}. Row: {audienceRowIndex}, column: {impressionsColumnIndex}. Correct format: '[DEMO] Avg Imps (000)'");
                    invalidAudience = true;
                }

                if (!vpvhMatch.Success)
                {
                    validationProblems.Add($"VPVH header is incorrect: {impressionsText}. Row: {audienceRowIndex}, column: {impressionsColumnIndex}. Correct format: '[DEMO] VPVH'");
                    invalidAudience = true;
                }

                if (!cpmMatch.Success)
                {
                    validationProblems.Add($"CPM header is incorrect: {impressionsText}. Row: {audienceRowIndex}, column: {impressionsColumnIndex}. Correct format: '[DEMO] CPM'");
                    invalidAudience = true;
                }

                if (invalidAudience)
                    break;

                var ratingAudience = ratingMatch.Groups["Audience"].Value.RemoveWhiteSpaces();
                var impressionsAudience = impressionsMatch.Groups["Audience"].Value.RemoveWhiteSpaces();
                var vpvphAudience = vpvhMatch.Groups["Audience"].Value.RemoveWhiteSpaces();
                var cpmAudience = cpmMatch.Groups["Audience"].Value.RemoveWhiteSpaces();

                if (ratingAudience != impressionsAudience ||
                    ratingAudience != vpvphAudience ||
                    ratingAudience != cpmAudience)
                {
                    validationProblems.Add($"Audience '{ratingAudience}' from cell(row: {audienceRowIndex}, column: {ratingAudience}) should be the same as the audience found in the next three columns (Impressions, VPHVP and CPM)");
                    break;
                }

                var audience = AudienceCache.GetBroadcastAudienceByCode(ratingAudience);

                if (audience == null)
                {
                    validationProblems.Add($"Unknown audience is specified: {ratingAudience}. Row: {audienceRowIndex}, column: {ratingColumnIndex}");
                    break;
                }

                if (result.Any(x => x.Code == audience.Code))
                {
                    validationProblems.Add($"Data for audience '{audience.Code}' have been already read. Please specify unique audiences. Row: {audienceRowIndex}, column: {ratingColumnIndex}");
                    break;
                }

                result.Add(audience);
                currentAudienceColumnIndex = currentAudienceColumnIndex + 4;
            }

            return result;
        }

        private void _ProcessEffectiveAndEndDates(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
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
                validationProblems.Add($"End date ({endDateText}) should be greater then effective date ({effectiveDateText})");
                validDate = false;
            }

            if (validDate)
            {
                header.EffectiveDate = effectiveDate;
                header.EndDate = endDate;
            }
        }

        private bool _IsLineEmpty(ExcelWorksheet worksheet, int rowIndex, int columnIndex, List<BroadcastAudience> audiences)
        {
            // Fixed headers: Program -> Rate -> HH Rtg -> HH Imps -> HH CPM
            // Audiences header: [Demo] Rtg -> [Demo] Imps -> [Demo] VPVH -> [Demo] CPM
            var cellsToCheck = 5 + (audiences.Count - 1) * 4;

            for (var i = 0; i < cellsToCheck; i++)
            {
                if (!string.IsNullOrWhiteSpace(worksheet.Cells[rowIndex, columnIndex + i].GetStringValue()))
                {
                    return false;
                }
            }

            return true;
        }

        private void _ProcessDaypartCode(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
        {
            var daypartCode = worksheet.Cells[DAYPART_CODE_CELL].GetStringValue();

            if (string.IsNullOrWhiteSpace(daypartCode))
            {
                validationProblems.Add("Daypart code is missing");
            }
            else if (!DaypartCodeRepository.ActiveDaypartCodeExists(daypartCode))
            {
                validationProblems.Add("Not acceptable daypart code is specified");
            }
            else
            {
                header.DaypartCode = daypartCode;
            }
        }

        private void _ProcessContractedDaypart(List<string> validationProblems, ProprietaryInventoryHeader header)
        {   
            header.ContractedDaypartId = _DaypartCache.GetIdByDaypart(_GetDefaultDaypart());
        }

        private DisplayDaypart _GetDefaultDaypart()
        {
            // Daypart for Syndication is always 24 hours for all days of the week.
            var defaultDaypart = new DisplayDaypart
            {
                StartTime = 0,
                EndTime = 86399, // 23h + 59m + 59s
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = true,
                Sunday = true
            };

            return defaultDaypart;
        }

        private void _ProcessNTIToNSIIncrease(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
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

        private void _ProcessShareBook(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header, out bool shareBookParsedCorrectly, out DateTime shareBook)
        {
            shareBookParsedCorrectly = false;
            shareBook = default(DateTime);
            var shareBookText = worksheet.Cells[SHARE_BOOK_CELL].GetTextValue();

            if (string.IsNullOrWhiteSpace(shareBookText))
            {
                validationProblems.Add("Share book is missing");
            }
            else if (!DateTime.TryParseExact(shareBookText, BOOK_DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out shareBook))
            {
                validationProblems.Add($"Share book ({shareBookText}) is not in the correct format ({(string.Join(", ", BOOK_DATE_FORMATS))})");
            }
            else
            {
                header.ShareBookId = MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(shareBook.Year, shareBook.Month).Id;
                shareBookParsedCorrectly = true;
            }
        }

        private void _ProcessHutBook(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header, bool shareBookParsedCorrectly, DateTime shareBook)
        {
            string hutBookText = worksheet.Cells[HUT_BOOK_CELL].GetTextValue();

            if (string.IsNullOrWhiteSpace(hutBookText)) return;

            if (!DateTime.TryParseExact(hutBookText, BOOK_DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime hutBook))
            {
                validationProblems.Add($"Hut book ({hutBookText}) is not in the correct format ({(string.Join(", ", BOOK_DATE_FORMATS))})");
            }
            else if (shareBookParsedCorrectly && hutBook >= shareBook)
            {
                validationProblems.Add("HUT Book must be prior to the Share book");
            }
            else
            {
                header.HutBookId = MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(hutBook.Year, hutBook.Month).Id;
            }
        }

        private void _ProcessPlaybackType(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
        {
            var playbackString = worksheet.Cells[PLAYBACK_TYPE_CELL].GetStringValue()?.RemoveWhiteSpaces();

            if (string.IsNullOrWhiteSpace(playbackString))
            {
                validationProblems.Add($"Playback type is missing");
                return;
            }

            var playback = EnumHelper.GetEnumValueFromDescription<ProposalEnums.ProposalPlaybackType>(playbackString);

            if (playback == 0)
            {
                validationProblems.Add($"Invalid playback type ({playbackString})");
            }
            else
            {
                header.PlaybackType = playback;
            }
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

