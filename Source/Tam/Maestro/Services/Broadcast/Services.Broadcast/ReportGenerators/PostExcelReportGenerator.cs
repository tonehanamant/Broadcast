using Common.Services.Extensions;
using Common.Services.Repositories;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.ReportGenerators
{
    public class PostExcelReportGenerator : IReportGenerator<PostPrePostingFile>
    {
        private static readonly HashSet<string> _ExcelFileHeaders = new HashSet<string>
        {
            RANK,
            MARKET,
            STATION,
            AFFILIATE,
            WEEKSTART,
            DAY,
            DATE,
            TIMEAIRED,
            PROGRAMNAME,
            SPOTLENGTH,
            HOUSEISCI,
            CLIENTISCI,
            ADVERTISER,
            INVENTORYSOURCE,
            INVENTORYSOURCEDAYPART,
            ADVERTISEROUTOFSPECREASON,
            INVENTORYOUTOFSPECREASON,
            ESTIMATE,
            DETECTEDVIA,
            SPOT,
            POSTINGBOOK,
            PLAYBACKTYPE
        };

        private const string MARKET = "Market";
        private const string STATION = "Station";
        private const string RANK = "Rank";
        private const string SPOT = "Spot";
        private const string DETECTEDVIA = "Detected Via";
        private const string ESTIMATE = "Estimate";
        private const string ADVERTISEROUTOFSPECREASON = "Advertiser Out of Spec Reason";
        private const string INVENTORYOUTOFSPECREASON = "Inventory Out of Spec Reason";
        private const string INVENTORYSOURCEDAYPART = "Daypart";
        private const string INVENTORYSOURCE = "Inventory Source";
        private const string ADVERTISER = "Advertiser";
        private const string CLIENTISCI = "Client ISCI";
        private const string HOUSEISCI = "House ISCI";
        private const string SPOTLENGTH = "Length";
        private const string PROGRAMNAME = "Program Name";
        private const string TIMEAIRED = "Time Aired";
        private const string DATE = "Date";
        private const string DAY = "Day";
        private const string WEEKSTART = "Weekstart";
        private const string AFFILIATE = "Affiliate";
        private const string POSTINGBOOK = "Posting Book";
        private const string PLAYBACKTYPE = "Playback type";

        private readonly IDataRepositoryFactory _Factory;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly IRatingForecastService _RatingForecastService;

        public PostExcelReportGenerator(IDataRepositoryFactory factory, IImpressionAdjustmentEngine engine, IRatingForecastService ratingForecastService)
        {
            _Factory = factory;
            _ImpressionAdjustmentEngine = engine;
            _RatingForecastService = ratingForecastService;
        }

        public ReportOutput Generate(PostPrePostingFile prePostingFile)
        {
            var output = new ReportOutput(string.Format("PostReport_{0}.xlsx", prePostingFile.Id));

            var package = GenerateExcelPackage(prePostingFile);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        internal ExcelPackage GenerateExcelPackage(PostPrePostingFile prePostingFile)
        {
            var package = new ExcelPackage(new MemoryStream());
            var reportData = prePostingFile.FileDetails;

            var ws = package.Workbook.Worksheets.Add("Post Data");
            ws.View.ShowGridLines = false;
            ws.Cells.Style.Font.Size = 8;
            ws.Cells.Style.Font.Name = "Tahoma";

            var columnOffset = 1;
            _BuildCommonHeader(ws, 1, ref columnOffset, prePostingFile);

            // tables
            var rowOffset = 2;
            columnOffset = 1;

            var postingBookMonthAndYear = _GetPostingBookMonthAndYear(prePostingFile.PostingBookId);
            var playbackTypeDescription = _GetPlaybackTypeDescription(prePostingFile.PlaybackType);

            foreach (var row in reportData)
            {
                ws.Cells[rowOffset, columnOffset++].Value = row.Rank;
                ws.Cells[rowOffset, columnOffset++].Value = row.Market;
                ws.Cells[rowOffset, columnOffset++].Value = row.Station;
                ws.Cells[rowOffset, columnOffset++].Value = row.Affiliate;
                ws.Cells[rowOffset, columnOffset++].Value = row.Weekstart.ToString(@"M\/d\/yyyy");
                ws.Cells[rowOffset, columnOffset++].Value = row.Day;
                ws.Cells[rowOffset, columnOffset++].Value = row.Date.ToString(@"M\/d\/yyyy");
                ws.Cells[rowOffset, columnOffset++].Value = row.Date.Add(TimeSpan.FromSeconds(row.TimeAired)).ToString(@"h\:mm\:ss tt");
                ws.Cells[rowOffset, columnOffset++].Value = row.ProgramName;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotLength;
                ws.Cells[rowOffset, columnOffset++].Value = row.HouseISCI;
                ws.Cells[rowOffset, columnOffset++].Value = row.ClientISCI;
                ws.Cells[rowOffset, columnOffset++].Value = row.Advertiser;
                ws.Cells[rowOffset, columnOffset++].Value = row.InventorySource;
                ws.Cells[rowOffset, columnOffset++].Value = row.InventorySourceDaypart;
                ws.Cells[rowOffset, columnOffset++].Value = row.AdvertiserOutOfSpecReason;
                ws.Cells[rowOffset, columnOffset++].Value = row.InventoryOutOfSpecReason;
                ws.Cells[rowOffset, columnOffset++].Value = row.EstimateID;
                ws.Cells[rowOffset, columnOffset++].Value = row.DetectedVia;
                ws.Cells[rowOffset, columnOffset++].Value = row.Spot;
                ws.Cells[rowOffset, columnOffset++].Value = postingBookMonthAndYear;
                ws.Cells[rowOffset, columnOffset++].Value = playbackTypeDescription;
                var imp = row.Impressions.ToDictionary(i => i.Demo);
                foreach (var demo in prePostingFile.Demos)
                {
                    var value = imp.ContainsKey(demo) ? _ImpressionAdjustmentEngine.AdjustImpression(imp[demo].Impression, prePostingFile.Equivalized, row.SpotLength)
                                                      : 0;
                    ws.Cells[rowOffset, columnOffset].Style.Numberformat.Format = "#,#";
                    ws.Cells[rowOffset, columnOffset].Value = value;
                    columnOffset++;
                }

                rowOffset++;
                columnOffset = 1;
            }

            for (var i = 1; i <= columnOffset; i++)
            {
                ws.Cells[reportData.Count + 2, i].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            }

            ws.Cells.AutoFitColumns();
            return package;
        }

        private void _BuildCommonHeader(ExcelWorksheet ws, int rowOffset, ref int columnOffset, PostPrePostingFile scheduleReportDto)
        {
            // header
            foreach (var header in _ExcelFileHeaders)
            {
                ws.Cells[rowOffset, columnOffset++].Value = header;
            }

            foreach (var audience in _Factory.GetDataRepository<IAudienceRepository>().GetAudiencesByIds(scheduleReportDto.Demos))
            {
                ws.Cells[rowOffset, columnOffset++].Value = "Impressions (" + audience.Display + ")";
            }

            for (var i = 1; i < columnOffset; i++)
            {
                ws.Cells[rowOffset, i].Style.Font.Bold = true;
                ws.Cells[rowOffset, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            }
        }

        private string _GetPostingBookMonthAndYear(int postingBookId)
        {
            return _RatingForecastService.GetMediaMonthCrunchStatuses()
                .Where(m => m.Crunched == CrunchStatus.Crunched && m.MediaMonth.Id == postingBookId)
                .Select(m => m.MediaMonth.MediaMonthX)
                .First();
        }

        private static string _GetPlaybackTypeDescription(ProposalEnums.ProposalPlaybackType playbackType)
        {
            var playbackTypes = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalPlaybackType>();
            return playbackTypes.Where(p => p.Id == (int) playbackType).Select(p => p.Display).First();
        }
    }
}
