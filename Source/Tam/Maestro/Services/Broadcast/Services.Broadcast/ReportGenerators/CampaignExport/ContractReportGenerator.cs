using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using Services.Broadcast.Entities.Campaign;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class ContractReportGenerator
    {
        private readonly string FOOTER_INFO_COLUMN_INDEX = "D";
        private readonly int ROWS_TO_COPY = 4;
        private readonly string START_COLUMN = "A";
        private readonly string PLAN_NAME_COLUMN = "C";
        private int planNameRowIndex = 7;
        private int currentRowIndex = 0;
        private int firstDataRow = 9;

        internal void PopulateContractTab(CampaignReportData campaignReportData, ExcelWorksheet contractWorksheet)
        {
            ExportSharedLogic.AddEmptyTables(contractWorksheet, campaignReportData.ContractQuarterTables.Count()
                , planNameRowIndex, ROWS_TO_COPY);
            currentRowIndex = firstDataRow;
            for (int i = 0; i < campaignReportData.ContractQuarterTables.Count(); i++)
            {
                _AddTableData(contractWorksheet, campaignReportData.ContractQuarterTables[i]);

                //update row indices if there is another table
                if (i < campaignReportData.ContractQuarterTables.Count() - 1)
                {
                    //next table starts 2 rows below
                    currentRowIndex += 2;
                    planNameRowIndex = currentRowIndex;

                    //skip the header of the table
                    firstDataRow = currentRowIndex + 2;
                }
            }
            _AddTotalContractTabData(campaignReportData.ContractTotals, contractWorksheet);

            //content restrinctions row is 8 rows below the total row on this tab
            currentRowIndex += 8;
            ExportSharedLogic.PopulateContentRestrictions(contractWorksheet, campaignReportData.DaypartsData,
                $"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}");
        }

        private void _AddTotalContractTabData(List<object> contractTotals, ExcelWorksheet contractWorksheet)
        {
            //the total table data row is 4th below the last total row
            currentRowIndex += 4;
            contractWorksheet.Cells[$"{PLAN_NAME_COLUMN}{currentRowIndex}"]
                    .LoadFromArrays(new List<object[]> { contractTotals.ToArray() });
        }

        private void _AddTableData(ExcelWorksheet contractWorksheet, ContractQuarterTableData table)
        {
            contractWorksheet.Cells[$"{PLAN_NAME_COLUMN}{planNameRowIndex}"].Value = table.Title;

            //set height for the table header
            contractWorksheet.Row(planNameRowIndex + 1).Height = ExportSharedLogic.ROW_HEIGHT;

            currentRowIndex = firstDataRow;
            //add count - 1 rows because we already have 1 row in the table
            contractWorksheet.InsertRow(currentRowIndex + 1, table.Rows.Count - 1, firstDataRow);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                row.Insert(0, i % 2 == 0 ? "Odd" : "Even"); // this value is for the conditional formatting
                row.Insert(1, null); //column B does not have data on it

                contractWorksheet.Cells[$"{START_COLUMN}{currentRowIndex}"]
                    .LoadFromArrays(new List<object[]> { row.ToArray() });

                contractWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
                currentRowIndex++;  //go to next row
            }

            var totalRow = table.TotalRow;
            totalRow.Insert(0, table.Rows.Count % 2 == 0 ? "Odd" : "Even");
            totalRow.Insert(1, null);
            contractWorksheet.Cells[$"{START_COLUMN}{currentRowIndex}"]
                    .LoadFromArrays(new List<object[]> { totalRow.ToArray() });
            contractWorksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
        }
    }
}
