using OfficeOpenXml;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.ReportGenerators.ProgramMapping;

namespace Services.Broadcast.IntegrationTests.UnitTests.ReportGenerator
{
	public class UnMappedProgramNameReportGeneratorTestClass : UnMappedProgramNameReportGenerator
	{
		public ExcelPackage UT_GetFileWithData(UnMappedProgramNameReportData reportData)
		{
			return _GetFileWithData(reportData);
		}
	}
}