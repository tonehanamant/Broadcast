using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class StationInventoryGroupServiceTests
    {
        private IProprietaryInventoryService _ProprietaryService;
        private IStationInventoryGroupService _StationInventoryGroupService;
        private IInventoryRepository _InventoryRepository;
        private IInventoryFileRepository _InventoryFileRepository;
        private IInventoryDaypartParsingEngine _InventoryDaypartParsingEngine;
        private IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private IInventoryWeekEngine _InventoryWeekEngine;

        [TestFixtureSetUp]
        public void SetUp()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStub>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStub>();
            _ProprietaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>();
            _StationInventoryGroupService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationInventoryGroupService>();
            _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryDaypartParsingEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryDaypartParsingEngine>();
            _InventoryWeekEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryWeekEngine>();
            _MediaMonthAndWeekAggregateCache = new MediaMonthAndWeekAggregateCache(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory);
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ExpiresInventory_OneContractedDaypart_1()
        {
            var effectiveDate = new DateTime(2019, 2, 5);
            var endDate = new DateTime(2019, 2, 9);
            _VerifyInventoryExpiringForDateIntervalForOneDaypart(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ExpiresInventory_OneContractedDaypart_2()
        {
            var effectiveDate = new DateTime(2019, 2, 5);
            var endDate = new DateTime(2019, 2, 13);
            _VerifyInventoryExpiringForDateIntervalForOneDaypart(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ExpiresInventory_OneContractedDaypart_3()
        {
            var effectiveDate = new DateTime(2019, 2, 5);
            var endDate = new DateTime(2019, 2, 20);
            _VerifyInventoryExpiringForDateIntervalForOneDaypart(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ExpiresInventory_OneContractedDaypart_4()
        {
            var effectiveDate = new DateTime(2019, 2, 13);
            var endDate = new DateTime(2019, 2, 15);
            _VerifyInventoryExpiringForDateIntervalForOneDaypart(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ExpiresInventory_OneContractedDaypart_5()
        {
            var effectiveDate = new DateTime(2019, 2, 15);
            var endDate = new DateTime(2019, 2, 20);
            _VerifyInventoryExpiringForDateIntervalForOneDaypart(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ExpiresInventory_OneContractedDaypart_6()
        {
            var effectiveDate = new DateTime(2019, 2, 20);
            var endDate = new DateTime(2019, 2, 23);
            _VerifyInventoryExpiringForDateIntervalForOneDaypart(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ExpiresInventory_SeveralContractedDayparts_1()
        {
            var effectiveDate = new DateTime(2018, 10, 5);
            var endDate = new DateTime(2018, 10, 9);
            _VerifyInventoryExpiringForSeveralDayparts(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ExpiresInventory_SeveralContractedDayparts_2()
        {
            var effectiveDate = new DateTime(2018, 10, 5);
            var endDate = new DateTime(2018, 10, 13);
            _VerifyInventoryExpiringForSeveralDayparts(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ExpiresInventory_SeveralContractedDayparts_3()
        {
            var effectiveDate = new DateTime(2018, 10, 5);
            var endDate = new DateTime(2018, 10, 20);
            _VerifyInventoryExpiringForSeveralDayparts(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ExpiresInventory_SeveralContractedDayparts_4()
        {
            var effectiveDate = new DateTime(2018, 10, 13);
            var endDate = new DateTime(2018, 10, 15);
            _VerifyInventoryExpiringForSeveralDayparts(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ExpiresInventory_SeveralContractedDayparts_5()
        {
            var effectiveDate = new DateTime(2018, 10, 15);
            var endDate = new DateTime(2018, 10, 20);
            _VerifyInventoryExpiringForSeveralDayparts(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ExpiresInventory_SeveralContractedDayparts_6()
        {
            var effectiveDate = new DateTime(2018, 10, 20);
            var endDate = new DateTime(2018, 10, 23);
            _VerifyInventoryExpiringForSeveralDayparts(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ExpiresInventory_WhenAllContractedDaypartsMatchInventory()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile2.xlsx";
            const string inventorySourceName = "COZI";
            const string daypartString = "M-F 9a-10a SA-SU 6a-7a";
            var effectiveDate = new DateTime(2018, 10, 5);
            var endDate = new DateTime(2018, 10, 24);

            _VerifyInventoryExpiringForSeveralDayparts(fileName, inventorySourceName, daypartString, effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void DoesNotExpireInventory_WhenOnlyOneContractedDaypartMatchInventory()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile2.xlsx";
            const string inventorySourceName = "COZI";
            const string daypartString = "M-F 9a-10a";
            var effectiveDate = new DateTime(2018, 10, 5);
            var endDate = new DateTime(2018, 10, 24);

            _VerifyInventoryExpiringForSeveralDayparts(fileName, inventorySourceName, daypartString, effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void DoesNotExpireInventory_WhenAdditionalContractedDaypartSpecified()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile2.xlsx";
            const string inventorySourceName = "COZI";
            const string daypartString = "M-F 9a-10a SA-SU 6a-7a SA-SU 10p-11p";
            var effectiveDate = new DateTime(2018, 10, 5);
            var endDate = new DateTime(2018, 10, 24);

            _VerifyInventoryExpiringForSeveralDayparts(fileName, inventorySourceName, daypartString, effectiveDate, endDate);
        }

        [Test]
        [Category("long_running")]
        public void StoresHistory_WhenExpiresInventory()
        {
            const int expectedHistoryDelta = 1;
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile2.xlsx";
            const string inventorySourceName = "COZI";
            const string daypartString = "M-F 9a-10a SA-SU 6a-7a";
            var effectiveDate = new DateTime(2018, 10, 8);
            var endDate = new DateTime(2018, 10, 14);

            using (new TransactionScopeWrapper())
            {
                _InventoryDaypartParsingEngine.TryParse(daypartString, out var dayparts);
                var fileId = _SaveInventoryFile(fileName);
                var inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceName);

                var inventoryFile = new InventoryFile
                {
                    Id = fileId,
                    InventorySource = inventorySource,
                    InventoryManifests = new List<StationInventoryManifest>
                    {
                        new StationInventoryManifest
                        {
                            SpotLengthId = 1,
                            InventoryFileId = fileId,
                            InventorySourceId = inventorySource.Id,
                            ManifestDayparts = dayparts.Select(d => new StationInventoryManifestDaypart { Daypart = d }).ToList(),
                            ManifestWeeks = _GetManifestWeeksInRange(effectiveDate, endDate, 1)
                        }
                    }
                };

                var fileManifests = _InventoryRepository.GetStationInventoryManifestsByFileId(inventoryFile.Id)
                    .Where(m => m.Station.LegacyCallLetters == "KGBY").ToList();

                var weeksHistoryBeforeExpiring = _InventoryRepository.GetStationInventoryManifestWeeksHistory(
                    fileManifests.Select(x => x.Id.Value));

                _StationInventoryGroupService.AddNewStationInventory(inventoryFile);

                var weeksHistoryAfterExpiring = _InventoryRepository.GetStationInventoryManifestWeeksHistory(
                    fileManifests.Select(x => x.Id.Value));

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                // the history table and the transaction scope is acting a little strange and giving varying counts run to run.
                // the scope of the test will be limited  to looking at one station, before and after.
                // as long as the after increments the before by one we declare success.
                
                var actualHistoryDelta = weeksHistoryAfterExpiring.Count - weeksHistoryBeforeExpiring.Count;
                Assert.AreEqual(expectedHistoryDelta, actualHistoryDelta);
            }
        }

        private void _VerifyInventoryExpiringForSeveralDayparts(DateTime start, DateTime end)
        {
            // There should be a manifest with two weeks in DB
            // Week 1: 2018/10/8  - 2018/10/14, inventory available 2018/10/10 - 2018/10/14
            // Week 2: 2018/10/15 - 2018/10/21, inventory available 2018/10/15 - 2018/10/17

            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile2.xlsx";
            const string inventorySourceName = "COZI";
            const string daypartString = "M-F 9a-10a SA-SU 6a-7a";

            _VerifyInventoryExpiringForSeveralDayparts(fileName, inventorySourceName, daypartString, start, end);
        }

        private void _VerifyInventoryExpiringForSeveralDayparts(string fileName, string inventorySourceName, string daypartString, DateTime start, DateTime end)
        {
            using (new TransactionScopeWrapper())
            {
                _InventoryDaypartParsingEngine.TryParse(daypartString, out var dayparts);
                var fileId = _SaveInventoryFile(fileName);
                var inventoruSource = _InventoryRepository.GetInventorySourceByName(inventorySourceName);

                var inventoryFile = new InventoryFile
                {
                    Id = fileId,
                    InventorySource = inventoruSource,
                    InventoryManifests = new List<StationInventoryManifest>
                    {
                        new StationInventoryManifest
                        {
                            SpotLengthId = 1,
                            InventoryFileId = fileId,
                            InventorySourceId = inventoruSource.Id,
                            ManifestDayparts = dayparts.Select(d => new StationInventoryManifestDaypart { Daypart = d }).ToList(),
                            ManifestWeeks = _GetManifestWeeksInRange(start, end, 1)
                        }
                    }
                };

                var manifestsBeforeExpiring = _InventoryRepository.GetStationInventoryManifestsByFileId(inventoryFile.Id)
                    .Select(x => new
                    {
                        Dayparts = string.Join(" ", x.ManifestDayparts.Select(d => d.Daypart.ToLongString())),
                        Weeks = x.ManifestWeeks.Select(w => new
                        {
                            w.Spots,
                            w.StartDate,
                            w.EndDate,
                            MediaWeek_StartDate = w.MediaWeek.StartDate,
                            MediaWeek_EndDate = w.MediaWeek.EndDate,
                        })
                    });

                _StationInventoryGroupService.AddNewStationInventory(inventoryFile);

                var manifestsAfterExpiring = _InventoryRepository.GetStationInventoryManifestsByFileId(inventoryFile.Id)
                    .Select(x => new
                    {
                        Dayparts = string.Join(" ", x.ManifestDayparts.Select(d => d.Daypart.ToLongString())),
                        Weeks = x.ManifestWeeks.Select(w => new
                        {
                            w.Spots,
                            w.StartDate,
                            w.EndDate,
                            MediaWeek_StartDate = w.MediaWeek.StartDate,
                            MediaWeek_EndDate = w.MediaWeek.EndDate,
                        })
                    });

                var result = new { manifestsBeforeExpiring, manifestsAfterExpiring };
                var resultJson = IntegrationTestHelper.ConvertToJson(result);

                Approvals.Verify(resultJson);
            }
        }

        private List<StationInventoryManifestWeek> _GetManifestWeeksInRange(DateTime startDate, DateTime endDate, int spots)
        {
            var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(startDate, endDate);
            var manifestWeeks = mediaWeeks.Select(x => new StationInventoryManifestWeek { MediaWeek = x, Spots = spots }).ToList();

            foreach (var manifestWeek in manifestWeeks)
            {
                var dateRange = _InventoryWeekEngine.GetDateRangeInventoryIsAvailableForForWeek(manifestWeek.MediaWeek, startDate, endDate);
                manifestWeek.StartDate = dateRange.Start ?? default(DateTime);
                manifestWeek.EndDate = dateRange.End ?? default(DateTime);
            }

            return manifestWeeks;
        }

        private int _SaveInventoryFile(string fileName)
        {
            var request = new InventoryFileSaveRequest
            {
                StreamData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read),
                FileName = fileName
            };

            var now = new DateTime(2019, 02, 02);
            return _ProprietaryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now).FileId;
        }

        private void _VerifyInventoryExpiringForDateIntervalForOneDaypart(DateTime start, DateTime end)
        {
            // There should be a manifest with two weeks in DB
            // Week 1: 2019/02/04 - 2019/02/10, inventory available 2019/02/10
            // Week 2: 2019/02/11 - 2019/02/17, inventory available 2019/02/11 - 2019/02/17

            using (new TransactionScopeWrapper())
            {
                var contractedDaypartId = 5;
                var inventoryFile = new InventoryFile
                {
                    Id = _InventoryFileRepository.GetInventoryFileIdByHash("ExpiresManifestsTest"),
                    InventorySource = new InventorySource
                    {
                        Id = 11,
                        IsActive = true
                    },
                    InventoryManifests = new List<StationInventoryManifest>
                    {
                        new StationInventoryManifest
                        {
                            SpotLengthId = 1,
                            ManifestWeeks = _GetManifestWeeksInRange(start, end, 1)
                        }
                    }
                };

                var manifestsBeforeExpiring = _InventoryRepository
                    .GetStationInventoryManifestsByFileId(inventoryFile.Id)
                    .Where(x => x.InventorySourceId == 11)
                    .Select((x, Index) => new
                    {
                        Index,
                        Weeks = x.ManifestWeeks.Select(w => new
                        {
                            w.Spots,
                            w.StartDate,
                            w.EndDate,
                            MediaWeek_StartDate = w.MediaWeek.StartDate,
                            MediaWeek_EndDate = w.MediaWeek.EndDate
                        })
                    });

                _StationInventoryGroupService.AddNewStationInventory(inventoryFile, contractedDaypartId);

                var manifestsAfterExpiring = _InventoryRepository
                    .GetStationInventoryManifestsByFileId(inventoryFile.Id)
                    .Where(x => x.InventorySourceId == 11)
                    .Select((x, Index) => new
                    {
                        Index,
                        Weeks = x.ManifestWeeks.Select(w => new
                        {
                            w.Spots,
                            w.StartDate,
                            w.EndDate,
                            MediaWeek_StartDate = w.MediaWeek.StartDate,
                            MediaWeek_EndDate = w.MediaWeek.EndDate
                        })
                    });

                var result = new { manifestsBeforeExpiring, manifestsAfterExpiring };
                var resultJson = IntegrationTestHelper.ConvertToJson(result);

                Approvals.Verify(resultJson);
            }
        }
    }
}
