using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventorySummaryServiceTests
    {
        private readonly IInventorySummaryService _InventoryCardService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventorySummaryService>();
        private readonly IInventoryRepository _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        private readonly IDaypartCodeRepository _DaypartCodeRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDaypartCodeRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummaries_ProprietaryOAndO()
        {
            var inventoryCards = _InventoryCardService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                InventorySourceId = 11,
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummaryDetailsTest()
        {
            var inventoryCards = _InventoryCardService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                InventorySourceId = 4,
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryUnitsTest()
        {
            var inventorySourceId = 4; // TTWN
            var daypartCodeId = 1; // EMN
            var startDate = new DateTime(2019, 4, 1);
            var endDate = new DateTime(2019, 6, 30, 23, 59, 59);

            var units = _InventoryCardService.GetInventoryUnits(inventorySourceId, daypartCodeId, startDate, endDate);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(units));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryQuartersBySourceAndDaypartCodeTest()
        {
            var inventorySourceId = _InventoryRepository.GetInventorySourceByName("NBC O&O").Id;
            var daypartCodeId = _DaypartCodeRepository.GetDaypartCodeByCode("EMN").Id;

            var quarters = _InventoryCardService.GetInventoryQuarters(inventorySourceId, daypartCodeId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryQuartersBySourceAndDaypartCodeTest_NoData()
        {
            var inventorySourceId = _InventoryRepository.GetInventorySourceByName("NBC O&O").Id;
            var daypartCodeId = _DaypartCodeRepository.GetDaypartCodeByCode("DIGI").Id;

            var quarters = _InventoryCardService.GetInventoryQuarters(inventorySourceId, daypartCodeId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDaypartCodesTest()
        {
            var inventorySource = _InventoryRepository.GetInventorySourceByName("NBC O&O");
            var daypartCodes = _InventoryCardService.GetDaypartCodes(inventorySource.Id);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DaypartCodeDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartCodes, jsonSettings));
        }

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummaryFilterByDaypartCodeIdTest()
        {
            var inventoryCards = _InventoryCardService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                Quarter = new InventorySummaryQuarter
                {
                    Quarter = 1,
                    Year = 2019
                },
                DaypartCodeId = 1
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummarySourceTypes()
        {
            var sourceTypes = _InventoryCardService.GetInventorySourceTypes();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(sourceTypes));
        }

        [Test]
        [TestCase(InventorySourceTypeEnum.Barter)]
        [TestCase(InventorySourceTypeEnum.Diginet)]
        [TestCase(InventorySourceTypeEnum.OpenMarket)]
        [TestCase(InventorySourceTypeEnum.ProprietaryOAndO)]
        [TestCase(InventorySourceTypeEnum.Syndication)]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesFilterBySourceTypeTest(InventorySourceTypeEnum inventorySourceType)
        {
            using (ApprovalResults.ForScenario(inventorySourceType))
            {
                var inventoryCards = _InventoryCardService.GetInventorySummaries(new InventorySummaryFilterDto
                {
                    Quarter = new InventorySummaryQuarter
                    {
                        Quarter = 1,
                        Year = 2018
                    },
                    InventorySourceType = inventorySourceType
                }, new DateTime(2019, 04, 01));

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
            }
        }
    }
}
