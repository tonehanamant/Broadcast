using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.IO;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ReportGenerators
{
    public class CampaignReportGenerator : IReportGenerator<CampaignReportData>
    {
        private readonly string PROPOSAL_WORKSHEET_NAME = "Proposal";
        private readonly string CONTRACT_WORKSHEET_NAME = "Contract";
        private readonly string TERMS_WORKSHEET_NAME = "Terms & Conditions";
        private readonly string FLOW_CHART_WORKSHEET_NAME = "Flow Chart";
        
        private readonly string TEMPLATE_FILENAME = "Template - Campaign Export.xlsx";
        
        /// <summary>
        /// Generates a report of type CampaignReport
        /// </summary>
        /// <param name="dataObject">Data object used to generate the file</param>
        /// <returns>
        /// ReportOutput object containing the generated file stream
        /// </returns>
        public ReportOutput Generate(CampaignReportData dataObject)
        {
            var output = new ReportOutput(filename: dataObject.CampaignExportFileName);

            ExcelPackage package = _GetFileWithData(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage _GetFileWithData(CampaignReportData campaignReportData)
        {
            string templateFilePath = $@"{BroadcastServiceSystemParameter.BroadcastExcelTemplatesPath}\{TEMPLATE_FILENAME}";
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);
            ExcelWorksheet proposalWorksheet = ExportSharedLogic.GetWorksheet(templateFilePath, package, PROPOSAL_WORKSHEET_NAME);
            new ProposalReportGenerator().PopulateProposalTab(campaignReportData, proposalWorksheet);

            ExcelWorksheet flowChartWorksheet = ExportSharedLogic.GetWorksheet(templateFilePath, package, FLOW_CHART_WORKSHEET_NAME);
            new FlowChartReportGenerator().PopulateFlowChartTab(campaignReportData, flowChartWorksheet);
            if (campaignReportData.Status.Equals("Proposal"))
            {
                package.Workbook.Worksheets.Delete(CONTRACT_WORKSHEET_NAME);
                package.Workbook.Worksheets.Delete(TERMS_WORKSHEET_NAME);
                package.Workbook.Worksheets.MoveToStart(PROPOSAL_WORKSHEET_NAME);
            }
            else
            {
                ExcelWorksheet contractWorksheet = ExportSharedLogic.GetWorksheet(TEMPLATE_FILENAME, package, CONTRACT_WORKSHEET_NAME);
                // _PopulateContractWorksheet(contractWorksheet, dataObject);
            }

            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }  
    }
}
