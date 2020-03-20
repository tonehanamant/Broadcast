using ApprovalTests;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using ApprovalTests.Reporters;
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

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            return jsonSettings;
        }
    }
}