using NUnit.Framework;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.UnitTests.InventoryRatingsProcessing
{
    [Category("short_running")]
    public class MediaMonthCrunchStatusTests
    {
        [Test]
        public void NoMarkets()
        {
            var status = new RatingsForecastStatus();
            var result = new MediaMonthCrunchStatus(status, 0);
            Assert.AreEqual(result.Crunched, Entities.Enums.CrunchStatusEnum.NoMarkets);

        }

        [Test]
        public void Crunched()
        {
            var status = new RatingsForecastStatus
            {
                ViewerMarkets = 10,
                UsageMarkets = 10,
                UniverseMarkets = 10
            };

            var result = new MediaMonthCrunchStatus(status, 20);
            Assert.AreEqual(result.Crunched, Entities.Enums.CrunchStatusEnum.Crunched);
        }

        [Test]
        public void Incomplete()
        {
            var status = new RatingsForecastStatus
            {
                ViewerMarkets = 0,
                UsageMarkets = 10,
                UniverseMarkets = 0
            };

            var result = new MediaMonthCrunchStatus(status, 20);
            Assert.AreEqual(result.Crunched, Entities.Enums.CrunchStatusEnum.Incomplete);

        }

        [Test]
        public void NotCrunched()
        {
            var status = new RatingsForecastStatus
            {
                ViewerMarkets = 0,
                UsageMarkets = 0,
                UniverseMarkets = 0
            };

            var result = new MediaMonthCrunchStatus(status, 20);
            Assert.AreEqual(result.Crunched, Entities.Enums.CrunchStatusEnum.NotCrunched);
        }

    }
}
