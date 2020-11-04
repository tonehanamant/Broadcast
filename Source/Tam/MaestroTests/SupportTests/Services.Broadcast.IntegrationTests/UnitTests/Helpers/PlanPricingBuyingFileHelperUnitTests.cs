using NUnit.Framework;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class PlanPricingBuyingFileHelperUnitTests
    {
        [Test]
        public void GetRequestFileName()
        {
            const int planId = 122;
            const int jobId = 425;
            const string environment = "prod";
            var currentDateTime = new DateTime(2020, 10, 20, 14, 59, 20);
            const string expectedResult = "prod-request-122_425-20201020_145920.log";

            var result = PlanPricingBuyingFileHelper.GetRequestFileName(environment, planId, jobId, currentDateTime);

            Assert.AreEqual(expectedResult, result);
        }
    }
}