using ApprovalTests;
using EntityFrameworkMapping.Broadcast;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Repositories;
using System;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class MarketServiceIntegrationTests
    {
        private readonly IMarketService _MarketService = IntegrationTestApplicationServiceFactory.GetApplicationService<IMarketService>();
        private readonly IMarketCoverageRepository _MarketCoverageRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();

        [Test]
        public void MarketService_CanResetMarketCoverages()
        {
            const string filename = @".\Files\Market_Coverages.xlsx";

            _MarketService.LoadCoverages(filename);

            var jsonSerializerSettings = _GetJsonSerializerSettingsForMarketCoverages();
            var loadedCoverages = _MarketCoverageRepository.GetAll();
            var loadedCoveragesJson = IntegrationTestHelper.ConvertToJson(loadedCoverages, jsonSerializerSettings);

            Approvals.Verify(loadedCoveragesJson);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Markets which were not found: Fargo, Cheyenne-Scottsbluff.")]
        public void MarketService_ThrowsException_IfLoadingFileWithNotExistingMarkets()
        {
            const string filename = @".\Files\Market_Coverages_With_Not_Existing_Markets.xlsx";

            _MarketService.LoadCoverages(filename);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void MarketService_ThrowsException_IfCanNotLoadFile()
        {
            const string wrongFileName = @".\Files\WrongFile.xlsx";

            _MarketService.LoadCoverages(wrongFileName);
        }

        private JsonSerializerSettings _GetJsonSerializerSettingsForMarketCoverages()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(market_coverages), "market");

            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
    }
}
