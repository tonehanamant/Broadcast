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

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitServiceTests
    {
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();
        private readonly IAffidavitService _Sut;
        private readonly IAffidavitRepository _Repo;

        public AffidavitServiceTests()
        {
            _Sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitService>();
            _Repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();
        }
        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService()
        {
            using (new TransactionScopeWrapper())
            {
                var request = _SetupAdffidavit();

                int id = _Sut.SaveAffidavit(request);

                var affidavite = _Repo.GetAffidavit(id);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(affidavit_files), "created_date");
                jsonResolver.Ignore(typeof(affidavit_files), "id");
                jsonResolver.Ignore(typeof(affidavit_file_details), "id");
                jsonResolver.Ignore(typeof(affidavit_file_details), "affidavit_client_scrubs");
                jsonResolver.Ignore(typeof(affidavit_file_details), "affidavit_file_id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(affidavite, jsonSettings);
                Approvals.Verify(json);
            }
        }

        private static AffidavitSaveRequest _SetupAdffidavit()
        {
            AffidavitSaveRequest request = new AffidavitSaveRequest();
            request.FileHash = "abc123";
            request.Source = (int) AffidaviteFileSource.Strata;
            request.FileName = "test.file";

            var detail = new AffidavitSaveRequestDetail();
            detail.AirTime = DateTime.Parse("12/29/2018 10:04AM");
            detail.Isci = "ISCI";
            detail.ProgramName = "Programs R Us";
            detail.SpotLength = 30;
            detail.Station = "WNBC";
            request.Details.Add(detail);
            return request;
        }

        [Ignore]
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
                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations).SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;
                
                AllocationProgram(proposalDetailId, programId,proposal.FlightWeeks.First().MediaWeekId);
                
                var request = _SetupAdffidavit();
                int id = _Sut.SaveAffidavit(request);
                
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
