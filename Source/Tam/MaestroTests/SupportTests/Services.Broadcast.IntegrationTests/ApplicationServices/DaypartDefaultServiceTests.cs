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
    public class StandardDaypartServiceTests
    {
        private readonly IStandardDaypartService _StandardDaypartService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStandardDaypartService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStandardDayparts()
        {
            var daypartCodes = _StandardDaypartService.GetAllStandardDayparts();

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StandardDaypartDto), "Id");
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartCodes, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStandardDaypartsWithAllData()
        {
            var standardDayparts = _StandardDaypartService.GetAllStandardDaypartsWithAllData();

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(StandardDaypartDto), "Id");
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(standardDayparts, jsonSettings));
        }
    }
}
