using OfficeOpenXml;
using Services.Broadcast.Entities.Vpvh;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IVpvhExportEngine
    {
        /// <summary>
        /// Converts VPVH quarter list to an excel file
        /// </summary>
        /// <param name="vpvhQuarters">VPVH quarter list</param>
        /// <returns>Excel file</returns>
        ExcelPackage ExportQuarters(List<VpvhQuarter> vpvhQuarters);
    }

    public class VpvhExportEngine : BroadcastBaseClass, IVpvhExportEngine
    {


        public ExcelPackage ExportQuarters(List<VpvhQuarter> vpvhQuarters)
        {
            const string amNewsTabName = "AM News";
            const string pmNewsTabName = "PM News";
            const string synAllTabName = "Syn All";
            const string tdnTabName = "TDN";
            const string tdnsTabName = "TDNS";
            const string audienceHeader = "Audience";


            var quarters = vpvhQuarters.Select(v => new { v.Quarter, v.Year }).Distinct().OrderByDescending(v => v.Year).ThenBy(v => v.Quarter).ToList();
            var audiences = vpvhQuarters.Select(v => v.Audience.AudienceString).Distinct().OrderBy(v => v).ToList();

            var excelPackage = new ExcelPackage();
            var amNewsTab = excelPackage.Workbook.Worksheets.Add(amNewsTabName);
            var pmNewsTab = excelPackage.Workbook.Worksheets.Add(pmNewsTabName);
            var synAllTab = excelPackage.Workbook.Worksheets.Add(synAllTabName);
            var tdnTab = excelPackage.Workbook.Worksheets.Add(tdnTabName);
            var tdnsTab = excelPackage.Workbook.Worksheets.Add(tdnsTabName);

            var columnIndex = 1;
            var rowIndex = 1;
            _SetCellValue(amNewsTab, rowIndex, columnIndex, audienceHeader, true);
            _SetCellValue(pmNewsTab, rowIndex, columnIndex, audienceHeader, true);
            _SetCellValue(synAllTab, rowIndex, columnIndex, audienceHeader, true);
            _SetCellValue(tdnTab, rowIndex, columnIndex, audienceHeader, true);
            _SetCellValue(tdnsTab, rowIndex, columnIndex, audienceHeader, true);

            for (var audienceIndex = 0; audienceIndex < audiences.Count; audienceIndex++)
            {
                rowIndex++;

                var audience = audiences[audienceIndex];

                _SetCellValue(amNewsTab, rowIndex, columnIndex, audience);
                _SetCellValue(pmNewsTab, rowIndex, columnIndex, audience);
                _SetCellValue(synAllTab, rowIndex, columnIndex, audience);
                _SetCellValue(tdnTab, rowIndex, columnIndex, audience);
                _SetCellValue(tdnsTab, rowIndex, columnIndex, audience);
            }

            columnIndex++;
            for (var quarterIndex = 0; quarterIndex < quarters.Count; quarterIndex++)
            {
                rowIndex = 1;

                var quarter = $"{quarters[quarterIndex].Quarter}Q{quarters[quarterIndex].Year}";

                _SetCellValue(amNewsTab, rowIndex, columnIndex, quarter, true);
                _SetCellValue(pmNewsTab, rowIndex, columnIndex, quarter, true);
                _SetCellValue(synAllTab, rowIndex, columnIndex, quarter, true);
                _SetCellValue(tdnTab, rowIndex, columnIndex, quarter, true);
                _SetCellValue(tdnsTab, rowIndex, columnIndex, quarter, true);

                foreach(var audience in audiences)
                {
                    rowIndex++;

                    var vpvhQuarter = vpvhQuarters.FirstOrDefault(v => v.Quarter == quarters[quarterIndex].Quarter && v.Year == quarters[quarterIndex].Year && v.Audience.AudienceString == audience);

                    _SetCellValue(amNewsTab, rowIndex, columnIndex, vpvhQuarter.AMNews);
                    _SetCellValue(pmNewsTab, rowIndex, columnIndex, vpvhQuarter.PMNews);
                    _SetCellValue(synAllTab, rowIndex, columnIndex, vpvhQuarter.SynAll);
                    _SetCellValue(tdnTab, rowIndex, columnIndex, vpvhQuarter.Tdn);
                    _SetCellValue(tdnsTab, rowIndex, columnIndex, vpvhQuarter.Tdns);
                }

                columnIndex++;
            }

            return excelPackage;
        }

        private void _SetCellValue(ExcelWorksheet excelWorksheet, int row, int column, string value, bool bold = false)
        {
            excelWorksheet.Cells[row, column].Value = value;
            if (bold)
                excelWorksheet.Cells[row, column].Style.Font.Bold = true;
        }

        private void _SetCellValue(ExcelWorksheet excelWorksheet, int row, int column, double value)
        {
            excelWorksheet.Cells[row, column].Value = value;
            excelWorksheet.Cells[row, column].Style.Numberformat.Format = "#0.000";
        }
    }
}
