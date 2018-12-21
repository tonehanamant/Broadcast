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
    public class PostReportGenerator : IReportGenerator<PostReport>
    {
        private static readonly HashSet<string> _QuarterTableHeader = new HashSet<string>
        {
            RANK,
            MARKET,
            STATION,
            NETWORK,
            WEEK_START,
            DAY,
            DATE,
            TIME,
            PROGRAM,
            LENGTH,
            ISCI,
            ADVERTISER,
            BRAND,
            DAYPART,
            SPOT,
            POSTING_BOOK,
            PLAYBACK_TYPE
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
        private const string TOTAL_DEMO = "Total Demo Impressions (000)";
        private const string CPM = "CPM";

        private const string RANK = "Rank";
        private const string MARKET = "Market";
        private const string STATION = "Station";
        private const string NETWORK = "Network Affiliate";
        private const string WEEK_START = "Week Start";
        private const string DAY = "Day";
        private const string DATE = "Date";
        private const string TIME = "Time";
        private const string PROGRAM = "Program";
        private const string LENGTH = "Length";
        private const string ISCI = "ISCI";
        private const string ADVERTISER = "Advertiser";
        private const string BRAND = "Brand";
        private const string DAYPART = "Daypart";
        private const string SPOT = "Spot";
        private const string POSTING_BOOK = "Posting Book";
        private const string PLAYBACK_TYPE = "Playback type";

        private const string BLUE_COLOR_CODE = "#A1E5FD";
        private const string GRAY_COLOR_CODE = "#b4bac4";
        private const string IMPRESSIONS_FORMAT = "#,##0,";
        private const string QUARTER_TAB_IMPRESSIONS_FORMAT = "#,##0.0000";
        private const string MONEY_FORMAT = "$###,###,##0";
        private const string PERCENTAGE_FORMAT = "#0.00%";
        private const string FONT_SUMMARY_TAB = "Calibri";
        private const string FONT_QUARTER_TAB = "Tahoma";
        private const int FONT_SIZE_SUMMARY_TAB = 12;
        private const int FONT_SIZE_QUARTER_TAB = 8;

        private readonly Color HEADER_BACKGROUND_COLOR = ColorTranslator.FromHtml(BLUE_COLOR_CODE);
        private readonly Color NOTES_BACKGROUND_COLOR = ColorTranslator.FromHtml(GRAY_COLOR_CODE);

        private readonly Image _Logo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logo">Image object used as logo</param>
        public PostReportGenerator(Image logo)
        {
            _Logo = logo;
        }

        /// <summary>
        /// Generates a NSI Post Report
        /// </summary>
        /// <param name="reportData">Data object used to generate the report</param>
        /// <returns>ReportOutput object containing the generated file stream</returns>
        public ReportOutput Generate(PostReport reportData)
        {
            var output = new ReportOutput(reportData.ReportName);

            var package = _GenerateExcelPackage(reportData);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage _GenerateExcelPackage(PostReport reportData)
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

        private void _AddQuarterTab(ExcelWorksheet ws, PostReport reportData, PostReport.NsiPostReportQuarterTab quarterTab)
        {
            var excelPicture = ws.Drawings.AddPicture("logo", _Logo);
            excelPicture.SetPosition(1, 0, 1, 0);
            excelPicture.SetSize(50);

            ws.View.ShowGridLines = false;
            ws.Cells.Style.Font.Size = FONT_SIZE_QUARTER_TAB;
            ws.Cells.Style.Font.Name = FONT_QUARTER_TAB;

            ws.Cells["I4"].Value = quarterTab.Title;
            ws.Cells["I4"].Style.Font.Bold = true;
            ws.Cells["I4"].Style.Font.Size = 10;
            ws.Cells["I4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            const int firstDataRow = 10;
            const int firstDataColumn = 2;
            var columnOffset = firstDataColumn;
            var rowOffset = firstDataRow;

            _BuildCommonHeader(ws, reportData);
            ws.View.FreezePanes(firstDataRow, 1);

            // tables
            foreach (var row in quarterTab.TabRows)
            {
                ws.Cells[rowOffset, columnOffset++].Value = row.Rank;
                ws.Cells[rowOffset, columnOffset++].Value = row.Market;
                ws.Cells[rowOffset, columnOffset++].Value = row.Station;
                ws.Cells[rowOffset, columnOffset++].Value = row.NetworkAffiliate;
                ws.Cells[rowOffset, columnOffset++].Value = row.WeekStart.ToString(@"M\/d\/yyyy");
                ws.Cells[rowOffset, columnOffset++].Value = row.DateAired.DayOfWeek;
                ws.Cells[rowOffset, columnOffset++].Value = row.DateAired.ToString(@"M\/d\/yyyy");
                ws.Cells[rowOffset, columnOffset++].Value = row.DateAired.Add(TimeSpan.FromSeconds(row.TimeAired)).ToString(@"h\:mm\:ss tt");
                ws.Cells[rowOffset, columnOffset++].Value = row.ProgramName;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotLength;
                ws.Cells[rowOffset, columnOffset++].Value = row.Isci;
                ws.Cells[rowOffset, columnOffset++].Value = row.Advertiser;
                ws.Cells[rowOffset, columnOffset++].Value = row.Brand ?? string.Empty;
                ws.Cells[rowOffset, columnOffset++].Value = row.DaypartName;
                ws.Cells[rowOffset, columnOffset++].Value = 1; // should be always 1 spot
                ws.Cells[rowOffset, columnOffset++].Value = row.ProposalDetailPostingBook ?? string.Empty;
                ws.Cells[rowOffset, columnOffset++].Value = row.ProposalDetailPlaybackType ?? string.Empty;

                foreach (var demo in reportData.ProposalAudiences.Select(a => a.Id).ToList())
                {
                    var value = row.AudienceImpressions.ContainsKey(demo) ? row.AudienceImpressions[demo] / 1000 : 0;
                    ws.Cells[rowOffset, columnOffset].Style.Numberformat.Format = QUARTER_TAB_IMPRESSIONS_FORMAT;
                    ws.Cells[rowOffset, columnOffset].Value = value;
                    columnOffset++;
                }
                                
                //Apply formatting to every cell. Using ranges you cannot create the correct border
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

        private void _AddSummaryTab(ExcelWorksheet wsSummary, PostReport reportData)
        {
            wsSummary.View.ShowGridLines = false;
            wsSummary.Cells.Style.Font.Size = FONT_SIZE_SUMMARY_TAB;
            wsSummary.Cells.Style.Font.Name = FONT_SUMMARY_TAB;
            int rowOffset = _AddSummaryTabHeader(wsSummary, reportData);
            rowOffset = _AddSummaryTabQuartersTable(wsSummary, reportData, rowOffset);

            if (!string.IsNullOrEmpty(reportData.ProposalNotes))
            {
                _AddProposalNotes(wsSummary, rowOffset, reportData.ProposalNotes);
            }
        }

        private void _AddProposalNotes(ExcelWorksheet wsSummary, int rowOffset, string proposalNotes)
        {
            int offset = proposalNotes.Length / 120 + proposalNotes.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
            wsSummary.Cells[$"B{rowOffset}"].Value = proposalNotes;
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset + offset}"].Merge = true;
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset + offset}"].Style.WrapText = true;
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset + offset}"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset + offset}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset + offset}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset + offset}"].Style.Fill.BackgroundColor.SetColor(NOTES_BACKGROUND_COLOR);
        }

        private int _AddSummaryTabQuartersTable(ExcelWorksheet wsSummary, PostReport reportData, int rowOffset)
        {
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
                    wsSummary.Cells[$"F{rowOffset}"].Value = row.ProposalWeekCost;
                    wsSummary.Cells[$"G{rowOffset}"].Value = row.ProposalWeekTotalCost;
                    wsSummary.Cells[$"I{rowOffset}"].Value = row.ProposalWeekImpressionsGoal;
                    wsSummary.Cells[$"J{rowOffset}"].Value = row.ProposalWeekTotalImpressionsGoal;
                    wsSummary.Cells[$"K{rowOffset}"].Value = row.ProposalWeekCPM;
                    wsSummary.Cells[$"M{rowOffset}"].Value = row.ActualImpressions;
                    wsSummary.Cells[$"N{rowOffset}"].Value = row.DeliveredImpressionsPercentage;
                    rowOffset++;
                }

                wsSummary.Cells[$"B{rowOffset}"].Value = "Total";
                wsSummary.Cells[$"D{rowOffset}"].Formula = $"SUM(D{firstTableRow}:D{rowOffset - 1})";
                wsSummary.Cells[$"G{rowOffset}"].Formula = $"SUM(G{firstTableRow}:G{rowOffset - 1})";
                wsSummary.Cells[$"J{rowOffset}"].Formula = $"SUM(J{firstTableRow}:J{rowOffset - 1})";
                wsSummary.Cells[$"K{rowOffset}"].Formula = $"G{rowOffset} / J{rowOffset} * 1000";
                wsSummary.Cells[$"M{rowOffset}"].Formula = $"SUM(M{firstTableRow}:M{rowOffset - 1})";
                wsSummary.Cells[$"N{rowOffset}"].Formula = $"M{rowOffset} / J{rowOffset}";

                //styles for the table header
                _ApplyStylesForSummaryQuarterTable(wsSummary, rowOffset, firstTableRow);

                rowOffset += 2;
            }

            return rowOffset;
        }

        private void _ApplyStylesForSummaryQuarterTable(ExcelWorksheet wsSummary, int rowOffset, int firstTableRow)
        {
            int tableHeaderRowIndex = firstTableRow - 1;
            wsSummary.Cells[$"B{tableHeaderRowIndex - 1}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);  //table name
            wsSummary.Cells[$"M{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex - 1}"].Merge = true;     //Post section header
            wsSummary.Cells[$"B{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsSummary.Cells[$"B{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.Font.Bold = true;

            wsSummary.Cells[$"B{tableHeaderRowIndex}:K{tableHeaderRowIndex}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSummary.Cells[$"B{tableHeaderRowIndex}:K{tableHeaderRowIndex}"].Style.Fill.BackgroundColor.SetColor(HEADER_BACKGROUND_COLOR);

            wsSummary.Cells[$"M{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSummary.Cells[$"M{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.Fill.BackgroundColor.SetColor(HEADER_BACKGROUND_COLOR);
            wsSummary.Cells[$"B{tableHeaderRowIndex}:K{tableHeaderRowIndex}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);
            wsSummary.Cells[$"M{tableHeaderRowIndex - 1}:N{tableHeaderRowIndex}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            //styles for data
            wsSummary.Cells[$"M{firstTableRow}:M{rowOffset}"].Style.Numberformat.Format = IMPRESSIONS_FORMAT;
            wsSummary.Cells[$"N{firstTableRow}:N{rowOffset}"].Style.Numberformat.Format = PERCENTAGE_FORMAT;
            wsSummary.Cells[$"I{firstTableRow}:J{rowOffset}"].Style.Numberformat.Format = IMPRESSIONS_FORMAT;
            wsSummary.Cells[$"F{firstTableRow}:G{rowOffset}"].Style.Numberformat.Format = MONEY_FORMAT;
            wsSummary.Cells[$"K{firstTableRow}:K{rowOffset}"].Style.Numberformat.Format = MONEY_FORMAT;
            wsSummary.Cells[$"B{firstTableRow}:N{rowOffset}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //styles for totals
            wsSummary.Cells[$"B{rowOffset}:N{rowOffset}"].Style.Font.Bold = true;

            wsSummary.Cells[$"B{rowOffset}:K{rowOffset}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset}"].Style.Fill.BackgroundColor.SetColor(HEADER_BACKGROUND_COLOR);
            wsSummary.Cells[$"B{rowOffset}:K{rowOffset}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            wsSummary.Cells[$"M{rowOffset}:N{rowOffset}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSummary.Cells[$"M{rowOffset}:N{rowOffset}"].Style.Fill.BackgroundColor.SetColor(HEADER_BACKGROUND_COLOR);
            wsSummary.Cells[$"M{rowOffset}:N{rowOffset}"].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            wsSummary.Cells[$"M{rowOffset}:M{rowOffset}"].Style.Numberformat.Format = IMPRESSIONS_FORMAT;

            wsSummary.Cells[$"B{tableHeaderRowIndex}:N{rowOffset}"].AutoFitColumns();
        }

        private int _AddSummaryTabHeader(ExcelWorksheet wsSummary, PostReport reportData)
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
                { "I9", "Guaranteed Demo:"},
                { "I10", "Post Type:"},
                { "I11", "Unit Length:" },
                { "I12", "Date:"},
                { "I13", "Report:"},
                { "J9", reportData.GuaranteedDemo },
                { "J10", reportData.PostType},
                { "J11", reportData.SpotLengthsDisplay},
                { "J12", DateTime.Now.ToShortDateString()},
            };

            foreach (var item in values)
            {
                wsSummary.Cells[item.Key].Value = item.Value;
            }
            int rowOffset = 17;
            foreach (var item in reportData.FlightDates)
            {
                wsSummary.Cells[$"C{rowOffset++}"].Value = item;
            }

            wsSummary.Cells[$"B{rowOffset}"].Value = "Posting Book:";
            wsSummary.Cells[$"C{rowOffset++}"].Value = string.Join(", ", reportData.PostingBooks);
            wsSummary.Cells[$"B{rowOffset}"].Value = "Playback Type:";
            wsSummary.Cells[$"C{rowOffset++}"].Value = string.Join(", ", reportData.PlaybackTypes);

            wsSummary.Cells[$"B9:B{rowOffset}"].Style.Font.Bold = true;
            wsSummary.Cells[$"B9:B{rowOffset}"].Style.Font.Size = 10;
            wsSummary.Cells[$"B9:B{rowOffset}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            wsSummary.Cells["I9:I13"].Style.Font.Bold = true;
            wsSummary.Cells["I9:I13"].Style.Font.Size = 10;
            wsSummary.Cells["I9:I13"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            return ++rowOffset;
        }

        private void _BuildCommonHeader(ExcelWorksheet ws, PostReport reportData)
        {
            // header
            var rowOffset = 9;
            int columnOffset = 2;
            foreach (var header in _QuarterTableHeader)
            {
                ws.Cells[rowOffset, columnOffset++].Value = header;
            }

            foreach (var audience in reportData.ProposalAudiences)
            {
                ws.Cells[rowOffset, columnOffset++].Value = $"{audience.Display} (000)";
            }
            
            ws.Cells[rowOffset, 2, rowOffset, columnOffset - 1].Style.Font.Bold = true;
            ws.Cells[rowOffset, 2, rowOffset, columnOffset - 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            ws.Cells[rowOffset, 2, rowOffset, columnOffset - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[rowOffset, 2, rowOffset, columnOffset - 1].Style.Fill.BackgroundColor.SetColor(HEADER_BACKGROUND_COLOR);
            ws.Cells[rowOffset, 2, rowOffset, columnOffset - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
    }
}
