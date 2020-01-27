using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.IO;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ReportGenerators.ProgramLineup
{
    public class ProgramLineupReportGenerator : IReportGenerator<ProgramLineupReportData>
    {
        private readonly string ALLOCATIONS_WORKSHEET_NAME = "Allocations";
        private readonly string DEFAULT_VIEW_WORKSHEET_NAME = "Default View";
        private readonly string DETAILED_VIEW_WORKSHEET_NAME = "Detailed View";

        private readonly string TEMPLATE_FILENAME = "Template - Program Lineup.xlsx";
        
        public ReportOutput Generate(ProgramLineupReportData dataObject)
        {
            var output = new ReportOutput(filename: dataObject.ExportFileName);

            ExcelPackage package = _GetFileWithData(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage _GetFileWithData(ProgramLineupReportData programLineupReportData)
        {
            string templateFilePath = $@"{BroadcastServiceSystemParameter.BroadcastExcelTemplatesPath}\{TEMPLATE_FILENAME}";
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);

            ExcelWorksheet detailedViewTab = ExportSharedLogic.GetWorksheet(templateFilePath, package, DETAILED_VIEW_WORKSHEET_NAME);
            new DetailedViewReportGenerator().PopulateTab(detailedViewTab, programLineupReportData);
            
            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }
    }
}
