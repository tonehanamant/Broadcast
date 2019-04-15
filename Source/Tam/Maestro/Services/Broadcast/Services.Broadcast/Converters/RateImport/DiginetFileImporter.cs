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
using Services.Broadcast.Extensions;

namespace Services.Broadcast.Converters.RateImport
{
    public class DiginetFileImporter : BarterFileImporterBase
    {
        private const string EFFECTIVE_DATE_CELL = "B4";
        private const string END_DATE_CELL = "B5";
        private const string NTI_TO_NSI_INCREASE_CELL = "B6";
        private const string DEFAULT_DAYPART_CODE = "DIGI";

        private readonly IDaypartCache _DaypartCache;

        public DiginetFileImporter(
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
            _ProcessNTIToNSIIncrease(worksheet, validationProblems, header);

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
            header.DaypartCode = DEFAULT_DAYPART_CODE;
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
    }
}
