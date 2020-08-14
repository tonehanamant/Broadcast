using OfficeOpenXml;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.Linq;
using Tam.Maestro.Common;

namespace Services.Broadcast.ReportGenerators.Quote
{
    public class RateDetailsReportGenerator
    {
        private const int HEADER_DATA_COLUMN = 2;
        private const int HEADER_TIME_GENERATED_ROW = 1;

        private const int TABLE_COLUMN_START = 1;
        private const int TABLE_ROW_START = 7;
        private const int TABLE_DATA_ROW_START = 8;

        private const int TABLE_BASE_COLUMN_COUNT = 12;

        private const int TABLE_DATA_DYNAMIC_DEMOS_COLUMN_START = 7;
        private const int TABLE_COLUMN_HOUSEHOLD_CPM = 6;
        private const int TABLE_TEMPLATE_EXISTING_ROW_COUNT = 2;

        public void PopulateTab(ExcelWorksheet worksheet, QuoteReportData reportData)
        {
            var tableDimensions = _SetupDetailsTable(worksheet, reportData);

            _PopulateHeader(worksheet, reportData);
            _PopulateDetailLines(worksheet, reportData, tableDimensions);
        }

        private void _PopulateHeader(ExcelWorksheet worksheet, QuoteReportData reportData)
        {
            worksheet.Cells[HEADER_TIME_GENERATED_ROW, HEADER_DATA_COLUMN].Value = reportData.GeneratedTimeStamp;
        }

        private void _PopulateDetailLines(ExcelWorksheet worksheet, QuoteReportData reportData, TableDimensions tableDimensions)
        {
            if (reportData.RateDetailsTableData.Length == 0)
            {
                return;
            }

            var tableDataTopLeftCell = (Row: TABLE_DATA_ROW_START, Column: TABLE_COLUMN_START);
            var tableRowCount = reportData.RateDetailsTableData.Length;
            var tableBottomRightCell = (Row: TABLE_DATA_ROW_START + tableRowCount, Column: tableDimensions.ColumnCount);

            worksheet.Cells[
                tableDataTopLeftCell.Row, tableDataTopLeftCell.Column,
                tableBottomRightCell.Row, tableBottomRightCell.Column
            ].LoadFromArrays(reportData.RateDetailsTableData);
        }

        /// <summary>
        /// Adds the columns for the dynamic audiences.
        /// Returns the number of dynamic audience columns.
        /// </summary>
        /// <returns>Dynamic audience column count.</returns>
        private int _AddDynamicDemoColumns(ExcelWorksheet worksheet, QuoteReportData reportData)
        {
            // there will always be at least one audience
            var dynamicAudienceCount = reportData.RateDetailsTableAudienceHeaders.First().Length;
            var newAudienceColumnCount = dynamicAudienceCount - 1; // subtract one for the existing column
            var firstAudienceColumn = TABLE_DATA_DYNAMIC_DEMOS_COLUMN_START;
            var lastAudienceColumn = firstAudienceColumn + newAudienceColumnCount;

            // insert the columns
            worksheet.InsertColumn(firstAudienceColumn, newAudienceColumnCount, TABLE_COLUMN_HOUSEHOLD_CPM);
            // size the new columns
            var audienceWidth = worksheet.Column(TABLE_COLUMN_HOUSEHOLD_CPM).Width;
            Enumerable.Range(firstAudienceColumn, newAudienceColumnCount).ForEach(i => worksheet.Column(i).Width = audienceWidth);

            // populate the headers
            worksheet.Cells[FromRow: TABLE_ROW_START, FromCol: firstAudienceColumn, ToRow: TABLE_ROW_START, ToCol: lastAudienceColumn]
                .LoadFromArrays(reportData.RateDetailsTableAudienceHeaders);
            
            return dynamicAudienceCount;
        }

        private TableDimensions _SetupDetailsTable(ExcelWorksheet worksheet, QuoteReportData reportData)
        {
            var dynamicAudienceColumnCount = _AddDynamicDemoColumns(worksheet, reportData);
            var tableDimensions = new TableDimensions
            {
                RowCount = reportData.RateDetailsTableData.Length, 
                ColumnCount = TABLE_BASE_COLUMN_COUNT + dynamicAudienceColumnCount
            };

            if (tableDimensions.RowCount > TABLE_TEMPLATE_EXISTING_ROW_COUNT)
            {
                // perform an insert to cascade the change into the header formulas
                var insertRowStartIndex = TABLE_DATA_ROW_START + 1;
                var copyStylesFromRowIndex = TABLE_DATA_ROW_START;
                // subtract 2 for the rows before and after the rows we're inserting.
                var insertRowCount = tableDimensions.RowCount - TABLE_TEMPLATE_EXISTING_ROW_COUNT;
                worksheet.InsertRow(insertRowStartIndex, insertRowCount, copyStylesFromRowIndex);
            }

            // perform the extend to copy the table row column styles and formulas through the added rows
            var copyRowStartIndex = TABLE_DATA_ROW_START;
            var copyLastColumnIndex = TABLE_COLUMN_START + tableDimensions.ColumnCount - 1;
            var copyFromSourceCell = (Row: copyRowStartIndex, Column: TABLE_COLUMN_START);
            var copyToSourceCell = (Row: copyRowStartIndex, Column: copyLastColumnIndex);
            // subtract 1 for the row we are copying
            var copyRowCount = tableDimensions.RowCount - 1; 
            // this will copy the table column styles and formulas
            ExportSharedLogic.ExtendTable(worksheet, copyFromSourceCell, copyToSourceCell, copyRowCount);

            return tableDimensions;
        }

        private class TableDimensions
        {
            public int RowCount { get; set; }
            public int ColumnCount { get; set; }
        }
    }
}
