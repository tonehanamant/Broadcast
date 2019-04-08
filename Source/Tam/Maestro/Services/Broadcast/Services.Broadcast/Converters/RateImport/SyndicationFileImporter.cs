using System;
using System.Collections.Generic;
using System.Globalization;
using Common.Services;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public class SyndicationFileImporter : BarterFileImporterBase
    {
        private const string EFFECTIVE_DATE_CELL = "B4";
        private const string END_DATE_CELL = "B5";
        private const string DAYPART_CODE_CELL = "B6";
        private const string NTI_TO_NSI_INCREASE_CELL = "B7";
        private const string SHARE_BOOK_CELL = "B8";
        private const string HUT_BOOK_CELL = "B9";
        private const string PLAYBACK_TYPE_CELL = "B10";

        private readonly IDaypartCache _DaypartCache;

        public SyndicationFileImporter(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine,
            IDaypartCache daypartCache) : base(
                broadcastDataRepositoryFactory,
                broadcastAudiencesCache,
                inventoryDaypartParsingEngine,
                mediaMonthAndWeekAggregateCache,
                stationProcessingEngine,
                spotLengthEngine)
        {
            _DaypartCache = daypartCache;
        }

        public override void LoadAndValidateDataLines(ExcelWorksheet worksheet, BarterInventoryFile barterFile)
        {
        }

        public override void PopulateManifests(BarterInventoryFile barterFile, List<DisplayBroadcastStation> stations)
        {
        }

        protected override void LoadAndValidateHeaderData(ExcelWorksheet worksheet, BarterInventoryFile barterFile)
        {
            var header = new BarterInventoryHeader();
            var validationProblems = new List<string>();
            
            _ProcessEffectiveAndEndDates(worksheet, validationProblems, header);
            _ProcessDaypartCode(worksheet, validationProblems, header);
            _ProcessContractedDaypart(validationProblems, header);
            _ProcessNTIToNSIIncrease(worksheet, validationProblems, header);
            _ProcessShareBook(worksheet, validationProblems, header, out var shareBookParsedCorrectly, out var shareBook);
            _ProcessHutBook(worksheet, validationProblems, header, shareBookParsedCorrectly, shareBook);
            _ProcessPlaybackType(worksheet, validationProblems, header);

            barterFile.Header = header;
            barterFile.ValidationProblems.AddRange(validationProblems);
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

        private void _ProcessContractedDaypart(List<string> validationProblems, BarterInventoryHeader header)
        {
            // all day and week daypart
            var daypart = new DisplayDaypart
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
            header.ContractedDaypartId = _DaypartCache.GetIdByDaypart(daypart);
        }

        private void _ProcessNTIToNSIIncrease(ExcelWorksheet worksheet, List<string> validationProblems, BarterInventoryHeader header)
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

        private void _ProcessPlaybackType(ExcelWorksheet worksheet, List<string> validationProblems, BarterInventoryHeader header)
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
    }
}
