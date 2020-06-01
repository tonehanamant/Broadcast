using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProgramMapping;

namespace Services.Broadcast.ReportGenerators.ProgramMapping
{
	public class UnMappedProgramNameReportGenerator : IReportGenerator<UnMappedProgramNameReportData>
	{
		
		public const string UNMAPPED_PRGMS_WS = "UnMappedPrograms";
		public const string CELL_A1_Header = "Rate Card program Name";
		public ReportOutput Generate(UnMappedProgramNameReportData dataObject)
		{
			var output = new ReportOutput(dataObject.ExportFileName);

			var package = _GetFileWithData(dataObject);

			package.SaveAs(output.Stream);
			package.Dispose();
			output.Stream.Position = 0;

			return output;
		}

		protected ExcelPackage _GetFileWithData(UnMappedProgramNameReportData reportData)
		{
			var excelPackage = new ExcelPackage();
			var excelInventoryTab = excelPackage.Workbook.Worksheets.Add(UNMAPPED_PRGMS_WS);

			PopulateTab(excelInventoryTab, reportData.ProgramNames);


			return excelPackage;
		}

		private void PopulateTab(ExcelWorksheet worksheet, List<string> reportData)
		{
			_PopulateHeaders(worksheet);

			_PopulateTable(worksheet, reportData);
		}

		private void _PopulateHeaders(ExcelWorksheet worksheet)
		{
			worksheet.Cells["A1"].Value = CELL_A1_Header;
			worksheet.Column(1).Width = 50;
			worksheet.Cells["A1"].Style.Font.Bold = true;
		}

		private void _PopulateTable(ExcelWorksheet worksheet, List<string> reportData)
		{
			var tableData = reportData
				.Select(x => new object[]
				{
					x
				})
				.ToList();

			worksheet.Cells[2, 1].LoadFromArrays(tableData);
		}
	}
}