using OfficeOpenXml;
using Services.Broadcast.Entities.Campaign;
using System.Collections.Generic;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class FlowChartReportGenerator
    {
        private readonly string FIRST_MONTH_COLUMN = "C";
        private readonly string SECOND_MONTH_COLUMN = "G";
        private readonly string THIRD_MONTH_COLUMN = "K";
        private readonly string TABLE_TITLE_COLUMN = "B";

        private readonly int ROWS_TO_COPY = 8;
        private int planNameRowIndex = 7;
        private int currentRowIndex = 0;

        public void PopulateFlowChartTab(CampaignReportData campaignReportData, ExcelWorksheet flowChartWorksheet)
        {
            currentRowIndex = planNameRowIndex;
            ExportSharedLogic.AddEmptyTables(flowChartWorksheet
                , campaignReportData.FlowChartQuarterTables.Count
                , planNameRowIndex, planNameRowIndex + ROWS_TO_COPY, ROWS_TO_COPY);
            foreach (var table in campaignReportData.FlowChartQuarterTables)
            {
                _AddFlowChartTable(flowChartWorksheet, table);
            }
        }

        private void _AddFlowChartTable(ExcelWorksheet flowChartWorksheet, FlowChartQuarterTableData table)
        {
            //add table title and months label
            flowChartWorksheet.Cells[$"{TABLE_TITLE_COLUMN}{currentRowIndex}"].Value = table.TableTitle;
            flowChartWorksheet.Cells[$"{FIRST_MONTH_COLUMN}{currentRowIndex}"].Value = table.MonthsLabel[0];
            flowChartWorksheet.Cells[$"{SECOND_MONTH_COLUMN}{currentRowIndex}"].Value = table.MonthsLabel[1];
            flowChartWorksheet.Cells[$"{THIRD_MONTH_COLUMN}{currentRowIndex}"].Value = table.MonthsLabel[2];

            currentRowIndex++;

            //add week start dates
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex}"]
                .LoadFromArrays(new List<object[]> { table.WeeksStartDate.ToArray() });
            currentRowIndex++;

            //add distribution percentages
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex}"]
                .LoadFromArrays(new List<object[]> { table.DistributionPercentages.ToArray() });
            currentRowIndex++;

            //add units
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex}"]
                .LoadFromArrays(new List<object[]> { table.UnitsValues.ToArray() });
            currentRowIndex++;

            //add impressions
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex}"]
                .LoadFromArrays(new List<object[]> { table.ImpressionsValues.ToArray() });
            currentRowIndex++;

            //add CPM
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex}"]
                .LoadFromArrays(new List<object[]> { table.CPMValues.ToArray() });
            currentRowIndex++;

            //add cost
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex}"]
                .LoadFromArrays(new List<object[]> { table.CostValues.ToArray() });
            currentRowIndex++;

            //add hiatus days
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex}"]
                .LoadFromArrays(new List<object[]> { table.HiatusDaysFormattedValues.ToArray() });

            //next table will be starting 2 rows down
            currentRowIndex += 2;
        }
    }
}
