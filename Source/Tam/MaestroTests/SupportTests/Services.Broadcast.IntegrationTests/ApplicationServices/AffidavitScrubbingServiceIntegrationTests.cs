using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class AffidavitScrubbingServiceIntegrationTests
    {
        private readonly IAffidavitScrubbingService _PostService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>();

        [Test]
        public void GetPostsTest()
        {
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_PostService.GetPosts()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetClientPostScrubbingProposalHeader()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostService.GetClientPostScrubbingProposalHeader(253);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ClientPostScrubbingProposalHeaderDto), "Id");
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
        public void GetClientPostScrubbingProposalDetail()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PostService.GetClientPostScrubbingProposalDetail(253, 14);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ClientPostScrubbingProposalDetailDto), "Id");

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
