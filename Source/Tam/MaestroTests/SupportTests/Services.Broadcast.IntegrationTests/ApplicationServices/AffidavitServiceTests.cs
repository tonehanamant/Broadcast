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
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();
        private readonly IAffidavitService _Sut;
        private readonly IAffidavitRepository _Repo;

        private readonly LookupDto Genre1 = new LookupDto() {Id = 13, Display = "Do It Yourself"};
        private readonly LookupDto Genre2 = new LookupDto() { Id = 47, Display = "Thriller & Suspense" };

        private readonly string ProgramName1 = "Program Names R us";

        public AffidavitServiceTests()
        {
            _Sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitService>();
            _Repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAdffidavit();

                int id = _Sut.SaveAffidavit(request,"test user",DateTime.Now);

                VerifyAffidavit(id);
            }
        }

        private void VerifyAffidavit(int id)
        {
            var affidavite = _Repo.GetAffidavit(id,true);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(AffidavitFile), "CreatedDate");
            jsonResolver.Ignore(typeof(AffidavitFile), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileDetail), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileDetail), "AffidavitFileId");
            jsonResolver.Ignore(typeof(AffidavitClientScrub), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileDetailAudience), "AffidavitFileDetailId");
            jsonResolver.Ignore(typeof(AffidavitClientScrub), "AffidavitFileDetailId");
            jsonResolver.Ignore(typeof(AffidavitClientScrub), "ModifiedDate");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(affidavite, jsonSettings);
            Approvals.Verify(json);
        }

        private AffidavitSaveRequest _SetupAdffidavit()
        {
            AffidavitSaveRequest request = new AffidavitSaveRequest();
            request.FileHash = "abc123";
            request.Source = (int) AffidaviteFileSource.Strata;
            request.FileName = "test.file";

            var detail = new AffidavitSaveRequestDetail();
            detail.AirTime = DateTime.Parse("06/29/2017 8:04AM");
            detail.Isci = "DDDDDDDD";
            detail.ProgramName = ProgramName1;
            detail.SpotLength = 30;
            detail.Genre = Genre1.Display;
            detail.Station = "WWSB";
            request.Details.Add(detail);
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

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416,413 );

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() {new ProposalWeekIsciDto() { Brand = "WAWA",ClientIsci = "WAWA",HouseIsci = "WAWA"} };
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", DateTime.Now);

                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations).SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;
                
                AllocationProgram(proposalDetailId, programId,proposal.FlightWeeks.First().MediaWeekId);
                
                var request = _SetupAdffidavit();
                request.Details.First().Isci = "WAWA";
                int id = _Sut.SaveAffidavit(request, "test user", DateTime.Now);
                VerifyAffidavit(id);
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

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA" } };
                proposal.Details.First().GenreCriteria.Add(new GenreCriteria() {Contain = ContainTypeEnum.Include, Genre = new LookupDto() {Id = Genre1.Id,Display= Genre1.Display} });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", DateTime.Now);

                var request = _SetupAdffidavit();
                request.Details.First().Isci = "WAWA";
                int id = _Sut.SaveAffidavit(request, "test user", DateTime.Now);
                VerifyAffidavit(id);
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

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA" } };
                proposal.Details.First().GenreCriteria.Add(new GenreCriteria() { Contain = ContainTypeEnum.Exclude, Genre = new LookupDto() { Id = Genre1.Id, Display = Genre1.Display } });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", DateTime.Now);

                var request = _SetupAdffidavit();
                request.Details.First().Isci = "WAWA";
                int id = _Sut.SaveAffidavit(request, "test user", DateTime.Now);
                VerifyAffidavit(id);
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

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA" } };
                proposal.Details.First().ProgramCriteria.Add(new ProgramCriteria() { Contain = ContainTypeEnum.Include, Program = new LookupDto {Display = ProgramName1, Id = 1 } });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", DateTime.Now);

                var request = _SetupAdffidavit();
                request.Details.First().Isci = "WAWA";
                int id = _Sut.SaveAffidavit(request, "test user", DateTime.Now);
                VerifyAffidavit(id);
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

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                proposal.Details.First().Quarters.First().Weeks.First().Iscis = new List<ProposalWeekIsciDto>() { new ProposalWeekIsciDto() { Brand = "WAWA", ClientIsci = "WAWA", HouseIsci = "WAWA" } };
                proposal.Details.First().ProgramCriteria.Add(new ProgramCriteria() { Contain = ContainTypeEnum.Exclude, Program = new LookupDto {Display = ProgramName1, Id = 100 } });
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "test user", DateTime.Now);

                var request = _SetupAdffidavit();
                request.Details.First().Isci = "WAWA";
                int id = _Sut.SaveAffidavit(request, "test user", DateTime.Now);
                VerifyAffidavit(id);
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
