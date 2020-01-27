using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.Entities.Campaign;

namespace Services.Broadcast.ReportGenerators.ProgramLineup
{
    public class DetailedViewReportGenerator
    {
        #region Cells addresses
        private readonly string PLAN_HEADER_NAME_CELL = "D2";
        private readonly string REPORT_GENERATED_AND_ACCURACY_ESTIMATE_DATES_CELL = "I2";
        private readonly string AGENCY_CELL = "B6";
        private readonly string CLIENT_CELL = "D6";
        private readonly string FLIGHT_CELL = "E6";
        private readonly string GUARANTEED_DEMO_CELL = "G6";
        private readonly string SPOT_LENGTH_CELL = "H6";
        private readonly string POSTING_TYPE_CELL = "I6";
        private readonly string ACCOUNT_EXECUTIVE_CELL = "J6";
        private readonly string CLIENT_CONTRACT_CELL = "K6";
        #endregion

        internal void PopulateTab(ExcelWorksheet worksheet, ProgramLineupReportData reportData)
        {
            _AddHeaderInformation(worksheet, reportData);
        }

        private void _AddHeaderInformation(ExcelWorksheet worksheet, ProgramLineupReportData reportData)
        {
            worksheet.Cells[PLAN_HEADER_NAME_CELL].Value = reportData.PlanHeaderName;
            worksheet.Cells[REPORT_GENERATED_AND_ACCURACY_ESTIMATE_DATES_CELL].Value = 
                $"Report generated on {reportData.ReportGeneratedDate}\nMost accurate estimate as of {reportData.AccuracyEstimateDate}";
            
            worksheet.Cells[AGENCY_CELL].Value = reportData.Agency;
            worksheet.Cells[CLIENT_CELL].Value = reportData.Client;
            worksheet.Cells[FLIGHT_CELL].Value = $"{reportData.FlightStartDate} - {reportData.FlightEndDate}";
            worksheet.Cells[GUARANTEED_DEMO_CELL].Value = reportData.GuaranteedDemo;
            worksheet.Cells[SPOT_LENGTH_CELL].Value = reportData.SpotLength;
            worksheet.Cells[POSTING_TYPE_CELL].Value = reportData.PostingType;
            worksheet.Cells[ACCOUNT_EXECUTIVE_CELL].Value = reportData.AccountExecutive;
            worksheet.Cells[CLIENT_CONTRACT_CELL].Value = reportData.ClientContact;
        }
    }
}
