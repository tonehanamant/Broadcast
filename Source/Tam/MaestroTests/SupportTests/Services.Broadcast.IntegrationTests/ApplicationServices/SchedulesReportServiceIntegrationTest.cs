using System;
using NUnit.Framework;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.ApplicationServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EntityFrameworkMapping.Broadcast;
using Moq;
using Services.Broadcast.Aggregates;
using Services.Broadcast.Entities;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class SchedulesReportServiceIntegrationTest
    {
        readonly ISchedulesReportService _Sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();

        [Test]
        public void ClientReport_RelatedSchedules_UsePostType_OfOriginalSchedule()
        {
            var aggregate = new SchedulesAggregate(new schedule { markets = new List<market>() }, new List<ScheduleAudience>(), null, null, null, null, new List<bvs_file_details>(), null, null,
                SchedulePostType.NSI, RatesFile.RateSourceType.Assembly, true, DateTime.MaxValue, DateTime.MaxValue,null);

            var relatedAggregate = new SchedulesAggregate(new schedule(), null, null, null, null, null, new List<bvs_file_details>(), null, null,
                SchedulePostType.NTI, RatesFile.RateSourceType.Assembly, true, DateTime.MaxValue, DateTime.MaxValue, null);

            var scheduleReportDto = new ScheduleReportDto { ScheduleId = 420 };

            var dtoFactoryService = new Mock<IScheduleReportDtoFactoryService>();
            dtoFactoryService.Setup(s => s.GenereteScheduleReportData(It.Is<SchedulesAggregate>(sa => sa.PostType == aggregate.PostType), ScheduleReportType.Client)).Returns(scheduleReportDto);

            var scheduleFactoryService = new Mock<IScheduleAggregateFactoryService>();
            scheduleFactoryService.Setup(s => s.GetScheduleAggregate(scheduleReportDto.ScheduleId)).Returns(relatedAggregate);

            var sut = new SchedulesReportService(dtoFactoryService.Object, scheduleFactoryService.Object, null, null, new StubbedSMSClient());

            var relatedScheduleIds = new List<int> { scheduleReportDto.ScheduleId };
            var ret = sut._GetListOfRelatedScheduleAggregates(relatedScheduleIds, aggregate, new List<string>());

            Assert.That(ret.Single(), Is.EqualTo(scheduleReportDto));
        }

        [Test]
        [Ignore]
        public void GenerateReportFile()
        {
            const int scheduleId = 975;
            var report = _Sut.GenerateScheduleReport(scheduleId);
            File.WriteAllBytes(@"\\tsclient\cadent\" + @"bvsreport" + scheduleId + ".xlsx", report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
//            File.WriteAllBytes(string.Format("..\\bvsreport{0}.xlsx", scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            Assert.IsNotNull(report.Stream);
        }

        [Test]
        [Ignore]
        public void Generate3rdPartReportFile()
        {
            const int scheduleId = 2365;
            var report = _Sut.GenerateClientReport(scheduleId);
            File.WriteAllBytes(@"\\tsclient\cadent\" + @"3rdPartyClientReport" + scheduleId + ".xlsx", report.Stream.GetBuffer());
            Assert.IsNotNull(report.Stream);
        }

        [Test]
        public void GenerateTestReport()
        {            
            var reportStream = new MemoryStream();
            var inSpecData = new List<BvsTestData>
                {
                    new BvsTestData{ Rank = 59, Market = "ALBANY,NY", Station = "WNYT", Affiliate = "CBS", ProgramName = "Milioanaire", DisplayDaypart = null, Cost = 50, OrderedSpots = 2, DeliveredSpots = 1, OrderedImpressions = 0, DeliveredImpressions = 100 },
                    new BvsTestData{ Rank = 59, Market = "ALBANY,NY", Station = "WNYT", Affiliate = "CBS", ProgramName = "Milioanaire", DisplayDaypart = null, Cost = 100, OrderedSpots = 4, DeliveredSpots = 2, OrderedImpressions = 500, DeliveredImpressions = 150 }
                };

            var outOfSpecData = new List<BvsTestData>
                {
                    new BvsTestData{ Rank = 3, Market = "Chicago", Station = "WBBM-TV", Affiliate = "CBS", ProgramName = "Milioanaire", DisplayDaypart = null, Cost = null, OrderedSpots = null, DeliveredSpots = null, OrderedImpressions = null, DeliveredImpressions = 200 },
                    new BvsTestData{ Rank = 1, Market = "New York", Station = "WCBS-TV", Affiliate = "CBS", ProgramName = "Milioanaire", DisplayDaypart = null, Cost = null, OrderedSpots = null, DeliveredSpots = null, OrderedImpressions = null, DeliveredImpressions = 250 }
                };
            var reportData = inSpecData.Concat(outOfSpecData).ToList();

            //var reportFileInfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\BVS Report.xlsx");
            using (var package = new ExcelPackage(new MemoryStream()))
            {
                if (package.Workbook.Worksheets["test"] != null)
                    package.Workbook.Worksheets.Delete("test");

                var ws = package.Workbook.Worksheets.Add("test");
                ws.Row(1).Height = 25.5;
                ws.View.ShowGridLines = true;

                int rowOffset = 1;
                int columnOffset = 1;

                ws.Cells[rowOffset, columnOffset++].Value = "Rank";
                ws.Cells[rowOffset, columnOffset++].Value = "Market";
                ws.Cells[rowOffset, columnOffset++].Value = "Station";
                ws.Cells[rowOffset, columnOffset++].Value = "Affiliate";
                ws.Cells[rowOffset, columnOffset++].Value = "Program";
                ws.Cells[rowOffset, columnOffset++].Value = "Daypart";
                ws.Cells[rowOffset, columnOffset++].Value = "Cost";
                ws.Cells[rowOffset, columnOffset++].Value = "Ordered Spots";
                ws.Cells[rowOffset, columnOffset++].Value = "Delivered Spots";
                ws.Cells[rowOffset, columnOffset++].Value = "Ordered Impressions";
                ws.Cells[rowOffset, columnOffset].Value = "Delivered Impressions";

                ws.Cells["A1:K1"].Style.Font.Bold = true;
                ws.Cells["A1:K1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells["G2:G" + (reportData.Count + 1)].Style.Numberformat.Format = "$0.00";

                ws.Cells[2, 1].LoadFromCollection(reportData);

                var totalOrderedSpots = reportData.Sum(x => x.OrderedSpots);
                var totalDeliveredSpots = reportData.Sum(x => x.DeliveredSpots);

                ws.Cells[6, 8].Value = totalOrderedSpots;
                ws.Cells[6, 9].Value = totalDeliveredSpots;

                var totalOrderedImpressions = reportData.Sum(x => x.OrderedImpressions);
                var totalDeliveredImpressions = reportData.Sum(x => x.DeliveredImpressions);
                ws.Cells[6, 10].Value = totalOrderedImpressions;
                ws.Cells[6, 11].Value = totalDeliveredImpressions;

                ws.Cells.AutoFitColumns();
                package.SaveAs(reportStream);
            }

            Assert.IsNotNull(reportStream);
            Assert.IsTrue(reportStream.Position > 0);
            //File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"\BVS Report.xlsx", reportStream.GetBuffer());
        }

        private class BvsTestData
        {
            public int Rank { get; set; }
            public string Market { get; set; }
            public string Station { get; set; }
            public string Affiliate { get; set; }
            public string ProgramName { get; set; }
            public DisplayDaypart DisplayDaypart { get; set; }
            public double? Cost { get; set; }
            public int? OrderedSpots { get; set; }
            public int? DeliveredSpots { get; set; }
            public int? OrderedImpressions { get; set; }
            public double? DeliveredImpressions { get; set; }
        }
    }
}
