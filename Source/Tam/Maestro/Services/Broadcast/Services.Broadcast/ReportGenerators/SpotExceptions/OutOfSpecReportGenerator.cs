using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Services.Broadcast.ReportGenerators.SpotExceptions
{
    public class OutOfSpecReportGenerator
    {
        private readonly ExcelWorksheet WORKSHEET;      
        private readonly int firstDataRow = 2;
        private readonly string START_COLUMN = "A";
        private readonly string BASE_SHEET = "BaseSheet";
        private readonly string TEMPLATES_FILE_PATH;      
        private readonly string OUTOFSPEC_EXPORT_TEMPLATE_FILENAME = "Template - Out of Spec Report Buying Team.xlsx";
        public OutOfSpecReportGenerator(string templatesPath)
        {
            TEMPLATES_FILE_PATH = templatesPath;           
        }
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="OutOfSpecReportGenerator" /> class.
        /// </summary>
        /// <param name="worksheet">The worksheet.</param>
        public OutOfSpecReportGenerator(ExcelWorksheet worksheet)
        {
            WORKSHEET = worksheet;
        }

        public ReportOutput Generate(OutOfSpecExportReportData dataObject)
        {
            var output = new ReportOutput(filename: dataObject.OutOfSpecExportFileName);

            ExcelPackage package = _GetFileWithData(dataObject.OutOfSpecs);
            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }
        private ExcelPackage _GetFileWithData(List<OutOfSpecExportReportDto> outOfSpecExportReportData)
        {
            var templateFilePath = Path.Combine(TEMPLATES_FILE_PATH, OUTOFSPEC_EXPORT_TEMPLATE_FILENAME);            
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);
            if (outOfSpecExportReportData.Any())
            {
                var outOfSpecGroupedByAdvertiser = outOfSpecExportReportData.GroupBy(x => x.AdvertiserName).OrderByDescending(c => c.Key)
                                .Select(std => new
                                {
                                    Key = std.Key,
                                    OutPfSpec = std.OrderBy(x => x.Date).ThenBy(x => x.TimeAired)
                                });
                foreach (var outOfSpecItem in outOfSpecGroupedByAdvertiser)
                {
                    package.Workbook.Worksheets.Copy(BASE_SHEET, outOfSpecItem.Key);
                    ExcelWorksheet newsWorksheet = ExportSharedLogic.GetWorksheet(templateFilePath, package, outOfSpecItem.Key);
                    new OutOfSpecReportGenerator(newsWorksheet)._PopulateOutOfSpecReportTabS(outOfSpecItem.OutPfSpec.ToList());
                }
                package.Workbook.Worksheets.Delete(BASE_SHEET);
            }
            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }
        /// <summary>
        /// Populates the out of spec report tabs.
        /// </summary>
        /// <param name="outOfSpecExportReportData">The out of spec report data.</param>
        private void _PopulateOutOfSpecReportTabS(List<OutOfSpecExportReportDto> outOfSpecExportReportData)
        {
            int currentRowIndex = firstDataRow;            
            for (int i = 0; i < outOfSpecExportReportData.Count; i++)
            {
                var table = outOfSpecExportReportData[i];
                WORKSHEET.Cells[firstDataRow, ExportSharedLogic.FIRST_COLUMNS_INDEX, firstDataRow, ExportSharedLogic.END_COLUMN_INDEX]
                    .Copy(WORKSHEET.Cells[currentRowIndex, ExportSharedLogic.FIRST_COLUMNS_INDEX]);

                _SetRowData(currentRowIndex, START_COLUMN, _GetTableRowObjects(table));
                currentRowIndex++;
            }
        }

        /// <summary>
        /// Gets the quarter table row objects.
        /// </summary>
        /// <param name="rowData">The row data.</param>
        /// <returns></returns>
        private List<object> _GetTableRowObjects(OutOfSpecExportReportDto rowData)
        {
            var row = new List<object>() {
                rowData.MarketRank,
                rowData.Market,
                rowData.Station,
                rowData.Affiliate,
                rowData.WeekStartDate,
                rowData.Day,
                rowData.Date,
                rowData.TimeAired,
                rowData.ProgramName,
                rowData.Length,
                rowData.HouseIsci,
                rowData.ClientIsci,
                rowData.AdvertiserName,
                rowData.InventorySource,
                rowData.InventorySourceDaypart,               
                rowData.InventoryOutOfSpecReason,
                rowData.Estimates,
                rowData.Comment               
            };

            return row;
        }
        /// <summary>
        /// Sets the row data.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="totalRow">The total row.</param>
        private void _SetRowData(int rowIndex, string columnIndex, List<object> totalRow)
        {
            WORKSHEET.Row(rowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            WORKSHEET.Cells[$"{columnIndex}{rowIndex}"]
                .LoadFromArrays(new List<object[]> { totalRow.ToArray() });
        }
    }
}
