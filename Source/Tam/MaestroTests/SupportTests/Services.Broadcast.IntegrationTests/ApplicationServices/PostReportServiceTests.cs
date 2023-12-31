﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class PostReportServiceTests
    {
        private readonly IPostReportService _PostReportService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostReportService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetNsiPostReportData()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetPostReportData(253);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PostReport), "ProposalId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Ignore("Not certain why we are ignoring this...")]
        [UseReporter(typeof(DiffReporter))]
        public void GetNsiPostReportDataWithHiatusWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetPostReportData(25999);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PostReport), "ProposalId");

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
                var result = _PostReportService.GetPostReportData(26000);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PostReport), "ProposalId");

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
                var result = _PostReportService.GetPostReportData(253, withOvernightImpressions: true);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PostReport), "ProposalId");

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
        public void GetNsiPostReportDataWithAduAdjustments()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetPostReportData(26009);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PostReport), "ProposalId");

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
        public void GetNsiPostReportDataWithAduNoAdjustments()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetPostReportData(26010);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PostReport), "ProposalId");

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
        public void GetNsiPostReportDataWithProposalNotes()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetPostReportData(249, withOvernightImpressions: true);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PostReport), "ProposalId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [Ignore("Not certain why we are ignoring this...")]
        public void GenerateReportFile()
        {
            const int proposalId = 253;
            var report = _PostReportService.GeneratePostReport(proposalId);
            File.WriteAllBytes(@"\\tsclient\cadent\" + @"NSIPostReport" + proposalId + ".xlsx", report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
                                                                                                                           //            File.WriteAllBytes(string.Format("..\\bvsreport{0}.xlsx", scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            Assert.IsNotNull(report.Stream);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReportTest()
        {
            var expectedFileName = "STAPLES EMN 15 8-27-18.txt";
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
        [UseReporter(typeof(DiffReporter))]
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReportWithMultipleStationsTest()
        {
            var result = _PostReportService.GetMyEventsReportData(26003);
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReportWithMyEventsReportName()
        {
            var result = _PostReportService.GetMyEventsReportData(26007);
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReportWithMyEventsReportNameAndNullReportName()
        {
            var result = _PostReportService.GetMyEventsReportData(26008);
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReportForProposalWithSameIsciInTwoWeeksInSameDetail()
        {
            var result = _PostReportService.GetMyEventsReportData(3132);
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateNsiReportWithStationWithoutMarketRank()
        {
            var result = _PostReportService.GetPostReportData(3134);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReportWithSorting()
        {
            var result = _PostReportService.GetMyEventsReportData(26013);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReportWithOverriddenStatusAndNonMatchingWeek()
        {
            var result = _PostReportService.GetMyEventsReportData(26014);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReport_SimplifiedStationName()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetMyEventsReportData(253);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateMyEventsReport_UpdateTimeToNationalClock()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetMyEventsReportData(253);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetNsiPostReportDataWithEquivalizedImpressions()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetPostReportData(26015);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetNtiPostReportData()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetPostReportData(26006, isNtiReport: true);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PostReport), "ProposalId");

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
        public void GetNtiPostReportData_PRI711()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostReportService.GetPostReportData(25999, isNtiReport: true);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(PostReport), "ProposalId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }
    }
}
