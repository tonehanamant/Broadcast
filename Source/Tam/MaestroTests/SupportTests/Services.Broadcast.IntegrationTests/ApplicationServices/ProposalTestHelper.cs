using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.OpenMarketInventory;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    public static class ProposalTestHelper
    {
        private static readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
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
            var service = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
            proposal = service.SaveProposal(proposal, "Integration user", DateTime.Now);

            OpenMarketAllocationSaveRequest allocationSaveRequest;

            using (var fileStream = new FileStream(string.Format(".\\Files\\proposal_basic_allocations.json"), FileMode.Open))
            {
                var reader = new StreamReader(fileStream);
                json = reader.ReadToEnd();
                allocationSaveRequest = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<OpenMarketAllocationSaveRequest>(json);
            }

            var versionDetailId = proposal.Details.First().Id.Value;
            allocationSaveRequest.ProposalVersionDetailId = versionDetailId;
            var appService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();
            allocationSaveRequest.Username = "Integration Tester";
            appService.SaveInventoryAllocations(allocationSaveRequest);

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
