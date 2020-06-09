using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.InventoryExport
{
    public class InventoryExportGenerator
    {
        private readonly string INVENTORY_WORKSHEET_NAME = "Inventory";

        private readonly string TEMPLATE_FILENAME = "Template - Open Market inventory.xlsx";
        private readonly string TEMPLATES_PATH;

        public InventoryExportGenerator(string templatesPath)
        {
            TEMPLATES_PATH = templatesPath;
        }

        public ReportOutput Generate(InventoryExportReportData dataObject)
        {
            var output = new ReportOutput(filename: dataObject.ExportFileName);

            ExcelPackage package = _GetFileWithData(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage _GetFileWithData(InventoryExportReportData reportData)
        {
            var templateFilePath = Path.Combine(TEMPLATES_PATH, TEMPLATE_FILENAME);
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);

            var inventoryViewTab = ExportSharedLogic.GetWorksheet(templateFilePath, package, INVENTORY_WORKSHEET_NAME);
            var inventoryViewGenerator = new InventoryViewReportGenerator();
            inventoryViewGenerator.PopulateTab(inventoryViewTab, reportData);

            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }
    }
}