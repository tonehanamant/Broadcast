using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Helpers;
using System.IO;
using System.Linq;


namespace Services.Broadcast.ReportGenerators.SpotExceptions
{
   public class OutOfSpecReportGenerator 
    {
        private readonly string TEMPLATES_FILE_PATH;      
        private readonly string OUTOFSPEC_EXPORT_TEMPLATE_FILENAME = "Template - Out of Spec Report Buying Team.xlsx";
        public OutOfSpecReportGenerator(string templatesPath)
        {
            TEMPLATES_FILE_PATH = templatesPath;           
        }
        public ReportOutput Generate(OutOfSpecExportReportData dataObject)
        {
            var output = new ReportOutput(filename: dataObject.ExportFileName);

            ExcelPackage package = _GetFileWithData();
            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }
        private ExcelPackage _GetFileWithData()
        {
            var templateFilePath = Path.Combine(TEMPLATES_FILE_PATH, OUTOFSPEC_EXPORT_TEMPLATE_FILENAME);            
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);            
            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }
    }
}
