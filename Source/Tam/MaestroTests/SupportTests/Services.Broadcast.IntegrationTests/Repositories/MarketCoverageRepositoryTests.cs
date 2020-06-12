using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class MarketCoverageRepositoryTests
    {
        readonly IMarketCoverageRepository _MarketRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetLatestTop100MarketCoveragesTest()
        {
            var top100Markets = _MarketRepository.GetLatestTop100MarketCoverages();

            Assert.AreEqual(100, top100Markets.MarketCoveragesByMarketCode.Count);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(top100Markets));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetLatestTop50MarketCoveragesTest()
        {
            var top50Markets = _MarketRepository.GetLatestTop50MarketCoverages();

            Assert.AreEqual(50, top50Markets.MarketCoveragesByMarketCode.Count);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(top50Markets));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetLatestTop25MarketCoveragesTest()
        {
            var top25Markets = _MarketRepository.GetLatestTop25MarketCoverages();

            Assert.AreEqual(25, top25Markets.MarketCoveragesByMarketCode.Count);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(top25Markets));
        }
    }
}
