using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class DaypartTypeServiceTests
    {
        private readonly IDaypartTypeService _DaypartTypeService = IntegrationTestApplicationServiceFactory.GetApplicationService<IDaypartTypeService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDaypartTypes()
        {
            using (new TransactionScopeWrapper())
            {
                var daypartTypes = _DaypartTypeService.GetDaypartTypes();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartTypes));
            }
        }
    }
}