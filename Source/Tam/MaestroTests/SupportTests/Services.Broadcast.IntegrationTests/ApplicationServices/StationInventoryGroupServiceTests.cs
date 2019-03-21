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

        private void _VerifyGroupsExpiringForDateInterval(DateTime start, DateTime end)
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryFileId = _InventoryFileRepository.GetInventoryFileIdByHash("ExpiresGroupsTest");
                var contractedDaypartId = 5;
                var inventoryFile = new InventoryFile
                {
                    InventorySource = new InventorySource
                    {
                        Id = 7,
                        IsActive = true
                    }
                };

                var groupsBeforeExpiring = _InventoryRepository.GetStationInventoryGroupsByFileId(inventoryFileId)
                    .Select((x, index) => new
                    {
                        index,
                        x.StartDate,
                        x.EndDate,
                        Manifests = x.Manifests.Select(m => new { m.EffectiveDate, m.EndDate })
                    });

                _StationInventoryGroupService.AddNewStationInventory(inventoryFile, start, end, contractedDaypartId);

                var groupsAfterExpiring = _InventoryRepository.GetStationInventoryGroupsByFileId(inventoryFileId)
                    .Select((x, index) => new
                    {
                        index,
                        x.StartDate,
                        x.EndDate,
                        Manifests = x.Manifests.Select(m => new { m.EffectiveDate, m.EndDate })
                    });

                var result = new { groupsBeforeExpiring, groupsAfterExpiring };
                var resultJson = IntegrationTestHelper.ConvertToJson(result);

                Approvals.Verify(resultJson);
            }
        }
    }
}
