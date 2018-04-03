using Common.Services.Repositories;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.ReportGenerators
{
    public class NSIPostReportGenerator : IReportGenerator<NsiPostReport>
    {
        private static readonly HashSet<string> _ExcelFileHeaders = new HashSet<string>
        {
            RANK,
            MARKET,
            STATION,
            NETWORK,
            WEEK_START,
            DATE,
            TIME,
            PROGRAM,
            LENGTH,
            ISCI,
            ADVERTISER,
            DAYPART
        };

        private const string RANK = "Rank";
        private const string MARKET = "Market";
        private const string STATION = "Station";
        private const string NETWORK = "Network Affiliate";
        private const string WEEK_START = "Week Start";
        private const string DATE = "Date";
        private const string TIME = "Time";
        private const string PROGRAM = "Program";
        private const string LENGTH = "Length";
        private const string ISCI = "ISCI";
        private const string ADVERTISER = "Advertiser";
        private const string DAYPART = "Daypart";
        
        private readonly IDataRepositoryFactory _Factory;
        private readonly Image _Logo;


        public NSIPostReportGenerator(IDataRepositoryFactory factory, Image logo)
        {
            _Factory = factory;
            _Logo = logo;
        }

        public ReportOutput Generate(NsiPostReport reportData)
        {
            var output = new ReportOutput(string.Format("NSI Post Report_{0}.xlsx", reportData.ProposalId));

            var package = GenerateExcelPackage(reportData);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        internal ExcelPackage GenerateExcelPackage(NsiPostReport reportData)
        {
            var package = new ExcelPackage(new MemoryStream());

            foreach(var quarterTab in reportData.QuarterTabs)
            {
                var ws = package.Workbook.Worksheets.Add(quarterTab.TabName);

                var excelPicture = ws.Drawings.AddPicture("logo", _Logo);
                excelPicture.SetPosition(1, 0, 1, 0);
                excelPicture.SetSize(50);

                ws.View.ShowGridLines = false;
                ws.Cells.Style.Font.Size = 8;
                ws.Cells.Style.Font.Name = "Tahoma";

                var titleRow = 4;
                var titleColumn = 9;
                ws.Cells[titleRow, titleColumn].Value = quarterTab.Title;
                ws.Cells[titleRow, titleColumn].Style.Font.Bold = true;
                ws.Cells[titleRow, titleColumn].Style.Font.Size = 10;
                ws.Cells[titleRow, titleColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var firstDataColumn = 2;
                var columnOffset = firstDataColumn;
                var headerRow = 9;
                _BuildCommonHeader(ws, headerRow, ref columnOffset, reportData);
                ws.View.FreezePanes(headerRow+1,1);

                // tables
                var rowOffset = 10;
                columnOffset = firstDataColumn;

                foreach (var row in quarterTab.TabRows)
                {

                    ws.Cells[rowOffset, columnOffset++].Value = row.Rank;
                    ws.Cells[rowOffset, columnOffset++].Value = row.Market;
                    ws.Cells[rowOffset, columnOffset++].Value = row.Station;
                    ws.Cells[rowOffset, columnOffset++].Value = row.NetworkAffiliate;
                    ws.Cells[rowOffset, columnOffset++].Value = row.WeekStart.ToString(@"M\/d\/yyyy");
                    ws.Cells[rowOffset, columnOffset++].Value = row.DateAired.ToString(@"M\/d\/yyyy");
                    ws.Cells[rowOffset, columnOffset++].Value = row.DateAired.Add(TimeSpan.FromSeconds(row.TimeAired)).ToString(@"h\:mm\:ss tt");
                    ws.Cells[rowOffset, columnOffset++].Value = row.ProgramName;
                    ws.Cells[rowOffset, columnOffset++].Value = row.SpotLength;
                    ws.Cells[rowOffset, columnOffset++].Value = row.Isci;
                    ws.Cells[rowOffset, columnOffset++].Value = row.Advertiser;
                    ws.Cells[rowOffset, columnOffset++].Value = row.DaypartName;
                    foreach (var demo in reportData.ProposalAudiences.Select(a => a.Id).ToList())
                    {
                        var value = row.AudienceImpressions.ContainsKey(demo) ? row.AudienceImpressions[demo] : 0;
                        ws.Cells[rowOffset, columnOffset].Style.Numberformat.Format = "#,#";
                        ws.Cells[rowOffset, columnOffset].Value = value;
                        columnOffset++;
                    }

                    //Apply formatting to the whole row
                    for(int i = firstDataColumn; i < columnOffset; i++)
                    {
                        ws.Cells[rowOffset, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[rowOffset, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    rowOffset++;
                    columnOffset = firstDataColumn;
                }

                ws.Cells.AutoFitColumns();
            }  

            return package;
        }

        private void _BuildCommonHeader(ExcelWorksheet ws, int rowOffset, ref int columnOffset, NsiPostReport reportData)
        {
            // header
            var backgroundColor = System.Drawing.ColorTranslator.FromHtml("#A1E5FD");
            foreach (var header in _ExcelFileHeaders)
            {
                ws.Cells[rowOffset, columnOffset++].Value = header;
            }

            foreach (var audience in reportData.ProposalAudiences)
            {
                ws.Cells[rowOffset, columnOffset++].Value = audience.Display;
            }

            for (var i = 2; i < columnOffset; i++)
            {
                ws.Cells[rowOffset, i].Style.Font.Bold = true;
                ws.Cells[rowOffset, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                ws.Cells[rowOffset, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowOffset, i].Style.Fill.BackgroundColor.SetColor(backgroundColor);
                ws.Cells[rowOffset, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            }
        }
    }
}
