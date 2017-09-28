using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ReportGenerators
{
    public class BvsExcelReportGenerator
    {
        private ScheduleReportDto _ScheduleReportDto;
        private Image _Logo;
        private const string DateFormat = "MM/dd/yyyy";

        public BvsExcelReportGenerator(ScheduleReportDto scheduleReportDto, Image logo)
        {
            _ScheduleReportDto = scheduleReportDto;
            _Logo = logo;
        }

        public ReportOutput GetScheduleReport()
        {
            var output = new ReportOutput(string.Format("ScheduleReport_{0}.xlsx", _ScheduleReportDto.ScheduleId));

            using (var package = new ExcelPackage(new MemoryStream()))
            {
                _GenerateAdvertiserDataSheet(package);
                _GenerateBroadcastWeeksDataSheets(package, true);
                _GenerateStationSummaryDataSheet(package);
                _GenerateOutofSpecToDate(package, true);
                _GenerateSpotDetailDataSheet(package, false, false, false);

                package.SaveAs(output.Stream);
                output.Stream.Position = 0;
            }

            return output;
        }

        public ReportOutput GetClientReport()
        {
            var output = new ReportOutput(string.Format("ClientReport_{0}.xlsx", _ScheduleReportDto.ScheduleId));

            using (var package = new ExcelPackage(new MemoryStream()))
            {
                _GenerateAdvertiserDataSheet(package);
                _GenerateBroadcastWeeksDataSheets(package, false);
                _GenerateStationSummaryDataSheet(package);
                _GenerateOutofSpecToDate(package, false);
                _GenerateSpotDetailDataSheet(package, false, true, true);
                _GenerateDeliveryBySourceSheet(package);

                package.SaveAs(output.Stream);
                output.Stream.Position = 0;
            }

            return output;
        }

        public ReportOutput GetProviderReport()
        {
            var output = new ReportOutput(string.Format("ProviderReport_{0}.xlsx", _ScheduleReportDto.ScheduleId));

            using (var package = new ExcelPackage(new MemoryStream()))
            {
                _GenerateAdvertiserDataSheet(package);
                _GenerateBroadcastWeeksDataSheets(package, false);
                _GenerateStationSummaryDataSheet(package);
                _GenerateOutofSpecToDate(package, false);
                _GenerateSpotDetailDataSheet(package, true, false, false);
                _GenerateAdvertiserDelivery(package);

                package.SaveAs(output.Stream);
                output.Stream.Position = 0;
            }

            return output;
        }

        private void _GenerateAdvertiserDelivery(ExcelPackage package)
        {
            var deliveryByAdvertiser = _ScheduleReportDto.SpotsAndImpressionsDeliveryByAdvertiser;
            var tabName = "Advertiser Delivery";
            if (package.Workbook.Worksheets[tabName] != null)
            {
                package.Workbook.Worksheets.Delete(tabName);
            }
            var ws = package.Workbook.Worksheets.Add(tabName);

            ws.Row(1).Height = 25.5;
            ws.View.ShowGridLines = true;
            ws.Cells.Style.Font.Size = 9;
            ws.Cells.Style.Font.Name = "Calibri";

            var columnOffset = 1;

            ws.Cells[1, columnOffset++].Value = "Advertiser";
            ws.Cells[1, columnOffset++].Value = "Spots";

            foreach (var audience in _ScheduleReportDto.StationSummaryData.ScheduleAudiences)
            {
                var column = columnOffset++;
                ws.Cells[1, column].Value = "Delivered Impressions (" + audience.AudienceName + ")";
                ws.Column(column).Style.Numberformat.Format = "#,0";
            }

            for (int i = 1; i <= columnOffset; i++)
            {
                ws.Cells[1, i].Style.Font.Bold = true;
                ws.Cells[1, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            var rowOffset = 2;
            foreach (var row in deliveryByAdvertiser)
            {
                columnOffset = 1;
                ws.Cells[rowOffset, columnOffset++].Value = row.AdvertiserName;
                ws.Cells[rowOffset, columnOffset++].Value = row.Spots;
                foreach (var audience in _ScheduleReportDto.StationSummaryData.ScheduleAudiences)
                {
                    ws.Cells[rowOffset, columnOffset++].Value = _FormatImpressionsValue(row.AudienceImpressions[audience.AudienceId]);
                }
                rowOffset++;
            }

            ws.Cells.AutoFitColumns();
        }

        private void _GenerateAdvertiserDataSheet(ExcelPackage package)
        {
            var reportData = _ScheduleReportDto.AdvertiserData.ReportData;

            if (package.Workbook.Worksheets["Advertiser - Quarter"] != null)
                package.Workbook.Worksheets.Delete("Advertiser - Quarter");

            var ws = package.Workbook.Worksheets.Add("Advertiser - Quarter");
            ws.Row(1).Height = 25.5;
            ws.View.ShowGridLines = true;
            ws.Cells.Style.Font.Size = 9;
            ws.Cells.Style.Font.Name = "Calibri";

            var columnOffset = 1;
            _BuildCommonHeader(ws, 1, ref columnOffset, reportData.Count, false, false);

            // tables
            var rowOffset = 2;
            columnOffset = 1;
            //ws.Cells[2, 1].LoadFromCollection(reportData);
            foreach (var row in reportData)
            {
                ws.Cells[rowOffset, columnOffset++].Value = row.Rank;
                ws.Cells[rowOffset, columnOffset++].Value = row.Market;
                ws.Cells[rowOffset, columnOffset++].Value = row.Station;
                ws.Cells[rowOffset, columnOffset++].Value = row.Affiliate;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotLength;
                ws.Cells[rowOffset, columnOffset++].Value = row.ProgramName;
                ws.Cells[rowOffset, columnOffset++].Value = row.DisplayDaypart;
                ws.Cells[rowOffset, columnOffset++].Value = row.Cost;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotCost;
                ws.Cells[rowOffset, columnOffset++].Value = row.OrderedSpots;
                ws.Cells[rowOffset, columnOffset++].Value = row.DeliveredSpots;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotClearance;
                rowOffset++;
                columnOffset = 1;
            }

            // add audience and delivery
            _SetAudienceData(ws, 1, 13, reportData);

            _BuildCommonTotalsRow(ws, "J", "K", "L", _ScheduleReportDto.AdvertiserData.OrderedSpots, _ScheduleReportDto.AdvertiserData.DeliveredSpots, reportData.Count);

            // sumarry rows
            columnOffset = 13;
            var summaryIndexRowCell = reportData.Count + 4;

            foreach (var impressionsAndDelivery in _ScheduleReportDto.AdvertiserData.ImpressionsAndDelivey)
            {
                _BuildAudienceAndDeliveryTotalsRow(ws, impressionsAndDelivery, reportData.Count, true, ref columnOffset);
                _SetSummaryData(ws, "Quarterly Delivery", impressionsAndDelivery.AudienceName, impressionsAndDelivery.OrderedImpressions, impressionsAndDelivery.DeliveredImpressions, ref summaryIndexRowCell);
            }

            for (int i = 1; i <= columnOffset; i++)
            {
                ws.Cells[reportData.Count + 2, i].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            }

            ws.Cells.AutoFitColumns();
        }

        private void _GenerateBroadcastWeeksDataSheets(ExcelPackage package, bool includeDateColumns)
        {
            // add weekly detail information
            foreach (var weeklyData in _ScheduleReportDto.WeeklyData.ReportDataByWeek)
            {
                var sheetName = "Week In Spec - " + weeklyData.Week.Display.Replace('/', '.');
                if (package.Workbook.Worksheets[sheetName] != null)
                    package.Workbook.Worksheets.Delete(sheetName);

                var wsInspec = package.Workbook.Worksheets.Add(sheetName);
                _SetWeeklyTab(wsInspec, weeklyData, true, includeDateColumns);

                sheetName = "Week Out of Spec - " + weeklyData.Week.Display.Replace('/', '.');
                if (package.Workbook.Worksheets[sheetName] != null)
                    package.Workbook.Worksheets.Delete(sheetName);

                var wsOutofSpec = package.Workbook.Worksheets.Add(sheetName);
                _SetWeeklyTab(wsOutofSpec, weeklyData, false, includeDateColumns);
            }
        }

        private void _SetWeeklyTab(ExcelWorksheet ws, WeeklyImpressionAndDeliveryDto weeklyData, bool isInSpec, bool includeDateColumns)
        {
            ws.Cells.Style.Font.Size = 9;
            ws.Cells.Style.Font.Name = "Calibri";

            ws.Row(1).Height = 25.5;
            ws.View.ShowGridLines = true;
            var reportData = weeklyData.GetReportData(isInSpec);
            var isOutOfSpec = !isInSpec;

            int columnOffset = 1;
            _BuildCommonHeader(ws, 1, ref columnOffset, reportData.Count(), isOutOfSpec, includeDateColumns);
            ws.Cells[1, columnOffset].Value = "Spec Status";

            // tables
            var rowOffset = 2;
            columnOffset = 1;
            foreach (BvsReportData row in reportData)
            {
                ws.Cells[rowOffset, columnOffset++].Value = row.Rank;
                ws.Cells[rowOffset, columnOffset++].Value = row.Market;
                ws.Cells[rowOffset, columnOffset++].Value = row.Station;
                ws.Cells[rowOffset, columnOffset++].Value = row.Affiliate;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotLength;
                ws.Cells[rowOffset, columnOffset++].Value = row.ProgramName;
                ws.Cells[rowOffset, columnOffset++].Value = row.DisplayDaypart;
                if (isOutOfSpec)
                {
                    ws.Cells[rowOffset, columnOffset++].Value = row.Isci;
                }
                if (isOutOfSpec && includeDateColumns)
                {
                    ws.Cells[rowOffset, columnOffset++].Value = row.BvsDate.ToString(DateFormat);
                    ws.Cells[rowOffset, columnOffset++].Value = row.BroadcastDate.ToString(DateFormat);
                    ws.Cells[rowOffset, columnOffset++].Value = row.TimeAired.ToShortTimeString();
                }
                ws.Cells[rowOffset, columnOffset++].Value = row.Cost;
                ws.Cells[rowOffset, columnOffset++].Value = isOutOfSpec ? null : row.SpotCost;
                ws.Cells[rowOffset, columnOffset++].Value = row.OrderedSpots;
                ws.Cells[rowOffset, columnOffset++].Value = row.DeliveredSpots;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotClearance;
                ws.Cells[rowOffset, columnOffset + _ScheduleReportDto.WeeklyData.ScheduleAudiences.Count() * 2].Value
                    = row.SpecStatus;
                if (!row.SpecStatus.Equals("Match"))
                    ws.Cells[rowOffset, columnOffset + _ScheduleReportDto.WeeklyData.ScheduleAudiences.Count() * 2].Style.Font.Color.SetColor(Color.Red);

                rowOffset++;
                columnOffset = 1;
            }
            
                columnOffset = 13;
            if (isOutOfSpec)
                columnOffset = 14;

            if (isOutOfSpec)
                columnOffset = 14;

            if (isOutOfSpec && includeDateColumns)
                columnOffset = 17;

            // add audience and delivery
            _SetAudienceData(ws, 1, columnOffset, reportData);

            var orderedSpots = isInSpec ? weeklyData.OrderedSpots() : 0;

            _BuildWeeklyTabTotalRows(ws, isOutOfSpec, includeDateColumns, orderedSpots, weeklyData.DeliveredSpots(isInSpec),
                    reportData.Count);

            // sumarry rows
            var summaryIndexRowCell = reportData.Count + 4;

            foreach (var impressionsAndDelivery in weeklyData.ImpressionsAndDelivery)
            {
                _BuildAudienceAndDeliveryTotalsRow(ws, impressionsAndDelivery, reportData.Count, isInSpec, ref columnOffset);
                if (isInSpec)
                    _BuildSummaryRow(ws, impressionsAndDelivery, impressionsAndDelivery.AudienceName, "Weekly Delivery", ref summaryIndexRowCell);
                else
                    _BuildOutOfSpecSummaryRow(ws, impressionsAndDelivery, impressionsAndDelivery.AudienceName, "Out of spec Weekly Delivery",
                        summaryIndexRowCell++);
            }

            for (var i = 1; i <= columnOffset; i++)
            {
                ws.Cells[reportData.Count + 2, i].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            }

            ws.Cells.AutoFitColumns();
        }

        /// <summary>
        /// Build the total columns for the weekly tabs. The total values are shifted forward if extra columns are to be displayed to the spreadsheet.
        /// </summary>
        private void _BuildWeeklyTabTotalRows(ExcelWorksheet ws, bool isOutOfSpec, bool includeDateColumns, int? orderedSpots, int? deliveredSpots, int count)
        {
            var orderedSpotsColumn = "J";
            var deliveredSpotsColumn = "K";
            var spotClearanceColumn = "L";

            if (isOutOfSpec)
            {
                orderedSpotsColumn = "K";
                deliveredSpotsColumn = "L";
                spotClearanceColumn = "M";
            }

            if (isOutOfSpec && includeDateColumns)
            {
                orderedSpotsColumn = "N";
                deliveredSpotsColumn = "O";
                spotClearanceColumn = "P";
            }

            _BuildCommonTotalsRow(ws, orderedSpotsColumn, deliveredSpotsColumn, spotClearanceColumn, orderedSpots, deliveredSpots,
                    count);
        }

        private void _GenerateStationSummaryDataSheet(ExcelPackage package)
        {
            if (package.Workbook.Worksheets["Station Summary"] != null)
                package.Workbook.Worksheets.Delete("Station Summary");

            var ws = package.Workbook.Worksheets.Add("Station Summary");

            var stationSummary = _ScheduleReportDto.StationSummaryData;
            var reportData = stationSummary.ReportData;

            ws.Row(1).Height = 25.5;
            ws.View.ShowGridLines = true;
            ws.Cells.Style.Font.Size = 9;
            ws.Cells.Style.Font.Name = "Calibri";

            var columnOffset = 1;

            ws.Cells[1, columnOffset++].Value = "Rank";
            ws.Cells[1, columnOffset++].Value = "Market";
            ws.Cells[1, columnOffset++].Value = "Station";
            ws.Cells[1, columnOffset++].Value = "Affiliate";
            ws.Cells[1, columnOffset++].Value = "Spot Length";
            ws.Cells[1, columnOffset++].Value = "Ordered Spots";
            ws.Cells[1, columnOffset++].Value = "Delivered Spots";
            ws.Cells[1, columnOffset++].Value = "Spot Clearance";

            if (reportData.Count > 0)
            {   // clearance
                ws.Cells["H2:H" + (reportData.Count + 1)].Style.Numberformat.Format = "0.00%";// spot clearance column format             
            }

            _SetOrderedAndDeliveredImpressionsColumn(1, ref columnOffset, ws);

            ws.Cells[1, columnOffset++].Value = "Spec Status";
            ws.Cells[1, columnOffset].Value = "Out of Spec Spots";

            for (int i = 1; i <= columnOffset; i++)
            {
                ws.Cells[1, i].Style.Font.Bold = true;
                ws.Cells[1, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            var rowOffset = 2;
            columnOffset = 1;
            var outOfSpecCol = 1;
            foreach (var row in reportData)
            {
                ws.Cells[rowOffset, columnOffset++].Value = row.Rank;
                ws.Cells[rowOffset, columnOffset++].Value = row.Market;
                ws.Cells[rowOffset, columnOffset++].Value = row.Station;
                ws.Cells[rowOffset, columnOffset++].Value = row.Affiliate;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotLength;
                ws.Cells[rowOffset, columnOffset++].Value = row.OrderedSpots;
                ws.Cells[rowOffset, columnOffset++].Value = row.DeliveredSpots;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotClearance;
                ws.Cells[rowOffset, columnOffset + _ScheduleReportDto.StationSummaryData.ScheduleAudiences.Count() * 2].Value = row.SpecStatus;
                columnOffset++;
                outOfSpecCol = columnOffset + _ScheduleReportDto.StationSummaryData.ScheduleAudiences.Count() * 2;
                ws.Cells[rowOffset, outOfSpecCol].Value = row.OutOfSpecSpots ?? 0;
                rowOffset++;
                columnOffset = 1;
            }

            // add audience and delivery
            _SetAudienceData(ws, 1, 9, reportData);


            // impressions/delivery totals 
            var orderedSpots = stationSummary.OrderedSpots();
            _BuildCommonTotalsRow(ws, "F", "G", "H", orderedSpots, stationSummary.DeliveredSpots(true), reportData.Count);
            ws.Cells[reportData.Count + 2, outOfSpecCol].Value = reportData.Sum(d => d.OutOfSpecSpots);

            // sumarry rows
            columnOffset = 9;
            foreach (var impressionsAndDelivery in _ScheduleReportDto.StationSummaryData.ImpressionsAndDelivery)
            {
                ws.Cells[reportData.Count + 2, columnOffset++].Value = _FormatImpressionsValue(impressionsAndDelivery.OrderedImpressions);
                ws.Cells[reportData.Count + 2, columnOffset++].Value = _FormatImpressionsValue(impressionsAndDelivery.TotalDeliveredImpressions);
            }

            for (int i = 1; i <= columnOffset + 1; i++)
            {
                ws.Cells[reportData.Count + 2, i].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            }

            ws.Cells.AutoFitColumns();
        }

        private void _GenerateOutofSpecToDate(ExcelPackage package, bool includeDateColumns)
        {
            var sheetName = "Out of Spec To Date";
            if (package.Workbook.Worksheets[sheetName] != null)
                package.Workbook.Worksheets.Delete(sheetName);

            var ws = package.Workbook.Worksheets.Add(sheetName);

            var outOfSpecData = _ScheduleReportDto.OutOfSpecToDate;
            var reportData = outOfSpecData.GetOutOfSpec();

            ws.Row(1).Height = 25.5;
            ws.View.ShowGridLines = true;
            ws.Cells.Style.Font.Size = 9;
            ws.Cells.Style.Font.Name = "Calibri";

            var columnOffset = 1;

            ws.Cells[1, columnOffset++].Value = "Rank";
            ws.Cells[1, columnOffset++].Value = "Market";
            ws.Cells[1, columnOffset++].Value = "Station";
            ws.Cells[1, columnOffset++].Value = "Affiliate";
            ws.Cells[1, columnOffset++].Value = "Program Name";
            ws.Cells[1, columnOffset++].Value = "Daypart";
            ws.Cells[1, columnOffset++].Value = "Spot Length";
            ws.Cells[1, columnOffset++].Value = "Isci";
            if (includeDateColumns)
            {
                ws.Cells[1, columnOffset++].Value = "BVS Date";
                ws.Cells[1, columnOffset++].Value = "Broadcast Date";
                ws.Cells[1, columnOffset++].Value = "Time Aired";
            }
            ws.Cells[1, columnOffset++].Value = "Ordered Spots";
            ws.Cells[1, columnOffset++].Value = "Delivered Spots";
            ws.Cells[1, columnOffset++].Value = "Spot Clearance";

            //
            _SetOrderedAndDeliveredImpressionsColumn(1, ref columnOffset, ws);

            ws.Cells[1, columnOffset++].Value = "Out of spec Reason";
            ws.Cells[1, columnOffset].Value = "Out of Spec Spots";

            for (int i = 1; i <= columnOffset; i++)
            {
                ws.Cells[1, i].Style.Font.Bold = true;
                ws.Cells[1, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            var rowOffset = 2;
            columnOffset = 1;

            foreach (var row in reportData)
            {
                ws.Cells[rowOffset, columnOffset++].Value = row.Rank;
                ws.Cells[rowOffset, columnOffset++].Value = row.Market;
                ws.Cells[rowOffset, columnOffset++].Value = row.Station;
                ws.Cells[rowOffset, columnOffset++].Value = row.Affiliate;
                ws.Cells[rowOffset, columnOffset++].Value = row.ProgramName;
                ws.Cells[rowOffset, columnOffset++].Value = string.Empty;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotLength;
                ws.Cells[rowOffset, columnOffset++].Value = row.Isci;
                if (includeDateColumns)
                {
                    ws.Cells[rowOffset, columnOffset++].Value = row.BvsDate.ToString(DateFormat);
                    ws.Cells[rowOffset, columnOffset++].Value = row.BroadcastDate.ToString(DateFormat);
                    ws.Cells[rowOffset, columnOffset++].Value = row.TimeAired.ToShortTimeString();
                }
                ws.Cells[rowOffset, columnOffset++].Value = row.OrderedSpots;
                ws.Cells[rowOffset, columnOffset++].Value = row.DeliveredSpots;
                ws.Cells[rowOffset, columnOffset++].Value = row.SpotClearance;
                ws.Cells[rowOffset, columnOffset + _ScheduleReportDto.StationSummaryData.ScheduleAudiences.Count() * 2].Value = row.SpecStatus;
                ws.Cells[rowOffset, columnOffset + _ScheduleReportDto.StationSummaryData.ScheduleAudiences.Count() * 2].Style.Font.Color.SetColor(Color.Red);
                columnOffset++;
                ws.Cells[rowOffset, columnOffset + _ScheduleReportDto.StationSummaryData.ScheduleAudiences.Count() * 2].Value = row.OutOfSpecSpots;

                rowOffset++;
                columnOffset = 1;
            }
            // add audience and delivery
            _SetAudienceData(ws, 1, 12, reportData);

            // impressions/delivery totals 
            _BuildOutOfSpecToDateTotalRows(ws, includeDateColumns, outOfSpecData.DeliveredSpots(false), reportData.Count);

            // sumarry rows
            columnOffset = 12;

            if (includeDateColumns)
                columnOffset = 14;

            var summaryIndexRowCell = reportData.Count + 4;

            foreach (var impressionsAndDelivery in outOfSpecData.ImpressionsAndDelivery)
            {
                _BuildAudienceAndDeliveryTotalsRow(ws, impressionsAndDelivery, reportData.Count, false, ref columnOffset);

                var summaryImpressions = _ScheduleReportDto.StationSummaryData.ImpressionsAndDelivery
                    .Single(i => i.AudienceId == impressionsAndDelivery.AudienceId);
                _SetSummaryData(ws, "To Date Out of Spec", impressionsAndDelivery.AudienceName, summaryImpressions.TotalDeliveredImpressions, impressionsAndDelivery.OutOfSpecDeliveredImpressions, ref summaryIndexRowCell);
            }

            for (int i = 1; i <= columnOffset + 2; i++)
            {
                ws.Cells[reportData.Count + 2, i].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            }

            ws.Cells.AutoFitColumns();
        }

        /// <summary>
        /// Set name and excel number format for ordered/delievered impressions columns
        /// </summary>
        private void _SetOrderedAndDeliveredImpressionsColumn(int row, ref int columnOffset, ExcelWorksheet ws)
        {
            foreach (var audience in _ScheduleReportDto.StationSummaryData.ScheduleAudiences)
            {
                var columIndex = columnOffset++;
                ws.Cells[row, columIndex].Value = "Ordered Impressions (" + audience.AudienceName + ")";
                ws.Column(columIndex).Style.Numberformat.Format = "#,0";

                columIndex = columnOffset++;
                ws.Cells[row, columIndex].Value = "Delivered Impressions (" + audience.AudienceName + ")";
                ws.Column(columIndex).Style.Numberformat.Format = "#,0";
            }
        }

        private void _BuildOutOfSpecToDateTotalRows(ExcelWorksheet ws, bool includeDateColumns, int? deliveredSpots, int count)
        {
            var orderedSpotsColumn = "I";
            var deliveredSpotsColumn = "J";
            var spotClearanceColumn = "K";

            if (includeDateColumns)
            {
                orderedSpotsColumn = "L";
                deliveredSpotsColumn = "M";
                spotClearanceColumn = "N";
            }

            _BuildCommonTotalsRow(ws, orderedSpotsColumn, deliveredSpotsColumn, spotClearanceColumn, 0, deliveredSpots, count);
        }

        private void _GenerateDeliveryBySourceSheet(ExcelPackage package)
        {
            var tabName = "Inventory Sources";

            var deliveryBySource = _ScheduleReportDto.SpotsAndImpressionsBySource;
            if (package.Workbook.Worksheets[tabName] != null)
                package.Workbook.Worksheets.Delete(tabName);
            var ws = package.Workbook.Worksheets.Add(tabName);
            ws.Row(1).Height = 25.5;
            ws.View.ShowGridLines = true;
            ws.Cells.Style.Font.Size = 9;
            ws.Cells.Style.Font.Name = "Calibri";

            var columnOffset = 1;

            ws.Cells[1, columnOffset++].Value = "Inventory Source";
            ws.Cells[1, columnOffset++].Value = "Spots";

            foreach (var audience in _ScheduleReportDto.StationSummaryData.ScheduleAudiences)
            {
                var columIndex = columnOffset++;
                ws.Cells[1, columIndex].Value = "Delivered Impressions (" + audience.AudienceName + ")";
                ws.Column(columIndex).Style.Numberformat.Format = "#,0";
            }

            for (int i = 1; i <= columnOffset; i++)
            {
                ws.Cells[1, i].Style.Font.Bold = true;
                ws.Cells[1, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            var rowOffset = 2;
            foreach (var row in deliveryBySource)
            {
                columnOffset = 1;
                ws.Cells[rowOffset, columnOffset++].Value = row.Source.ToString();
                ws.Cells[rowOffset, columnOffset++].Value = row.Spots;
                foreach (var audience in _ScheduleReportDto.StationSummaryData.ScheduleAudiences)
                {
                    ws.Cells[rowOffset, columnOffset++].Value = _FormatImpressionsValue(row.AudienceImpressions[audience.AudienceId]);
                }
                rowOffset++;
            }

            ws.Cells.AutoFitColumns();

        }

        private void _GenerateSpotDetailDataSheet(ExcelPackage package, bool useNsiDelivery, bool includeBrand, bool includeInventorySource)
        {
            var tabName = "Spot Detail Pre-Post Report";
            var reportData = _ScheduleReportDto.SpotDetailData.ReportData;
            if (package.Workbook.Worksheets[tabName] != null)
                package.Workbook.Worksheets.Delete(tabName);

            var ws = package.Workbook.Worksheets.Add(tabName);
            ws.Row(1).Height = 25.5;
            ws.View.ShowGridLines = true;
            ws.Cells.Style.Font.Size = 9;
            ws.Cells.Style.Font.Name = "Calibri";

            var excelPicture = ws.Drawings.AddPicture("logo", _Logo);
            excelPicture.SetPosition(0, 0, 0, 0);
            excelPicture.SetSize(50);

            var columnOffset = 1;
            var rowOffset = 5;

            ws.Cells[rowOffset, columnOffset++].Value = "Rank";
            ws.Cells[rowOffset, columnOffset++].Value = "Market";
            ws.Cells[rowOffset, columnOffset++].Value = "Station";
            ws.Cells[rowOffset, columnOffset++].Value = "Affiliate";
            ws.Cells[rowOffset, columnOffset++].Value = "Program Name";
            ws.Cells[rowOffset, columnOffset++].Value = "Airtime";
            ws.Cells[rowOffset, columnOffset++].Value = "Date";
            ws.Cells[rowOffset, columnOffset++].Value = "Length";
            ws.Cells[rowOffset, columnOffset++].Value = "ISCI";
            ws.Cells[rowOffset, columnOffset++].Value = "Advertiser";
            ws.Cells[rowOffset, columnOffset++].Value = "Campaign";
            if (includeBrand)
            {
                ws.Cells[rowOffset, columnOffset++].Value = "Brand";
            }
            if (includeInventorySource)
            {
                ws.Cells[rowOffset, columnOffset++].Value = "Inventory Source";
            }

            foreach (var impressionsAndDelivery in _ScheduleReportDto.SpotDetailData.ImpressionsAndDelivery)
            {
                var columIndex = columnOffset++;
                ws.Cells[rowOffset, columIndex].Value = impressionsAndDelivery.AudienceName + " Impressions";
                ws.Column(columIndex).Style.Numberformat.Format = "#,0";
            }
            for (int i = 1; i <= columnOffset; i++)
            {
                ws.Cells[rowOffset, i].Style.Font.Bold = true;
                ws.Cells[rowOffset, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            columnOffset = 1;
            rowOffset = 6;

            foreach (var row in reportData)
            {
                ws.Cells[rowOffset, columnOffset++].Value = row.Rank;
                ws.Cells[rowOffset, columnOffset++].Value = row.Market;
                ws.Cells[rowOffset, columnOffset++].Value = row.Station;
                ws.Cells[rowOffset, columnOffset++].Value = row.Affiliate;
                ws.Cells[rowOffset, columnOffset++].Value = row.ProgramName;
                ws.Cells[rowOffset, columnOffset++].Value = row.TimeAired;
                ws.Cells[rowOffset, columnOffset++].Value = row.DateAired;
                ws.Cells[rowOffset, columnOffset++].Value = row.Length;
                ws.Cells[rowOffset, columnOffset++].Value = row.Isci;
                ws.Cells[rowOffset, columnOffset++].Value = row.Advertiser;
                ws.Cells[rowOffset, columnOffset++].Value = row.Campaign;
                if (includeBrand)
                {
                    ws.Cells[rowOffset, columnOffset++].Value = row.Brand;
                }
                if (includeInventorySource)
                {
                    ws.Cells[rowOffset, columnOffset++].Value = row.InventorySource.ToString();
                }

                foreach (var audience in _ScheduleReportDto.SpotDetailData.ImpressionsAndDelivery)
                {
                    if (useNsiDelivery) //use nsi post-type
                    {
                        ws.Cells[rowOffset, columnOffset++].Value = _FormatImpressionsValue(row.GetNsiDeliveredImpressions(audience.AudienceId));
                    }
                    else //use client-selected post-type
                    {
                        ws.Cells[rowOffset, columnOffset++].Value = _FormatImpressionsValue(row.GetDeliveredImpressions(audience.AudienceId));
                    }

                }

                rowOffset++;
                columnOffset = 1;
            }

            ws.Cells.AutoFitColumns();
        }

        private void _BuildAudienceAndDeliveryTotalsRow(ExcelWorksheet ws, ImpressionAndDeliveryDto impressionAndDeliveryDto, int count, bool isInSpec, ref int columnOffset)
        {
            if (isInSpec)
            {
                ws.Cells[count + 2, columnOffset++].Value = _FormatImpressionsValue(impressionAndDeliveryDto.OrderedImpressions);
                ws.Cells[count + 2, columnOffset++].Value = _FormatImpressionsValue(impressionAndDeliveryDto.DeliveredImpressions);
            }
            else
            {
                ++columnOffset;
                ws.Cells[count + 2, columnOffset++].Value = _FormatImpressionsValue(impressionAndDeliveryDto.OutOfSpecDeliveredImpressions);
                
            }
        }
        private void _BuildCommonHeader(ExcelWorksheet ws, int rowOffset, ref int columnOffset, int reportDataRowCount, bool isOutOfSpec, bool includeDateColumns)
        {
            // header
            ws.Cells[rowOffset, columnOffset++].Value = "Rank";
            ws.Cells[rowOffset, columnOffset++].Value = "Market";
            ws.Cells[rowOffset, columnOffset++].Value = "Station";
            ws.Cells[rowOffset, columnOffset++].Value = "Affiliate";
            ws.Cells[rowOffset, columnOffset++].Value = "Spot Length";
            ws.Cells[rowOffset, columnOffset++].Value = "Program";
            ws.Cells[rowOffset, columnOffset++].Value = "Daypart";
            if (isOutOfSpec)
            {
                ws.Cells[rowOffset, columnOffset++].Value = "Isci";
            }
            if (isOutOfSpec && includeDateColumns)
            {
                ws.Cells[rowOffset, columnOffset++].Value = "BVS Date";
                ws.Cells[rowOffset, columnOffset++].Value = "Broadcast Date";
                ws.Cells[rowOffset, columnOffset++].Value = "Time Aired";
            }
            ws.Cells[rowOffset, columnOffset++].Value = "Cost";
            ws.Cells[rowOffset, columnOffset++].Value = "Spot Cost";
            ws.Cells[rowOffset, columnOffset++].Value = "Ordered Spots";
            ws.Cells[rowOffset, columnOffset++].Value = "Delivered Spots";
            ws.Cells[rowOffset, columnOffset++].Value = "Spot Clearance";

            if (reportDataRowCount != 0)
            {
                // Cost
                ws.Cells["H2:H" + (reportDataRowCount + 1)].Style.Numberformat.Format = "$0.00";
                // spot cost
                ws.Cells["I2:I" + (reportDataRowCount + 1)].Style.Numberformat.Format = "$0.00";
                // Spot Clearance
                ws.Cells["L2:L" + (reportDataRowCount + 1)].Style.Numberformat.Format = "0.00%";
            }

            _SetOrderedAndDeliveredImpressionsColumn(rowOffset, ref columnOffset, ws);

            for (int i = 1; i <= columnOffset; i++)
            {
                ws.Cells[rowOffset, i].Style.Font.Bold = true;
                ws.Cells[rowOffset, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
        }
        private void _BuildCommonTotalsRow(ExcelWorksheet ws, string orderedSpotsColumn, string deliveredSpotsColumn, string spotClearanceColumn, int? totalOrderedSpots, int? totalDeliveredSpots, int reportDataRowCount)
        {
            // totals row
            ws.Cells[reportDataRowCount + 2, 1].Value = "Totals";
            ws.Cells[reportDataRowCount + 2, 1].Style.Font.Bold = true;

            ws.Cells[string.Format("{0}{1}", orderedSpotsColumn, reportDataRowCount + 2)].Value = totalOrderedSpots;
            ws.Cells[string.Format("{0}{1}", deliveredSpotsColumn, reportDataRowCount + 2)].Value = totalDeliveredSpots;
            if (totalOrderedSpots.HasValue)
                ws.Cells[string.Format("{0}{1}", spotClearanceColumn, reportDataRowCount + 2)].Value = (double)totalDeliveredSpots / (double)totalOrderedSpots;
            ws.Cells[string.Format("{0}{1}", spotClearanceColumn, reportDataRowCount + 2)].Style.Numberformat.Format = "0.00%";
        }
        private void _SetAudienceData(ExcelWorksheet ws, int rowOffset, int columnOffset, IEnumerable<BvsReportData> reportData)
        {
            var originalOffset = columnOffset;
            foreach (var reportRow in reportData)
            {
                rowOffset++;
                foreach (var audience in _ScheduleReportDto.AdvertiserData.ScheduleAudiences)
                {
                    ws.Cells[rowOffset, columnOffset++].Value = _FormatImpressionsValue(reportRow.GetOrderedImpressions(audience.AudienceId));
                    ws.Cells[rowOffset, columnOffset++].Value = _FormatImpressionsValue(reportRow.GetDeliveredImpressions(audience.AudienceId));
                }
                columnOffset = originalOffset;
            }
        }
        private void _BuildSummaryRow(ExcelWorksheet ws, ImpressionAndDeliveryDto impressionAndDeliveryDto, string audienceName, string summaryName, ref int summaryIndexRowCell)
        {
            _SetSummaryData(ws, summaryName, audienceName, impressionAndDeliveryDto.OrderedImpressions, impressionAndDeliveryDto.DeliveredImpressions, ref summaryIndexRowCell);
        }
        private void _BuildOutOfSpecSummaryRow(ExcelWorksheet ws, ImpressionAndDeliveryDto impressionAndDeliveryDto, string audienceName, string summaryName, int summaryIndexRowCell)
        {
            _SetSummaryData(ws, summaryName, audienceName, impressionAndDeliveryDto.TotalDeliveredImpressions, impressionAndDeliveryDto.OutOfSpecDeliveredImpressions, ref summaryIndexRowCell);
        }
        private void _SetSummaryData(ExcelWorksheet ws, string summaryName, string audienceName, double totalOrderedImpressions, double totalDeliveredImpressions, ref int summaryIndexRowCell)
        {
            ws.Cells[summaryIndexRowCell, 11].Value = string.Format("{0} ({1})", summaryName, audienceName);
            ws.Cells[summaryIndexRowCell, 12].Value = totalOrderedImpressions == 0 ? 0 : totalDeliveredImpressions / totalOrderedImpressions;
            ws.Cells[summaryIndexRowCell, 12].Style.Numberformat.Format = "0.00%";

            ws.Cells[summaryIndexRowCell, 11].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            ws.Cells[summaryIndexRowCell, 11, summaryIndexRowCell, 12].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            ws.Cells[summaryIndexRowCell, 11, summaryIndexRowCell, 12].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            ws.Cells[summaryIndexRowCell++, 12].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        // impressions value need to be divided by 1000 and later formated to have "," for thousands 
        internal double _FormatImpressionsValue(double imp)
        {
            return imp/1000;
        }

    }
}
