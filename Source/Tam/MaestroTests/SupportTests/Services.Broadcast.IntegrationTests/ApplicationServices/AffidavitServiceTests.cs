using ApprovalTests;
using ApprovalTests.Reporters;
using EntityFrameworkMapping.Broadcast;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitServiceTests
    {
        private readonly IProposalService _ProposalService ;
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();
        private readonly IAffidavitService _Sut;
        private readonly IAffidavitRepository _Repo;

        private readonly LookupDto Genre1 = new LookupDto() { Id = 13, Display = "Do It Yourself" };
        private readonly LookupDto Genre2 = new LookupDto() { Id = 47, Display = "Thriller & Suspense" };

        private readonly string ProgramName1 = "Program Names R us";

        public AffidavitServiceTests()
        {
            _Sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitService>();
            _Repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();

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
                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(AffidavitSaveResult), "Id");

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
                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

                VerifyAffidavit(result);
            }
        }

        private void VerifyAffidavit(AffidavitSaveResult result)
        {
            if (result.ValidationResults.Any() && !result.Id.HasValue)
            {
                var msg = AffidavitValidationResult.FormatValidationMessage(result.ValidationResults);
                Assert.IsTrue(!result.ValidationResults.Any(), msg);
            }

            Assert.IsTrue(result.Id.HasValue, result.ToString());

            AffidavitFile affidavit;
            affidavit = _Repo.GetAffidavit(result.Id.Value, true);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(AffidavitFile), "CreatedDate");
            jsonResolver.Ignore(typeof(AffidavitFile), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileDetail), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileDetail), "AffidavitFileId");
            jsonResolver.Ignore(typeof(AffidavitClientScrub), "Id");
            jsonResolver.Ignore(typeof(AffidavitClientScrubAudience), "AffidavitClientScrubId");
            jsonResolver.Ignore(typeof(AffidavitClientScrub), "AffidavitFileDetailId");
            jsonResolver.Ignore(typeof(AffidavitClientScrub), "ModifiedDate");
            jsonResolver.Ignore(typeof(AffidavitFile), "MediaMonthId");
            jsonResolver.Ignore(typeof(AffidavitFileProblem), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileProblem), "AffidavitFileId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(affidavit, jsonSettings);
            Approvals.Verify(json);
        }

        private AffidavitSaveRequest _SetupAffidavitMultipleIscis()
        {
            AffidavitSaveRequest request = new AffidavitSaveRequest
            {
                FileHash = "abc123",
                Source = (int)AffidaviteFileSourceEnum.Strata,
                FileName = "test.file"
            };

            var detail = new AffidavitSaveRequestDetail
            {
                AirTime = DateTime.Parse("06/08/2017 8:04AM"),
                Isci = "FFFFFF",
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
                InventorySource = AffidaviteFileSourceEnum.Strata,
                LeadOutGenre = "LeadOutGenre",
                Affiliate = "Affiate"
            };

            request.Details.Add(detail);

            return request;
        }

        private AffidavitSaveRequest _SetupAffidavit()
        {
            AffidavitSaveRequest request = new AffidavitSaveRequest
            {
                FileHash = "abc123",
                Source = (int)AffidaviteFileSourceEnum.Strata,
                FileName = "test.file",
                Details = new List<AffidavitSaveRequestDetail>()
                {
                    new AffidavitSaveRequestDetail
                    {
                        AirTime = DateTime.Parse("06/29/2017 8:04AM"),
                        Isci = "DDDDDDDD",
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
                        InventorySource = AffidaviteFileSourceEnum.Strata,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }
                }
            };
            return request;
        }

        private AffidavitSaveRequest _SetupAffidavit_WithEscaped_Doublequotes()
        {
            AffidavitSaveRequest request = new AffidavitSaveRequest
            {
                FileHash = "abc123",
                Source = (int)AffidaviteFileSourceEnum.Strata,
                FileName = "test.file",
                Details = new List<AffidavitSaveRequestDetail>()
                {
                    new AffidavitSaveRequestDetail
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
                        InventorySource = AffidaviteFileSourceEnum.Strata,
                        LeadOutGenre = "LeadOutGenre",
                        Affiliate = "Affiate",
                        Market = "market"
                    }
                }
            };
            return request;
        }

        private AffidavitSaveRequest _SetupBigAffidavit()
        {
            AffidavitSaveRequest request = new AffidavitSaveRequest
            {
                FileHash = "abc123",
                Source = (int)AffidaviteFileSourceEnum.Strata,
                FileName = "test.file",
                Details = new List<AffidavitSaveRequestDetail>(){ new AffidavitSaveRequestDetail
                    {
                        AirTime = DateTime.Parse("05/30/2016 8:00AM"),
                        Isci = "DDDDDDDD",
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
                        InventorySource = AffidaviteFileSourceEnum.Strata,
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
                "DDDDDDDD",
                "EEEEEEEEEEE",
                "FFFFFF"
            };
            int maxTimeAdd = 60 * 59;
            // first 3 stations of the top 16 markets
            var stations = new List<string>()
            {
                "WFUT","HPIX","WNJN","OTLA","NKJR","HMM","ESNS","WESV","GGN","WNJS","RDZN",
                "GPHL","QUBO","NKJR","FBN","RLZC","HMM","NKJR","WPXW","NKJR","GDCW","NKJR",
                "GENH","WBPX","NKJR","FBN","HMM","NTMD","KUVM","QUBO","NKJR","FBN","GMOR",
                "KTVW","KFPH","KPNX","NKJR","QUBO","FBN","NKJR","KBTC","FBN","KSTP","KAWE",
                "WCCO","HMM","HSTE","NKJR","KDVR","NKJR",
            };

            for (int c = 0; c < 102; c++)
            {
                var airTime = DateTime.Parse("2016-05-30 8:01AM").AddSeconds(c % maxTimeAdd).AddDays(c % 5);
                var isci = iscis[c % iscis.Count];
                var station = stations[c % stations.Count];
                request.Details.Add(new AffidavitSaveRequestDetail()
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
                    InventorySource = AffidaviteFileSourceEnum.Strata,
                    LeadOutGenre = "LeadOutGenre",
                    Affiliate = "Affiate",
                    Market = "market"
                });
            }

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

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 413, 416);

                //????var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA", Days = "M|T|W|TH|F|SA|SU" } };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                //????var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations).SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                //???? AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _Sut.SaveAffidavit(request, "test user", postingDate);
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

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA" } };
                proposal.Details.First().GenreCriteria.Add(new GenreCriteria() { Contain = ContainTypeEnum.Include, Genre = new LookupDto() { Id = Genre1.Id, Display = Genre1.Display } });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _Sut.SaveAffidavit(request, "test user", postingDate);
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

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA" } };
                proposal.Details.First().GenreCriteria.Add(new GenreCriteria() { Contain = ContainTypeEnum.Exclude, Genre = new LookupDto() { Id = Genre1.Id, Display = Genre1.Display } });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                var detail = request.Details.First();
                detail.Isci = "WAWA";
                detail.LeadOutGenre = detail.Genre;
                detail.LeadOutProgramName = detail.ProgramName;
                var result = _Sut.SaveAffidavit(request, "test user", postingDate);
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

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 413, 416);

                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                var postingDate = new DateTime(2016, 4, 20);
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.Add(new AffidavitSaveRequestDetail
                {
                    AirTime = DateTime.Parse("06/29/2017 8:04AM"),
                    Isci = "DDDDDDDD",
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
                    InventorySource = AffidaviteFileSourceEnum.Strata,
                    LeadOutGenre = "LeadOutGenre",
                    Affiliate = "Affiate"
                });
                request.Details.First().Isci = "Uknown_Unmatched_ISCI";
                var result = _Sut.SaveAffidavit(request, "test user", postingDate);
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

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA" } };
                proposal.Details.First().ProgramCriteria.Add(new ProgramCriteria() { Contain = ContainTypeEnum.Include, Program = new LookupDto { Display = ProgramName1, Id = 1 } });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _Sut.SaveAffidavit(request, "test user", postingDate);
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

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA" } };
                proposal.Details.First().ProgramCriteria.Add(new ProgramCriteria() { Contain = ContainTypeEnum.Exclude, Program = new LookupDto { Display = ProgramName1, Id = 100 } });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _Sut.SaveAffidavit(request, "test user", postingDate);
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

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA", Days = "M" } };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations).SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                var request = _SetupAffidavit();
                request.Details.First().Isci = "WAWA";
                var result = _Sut.SaveAffidavit(request, "test user", postingDate);
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
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();

                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA", Days = "TH" } };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations).SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                var request = _SetupAffidavit();

                request.Details.First().Isci = "WAWA";

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                var postingDate = new DateTime(2016, 4, 20);
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA", Days = "TH" } };

                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;

                // Daypart that does not include thurday (the airtime for the affidavit).
                proposal.Details.First().DaypartId = 88;

                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations).SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                var request = _SetupAffidavit();

                request.Details.First().Isci = "WAWA";

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                var postingDate = new DateTime(2016, 4, 20);
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);

                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA", Days = "M" } };

                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;

                // Daypart that does not include thurday (the airtime for the affidavit).
                proposal.Details.First().DaypartId = 88;

                _ProposalService.SaveProposal(proposal, "test user", postingDate);

                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations).SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                var request = _SetupAffidavit();

                request.Details.First().Isci = "WAWA";

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                var result = _Sut.SaveAffidavit(request, "test user", postingDate);

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

                List<AffidavitValidationResult> validationResults = new List<AffidavitValidationResult>();
                validationResults.Add(new AffidavitValidationResult()
                {
                    ErrorMessage = "Generic error message",
                    InvalidField = "ErrorField",
                    InvalidLine = 255
                });
                var postingDate = new DateTime(2016, 4, 20);
                var result = _Sut.SaveAffidavitValidationErrors(request, "test user", validationResults);

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
                var result = _Sut.ScrubUnlinkedAffidavitDetailsByIsci(request.Isci, DateTime.Now, "test-user");

                var affidavit = _Repo.GetAffidavit(157, true);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(AffidavitFile), "CreatedDate");
                jsonResolver.Ignore(typeof(AffidavitFile), "Id");
                jsonResolver.Ignore(typeof(AffidavitFileDetail), "Id");
                jsonResolver.Ignore(typeof(AffidavitFileDetail), "AffidavitFileId");
                jsonResolver.Ignore(typeof(AffidavitClientScrub), "Id");
                jsonResolver.Ignore(typeof(AffidavitClientScrubAudience), "AffidavitClientScrubId");
                jsonResolver.Ignore(typeof(AffidavitClientScrub), "AffidavitFileDetailId");
                jsonResolver.Ignore(typeof(AffidavitClientScrub), "ModifiedDate");
                jsonResolver.Ignore(typeof(AffidavitFile), "MediaMonthId");
                jsonResolver.Ignore(typeof(AffidavitFileProblem), "Id");
                jsonResolver.Ignore(typeof(AffidavitFileProblem), "AffidavitFileId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(affidavit, jsonSettings);
                Approvals.Verify(json);
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
                var result = _Sut.MapIsci(request, DateTime.Now, "test-user");

                var affidavit = _Repo.GetAffidavit(157, true);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(AffidavitFile), "CreatedDate");
                jsonResolver.Ignore(typeof(AffidavitFile), "Id");
                jsonResolver.Ignore(typeof(AffidavitFileDetail), "Id");
                jsonResolver.Ignore(typeof(AffidavitFileDetail), "AffidavitFileId");
                jsonResolver.Ignore(typeof(AffidavitClientScrub), "Id");
                jsonResolver.Ignore(typeof(AffidavitClientScrubAudience), "AffidavitClientScrubId");
                jsonResolver.Ignore(typeof(AffidavitClientScrub), "AffidavitFileDetailId");
                jsonResolver.Ignore(typeof(AffidavitClientScrub), "ModifiedDate");
                jsonResolver.Ignore(typeof(AffidavitFile), "MediaMonthId");
                jsonResolver.Ignore(typeof(AffidavitFileProblem), "Id");
                jsonResolver.Ignore(typeof(AffidavitFileProblem), "AffidavitFileId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(affidavit, jsonSettings);
                Approvals.Verify(json);
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

    }
}
