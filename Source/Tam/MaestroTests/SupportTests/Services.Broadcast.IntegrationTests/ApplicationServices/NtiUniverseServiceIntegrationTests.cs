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

        [Test]
        [UseReporter(typeof(DiffReporter))]
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
