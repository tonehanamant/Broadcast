using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.IO;
using System.Linq;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class AffidavitScrubbingServiceIntegrationTests
    {
        private readonly IAffidavitScrubbingService _AffidavitScrubbingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>();
        private readonly IAffidavitRepository _AffidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();

        [Test]
        public void GetPostsTest()
        {
            var result = _AffidavitScrubbingService.GetPosts();

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PostDto), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetClientScrubbingForProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitScrubbingService.GetClientScrubbingForProposal(253);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");

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
        public void GetClientScrubbingForProposal_BadMarket()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalId = 253;
                var aff = _AffidavitRepository.GetAffidavit(157);
                aff.AffidavitFileDetails[0].Station = "bad station";
                _AffidavitRepository.SaveAffidavitFile(aff);

                var result = _AffidavitScrubbingService.GetClientScrubbingForProposal(proposalId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");

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
        public void GetUnlinkedIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitScrubbingService.GetUnlinkedIscis();

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(UnlinkedIscisDto), "FileDetailsId");

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
        public void GetNsiPostReportData()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitScrubbingService.GetNsiPostReportData(253);

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
                var result = _AffidavitScrubbingService.GetNsiPostReportData(25999);

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
                var result = _AffidavitScrubbingService.GetNsiPostReportData(26000);

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
            var report = _AffidavitScrubbingService.GenerateNSIPostReport(proposalId);
            File.WriteAllBytes(@"\\tsclient\cadent\" + @"NSIPostReport" + proposalId + ".xlsx", report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
                                                                                                                           //            File.WriteAllBytes(string.Format("..\\bvsreport{0}.xlsx", scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            Assert.IsNotNull(report.Stream);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetClientScrubbingForProposalMultipleGenres()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitScrubbingService.GetClientScrubbingForProposal(255);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        public void GenerateMyEventsReportTest()
        {
            var expectedFileName = "Test Adve NAV 30 05-30-16.txt";
            var expectedFilePath = @".\Files\" + expectedFileName;
            var myEventsReport = _AffidavitScrubbingService.GenerateMyEventsReport(25999);
            var tempPath = Path.GetTempFileName();

            File.WriteAllBytes(tempPath, myEventsReport.Stream.ToArray());

            FileAssert.AreEqual(expectedFilePath, tempPath);
            Assert.AreEqual(expectedFileName, myEventsReport.Filename);
        }
    }
}
