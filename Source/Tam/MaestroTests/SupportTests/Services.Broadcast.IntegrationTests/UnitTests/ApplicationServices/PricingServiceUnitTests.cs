using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    public class PricingServiceUnitTests
    {
        private PricingService _PricingService;

        [SetUp]
        public void Setup()
        {
            _PricingService = new PricingService();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetUnitCaps()
        {
            var unitCaps = _PricingService.GetUnitCaps();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(unitCaps));
        }
    }
}
