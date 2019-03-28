using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class StationInventoryGroupServiceTests
    {
        private IStationInventoryGroupService _StationInventoryGroupService = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationInventoryGroupService>();
        private IInventoryRepository _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        private IInventoryFileRepository _InventoryFileRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();

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
