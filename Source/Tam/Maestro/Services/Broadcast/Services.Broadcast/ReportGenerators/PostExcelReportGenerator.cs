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
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.ReportGenerators
{
    public class PostExcelReportGenerator : BroadcastBaseClass, IReportGenerator<PostPrePostingFile>
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

        public ExcelPackageWrapper GetExcelPackageBase(PostPrePostingFile prePostingFile)
        {
            var package = new ExcelPackage(new MemoryStream());
            var ws = package.Workbook.Worksheets.Add("Post Data");
            var columnOffset = 1;
            _BuildCommonHeader(ws, 1, ref columnOffset, prePostingFile);

            var postingBookMonthAndYear = _GetPostingBookMonthAndYear(prePostingFile.PostingBookId);
            var playbackTypeDescription = _GetPlaybackTypeDescription(prePostingFile.PlaybackType);

            var result = new ExcelPackageWrapper
            {
                PrePostingFile = prePostingFile,
                Package = package,
                PostingBookMonthAndYear = postingBookMonthAndYear,
                PlaybackTypeDescription = playbackTypeDescription
            };

            return result;
        }

        public class ExcelPackageWrapper
        {
            public PostPrePostingFile PrePostingFile { get; set; }
            public ExcelPackage Package { get; set; }
            public int RowOffset { get; set; } = 2;
            public string PostingBookMonthAndYear { get; set; }
            public string PlaybackTypeDescription { get; set; }
            public int TotalDetailsCount { get; set; } = 0;
        }

        public ReportOutput FinalizeReportOutput(ExcelPackageWrapper packageWrapper)
        {
            var output = new ReportOutput(string.Format("PostReport_{0}.xlsx", packageWrapper.PrePostingFile.Id));
            var columnOffset = 1;

            var ws = packageWrapper.Package.Workbook.Worksheets["Post Data"];

            ws.View.ShowGridLines = false;
            ws.Cells.Style.Font.Size = 8;
            ws.Cells.Style.Font.Name = "Tahoma";
            for (var i = 1; i <= columnOffset; i++)
            {
                ws.Cells[packageWrapper.TotalDetailsCount + 2, i].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            }

            ws.Cells.AutoFitColumns();

            packageWrapper.Package.SaveAs(output.Stream);
            packageWrapper.Package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        public void AppendDetailsToReport(ExcelPackageWrapper packageWrapper, List<PostFileDetail> details)
        {
            //const string TIMER_TOTAL_DURATION = "Total Duration";
            //const string TIMER_STEP_GET_POSTING_BOOK = "Get Posting Book";
            //const string TIMER_STEP_GET_PLAYBACK_TYPE = "Get Playback Type";
            //const string TIMER_STEP_ITERATION_ONE = "Iteration One";
            //const string TIMER_STEP_ITERATION_TWO = "Iteration Two";
            //const string TIMER_STEP_SET_BORDERS = "Set Borders";

            var prePostingFile = packageWrapper.PrePostingFile;
            var package = packageWrapper.Package;

            //var lineCount = details.Count;
            //var demoCount = prePostingFile.DemoLookups.Count;
            //var iterations = lineCount * demoCount;
            //var countString = $"FileDetailLineCount : {lineCount}; DemoCount : {demoCount}; Iterations : {iterations};";

            //_LogInfo($"GenerateExcelPackage beginning. {countString}");

            //var timers = new ProcessWorkflowTimers();
            //timers.Start(TIMER_TOTAL_DURATION);

            
            var ws = package.Workbook.Worksheets["Post Data"];

            //var columnOffset = 1;
            //_BuildCommonHeader(ws, 1, ref columnOffset, prePostingFile);

            // tables
            var rowOffset = packageWrapper.RowOffset;
            var columnOffset = 1;

            //timers.Start(TIMER_STEP_GET_POSTING_BOOK);
            //var postingBookMonthAndYear = _GetPostingBookMonthAndYear(prePostingFile.PostingBookId);
            //timers.End(TIMER_STEP_GET_POSTING_BOOK);

            //timers.Start(TIMER_STEP_GET_PLAYBACK_TYPE);
            //var playbackTypeDescription = _GetPlaybackTypeDescription(prePostingFile.PlaybackType);
            //timers.End(TIMER_STEP_GET_PLAYBACK_TYPE);

            //var iterationCounter = 1;

            foreach (var row in details)
            {
                //if (iterationCounter == 1)
                //{
                //    timers.Start(TIMER_STEP_ITERATION_ONE);
                //}
                //else if (iterationCounter == 2)
                //{
                //    timers.Start(TIMER_STEP_ITERATION_TWO);
                //}

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
                ws.Cells[rowOffset, columnOffset++].Value = packageWrapper.PostingBookMonthAndYear;
                ws.Cells[rowOffset, columnOffset++].Value = packageWrapper.PlaybackTypeDescription;

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
                //iterationCounter++;
            }

            packageWrapper.RowOffset = rowOffset;
            packageWrapper.TotalDetailsCount += details.Count;
            
        }

        internal ExcelPackage GenerateExcelPackage(PostPrePostingFile prePostingFile)
        {
            const string TIMER_TOTAL_DURATION = "Total Duration";
            const string TIMER_STEP_GET_POSTING_BOOK = "Get Posting Book";
            const string TIMER_STEP_GET_PLAYBACK_TYPE = "Get Playback Type";
            const string TIMER_STEP_ITERATION_ONE = "Iteration One";
            const string TIMER_STEP_ITERATION_TWO = "Iteration Two";
            const string TIMER_STEP_SET_BORDERS = "Set Borders";            

            var lineCount = prePostingFile.FileDetails.Count;
            var demoCount = prePostingFile.DemoLookups.Count;
            var iterations = lineCount * demoCount;
            var countString = $"FileDetailLineCount : {lineCount}; DemoCount : {demoCount}; Iterations : {iterations};";

            _LogInfo($"GenerateExcelPackage beginning. {countString}");

            var timers = new ProcessWorkflowTimers();
            timers.Start(TIMER_TOTAL_DURATION);

            try
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

                timers.Start(TIMER_STEP_GET_POSTING_BOOK);
                var postingBookMonthAndYear = _GetPostingBookMonthAndYear(prePostingFile.PostingBookId);
                timers.End(TIMER_STEP_GET_POSTING_BOOK);

                timers.Start(TIMER_STEP_GET_PLAYBACK_TYPE);
                var playbackTypeDescription = _GetPlaybackTypeDescription(prePostingFile.PlaybackType);
                timers.End(TIMER_STEP_GET_PLAYBACK_TYPE);

                var iterationCounter = 1;

                foreach (var row in reportData)
                {
                    if (iterationCounter == 1)
                    {
                        timers.Start(TIMER_STEP_ITERATION_ONE);
                    }
                    else if (iterationCounter == 2)
                    {
                        timers.Start(TIMER_STEP_ITERATION_TWO);
                    }

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
                    iterationCounter++;

                    if (iterationCounter == 1)
                    {
                        timers.End(TIMER_STEP_ITERATION_ONE);
                    }
                    else if (iterationCounter == 2)
                    {
                        timers.End(TIMER_STEP_ITERATION_TWO);
                    }
                }

                timers.Start(TIMER_STEP_SET_BORDERS);
                for (var i = 1; i <= columnOffset; i++)
                {
                    ws.Cells[reportData.Count + 2, i].Style.Border.Top.Style = ExcelBorderStyle.Thick;
                }
                timers.End(TIMER_STEP_SET_BORDERS);

                ws.Cells.AutoFitColumns();
                return package;
            }
            finally
            {
                timers.End(TIMER_TOTAL_DURATION);
                var timersReport = timers.ToString();
                _LogInfo($"GenerateExcelPackage completed. {countString} Timers Report : '{timersReport}'");
            }
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
                .Where(m => m.Crunched == CrunchStatusEnum.Crunched && m.MediaMonth.Id == postingBookId)
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
