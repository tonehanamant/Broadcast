using NUnit.Framework;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.ReportGenerator
{
    [TestFixture]
    public class ProgramLineupReportDataTests
    {
        [Test]
        [TestCase("Contacts 2020", PostingTypeEnum.NSI, SpotAllocationModelMode.Quality, "Program_Lineup_Report_Contacts 2020_NSI_Q_10012020.xlsx")]
        [TestCase("Contacts 2020", PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, "Program_Lineup_Report_Contacts 2020_NSI_E_10012020.xlsx")]
        [TestCase("Contacts 2020", PostingTypeEnum.NSI, SpotAllocationModelMode.Floor, "Program_Lineup_Report_Contacts 2020_NSI_F_10012020.xlsx")]
        [TestCase("Contacts 2020", PostingTypeEnum.NTI, SpotAllocationModelMode.Quality, "Program_Lineup_Report_Contacts 2020_NTI_Q_10012020.xlsx")]
        [TestCase("Contacts 2020", PostingTypeEnum.NTI, SpotAllocationModelMode.Efficiency, "Program_Lineup_Report_Contacts 2020_NTI_E_10012020.xlsx")]
        [TestCase("Contacts 2020", PostingTypeEnum.NTI, SpotAllocationModelMode.Floor, "Program_Lineup_Report_Contacts 2020_NTI_F_10012020.xlsx")]
        [TestCase("Q4'21 TDN SYN Pricing V3 :15 :30", PostingTypeEnum.NSI, SpotAllocationModelMode.Quality, "Program_Lineup_Report_Q4'21 TDN SYN Pricing V3 15 30_NSI_Q_10012020.xlsx")]
        [TestCase("Q4'21 TDN SYN Pricing V3 :15 :30", PostingTypeEnum.NTI, SpotAllocationModelMode.Quality, "Program_Lineup_Report_Q4'21 TDN SYN Pricing V3 15 30_NTI_Q_10012020.xlsx")]
        [TestCase("Q4'21 TDN SYN Pricing V3 :15 :30", PostingTypeEnum.NSI, SpotAllocationModelMode.Efficiency, "Program_Lineup_Report_Q4'21 TDN SYN Pricing V3 15 30_NSI_E_10012020.xlsx")]
        [TestCase("Q4'21 TDN SYN Pricing V3 :15 :30", PostingTypeEnum.NTI, SpotAllocationModelMode.Efficiency, "Program_Lineup_Report_Q4'21 TDN SYN Pricing V3 15 30_NTI_E_10012020.xlsx")]
        [TestCase("Q4'21 TDN SYN Pricing V3 :15 :30", PostingTypeEnum.NSI, SpotAllocationModelMode.Floor, "Program_Lineup_Report_Q4'21 TDN SYN Pricing V3 15 30_NSI_F_10012020.xlsx")]
        [TestCase("Q4'21 TDN SYN Pricing V3 :15 :30", PostingTypeEnum.NTI, SpotAllocationModelMode.Floor, "Program_Lineup_Report_Q4'21 TDN SYN Pricing V3 15 30_NTI_F_10012020.xlsx")]
        public void GetFileName(string planName,PostingTypeEnum postingType,SpotAllocationModelMode spotAllocationModelMode,string expectedFileName)
        {
            var testDate = new DateTime(2020, 10, 1);
            var reportData = new ProgramLineupReportData();

            var result = reportData._GetFileName(planName,postingType,spotAllocationModelMode, testDate);

            Assert.AreEqual(expectedFileName, result);
        }
    }
}