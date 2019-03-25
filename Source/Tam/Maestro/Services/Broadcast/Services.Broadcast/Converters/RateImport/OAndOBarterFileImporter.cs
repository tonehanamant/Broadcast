using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using static Services.Broadcast.Entities.BarterInventory.BarterInventoryFile;

namespace Services.Broadcast.Converters.RateImport
{
    public class OAndOBarterFileImporter : BarterFileImporterBase
    {
        private const string INVENTORY_SOURCE_CELL = "B2";
        private const string SPOT_LENGTH_CELL = "B3";
        private const string EFFECTIVE_DATE_CELL = "B4";
        private const string END_DATE_CELL = "B5";
        private const string DAYPART_CODE_CELL = "B6";
        private const string CONTRACTED_DAYPART_CELL = "B7";
        private const string SHARE_BOOK_CELL = "B8";

        public OAndOBarterFileImporter(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine) : base(
                broadcastDataRepositoryFactory, 
                broadcastAudiencesCache, 
                inventoryDaypartParsingEngine, 
                mediaMonthAndWeekAggregateCache,
                stationProcessingEngine,
                spotLengthEngine)
        {
        }

        public override void LoadAndValidateDataLines(ExcelWorksheet worksheet, BarterInventoryFile barterFile)
        {
            const int firstColumnIndex = 2;
            const int emptyLinesLimitToStopProcessing = 5;

            // index might differ depending on hut book specified or not
            var stationHeaderRowIndex = _FindRowNumber("STATION", firstColumnIndex, 12, 14, worksheet);

            if (!stationHeaderRowIndex.HasValue)
            {
                barterFile.ValidationProblems.Add($"Can not find first data line");
                return;
            }

            var rowIndex = stationHeaderRowIndex.Value + 1;
            var weeks = _ReadWeeks(worksheet);
            var emptyLinesProcessedAfterLastDataLine = 0;

            while (true)
            {
                var line = _ReadDataLine(worksheet, rowIndex, firstColumnIndex, weeks, out var dayText, out var timeText);

                if (_IsSummaryLine(line))
                {
                    rowIndex++;
                    emptyLinesProcessedAfterLastDataLine = 0;
                    continue;
                }

                if (_IsLineEmpty(line))
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

                var validationProblems = _ValidateLine(line, rowIndex, dayText, timeText);

                if (validationProblems.Any())
                {
                    barterFile.ValidationProblems.AddRange(validationProblems);
                }
                else
                {
                    barterFile.DataLines.Add(line);
                }

                rowIndex++;
                emptyLinesProcessedAfterLastDataLine = 0;
            }
        }

        private List<string> _ValidateLine(BarterInventoryDataLine line, int rowIndex, string dayText, string timeText)
        {
            var validationProblems = new List<string>();

            if (string.IsNullOrWhiteSpace(line.Station))
            {
                validationProblems.Add($"Line {rowIndex} contains an empty station cell");
            }

            if (string.IsNullOrWhiteSpace(line.Program))
            {
                validationProblems.Add($"Line {rowIndex} contains an empty program name cell");
            }

            if (line.Dayparts == null)
            {
                var dayEmpty = string.IsNullOrWhiteSpace(dayText);
                var timeEmpty = string.IsNullOrWhiteSpace(timeText);

                if (dayEmpty)
                {
                    validationProblems.Add($"Line {rowIndex} contains an empty day cell");
                }

                if (timeEmpty)
                {
                    validationProblems.Add($"Line {rowIndex} contains an empty time cell");
                }

                if (!dayEmpty && !timeEmpty)
                {
                    validationProblems.Add($"Line {rowIndex} contains an invalid daypart(s): {dayText} {timeText}");
                }
            }

            if (!line.Impressions.HasValue)
            {
                validationProblems.Add($"Line {rowIndex} contains an empty impressions cell");
            }

            if (!line.CPM.HasValue)
            {
                validationProblems.Add($"Line {rowIndex} contains an empty CPM cell");
            }

            return validationProblems;
        }

        private BarterInventoryDataLine _ReadDataLine(
            ExcelWorksheet worksheet, 
            int rowIndex, 
            int firstColumnIndex, 
            List<int> mediaWeekIds, 
            out string dayText, 
            out string timeText)
        {
            var columnIndex = firstColumnIndex;

            // don`t simplify object initialization because line columns should be read with the current order 
            var line = new BarterInventoryDataLine();
            line.Station = worksheet.Cells[rowIndex, columnIndex++].GetStringValue();

            dayText = worksheet.Cells[rowIndex, columnIndex++].GetStringValue();
            timeText = worksheet.Cells[rowIndex, columnIndex++].GetStringValue();

            if (!string.IsNullOrWhiteSpace(dayText) && !string.IsNullOrWhiteSpace(timeText))
            {
                var daypartText = $"{dayText} {timeText}";

                if (DaypartParsingEngine.TryParse(daypartText, out var dayparts))
                {
                    line.Dayparts = dayparts;
                }
            }

            line.Program = worksheet.Cells[rowIndex, columnIndex++].GetStringValue();

            foreach (var weekId in mediaWeekIds)
            {
                line.Weeks.Add(new BarterInventoryDataLine.Week
                {
                    MediaWeekId = weekId,
                    Spots = worksheet.Cells[rowIndex, columnIndex++].GetIntValue()
                });
            }

            // skip an empty column
            columnIndex++;

            line.Impressions = worksheet.Cells[rowIndex, columnIndex++].GetDoubleValue();
            line.CPM = worksheet.Cells[rowIndex, columnIndex++].GetDecimalValue();

            return line;
        }

        private bool _IsSummaryLine(BarterInventoryDataLine line)
        {
            return !string.IsNullOrWhiteSpace(line.Station) &&
                string.IsNullOrWhiteSpace(line.Program) &&
                !line.Impressions.HasValue &&
                !line.CPM.HasValue &&
                line.Dayparts == null &&
                line.Weeks.All(x => !x.Spots.HasValue);
        }

        private bool _IsLineEmpty(BarterInventoryDataLine line)
        {
            return string.IsNullOrWhiteSpace(line.Station) &&
                string.IsNullOrWhiteSpace(line.Program) &&
                !line.Impressions.HasValue &&
                !line.CPM.HasValue &&
                line.Dayparts == null &&
                line.Weeks.All(x => !x.Spots.HasValue);
        }

        private List<int> _ReadWeeks(ExcelWorksheet worksheet)
        {
            const string hhHeader = "HH";
            const int firstColumnIndex = 6;
            var dateFormats = new string[] { "d-MMM-yyyy", "M/d/yyyy" };
            var result = new List<int>();
            var lastColumnIndex = firstColumnIndex;
            var weeksStartHeaderRowIndex = _FindRowNumber("Weeks Start", firstColumnIndex, 10, 13, worksheet);

            if (!weeksStartHeaderRowIndex.HasValue)
            {
                throw new Exception("Couldn't find first week column");
            }

            var weeksRowIndex = weeksStartHeaderRowIndex.Value + 1;
            var hhHeaderCellRowIndex = weeksStartHeaderRowIndex.Value + 2;

            // let's find lastColumnIndex by looking for cell value which starts from "HH"
            while (true)
            {
                try
                {
                    // hh cell should be 2 cell after last week column
                    var hhHeaderCell = worksheet.Cells[hhHeaderCellRowIndex, lastColumnIndex + 2].GetStringValue();
                    var ishhHeaderCell = !string.IsNullOrWhiteSpace(hhHeaderCell) && hhHeaderCell.StartsWith(hhHeader, StringComparison.OrdinalIgnoreCase);

                    if (ishhHeaderCell)
                    {
                        break;
                    }

                    lastColumnIndex++;
                }
                catch
                {
                    throw new Exception("Couldn't find last week column");
                }
            }

            var beforehhHeaderCell = worksheet.Cells[weeksRowIndex, lastColumnIndex + 1].GetStringValue();

            if (!string.IsNullOrWhiteSpace(beforehhHeaderCell))
            {
                throw new Exception("Valid template should contain an empty column between last week column and HH IMPS column");
            }

            for (var i = firstColumnIndex; i <= lastColumnIndex; i++)
            {
                var weekString = worksheet.Cells[weeksRowIndex, i].GetTextValue();

                if (string.IsNullOrWhiteSpace(weekString))
                {
                    throw new Exception($"Week is missing. Row: {weeksRowIndex}, column: {i}");
                }

                if (!DateTime.TryParseExact(weekString, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime week))
                {
                    throw new Exception($"Week date is not in the correct format ({(string.Join(", ", dateFormats))})");
                }

                var mediaWeekId = MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(week).Id;
                result.Add(mediaWeekId);
            }

            return result;
        }

        public override void PopulateManifests(BarterInventoryFile barterFile, List<DisplayBroadcastStation> stations)
        {
            barterFile.InventoryManifests = _GetStationInventoryManifests(barterFile, stations);
        }

        private List<StationInventoryManifest> _GetStationInventoryManifests(BarterInventoryFile barterFile, List<DisplayBroadcastStation> stations)
        {
            var fileHeader = barterFile.Header;
            var stationsDict = stations.ToDictionary(x => x.LegacyCallLetters, x => x, StringComparer.OrdinalIgnoreCase);

            return barterFile.DataLines
                .Select(x => new StationInventoryManifest
                {
                    EffectiveDate = fileHeader.EffectiveDate,
                    EndDate = fileHeader.EndDate,
                    InventorySourceId = barterFile.InventorySource.Id,
                    FileId = barterFile.Id,
                    Station = stationsDict[StationProcessingEngine.StripStationSuffix(x.Station)],
                    SpotLengthId = fileHeader.SpotLengthId.Value,
                    DaypartCode = fileHeader.DaypartCode,
                    ManifestAudiences = new List<StationInventoryManifestAudience>
                    {
                        new StationInventoryManifestAudience
                        {
                            Audience = new DisplayAudience { Id = fileHeader.AudienceId.Value },
                            CPM = x.CPM.Value,
                            Impressions = x.Impressions
                        }
                    },
                    ManifestDayparts = x.Dayparts.Select(d => new StationInventoryManifestDaypart
                    {
                        Daypart = d,
                        ProgramName = x.Program
                    }).ToList(),
                    ManifestWeeks = x.Weeks
                        .Where(w => w.Spots.HasValue) //exclude empty weeks
                        .Select(w => new StationInventoryManifestWeek
                    {
                        MediaWeek = new MediaWeek { Id = w.MediaWeekId },
                        Spots = w.Spots.Value
                    }).ToList()
                }).ToList();
        }

        protected override void LoadAndValidateHeaderData(ExcelWorksheet worksheet, BarterInventoryFile barterFile)
        {
            var header = new BarterInventoryHeader();
            var validationProblems = new List<string>();

            _ProcessInventorySource(worksheet, validationProblems);
            _ProcessSpotLength(worksheet, validationProblems, header);
            _ProcessEffectiveAndEndDates(worksheet, validationProblems, header);
            _ProcessDaypartCode(worksheet, validationProblems, header);
            _ProcessContractedDaypart(worksheet, validationProblems, header);
            _ProcessShareBook(worksheet, validationProblems, header, out var shareBookParsedCorrectly, out var shareBook);
            _ProcessHutBook(worksheet, validationProblems, header, shareBookParsedCorrectly, shareBook);
            _ProcessPlaybackType(worksheet, validationProblems, header);
            
            // for now we suppose it`s always House Holds
            header.AudienceId = AudienceCache.GetDisplayAudienceByCode(BroadcastConstants.HOUSEHOLD_CODE).Id;

            barterFile.Header = header;
            barterFile.ValidationProblems.AddRange(validationProblems);
        }

        private void _ProcessInventorySource(ExcelWorksheet worksheet, List<string> validationProblems)
        {
            if (string.IsNullOrWhiteSpace(worksheet.Cells[INVENTORY_SOURCE_CELL].GetStringValue()))
            {
                validationProblems.Add("Inventory source is missing");
            }
        }

        private void _ProcessSpotLength(ExcelWorksheet worksheet, List<string> validationProblems, BarterInventoryHeader header)
        {
            var spotLengthString = worksheet.Cells[SPOT_LENGTH_CELL].GetStringValue();

            if (string.IsNullOrWhiteSpace(spotLengthString))
            {
                validationProblems.Add("Spot length is missing");
                return;
            }

            spotLengthString = spotLengthString.Replace(":", string.Empty);

            if (!int.TryParse(spotLengthString, out var spotLength) || !SpotLengthEngine.SpotLengthExists(spotLength))
            {
                validationProblems.Add("Invalid spot length is specified");
            }
            else
            {
                header.SpotLengthId = SpotLengthEngine.GetSpotLengthIdByValue(spotLength);
            }
        }

        private void _ProcessEffectiveAndEndDates(ExcelWorksheet worksheet, List<string> validationProblems, BarterInventoryHeader header)
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

        private void _ProcessDaypartCode(ExcelWorksheet worksheet, List<string> validationProblems, BarterInventoryHeader header)
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

        private void _ProcessContractedDaypart(ExcelWorksheet worksheet, List<string> validationProblems, BarterInventoryHeader header)
        {
            var daypartString = worksheet.Cells[CONTRACTED_DAYPART_CELL].GetStringValue();

            if (string.IsNullOrWhiteSpace(daypartString))
            {
                validationProblems.Add("Contracted daypart is missing");
            }
            else if (!DaypartParsingEngine.TryParse(daypartString, out var displayDayparts))
            {
                validationProblems.Add($"Invalid contracted daypart ({daypartString})");
            }
            else if (displayDayparts.Count > 1)
            {
                validationProblems.Add($"Only one contracted daypart should be specified ({daypartString})");
            }
            else
            {
                header.ContractedDaypartId = displayDayparts.Single().Id;
            }
        }

        private void _ProcessShareBook(ExcelWorksheet worksheet, List<string> validationProblems, BarterInventoryHeader header, out bool shareBookParsedCorrectly, out DateTime shareBook)
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

        private void _ProcessHutBook(ExcelWorksheet worksheet, List<string> validationProblems, BarterInventoryHeader header, bool shareBookParsedCorrectly, DateTime shareBook)
        {
            // let`s check if hut book specified
            var hutBookRowIndex = _FindRowNumber("HUT Book", 1, 9, 10, worksheet);

            if (!hutBookRowIndex.HasValue) return;

            string hutBookText = worksheet.Cells[hutBookRowIndex.Value, 2].GetTextValue();

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

        private void _ProcessPlaybackType(ExcelWorksheet worksheet, List<string> validationProblems, BarterInventoryHeader header)
        {
            // index might differ depending on hut book specified or not
            var playbackRowIndex = _FindRowNumber("Playback type", 1, 9, 10, worksheet);

            if (!playbackRowIndex.HasValue)
            {
                validationProblems.Add($"Playback type is missing");
                return;
            }

            var playbackString = worksheet.Cells[playbackRowIndex.Value, 2].GetStringValue()?.RemoveWhiteSpaces();

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

        private int? _FindRowNumber(string searchString, int column, int rowStart, int rowEnd, ExcelWorksheet worksheet)
        {
            for (var row = rowStart; row <= rowEnd; row++)
            {
                var cellValue = worksheet.Cells[row, column].GetStringValue();

                if (!string.IsNullOrWhiteSpace(cellValue) && cellValue.Equals(searchString, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }

            return null;
        }

        public override void PopulateRates(BarterInventoryFile barterFile)
        {
            foreach (var manifest in barterFile.InventoryManifests)
            {
                var audience = manifest.ManifestAudiences.Single();

                manifest.ManifestRates.Add(new StationInventoryManifestRate
                {
                    SpotLengthId = manifest.SpotLengthId,
                    SpotCost = ProposalMath.CalculateCost(audience.CPM, audience.Impressions.Value)
                });
            }
        }
    }
}
