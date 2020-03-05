using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System;
using System.Linq;

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

        private Cell TableTopLeftCell = new Cell { Row = 9, Column = 2 };
        private Cell TableTopRightCell = new Cell { Row = 9, Column = 10 };
        #endregion

        internal void PopulateTab(ExcelWorksheet worksheet, ProgramLineupReportData reportData)
        {
            _AddHeaderInformation(worksheet, reportData);
            _PopulateTable(worksheet, reportData);
        }

        private void _AddHeaderInformation(ExcelWorksheet worksheet, ProgramLineupReportData reportData)
        {
            worksheet.Cells[PLAN_HEADER_NAME_CELL].Value = reportData.PlanHeaderName;
            worksheet.Cells[REPORT_GENERATED_AND_ACCURACY_ESTIMATE_DATES_CELL].Value = 
                $"Report generated on {reportData.ReportGeneratedDate}" +
                Environment.NewLine +
                $"Estimate as of {reportData.AccuracyEstimateDate}";
            
            worksheet.Cells[AGENCY_CELL].Value = reportData.Agency;
            worksheet.Cells[CLIENT_CELL].Value = reportData.Client;
            worksheet.Cells[FLIGHT_CELL].Value = $"{reportData.FlightStartDate} - {reportData.FlightEndDate}";
            worksheet.Cells[GUARANTEED_DEMO_CELL].Value = reportData.GuaranteedDemo;
            worksheet.Cells[SPOT_LENGTH_CELL].Value = reportData.SpotLength;
            worksheet.Cells[POSTING_TYPE_CELL].Value = reportData.PostingType;
            worksheet.Cells[ACCOUNT_EXECUTIVE_CELL].Value = reportData.AccountExecutive;
            worksheet.Cells[CLIENT_CONTRACT_CELL].Value = reportData.ClientContact;
        }

        private void _PopulateTable(ExcelWorksheet worksheet, ProgramLineupReportData reportData)
        {
            var tableBottomRightCell = new Cell
            {
                Row = TableTopLeftCell.Row + reportData.DetailedViewRows.Count - 1,
                Column = TableTopRightCell.Column
            };

            ExportSharedLogic.ExtendTable(
                worksheet,
                TableTopLeftCell,
                TableTopRightCell,
                reportData.DetailedViewRows.Count - 1);

            ExportSharedLogic.FormatTableRows(
                worksheet,
                TableTopLeftCell,
                tableBottomRightCell);

            var tableData = reportData.DetailedViewRows
                .Select(x => new object[]
                {
                    x.Rank,
                    x.DMA,
                    x.Station,
                    x.NetworkAffiliation,
                    x.Days,
                    x.TimePeriods,
                    x.Program,
                    x.Genre,
                    x.Daypart
                })
                .ToList();

            worksheet.Cells[TableTopLeftCell.Row, TableTopLeftCell.Column].LoadFromArrays(tableData);
        }
    }
}
