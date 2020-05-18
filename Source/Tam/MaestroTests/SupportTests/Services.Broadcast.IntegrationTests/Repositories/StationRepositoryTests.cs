using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    public class StationRepositoryTests
    {
        /// <summary>
        /// Gets the broadcast stations by market codes.
        /// </summary>
        [Test]
        public void GetBroadcastStationsByMarketCodes()
        {
            /*** Arrange ***/
            var marketCodes = new List<short> { 100, 101, 202 };

            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            List<DisplayBroadcastStation> result = null;

            /*** Act ***/
            using (new TransactionScopeWrapper())
            {
                result = repo.GetBroadcastStationsByMarketCodes(marketCodes);
            }

            /*** Assert ***/
            Assert.IsNotNull(result);
            // verify we got only the markets we asked for.
            // should have something for each market.
            var distinctMarkets = result.Where(s => s.MarketCode.HasValue).Select(s => s.MarketCode).Distinct().ToList();
            Assert.AreEqual(3, distinctMarkets.Count);
            Assert.IsTrue(distinctMarkets.Contains(100));
            Assert.IsTrue(distinctMarkets.Contains(101));
            Assert.IsTrue(distinctMarkets.Contains(202));
        }

        /// <summary>
        /// Test updating an unrated station to make it a rated station.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MakeUnratedStationIntoARatedStation()
        {
            const string testUnratedStationLegacyCallsign = "HOTF";
            const string testAffiliation = "ABC";
            const short testMarketCode = 123;
            const int testDistributorCode = 789;

            const string testUser = "MakeUnratedStationIntoARatedStation_TestUser";
            var testTime = new DateTime(2020, 04, 11, 1, 34, 26);

            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();

            // get the station 
            var testStation = repo.GetBroadcastStationByLegacyCallLetters(testUnratedStationLegacyCallsign);
            // verify it is 'unrated'
            Assert.IsNull(testStation.Affiliation);
            // make it rated
            testStation.Affiliation = testAffiliation;
            testStation.MarketCode = testMarketCode;
            testStation.Code = testDistributorCode;

            DisplayBroadcastStation savedStation;

            using (new TransactionScopeWrapper())
            {
                repo.UpdateStation(testStation, testUser, testTime);
                savedStation = repo.GetBroadcastStationByLegacyCallLetters(testUnratedStationLegacyCallsign);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedStation, _GetJsonSettings()));
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            return jsonSettings;
        }
    }
}