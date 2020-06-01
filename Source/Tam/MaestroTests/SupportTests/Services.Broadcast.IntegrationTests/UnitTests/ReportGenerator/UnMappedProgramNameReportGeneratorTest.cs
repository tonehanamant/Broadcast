using System.Collections.Generic;
using NUnit.Framework;
using Services.Broadcast.Entities.ProgramMapping;

namespace Services.Broadcast.IntegrationTests.UnitTests.ReportGenerator
{
	[TestFixture]
	public class UnMappedProgramNameReportGeneratorTest
	{
		[Test]
		public void GetFileWithDataTest()
		{
			var data = new UnMappedProgramNameReportData();
			var expectedData = new List<string>(){"test1", "test2"};
			
			data.ProgramNames = expectedData;

			var testClass = new UnMappedProgramNameReportGeneratorTestClass();
			/***Act***/
			var result = testClass.UT_GetFileWithData(data);
			/***Assert***/
			StringAssert.AreEqualIgnoringCase("UnMappedPrograms", result.Workbook.Worksheets[1].Name);
			StringAssert.AreEqualIgnoringCase("Rate Card program Name",
				result.Workbook.Worksheets[1].Cells["A1"].Value.ToString());
			Assert.AreEqual(1, result.Workbook.Worksheets[1].Dimension.End.Column);
			Assert.AreEqual(3, result.Workbook.Worksheets[1].Dimension.End.Row);
		}
	}
}