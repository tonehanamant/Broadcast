using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IBarterFileImporter : IApplicationService
    {
        void CheckFileHash();
        BarterInventoryFile GetPendingBarterInventoryFile(string userName);
        void ExtractData(BarterInventoryFile barterFile);
        void LoadFromSaveRequest(InventoryFileSaveRequest request);
    }

    public class BarterFileImporter : IBarterFileImporter
    {
        const string INVENTORY_SOURCE_CELL = "B2";
        const string DAYPART_CODE_CELL = "B3";
        const string EFFECTIVE_DATE_CELL = "B4";
        const string END_DATE_CELL = "B5";
        const string CPM_CELL = "B6";
        const string DEMO_CELL = "B7";
        const string CONTRACTED_DAYPART_CELL = "B8";
        const string SHARE_BOOK_CELL = "B9";
        const string HUT_BOOK_CELL = "B10";
        const string PLAYBACK_TYPE_CELL = "B11";

        const string BOOK_DATE_FORMAT = "MMM yy";
        string[] DATE_FORMATS = new string[2] { "MM/dd/yyyy", "M/dd/yyyy" };
        const string CPM_FORMAT = "##.##";

        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryDaypartParsingEngine _DaypartParsingEngine;
        private readonly IDaypartCache _DaypartCache;
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        private InventoryFileSaveRequest _Request { get; set; }
        private string _FileHash { get; set; }
        private ExcelWorksheet worksheet;
        private ExcelWorksheet _Worksheet
        {
            get
            {
                if (worksheet == null)
                {
                    worksheet = new ExcelPackage(_Request.StreamData).Workbook.Worksheets.First();
                }
                return worksheet;
            }
        }

        public BarterFileImporter(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IInventoryDaypartParsingEngine inventoryDaypartParsingEngine
            , IDaypartCache daypartCache
            , IBroadcastAudiencesCache broadcastAudiencesCache
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _DaypartParsingEngine = inventoryDaypartParsingEngine;
            _DaypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _AudienceCache = broadcastAudiencesCache;
        }

        public void LoadFromSaveRequest(InventoryFileSaveRequest request)
        {
            _Request = request;
            _FileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(request.StreamData));
        }

        public void CheckFileHash()
        {
            //check if file has already been loaded
            if (_InventoryFileRepository.GetInventoryFileIdByHash(_FileHash) > 0)
            {
                throw new BroadcastDuplicateInventoryFileException(
                    "Unable to load file. The selected file has already been loaded or is already loading.");
            }
        }

        public BarterInventoryFile GetPendingBarterInventoryFile(string userName)
        {
            InventorySource inventorySource = _InventoryRepository.GetInventorySourceByName(_Worksheet.Cells[INVENTORY_SOURCE_CELL].Value.ToString());
            var file = new BarterInventoryFile
            {
                FileName = _Request.FileName ?? "unknown",
                FileStatus = FileStatusEnum.Pending,
                Hash = _FileHash,
                CreatedBy = userName,
                CreatedDate = DateTime.Now,
                InventorySource = inventorySource
            };
            if (inventorySource == null)
            {
                file.ValidationProblems.Add("Could not find inventory source");
            }
            return file;
        }

        public void ExtractData(BarterInventoryFile barterFile)
        {
            using (_Worksheet)
            {
                _LoadAndValidateHeaderData(worksheet, barterFile);

            }
        }

        private void _LoadAndValidateHeaderData(ExcelWorksheet worksheet, BarterInventoryFile barterFile)
        {
            Dictionary<string, string> requiredProperties = new Dictionary<string, string>
                { {"Inv Source", INVENTORY_SOURCE_CELL }, {"Daypart Code",DAYPART_CODE_CELL }, {"Effective Date", EFFECTIVE_DATE_CELL }
                , {"End Date" , END_DATE_CELL}, {"CPM", CPM_CELL} , {"Demo", DEMO_CELL }, {"Contracted Daypart", CONTRACTED_DAYPART_CELL }
                , {"Share Book", SHARE_BOOK_CELL }, {"Playback type", PLAYBACK_TYPE_CELL } };

            var header = new BarterInventoryHeader();
            var validationProblems = new List<string>();

            requiredProperties.AsEnumerable().Where(x => string.IsNullOrWhiteSpace(x.Value)).ForEach(x => validationProblems.Add($"Required value for {x.Key} is missing"));

            header.DaypartCode = worksheet.Cells[DAYPART_CODE_CELL].GetStringValue();

            //Format mm/dd/yyyy and end date must be after start date
            string effectiveDateText = worksheet.Cells[EFFECTIVE_DATE_CELL].GetStringValue().Split(' ')[0]; //split is removing time section
            string endDateText = worksheet.Cells[END_DATE_CELL].GetStringValue().Split(' ')[0];

            if (!DateTime.TryParseExact(effectiveDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime effectiveDate))
            {
                validationProblems.Add($"Effective date is not in the correct format ({(string.Join(", ", DATE_FORMATS))})");
            }            
            if (!DateTime.TryParseExact(endDateText, DATE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                validationProblems.Add($"End date is not in the correct format ({(string.Join(" ", DATE_FORMATS))})");
            }
            if (endDate <= effectiveDate)
            {
                validationProblems.Add($"End date ({endDateText}) should be greater then effective date ({effectiveDateText})");
            }
            else
            {
                header.EffectiveDate = effectiveDate;
                header.EndDate = endDate;
            }

            //Format ##.##, no dollar sign in data
            var r = new Regex(@"^\d+(\.\d)?\d*$");
            var cpm = worksheet.Cells[CPM_CELL].GetStringValue();
            if (!r.IsMatch(cpm))
            {
                validationProblems.Add($"CPM is not in the correct format ({CPM_FORMAT})");
            }
            else
            if (!Decimal.TryParse(cpm, out decimal cpmValue))
            {
                validationProblems.Add($"Invalid value for CPM ({cpm})");
            }
            else
            {
                header.Cpm = cpmValue;
            }

            //Must be valid nelson demo.
            var demo = worksheet.Cells[DEMO_CELL].GetStringValue();
            if (!_AudienceCache.IsValidAudienceCode(demo))
            {
                validationProblems.Add($"Invalid demo ({demo})");
            }
            else
            {
                header.AudienceId = _AudienceCache.GetDisplayAudienceByCode(demo).Id;
            }

            //Format: M-F 6:30PM-11PM and Standard Cadent Daypart rules
            string daypartString = worksheet.Cells[CONTRACTED_DAYPART_CELL].GetStringValue();
            if (_DaypartParsingEngine.TryParse(daypartString, out var displayDayparts) && displayDayparts.Any() && displayDayparts.All(x => x.IsValid))
            {
                DisplayDaypart daypart = displayDayparts.Single();
                header.ContractedDaypartId = _DaypartCache.GetIdByDaypart(daypart);
            }
            else
            {
                validationProblems.Add($"Invalid contracted daypart ({daypartString})");
            }

            string shareBookText = worksheet.Cells[SHARE_BOOK_CELL].GetTextValue();
            if (!DateTime.TryParseExact(shareBookText, BOOK_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime shareBook))
            {
                validationProblems.Add($"Share book ({shareBookText}) is not in the correct format ({BOOK_DATE_FORMAT})");
            }
            else
            {
                header.ShareBookId = _MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(shareBook.Year, shareBook.Month).Id;
            }
            
            //format May 17 (MMM yy) and Hut book must be a media month prior to the Share book media month if value entered
            string hutBookText = worksheet.Cells[HUT_BOOK_CELL].GetTextValue();
            if (!string.IsNullOrWhiteSpace(hutBookText))
            {
                if (!DateTime.TryParseExact(hutBookText, BOOK_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime hutBook))
                {
                    validationProblems.Add($"Hut book ({hutBookText}) is not in the correct format ({BOOK_DATE_FORMAT})");
                }
                else
                if (hutBook >= shareBook)
                {
                    validationProblems.Add("HUT Book must be prior to the Share book");
                }
                else
                {
                    header.HutBookId = _MediaMonthAndWeekAggregateCache.GetMediaMonthByYearAndMonth(hutBook.Year, hutBook.Month).Id;
                }
            }
            
            var playbackString = worksheet.Cells[PLAYBACK_TYPE_CELL].GetStringValue().RemoveWhiteSpaces();
            ProposalEnums.ProposalPlaybackType playback = EnumHelper.GetEnumValueFromDescription<ProposalEnums.ProposalPlaybackType>(playbackString);
            if (playback == 0)
            {
                validationProblems.Add($"Invalid playback type ({playbackString})");
            }
            else
            {
                header.PlaybackType = playback;
            }

            barterFile.Header = header;
            barterFile.ValidationProblems.AddRange(validationProblems);
        }

    }
}
