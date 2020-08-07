using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.Quote
{
    public class QuoteReportGenerator : IReportGenerator<QuoteReportData>
    {
        private readonly string RATE_DETAILS_WORKSHEET_NAME = "Rate Details";

        private readonly string TEMPLATE_FILENAME = "Template - PlanQuote.xlsx";
        private readonly string TEMPLATES_PATH;

        public QuoteReportGenerator(string templatesPath)
        {
            TEMPLATES_PATH = templatesPath;
        }

        public ReportOutput Generate(QuoteReportData dataObject)
        {
            var output = new ReportOutput(filename: dataObject.ExportFileName);

            ExcelPackage package = _GetFileWithData(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage _GetFileWithData(QuoteReportData reportData)
        {
            string templateFilePath = Path.Combine(TEMPLATES_PATH, TEMPLATE_FILENAME);
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);

            ExcelWorksheet rateDetailsTab = ExportSharedLogic.GetWorksheet(templateFilePath, package, RATE_DETAILS_WORKSHEET_NAME);
            new RateDetailsReportGenerator().PopulateTab(rateDetailsTab, reportData);

            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }
    }
}
