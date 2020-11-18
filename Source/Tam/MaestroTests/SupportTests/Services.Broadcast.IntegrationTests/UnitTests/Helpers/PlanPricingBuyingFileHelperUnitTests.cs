using NUnit.Framework;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class PlanPricingBuyingFileHelperUnitTests
    {
        [Test]
        [TestCase(null, null, "UNK", "UNK")]
        [TestCase(" ", " ", "UNK", "UNK")]
        [TestCase(" prod ", " 2 ", "prod", "v2")]
        [TestCase("dev", "3", "dev", "v3")]
        public void GetRequestFileName(string environment, string apiVersion, string expectedEnv, string expectedApiV)
        {
            const int planId = 122;
            const int jobId = 425;
            var currentDateTime = new DateTime(2020, 10, 20, 14, 59, 20);
            var expectedResult = $"{expectedEnv}_{expectedApiV}-request-122_425-20201020_145920.log";

            var result = PlanPricingBuyingFileHelper.GetRequestFileName(environment, planId, jobId, apiVersion, currentDateTime);

            Assert.AreEqual(expectedResult, result);
        }
    }
}