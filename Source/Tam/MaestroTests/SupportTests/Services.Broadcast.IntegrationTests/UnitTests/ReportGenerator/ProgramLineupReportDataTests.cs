using NUnit.Framework;
using Services.Broadcast.Entities.Campaign;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.ReportGenerator
{
    [TestFixture]
    public class ProgramLineupReportDataTests
    {
        [Test]
        [TestCase("Contacts 2020", "Program_Lineup_Report_Contacts 2020_10012020.xlsx")]
        [TestCase("Q4'21 TDN SYN Pricing V3 :15 :30", "Program_Lineup_Report_Q4'21 TDN SYN Pricing V3 15 30_10012020.xlsx")]
        public void GetFileName(string planName, string expectedFileName)
        {
            var testDate = new DateTime(2020, 10, 1);
            var reportData = new ProgramLineupReportData();

            var result = reportData._GetFileName(planName, testDate);

            Assert.AreEqual(expectedFileName, result);
        }
    }
}