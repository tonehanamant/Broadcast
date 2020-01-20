using OfficeOpenXml;
using Services.Broadcast.Entities.Campaign;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class FlowChartReportGenerator
    {
        private readonly string FIRST_MONTH_COLUMN = "C";
        private readonly string SECOND_MONTH_COLUMN = "G";
        private readonly string THIRD_MONTH_COLUMN = "K";
        private readonly string TABLE_TITLE_COLUMN = "B";
        
        private readonly int ROWS_TO_COPY = 7;
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
            flowChartWorksheet.Cells[$"{FIRST_MONTH_COLUMN}{currentRowIndex}"].Value = table.Months[0].MonthName;
            flowChartWorksheet.Cells[$"{SECOND_MONTH_COLUMN}{currentRowIndex}"].Value = table.Months[1].MonthName;
            flowChartWorksheet.Cells[$"{THIRD_MONTH_COLUMN}{currentRowIndex}"].Value = table.Months[2].MonthName;
            
            currentRowIndex++;

            //add week start dates
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex++}"]
                .LoadFromArrays(new List<object[]> { table.Months.SelectMany(x => x.Weeks.Select(y => (object)y.WeekStartDate)).ToArray() });

            //add distribution percentages
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex++}"]
                .LoadFromArrays(new List<object[]> { table.Months.SelectMany(x => x.Weeks.Select(y => (object)y.DistributionPercentage)).ToArray() });
            
            //add units
            List<object> rowData = table.Months.SelectMany(x => x.Weeks.Select(y => (object)y.Units)).ToList();
            rowData.Add(table.Total.Units);
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex++}"]
                .LoadFromArrays(new List<object[]> {rowData.ToArray()});

            //add impressions
            rowData = table.Months.SelectMany(x => x.Weeks.Select(y => (object)y.Impressions)).ToList();
            rowData.Add(table.Total.Impressions);
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex++}"]
                .LoadFromArrays(new List<object[]>{ rowData.ToArray() });

            //add CPM
            rowData = table.Months.SelectMany(x => x.Weeks.Select(y => (object)y.CPM)).ToList();
            rowData.Add(table.Total.CPM);
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex++}"]
                .LoadFromArrays(new List<object[]> { rowData.ToArray() });

            //add cost
            rowData = table.Months.SelectMany(x => x.Weeks.Select(y => (object)y.Cost)).ToList();
            rowData.Add(table.Total.Cost);
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            flowChartWorksheet.Cells[$"C{currentRowIndex}"]
                .LoadFromArrays(new List<object[]> { rowData.ToArray() });
            
            //next table will be starting 2 rows down
            currentRowIndex += 2;
        }
    }
}
