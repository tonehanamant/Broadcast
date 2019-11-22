using OfficeOpenXml;
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
        private readonly string PROPOSAL_BY_QUARTER_WORKSHEET_NAME = "Proposal (By Quarter)";

        private readonly string NOT_FOUND_WORKSHEET = "Could not find worksheet {0} in template file {1}";

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
            var output = new ReportOutput($"{dataObject.CampaignName} exported proposal_{DateTime.Now.ToString("yyyy-MM-dd")}.xlsx");

            var package = GenerateExcelPackage(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage GenerateExcelPackage(CampaignReportData dataObject)
        {
            string templateFilePath = $@"{BroadcastServiceSystemParameter.BroadcastExcelTemplatesPath}\{TEMPLATE_FILENAME}";
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream:true);
            ExcelWorksheet proposalByQuarterWorksheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name.Equals(PROPOSAL_BY_QUARTER_WORKSHEET_NAME));
            if(proposalByQuarterWorksheet == null)
            {
                throw new Exception(string.Format(NOT_FOUND_WORKSHEET, PROPOSAL_BY_QUARTER_WORKSHEET_NAME, Path.GetFileName(templateFilePath)));
            }
            _PopulateProposalByQuarterWorksheet(proposalByQuarterWorksheet, dataObject);
            return package;
        }

        private void _PopulateProposalByQuarterWorksheet(ExcelWorksheet worksheet, CampaignReportData data)
        {
            worksheet.Cells["T2"].Value += data.CreatedDate;
            worksheet.Cells["D2"].Value += $"{data.CampaignName} {data.CampaignStartQuarter} - {data.CampaignEndQuarter}";
        }
    }
}
