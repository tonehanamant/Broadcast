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
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;

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
            // implement in the line data story 
        }

        public override void PopulateManifests(BarterInventoryFile barterFile, List<DisplayBroadcastStation> stations)
        {
            // implement in the line data story 
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
            var effectiveDateText = worksheet.Cells[EFFECTIVE_DATE_CELL].GetStringValue();
            var endDateText = worksheet.Cells[END_DATE_CELL].GetStringValue();
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
    }
}
