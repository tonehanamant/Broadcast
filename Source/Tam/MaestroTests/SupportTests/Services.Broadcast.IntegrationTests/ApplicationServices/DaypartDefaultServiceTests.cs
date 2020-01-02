using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class DaypartDefaultServiceTests
    {
        private readonly IDaypartDefaultService _DaypartDefaultService = IntegrationTestApplicationServiceFactory.GetApplicationService<IDaypartDefaultService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDaypartDefaults()
        {
            var daypartCodes = _DaypartDefaultService.GetAllDaypartDefaults();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartCodes));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDaypartDefaultsWithAllData()
        {
            var daypartDefaults = _DaypartDefaultService.GetAllDaypartDefaultsWithAllData();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartDefaults));
        }
    }
}
