using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class DaypartCodeServiceTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDaypartCodesTest()
        {
            var daypartCodeService = IntegrationTestApplicationServiceFactory.GetApplicationService<IDaypartCodeService>();

            var daypartCodes = daypartCodeService.GetAllDaypartCodes();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartCodes));
        }
    }
}
