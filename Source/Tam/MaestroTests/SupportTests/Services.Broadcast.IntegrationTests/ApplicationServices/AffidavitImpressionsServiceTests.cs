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
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitImpressionsServiceTests
    {
        private readonly IAffidavitImpressionsService _AffidavitImpressionsService;

        public AffidavitImpressionsServiceTests()
        {
            _AffidavitImpressionsService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitImpressionsService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void RecalculateAffidavitImpressionsForProposalDetailTest()
        {
            var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IProposalRepository>();
            var affidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();
            var proposal = proposalRepository.GetProposalById(26006);
            var proposalDetailId = proposal.Details.First().Id;

            using (new TransactionScopeWrapper())
            {
                _AffidavitImpressionsService.RecalculateAffidavitImpressionsForProposalDetail(proposalDetailId.Value);

                var affidavitFile = affidavitRepository.GetAffidavit(167, true);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(affidavitFile));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateAffidavitImpressionsForAffidavitFile()
        {
            var affidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();
            var affidavitFile = affidavitRepository.GetAffidavit(166, true);

            _AffidavitImpressionsService.CalculateAffidavitImpressionsForAffidavitFile(affidavitFile);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(affidavitFile));
        }
    }
}
