using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Microsoft.Practices.Unity;
using IntegrationTests.Common;

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

        [TestFixtureSetUp]
        public void SetUp()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IImpersonateUser, ImpersonateUserStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceDataLakeStubb>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();
            _ProprietaryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>();
            _StationInventoryGroupService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationInventoryGroupService>();
            _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryDaypartParsingEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryDaypartParsingEngine>();
            _MediaMonthAndWeekAggregateCache = new MediaMonthAndWeekAggregateCache(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresGroups()
        {
            var effectiveDate = new DateTime(2019, 2, 7);
            var endDate = new DateTime(2019, 2, 13);
            _VerifyGroupsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresGroups_2()
        {
            var effectiveDate = new DateTime(2019, 2, 11);
            var endDate = new DateTime(2019, 2, 16);
            _VerifyGroupsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresGroups_3()
        {
            var effectiveDate = new DateTime(2019, 2, 10);
            var endDate = new DateTime(2019, 2, 17);
            _VerifyGroupsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresGroups_4()
        {
            var effectiveDate = new DateTime(2019, 2, 13);
            var endDate = new DateTime(2019, 2, 24);
            _VerifyGroupsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresGroups_5()
        {
            var effectiveDate = new DateTime(2019, 2, 5);
            var endDate = new DateTime(2019, 2, 24);
            _VerifyGroupsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresManifests()
        {
            var effectiveDate = new DateTime(2019, 2, 7);
            var endDate = new DateTime(2019, 2, 13);
            _VerifyManifestsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresManifests_2()
        {
            var effectiveDate = new DateTime(2019, 2, 11);
            var endDate = new DateTime(2019, 2, 16);
            _VerifyManifestsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresManifests_3()
        {
            var effectiveDate = new DateTime(2019, 2, 10);
            var endDate = new DateTime(2019, 2, 17);
            _VerifyManifestsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresManifests_4()
        {
            var effectiveDate = new DateTime(2019, 2, 13);
            var endDate = new DateTime(2019, 2, 24);
            _VerifyManifestsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresManifests_5()
        {
            var effectiveDate = new DateTime(2019, 2, 5);
            var endDate = new DateTime(2019, 2, 24);
            _VerifyManifestsExpiringForDateInterval(effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ExpiresManifests_WhenAllContractedDaypartsMatchInventory()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile2.xlsx";
            const string inventorySourceName = "COZI";
            const string daypartString = "M-F 9a-10a SA-SU 6a-7a";
            var effectiveDate = new DateTime(2018, 10, 5);
            var endDate = new DateTime(2018, 10, 24);

            _VerifyManifestsExpiringForManifestDayparts(fileName, inventorySourceName, daypartString, effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DoesNotExpireManifests_WhenOnlyOneContractedDaypartMatchInventory()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile2.xlsx";
            const string inventorySourceName = "COZI";
            const string daypartString = "M-F 9a-10a";
            var effectiveDate = new DateTime(2018, 10, 5);
            var endDate = new DateTime(2018, 10, 24);

            _VerifyManifestsExpiringForManifestDayparts(fileName, inventorySourceName, daypartString, effectiveDate, endDate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DoesNotExpireManifests_WhenAdditionalContractedDaypartSpecified()
        {
            const string fileName = @"ProprietaryDataFiles\Diginet_ValidFile2.xlsx";
            const string inventorySourceName = "COZI";
            const string daypartString = "M-F 9a-10a SA-SU 6a-7a SA-SU 10p-11p";
            var effectiveDate = new DateTime(2018, 10, 5);
            var endDate = new DateTime(2018, 10, 24);

            _VerifyManifestsExpiringForManifestDayparts(fileName, inventorySourceName, daypartString, effectiveDate, endDate);
        }

        private void _VerifyManifestsExpiringForManifestDayparts(string fileName, string inventorySourceName, string daypartString, DateTime start, DateTime end)
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
                            EffectiveDate = start,
                            EndDate = end,
                            InventoryFileId = fileId,
                            InventorySourceId = inventoruSource.Id,
                            ManifestDayparts = dayparts.Select(d => new StationInventoryManifestDaypart { Daypart = d }).ToList(),
                            ManifestWeeks = _GetManifestWeeksInRange(start, end, 2)
                        }
                    }
                };

                var manifestsBeforeExpiring = _InventoryRepository.GetStationInventoryManifestsByFileId(inventoryFile.Id)
                    .Select((x, index) => new
                    {
                        x.EffectiveDate,
                        x.EndDate,
                        Daypart = string.Join(" ", x.ManifestDayparts.Select(d => d.Daypart.ToLongString())),
                        Weeks = x.ManifestWeeks.Select(w => new
                        {
                            w.Spots,
                            w.MediaWeek.StartDate,
                            w.MediaWeek.EndDate,
                        })
                    });

                _StationInventoryGroupService.AddNewStationInventory(inventoryFile, start, end);

                var manifestsAfterExpiring = _InventoryRepository.GetStationInventoryManifestsByFileId(inventoryFile.Id)
                    .Select((x, index) => new
                    {
                        x.EffectiveDate,
                        x.EndDate,
                        Daypart = string.Join(" ", x.ManifestDayparts.Select(d => d.Daypart.ToLongString())),
                        Weeks = x.ManifestWeeks.Select(w => new
                        {
                            w.Spots,
                            w.MediaWeek.StartDate,
                            w.MediaWeek.EndDate,
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
            return mediaWeeks.Select(x => new StationInventoryManifestWeek { MediaWeek = x, Spots = spots }).ToList();
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

        private void _VerifyGroupsExpiringForDateInterval(DateTime start, DateTime end)
        {
            using (new TransactionScopeWrapper())
            {
                var contractedDaypartId = 5;
                var inventoryFile = new InventoryFile
                {
                    Id = _InventoryFileRepository.GetInventoryFileIdByHash("ExpiresGroupsTest"),
                    InventorySource = new InventorySource
                    {
                        Id = 7,
                        IsActive = true
                    }
                };

                var groupsBeforeExpiring = _InventoryRepository.GetStationInventoryGroupsByFileId(inventoryFile.Id)
                    .Select((x, index) => new
                    {
                        index,
                        x.StartDate,
                        x.EndDate,
                        Manifests = x.Manifests.Select(m => new
                        {
                            m.EffectiveDate,
                            m.EndDate,
                            Weeks = m.ManifestWeeks.Select(w => new
                            {
                                w.Spots,
                                w.MediaWeek.StartDate,
                                w.MediaWeek.EndDate,
                            })
                        })
                    });

                _StationInventoryGroupService.AddNewStationInventory(inventoryFile, start, end, contractedDaypartId);

                var groupsAfterExpiring = _InventoryRepository.GetStationInventoryGroupsByFileId(inventoryFile.Id)
                    .Select((x, index) => new
                    {
                        index,
                        x.StartDate,
                        x.EndDate,
                        Manifests = x.Manifests.Select(m => new
                        {
                            m.EffectiveDate,
                            m.EndDate,
                            Weeks = m.ManifestWeeks.Select(w => new
                            {
                                w.Spots,
                                w.MediaWeek.StartDate,
                                w.MediaWeek.EndDate,
                            })
                        })
                    });

                var result = new { groupsBeforeExpiring, groupsAfterExpiring };
                var resultJson = IntegrationTestHelper.ConvertToJson(result);

                Approvals.Verify(resultJson);
            }
        }

        private void _VerifyManifestsExpiringForDateInterval(DateTime start, DateTime end)
        {
            using (new TransactionScopeWrapper())
            {
                var contractedDaypartId = 5;
                var inventoryFile = new InventoryFile
                {
                    Id = _InventoryFileRepository.GetInventoryFileIdByHash("ExpiresManifestsTest"),
                    InventorySource = new InventorySource
                    {
                        Id = 10,
                        IsActive = true
                    }
                };

                var manifestsBeforeExpiring = _InventoryRepository.GetStationInventoryManifestsByFileId(inventoryFile.Id)
                    .Select((x, index) => new
                    {
                        index,
                        x.EffectiveDate,
                        x.EndDate,
                        Weeks = x.ManifestWeeks.Select(w => new
                        {
                            w.Spots,
                            w.MediaWeek.StartDate,
                            w.MediaWeek.EndDate,
                        })
                    });

                _StationInventoryGroupService.AddNewStationInventory(inventoryFile, start, end, contractedDaypartId);

                var manifestsAfterExpiring = _InventoryRepository.GetStationInventoryManifestsByFileId(inventoryFile.Id)
                    .Select((x, index) => new
                    {
                        index,
                        x.EffectiveDate,
                        x.EndDate,
                        Weeks = x.ManifestWeeks.Select(w => new
                        {
                            w.Spots,
                            w.MediaWeek.StartDate,
                            w.MediaWeek.EndDate,
                        })
                    });

                var result = new { manifestsBeforeExpiring, manifestsAfterExpiring };
                var resultJson = IntegrationTestHelper.ConvertToJson(result);

                Approvals.Verify(resultJson);
            }
        }
    }
}
