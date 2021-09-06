using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Helpers;
using System.IO;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class CampaignReportGenerator : IReportGenerator<CampaignReportData>
    {
        private readonly string PROPOSAL_WORKSHEET_NAME = "Proposal";
        private readonly string CONTRACT_WORKSHEET_NAME = "Contract";
        private readonly string TERMS_WORKSHEET_NAME = "Terms & Conditions";
        private readonly string FLOW_CHART_WORKSHEET_NAME = "Flow Chart";
        private readonly string FLOW_CHART_TEMPLATE_TABLES_WORKSHEET_NAME = "Flow Chart Template Tables";

        private readonly string CAMPAIGN_EXPORT_TEMPLATE_FILENAME = "Template - Campaign Export.xlsx";
        private readonly string CAMPAIGN_EXPORT_WITH_SECONDARY_DEMOS_TEMPLATE_FILENAME = "Template - Campaign Export With Secondary Audiences.xlsx";
        private readonly string TEMPLATES_FILE_PATH;
        protected readonly IFeatureToggleHelper _FeatureToggleHelper;
        public CampaignReportGenerator(string templatesPath, IFeatureToggleHelper featureToggleHelper)
        {
            TEMPLATES_FILE_PATH = templatesPath;
            _FeatureToggleHelper = featureToggleHelper;
        }

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
            string templateFilePath = campaignReportData.HasSecondaryAudiences
                ? Path.Combine(TEMPLATES_FILE_PATH, CAMPAIGN_EXPORT_WITH_SECONDARY_DEMOS_TEMPLATE_FILENAME)
                : Path.Combine(TEMPLATES_FILE_PATH, CAMPAIGN_EXPORT_TEMPLATE_FILENAME);
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);
            ExcelWorksheet proposalWorksheet = ExportSharedLogic.GetWorksheet(templateFilePath, package, PROPOSAL_WORKSHEET_NAME);
            var proposalReportGenerator = new ProposalReportGenerator();
            proposalReportGenerator.PopulateProposalTab(campaignReportData, proposalWorksheet);

            ExcelWorksheet flowChartWorksheet = ExportSharedLogic.GetWorksheet(templateFilePath, package, FLOW_CHART_WORKSHEET_NAME);
            ExcelWorksheet flowChartTemplateTablesWorksheet = ExportSharedLogic.GetWorksheet(templateFilePath, package, FLOW_CHART_TEMPLATE_TABLES_WORKSHEET_NAME);            
            new FlowChartReportGenerator(flowChartWorksheet, flowChartTemplateTablesWorksheet,_FeatureToggleHelper)
                .PopulateFlowChartTab(campaignReportData, proposalReportGenerator.Dayparts);
            if (campaignReportData.Status.Equals("Proposal"))
            {
                package.Workbook.Worksheets.Delete(CONTRACT_WORKSHEET_NAME);
                package.Workbook.Worksheets.Delete(TERMS_WORKSHEET_NAME);
                package.Workbook.Worksheets.MoveToStart(PROPOSAL_WORKSHEET_NAME);
            }
            else
            {
                ExcelWorksheet contractWorksheet = ExportSharedLogic.GetWorksheet(CAMPAIGN_EXPORT_TEMPLATE_FILENAME, package, CONTRACT_WORKSHEET_NAME);
                new ContractReportGenerator(contractWorksheet).PopulateContractTab(campaignReportData
                    , proposalReportGenerator.MarketsCoverage
                    , proposalReportGenerator.Dayparts
                    , proposalReportGenerator.FlightHiatuses
                    , proposalReportGenerator.Notes);
            }

            //remove flow chart template tables worksheet
            package.Workbook.Worksheets.Delete(FLOW_CHART_TEMPLATE_TABLES_WORKSHEET_NAME);

            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }  
    }
}
