using OfficeOpenXml;
using Services.Broadcast.Entities.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class FlowChartReportGenerator
    {
        private readonly int FIRST_MONTH_COLUMN_INDEX = 3;
        private readonly int SECOND_MONTH_COLUMN_INDEX = 7;
        private readonly int THIRD_MONTH_COLUMN_INDEX = 11;
        private readonly string TABLE_TITLE_COLUMN = "B";
        private readonly (string Column, int Row) DAYPARTS = ("D", 5);

        private string[] hiatusDaysColumn = { "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P" };

        private readonly string TABLE_12_WEEKS = "A2:O9";       

        private readonly string TABLE_13_WEEKS_MONTH_1_V2 = "A12:P21";   //month 1 has 5 weeks
        private readonly string TABLE_13_WEEKS_MONTH_2_V2 = "A22:P31";   //month 2 has 5 weeks
        private readonly string TABLE_13_WEEKS_MONTH_3_V2 = "A32:P41";   //month 3 has 5 weeks

        private readonly string TABLE_14_WEEKS_MONTH_1_V2 = "A62:Q71";   //month 2 & 3 have 5 weeks
        private readonly string TABLE_14_WEEKS_MONTH_2_V2 = "A52:Q61";   //month 1 & 3 have 5 weeks
        private readonly string TABLE_14_WEEKS_MONTH_3_V2 = "A42:Q51";   //month 1 & 2 have 5 weeks

        private readonly int FIVE_WEEKS = 5;
        private readonly int FOUR_WEEKS = 4;

        private readonly int ROWS_TO_COPY = 9;
        private int planNameRowIndex = 7;
        private int currentRowIndex = 0;

        private readonly ExcelWorksheet WORKSHEET;
        private readonly ExcelWorksheet TEMPLATES_WORKSHEET;           
        public FlowChartReportGenerator(ExcelWorksheet flowChartWorksheet, ExcelWorksheet templatesWorksheet, IFeatureToggleHelper featureToggleHelper)
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
                    if (table.Months[0].WeeksInMonth == FIVE_WEEKS) result = TABLE_13_WEEKS_MONTH_1_V2;
                    if (table.Months[1].WeeksInMonth == FIVE_WEEKS) result = TABLE_13_WEEKS_MONTH_2_V2;
                    if (table.Months[2].WeeksInMonth == FIVE_WEEKS) result = TABLE_13_WEEKS_MONTH_3_V2;
                    break;
                case 14:
                    if (table.Months[0].WeeksInMonth == FOUR_WEEKS) result = TABLE_14_WEEKS_MONTH_1_V2;
                    if (table.Months[1].WeeksInMonth == FOUR_WEEKS) result = TABLE_14_WEEKS_MONTH_2_V2;
                    if (table.Months[2].WeeksInMonth == FOUR_WEEKS) result = TABLE_14_WEEKS_MONTH_3_V2;
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
            int offsetForThirdMonth = table.Months[1].WeeksInMonth == FIVE_WEEKS ? 1 + offsetForSecondMonth : offsetForSecondMonth;

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
            currentRowIndex++;

            //add Total Monthly Cost            
            if (!table.TableTitle.Contains("ADU"))
            {
                flowChartWorksheet.Cells[currentRowIndex, FIRST_MONTH_COLUMN_INDEX]
                    .Value = table.MonthlyCostValues[0];
                flowChartWorksheet.Cells[currentRowIndex, (SECOND_MONTH_COLUMN_INDEX + offsetForSecondMonth)]
                    .Value = table.MonthlyCostValues[1];
                flowChartWorksheet.Cells[currentRowIndex, (THIRD_MONTH_COLUMN_INDEX + offsetForThirdMonth)]
                    .Value = table.MonthlyCostValues[2];
                currentRowIndex++;
            }
            if (table.TableTitle.Contains("Summary"))
            {
                for (int i = 0; i < table.HiatusDaysFormattedValues.Count; i++)
                {
                    flowChartWorksheet.Cells[$"{hiatusDaysColumn[i]}{currentRowIndex}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                }
            }
            //next table will be starting 2 rows down
            currentRowIndex += 2;
        }
    }
}
