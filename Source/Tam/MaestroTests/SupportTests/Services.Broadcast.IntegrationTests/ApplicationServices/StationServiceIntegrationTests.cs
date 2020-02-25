using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class StationServiceIntegrationTests
    {
        private readonly IStationService _StationService;

        public StationServiceIntegrationTests()
        {
            _StationService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationService>();
        }

        [Test]
        public void CanLoadStationListFromForecastDatabase()
        {
            var results = _StationService.GetLatestNsiStationList();
            var latestMonth = _StationService.GetLatestStationDetailMediaMonthId();
            Assert.IsNotEmpty(results);
        }

        [Test]
        [TestCase("KOB", true)]
        [TestCase("", false)]
        [TestCase("INVALID", false)]
        public void CanCheckIfStationExistsByCallLetters(string stationCallLetters, bool expectedResult)
        {
            using (new TransactionScopeWrapper())
            {
                var result = _StationService.StationExistsInBroadcastDatabase(stationCallLetters);
                Assert.AreEqual(result, expectedResult);
            }
        }
    }
}
