using ApprovalTests;
using ApprovalTests.Reporters;
using EntityFrameworkMapping.Broadcast;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class MarketServiceIntegrationTests
    {
        private readonly IMarketService _MarketService = IntegrationTestApplicationServiceFactory.GetApplicationService<IMarketService>();
        private readonly IMarketCoverageRepository _MarketCoverageRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MarketService_CanUploadNewCoverageFile()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\Market_Coverages_2.xlsx";

                _MarketService.LoadCoverages(new FileStream(filename, FileMode.Open, FileAccess.Read), filename, "IntegrationTestUser", new DateTime(2018, 12, 18));

                var jsonSerializerSettings = _GetJsonSerializerSettingsForMarketCoverages();
                var loadedCoverages = _MarketCoverageRepository.GetAll();
                var loadedCoveragesJson = IntegrationTestHelper.ConvertToJson(loadedCoverages, jsonSerializerSettings);

                Approvals.Verify(loadedCoveragesJson);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Market coverage file already uploaded to the system")]
        public void MarketService_ThrowsException_IfUploadingFileAlreadyUploaded()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\Market_Coverages.xlsx";

                _MarketService.LoadCoverages(new FileStream(filename, FileMode.Open, FileAccess.Read), filename, "IntegrationTestUser", new DateTime(2018, 12, 18));

                var jsonSerializerSettings = _GetJsonSerializerSettingsForMarketCoverages();
                var loadedCoverages = _MarketCoverageRepository.GetAll();
                var loadedCoveragesJson = IntegrationTestHelper.ConvertToJson(loadedCoverages, jsonSerializerSettings);

                Approvals.Verify(loadedCoveragesJson);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Markets which were not found: Fargo, Cheyenne-Scottsbluff.")]
        public void MarketService_ThrowsException_IfLoadingFileWithNotExistingMarkets()
        {
            const string filename = @".\Files\Market_Coverages_With_Not_Existing_Markets.xlsx";

            _MarketService.LoadCoverages(new FileStream(filename, FileMode.Open, FileAccess.Read), filename, "IntegrationTestUser", new DateTime(2018, 12, 18));
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void MarketService_ThrowsException_IfCanNotLoadFile()
        {
            const string wrongFileName = @".\Files\WrongFile.xlsx";

            _MarketService.LoadCoverages(new FileStream(wrongFileName, FileMode.Open, FileAccess.Read), wrongFileName, "IntegrationTestUser", new DateTime(2018, 12, 18));
        }

        private JsonSerializerSettings _GetJsonSerializerSettingsForMarketCoverages()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(market_coverages), "market");
            jsonResolver.Ignore(typeof(MarketCoverage), "MarketCoverageFileId");

            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
    }
}
