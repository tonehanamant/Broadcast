using System;
using System.IO;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Entities.spotcableXML;
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


        [Test]
        [Ignore]
        public void Test_ProposalScx()
        { // inspired by BCOP-1675
            using (new TransactionScopeWrapper())
            {
                var proposal = ProposalTestHelper.CreateProposal();
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

            using (var fileStream = new FileStream(string.Format("c:\\scxFile.zip"), FileMode.OpenOrCreate))
            {
                result.Item2.CopyTo(fileStream);
                fileStream.Close();
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Test_Proposal_Scx_Converter()
        {
            using (new TransactionScopeWrapper())
            {
                const int proposalId = 270;
                var proposal = ProposalTestHelper.CreateProposal();
                var result = _ProposalScxConverter.BuildFromProposalDetail(proposal,proposal.Details.First());

                var jsonResolver = new IgnorableSerializerContractResolver();
                // remove start/end times because they are current date dependant
                jsonResolver.Ignore(typeof(detailLine), "startTime");
                jsonResolver.Ignore(typeof(detailLine), "endTime");
                jsonResolver.Ignore(typeof(document), "date");

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
