﻿using OfficeOpenXml;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping.Entities;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.ProgramMapping
{
    public class UnmappedProgramsReportGenerator : IReportGenerator<UnmappedProgramReportData>
    {
        private const string CELL_A1_HEADER = "Rate Card Program Name";
        private const string CELL_B1_HEADER = "Matched Program Name";
        private const string CELL_C1_HEADER = "Matched Program Genre";
        private const string CELL_E1_HEADER = "Confidence %";
        private const string TAB_NAME = "Unmapped Programs";
        private const int DATA_START_COLUMN = 1;
        private const int DATA_START_ROW = 2;
        
       

        public ReportOutput Generate(UnmappedProgramReportData dataObject)
        {
            var output = new ReportOutput(dataObject.ExportFileName);

            var package = _GetFileWithData(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }
        /// <summary>
        /// Generate the unmapped programs file 
        /// </summary>        
        public ReportOutput GenerateWithoutGenre(UnmappedProgramReportData dataObject)
        {
            var output = new ReportOutput(dataObject.ExportFileName);
            var package = _GetFileWithDataExcludesGenre(dataObject);
            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;
            return output;
        }

        private ExcelPackage _GetFileWithData(UnmappedProgramReportData dataObject)
        {
            var excelPackage = new ExcelPackage();
            var tab = excelPackage.Workbook.Worksheets.Add(TAB_NAME);
            _PopulateHeaders(tab);
            _PopulateData(tab, dataObject.UnmappedPrograms);
            return excelPackage;
        }
        private ExcelPackage _GetFileWithDataExcludesGenre(UnmappedProgramReportData dataObject)
        {
            var excelPackage = new ExcelPackage();
            var tab = excelPackage.Workbook.Worksheets.Add(TAB_NAME);
            _PopulateHeadersWithoutGenre(tab);
            _PopulateDataWithoutGenre(tab, dataObject.UnmappedPrograms);
            return excelPackage;
        }
      
        private void _PopulateHeaders(ExcelWorksheet worksheet)
        {
            worksheet.Cells["A1"].Value = CELL_A1_HEADER;
            worksheet.Column(1).Width = 50;
            worksheet.Cells["A1"].Style.Font.Bold = true;

            worksheet.Cells["B1"].Value = CELL_B1_HEADER;
            worksheet.Column(2).Width = 50;
            worksheet.Cells["B1"].Style.Font.Bold = true;

            worksheet.Cells["C1"].Value = CELL_C1_HEADER;
            worksheet.Column(3).Width = 50;
            worksheet.Cells["C1"].Style.Font.Bold = true;

            worksheet.Cells["E1"].Value = CELL_E1_HEADER;
            worksheet.Column(3).Width = 50;
            worksheet.Cells["E1"].Style.Font.Bold = true;
        }

        private void _PopulateData(ExcelWorksheet worksheet, List<UnmappedProgram> programs)
        {
            var tableData = programs.OrderByDescending(p => p.MatchConfidence).Select(p => new object[] { p.OriginalName, p.MatchedName, p.Genre, string.Empty, _GetMatchConfidenceString(p.MatchConfidence)});
            worksheet.Cells[DATA_START_ROW, DATA_START_COLUMN].LoadFromArrays(tableData);
        }
        private void _PopulateHeadersWithoutGenre(ExcelWorksheet worksheet)
        {
            worksheet.Cells["A1"].Value = CELL_A1_HEADER;
            worksheet.Column(1).Width = 50;
            worksheet.Cells["A1"].Style.Font.Bold = true;

            worksheet.Cells["B1"].Value = CELL_B1_HEADER;
            worksheet.Column(2).Width = 50;
            worksheet.Cells["B1"].Style.Font.Bold = true;

            worksheet.Cells["C1"].Value = CELL_E1_HEADER;
            worksheet.Column(3).Width = 50;
            worksheet.Cells["C1"].Style.Font.Bold = true;
           
        }
        private void _PopulateDataWithoutGenre(ExcelWorksheet worksheet, List<UnmappedProgram> programs)
        {
            var tableData = programs.OrderByDescending(p => p.MatchConfidence).Select(p => new object[] { p.OriginalName, 
                p.MatchedName,_GetMatchConfidenceString(p.MatchConfidence) });
            worksheet.Cells[DATA_START_ROW, DATA_START_COLUMN].LoadFromArrays(tableData);
        }

        private string _GetMatchConfidenceString(float confidence)
        {
            const double toPercent = 100.0;
            const int decimalPoints = 2;

            var val = Math.Round((confidence * toPercent), decimalPoints);
            var stringVal = $"{val}%";
            return stringVal;
        }
    }
}
