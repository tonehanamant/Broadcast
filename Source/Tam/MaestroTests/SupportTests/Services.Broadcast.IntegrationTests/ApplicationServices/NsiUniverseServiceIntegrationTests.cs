using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class NsiUniverseServiceIntegrationTests
    {
        private readonly INsiUniverseService _NsiUniverseService = IntegrationTestApplicationServiceFactory.GetApplicationService<INsiUniverseService>();

        [Test]
        public void GetMarketUniversesByAudience()
        {
            const int marketCode = 100;
            const int hhId = 31;

            const int sweepMediaMonth = 413;

            var universes = _NsiUniverseService.GetUniverseDataByAudience(hhId, sweepMediaMonth);

            Assert.AreEqual(380590.0d, universes[marketCode]);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetUniverseData()
        {
            const int hhId = 31;

            const int sweepMediaMonth = 413;
            var universes = _NsiUniverseService.GetUniverseDataByAudience(hhId, sweepMediaMonth);

            ApprovalTests.Approvals.Verify(IntegrationTestHelper.ConvertToJson(universes));
        }
    }
}
