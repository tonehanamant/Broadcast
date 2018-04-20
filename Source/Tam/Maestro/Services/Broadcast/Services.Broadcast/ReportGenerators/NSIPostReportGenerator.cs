using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ReportGenerators
{
    public class NSIPostReportGenerator : IReportGenerator<NsiPostReport>
    {
        private static readonly HashSet<string> _QuarterTableHeader = new HashSet<string>
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

        private static readonly HashSet<string> _QuarterSummaryTableHeader = new HashSet<string>
        {
            CONTRACT,
            WEEK,
            UNITS,
            UNIT_LENGTH,
            COST,
            TOTAL_COST,
            HH_RATING,
            DEMO,
            TOTAL_DEMO,
            CPM
        };

        private const string CONTRACT = "Contract";
        private const string WEEK = "Week";
        private const string UNITS = "Units";
        private const string UNIT_LENGTH = "Unit Length";
        private const string COST = "Cost";
        private const string TOTAL_COST = "Total Cost";
        private const string HH_RATING = "HH Rating";
        private const string DEMO = "Demo (000)";
        private const string TOTAL_DEMO = "Total Demo Impressions";
        private const string CPM = "CPM";

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
        private const string BACKGROUNT_COLOR_CODE = "#A1E5FD";
        private const string NUMBER_FORMAT = "###,###,##0";
        private const string MONEY_FORMAT = "$###,###,##0";
        private const string PERCENTAGE_FORMAT = "#0.00%";
        private Color BACKGROUND_COLOR = ColorTranslator.FromHtml(BACKGROUNT_COLOR_CODE);

        private readonly Image _Logo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logo">Image object used as logo</param>
        public NSIPostReportGenerator(Image logo)
        {
            _Logo = logo;
        }

        /// <summary>
        /// Generates a report of type T
        /// </summary>
        /// <param name="dataObject">Data object used to generate the file</param>
        /// <returns>ReportOutput object containing the generated file stream</returns>
        public ReportOutput Generate(NsiPostReport reportData)
        {
            var output = new ReportOutput(string.Format("NSI Post Report_{0}.xlsx", reportData.ProposalId));

            var package = _GenerateExcelPackage(reportData);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage _GenerateExcelPackage(NsiPostReport reportData)
        {
            var package = new ExcelPackage(new MemoryStream());

            ExcelWorksheet wsSummary = package.Workbook.Worksheets.Add($"{reportData.Advertiser} Post");
            _AddSummaryTab(wsSummary, reportData);

            foreach (var quarterTab in reportData.QuarterTabs)
            {
                var ws = package.Workbook.Worksheets.Add(quarterTab.TabName);
                _AddQuarterTab(ws, reportData, quarterTab);
            }

            return package;
        }

        private void _AddQuarterTab(ExcelWorksheet ws, NsiPostReport reportData, NsiPostReport.NsiPostReportQuarterTab quarterTab)
        {
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
            ws.View.FreezePanes(headerRow + 1, 1);

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
                for (int i = firstDataColumn; i < columnOffset; i++)
                {
                    ws.Cells[rowOffset, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Cells[rowOffset, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                rowOffset++;
                columnOffset = firstDataColumn;
            }

            ws.Cells.AutoFitColumns();
        }

        private void _AddSummaryTab(ExcelWorksheet wsSummary, NsiPostReport reportData)
        {
            wsSummary.View.ShowGridLines = false;
            wsSummary.Cells.Style.Font.Size = 12;
            wsSummary.Cells.Style.Font.Name = "Calibri";
            _AddSummaryTabHeader(wsSummary, reportData);
            _AddSummaryTabQuartersTable(wsSummary, reportData);
        }

        private void _AddSummaryTabQuartersTable(ExcelWorksheet wsSummary, NsiPostReport reportData)
        {
            int rowOffset = 21;

            foreach (var quarterTable in reportData.QuarterTables)
            {
                int columnOffset = 2;
                wsSummary.Cells[$"B{rowOffset}"].Value = quarterTable.TableName;
                wsSummary.Cells[$"M{rowOffset}"].Value = "Post";
                rowOffset++;

                //table header
                foreach (string name in _QuarterSummaryTableHeader)
                {
                    wsSummary.Cells[rowOffset, columnOffset++].Value = name;
                }
                wsSummary.Cells[$"M{rowOffset}"].Value = $"{reportData.GuaranteedDemo} (000)";
                wsSummary.Cells[$"N{rowOffset}"].Value = "% Del";
                rowOffset++;

                //data
                int firstTableRow = rowOffset;
                foreach (var row in quarterTable.TableRows)
                {
                    wsSummary.Cells[$"B{rowOffset}"].Value = row.Contract;
                    wsSummary.Cells[$"C{rowOffset}"].Value = row.WeekStartDate.ToShortDateString();
                    wsSummary.Cells[$"D{rowOffset}"].Value = row.Spots;
                    wsSummary.Cells[$"E{rowOffset}"].Value = row.SpotLength;
                    wsSummary.Cells[$"F{rowOffset}"].Formula = $"G{rowOffset} / D{rowOffset}";
                    wsSummary.Cells[$"G{rowOffset}"].Value = row.ProposalWeekCost;
                    wsSummary.Cells[$"I{rowOffset}"].Formula = $"J{rowOffset} / D{rowOffset}";
                    wsSummary.Cells[$"J{rowOffset}"].Value = row.ProposalWeekImpressionsGoal;
                    wsSummary.Cells[$"K{rowOffset}"].Formula = $"G{rowOffset} / J{rowOffset} * 1000";
                    wsSummary.Cells[$"M{rowOffset}"].Value = row.ActualImpressions;
                    wsSummary.Cells[$"N{rowOffset}"].Formula = $"M{rowOffset} / J{rowOffset}";
                    rowOffset++;
                }

                wsSummary.Cells[$"B{rowOffset}"].Value = "Total";
                wsSummary.Cells[$"D{rowOffset}"].Formula = $"SUM(D{firstTableRow}:D{rowOffset - 1})";
                wsSummary.Cells[$"G{rowOffset}"].Formula = $"SUM(G{firstTableRow}:G{rowOffset - 1})";
                wsSummary.Cells[$"J{rowOffset}"].Formula = $"SUM(J{firstTableRow}:J{rowOffset - 1})";
                wsSummary.Cells[$"K{rowOffset}"].Formula = $"SUM(K{firstTableRow}:K{rowOffset - 1})";
                wsSummary.Cells[$"M{rowOffset}"].Formula = $"SUM(M{firstTableRow}:M{rowOffset - 1})";
                wsSummary.Cells[$"N{rowOffset}"].Formula = $"M{rowOffset} / J{rowOffset}";

                //styles for the table header
                _ApplyStylesForSummaryQuarterTable(wsSummary, rowOffset, firstTableRow);

                rowOffset += 2;
            }
        }

        private void _ApplyStylesForSummaryQuarterTable(ExcelWorksheet wsSummary, int rowOffset, int firstTableRow)
        {
            int tableHeaderRowIndex = firstTableRow - 1;
            wsSummary.Cells[$"B{tableHeaderRowIndex - 1}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);  //table name
            wsSummary.Cells[$"M{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex - 1}"].Merge = true;     //Post section header
            wsSummary.Cells[$"D{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsSummary.Cells[$"B{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.Font.Bold = true;

            wsSummary.Cells[$"B{tableHeaderRowIndex}:K{tableHeaderRowIndex}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSummary.Cells[$"B{tableHeaderRowIndex}:K{tableHeaderRowIndex}"].Style.Fill.BackgroundColor.SetColor(BACKGROUND_COLOR);

            wsSummary.Cells[$"M{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSummary.Cells[$"M{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.Fill.BackgroundColor.SetColor(BACKGROUND_COLOR);
            wsSummary.Cells[$"B{tableHeaderRowIndex}:K{tableHeaderRowIndex}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);
            wsSummary.Cells[$"M{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            //styles for data
            wsSummary.Cells[$"N{firstTableRow}:N{rowOffset}"].Style.Numberformat.Format = PERCENTAGE_FORMAT;
            wsSummary.Cells[$"I{firstTableRow}:J{rowOffset}"].Style.Numberformat.Format = NUMBER_FORMAT;
            wsSummary.Cells[$"F{firstTableRow}:G{rowOffset}"].Style.Numberformat.Format = MONEY_FORMAT;
            wsSummary.Cells[$"K{firstTableRow}:K{rowOffset}"].Style.Numberformat.Format = MONEY_FORMAT;
            wsSummary.Cells[$"D{firstTableRow}:N{rowOffset}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //styles for totals
            wsSummary.Cells[$"B{rowOffset}:N{rowOffset}"].Style.Font.Bold = true;

            wsSummary.Cells[$"B{rowOffset}:K{rowOffset}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset}"].Style.Fill.BackgroundColor.SetColor(BACKGROUND_COLOR);
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            wsSummary.Cells[$"M{rowOffset}:N{rowOffset}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSummary.Cells[$"M{rowOffset}:N{rowOffset}"].Style.Fill.BackgroundColor.SetColor(BACKGROUND_COLOR);
            wsSummary.Cells[$"M{rowOffset}:N{rowOffset}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            wsSummary.Cells[$"B{tableHeaderRowIndex}:N{rowOffset}"].AutoFitColumns();
        }

        private void _AddSummaryTabHeader(ExcelWorksheet wsSummary, NsiPostReport reportData)
        {
            var excelPicture1 = wsSummary.Drawings.AddPicture("logo", _Logo);
            excelPicture1.SetPosition(1, 0, 1, 0);
            excelPicture1.SetSize(75);
            Dictionary<string, string> values = new Dictionary<string, string>() {
                { "B9", "Client:"},
                { "B10", "Contact:"},
                { "B11", "Agency:"},
                { "B12", "Salesperson:"},
                { "B16", "Daypart:"},
                { "B17", "Flight:" },
                { "C9", reportData.Advertiser},
                { "C17", string.Join(" & ", reportData.FlightDates.Select(x=> $"{x.Item1.Value.ToString(@"M\/d\/yyyy")}-{x.Item2.Value.ToString(@"M\/d\/yyyy")}").ToList())},
                { "I9", "Guaranteed Demo:"},
                { "I10", "Post Type:"},
                { "I11", "Unit Length:" },
                { "I12", "Date:"},
                { "I13", "Report:"},
                { "J9", reportData.GuaranteedDemo },
                { "J10", "NSI"},
                { "J11", string.Join(",", reportData.QuarterTabs.SelectMany(x => x.TabRows.Select(y => y.SpotLength)).Distinct().OrderBy(x=>x).Select(x => $":{x}").ToList())},
                { "J12", DateTime.Now.ToShortDateString()},
            };

            foreach (var item in values)
            {
                wsSummary.Cells[item.Key].Value = item.Value;
                if (item.Key.Equals("C17"))
                {
                    wsSummary.Cells[item.Key].Style.Font.Bold = true;
                    wsSummary.Cells[item.Key].Style.Font.Size = 16;
                }
            }

            wsSummary.Cells["B9:B17"].Style.Font.Bold = true;
            wsSummary.Cells["B9:B17"].Style.Font.Size = 10;
            wsSummary.Cells["B9:B17"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            wsSummary.Cells["I9:I13"].Style.Font.Bold = true;
            wsSummary.Cells["I9:I13"].Style.Font.Size = 10;
            wsSummary.Cells["I9:I13"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;            
        }

        private void _BuildCommonHeader(ExcelWorksheet ws, int rowOffset, ref int columnOffset, NsiPostReport reportData)
        {
            // header
            foreach (var header in _QuarterTableHeader)
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
                ws.Cells[rowOffset, i].Style.Fill.BackgroundColor.SetColor(BACKGROUND_COLOR);
                ws.Cells[rowOffset, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            }
        }
    }
}
