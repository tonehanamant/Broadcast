using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using System.IO;
using System.IO.Compression;

namespace Services.Broadcast.ReportGenerators
{
    public class SpotTrackerReportGenerator : IReportGenerator<SpotTrackerReport>
    {
        public ReportOutput Generate(SpotTrackerReport spotTrackerReport)
        {
            var result = new ReportOutput(spotTrackerReport.ZipFileName)
            {
                Stream = new MemoryStream()
            };

            using (var archiveFile = new ZipArchive(result.Stream, ZipArchiveMode.Create, true))
            {
                foreach (var proposalDetail in spotTrackerReport.Details)
                {
                    var reportFile = _SaveProposalDetailToSpotTrackerReportFile(proposalDetail);
                    _SaveSpotTrackerReportFileToArchive(proposalDetail, archiveFile, reportFile);
                }
            }

            result.Stream.Seek(0, SeekOrigin.Begin);

            return result;
        }

        private OfficeOpenXml.ExcelPackage _SaveProposalDetailToSpotTrackerReportFile(SpotTrackerReport.Detail proposalDetail)
        {
            var reportFile = new OfficeOpenXml.ExcelPackage(new MemoryStream());

            foreach (var week in proposalDetail.Weeks)
            {
                _AddWeekTabToSpotTrackerReportFile(week, reportFile);
            }

            return reportFile;
        }

        private void _AddWeekTabToSpotTrackerReportFile(SpotTrackerReport.Detail.Week week, OfficeOpenXml.ExcelPackage reportFile)
        {
            const string dateFormat = @"MM-dd-yy";
            const int fontSize = 12;
            const string fontName = "Calibri";
            const int marketColumn = 1;
            const int affiliateColumn = 2;
            const int stationColumn = 3;
            const int spotsOrderedColumn = 4;
            const int spotsDeliveredColumn = 5;

            var weekMonday = week.StartDate.GetNextMonday().ToString(dateFormat);
            var excelWorksheet = reportFile.Workbook.Worksheets.Add($"{weekMonday}");

            excelWorksheet.Cells.Style.Font.Size = fontSize;
            excelWorksheet.Cells.Style.Font.Name = fontName;

            var row = 1;

            // Headers
            excelWorksheet.Cells[row, marketColumn].Value = "Market";
            excelWorksheet.Cells[row, affiliateColumn].Value = "Network Affiliate";
            excelWorksheet.Cells[row, stationColumn].Value = "Station";
            excelWorksheet.Cells[row, spotsOrderedColumn].Value = "Spots Booked";
            excelWorksheet.Cells[row, spotsDeliveredColumn].Value = "Spots Delivered";
            row++;

            // Content
            foreach (var stationSpots in week.StationSpotsValues)
            {
                excelWorksheet.Cells[row, marketColumn].Value = stationSpots.Market;
                excelWorksheet.Cells[row, affiliateColumn].Value = stationSpots.Affiliate;
                excelWorksheet.Cells[row, stationColumn].Value = stationSpots.Station;
                excelWorksheet.Cells[row, spotsOrderedColumn].Value = stationSpots.SpotsOrdered;
                excelWorksheet.Cells[row, spotsDeliveredColumn].Value = stationSpots.SpotsDelivered;
                row++;
            }

            excelWorksheet.Cells.AutoFitColumns();
        }

        private void _SaveSpotTrackerReportFileToArchive(
            SpotTrackerReport.Detail proposalDetail,
            ZipArchive archiveFile,
            OfficeOpenXml.ExcelPackage reportFile)
        {
            var archiveEntry = archiveFile.CreateEntry(proposalDetail.FileName, CompressionLevel.Fastest);

            using (var zippedStreamEntry = archiveEntry.Open())
            {
                reportFile.SaveAs(zippedStreamEntry);
            }
        }
    }
}
