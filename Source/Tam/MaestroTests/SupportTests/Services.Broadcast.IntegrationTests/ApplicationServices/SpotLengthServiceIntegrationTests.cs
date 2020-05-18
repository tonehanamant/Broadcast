using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class SpotLengthServiceIntegrationTests
    {
        private readonly ISpotLengthService _SpotLengthService;

        public SpotLengthServiceIntegrationTests()
        {
            _SpotLengthService = IntegrationTestApplicationServiceFactory.GetApplicationService<ISpotLengthService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAll()
        {
            using (new TransactionScopeWrapper())
            {
                var spotLengths = _SpotLengthService.GetAllSpotLengths();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(spotLengths));
            }
        }
    }
}
