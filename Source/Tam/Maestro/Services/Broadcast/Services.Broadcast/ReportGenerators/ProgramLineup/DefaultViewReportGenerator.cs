using OfficeOpenXml;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System.Collections.Generic;
using System.Linq;
using static Services.Broadcast.Entities.Campaign.ProgramLineupReportData;

namespace Services.Broadcast.ReportGenerators.ProgramLineup
{
    public class DefaultViewReportGenerator
    {
        private (int Row, int Column) TableTopLeftCell = (Row: 9, Column: 3);
        private (int Row, int Column) TableTopRightCell = (Row: 9, Column: 9);

        internal void PopulateTab(ExcelWorksheet worksheet, List<DefaultViewRowDisplay> rows)
        {
            _PopulateTable(worksheet, rows);
        }

        private void _PopulateTable(ExcelWorksheet worksheet, List<DefaultViewRowDisplay> rows)
        {
            (int Row, int Column) tableBottomRightCell =
            (
                Row: TableTopLeftCell.Row + rows.Count - 1
                , TableTopRightCell.Column
            );

            ExportSharedLogic.ExtendTable(
                worksheet,
                TableTopLeftCell,
                TableTopRightCell,
                rows.Count - 1);

            ExportSharedLogic.FormatTableRows(
                worksheet,
                TableTopLeftCell,
                tableBottomRightCell);

            var tableData = rows
                .Select(x => new object[]
                {
                    ExportSharedLogic.EMPTY_CELL,
                    x.Program,
                    x.Weight,
                    x.Genre,
                    x.NoOfStations,
                    x.NoOfMarkets,
                    ExportSharedLogic.EMPTY_CELL
                })
                .ToList();

            worksheet.Cells[TableTopLeftCell.Row, TableTopLeftCell.Column].LoadFromArrays(tableData);
        }
    }
}
