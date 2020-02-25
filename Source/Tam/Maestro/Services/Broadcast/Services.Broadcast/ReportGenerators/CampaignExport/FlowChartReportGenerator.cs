using OfficeOpenXml;
using Services.Broadcast.Entities.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class FlowChartReportGenerator
    {
        private readonly int FIRST_MONTH_COLUMN_INDEX = 3;
        private readonly int SECOND_MONTH_COLUMN_INDEX = 7;
        private readonly int THIRD_MONTH_COLUMN_INDEX = 11;
        private readonly string TABLE_TITLE_COLUMN = "B";
        private readonly (string Column, int Row) DAYPARTS = ("D", 5);

        private readonly string TABLE_12_WEEKS = "A2:O9";

        private readonly string TABLE_13_WEEKS_MONTH_1 = "A11:P18";   //month 1 has 5 weeks
        private readonly string TABLE_13_WEEKS_MONTH_2 = "A20:P27";   //month 2 has 5 weeks
        private readonly string TABLE_13_WEEKS_MONTH_3 = "A29:P36";   //month 3 has 5 weeks

        private readonly string TABLE_14_WEEKS_MONTH_1 = "A56:Q63";   //month 2 & 3 have 5 weeks        
        private readonly string TABLE_14_WEEKS_MONTH_2 = "A47:Q54";   //month 1 & 3 have 5 weeks
        private readonly string TABLE_14_WEEKS_MONTH_3 = "A38:Q45";   //month 1 & 2 have 5 weeks

        private readonly int FIVE_WEEKS = 5;
        private readonly int FOUR_WEEKS = 4;

        private readonly int ROWS_TO_COPY = 8;
        private int planNameRowIndex = 7;
        private int currentRowIndex = 0;
        
        private readonly ExcelWorksheet WORKSHEET;
        private readonly ExcelWorksheet TEMPLATES_WORKSHEET;

        public FlowChartReportGenerator(ExcelWorksheet flowChartWorksheet, ExcelWorksheet templatesWorksheet)
        {
            WORKSHEET = flowChartWorksheet;
            TEMPLATES_WORKSHEET = templatesWorksheet;
        }

        public void PopulateFlowChartTab(CampaignReportData campaignReportData, string dayparts)
        {
            _PopulateDayparts(dayparts);
            currentRowIndex = planNameRowIndex;            
            foreach (var table in campaignReportData.FlowChartQuarterTables)
            {
                string address = _FindTemplateTableAddress(TEMPLATES_WORKSHEET, table);
                _CopyTemplateTable(TEMPLATES_WORKSHEET, WORKSHEET, address);
                _AddFlowChartTable(WORKSHEET, table);
            }
        }

        private void _PopulateDayparts(string dayparts)
        {
            WORKSHEET.Cells[$"{DAYPARTS.Column}{DAYPARTS.Row}"].Value = dayparts;
        }
        
        private string _FindTemplateTableAddress(ExcelWorksheet templateTablesWorksheet, FlowChartQuarterTableData table)
        {
            string result = string.Empty;
            switch (table.TotalWeeksInQuarter)
            {
                case 12:
                    result = TABLE_12_WEEKS;
                    break;
                case 13:
                    if (table.Months[0].WeeksInMonth == FIVE_WEEKS) result = TABLE_13_WEEKS_MONTH_1;
                    if (table.Months[1].WeeksInMonth == FIVE_WEEKS) result = TABLE_13_WEEKS_MONTH_2;
                    if (table.Months[2].WeeksInMonth == FIVE_WEEKS) result = TABLE_13_WEEKS_MONTH_3;
                    break;
                case 14:
                    if (table.Months[0].WeeksInMonth == FOUR_WEEKS) result = TABLE_14_WEEKS_MONTH_1;
                    if (table.Months[1].WeeksInMonth == FOUR_WEEKS) result = TABLE_14_WEEKS_MONTH_2;
                    if (table.Months[2].WeeksInMonth == FOUR_WEEKS) result = TABLE_14_WEEKS_MONTH_3;
                    break;
            }
            return string.IsNullOrWhiteSpace(result)
                ? throw new ApplicationException($"Could not find the correct flow chart template table. Quarter {table.QuarterLabel}")
                : result;
        }

        private void _CopyTemplateTable(ExcelWorksheet templateTablesWorksheet
            , ExcelWorksheet flowChartWorksheet, string address)
        {
            flowChartWorksheet.InsertRow(currentRowIndex, ROWS_TO_COPY + 1);

            templateTablesWorksheet.Cells[address]
                    .Copy(flowChartWorksheet.Cells[currentRowIndex, 1]);
        }

        private void _AddFlowChartTable(ExcelWorksheet flowChartWorksheet, FlowChartQuarterTableData table)
        {
            //add table title and months label
            int offsetForSecondMonth = table.Months[0].WeeksInMonth == FIVE_WEEKS ? 1 : 0;
            int offsetForThirdMonth = table.Months[1].WeeksInMonth == FIVE_WEEKS ? 1+offsetForSecondMonth : offsetForSecondMonth;

            flowChartWorksheet.Cells[$"{TABLE_TITLE_COLUMN}{currentRowIndex}"].Value = table.TableTitle;
            flowChartWorksheet.Cells[currentRowIndex, FIRST_MONTH_COLUMN_INDEX]
                .Value = table.Months[0].Name;
            flowChartWorksheet.Cells[currentRowIndex, (SECOND_MONTH_COLUMN_INDEX + offsetForSecondMonth)]
                .Value = table.Months[1].Name;
            flowChartWorksheet.Cells[currentRowIndex, (THIRD_MONTH_COLUMN_INDEX + offsetForThirdMonth)]
                .Value = table.Months[2].Name;

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
            flowChartWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT_LARGE;
            flowChartWorksheet.Cells[$"C{currentRowIndex}"]
                .LoadFromArrays(new List<object[]> { table.HiatusDaysFormattedValues.ToArray() });

            //next table will be starting 2 rows down
            currentRowIndex += 2;
        }
    }
}
