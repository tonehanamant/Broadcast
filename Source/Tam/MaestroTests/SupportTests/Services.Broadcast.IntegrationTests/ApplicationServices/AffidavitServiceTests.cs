using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.OpenMarketInventory;
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
    public class AffidavitServiceTests
    {
        private const string ISCI1 = "DDDDDDDD";
        private const string ISCI2 = "FFFFFF";

        private readonly IProposalService _ProposalService;
        private readonly IIsciService _IsciService;
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();

        private readonly IPostRepository _PostRepository;
        private readonly IAffidavitService _AffidavitService;
        private readonly IAffidavitRepository _AffidavitRepository;

        private readonly LookupDto Genre1 = new LookupDto() {Id = 13, Display = "Do It Yourself"};
        private readonly LookupDto Genre2 = new LookupDto() {Id = 47, Display = "Thriller & Suspense"};

        private readonly string ProgramName1 = "Program Names R us";

        public AffidavitServiceTests()
        {
            _AffidavitService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitService>();
            _IsciService = IntegrationTestApplicationServiceFactory.GetApplicationService<IIsciService>();
            _AffidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();

            _PostRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IPostRepository>();
            _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveBigAffidaviteService()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupBigAffidavit();
                var postingDate = new DateTime(2016, 4, 20);
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService_Validation_Problems()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();

                var det = request.Details.First();
                det.Affiliate = "";
                det.AirTime = DateTime.MinValue;
                det.InventorySource = 0;

                var postingDate = new DateTime(2016, 4, 20);

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVSaveResult), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
                Approvals.Verify(json);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService_With_Escaped_chars()
        {
            using (new TransactionScopeWrapper())
            {
                var postingDate = new DateTime(2016, 4, 20);
                var request = _SetupAffidavit_WithEscaped_Doublequotes();
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();
                var postingDate = new DateTime(2016, 4, 20);

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService_MarriedIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();
                var detail = request.Details.First();
                detail.SpotLength = 60;
                detail.Isci = "DDDDDDDDD1996";

                var postingDate = new DateTime(2016, 4, 20);

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteServiceWithShowTypes()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();
                var postingDate = new DateTime(2016, 4, 20);

                request.Details.First().ShowType = "Drama1";
                request.Details.First().LeadInShowType = "Drama2";
                request.Details.First().LeadOutShowType = "Drama3";

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteServiceWithLeadInEndTime()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();
                var postingDate = new DateTime(2016, 4, 20);

                request.Details.First().LeadInEndTime = DateTime.Parse("06/29/2017 10:00 AM");

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteServiceWithLeadOutStartTime()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();
                var postingDate = new DateTime(2016, 4, 20);

                request.Details.First().LeadOutStartTime = DateTime.Parse("06/29/2017 12:12 AM");

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteServiceWithShowType()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();
                var postingDate = new DateTime(2016, 4, 20);

                request.Details.First().ShowType = "Drama";

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteServiceMultipleIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavitMultipleIscis();
                var postingDate = new DateTime(2016, 4, 20);

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        private void VerifyAffidavit(WWTVSaveResult result)
        {
            if (result.ValidationResults.Any() && !result.Id.HasValue)
            {
                var msg = WWTVInboundFileValidationResult.FormatValidationMessage(result.ValidationResults);
                Assert.IsTrue(!result.ValidationResults.Any(), msg);
            }

            Assert.IsTrue(result.Id.HasValue, result.ToString());

            ScrubbingFile affidavit = _AffidavitRepository.GetAffidavit(result.Id.Value, true);
            VerifyAffidavit(affidavit);
        }

        private void VerifyAffidavit(ScrubbingFile affidavit)
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
            var json = IntegrationTestHelper.ConvertToJson(affidavit, jsonSettings);
            Approvals.Verify(json);
        }

        private InboundFileSaveRequest _SetupAffidavitMultipleIscis()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int) DeliveryFileSourceEnum.Strata,
                FileName = "test.file"
            };

            var detail = new InboundFileSaveRequestDetail
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
                InventorySource = DeliveryFileSourceEnum.Strata,
                LeadOutGenre = "LeadOutGenre",
                Affiliate = "Affiate"
            };

            request.Details.Add(detail);

            return request;
        }
        
        private InboundFileSaveRequest _SetupAffidavit()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int)DeliveryFileSourceEnum.Strata,
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
                        InventorySource = DeliveryFileSourceEnum.Strata,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }
                }
            };
            return request;
        }

        private InboundFileSaveRequest _SetupAffidavit_TwoDetails()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int) DeliveryFileSourceEnum.Strata,
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
                        InventorySource = DeliveryFileSourceEnum.Strata,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    },
                    new InboundFileSaveRequestDetail
                    {
                        AirTime = DateTime.Parse("06/29/2017 8:04AM"),
                        Isci = "foey",
                        ProgramName = ProgramName1 + " Part II",
                        SpotLength = 30,
                        Genre = Genre1.Display,
                        Station = "WABC",
                        LeadInEndTime = DateTime.Parse("06/29/2017 8:31AM"),
                        LeadOutStartTime = DateTime.Parse("06/29/2017 8:02AM"),
                        ShowType = "News",
                        LeadInShowType = "Comedy",
                        LeadOutShowType = "Documentary",
                        LeadInGenre = "News",
                        LeadOutProgramName = "LeadOutProgramName",
                        LeadInProgramName = "LeadInProgramName",
                        InventorySource = DeliveryFileSourceEnum.Strata,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }

                }
            };
            return request;
        }

        private InboundFileSaveRequest _SetupAffidavit_WithEscaped_Doublequotes()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int)DeliveryFileSourceEnum.Strata,
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
                        InventorySource = DeliveryFileSourceEnum.Strata,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }
                }
            };
            return request;
        }

        private InboundFileSaveRequest _SetupBigAffidavit()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int)DeliveryFileSourceEnum.Strata,
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
                        InventorySource = DeliveryFileSourceEnum.Strata,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }
                }
            };
            var iscis = new List<string>()
            {
                "AAAAAAAA",
                "BBBBBBBBB",
                "CCCCCCCCCC",
                ISCI1,
                "EEEEEEEEEEE",
                ISCI2
            };
            int maxTimeAdd = 60 * 59;
            // first 3 stations of the top 16 markets
            var stations = new List<string>()
            {
                "WFUT",
                "HPIX",
                "WNJN",
                "OTLA",
                "NKJR",
                "HMM",
                "ESNS",
                "WESV",
                "GGN",
                "WNJS",
                "RDZN",
                "GPHL",
                "QUBO",
                "NKJR",
                "FBN",
                "RLZC",
                "HMM",
                "NKJR",
                "WPXW",
                "NKJR",
                "GDCW",
                "NKJR",
                "GENH",
                "WBPX",
                "NKJR",
                "FBN",
                "HMM",
                "NTMD",
                "KUVM",
                "QUBO",
                "NKJR",
                "FBN",
                "GMOR",
                "KTVW",
                "KFPH",
                "KPNX",
                "NKJR",
                "QUBO",
                "FBN",
                "NKJR",
                "KBTC",
                "FBN",
                "KSTP",
                "KAWE",
                "WCCO",
                "HMM",
                "HSTE",
                "NKJR",
                "KDVR",
                "NKJR",
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
                    InventorySource = DeliveryFileSourceEnum.Strata,
                    LeadOutGenre = "LeadOutGenre",
                    Affiliate = "Affiate",
                    Market = "market"
                });
            }

            return request;
        }

        private InboundFileSaveRequest _SetupAffidavitWithSameIsciForTwoSpots()
        {
            InboundFileSaveRequest request = new InboundFileSaveRequest
            {
                FileHash = "abc123",
                Source = (int)DeliveryFileSourceEnum.Strata,
                FileName = "test.file",
                Details = new List<InboundFileSaveRequestDetail>()
                {
                    new InboundFileSaveRequestDetail
                    {
                        AirTime = DateTime.Parse("12/23/2018 6:15"),
                        Isci = "BCOP3282",
                        ProgramName = ProgramName1,
                        SpotLength = 15,
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
                        InventorySource = DeliveryFileSourceEnum.Strata,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    },
                    new InboundFileSaveRequestDetail
                    {
                        AirTime = DateTime.Parse("12/31/2018 6:15"),
                        Isci = "BCOP3282",
                        ProgramName = ProgramName1,
                        SpotLength = 15,
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
                        InventorySource = DeliveryFileSourceEnum.Strata,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }
                }
            };
            return request;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Affidavit_Station_Market_Scrub()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 413, 416);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto()
                    {
                        Brand = "WAWA",
                        ClientIsci = "WAWA",
                        HouseIsci = "WAWA",
                        Days = "M|T|W|TH|F|SA|SU"
                    }
                };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);
                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Affidavit_Station_Genre_Scrub_inclusive()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto() {Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA"}
                };
                proposal.Details.First().GenreCriteria.Add(new GenreCriteria()
                {
                    Contain = ContainTypeEnum.Include,
                    Genre = new LookupDto() {Id = Genre1.Id, Display = Genre1.Display}
                });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);
                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Affidavit_Station_Genre_Scrub_exclusive()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto() {Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA"}
                };
                proposal.Details.First().GenreCriteria.Add(new GenreCriteria()
                {
                    Contain = ContainTypeEnum.Exclude,
                    Genre = new LookupDto() {Id = Genre1.Id, Display = Genre1.Display}
                });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                var detail = request.Details.First();
                detail.Isci = "WAWA";
                detail.LeadOutGenre = detail.Genre;
                detail.LeadOutProgramName = detail.ProgramName;
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);
                VerifyAffidavit(result);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void Ensure_Correct_Scrubbing_Records_Following_Unmatched_Record()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 413, 416);

                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                var postingDate = new DateTime(2016, 4, 20);
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.Add(new InboundFileSaveRequestDetail
                {
                    AirTime = DateTime.Parse("06/29/2017 8:04AM"),
                    Isci = ISCI1,
                    ProgramName = ProgramName1,
                    SpotLength = 30,
                    Genre = Genre1.Display,
                    Station = "WWSB",
                    ShowType = "News",
                    LeadInShowType = "Comedy",
                    LeadOutShowType = "Documentary",
                    LeadInGenre = "News",
                    LeadOutProgramName = "LeadOutProgramName",
                    LeadInProgramName = "LeadInProgramName",
                    InventorySource = DeliveryFileSourceEnum.Strata,
                    LeadOutGenre = "LeadOutGenre",
                    Affiliate = "Affiate"
                });
                request.Details.First().Isci = "Uknown_Unmatched_ISCI";
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);
                VerifyAffidavit(result);
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Affidavit_Station_Program_Scrub_inclusive()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto() {Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA"}
                };
                proposal.Details.First().ProgramCriteria.Add(new ProgramCriteria()
                {
                    Contain = ContainTypeEnum.Include,
                    Program = new LookupDto {Display = ProgramName1, Id = 1}
                });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);
                VerifyAffidavit(result);
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Affidavit_Station_Program_Scrub_exclusive()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto() {Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA"}
                };
                proposal.Details.First().ProgramCriteria.Add(new ProgramCriteria()
                {
                    Contain = ContainTypeEnum.Exclude,
                    Program = new LookupDto {Display = ProgramName1, Id = 100}
                });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);
                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Affidavit_Scrub_Isci_Days_No_Match()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto() {Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA", Days = "M"}
                };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations)
                    .SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);
                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Affidavit_Scrub_Isci_Days_Match()
        {
            using (new TransactionScopeWrapper())
            {
                var postingDate = new DateTime(2016, 4, 20);
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>();

                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto() {Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA", Days = "TH"}
                };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations)
                    .SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                var request = _SetupAffidavit();

                request.Details.First().Isci = "WAWA";

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Affidavit_Match_Proposal_Week_Isci_Day_Without_Matching_Daypart_day()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>();
                var postingDate = new DateTime(2016, 4, 20);
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto() {Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA", Days = "TH"}
                };

                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
    
                // Daypart that does not include thurday (the airtime for the affidavit).
                proposal.Details.First().DaypartId = 88;

                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations)
                    .SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                var request = _SetupAffidavit();

                request.Details.First().Isci = "WAWA";

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Affidavit_Match_Time_When_Daypart_Day_Does_Not_Match()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>();
                var postingDate = new DateTime(2016, 4, 20);
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto() {Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA", Days = "M"}
                };

                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;

                // Daypart that does not include thurday (the airtime for the affidavit).
                proposal.Details.First().DaypartId = 88;

                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations)
                    .SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                var request = _SetupAffidavit();

                request.Details.First().Isci = "WAWA";

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService_Funky_TV_Station_Name_BCOP3092()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();

                request.Details.First().Station = "WBTS-TV";

                var postingDate = new DateTime(2016, 4, 20);

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService_Funky_Station_Name_BCOP3159()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();

                request.Details.First().Station = "WNBC-SR";

                var postingDate = new DateTime(2016, 4, 20);

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService_Funky_Station_Name_BCOP3159_Dash_Only()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();

                request.Details.First().Station = "WNBC-";

                var postingDate = new DateTime(2016, 4, 20);

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidavitValidationErrors()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit();

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
                var result = _AffidavitService.SaveAffidavitValidationErrors(request, "test user", validationResults);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubUnlinkedIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new ScrubIsciRequest
                {
                    Isci = "AAAAAAAA"
                };
                
                var result = _AffidavitService.ScrubUnlinkedAffidavitDetailsByIsci(request.Isci, new DateTime(2019, 3, 31), "test-user");

                var affidavit = _AffidavitRepository.GetAffidavit(157, true);

                VerifyAffidavit(affidavit);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MapIsci_EffectiveGenreNull()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new MapIsciDto
                {
                    OriginalIsci = "SomeISCI3",
                    EffectiveIsci = "AAAAAAAA1"
                };
                var result = _AffidavitService.MapIsci(request, new DateTime(2018, 06, 01), "test-user");

                var affidavit = _AffidavitRepository.GetAffidavit(160, true);

                VerifyAffidavit(affidavit);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MapIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new MapIsciDto
                {
                    OriginalIsci = "ToBeArchivedIsci",
                    EffectiveIsci = "AAAAAAAA1"
                };
                var result = _AffidavitService.MapIsci(request, new DateTime(2018, 06, 01), "test-user");

                var affidavit = _AffidavitRepository.GetAffidavit(157, true);

                VerifyAffidavit(affidavit);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SwapProposalDetail()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new SwapProposalDetailRequest
                {
                    ScrubbingIds = new List<int> {1008, 1009},
                    ProposalDetailId = 9954
                };
                
                var result = _AffidavitService.SwapProposalDetails(request, new DateTime(2019, 3, 31), "test-user");

                var affidavit = _AffidavitRepository.GetAffidavit(161, true);

                VerifyAffidavit(affidavit);
            }
        }

        private void AllocationProgram(int proposalDetailId, int programId, int mediaWeekId)
        {
            var request = new OpenMarketAllocationSaveRequest
            {
                ProposalVersionDetailId = proposalDetailId,
                Username = "test-user",
                Weeks = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek>
                {
                    new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek
                    {
                        MediaWeekId = mediaWeekId,
                        Programs = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram>
                        {
                            new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram
                            {
                                UnitImpressions = 1000,
                                TotalImpressions = 10000,
                                ProgramId = programId,
                                Spots = 10
                            }
                        }
                    }
                }
            };

            _ProposalOpenMarketInventoryService.SaveInventoryAllocations(request);
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService_Multiple_Isci_Mappings_BCOP3269()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavit_TwoDetails();
                var postingDate = new DateTime(2016, 4, 20);

                _AffidavitService.MapIsci(new MapIsciDto() {EffectiveIsci = "fff", OriginalIsci = "DDDDDDDD"}
                    , postingDate
                    , "Mapped Thing");

                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(WWTVSaveResult), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(result, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteServiceSameIsciInSameDetailWithDifferentWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAffidavitWithSameIsciForTwoSpots();
                var postingDate = new DateTime(2018, 7, 2);
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        // ensure saving affidavit + a rescrub changes nothing
        public void ReScrubProposalDetail_Scrub_Rescrub_NoChange()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 413, 416);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto()
                    {
                        Brand = "WAWA",
                        ClientIsci = "WAWA",
                        HouseIsci = "WAWA",
                        Days = "M|T|W|TH|F|SA|SU"
                    }
                };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var saveResult = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                _AffidavitService.RescrubProposalDetail(new RescrubProposalDetailRequest() { ProposalId = proposal.Id.Value, ProposalDetailId = proposalDetailId }, "test user", postingDate);
                VerifyAffidavit(saveResult);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReScrubProposalDetail_Basic_Changes_Isci()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 413, 416);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto()
                    {
                        Brand = "WAWA",
                        ClientIsci = "WAWA",
                        HouseIsci = "WAWA",
                        Days = "M|T|W|TH|F|SA|SU"
                    }
                };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var saveResult = _AffidavitService.SaveAffidavit(request, "test user", postingDate);
                proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);

                var isci = proposal.Details.First().Quarters.First().Weeks.First().Iscis.First();
                isci.HouseIsci = "WAWA2";
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                _AffidavitService.RescrubProposalDetail(new RescrubProposalDetailRequest() { ProposalId = proposal.Id.Value, ProposalDetailId = proposalDetailId}, "test user", postingDate);

                VerifyAffidavit(saveResult);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),ExpectedMessage = AffidavitService.ProposalNotContractedMessage, MatchType = MessageMatch.Exact)]
        public void ReScrubProposalDetail_Proposal_Not_Contracted()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 413, 416);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto()
                    {
                        Brand = "WAWA",
                        ClientIsci = "WAWA",
                        HouseIsci = "WAWA",
                        Days = "M|T|W|TH|F|SA|SU"
                    }
                };
                _AffidavitService.RescrubProposalDetail(new RescrubProposalDetailRequest() { ProposalId = proposal.Id.Value, ProposalDetailId = proposalDetailId }, "test user", postingDate);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReScrubProposalDetail_Basic_Changes()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);
                var postingDate = new DateTime(2016, 4, 20);

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 413, 416);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>()
                {
                    new ProposalWeekIsciDto()
                    {
                        Brand = "WAWA",
                        ClientIsci = "WAWA",
                        HouseIsci = "WAWA",
                        Days = "M|T|W|TH|F|SA|SU",
                        Monday = true,
                        Tuesday = true,
                        Wednesday = true,
                        Thursday = true,
                        Friday = true,
                        Saturday = true,
                        Sunday = true
                    }
                };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var saveResult = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                proposal = _ProposalService.UnorderProposal(proposal.Id.Value,"test user");

                // at this point we've setup and scrubbed first time.
                // now make changes, rescrub and verify results
                var detail = proposal.Details.First();
                var isci = detail.Quarters.First().Weeks.First().Iscis.First();
                isci.Thursday = false;

                var daypartCache = IntegrationTestApplicationServiceFactory.Instance.Resolve<IDaypartCache>();

                detail.DaypartId = 1395; // change to 'M,SU 1AM-2AM'
                detail.Daypart = DaypartDto.ConvertDisplayDaypart(daypartCache.GetDisplayDaypart(1395));
                detail.ShowTypeCriteria.Add(new ShowTypeCriteria
                    {
                        Id = 1,
                        Contain = ContainTypeEnum.Include,
                        ShowType = new LookupDto { Id = 1, Display = "Movie" }
                    }
                );
                detail.GenreCriteria.Add(new GenreCriteria
                    {
                        Id = 1,
                        Contain = ContainTypeEnum.Include,
                        Genre = new LookupDto { Id = 1, Display = "Action" }
                    }
                );
                detail.ProgramCriteria.Add(new ProgramCriteria
                    {
                        Id = 1,
                        Contain = ContainTypeEnum.Include,
                        Program = new LookupDto { Id = 1, Display = "West World" }
                    }
                );
                proposal.MarketGroupId = null;
                proposal.Markets = new List<ProposalMarketDto>()
                {
                    new ProposalMarketDto
                    {
                        Id = 100,
                        IsBlackout = false
                    },
                    new ProposalMarketDto
                    {
                        Id = 196,
                        IsBlackout = true
                    }
                };

                proposal = _ProposalService.SaveProposal(proposal, "test user", postingDate);

                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                _AffidavitService.RescrubProposalDetail(new RescrubProposalDetailRequest() { ProposalId = proposal.Id.Value, ProposalDetailId = proposalDetailId }, "test user", postingDate);

                VerifyAffidavit(saveResult);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPostsTest()
        {
            var result = _AffidavitService.GetPosts();

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PostedContract), "ContractId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPostsTestWithNtiAdjustments()
        {
            var result = _AffidavitService.GetPosts();
            var contract = result.Posts.First(x => x.ContractId == 26011);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PostedContract), "Id");
            jsonResolver.Ignore(typeof(PostedContract), "ContractId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(contract, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPostsTestWithoutNtiAdjustments()
        {
            var result = _AffidavitService.GetPosts();
            var contract = result.Posts.First(x => x.ContractId == 26012);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PostedContract), "Id");
            jsonResolver.Ignore(typeof(PostedContract), "ContractId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(contract, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetClientScrubbingForProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _AffidavitService.GetClientScrubbingForProposal(253, scrubbingRequest);

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
                var scrubbingRequest = new ProposalScrubbingRequest() { ScrubbingStatusFilter = ScrubbingStatus.InSpec };
                var result = _AffidavitService.GetClientScrubbingForProposal(253, scrubbingRequest);

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
                    new ProposalScrubbingRequest() { ScrubbingStatusFilter = ScrubbingStatus.OutOfSpec };
                var result = _AffidavitService.GetClientScrubbingForProposal(253, scrubbingRequest);

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
                aff.FileDetails[0].Station = "bad station";
                aff.FileDetails[0].ClientScrubs[0].Comment = "Client Scrub Comment";
                _AffidavitRepository.SaveAffidavitFile(aff);

                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _AffidavitService.GetClientScrubbingForProposal(proposalId, scrubbingRequest);

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
                var result = _AffidavitService.GetUnlinkedIscis();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetArchivedIscis()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitService.GetArchivedIscis();

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
        public void ArchiveIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _AffidavitService.ArchiveUnlinkedIsci(new List<string>() { "SomeISCI" }, "ApprovedTest");

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
                var result = _AffidavitService.ArchiveUnlinkedIsci(new List<string>() { "ToBeArchivedIsci" }, "ApprovedTest");
                _AffidavitService.ArchiveUnlinkedIsci(new List<string>() { "ToBeArchivedIsci" }, "ApprovedTest");
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UndoArchiveIsci()
        {
            using (new TransactionScopeWrapper())
            {
                _AffidavitService.UndoArchiveUnlinkedIsci(new List<long>() { 4286 }, DateTime.Now, "ApprovedTest");
                var result = _AffidavitService.GetUnlinkedIscis();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [UseReporter(typeof(DiffReporter))]
        public void GetClientScrubbingForProposalMultipleGenres()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _AffidavitService.GetClientScrubbingForProposal(255, scrubbingRequest);

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
                InboundFileSaveRequest affidavitSaveRequest = new InboundFileSaveRequest
                {
                    FileHash = "abc123",
                    Source = (int)DeliveryFileSourceEnum.Strata,
                    FileName = "test.file",
                    Details = new List<InboundFileSaveRequestDetail>()
                    {
                        new InboundFileSaveRequestDetail
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
                            InventorySource = DeliveryFileSourceEnum.Strata,
                            LeadOutGenre = "LeadOutGenre",
                            Affiliate = "Affiate",
                            Market = "market"
                        }
                    }
                };

                _AffidavitService.SaveAffidavit(affidavitSaveRequest, "TestUser", DateTime.Parse("06/29/2017 8:04AM"));

                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _AffidavitService.GetClientScrubbingForProposal(253, scrubbingRequest);

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
        [ExpectedException(typeof(Exception))]
        public void ClientScrubbingOverrides_Bad_ProposalId_Used()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var scrubs = _AffidavitService.GetClientScrubbingForProposal(253, scrubbingRequest);

                // grab second item and override it
                var scrubIds = new List<int>() { scrubs.ClientScrubs.First().ScrubbingClientId };

                ScrubStatusOverrideRequest overrides = new ScrubStatusOverrideRequest()
                {
                    ProposalId = 2543, // different proposal Id should throw exception
                    ScrubIds = scrubIds,
                    OverrideStatus = ScrubbingStatus.OutOfSpec
                };
                var result = _AffidavitService.OverrideScrubbingStatus(overrides);
            }
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ClientScrubbingOverrides()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var scrubs = _AffidavitService.GetClientScrubbingForProposal(253, scrubbingRequest);

                // grab second item and override it
                var scrubIds = new List<int>() { scrubs.ClientScrubs.Last().ScrubbingClientId };

                ScrubStatusOverrideRequest overrides = new ScrubStatusOverrideRequest()
                {
                    ProposalId = 253,
                    ScrubIds = scrubIds,
                    OverrideStatus = ScrubbingStatus.OutOfSpec
                };
                var result = _AffidavitService.OverrideScrubbingStatus(overrides);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailPostScrubbingDto), "ScrubbingClientId");
                jsonResolver.Ignore(typeof(ProposalDetailPostScrubbingDto), "SpotLengthId");

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
        public void ClientScrubbingOverrides_Override_Ignoring_Current_Status()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var scrubs = _AffidavitService.GetClientScrubbingForProposal(253, scrubbingRequest);

                // grab second item and override it
                var scrubIds = new List<int>() { scrubs.ClientScrubs.Last().ScrubbingClientId };

                ScrubStatusOverrideRequest overrides = new ScrubStatusOverrideRequest()
                {
                    ProposalId = 253,
                    ScrubIds = scrubIds,
                    OverrideStatus = ScrubbingStatus.InSpec // the current status of this record is InSpec, so this will not cause override.
                };
                var result = _AffidavitService.OverrideScrubbingStatus(overrides);

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
        public void ClientScrubbing_UndoOverride()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubIds = new List<int>() { 1010, 5614 };

                ScrubStatusOverrideRequest overrides = new ScrubStatusOverrideRequest()
                {
                    ProposalId = 26000,
                    ScrubIds = scrubIds
                };
                var result = _AffidavitService.UndoOverrideScrubbingStatus(overrides);

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
        public void SaveAffidavit_With_BlackListedIscis_BCOP3658()
        {
            using (new TransactionScopeWrapper())
            {
                _IsciService.BlacklistIscis(new List<string>() { ISCI1 }, "Test User");
                var request = _SetupAffidavit();

                var postingDate = new DateTime(2016, 4, 20);
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);

                // no scrubs?  Good!
                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Scrub_TimeDateMatch_PRI814()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(25999);
                proposal.Details[1].Quarters[0].Weeks[0].IsHiatus = true;
                _ProposalService.CalculateProposalChanges(new ProposalChangeRequest() {Details = proposal.Details});
                _ProposalService.SaveProposal(proposal,"test user",DateTime.Now);

                var request = _SetupAffidavit();
                request.Details[0].AirTime = DateTime.Parse("2016-06-06 9:04AM");
                request.Details[0].Isci = "FFFFFF6";

                var postingDate = new DateTime(2016, 4, 20);
                var result = _AffidavitService.SaveAffidavit(request, "test user", postingDate);
                VerifyAffidavit(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetClientScrubbingForProposal_WithEmptyPostingBook()
        {
            using (new TransactionScopeWrapper())
            {
                var scrubbingRequest = new ProposalScrubbingRequest();
                var result = _AffidavitService.GetClientScrubbingForProposal(256, scrubbingRequest);

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
    }
}
