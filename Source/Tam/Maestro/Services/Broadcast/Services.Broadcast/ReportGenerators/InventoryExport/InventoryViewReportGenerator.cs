using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.InventoryExport
{
    public class InventoryViewReportGenerator
    {
        private const int INVENTORY_TABLE_START_ROW = 5;
        private const int INVENTORY_TABLE_START_COLUMN = 1;
        private const int INVENTORY_TABLE_LAST_STATIC_COLUMN = 13;
        private const int HEADER_DATA_COLUMN = 1;

        internal void PopulateTab(ExcelWorksheet worksheet, InventoryExportReportData reportData)
        {
            _PopulateTopFields(worksheet, reportData);
            _PopulateInventoryTable(worksheet, reportData);
        }

        private void _PopulateTopFields(ExcelWorksheet worksheet, InventoryExportReportData reportData)
        {
            worksheet.Cells[1, HEADER_DATA_COLUMN].Value = reportData.GeneratedTimestampValue;
            worksheet.Cells[3, HEADER_DATA_COLUMN].Value = reportData.ShareBookValue;
        }

        private void _PopulateInventoryTable(ExcelWorksheet worksheet, InventoryExportReportData reportData)
        {
            var tableDataStartRow = INVENTORY_TABLE_START_ROW + 1;

            var providedAudienceColumnCount = reportData.ProvidedAudienceHeaders.FirstOrDefault()?.Length ?? 0;
            var weeklyColumnCount = reportData.WeeklyColumnHeaders.First().Length;

            var audiencesStartColumn = INVENTORY_TABLE_LAST_STATIC_COLUMN + 1;
            var weeksStartColumn = audiencesStartColumn + providedAudienceColumnCount;
            var lastColumnIndex = weeksStartColumn + weeklyColumnCount;

            _FormatRows(worksheet, audiencesStartColumn, providedAudienceColumnCount,
                weeksStartColumn, weeklyColumnCount);

            var tableDataTopLeftCell = (Row: tableDataStartRow, Column: INVENTORY_TABLE_START_COLUMN);
            var tableDataTopRightCell = (Row: tableDataTopLeftCell.Row, Column: lastColumnIndex);
            ExportSharedLogic.ExtendTable(
                worksheet,
                tableDataTopLeftCell,
                tableDataTopRightCell,
                reportData.InventoryTableData.Length);

            worksheet.Cells[INVENTORY_TABLE_START_ROW, audiencesStartColumn].LoadFromArrays(reportData.ProvidedAudienceHeaders);
            worksheet.Cells[INVENTORY_TABLE_START_ROW, weeksStartColumn].LoadFromArrays(reportData.WeeklyColumnHeaders);
            worksheet.Cells[tableDataStartRow, INVENTORY_TABLE_START_COLUMN].LoadFromArrays(reportData.InventoryTableData);
        }

        private void _FormatRows(ExcelWorksheet worksheet, int audienceStart, int audienceCount, int weekStart, int weekCount)
        {
            var tableHeaderRow = INVENTORY_TABLE_START_ROW;
            var tableDataStartRow = tableHeaderRow + 1;

            var weekColumns = Enumerable.Range(weekStart, weekCount).ToList();
            var audienceColumns = Enumerable.Range(audienceStart, audienceCount).ToList();
            
            var centeredColumns = weekColumns.Concat(audienceColumns).ToList();
            var boldColumns = weekColumns;

            // $123
            var rateColumnIndexes = new List<int>
            {
                11 // '30 Rate'
            };

            // 1,234
            var impressionColumnIndexes = new List<int>
            {
                12 // 'HH Imp(000)'
            };

            // $123.45
            var costColumnIndex = new List<int>
            {
                13 // '30 CPM'
            };

            rateColumnIndexes.AddRange(weekColumns);
            impressionColumnIndexes.AddRange(audienceColumns);

            const int audienceColumnWidth = 15;
            const int weekColumnWidth = 10;

            weekColumns.ForEach(i => worksheet.Column(i).Width = weekColumnWidth);
            audienceColumns.ForEach(i => worksheet.Column(i).Width = audienceColumnWidth);

            centeredColumns.ForEach(i =>
            {
                worksheet.Column(i).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                worksheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            });
            boldColumns.ForEach(i => worksheet.Cells[tableHeaderRow, i].Style.Font.Bold = true);
            
            rateColumnIndexes.ForEach(i=> worksheet.Cells[tableDataStartRow, i].Style.Numberformat.Format = "$###,###,##0");
            impressionColumnIndexes.ForEach(i=> worksheet.Cells[tableDataStartRow, i].Style.Numberformat.Format = "###,###,##0");
            costColumnIndex.ForEach(i => worksheet.Cells[tableDataStartRow, i].Style.Numberformat.Format = "$###,###,##0.00");
        }
    }
}