using System;
using System.IO;
using System.Linq;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    public static class ProposalTestHelper
    {
        private static readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        private static readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();
        private static readonly IProposalRepository _ProposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
        private const string ProposalPickleTestName = "Pickle Rick Test";

        public static ProposalDto CreateProposal()
        {
            string json;
            ProposalDto proposal;

            using (var fileStream = new FileStream(".\\Files\\proposal_basic.json", FileMode.Open))
            {
                var reader = new StreamReader(fileStream);
                json = reader.ReadToEnd();
                proposal = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<ProposalDto>(json);
            }
            proposal.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
            proposal = _ProposalService.SaveProposal(proposal, "Integration user", DateTime.Now);

            OpenMarketAllocationSaveRequest allocationSaveRequest;

            using (var fileStream = new FileStream(string.Format(".\\Files\\proposal_basic_allocations.json"), FileMode.Open))
            {
                var reader = new StreamReader(fileStream);
                json = reader.ReadToEnd();
                allocationSaveRequest = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<OpenMarketAllocationSaveRequest>(json);
            }

            allocationSaveRequest.Username = "Integration Tester";
            allocationSaveRequest.ProposalVersionDetailId = proposal.Details.First().Id.Value;
            _ProposalOpenMarketInventoryService.SaveInventoryAllocations(allocationSaveRequest);

            return proposal;
        }
        
        public static int GetPickleProposalDetailId(ref ProposalDto proposal)
        {
            var proposalId = _ProposalService.GetAllProposals().First(p => p.ProposalName == ProposalPickleTestName).Id;
            proposal = _ProposalService.GetProposalById(proposalId);
            var proposalDetailId = proposal.Details.First().Id.Value;
            return proposalDetailId;
        }
    }

}
