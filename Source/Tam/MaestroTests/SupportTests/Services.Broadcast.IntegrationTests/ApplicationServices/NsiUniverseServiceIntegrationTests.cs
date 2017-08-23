using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class NsiUniverseServiceIntegrationTests
    {
        private readonly INsiUniverseService _NsiUniverseService = IntegrationTestApplicationServiceFactory.GetApplicationService<INsiUniverseService>();

        [Test]
        public void GetLatestSweepsMonth()
        {
            var sweepMediaMonth =
               IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
               .GetDataRepository<IPostingBookRepository>()
               .GetLatestPostableMediaMonth(BroadcastConstants.PostableMonthMarketThreshold);

            Assert.AreEqual(sweepMediaMonth, 422);
        }

        [Test]
        public void GetMarketUniversesByAudience()
        {
            const int marketCode = 100;
            const int hhId = 31;

            var sweepMediaMonth = 413;

            var universes = _NsiUniverseService.GetUniverseDataByAudience(hhId, sweepMediaMonth);

            Assert.AreEqual(380590.0d, universes[marketCode]);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetUniverseData()
        {
            const int hhId = 31;

            var sweepMediaMonth = 413;
            var universes = _NsiUniverseService.GetUniverseDataByAudience(hhId, sweepMediaMonth);

            ApprovalTests.Approvals.Verify(IntegrationTestHelper.ConvertToJson(universes));
        }
    }
}
