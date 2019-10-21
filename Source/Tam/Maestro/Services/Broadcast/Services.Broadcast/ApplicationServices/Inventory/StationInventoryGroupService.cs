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
using Services.Broadcast.Cache;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices
{

    public interface IStationInventoryGroupService : IApplicationService
    {
        void AddNewStationInventoryOpenMarket(InventoryFileBase inventoryFile);
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

        public void AddNewStationInventoryOpenMarket(InventoryFileBase inventoryFile)
        {
            if (inventoryFile.InventorySource == null || !inventoryFile.InventorySource.IsActive)
                throw new Exception(string.Format("The selected source type is invalid or inactive."));

            _ExpireExistingInventoryManifestWeeks(inventoryFile);
            _AddNewManifests(inventoryFile);
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
            Parallel.ForEach(audiences, (a) =>
            {
                a.Audience = _audiencesCache.GetDisplayAudienceById(a.Audience.Id);
            });
        }

        private void _SetInventoryGroupsDayparts(List<StationInventoryGroup> stationInventoryGroups)
        {
            var dayparts = stationInventoryGroups.SelectMany(ig => ig.Manifests.SelectMany(m => m.ManifestDayparts.Select(md => md.Daypart)));
            Parallel.ForEach(dayparts, (d) =>
            {
                d = _daypartCache.GetDisplayDaypart(d.Id);
            });
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

            _inventoryRepository.AddNewInventoryGroups(inventoryFile);
            _AddNewManifests(inventoryFile);
        }

        private void _AddNewManifests(InventoryFileBase inventoryFile)
        {            
            var newManifests = inventoryFile.InventoryManifests.Where(m => m.Id == null);

            _inventoryRepository.AddNewManifests(newManifests, inventoryFile.Id, inventoryFile.InventorySource.Id);
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

            var taskList = new List<Task<List<StationInventoryManifestWeek>>>();

            // match manifest weeks by contracted dayparts and media weeks
            foreach (var manifest in existingInventoryManifests)
            {
                taskList.Add(Task.Run(() =>
                {
                    var contractedDaypartIds = manifest.ManifestDayparts.Select(x => x.Daypart.Id).OrderBy(x => x);

                    if (manifestContractedDaypartSets.Any(x => contractedDaypartIds.SequenceEqual(x)))
                    {
                        return manifest.ManifestWeeks.Where(w => allManifestMediaWeekIds.Contains(w.MediaWeek.Id)).ToList();
                    }
                    return new List<StationInventoryManifestWeek>();
                }));

            }
            weeksToExpire = Task.WhenAll(taskList).Result.SelectMany(x => x).Where(x => x != null).Distinct().ToList();
            if (weeksToExpire.Any())
            {
                _inventoryRepository.RemoveManifestWeeks(weeksToExpire);
            }
        }

        private void _ExpireExistingInventoryManifestWeeks(InventoryFileBase inventoryFile)
        {

            var allManifests = inventoryFile.GetAllManifests();

            var manifestsByMarketDaypart = allManifests.GroupBy(m => new { m.Station.MarketCode, m.ManifestDayparts.Single().Daypart})
                                            .Where(g => g.Key.MarketCode.HasValue).ToList();
            var count = manifestsByMarketDaypart.Count;
            foreach (var manifestGroup in manifestsByMarketDaypart)
            {
                var weeks = manifestGroup.SelectMany(m => m.ManifestWeeks.Select(w => w.MediaWeek.Id)).Distinct().ToList();
                _inventoryRepository.RemoveManifestWeeksByMarketAndDaypart
                    (weeks, manifestGroup.Key.MarketCode.Value, manifestGroup.Key.Daypart.Id);

            }
        }
    }
}
