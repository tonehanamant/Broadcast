using System;
using System.IO;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.OpenMarketInventory;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    class ProposalScxConverterTest
    {
        public const int AdvertiserId = 12;
        public const int MainDemoId = 12;
        private IProposalService _ProposalService;
        private IProposalScxConverter _ProposalScxConverter; 

        public ProposalScxConverterTest()
        {
            _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
            _ProposalScxConverter = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalScxConverter>();
        }

        public ProposalDto Create_Proposal()
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
            proposal = _ProposalService.SaveProposal(proposal,"Integration user",DateTime.Now);

//            json = @"{""ProposalVersionDetailId"":394,""Weeks"":[{""MediaWeekId"":713,""Programs"":[{""ProgramId"":79,""Spots"":3,""Impressions"":184.63887499999996},{""ProgramId"":218,""Spots"":4,""Impressions"":0}]},{""MediaWeekId"":714,""Programs"":[{""ProgramId"":80,""Spots"":3,""Impressions"":48.3829375}]}],""Filter"":{""ProgramNames"":[],""Genres"":[],""DayParts"":[],""Affiliations"":[],""Markets"":[],""SpotFilter"":1}}";
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

        [Test]
        [Ignore]
        public void Test_ProposalScx()
        { // inspired by BCOP-1675
            using (new TransactionScopeWrapper())
            {
                var proposal = Create_Proposal();
                //var result1 = _ProposalScxConverter.ConvertProposal(proposal);
                var result = _ProposalService.GenerateScxFileArchive(proposal.Id.Value);
                using (var fileStream = new FileStream(string.Format("..\\File.zip"), FileMode.OpenOrCreate))
                {
                    result.Item2.CopyTo(fileStream);
                    fileStream.Close();
                }
            }
        }

        [Ignore]
        [Test]
        public void Tester_Bester()
        {
            const int proposalId = 270;

            var result = _ProposalService.GenerateScxFileArchive(proposalId);

            using (var fileStream = new FileStream(string.Format("..\\File.zip"), FileMode.OpenOrCreate))
            {
                result.Item2.CopyTo(fileStream);
                fileStream.Close();
            }
        }

    }
}
