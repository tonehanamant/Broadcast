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
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class AffidavitImpressionsServiceTests
    {
        private readonly IImpressionsService _AffidavitImpressionsService;

        public AffidavitImpressionsServiceTests()
        {
            _AffidavitImpressionsService = IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionsService>();
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
                _AffidavitImpressionsService.RecalculateImpressionsForProposalDetail(proposalDetailId.Value);

                var affidavitFile = affidavitRepository.GetAffidavit(167, true);

                VerifyAffidavit(affidavitFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateAffidavitImpressionsForAffidavitFile()
        {
            var affidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();
            var affidavitFile = affidavitRepository.GetAffidavit(166, true);

            _AffidavitImpressionsService.CalculateImpressionsForFileDetails(affidavitFile.FileDetails);

            VerifyAffidavit(affidavitFile);
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

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(affidavit, jsonSettings);
            Approvals.Verify(json);
        }
    }
}
