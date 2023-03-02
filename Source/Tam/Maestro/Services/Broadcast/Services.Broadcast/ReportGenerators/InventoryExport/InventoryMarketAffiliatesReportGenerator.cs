using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventoryMarketAffiliates;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.InventoryExport
{
    public class InventoryMarketAffiliatesReportGenerator : IReportGenerator<InventoryMarketAffiliatesData>
    {
        private readonly ExcelWorksheet WORKSHEET;
        private readonly string TEMPLATES_FILE_PATH;
        private readonly string INVENTORY_MARKET_AFFILIATES_EXPORT_TEMPLATE_FILENAME = "Template - Affiliate Availability Per Market.xlsx";
        private readonly string NEWS_WORKSHEET_NAME = "News";
        private readonly string NON_NEWS_WORKSHEET_NAME = "Non-News";
        private int currentRowIndex = 0;
        private int firstDataRow = 7;
        private readonly string START_COLUMN = "B";

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="InventoryMarketAffiliatesReportGenerator" /> class.
        /// </summary>
        /// <param name="templatesPath">The templates path.</param>
        public InventoryMarketAffiliatesReportGenerator(string templatesPath)
        {
            TEMPLATES_FILE_PATH = templatesPath;
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="InventoryMarketAffiliatesReportGenerator" /> class.
        /// </summary>
        /// <param name="worksheet">The worksheet.</param>
        public InventoryMarketAffiliatesReportGenerator(ExcelWorksheet worksheet)
        {
            WORKSHEET = worksheet;
        }

        /// <summary>
        /// Generates a report of type T
        /// </summary>
        /// <param name="dataObject">Data object used to generate the file</param>
        /// <returns>ReportOutput object containing the generated file stream</returns>
        public ReportOutput Generate(InventoryMarketAffiliatesData dataObject)
        {
            var output = new ReportOutput(filename: dataObject.MarketAffiliatesExportFileName);

            ExcelPackage package = _GetFileWithData(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        /// <summary>
        /// Gets the file with data.
        /// </summary>
        /// <param name="inventoryMarketAffiliates">The inventory market affiliates report data.</param>
        /// <returns></returns>
        private ExcelPackage _GetFileWithData(InventoryMarketAffiliatesData inventoryMarketAffiliates)
        {
            string templateFilePath = Path.Combine(TEMPLATES_FILE_PATH, INVENTORY_MARKET_AFFILIATES_EXPORT_TEMPLATE_FILENAME);
            var package = new ExcelPackage(new FileInfo(templateFilePath));           
            
            ExcelWorksheet newsWorksheet = ExportSharedLogic.GetWorksheet(templateFilePath, package, NEWS_WORKSHEET_NAME);
            new InventoryMarketAffiliatesReportGenerator(newsWorksheet)._PopulateMarketAffiliateTab(inventoryMarketAffiliates.NewsMarketAffiliates);

            ExcelWorksheet nonNewsWorksheet = ExportSharedLogic.GetWorksheet(templateFilePath, package, NON_NEWS_WORKSHEET_NAME);
            new InventoryMarketAffiliatesReportGenerator(nonNewsWorksheet)._PopulateMarketAffiliateTab(inventoryMarketAffiliates.NonNewsMarketAffiliates);

            package.Workbook.Worksheets.First().Select();

            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }

        /// <summary>
        /// Populates the market affiliate tab.
        /// </summary>
        /// <param name="inventoryMarketAffiliates">The inventory market affiliates.</param>
        private void _PopulateMarketAffiliateTab(List<InventoryMarketAffiliates> inventoryMarketAffiliates)
        {
            currentRowIndex = firstDataRow;
            for (int i = 0; i < inventoryMarketAffiliates.Count(); i++)
            {
                var table = inventoryMarketAffiliates[i];
                WORKSHEET.Cells[firstDataRow, ExportSharedLogic.FIRST_COLUMNS_INDEX, firstDataRow, ExportSharedLogic.END_COLUMN_INDEX]
                    .Copy(WORKSHEET.Cells[currentRowIndex, ExportSharedLogic.FIRST_COLUMNS_INDEX]);

                _SetRowData(currentRowIndex, START_COLUMN, _GetQuarterTableRowObjects(table));
                currentRowIndex++;
            }
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

        /// <summary>
        /// Gets the quarter table row objects.
        /// </summary>
        /// <param name="rowData">The row data.</param>
        /// <returns></returns>
        private List<object> _GetQuarterTableRowObjects(InventoryMarketAffiliates rowData)
        {
            var row = new List<object>() {
                rowData.marketName,
                rowData.marketRank,
                rowData.affiliates,
                rowData.inventory,
                rowData.aggregate,
            };

            return row;
        }
    }
}
