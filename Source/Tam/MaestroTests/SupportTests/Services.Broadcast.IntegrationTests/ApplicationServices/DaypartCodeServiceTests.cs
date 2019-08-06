using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class DaypartCodeServiceTests
    {
        private readonly IDaypartCodeService _DaypartCodeService = IntegrationTestApplicationServiceFactory.GetApplicationService<IDaypartCodeService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDaypartCodesTest()
        {
            var daypartCodes = _DaypartCodeService.GetAllDaypartCodes();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartCodes));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDaypartCodeDefaults()
        {
            using (new TransactionScopeWrapper())
            {
                var daypartDetaults = _DaypartCodeService.GetDaypartCodeDefaults();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartDetaults));
            }
        }
    }
}
