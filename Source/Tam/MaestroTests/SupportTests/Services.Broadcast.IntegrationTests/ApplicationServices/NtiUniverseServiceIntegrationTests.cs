using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Nti;
using Services.Broadcast.Repositories;
using System;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class NtiUniverseServiceIntegrationTests
    {
        private const string IntegrationTestUser = "IntegrationTestUser";
        private readonly DateTime CreatedDate = new DateTime(2019, 5, 14);
        private readonly INtiUniverseService _NtiUniverseService = IntegrationTestApplicationServiceFactory.GetApplicationService<INtiUniverseService>();
        private readonly INtiUniverseRepository _NtiUniverseRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<INtiUniverseRepository>();
        private const int AUDIENCE_ID = 31;

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void LoadUniversesTest()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\LoadUniversesTest.xlsx";

                _NtiUniverseService.LoadUniverses(new FileStream(filename, FileMode.Open, FileAccess.Read), IntegrationTestUser, CreatedDate);

                var jsonSerializerSettings = _GetJsonSerializerSettingsForNTIUniverses();
                var loadedNsiUniverses = _NtiUniverseRepository.GetLatestLoadedNsiUniverses();
                var loadedNsiUniversesJson = IntegrationTestHelper.ConvertToJson(loadedNsiUniverses, jsonSerializerSettings);

                Approvals.Verify(loadedNsiUniversesJson);
            }
        }

        [Test]
        [Category("short_running")]
        public void GetNtiUniverse()
        {
            var ntiUniverse = _NtiUniverseService.GetLatestNtiUniverseByYear(AUDIENCE_ID, 2019);

            Assert.AreEqual(120600000, ntiUniverse);
        }

        [Test]
        [Category("short_running")]
        public void GetNtiUniverse_UsePreviousYear()
        {
            var ntiUniverse = _NtiUniverseService.GetLatestNtiUniverseByYear(AUDIENCE_ID, 2020);

            Assert.AreEqual(120600000, ntiUniverse);
        }

        [Test]
        [Category("short_running")]
        public void GetNtiUniverse_DoesntHaveNtiData()
        {
            Assert.That(() => _NtiUniverseService.GetLatestNtiUniverseByYear(AUDIENCE_ID, 2004), Throws.TypeOf<Exception>().With.Message.EqualTo("NTI universe not found."));
        }

        private JsonSerializerSettings _GetJsonSerializerSettingsForNTIUniverses()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(NtiUniverseHeader), "Id");
            jsonResolver.Ignore(typeof(NtiUniverseDetail), "Id");
            jsonResolver.Ignore(typeof(NtiUniverse), "Id");
            jsonResolver.Ignore(typeof(BroadcastAudience), "Id");

            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
    }
}
