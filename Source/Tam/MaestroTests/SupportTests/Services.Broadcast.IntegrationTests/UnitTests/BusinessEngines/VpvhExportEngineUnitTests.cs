using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Vpvh;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    public class VpvhExportEngineUnitTests
    {
        private IVpvhExportEngine _VpvhExportEngine;

        [SetUp]
        public void SetUp()
        {
            _VpvhExportEngine = new VpvhExportEngine();
        }

        [Test]
        public void ExportQuarters_GenerateFiveTabs()
        {
            var excelFile = _VpvhExportEngine.ExportQuarters(_GetVpvhQuarters());

            Assert.AreEqual(5, excelFile.Workbook.Worksheets.Count);
            Assert.AreEqual("AM News", excelFile.Workbook.Worksheets[1].Name);
            Assert.AreEqual("PM News", excelFile.Workbook.Worksheets[2].Name);
            Assert.AreEqual("Syn All", excelFile.Workbook.Worksheets[3].Name);
            Assert.AreEqual("TDN", excelFile.Workbook.Worksheets[4].Name);
            Assert.AreEqual("TDNS", excelFile.Workbook.Worksheets[5].Name);
        }

        [Test]
        public void ExportQuarters_GenerateRowForEveryAudience()
        {
            var excelFile = _VpvhExportEngine.ExportQuarters(_GetVpvhQuarters());

            for (var i = 1; i <= 5; i++)
            {
                Assert.AreEqual("W18+", excelFile.Workbook.Worksheets[i].Cells[2, 1].Value);
                Assert.AreEqual("W18-24", excelFile.Workbook.Worksheets[i].Cells[3, 1].Value);
                Assert.AreEqual("W55+", excelFile.Workbook.Worksheets[i].Cells[4, 1].Value);
            }
        }

        [Test]
        public void ExportQuarters_GenerateColumnForEveryQuarter()
        {
            var excelFile = _VpvhExportEngine.ExportQuarters(_GetVpvhQuarters());

            for (var i = 1; i <= 5; i++)
            {
                Assert.AreEqual("1Q19", excelFile.Workbook.Worksheets[i].Cells[1, 2].Value);
                Assert.AreEqual("2Q19", excelFile.Workbook.Worksheets[i].Cells[1, 3].Value);
            }
        }

        [Test]
        public void ExportQuarters_AmNewsValues()
        {
            var excelFile = _VpvhExportEngine.ExportQuarters(_GetVpvhQuarters());

            Assert.AreEqual(0.351, excelFile.Workbook.Worksheets[1].Cells[2, 2].Value);
            Assert.AreEqual(0.353, excelFile.Workbook.Worksheets[1].Cells[3, 2].Value);
            Assert.AreEqual(0.352, excelFile.Workbook.Worksheets[1].Cells[4, 2].Value);
            Assert.AreEqual(0.354, excelFile.Workbook.Worksheets[1].Cells[2, 3].Value);
            Assert.AreEqual(0.356, excelFile.Workbook.Worksheets[1].Cells[3, 3].Value);
            Assert.AreEqual(0.355, excelFile.Workbook.Worksheets[1].Cells[4, 3].Value);
        }

        [Test]
        public void ExportQuarters_PmNewsValues()
        {
            var excelFile = _VpvhExportEngine.ExportQuarters(_GetVpvhQuarters());

            Assert.AreEqual(0.684, excelFile.Workbook.Worksheets[2].Cells[2, 2].Value);
            Assert.AreEqual(0.984, excelFile.Workbook.Worksheets[2].Cells[3, 2].Value);
            Assert.AreEqual(0.484, excelFile.Workbook.Worksheets[2].Cells[4, 2].Value);
            Assert.AreEqual(0.684, excelFile.Workbook.Worksheets[2].Cells[2, 3].Value);
            Assert.AreEqual(0.984, excelFile.Workbook.Worksheets[2].Cells[3, 3].Value);
            Assert.AreEqual(0.484, excelFile.Workbook.Worksheets[2].Cells[4, 3].Value);
        }

        [Test]
        public void ExportQuarters_SynAllValues()
        {
            var excelFile = _VpvhExportEngine.ExportQuarters(_GetVpvhQuarters());

            Assert.AreEqual(0.298, excelFile.Workbook.Worksheets[3].Cells[2, 2].Value);
            Assert.AreEqual(0.798, excelFile.Workbook.Worksheets[3].Cells[3, 2].Value);
            Assert.AreEqual(0.198, excelFile.Workbook.Worksheets[3].Cells[4, 2].Value);
            Assert.AreEqual(0.298, excelFile.Workbook.Worksheets[3].Cells[2, 3].Value);
            Assert.AreEqual(0.798, excelFile.Workbook.Worksheets[3].Cells[3, 3].Value);
            Assert.AreEqual(0.198, excelFile.Workbook.Worksheets[3].Cells[4, 3].Value);
        }

        [Test]
        public void ExportQuarters_TdnValues()
        {
            var excelFile = _VpvhExportEngine.ExportQuarters(_GetVpvhQuarters());

            Assert.AreEqual(2, excelFile.Workbook.Worksheets[4].Cells[2, 2].Value);
            Assert.AreEqual(2.2, excelFile.Workbook.Worksheets[4].Cells[3, 2].Value);
            Assert.AreEqual(2.5, excelFile.Workbook.Worksheets[4].Cells[4, 2].Value);
            Assert.AreEqual(2.6, excelFile.Workbook.Worksheets[4].Cells[2, 3].Value);
            Assert.AreEqual(2.4, excelFile.Workbook.Worksheets[4].Cells[3, 3].Value);
            Assert.AreEqual(2.8, excelFile.Workbook.Worksheets[4].Cells[4, 3].Value);
        }

        [Test]
        public void ExportQuarters_TdnsValues()
        {
            var excelFile = _VpvhExportEngine.ExportQuarters(_GetVpvhQuarters());

            Assert.AreEqual(3.1, excelFile.Workbook.Worksheets[5].Cells[2, 2].Value);
            Assert.AreEqual(3.3, excelFile.Workbook.Worksheets[5].Cells[3, 2].Value);
            Assert.AreEqual(3.2, excelFile.Workbook.Worksheets[5].Cells[4, 2].Value);
            Assert.AreEqual(3.4, excelFile.Workbook.Worksheets[5].Cells[2, 3].Value);
            Assert.AreEqual(3.6, excelFile.Workbook.Worksheets[5].Cells[3, 3].Value);
            Assert.AreEqual(3.5, excelFile.Workbook.Worksheets[5].Cells[4, 3].Value);
        }

        private List<VpvhQuarter> _GetVpvhQuarters() =>
            new List<VpvhQuarter>
            {
                new VpvhQuarter{ AMNews = 0.351, Audience=new DisplayAudience(1, "W18+"),   PMNews = 0.684, SynAll = 0.298, Quarter = 1, Year = 2019, Tdn = 2, Tdns = 3.1},
                new VpvhQuarter{ AMNews = 0.352, Audience=new DisplayAudience(2, "W55+"),   PMNews = 0.484, SynAll = 0.198, Quarter = 1, Year = 2019, Tdn = 2.5, Tdns = 3.2},
                new VpvhQuarter{ AMNews = 0.353, Audience=new DisplayAudience(3, "W18-24"), PMNews = 0.984, SynAll = 0.798, Quarter = 1, Year = 2019, Tdn = 2.2, Tdns = 3.3},
                new VpvhQuarter{ AMNews = 0.354, Audience=new DisplayAudience(1, "W18+"),   PMNews = 0.684, SynAll = 0.298, Quarter = 2, Year = 2019, Tdn = 2.6, Tdns = 3.4},
                new VpvhQuarter{ AMNews = 0.355, Audience=new DisplayAudience(2, "W55+"),   PMNews = 0.484, SynAll = 0.198, Quarter = 2, Year = 2019, Tdn = 2.8, Tdns = 3.5},
                new VpvhQuarter{ AMNews = 0.356, Audience=new DisplayAudience(3, "W18-24"), PMNews = 0.984, SynAll = 0.798, Quarter = 2, Year = 2019, Tdn = 2.4, Tdns = 3.6}
            };
    }
}
