using OfficeOpenXml;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.Collections.Generic;
using System.Linq;
using static Services.Broadcast.Entities.Campaign.ProgramLineupReportData;

namespace Services.Broadcast.ReportGenerators.ProgramLineup
{
    public class AllocationsReportGenerator
    {
        private (int Row, int StartColumn, int EndColumn) AllocationsByDaypartTable = (Row: 11, StartColumn: 3, EndColumn:5);
        private (int Row, int StartColumn, int EndColumn) AllocationsByGenreTable = (Row: 15, StartColumn: 3, EndColumn: 5);
        private (int Row, int StartColumn, int EndColumn) AllocationsByDMATable = (Row: 19, StartColumn: 3, EndColumn: 5);

        internal void PopulateTab(ExcelWorksheet worksheet, ProgramLineupReportData reportData)
        {
            _PopulateAllocationTable(worksheet, reportData.AllocationByDMAViewRows, AllocationsByDMATable);
            _PopulateAllocationTable(worksheet, reportData.AllocationByGenreViewRows, AllocationsByGenreTable);
            _PopulateAllocationTable(worksheet, reportData.AllocationByDaypartViewRows, AllocationsByDaypartTable);
        }

        private void _PopulateAllocationTable(ExcelWorksheet worksheet
            , List<AllocationViewRowDisplay> rows
            , (int Row, int StartColumn, int EndColumn) allocationsTable)
        {
            (int Row, int Column) tableBottomRightCell =
            (
                Row: (allocationsTable.Row + (rows.Count - 1))
                , allocationsTable.EndColumn
            );
            worksheet.InsertRow(allocationsTable.Row + 1, rows.Count - 1);
            
            ExportSharedLogic.ExtendTable(
                worksheet,
                (allocationsTable.Row, allocationsTable.StartColumn),
                (allocationsTable.Row, allocationsTable.EndColumn),
                 rows.Count - 1);

            ExportSharedLogic.FormatTableRows(
                worksheet,
                (allocationsTable.Row, allocationsTable.StartColumn),
                tableBottomRightCell);

            var tableData = rows
                .Select(x => new object[]
                {
                    ExportSharedLogic.EMPTY_CELL,
                    x.FilterLabel,
                    x.Weight
                })
                .ToList();

            worksheet.Cells[allocationsTable.Row, allocationsTable.StartColumn].LoadFromArrays(tableData);
        }
    }
}
