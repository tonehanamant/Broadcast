using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using Common.Services.Repositories;
using Tam.Maestro.Common;
using Common.Services;
using Tam.Maestro.Common.DataLayer;
using Services.Broadcast.Entities.StationInventory;

namespace Services.Broadcast.ApplicationServices
{

    public interface IStationInventoryGroupService : IApplicationService
    {
        void AddNewStationInventoryGroups(InventoryFileBase inventoryFile, DateTime newEffectiveDate);
        void AddNewStationInventory(InventoryFileBase inventoryFile, int? contractedDaypartId = null);
        List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId);
    }

    public class StationInventoryGroupService : IStationInventoryGroupService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IDaypartCache _daypartCache;
        private readonly IBroadcastAudiencesCache _audiencesCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public StationInventoryGroupService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache, 
            IBroadcastAudiencesCache audiencesCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _inventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _daypartCache = daypartCache;
            _audiencesCache = audiencesCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public void AddNewStationInventoryGroups(InventoryFileBase inventoryFile, DateTime newEffectiveDate)
        {
            if (inventoryFile.InventorySource == null || !inventoryFile.InventorySource.IsActive)
                throw new Exception(string.Format("The selected source type is invalid or inactive."));

            _ExpireExistingInventoryGroups(inventoryFile.InventoryGroups, inventoryFile.InventorySource, newEffectiveDate);
            _inventoryRepository.AddNewInventory(inventoryFile);
        }

        private void _ExpireExistingInventoryGroups(IEnumerable<StationInventoryGroup> groups, InventorySource source, DateTime newEffectiveDate)
        {
            var expireDate = newEffectiveDate.AddDays(-1);
            var groupNames = groups.Select(g => g.Name).Distinct().ToList();
            var existingInventory = _inventoryRepository.GetActiveInventoryBySourceAndName(source, groupNames, newEffectiveDate);

            if (!existingInventory.Any())
                return;

            _inventoryRepository.ExpireInventoryGroupsAndManifests(existingInventory, expireDate, newEffectiveDate);
        }

        public List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var stationInventoryGroups = _inventoryRepository.GetStationInventoryGroupsByFileId(fileId);
                _SetInventoryGroupsDayparts(stationInventoryGroups);
                _SetInventoryGroupsAudiences(stationInventoryGroups);
                return stationInventoryGroups;
            }
        }

        private void _SetInventoryGroupsAudiences(List<StationInventoryGroup> stationInventoryGroups)
        {
            var audiences = stationInventoryGroups.SelectMany(m => m.Manifests.SelectMany(d => d.ManifestAudiences));

            audiences.ForEach(a=> a.Audience = _audiencesCache.GetDisplayAudienceById(a.Audience.Id));
        }

        private void _SetInventoryGroupsDayparts(List<StationInventoryGroup> stationInventoryGroups)
        {
            var dayparts = stationInventoryGroups.SelectMany(ig => ig.Manifests.SelectMany(m => m.ManifestDayparts.Select(md => md.Daypart)));

            dayparts.ForEach(d=> d = _daypartCache.GetDisplayDaypart(d.Id));
        }

        public void AddNewStationInventory(InventoryFileBase inventoryFile, int? contractedDaypartId)
        {
            if (inventoryFile.InventorySource == null || !inventoryFile.InventorySource.IsActive)
                throw new Exception("The selected source type is invalid or inactive.");

            if (contractedDaypartId.HasValue)
            {
                _ExpireExistingInventoryManifestWeeks(inventoryFile, contractedDaypartId.Value);
            }
            else
            {
                _ExpireExistingInventoryManifestWeeksByManifestDayparts(inventoryFile);
            }

            _inventoryRepository.AddNewInventory(inventoryFile);
        }

        private void _ExpireExistingInventoryManifestWeeks(InventoryFileBase inventoryFile, int contractedDaypartId)
        {
            var allManifestMediaWeekIds = inventoryFile
                .GetAllManifests()
                .SelectMany(x => x.ManifestWeeks)
                .Select(x => x.MediaWeek.Id)
                .Distinct();

            var weeksToExpire = _inventoryRepository.GetStationInventoryManifestWeeks(
                inventoryFile.InventorySource,
                contractedDaypartId,
                allManifestMediaWeekIds);

            _inventoryRepository.RemoveManifestWeeks(weeksToExpire);
        }

        private void _ExpireExistingInventoryManifestWeeksByManifestDayparts(InventoryFileBase inventoryFile)
        {
            var manifestContractedDaypartSets = inventoryFile.InventoryManifests.Select(x => x.ManifestDayparts.Select(d => d.Daypart.Id).OrderBy(d => d)).ToList();
            var existingInventoryManifests = _inventoryRepository.GetInventoryManifestsBySource(inventoryFile.InventorySource);
            var weeksToExpire = new List<StationInventoryManifestWeek>();
            var allManifestMediaWeekIds = inventoryFile.InventoryManifests
                .SelectMany(x => x.ManifestWeeks)
                .Select(x => x.MediaWeek.Id)
                .Distinct();

            // match manifest weeks by contracted dayparts and media weeks
            foreach (var manifest in existingInventoryManifests)
            {
                var contractedDaypartIds = manifest.ManifestDayparts.Select(x => x.Daypart.Id).OrderBy(x => x);

                if (manifestContractedDaypartSets.Any(x => contractedDaypartIds.SequenceEqual(x)))
                {
                    var manifestWeeksToExpire = manifest.ManifestWeeks.Where(w => allManifestMediaWeekIds.Contains(w.MediaWeek.Id));
                    weeksToExpire.AddRange(manifestWeeksToExpire);
                }
            }

            _inventoryRepository.RemoveManifestWeeks(weeksToExpire);
        }
    }
}
