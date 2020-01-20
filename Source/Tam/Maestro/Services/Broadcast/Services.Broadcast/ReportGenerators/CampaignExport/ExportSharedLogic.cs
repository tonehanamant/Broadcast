using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public static class ExportSharedLogic
    {
        public static readonly int ROW_HEIGHT = 24;
        public static readonly int END_COLUMN_INDEX = 25;
        public static readonly int FIRST_COLUMNS_INDEX = 1;
        private static readonly string NOT_FOUND_WORKSHEET = "Could not find worksheet {0} in template file {1}";

        /// <summary>
        /// Gets a worksheet by name.
        /// </summary>
        /// <param name="templateFilePath">The template file path.</param>
        /// <param name="package">The package to find the worksheet in.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns>ExcelWorksheet object</returns>
        /// <exception cref="Exception">Throws "Could not find worksheet in template file" if the worksheet was not found</exception>
        public static ExcelWorksheet GetWorksheet(string templateFilePath, ExcelPackage package, string sheetName)
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name.Equals(sheetName));
            if (worksheet == null)
            {
                throw new Exception(string.Format(NOT_FOUND_WORKSHEET, sheetName, Path.GetFileName(templateFilePath)));
            }

            return worksheet;
        }


        /// <summary>
        /// Adds copies of empty tables based on a specific table.
        /// </summary>
        /// <param name="worksheet">The worksheet.</param>
        /// <param name="count">Number of tables to add</param>
        /// <param name="firstRowIndex">Row index of the first row from the source table</param>
        /// <param name="endRowIndex">Row index of the last row from the source table</param>
        /// <param name="rowsToCopy">Number of rows to copy</param>
        public static void AddEmptyTables(ExcelWorksheet worksheet, int count, int firstRowIndex
            , int endRowIndex, int rowsToCopy)
        {
            worksheet.InsertRow(firstRowIndex + rowsToCopy, (count - 1) * (rowsToCopy + 1));

            for (int i = 1; i < count; i++)
            {
                worksheet.Cells[firstRowIndex, ExportSharedLogic.FIRST_COLUMNS_INDEX, endRowIndex, ExportSharedLogic.END_COLUMN_INDEX]
                    .Copy(worksheet.Cells[firstRowIndex + (i * rowsToCopy) + i, ExportSharedLogic.FIRST_COLUMNS_INDEX]);
            }
        }
    }
}
