using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventorySummaryServiceTests
    {
        private IInventorySummaryService _InventoryCardService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventorySummaryService>();
        private IInventoryRepository _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySourcesTest()
        {
            var inventoryCards = _InventoryCardService.GetInventorySources();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryQuartersTest()
        {
            var inventoryCards = _InventoryCardService.GetInventoryQuarters(new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesInventorySourceTest()
        {
            var inventoryCards = _InventoryCardService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                InventorySourceId = 3,
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesInventorySourceNoDataTest()
        {
            var inventoryCards = _InventoryCardService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                InventorySourceId = 8
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesQuarterTest()
        {
            var inventoryCards = _InventoryCardService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                Quarter = new InventorySummaryQuarter
                {
                    Quarter = 1,
                    Year = 2019
                }
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesWithOpenMarketTest()
        {
            var request = new InventorySummaryFilterDto
            {
                Quarter = new InventorySummaryQuarter
                {
                    Quarter = 1,
                    Year = 2018
                }
            };

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(InventorySummaryDto), "InventorySourceId");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var openMarketCard = _InventoryCardService.GetInventorySummaries(request, new DateTime(2019, 04, 01)).Single(x => x.InventorySourceName == "Open Market");
            var openMarketCardJson = IntegrationTestHelper.ConvertToJson(openMarketCard, jsonSettings);

            Approvals.Verify(openMarketCardJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummaryOpenMarketTest()
        {
            var request = new InventorySummaryFilterDto
            {
                InventorySourceId = _InventoryRepository.GetInventorySourceByName("Open Market").Id,
                Quarter = new InventorySummaryQuarter
                {
                    Quarter = 1,
                    Year = 2018
                }
            };

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(InventorySummaryDto), "InventorySourceId");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var openMarketCard = _InventoryCardService.GetInventorySummaries(request, new DateTime(2019, 04, 01));
            var openMarketCardJson = IntegrationTestHelper.ConvertToJson(openMarketCard, jsonSettings);

            Approvals.Verify(openMarketCardJson);
        }
    }
}
