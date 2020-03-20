using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class AffiliateServiceIntegrationTests
    {
        private IAffiliateService _AffiliateService;

        [SetUp]
        public void Setup()
        {
            _AffiliateService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffiliateService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllAffiliates()
        {
            using (new TransactionScopeWrapper())
            {
                var affiliates = _AffiliateService.GetAllAffiliates();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(affiliates));
            }
        }
    }
}
