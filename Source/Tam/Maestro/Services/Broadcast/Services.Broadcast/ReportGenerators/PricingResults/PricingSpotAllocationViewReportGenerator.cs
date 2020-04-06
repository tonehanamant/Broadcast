using OfficeOpenXml;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.PricingResults
{
    public class PricingSpotAllocationViewReportGenerator
    {
        #region Cells addresses
        private readonly string PLAN_ID_CELL = "B1";
        private readonly string PLAN_VERSION_CELL = "B2";

        private readonly string TOTAL_SPOTS_CELL = "C5";
        private readonly string TOTAL_IMPRESSIONS_CELL = "D5";
        private readonly string TOTAL_COST_CELL = "E5";
        private readonly string TOTAL_CPM_CELL = "F5";
        
        private (int Row, int Column) TableTopLeftCell = (Row: 9, Column: 1);
        private (int Row, int Column) TableTopRightCell = (Row: 9, Column: 15);
        #endregion

        internal void PopulateTab(ExcelWorksheet worksheet, PricingResultsReportData reportData)
        {
            _PopulateHeaders(worksheet, reportData);
            _PopulateTotals(worksheet, reportData);
            _PopulateTable(worksheet, reportData);
        }

        private void _PopulateHeaders(ExcelWorksheet worksheet, PricingResultsReportData reportData)
        {
            worksheet.Cells[PLAN_ID_CELL].Value = reportData.PlanId;
            worksheet.Cells[PLAN_VERSION_CELL].Value = reportData.PlanVersion;
        }

        private void _PopulateTotals(ExcelWorksheet worksheet, PricingResultsReportData reportData)
        {
            worksheet.Cells[TOTAL_SPOTS_CELL].Value = reportData.SpotAllocationTotals.Spots;
            worksheet.Cells[TOTAL_IMPRESSIONS_CELL].Value = reportData.SpotAllocationTotals.Impressions;
            worksheet.Cells[TOTAL_COST_CELL].Value = reportData.SpotAllocationTotals.Cost;
            worksheet.Cells[TOTAL_CPM_CELL].Value = reportData.SpotAllocationTotals.CPM;
        }

        private void _PopulateTable(ExcelWorksheet worksheet, PricingResultsReportData reportData)
        {
            ExportSharedLogic.ExtendTable(
                worksheet,
                TableTopLeftCell,
                TableTopRightCell,
                reportData.SpotAllocations.Count - 1);

            var tableData = reportData.SpotAllocations
                .Select(x => new object[]
                {
                    x.ProgramName,
                    x.Genre,
                    x.ShowType,
                    x.Station,
                    x.Market,
                    x.DaypartCode,
                    x.Spots,
                    x.TotalImpressions,
                    x.TotalCost,
                    x.CPM,
                    x.PlanWeekNumber,
                    x.StartDate,
                    x.StartTime,
                    x.EndDate,
                    x.EndTime
                })
                .ToList();

            worksheet.Cells[TableTopLeftCell.Row, TableTopLeftCell.Column].LoadFromArrays(tableData);
        }
    }
}
