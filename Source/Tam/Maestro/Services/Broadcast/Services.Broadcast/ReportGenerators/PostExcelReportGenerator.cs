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

namespace Services.Broadcast.ReportGenerators
{
    public class PostExcelReportGenerator : IReportGenerator<PostFile>
    {
        static readonly HashSet<string> ExcelFileHeaders = new HashSet<string>
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
            INVENTORYOUTOFSPECREASON,
            ESTIMATE,
            DETECTEDVIA,
            SPOT,
            POSTINGBOOK,
            PLAYBACKTYPE
        };

        const string MARKET = "Market";
        const string STATION = "Station";
        const string RANK = "Rank";
        const string SPOT = "Spot";
        const string DETECTEDVIA = "Detected Via";
        const string ESTIMATE = "Estimate";
        const string INVENTORYOUTOFSPECREASON = "Out of Spec Reason";
        const string INVENTORYSOURCEDAYPART = "Daypart";
        const string INVENTORYSOURCE = "Inventory Source";
        const string ADVERTISER = "Advertiser";
        const string CLIENTISCI = "Client ISCI";
        const string HOUSEISCI = "House ISCI";
        const string SPOTLENGTH = "Length";
        const string PROGRAMNAME = "Program Name";
        const string TIMEAIRED = "Time Aired";
        const string DATE = "Date";
        const string DAY = "Day";
        const string WEEKSTART = "Weekstart";
        const string AFFILIATE = "Affiliate";
        const string POSTINGBOOK = "Posting Book";
        const string PLAYBACKTYPE = "Playback type";

        private readonly IDataRepositoryFactory _Factory;
        private readonly IRatingForecastService _RatingForecastService;

        public PostExcelReportGenerator(IDataRepositoryFactory factory, IRatingForecastService ratingForecastService)
        {
            _Factory = factory;
            _RatingForecastService = ratingForecastService;
        }

        public ReportOutput Generate(PostFile file)
        {
            var output = new ReportOutput(string.Format("PostReport_{0}.xlsx", file.Id));

            var package = GenerateExcelPackage(file);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        internal ExcelPackage GenerateExcelPackage(PostFile file)
        {
            var package = new ExcelPackage(new MemoryStream());
            var reportData = file.FileDetails;

            var ws = package.Workbook.Worksheets.Add("Post Data");
            ws.View.ShowGridLines = false;
            ws.Cells.Style.Font.Size = 8;
            ws.Cells.Style.Font.Name = "Tahoma";

            var columnOffset = 1;
            _BuildCommonHeader(ws, 1, ref columnOffset, file);

            // tables
            var rowOffset = 2;
            columnOffset = 1;

            var postingBookMonthAndYear = _GetPostingBookMonthAndYear(file.PostingBookId);
            var playbackTypeDescription = _GetPlaybackTypeDescription(file.PlaybackType);

            foreach (var row in reportData)
            {
                ws.Cells[rowOffset, columnOffset++].Value = row.Rank;
                ws.Cells[rowOffset, columnOffset++].Value = row.Market;
                ws.Cells[rowOffset, columnOffset++].Value = row.Station;
                ws.Cells[rowOffset, columnOffset++].Value = row.Affiliate;
                ws.Cells[rowOffset, columnOffset++].Value = row.Weekstart.ToString(@"M\/d\/yyyy");
                ws.Cells[rowOffset, columnOffset++].Value = row.Day;
                ws.Cells[rowOffset, columnOffset++].Value = row.Date.ToString(@"M\/d\/yyyy");
                ws.Cells[rowOffset, columnOffset++].Value = row.Date.Add(TimeSpan.FromSeconds(row.TimeAired)).ToString(@"h\:m\:ss tt");
                ws.Cells[rowOffset, columnOffset++].Value = row.ProgramName;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotLength;
                ws.Cells[rowOffset, columnOffset++].Value = row.HouseISCI;
                ws.Cells[rowOffset, columnOffset++].Value = row.ClientISCI;
                ws.Cells[rowOffset, columnOffset++].Value = row.Advertiser;
                ws.Cells[rowOffset, columnOffset++].Value = row.InventorySource;
                ws.Cells[rowOffset, columnOffset++].Value = row.InventorySourceDaypart;
                ws.Cells[rowOffset, columnOffset++].Value = row.InventoryOutOfSpecReason;
                ws.Cells[rowOffset, columnOffset++].Value = row.EstimateID;
                ws.Cells[rowOffset, columnOffset++].Value = row.DetectedVia;
                ws.Cells[rowOffset, columnOffset++].Value = row.Spot;
                ws.Cells[rowOffset, columnOffset++].Value = postingBookMonthAndYear;
                ws.Cells[rowOffset, columnOffset++].Value = playbackTypeDescription;
                var imp = row.Impressions.ToDictionary(i => i.Demo);
                foreach (var demo in file.Demos)
                {
                    var value = imp.ContainsKey(demo) ? EquivalizeImpressions(file.Equivalized, row.SpotLength, imp[demo].Impression)
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

        private void _BuildCommonHeader(ExcelWorksheet ws, int rowOffset, ref int columnOffset, PostFile scheduleReportDto)
        {
            // header
            foreach (var header in ExcelFileHeaders)
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

        private string _GetPlaybackTypeDescription(ProposalEnums.ProposalPlaybackType playbackType)
        {
            var playbackTypes = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalPlaybackType>();
            return playbackTypes.Where(p => p.Id == (int) playbackType).Select(p => p.Display).First();
        }

        internal static double EquivalizeImpressions(bool isEquivalized, int spotLength, double impressions)
        {
            if (!isEquivalized)
                return impressions;

            switch (spotLength)
            {
                case 15:
                    return impressions / 2;
                case 30:
                    return impressions;
                case 60:
                    return impressions * 2;
                case 90:
                    return impressions * 3;
                case 120:
                    return impressions * 4;
                default:
                    return impressions;

            }
        }
    }
}
