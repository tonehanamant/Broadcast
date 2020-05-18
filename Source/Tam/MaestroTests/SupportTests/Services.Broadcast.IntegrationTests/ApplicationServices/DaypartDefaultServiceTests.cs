using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class DaypartDefaultServiceTests
    {
        private readonly IDaypartDefaultService _DaypartDefaultService = IntegrationTestApplicationServiceFactory.GetApplicationService<IDaypartDefaultService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDaypartDefaults()
        {
            var daypartCodes = _DaypartDefaultService.GetAllDaypartDefaults();

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DaypartDefaultDto), "Id");
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartCodes, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDaypartDefaultsWithAllData()
        {
            var daypartDefaults = _DaypartDefaultService.GetAllDaypartDefaultsWithAllData();

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DaypartDefaultDto), "Id");
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartDefaults, jsonSettings));
        }
    }
}
