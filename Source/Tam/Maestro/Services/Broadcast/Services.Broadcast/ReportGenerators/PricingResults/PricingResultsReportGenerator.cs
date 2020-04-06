using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.PricingResults
{
    public class PricingResultsReportGenerator : IReportGenerator<PricingResultsReportData>
    {
        private readonly string PRICING_SPOT_ALLOCATION_VIEW_WORKSHEET_NAME = "Allocations";

        private readonly string TEMPLATE_FILENAME = "Template - Pricing Results.xlsx";
        private readonly string TEMPLATES_PATH;

        public PricingResultsReportGenerator(string templatesPath)
        {
            TEMPLATES_PATH = templatesPath;
        }

        public ReportOutput Generate(PricingResultsReportData dataObject)
        {
            var output = new ReportOutput(filename: dataObject.ExportFileName);

            ExcelPackage package = _GetFileWithData(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage _GetFileWithData(PricingResultsReportData reportData)
        {
            string templateFilePath = Path.Combine(TEMPLATES_PATH, TEMPLATE_FILENAME);
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);

            ExcelWorksheet allocationsViewTab = ExportSharedLogic.GetWorksheet(templateFilePath, package, PRICING_SPOT_ALLOCATION_VIEW_WORKSHEET_NAME);
            new PricingSpotAllocationViewReportGenerator().PopulateTab(allocationsViewTab, reportData);

            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }
    }
}
