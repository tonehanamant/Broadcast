using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using Services.Broadcast.Entities.Campaign;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class ContractReportGenerator
    {
        private readonly ExcelWorksheet WORKSHEET;
        private readonly string FOOTER_INFO_COLUMN = "D";
        private readonly string TOTAL_START_COLUMN = "D";
        private readonly int ROWS_TO_COPY = 4;
        private readonly string START_COLUMN = "A";
        private readonly string PLAN_NAME_COLUMN = "C";
        private int planNameRowIndex = 7;
        private int currentRowIndex = 0;
        private int firstDataRow = 9;

        public ContractReportGenerator(ExcelWorksheet contractWorksheet)
        {
            WORKSHEET = contractWorksheet;
        }

        internal void PopulateContractTab(CampaignReportData campaignReportData
            , string marketsCoverage, string dayparts, string flightHiatuses, string notes)
        {
            ExportSharedLogic.AddEmptyTables(WORKSHEET, campaignReportData.ContractQuarterTables.Count()
                , planNameRowIndex, ROWS_TO_COPY);
            currentRowIndex = firstDataRow;
            for (int i = 0; i < campaignReportData.ContractQuarterTables.Count(); i++)
            {
                _AddTable(campaignReportData, i);
            }
            _AddContractTabTotalTable(campaignReportData.ContractTotals);
            _AddFooterData(campaignReportData, marketsCoverage, dayparts, flightHiatuses, notes);
        }

        private void _AddFooterData(CampaignReportData campaignReportData, string marketsCoverage
            , string dayparts, string flightHiatuses, string notes)
        {
            //markets coverage row is 4 rows below the total table
            currentRowIndex += 4;
            WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN}{currentRowIndex}"].Value = marketsCoverage;

            //dayparts row is 2 below
            currentRowIndex += 2;
            WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN}{currentRowIndex}"].Value = dayparts;

            //content restrinctions row is 2 below
            currentRowIndex += 2;
            ExportSharedLogic.PopulateContentRestrictions(WORKSHEET, campaignReportData.DaypartsData,
                $"{FOOTER_INFO_COLUMN}{currentRowIndex}");

            //flight hiatuses row is 2 below
            currentRowIndex += 2;
            WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN}{currentRowIndex}"].Value = flightHiatuses;

            //notes row is 2 below
            currentRowIndex += 2;
            WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN}{currentRowIndex}"].Value = notes;
        }

        private void _AddTable(CampaignReportData campaignReportData, int tableIndex)
        {
            _AddTableData(campaignReportData.ContractQuarterTables[tableIndex]);

            //update row indices if there is another table
            if (tableIndex < campaignReportData.ContractQuarterTables.Count() - 1)
            {
                //next table starts 2 rows below
                currentRowIndex += 2;
                planNameRowIndex = currentRowIndex;

                //skip the header of the table
                firstDataRow = currentRowIndex + 2;
            }
        }

        private void _AddContractTabTotalTable(List<object> contractTotals)
        {
            //the total table data row is 4th below the last total row
            currentRowIndex += 4;
            WORKSHEET.Cells[$"{PLAN_NAME_COLUMN}{currentRowIndex}"]
                    .LoadFromArrays(new List<object[]> { contractTotals.ToArray() });
        }

        private void _AddTableData(ContractQuarterTableData table)
        {
            WORKSHEET.Cells[$"{PLAN_NAME_COLUMN}{planNameRowIndex}"].Value = table.Title;

            //set height for the table header
            WORKSHEET.Row(planNameRowIndex + 1).Height = ExportSharedLogic.ROW_HEIGHT;

            currentRowIndex = firstDataRow;
            //add count - 1 rows because we already have 1 row in the table
            WORKSHEET.InsertRow(currentRowIndex + 1, table.Rows.Count - 1, firstDataRow);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                row.Insert(0, i % 2 == 0 ? "Odd" : "Even"); // this value is for the conditional formatting
                row.Insert(1, null); //column B does not have data on it

                WORKSHEET.Cells[$"{START_COLUMN}{currentRowIndex}"]
                    .LoadFromArrays(new List<object[]> { row.ToArray() });

                WORKSHEET.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
                currentRowIndex++;  //go to next row
            }

            WORKSHEET.Cells[$"{TOTAL_START_COLUMN}{currentRowIndex}"]
                    .LoadFromArrays(new List<object[]> { table.TotalRow.ToArray() });
            WORKSHEET.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
        }
    }
}
