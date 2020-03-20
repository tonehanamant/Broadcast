using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Ignore]
    public class PostLogServiceIntegrationTests
    {
        private readonly IPostLogRepository _PostLogRepository;
        private readonly IPostLogService _PostLogService;
        private const string _UserName = "PostLog Post Processing Test User";
        private const string ISCI1 = "DDDDDDDD";
        private const string ISCI2 = "FFFFFF";
        private readonly string ProgramName1 = "Program Names R us";
        private readonly LookupDto Genre1 = new LookupDto() { Id = 13, Display = "Do It Yourself" };
        private readonly LookupDto Genre2 = new LookupDto() { Id = 47, Display = "Thriller & Suspense" };

        public PostLogServiceIntegrationTests()
        {
            _PostLogService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostLogService>();
            _PostLogRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostLogRepository>();
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_GetPostsTest()
        {
            var date = new DateTime(2018, 9, 3);
            var result = _PostLogService.GetPostLogs(date);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PostedContract), "ContractId");
            jsonResolver.Ignore(typeof(PostedContract), "UploadDate");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_GetUnlinkedIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostLogService.GetUnlinkedIscis();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_GetArchivedIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostLogService.GetArchivedIscis();

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ArchivedIscisDto), "FileDetailId");

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
        public void PostLogService_ArchiveIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostLogService.ArchiveUnlinkedIsci(new List<string>() { "SomeISCI" }, "ApprovedTest");

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [ExpectedException(typeof(Exception),
            ExpectedMessage = "There are already blacklisted iscis in your list", MatchType = MessageMatch.Exact)]
        public void PostLogService_ArchiveIsci_ExistingIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostLogService.ArchiveUnlinkedIsci(new List<string>() { "ToBeArchivedIsci" }, "ApprovedTest");
                _PostLogService.ArchiveUnlinkedIsci(new List<string>() { "ToBeArchivedIsci" }, "ApprovedTest");
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_UndoArchiveIsci()
        {
            using (new TransactionScopeWrapper())
            {
                _PostLogService.UndoArchiveUnlinkedIsci(new List<long>() { 4 }, DateTime.Now, "ApprovedTest");
                var result = _PostLogService.GetUnlinkedIscis();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SaveLargePostLogFile()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupBigPostLog();
                var postingDate = new DateTime(2016, 4, 20);
                var result = _PostLogService.SaveKeepingTracFile(request, "test user", postingDate);

                VerifyPostLog(result);
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SaveKeepingTracFile_ValidationProblems()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupPostLog();

                var det = request.Details.First();
                det.Affiliate = string.Empty;
                det.AirTime = DateTime.MinValue;
                det.InventorySource = 0;

                var result = _PostLogService.SaveKeepingTracFile(request, "test user", new DateTime(2016, 4, 20));
                VerifyPostLog(result);
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SaveKeepingTracFile_WithEscapedChars()
        {
            using (new TransactionScopeWrapper())
            {
                var postingDate = new DateTime(2016, 4, 20);
                var request = _SetupPostLog_WithEscaped_Doublequotes();
                var result = _PostLogService.SaveKeepingTracFile(request, "test user", postingDate);

                VerifyPostLog(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SaveKeepingTracFile()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupPostLog();
                var postingDate = new DateTime(2016, 4, 20);

                var result = _PostLogService.SaveKeepingTracFile(request, "test user", postingDate);

                VerifyPostLog(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavesKeepingTracFile_WithMarriedIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupPostLog();
                var detail = request.Details.First();
                detail.SpotLength = 60;
                detail.Isci = "DDDDDDDDD1996";
                var postingDate = new DateTime(2016, 4, 20);

                var result = _PostLogService.SaveKeepingTracFile(request, "test user", postingDate);

                VerifyPostLog(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SaveKeepingTracFileWithShowTypes()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupPostLog();
                var postingDate = new DateTime(2016, 4, 20);

                request.Details.First().ShowType = "Drama1";
                request.Details.First().LeadInShowType = "Drama2";
                request.Details.First().LeadOutShowType = "Drama3";

                var result = _PostLogService.SaveKeepingTracFile(request, "test user", postingDate);

                VerifyPostLog(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SaveKeepingTracFileWithLeadInEndTime()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupPostLog();
                var postingDate = new DateTime(2016, 4, 20);

                request.Details.First().LeadInEndTime = DateTime.Parse("06/29/2017 10:00 AM");

                var result = _PostLogService.SaveKeepingTracFile(request, "test user", postingDate);

                VerifyPostLog(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SaveKeepingTracFileWithLeadOutStartTime()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupPostLog();
                var postingDate = new DateTime(2016, 4, 20);

                request.Details.First().LeadOutStartTime = DateTime.Parse("06/29/2017 12:12 AM");

                var result = _PostLogService.SaveKeepingTracFile(request, "test user", postingDate);

                VerifyPostLog(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SaveKeepingTracFileWithShowType()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupPostLog();
                var postingDate = new DateTime(2016, 4, 20);

                request.Details.First().ShowType = "Drama";

                var result = _PostLogService.SaveKeepingTracFile(request, "test user", postingDate);

                VerifyPostLog(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SaveKeepingTracFileMultipleIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupPostLogMultipleIscis();
                var postingDate = new DateTime(2016, 4, 20);

                var result = _PostLogService.SaveKeepingTracFile(request, "test user", postingDate);

                VerifyPostLog(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SavePostLogValidationErrors()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupPostLog();

                List<WWTVInboundFileValidationResult> validationResults = new List<WWTVInboundFileValidationResult>
                {
                    new WWTVInboundFileValidationResult()
                    {
                        ErrorMessage = "Generic error message",
                        InvalidField = "ErrorField",
                        InvalidLine = 255
                    }
                };
                var postingDate = new DateTime(2016, 4, 20);
                var result = _PostLogService.SaveKeepingTracValidationErrors(request, "test user", validationResults);

                VerifyPostLog(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_ScrubUnlinkedIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new ScrubIsciRequest
                {
                    Isci = "AAAAAAAA"
                };
                _PostLogService.ScrubUnlinkedPostLogDetailsByIsci(request.Isci, new DateTime(2019, 3, 31), "test-user");

                var postLogFile = _PostLogRepository.GetPostLogFile(1, true);

                VerifyPostLog(postLogFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_Hiatus_TimeMatch_PRI814()
        {
            using (new TransactionScopeWrapper())
            {
                var date = new DateTime(2019, 3, 31);
                IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
                var proposal = _ProposalService.GetProposalById(253);
                proposal.Details[0].Quarters[0].Weeks[0].IsHiatus = true;
                _ProposalService.CalculateProposalChanges(new ProposalChangeRequest() { Details = proposal.Details });
                _ProposalService.SaveProposal(proposal, "test user", date);


                var request = new ScrubIsciRequest
                {
                    Isci = "AAAAAAAA"
                };
               
                _PostLogService.ScrubUnlinkedPostLogDetailsByIsci(request.Isci, date, "test-user");

                var postLogFile = _PostLogRepository.GetPostLogFile(1, true);

                VerifyPostLog(postLogFile);
            }
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_MapIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new MapIsciDto
                {
                    OriginalIsci = "ToBeArchivedIsci",
                    EffectiveIsci = "AAAAAAAA1"
                };
                var result = _PostLogService.MapIsci(request, new DateTime(2018, 06, 01), "test-user");

                var postLogFile = _PostLogRepository.GetPostLogFile(1, true);

                VerifyPostLog(postLogFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_GetClientScrubbingForProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _PostLogService.GetClientScrubbingForProposal(253, scrubbingRequest);

                VerifyClientPostScrubbingObject(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_GetClientScrubbingForProposal_InSpecOnly()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest() { ScrubbingStatusFilter = ScrubbingStatus.InSpec };
                var result = _PostLogService.GetClientScrubbingForProposal(253, scrubbingRequest);

                VerifyClientPostScrubbingObject(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_GetClientScrubbingForProposal_OutOfSpecOnly()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest() { ScrubbingStatusFilter = ScrubbingStatus.OutOfSpec };
                var result = _PostLogService.GetClientScrubbingForProposal(253, scrubbingRequest);

                VerifyClientPostScrubbingObject(result);
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_OverrideScrubbingStatus()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubs = _PostLogService.GetClientScrubbingForProposal(253, new ProposalScrubbingRequest());

                // grab last item and override it
                var scrubIds = new List<int>() { scrubs.ClientScrubs.Last().ScrubbingClientId };

                ScrubStatusOverrideRequest overrides = new ScrubStatusOverrideRequest()
                {
                    ProposalId = 253,
                    ScrubIds = scrubIds,
                    OverrideStatus = ScrubbingStatus.OutOfSpec
                };
                var result = _PostLogService.OverrideScrubbingStatus(overrides);

                VerifyClientPostScrubbingObject(result);
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_UndoOverrideSclientScrubbingStatus()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubs = _PostLogService.GetClientScrubbingForProposal(253, new ProposalScrubbingRequest());

                // grab last item and override it
                var scrubIds = new List<int>() { scrubs.ClientScrubs.Last().ScrubbingClientId };

                ScrubStatusOverrideRequest overrides = new ScrubStatusOverrideRequest()
                {
                    ProposalId = 253,
                    ScrubIds = scrubIds,
                    OverrideStatus = ScrubbingStatus.OutOfSpec
                };
                var result = _PostLogService.OverrideScrubbingStatus(overrides);
                Assert.IsTrue(result.ClientScrubs.Last().StatusOverride == true);

                //undo the override
                result = _PostLogService.UndoOverrideScrubbingStatus(overrides);
                VerifyClientPostScrubbingObject(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PostLogService_SwapProposalDetail()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new SwapProposalDetailRequest
                {
                    ScrubbingIds = new List<int> { 3029, 3502 },
                    ProposalDetailId = 11199
                };
                var result = _PostLogService.SwapProposalDetails(request, new DateTime(2019, 3, 31), "test-user");

                var postlogFile = _PostLogRepository.GetPostLogFile(1, true);

                VerifyPostLog(postlogFile);
            }
        }

        private void VerifyPostLog(WWTVSaveResult result)
        {
            if (result.ValidationResults.Any() && !result.Id.HasValue)
            {
                var msg = WWTVInboundFileValidationResult.FormatValidationMessage(result.ValidationResults);
                Assert.IsTrue(!result.ValidationResults.Any(), msg);
            }

            Assert.IsTrue(result.Id.HasValue, result.ToString());

            ScrubbingFile postLogFile = _PostLogRepository.GetPostLogFile(result.Id.Value, true);
            VerifyPostLog(postLogFile);
        }

        private void VerifyPostLog(ScrubbingFile postlogFile)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ScrubbingFile), "CreatedDate");
            jsonResolver.Ignore(typeof(ScrubbingFile), "Id");
            jsonResolver.Ignore(typeof(ScrubbingFileDetail), "Id");
            jsonResolver.Ignore(typeof(ScrubbingFileDetail), "ScrubbingFileId");
            jsonResolver.Ignore(typeof(ClientScrub), "Id");
            jsonResolver.Ignore(typeof(ScrubbingFileAudiences), "ClientScrubId");
            jsonResolver.Ignore(typeof(ClientScrub), "ScrubbingFileDetailId");
            jsonResolver.Ignore(typeof(ClientScrub), "ModifiedDate");
            jsonResolver.Ignore(typeof(ScrubbingFile), "MediaMonthId");
            jsonResolver.Ignore(typeof(FileProblem), "Id");
            jsonResolver.Ignore(typeof(FileProblem), "FileId");
            jsonResolver.Ignore(typeof(ClientScrub), "ProposalVersionDetailQuarterWeekId");
            jsonResolver.Ignore(typeof(ClientScrub), "ProposalVersionDetailId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(postlogFile, jsonSettings);
            Approvals.Verify(json);
        }

        private void VerifyClientPostScrubbingObject(ClientPostScrubbingProposalDto result)
        {
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

        private InboundFileSaveRequest _SetupBigPostLog()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int)DeliveryFileSourceEnum.KeepingTrac,
                FileName = "test.file",
                Details = new List<InboundFileSaveRequestDetail>()
                {
                    new InboundFileSaveRequestDetail
                    {
                        AirTime = DateTime.Parse("05/30/2016 8:00AM"),
                        Isci = ISCI1,
                        ProgramName = ProgramName1,
                        SpotLength = 30,
                        Genre = Genre1.Display,
                        Station = "WNBC",
                        LeadInEndTime = DateTime.Parse("06/29/2017 8:31AM"),
                        LeadOutStartTime = DateTime.Parse("06/29/2017 8:02AM"),
                        ShowType = "News",
                        LeadInShowType = "Comedy",
                        LeadOutShowType = "Documentary",
                        LeadInGenre = "News",
                        LeadOutProgramName = "LeadOutProgramName",
                        LeadInProgramName = "LeadInProgramName",
                        InventorySource = DeliveryFileSourceEnum.KeepingTrac,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }
                }
            };
            var iscis = new List<string>()
            {
                "AAAAAAAA", "BBBBBBBBB", "CCCCCCCCCC", ISCI1, "EEEEEEEEEEE", ISCI2
            };
            int maxTimeAdd = 60 * 59;
            // first 3 stations of the top 16 markets
            var stations = new List<string>()
            {
                "WFUT", "HPIX", "WNJN", "OTLA", "NKJR", "HMM", "ESNS", "WESV", "GGN", "WNJS","RDZN", "GPHL", "QUBO", "NKJR", "FBN", "RLZC", "HMM", "NKJR",
                "WPXW", "NKJR", "GDCW", "NKJR", "GENH", "WBPX", "NKJR", "FBN", "HMM", "NTMD", "KUVM", "QUBO", "NKJR", "FBN", "GMOR", "KTVW", "KFPH", "KPNX",
                "NKJR", "QUBO", "FBN", "NKJR", "KBTC", "FBN", "KSTP", "KAWE", "WCCO", "HMM", "HSTE", "NKJR", "KDVR", "NKJR",
            };

            for (int c = 0; c < 102; c++)
            {
                var airTime = DateTime.Parse("2016-05-30 8:01AM").AddSeconds(c % maxTimeAdd).AddDays(c % 5);
                var isci = iscis[c % iscis.Count];
                var station = stations[c % stations.Count];
                request.Details.Add(new InboundFileSaveRequestDetail()
                {
                    AirTime = airTime,
                    Isci = isci,
                    ProgramName = ProgramName1,
                    SpotLength = 30,
                    Genre = Genre1.Display,
                    Station = station,
                    LeadInEndTime = DateTime.Parse("06/29/2017 8:31AM"),
                    LeadOutStartTime = DateTime.Parse("06/29/2017 8:02AM"),
                    ShowType = "News",
                    LeadInShowType = "Comedy",
                    LeadOutShowType = "Documentary",
                    LeadInGenre = "News",
                    LeadOutProgramName = "LeadOutProgramName",
                    LeadInProgramName = "LeadInProgramName",
                    InventorySource = DeliveryFileSourceEnum.KeepingTrac,
                    LeadOutGenre = "LeadOutGenre",
                    Affiliate = "Affiate",
                    Market = "market"
                });
            }

            var lastDetail = request.Details.Last();
            lastDetail.ProgramName = null;
            lastDetail.SuppliedProgramName = "SuppliedProgramName";
            
            return request;
        }

        private InboundFileSaveRequest _SetupPostLog()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int)DeliveryFileSourceEnum.KeepingTrac,
                FileName = "test.file",
                Details = new List<InboundFileSaveRequestDetail>()
                {
                    new InboundFileSaveRequestDetail
                    {
                        AirTime = DateTime.Parse("06/29/2017 8:04AM"),
                        Isci = ISCI1,
                        ProgramName = ProgramName1,
                        SpotLength = 30,
                        Genre = Genre1.Display,
                        Station = "WWSB",
                        LeadInEndTime = DateTime.Parse("06/29/2017 8:31AM"),
                        LeadOutStartTime = DateTime.Parse("06/29/2017 8:02AM"),
                        ShowType = "News",
                        LeadInShowType = "Comedy",
                        LeadOutShowType = "Documentary",
                        LeadInGenre = "News",
                        LeadOutProgramName = "LeadOutProgramName",
                        LeadInProgramName = "LeadInProgramName",
                        InventorySource = DeliveryFileSourceEnum.KeepingTrac,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }
                }
            };
            return request;
        }

        private InboundFileSaveRequest _SetupPostLog_WithEscaped_Doublequotes()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int)DeliveryFileSourceEnum.KeepingTrac,
                FileName = "test.file",
                Details = new List<InboundFileSaveRequestDetail>()
                {
                    new InboundFileSaveRequestDetail
                    {
                        AirTime = DateTime.Parse("06/29/2017 8:04AM"),
                        Isci = "DD\"DDDDDD",
                        ProgramName = ProgramName1,
                        SpotLength = 30,
                        Genre = Genre1.Display,
                        Station = "WWSB",
                        LeadInEndTime = DateTime.Parse("06/29/2017 8:31AM"),
                        LeadOutStartTime = DateTime.Parse("06/29/2017 8:02AM"),
                        ShowType = "News",
                        LeadInShowType = "Comedy",
                        LeadOutShowType = "Documentary",
                        LeadInGenre = "News",
                        LeadOutProgramName = "LeadOutProgramName",
                        LeadInProgramName = "LeadInProgramName",
                        InventorySource = DeliveryFileSourceEnum.KeepingTrac,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }
                }
            };
            return request;
        }

        private InboundFileSaveRequest _SetupPostLogMultipleIscis()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int)DeliveryFileSourceEnum.KeepingTrac,
                FileName = "test.file"
            };

            request.Details.Add(new InboundFileSaveRequestDetail
            {
                AirTime = DateTime.Parse("06/08/2017 8:04AM"),
                Isci = ISCI2,
                ProgramName = ProgramName1,
                SpotLength = 30,
                Genre = Genre1.Display,
                Station = "WWSB",
                LeadInEndTime = DateTime.Parse("06/29/2017 8:31AM"),
                LeadOutStartTime = DateTime.Parse("06/29/2017 8:02AM"),
                ShowType = "News",
                LeadInShowType = "Comedy",
                LeadOutShowType = "Documentary",
                LeadInGenre = "News",
                LeadOutProgramName = "LeadOutProgramName",
                LeadInProgramName = "LeadInProgramName",
                InventorySource = DeliveryFileSourceEnum.KeepingTrac,
                LeadOutGenre = "LeadOutGenre",
                Affiliate = "Affiate"
            });
            
            return request;
        }
    }
}
