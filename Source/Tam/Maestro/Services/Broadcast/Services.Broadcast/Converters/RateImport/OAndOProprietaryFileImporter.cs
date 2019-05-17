﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using static Services.Broadcast.Entities.ProprietaryInventory.ProprietaryInventoryFile;

namespace Services.Broadcast.Converters.RateImport
{
    public class OAndOProprietaryFileImporter : ProprietaryFileImporterBase
    {
        private const string INVENTORY_SOURCE_CELL = "B3";
        private const string SPOT_LENGTH_CELL = "B4";
        private const string EFFECTIVE_DATE_CELL = "B5";
        private const string END_DATE_CELL = "B6";
        private const string DAYPART_CODE_CELL = "B7";
        private const string CONTRACTED_DAYPART_CELL = "B8";
        private const string SHARE_BOOK_CELL = "B9";

        public OAndOProprietaryFileImporter(
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

        public override void LoadAndValidateDataLines(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            const int firstColumnIndex = 1;
            const int emptyLinesLimitToStopProcessing = 5;
            const int firstRowToSearch = 15;
            const int lastRowToSearch = 16;

            // index might differ depending on hut book specified or not
            var stationHeaderRowIndex = _FindRowNumber("Station", firstColumnIndex, firstRowToSearch, lastRowToSearch, worksheet);

            if (!stationHeaderRowIndex.HasValue)
            {
                proprietaryFile.ValidationProblems.Add($"Can not find first data line");
                return;
            }

            var rowIndex = stationHeaderRowIndex.Value + 1;
            var weeks = _ReadWeeks(worksheet, proprietaryFile);
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
                    proprietaryFile.ValidationProblems.AddRange(validationProblems);
                }
                else
                {
                    proprietaryFile.DataLines.Add(line);
                }

                rowIndex++;
                emptyLinesProcessedAfterLastDataLine = 0;
            }
        }

        private List<string> _ValidateLine(ProprietaryInventoryDataLine line, int rowIndex, string dayText, string timeText)
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

        private ProprietaryInventoryDataLine _ReadDataLine(
            ExcelWorksheet worksheet, 
            int rowIndex, 
            int firstColumnIndex, 
            List<MediaWeek> mediaWeeks, 
            out string dayText, 
            out string timeText)
        {
            var columnIndex = firstColumnIndex;

            // don`t simplify object initialization because line columns should be read with the current order 
            var line = new ProprietaryInventoryDataLine();
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

            foreach (var week in mediaWeeks)
            {
                line.Weeks.Add(new ProprietaryInventoryDataLine.Week
                {
                    MediaWeek = week,
                    Spots = worksheet.Cells[rowIndex, columnIndex++].GetIntValue()
                });
            }

            line.Impressions = worksheet.Cells[rowIndex, columnIndex++].GetDoubleValue();
            line.CPM = worksheet.Cells[rowIndex, columnIndex++].GetDecimalValue();

            return line;
        }

        private bool _IsSummaryLine(ProprietaryInventoryDataLine line)
        {
            return !string.IsNullOrWhiteSpace(line.Station) &&
                string.IsNullOrWhiteSpace(line.Program) &&
                !line.Impressions.HasValue &&
                !line.CPM.HasValue &&
                line.Dayparts == null &&
                line.Weeks.All(x => !x.Spots.HasValue);
        }

        private bool _IsLineEmpty(ProprietaryInventoryDataLine line)
        {
            return string.IsNullOrWhiteSpace(line.Station) &&
                string.IsNullOrWhiteSpace(line.Program) &&
                !line.Impressions.HasValue &&
                !line.CPM.HasValue &&
                line.Dayparts == null &&
                line.Weeks.All(x => !x.Spots.HasValue);
        }

        private List<MediaWeek> _ReadWeeks(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            const int firstColumnIndex = 5;
            const int firstRowIndex = 14;
            const int lastRowIndex = 15;
            var header = proprietaryFile.Header;
            var audienceCode = header.Audience.Code;
            var validMediaWeekIds = MediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(header.EffectiveDate, header.EndDate).Select(x => x.Id);
            var dateFormats = new string[] { "d-MMM-yyyy", "M/d/yyyy" };
            var result = new List<MediaWeek>();
            var lastColumnIndex = firstColumnIndex;
            var weeksStartHeaderRowIndex = _FindRowNumber("Start Week", firstColumnIndex, firstRowIndex, lastRowIndex, worksheet);

            if (!weeksStartHeaderRowIndex.HasValue)
            {
                throw new Exception("Couldn't find first week column");
            }

            var weeksRowIndex = weeksStartHeaderRowIndex.Value + 1;
            var audienceCodeHeaderCellRowIndex = weeksStartHeaderRowIndex.Value + 1;

            // let's find lastColumnIndex by looking for cell value which starts from audienceCode
            while (true)
            {
                try
                {
                    // audienceCode cell should be after last week column
                    var audienceCodeHeaderCell = worksheet.Cells[audienceCodeHeaderCellRowIndex, lastColumnIndex + 1].GetStringValue();
                    var isAudienceCodeHeaderCell = !string.IsNullOrWhiteSpace(audienceCodeHeaderCell) && audienceCodeHeaderCell.StartsWith(audienceCode, StringComparison.OrdinalIgnoreCase);

                    if (isAudienceCodeHeaderCell)
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

                var mediaWeek = MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(week);

                if (result.Any(x => x.Id == mediaWeek.Id))
                {
                    throw new Exception($"Week that contains date: {weekString} has been specified several times");
                }
                
                if (!validMediaWeekIds.Contains(mediaWeek.Id))
                {
                    throw new Exception($"Week: {weekString} should be inside or intersect with the date range specified in the header (effective and end dates). Row: {weeksRowIndex}, column: {i}");
                }

                result.Add(mediaWeek);
            }

            return result;
        }

        public override void PopulateManifests(ProprietaryInventoryFile proprietaryFile, List<DisplayBroadcastStation> stations)
        {
            proprietaryFile.InventoryManifests = _GetStationInventoryManifests(proprietaryFile, stations);
        }

        private List<StationInventoryManifest> _GetStationInventoryManifests(ProprietaryInventoryFile proprietaryFile, List<DisplayBroadcastStation> stations)
        {
            var fileHeader = proprietaryFile.Header;
            var stationsDict = stations.ToDictionary(x => x.LegacyCallLetters, x => x, StringComparer.OrdinalIgnoreCase);

            return proprietaryFile.DataLines
                .Select(x => new StationInventoryManifest
                {
                    InventorySourceId = proprietaryFile.InventorySource.Id,
                    InventoryFileId = proprietaryFile.Id,
                    Station = stationsDict[StationProcessingEngine.StripStationSuffix(x.Station)],
                    SpotLengthId = fileHeader.SpotLengthId.Value,
                    DaypartCode = fileHeader.DaypartCode,
                    ManifestAudiences = new List<StationInventoryManifestAudience>
                    {
                        new StationInventoryManifestAudience
                        {
                            Audience = fileHeader.Audience.ToDisplayAudience(),
                            CPM = x.CPM.Value,
                            Impressions = x.Impressions.Value * 1000
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
                        MediaWeek = w.MediaWeek,
                        Spots = w.Spots.Value
                    }).ToList()
                }).ToList();
        }

        protected override void LoadAndValidateHeaderData(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            var header = new ProprietaryInventoryHeader();
            var validationProblems = new List<string>();

            _ProcessInventorySource(worksheet, validationProblems);
            _ProcessSpotLength(worksheet, validationProblems, header);
            _ProcessEffectiveAndEndDates(worksheet, validationProblems, header);
            _ProcessDaypartCode(worksheet, validationProblems, header);
            _ProcessContractedDaypart(worksheet, validationProblems, header);
            _ProcessShareBook(worksheet, validationProblems, header, out var shareBookParsedCorrectly, out var shareBook);
            _ProcessHutBook(worksheet, validationProblems, header, shareBookParsedCorrectly, shareBook);
            _ProcessPlaybackType(worksheet, validationProblems, header);
            _ProcessAudience(worksheet, validationProblems, header);         

            proprietaryFile.Header = header;
            proprietaryFile.ValidationProblems.AddRange(validationProblems);
        }

        private void _ProcessAudience(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
        {
            // let`s find header with audience
            const int firstColumnIndex = 6;
            const int firstRowIndex = 15;
            const int lastRowIndex = 16;
            var columnIndex = firstColumnIndex;
            var audienceRegex = new Regex(@"(?<Audience>([a-z-+\d])+)\s+IMPS.*", RegexOptions.IgnoreCase);

            while (true)
            {
                try
                {
                    var audienceParsed = false;

                    for (var i = firstRowIndex; i <= lastRowIndex; i++)
                    {
                        var cellValue = worksheet.Cells[i, columnIndex].GetStringValue() ?? string.Empty;
                        var match = audienceRegex.Match(cellValue);

                        if (match.Success)
                        {
                            var audienceCode = match.Groups["Audience"].Value;
                            header.Audience = AudienceCache.GetBroadcastAudienceByCode(audienceCode);
                            audienceParsed = true;
                            break;
                        }
                    }

                    if (audienceParsed)
                    {
                        break;
                    }

                    columnIndex++;
                }
                catch
                {
                    throw new Exception("Couldn't find audience. Please specify '{audience} IMPS' cell");
                }
            }

            if (header.Audience == null)
            {
                throw new Exception("Unknown audience was found. Please specify '{audience} IMPS' cell with valid audience");
            }
        }

        private void _ProcessInventorySource(ExcelWorksheet worksheet, List<string> validationProblems)
        {
            if (string.IsNullOrWhiteSpace(worksheet.Cells[INVENTORY_SOURCE_CELL].GetStringValue()))
            {
                validationProblems.Add("Inventory source is missing");
            }
        }

        private void _ProcessSpotLength(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
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

        private void _ProcessContractedDaypart(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
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
            const int hutBookColumn = 1;
            const int hutBookRowStart = 10;
            const int hutBookRowEnd = 11;

            // let`s check if hut book specified
            var hutBookRowIndex = _FindRowNumber("HUT Book", hutBookColumn, hutBookRowStart, hutBookRowEnd, worksheet);

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

        private void _ProcessPlaybackType(ExcelWorksheet worksheet, List<string> validationProblems, ProprietaryInventoryHeader header)
        {
            const int playbackTypeColumn = 1;
            const int playbackTypeRowStart = 10;
            const int playbackTypeRowEnd = 11;

            // index might differ depending on hut book specified or not
            var playbackRowIndex = _FindRowNumber("Playback Type", playbackTypeColumn, playbackTypeRowStart, playbackTypeRowEnd, worksheet);

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
    }
}
