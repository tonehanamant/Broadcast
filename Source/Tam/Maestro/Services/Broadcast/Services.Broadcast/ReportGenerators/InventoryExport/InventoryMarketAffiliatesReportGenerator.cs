using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventoryMarkets;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.InventoryExport
{
    public class InventoryMarketAffiliatesReportGenerator : IReportGenerator<InventoryMarketAffiliatesReportData>
    {
        private readonly string TEMPLATES_FILE_PATH;
        private readonly string INVENTORY_MARKET_AFFILIATES_EXPORT_TEMPLATE_FILENAME = "Template - Affiliate Availability Per Market.xlsx";

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
        /// Generates a report of type T
        /// </summary>
        /// <param name="dataObject">Data object used to generate the file</param>
        /// <returns>ReportOutput object containing the generated file stream</returns>
        public ReportOutput Generate(InventoryMarketAffiliatesReportData dataObject)
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
        /// <param name="inventoryMarketAffiliatesReportData">The inventory market affiliates report data.</param>
        /// <returns></returns>
        private ExcelPackage _GetFileWithData(InventoryMarketAffiliatesReportData inventoryMarketAffiliatesReportData)
        {
            string templateFilePath = Path.Combine(TEMPLATES_FILE_PATH, INVENTORY_MARKET_AFFILIATES_EXPORT_TEMPLATE_FILENAME);
            var package = new ExcelPackage(new FileInfo(templateFilePath));

            package.Workbook.Worksheets.First().Select();

            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }
    }
}
