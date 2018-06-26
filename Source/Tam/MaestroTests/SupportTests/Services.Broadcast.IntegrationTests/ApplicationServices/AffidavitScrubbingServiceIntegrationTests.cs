using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class AffidavitScrubbingServiceIntegrationTests
    {
        private readonly IAffidavitService _AffidavitService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitService>();

        private readonly IAffidavitScrubbingService _AffidavitScrubbingService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>();

        private readonly IPostReportService _PostReportService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IPostReportService>();

        private readonly IAffidavitRepository _AffidavitRepository = IntegrationTestApplicationServiceFactory
            .BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();

        [Test]
        public void GetPostsTest()
        {
            var result = _AffidavitScrubbingService.GetPosts();

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PostDto), "Id");
            jsonResolver.Ignore(typeof(PostDto), "ContractId");

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
                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _AffidavitScrubbingService.GetClientScrubbingForProposal(253, scrubbingRequest);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailPostScrubbingDto), "ScrubbingClientId");

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
        public void GetClientScrubbingForProposal_InSpecOnly()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest() {ScrubbingStatusFilter = ScrubbingStatus.InSpec};
                var result = _AffidavitScrubbingService.GetClientScrubbingForProposal(253, scrubbingRequest);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailPostScrubbingDto), "ScrubbingClientId");

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
        public void GetClientScrubbingForProposal_OutOfSpecOnly()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest =
                    new ProposalScrubbingRequest() {ScrubbingStatusFilter = ScrubbingStatus.OutOfSpec};
                var result = _AffidavitScrubbingService.GetClientScrubbingForProposal(253, scrubbingRequest);

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

                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _AffidavitScrubbingService.GetClientScrubbingForProposal(proposalId, scrubbingRequest);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailPostScrubbingDto), "ScrubbingClientId");

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
                var result = _AffidavitScrubbingService.GetUnlinkedIscis(false);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(UnlinkedIscisDto), "FileDetailId");

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
        public void GetArchivedIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitScrubbingService.GetUnlinkedIscis(true);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(UnlinkedIscisDto), "FileDetailId");

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
        public void ArchiveIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitScrubbingService.ArchiveUnlinkedIsci(new List<long>() { 1003 }, "ApprovedTest");

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [ExpectedException(typeof(System.Exception),
            ExpectedMessage = "There are already blacklisted iscis in your list", MatchType = MessageMatch.Exact)]
        public void ArchiveIsci_ExistingIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitScrubbingService.ArchiveUnlinkedIsci(new List<long>() {4286}, "ApprovedTest");
                _AffidavitScrubbingService.ArchiveUnlinkedIsci(new List<long>() {4286}, "ApprovedTest");
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UndoArchiveIsci()
        {
            using (new TransactionScopeWrapper())
            {
                _AffidavitScrubbingService.UndoArchiveUnlinkedIsci(new List<long>() { 4286 }, DateTime.Now, "ApprovedTest");
                var result = _AffidavitScrubbingService.GetUnlinkedIscis(false);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [UseReporter(typeof(DiffReporter))]
        public void GetClientScrubbingForProposalMultipleGenres()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _AffidavitScrubbingService.GetClientScrubbingForProposal(255, scrubbingRequest);

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
        [UseReporter(typeof(DiffReporter))]
        public void GetClientScrubbingReturnsEffectiveProgramGenreShowtype()
        {
            using (new TransactionScopeWrapper())
            {
                AffidavitSaveRequest affidavitSaveRequest = new AffidavitSaveRequest
                {
                    FileHash = "abc123",
                    Source = (int) AffidaviteFileSourceEnum.Strata,
                    FileName = "test.file",
                    Details = new List<AffidavitSaveRequestDetail>()
                    {
                        new AffidavitSaveRequestDetail
                        {
                            AirTime = DateTime.Parse("06/29/2017 8:00AM"),
                            Isci = "FFFFFF",
                            ProgramName = "MainProgramName",
                            SpotLength = 30,
                            Genre = "MainGenre",
                            Station = "WWSB",
                            LeadInEndTime = DateTime.Parse("06/29/2017 8:31AM"),
                            LeadOutStartTime = DateTime.Parse("06/29/2017 8:02AM"),
                            ShowType = "Movie",
                            LeadInShowType = "LeadInShowType",
                            LeadOutShowType = "LeadOutShowType",
                            LeadInGenre = "LeadInGenre",
                            LeadOutProgramName = "LeadOutProgramName",
                            LeadInProgramName = "LeadInProgramName",
                            InventorySource = AffidaviteFileSourceEnum.Strata,
                            LeadOutGenre = "LeadOutGenre",
                            Affiliate = "Affiate",
                            Market = "market"
                        }
                    }
                };

                _AffidavitService.SaveAffidavit(affidavitSaveRequest, "TestUser", DateTime.Parse("06/29/2017 8:04AM"));

                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _AffidavitScrubbingService.GetClientScrubbingForProposal(253, scrubbingRequest);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailPostScrubbingDto), "ScrubbingClientId");
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
        [ExpectedException(typeof(System.Exception))]
        public void ClientScrubbingOverrides_Bad_ProposalId_Used()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var scrubs = _AffidavitScrubbingService.GetClientScrubbingForProposal(253, scrubbingRequest);

                // grab second item and override it
                var scrubIds =
                    new System.Collections.Generic.List<int>() {scrubs.ClientScrubs.First().ScrubbingClientId};

                ScrubStatusOverrideRequest overrides = new ScrubStatusOverrideRequest()
                {
                    ProposalId = 2543, // different proposal Id should throw exception
                    ScrubIds = scrubIds,
                    OverrideStatus = ScrubbingStatus.OutOfSpec
                };
                var result = _AffidavitScrubbingService.OverrideScrubbingStatus(overrides);
            }
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ClientScrubbingOverrides()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var scrubs = _AffidavitScrubbingService.GetClientScrubbingForProposal(253, scrubbingRequest);

                // grab second item and override it
                var scrubIds = new List<int>() { scrubs.ClientScrubs.Last().ScrubbingClientId };

                ScrubStatusOverrideRequest overrides = new ScrubStatusOverrideRequest()
                {
                    ProposalId = 253,
                    ScrubIds = scrubIds,
                    OverrideStatus = ScrubbingStatus.OutOfSpec  
                };
                var result = _AffidavitScrubbingService.OverrideScrubbingStatus(overrides);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailPostScrubbingDto), "ScrubbingClientId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ClientScrubbingOverrides_Exclude_Existing_Status_Overrides()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var scrubs = _AffidavitScrubbingService.GetClientScrubbingForProposal(253, scrubbingRequest);

                // grab second item and override it
                var scrubIds = new List<int>() { scrubs.ClientScrubs.Last().ScrubbingClientId };

                ScrubStatusOverrideRequest overrides = new ScrubStatusOverrideRequest()
                {
                    ProposalId = 253,
                    ScrubIds = scrubIds,
                    OverrideStatus = ScrubbingStatus.InSpec // the current status of this record is InSpec, so this will not cause override.
                };
                var result = _AffidavitScrubbingService.OverrideScrubbingStatus(overrides);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailPostScrubbingDto), "ScrubbingClientId");

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
        public void FindValidIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitScrubbingService.FindValidIscis(string.Empty);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

    }
}
