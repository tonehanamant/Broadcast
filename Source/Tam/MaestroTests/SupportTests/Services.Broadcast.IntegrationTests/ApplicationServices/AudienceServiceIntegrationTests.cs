using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class AudienceServiceIntegrationTests
    {
        private readonly IAudienceService _AudienceService;

        public AudienceServiceIntegrationTests()
        {
            _AudienceService = IntegrationTestApplicationServiceFactory.GetApplicationService<IAudienceService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAudienceTypes()
        {
            using (new TransactionScopeWrapper())
            {
                var audienceTypes = _AudienceService.GetAudienceTypes();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(audienceTypes));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                var audiences = _AudienceService.GetAudiences();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(audiences));
            }
        }
    }
}
