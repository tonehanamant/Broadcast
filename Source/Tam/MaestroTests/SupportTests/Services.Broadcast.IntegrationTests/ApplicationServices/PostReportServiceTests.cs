using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PostReportServiceTests
    {
        private readonly IPostReportService _PostReportService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostReportService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetNsiPostReportData()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetNsiPostReportData(253);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(NsiPostReport), "ProposalId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetNsiPostReportDataWithHiatusWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetNsiPostReportData(25999);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(NsiPostReport), "ProposalId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetNsiPostReportDataWithImpressionsForEachWeek()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetNsiPostReportData(26000);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(NsiPostReport), "ProposalId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetNsiPostReportDataWithOvernightImpressions()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetNsiPostReportData(253, true);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(NsiPostReport), "ProposalId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        public void GenerateReportFile()
        {
            const int proposalId = 253;
            var report = _PostReportService.GenerateNSIPostReport(proposalId);
            File.WriteAllBytes(@"\\tsclient\cadent\" + @"NSIPostReport" + proposalId + ".xlsx", report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
                                                                                                                           //            File.WriteAllBytes(string.Format("..\\bvsreport{0}.xlsx", scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            Assert.IsNotNull(report.Stream);
        }

        [Test]
        public void GenerateMyEventsReportTest()
        {
            var expectedFileName = "Test Adve NAV 30 05-30-16.txt";
            var expectedFilePath = @".\Files\" + expectedFileName;
            var myEventsReport = _PostReportService.GenerateMyEventsReport(25999);
            var tempPath = Path.GetTempFileName();

            File.WriteAllBytes(tempPath, myEventsReport.Stream.ToArray());

            using (var zip = new ZipArchive(myEventsReport.Stream, ZipArchiveMode.Read))
            {
                var firstEntry = zip.GetEntry(expectedFileName);
                using (var entry = firstEntry.Open())
                using (var ms = new MemoryStream())
                {
                    entry.CopyTo(ms);
                    ms.Position = 0;
                    FileAssert.AreEqual(File.OpenRead(expectedFilePath), ms);
                }
                Assert.AreEqual(expectedFileName, firstEntry.FullName);
            }
        }

        [Test]
        public void GenerateMultipleMyEventsReportsTest()
        {
            const int expectedNumberOfEntries = 2;
            var expectedReportNames = new List<string>()
            {
                "Test Adv NAV3 30 05-30-16.txt",
                "Test Adv NAV4 30 05-30-16.txt"
            };
            var expectedFilePath = new List<string>()
            {
                @".\Files\" + expectedReportNames[0],
                @".\Files\" + expectedReportNames[1],
            };
            var myEventsReport = _PostReportService.GenerateMyEventsReport(26001);

            using (var zip = new ZipArchive(myEventsReport.Stream, ZipArchiveMode.Read))
            {
                Assert.AreEqual(expectedNumberOfEntries, zip.Entries.Count);
                var firstEntry = zip.GetEntry(expectedReportNames[0]);
                using (var entry = firstEntry.Open())
                using (var ms = new MemoryStream())
                {
                    entry.CopyTo(ms);
                    ms.Position = 0;
                    FileAssert.AreEqual(File.OpenRead(expectedFilePath[0]), ms);
                }
                Assert.AreEqual(expectedReportNames[0], firstEntry.FullName);
                var secondEntry = zip.GetEntry(expectedReportNames[1]);
                using (var entry = secondEntry.Open())
                using (var ms = new MemoryStream())
                {
                    entry.CopyTo(ms);
                    ms.Position = 0;
                    FileAssert.AreEqual(File.OpenRead(expectedFilePath[1]), ms);
                }
                Assert.AreEqual(expectedReportNames[1], secondEntry.FullName);
            }
        }

        [Test]
        public void GenerateMyEventsReportWithAdjustedTimeWindowTest()
        {
            var myEventsReportData = _PostReportService.GetMyEventsReportData(25999);
            var firstExpectedDate = new DateTime(1, 1, 1, 9, 0, 0);
            var secondExpectedDate = new DateTime(1, 1, 1, 9, 3, 0);

            Assert.AreEqual(firstExpectedDate, myEventsReportData[0].Lines[0].LineupStartTime);
            Assert.AreEqual(secondExpectedDate, myEventsReportData[0].Lines[1].LineupStartTime);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReportWithAdjustedTimeWindowMultipleTest()
        {
            var result = _PostReportService.GetMyEventsReportData(26002);
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }
    }
}
