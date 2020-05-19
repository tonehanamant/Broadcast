using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
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
        public void LoadUniverses_FileProcessSuccess()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

                _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate);
            }
        }

        [Test]
        [Category("long_running")]
        public void LoadUniverses_FileProcessFail()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\Vpvh\VPVH_invalidAudience.xlsx";

                Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_invalidAudience.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid audience."));
            }
        }
    }
}
