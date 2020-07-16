using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Vpvh;
using System;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class VpvhServiceIntegrationTests
    {
        private const string IntegrationTestUser = "IntegrationTestUser";
        private readonly DateTime CreatedDate = new DateTime(2022, 5, 14);
        private readonly IVpvhService _VpvhService = IntegrationTestApplicationServiceFactory.GetApplicationService<IVpvhService>();

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public void LoadVpvhs_FileProcessSuccess()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

                _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx"
                    , IntegrationTestUser, CreatedDate);

                var quarters = _VpvhService.GetQuarters(new Entities.QuarterDto { Quarter = 1, Year = 2017 });

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(VpvhQuarter), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarters, jsonSettings));
            }
        }

        [Test]
        [Category("long_running")]
        public void LoadVpvhs_FileProcessFail()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\Vpvh\VPVH_invalidAudience.xlsx";

                Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_invalidAudience.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid audience."));
            }
        }

        [Test]
        [Category("long_running")]
        public void Export_GenerateFile()
        {
            var fileStream = _VpvhService.Export();

            Assert.IsNotNull(fileStream);
        }
    }
}
