using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Repositories;
using System;
using System.IO;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Microsoft.Practices.Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventorySummaryServiceTests
    {
        private readonly IInventorySummaryService _InventorySummaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventorySummaryService>();
        private readonly IInventoryRepository _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        private readonly IDaypartCodeRepository _DaypartCodeRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDaypartCodeRepository>();       
        private IProprietaryInventoryService _ProprietaryService;
        private IInventoryRepository _IInventoryRepository;
        private IInventoryRatingsProcessingService _InventoryRatingsProcessingService;
        private IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;

        [TestFixtureSetUp]
        public void init()
        {
            try
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
                _ProprietaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>();
                _IInventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
                _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
                _InventoryFileRatingsJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRatingsJobsRepository>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesProprietaryOAndO()
        {
            var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                InventorySourceId = 11,
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummaryDetailsTest()
        {
            const string fileName = @"ProprietaryDataFiles\Barter_ValidFormat_SingleBook_ShortDateRange.xlsx";

            using (new TransactionScopeWrapper())
            {
                var request = new InventoryFileSaveRequest
                {
                    StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var result = _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", new DateTime(2019, 02, 02));
                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.id.Value);

                var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
                {
                    InventorySourceId = 4,
                }, new DateTime(2019, 04, 01));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventorySummaryDto), "LastUpdatedDate");
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards, jsonSerializerSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryUnitsTest()
        {
            var inventorySourceId = 4; // TTWN
            var daypartCodeId = 1; // EMN
            var startDate = new DateTime(2019, 4, 1);
            var endDate = new DateTime(2019, 6, 30, 23, 59, 59);

            var units = _InventorySummaryService.GetInventoryUnits(inventorySourceId, daypartCodeId, startDate, endDate);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(units));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryQuartersBySourceAndDaypartCodeTest()
        {
            var inventorySourceId = _InventoryRepository.GetInventorySourceByName("NBC O&O").Id;
            var daypartCodeId = _DaypartCodeRepository.GetDaypartCodeByCode("EMN").Id;

            var quarters = _InventorySummaryService.GetInventoryQuarters(inventorySourceId, daypartCodeId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryQuartersBySourceAndDaypartCodeTest_NoData()
        {
            var inventorySourceId = _InventoryRepository.GetInventorySourceByName("NBC O&O").Id;
            var daypartCodeId = _DaypartCodeRepository.GetDaypartCodeByCode("DIGI").Id;

            var quarters = _InventorySummaryService.GetInventoryQuarters(inventorySourceId, daypartCodeId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarters));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDaypartCodesTest()
        {
            var inventorySource = _InventoryRepository.GetInventorySourceByName("NBC O&O");
            var daypartCodes = _InventorySummaryService.GetDaypartCodes(inventorySource.Id);

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
            var inventoryCards = _InventorySummaryService.GetInventorySources();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryQuartersTest()
        {
            var inventoryCards = _InventorySummaryService.GetInventoryQuarters(new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesInventorySourceTest()
        {
            var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                InventorySourceId = 3,
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesInventorySourceNoDataTest()
        {
            var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                InventorySourceId = 8
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesQuarterTest()
        {
            var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
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

            var openMarketCard = _InventorySummaryService.GetInventorySummaries(request, new DateTime(2019, 04, 01)).Single(x => x.InventorySourceName == "Open Market");
            var openMarketCardJson = IntegrationTestHelper.ConvertToJson(openMarketCard);

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

            var openMarketCard = _InventorySummaryService.GetInventorySummaries(request, new DateTime(2019, 04, 01));
            var openMarketCardJson = IntegrationTestHelper.ConvertToJson(openMarketCard);

            Approvals.Verify(openMarketCardJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummariesWithSyndicationTest()
        {
            var request = new InventorySummaryFilterDto
            {
                Quarter = new InventorySummaryQuarter
                {
                    Quarter = 1,
                    Year = 2018
                }
            };

            var cards = _InventorySummaryService.GetInventorySummaries(request, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(cards));
        }

        [Test]
        [TestCase("20th Century Fox (Twentieth Century)")]
        [TestCase("CBS Synd")]
        [TestCase("NBCU Syn")]
        [TestCase("WB Syn")]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummary_Syndication(string inventorySourceName)
        {
            using (ApprovalResults.ForScenario(inventorySourceName))
            {
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceName);
                var request = new InventorySummaryFilterDto
                {
                    InventorySourceId = inventorySource.Id,
                    Quarter = new InventorySummaryQuarter
                    {
                        Quarter = 1,
                        Year = 2018
                    }
                };

                var card = _InventorySummaryService.GetInventorySummaries(request, new DateTime(2019, 04, 01));
                var cardJson = IntegrationTestHelper.ConvertToJson(card);

                Approvals.Verify(cardJson);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventorySummaryFilterByDaypartCodeIdTest()
        {
            var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
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
            var sourceTypes = _InventorySummaryService.GetInventorySourceTypes();

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
                var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
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
