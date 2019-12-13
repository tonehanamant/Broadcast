using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using System;
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

        private readonly string NOT_FOUND_WORKSHEET = "Could not find worksheet {0} in template file {1}";

        private readonly string TEMPLATE_FILENAME = "Template - Campaign Export.xlsx";

        #region Cells addresses
        private readonly string CREATED_DATE_CELL = "T2";
        private readonly string CAMPAIGN_NAME_CELL = "F2";
        private readonly string AGENCY_NAME_CELL = "C5";
        private readonly string CLIENT_NAME_CELL = "E5";
        private readonly string CAMPAIGN_FLIGHT_CELL = "H5";
        private readonly string GUARANTEED_DEMO_CELL = "J5";
        private readonly string CAMPAIGN_SPOT_LENGTH_CELL = "L5";
        private readonly string POSTING_TYPE_CELL = "N5";
        private readonly string STATUS_CELL = "T5";
        #endregion

        /// <summary>
        /// Generates a report of type CampaignReport
        /// </summary>
        /// <param name="dataObject">Data object used to generate the file</param>
        /// <returns>
        /// ReportOutput object containing the generated file stream
        /// </returns>
        public ReportOutput Generate(CampaignReportData dataObject)
        {
            var output = new ReportOutput(filename: $"Broadcast Export Plan Rev - {DateTime.Now.ToString("MM-dd")}.xlsx");

            var package = _GetFileWithData(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage _GetFileWithData(CampaignReportData campaignReportData)
        {
            string templateFilePath = $@"{BroadcastServiceSystemParameter.BroadcastExcelTemplatesPath}\{TEMPLATE_FILENAME}";
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);

            ExcelWorksheet proposalWorksheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name.Equals(PROPOSAL_WORKSHEET_NAME));
            if (proposalWorksheet == null)
            {
                throw new Exception(string.Format(NOT_FOUND_WORKSHEET, PROPOSAL_WORKSHEET_NAME, Path.GetFileName(templateFilePath)));
            }
            _PopulateProposalWorksheetHeader(proposalWorksheet, campaignReportData);

            if (campaignReportData.Status.Equals("Proposal"))
            {
                package.Workbook.Worksheets.Delete(CONTRACT_WORKSHEET_NAME);
                package.Workbook.Worksheets.Delete(TERMS_WORKSHEET_NAME);
                package.Workbook.Worksheets.MoveToStart(PROPOSAL_WORKSHEET_NAME);
            }
            else
            {
                ExcelWorksheet contractWorksheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name.Equals(PROPOSAL_WORKSHEET_NAME));
                if (contractWorksheet == null)
                {
                    throw new Exception(string.Format(NOT_FOUND_WORKSHEET, PROPOSAL_WORKSHEET_NAME, Path.GetFileName(templateFilePath)));
                }
               // _PopulateContractWorksheet(contractWorksheet, dataObject);
            }

            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }
        
        private void _PopulateProposalWorksheetHeader(ExcelWorksheet worksheet, CampaignReportData data)
        {
            worksheet.Cells[CREATED_DATE_CELL].Value += data.CreatedDate;
            worksheet.Cells[CAMPAIGN_NAME_CELL].Value += data.CampaignName;
            worksheet.Cells[AGENCY_NAME_CELL].Value = data.AgencyName;
            worksheet.Cells[CLIENT_NAME_CELL].Value = data.ClientName;
            worksheet.Cells[CAMPAIGN_FLIGHT_CELL].Value = $"{data.CampaignFlightStartDate} - {data.CampaignFlightEndDate}";
            worksheet.Cells[GUARANTEED_DEMO_CELL].Value = data.GuaranteedDemo;
            worksheet.Cells[CAMPAIGN_SPOT_LENGTH_CELL].Value = string.Join(", ", data.SpotLengths);
            worksheet.Cells[POSTING_TYPE_CELL].Value = data.PostingType;
            worksheet.Cells[STATUS_CELL].Value = data.Status;
        }

        private void _PopulateContractWorksheet(ExcelWorksheet contractWorksheet, CampaignReportData dataObject)
        {
            throw new NotImplementedException();
        }
    }
}
