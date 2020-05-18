using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Common.Services;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Extensions;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventorySummaryServiceTests
    {
        private readonly ILogoService _LogoService = IntegrationTestApplicationServiceFactory.GetApplicationService<ILogoService>();
        private readonly IInventorySummaryService _InventorySummaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventorySummaryService>();
        private readonly IInventorySummaryCache _InventorySummaryCache = IntegrationTestApplicationServiceFactory.Instance.Resolve<IInventorySummaryCache>();
        private readonly IInventoryRepository _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
        private readonly IInventoryExportRepository _InventoryExportRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryExportRepository>();

        private InventoryFileTestHelper _InventoryFileTestHelper;
        private int nbcOAndO_InventorySourceId = 0;

        [TestFixtureSetUp]
        public void Init()
        {
            try
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
                nbcOAndO_InventorySourceId = _InventoryRepository.GetInventorySourceByName("NBC O&O").Id;
                _InventoryFileTestHelper = new InventoryFileTestHelper();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventorySummaries_CNN_WithLogo()
        {
            const string fileName = @"CNN.jpg";
            const int inventorySourceId = 5;
            const string createdBy = "IntegrationTestUser";

            using (new TransactionScopeWrapper())
            {
                var now = new DateTime(2019, 02, 02);
                var request = new FileRequest
                {
                    RawData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read).ToBase64String(),
                    FileName = fileName
                };

                _LogoService.SaveInventoryLogo(inventorySourceId, request, createdBy, now);

                var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
                {
                    InventorySourceId = inventorySourceId,
                }, new DateTime(2019, 04, 01));

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventorySummaries_OpenMarket()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryCards = _GetOpenMarketSummaryForQuarter(2018, 1)
                              .Union(_GetOpenMarketSummaryForQuarter(2018, 2));

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

        private IEnumerable<InventorySummaryDto> _GetOpenMarketSummaryForQuarter(int year, int quarter)
        {
            return _InventorySummaryService.GetInventorySummaries(
                new InventorySummaryFilterDto
                {
                    Quarter = new QuarterDto { Year = year, Quarter = quarter }
                }, new DateTime(2018, 1, 15))
                .Where(x => x.InventorySourceId == 1);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void SavesDiginetInventoryFileManifests_WithDaypartCodes_WithoutFullHours()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
                {
                    InventorySourceId = 23,
                }, new DateTime(2018, 10, 02));

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
        [Category("short_running")]
        public void GetInventorySummaries_Diginet()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
                {
                    InventorySourceId = 23,
                }, new DateTime(2018, 10, 02));

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
        [Category("short_running")]
        public void GetInventorySummaries_ProprietaryOAndO()
        {
            var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                InventorySourceId = 11,
            }, new DateTime(2019, 07, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetInventorySummaries_ProprietaryOAndO_WithCache()
        {
            const int inventorySourceId = 10;

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("O&O ABC O&O 3Q2021.xlsx", new DateTime(2019, 07, 20));
                _InventorySummaryService.AggregateInventorySummaryData(new List<int> { inventorySourceId });

                var inventoryCards = _InventorySummaryService.GetInventorySummariesWithCache(new InventorySummaryFilterDto
                {
                    InventorySourceId = inventorySourceId,
                }, new DateTime(2019, 07, 01));

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
        [Category("long_running")]
        public void AggregateInventorySummaryData_Syndication()
        {
            const int inventorySourceId = 13;
            const string testFileName = "Syndication_ValidFile2.xlsx";
            var currentDateTime = new DateTime(2020, 01, 27);

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile(testFileName, currentDateTime);
                _InventorySummaryService.AggregateInventorySummaryData(new List<int> { inventorySourceId });

                var inventoryCards = _InventorySummaryService.GetInventorySummariesWithCache(new InventorySummaryFilterDto
                {
                    InventorySourceId = inventorySourceId,
                }, currentDateTime);

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
        [Category("long_running")]
        public void AggregateInventorySummaryData_Syndication_AggregateAffectedDates()
        {
            const int inventorySourceId = 13;
            const string testFileName1 = "Syndication_ValidFile6.xlsx";
            const string testFileName2 = "Syndication_ValidFile7.xlsx";
            var currentDateTime = new DateTime(2019, 05, 1);

            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile(testFileName1);
                _InventorySummaryService.AggregateInventorySummaryData(new List<int> { inventorySourceId });

                var inventoriesBeforeAggregation = _InventorySummaryService.GetInventorySummariesWithCache(new InventorySummaryFilterDto
                {
                    InventorySourceId = inventorySourceId,
                }, currentDateTime);

                var inventorySummariesPreUpdate = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
                {
                    Quarter = new QuarterDto
                    {
                        Quarter = 2,
                        Year = 2019
                    },
                    InventorySourceType = InventorySourceTypeEnum.Syndication,
                    InventorySourceId = inventorySourceId
                }, currentDateTime);

                var exportableQuarterSummariesBefore = _InventoryExportRepository.GetInventoryQuartersForSource(inventorySourceId);

                var quarterPreUpdate = inventorySummariesPreUpdate.FirstOrDefault(i => i.Quarter.Quarter == 2 && i.Quarter.Year == 2019);

                Thread.Sleep(1000);

                _InventoryFileTestHelper.UploadProprietaryInventoryFile(testFileName2);
                _InventorySummaryService.AggregateInventorySummaryData(new List<int> { inventorySourceId }, new DateTime(2019, 5, 2), new DateTime(2019, 5, 5));

                var inventorySummariesPostUpdate = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
                {
                    Quarter = new QuarterDto
                    {
                        Quarter = 2,
                        Year = 2019
                    },
                    InventorySourceType = InventorySourceTypeEnum.Syndication,
                    InventorySourceId = inventorySourceId
                }, currentDateTime);
                var quarterPostUpdate = inventorySummariesPostUpdate.FirstOrDefault(i => i.Quarter.Quarter == 2 && i.Quarter.Year == 2019);
                
                var exportableQuarterSummariesAfter = _InventoryExportRepository.GetInventoryQuartersForSource(inventorySourceId);

                Assert.Greater(quarterPostUpdate.LastUpdatedDate.Value.Ticks, quarterPreUpdate.LastUpdatedDate.GetValueOrDefault().Ticks);
                // validate the untouched quarters remained for file export
                Assert.AreEqual(exportableQuarterSummariesBefore.Count, exportableQuarterSummariesAfter.Count);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventorySummaryDetailsTest()
        {
            using (new TransactionScopeWrapper())
            {
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
        [Category("short_running")]
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
        [Category("long_running")]
        public void ReturnsUnits_WhenRatingsAreProcessed()
        {
            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_Q1_2025.xlsx", processInventoryRatings: true);

                var inventorySourceId = 4; // TTWN
                var daypartCodeId = 1; // EMN
                var startDate = new DateTime(2025, 1, 1);
                var endDate = new DateTime(2025, 3, 31);

                var units = _InventorySummaryService.GetInventoryUnits(inventorySourceId, daypartCodeId, startDate, endDate);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(units));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void DoesNotReturnUnits_WhenRatingsAreNotProcessed()
        {
            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_Q1_2025.xlsx", processInventoryRatings: false);

                var inventorySourceId = 4; // TTWN
                var daypartCodeId = 1; // EMN
                var startDate = new DateTime(2025, 1, 1);
                var endDate = new DateTime(2025, 3, 31);

                var units = _InventorySummaryService.GetInventoryUnits(inventorySourceId, daypartCodeId, startDate, endDate);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(units));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void DoesNotReturnUnits_WhenTwoFilesAreUploaded_ButLastOneWithoutRatingsProcessed()
        {
            using (new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_Q1_2025.xlsx", processInventoryRatings: true);
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("Barter_Q1_2025_2.xlsx", processInventoryRatings: false);

                var inventorySourceId = 4; // TTWN
                var daypartCodeId = 1; // EMN
                var startDate = new DateTime(2025, 1, 1);
                var endDate = new DateTime(2025, 3, 31);

                var units = _InventorySummaryService.GetInventoryUnits(inventorySourceId, daypartCodeId, startDate, endDate);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(units));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventoryQuartersBySourceAndDaypartCodeTest()
        {
            using (new TransactionScopeWrapper())
            {
                var daypartCodeId = _DaypartDefaultRepository.GetDaypartDefaultByCode("OVN").Id;

                var quarters = _InventorySummaryService.GetInventoryQuarters(nbcOAndO_InventorySourceId, daypartCodeId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarters));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetDaypartDefaultTest()
        {
            var daypartCodes = _InventorySummaryService.GetDaypartDefaults(nbcOAndO_InventorySourceId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DaypartDefaultDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypartCodes, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventorySourcesTest()
        {
            var inventoryCards = _InventorySummaryService.GetInventorySources();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventoryQuartersTest()
        {
            var inventoryCards = _InventorySummaryService.GetInventoryQuarters(new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
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
        [Category("short_running")]
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
        [Category("short_running")]
        public void GetInventorySummariesQuarterTest()
        {
            var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                Quarter = new QuarterDto
                {
                    Quarter = 1,
                    Year = 2019
                }
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventorySummariesWithOpenMarketTest()
        {
            var request = new InventorySummaryFilterDto
            {
                Quarter = new QuarterDto
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
        [Category("short_running")]
        public void GetInventorySummaryOpenMarketTest()
        {
            var request = new InventorySummaryFilterDto
            {
                InventorySourceId = 1,  //open market
                Quarter = new QuarterDto
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
        [Category("short_running")]
        public void GetInventorySummariesWithSyndicationTest()
        {
            var request = new InventorySummaryFilterDto
            {
                Quarter = new QuarterDto
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
        [Category("short_running")]
        public void GetInventorySummary_Syndication(string inventorySourceName)
        {
            using (ApprovalResults.ForScenario(inventorySourceName))
            {
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceName);
                var request = new InventorySummaryFilterDto
                {
                    InventorySourceId = inventorySource.Id,
                    Quarter = new QuarterDto
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
        [Category("short_running")]
        public void GetInventorySummaryFilterByDaypartCodeIdTest()
        {
            var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
            {
                Quarter = new QuarterDto
                {
                    Quarter = 1,
                    Year = 2019
                },
                DaypartDefaultId = 1
            }, new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventorySummarySourceTypes()
        {
            var sourceTypes = _InventorySummaryService.GetInventorySourceTypes();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(sourceTypes));
        }

        [Test]
        [TestCase(InventorySourceTypeEnum.Barter, 2, 2019)]
        [TestCase(InventorySourceTypeEnum.Diginet, 4, 2018)]
        [TestCase(InventorySourceTypeEnum.OpenMarket, 4, 2018)]
        [TestCase(InventorySourceTypeEnum.ProprietaryOAndO, 1, 2019)]
        [TestCase(InventorySourceTypeEnum.Syndication, 1, 2018)]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetInventorySummariesFilterBySourceTypeTest(InventorySourceTypeEnum inventorySourceType, int quarterNumber, int quarterYear)
        {
            using (ApprovalResults.ForScenario(inventorySourceType, quarterNumber, quarterYear))
            {
                var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
                {
                    Quarter = new QuarterDto
                    {
                        Quarter = quarterNumber,
                        Year = quarterYear
                    },
                    InventorySourceType = inventorySourceType
                }, new DateTime(2019, 04, 01));

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ProprietaryOAndO_CanAggregateData_HutIsNull()
        {
            using (var trx = new TransactionScopeWrapper())
            {
                _InventoryFileTestHelper.UploadProprietaryInventoryFile("O&O ABC O&O 3Q2021.xlsx", new DateTime(2019, 07, 20));

                //aggregate the data
                _InventorySummaryService.AggregateInventorySummaryData(new List<int> { 10 });

                var inventoryCards = _InventorySummaryService.GetInventorySummaries(new InventorySummaryFilterDto
                {
                    InventorySourceId = 10,
                }, new DateTime(2019, 07, 01));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(InventorySummaryDto), "LastUpdatedDate");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards, jsonSettings));
            }
        }

        [Test]
        [Ignore("This test is used to load and aggregate all the data in the db and saved it to inventory_summary tables")]
        public void LoadAggregationDataIntoDb()
        {
            var inventorySources = new List<int>
                {
                    1       ,//   Open Market
                    3       ,//   TVB
                    4       ,//   TTWN
                    5       ,//   CNN
                    6       ,//   Sinclair
                    7       ,//   LilaMax
                    8       ,//   MLB
                    9       ,//   Ference Media
                    10       ,//  ABC O&O
                    11       ,//  NBC O&O
                    12       ,//  KATZ
                    13       ,//  20th Century Fox (Twentieth Century)
                    14       ,//  CBS Synd
                    15       ,//  NBCU Syn
                    16       ,//  WB Syn
                    17       ,//  Antenna TV
                    18       ,//  Bounce
                    19       ,//  BUZZR
                    20       ,//  COZI
                    21       ,//  Escape
                    22       ,//  Grit
                    23       ,//  HITV
                    24       ,//  Laff
                    25       ,//  Me TV
                };

            List<string> files = new List<string>
            {
                //@"ProprietaryDataFiles\Diginet_WithDaypartCodes_WithoutFullHours.xlsx",
                //@"ProprietaryDataFiles\Barter_ValidFormat_SingleBook_ShortDateRange.xlsx",
                //@"ImportingRateData\Open Market provided and projected imps.xml",
                //@"ProprietaryDataFiles\Diginet_WithDaypartCodes.xlsx"
            };

            using (var trx = new TransactionScopeWrapper())
            {
                ////load data from files into db
                //foreach (var filePath in files)
                //{
                //    var request = new InventoryFileSaveRequest
                //    {
                //        StreamData = new FileStream($@".\Files\{filePath}", FileMode.Open, FileAccess.Read),
                //        FileName = filePath
                //    };

                //    if (Path.GetExtension(filePath).Equals(".xml"))
                //    {
                //        _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", new DateTime(2019, 07, 20));
                //    }
                //    else
                //    {
                //        _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", new DateTime(2019, 07, 20));
                //    }

                //}

                ////process the imported data
                //foreach (var job in _InventoryFileRatingsJobsRepository.GetJobsBatch(files.Count()))
                //{
                //    _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.id.Value);
                //}

                //aggregate the data
                _InventorySummaryService.AggregateInventorySummaryData(inventorySources);

                //uncomment this to save the data
                //trx.Complete();
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Ignore]
        public void GetInventorySummariesWithCache()
        {
            const int inventorySourceId = 5;

            using (new TransactionScopeWrapper())
            {
                var requestFilter = new InventorySummaryFilterDto
                {
                    InventorySourceId = inventorySourceId,
                };
                var requestDate = new DateTime(2019, 04, 01);
                var cleanCacheSize = _InventorySummaryCache.GetItemCount(false);
                var inventoryCards = _InventorySummaryService.GetInventorySummariesWithCache(requestFilter, requestDate);
                var usedCacheSize = _InventorySummaryCache.GetItemCount(false);
                inventoryCards = _InventorySummaryService.GetInventorySummariesWithCache(requestFilter, requestDate);
                var reusedCacheSize = _InventorySummaryCache.GetItemCount(false);

                _InventoryFileTestHelper.UploadOpenMarketInventoryFile("Open Market projected imps.xml");

                _InventorySummaryService.AggregateInventorySummaryData(new List<int> { inventorySourceId });

                inventoryCards = _InventorySummaryService.GetInventorySummariesWithCache(requestFilter, requestDate);
                var afterNewFileCacheSize = _InventorySummaryCache.GetItemCount(false);

                Assert.True(cleanCacheSize + 1 == usedCacheSize); //check new request is cached
                Assert.True(usedCacheSize == reusedCacheSize); //check cache used for the same request
                Assert.True(usedCacheSize + 1 == afterNewFileCacheSize); //check new data cached after file upload
            }
        }
    }
}
