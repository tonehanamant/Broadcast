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
        void AddNewStationInventory(InventoryFileBase inventoryFile, DateTime newEffectiveDate, DateTime newEndDate, int contractedDaypartId);
        List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId);
    }

    public class StationInventoryGroupService : IStationInventoryGroupService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IDaypartCache _daypartCache;
        private readonly IBroadcastAudiencesCache _audiencesCache;

        public StationInventoryGroupService(IDataRepositoryFactory broadcastDataRepositoryFactory
                                            ,IDaypartCache daypartCache
                                            , IBroadcastAudiencesCache audiencesCache)
        {
            _inventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _daypartCache = daypartCache;
            _audiencesCache = audiencesCache;
        }

        public string GenerateGroupName(string daypartCode, int slotNumber)
        {
           return null;    
        }

        public void AddNewStationInventoryGroups(InventoryFileBase inventoryFile, DateTime newEffectiveDate)
        {
            if (inventoryFile.InventorySource == null || !inventoryFile.InventorySource.IsActive)
                throw new Exception(string.Format("The selected source type is invalid or inactive."));

            var expireDate = newEffectiveDate.AddDays(-1);
            
            _ExpireExistingInventory(inventoryFile.InventoryGroups, inventoryFile.InventorySource, expireDate, newEffectiveDate);
            _inventoryRepository.AddNewInventoryGroups(inventoryFile);
        }

        private List<StationInventoryGroup> _ExpireExistingInventory(IEnumerable<StationInventoryGroup> groups, InventorySource source, DateTime expireDate, DateTime newEffectiveDate)
        {
            var groupNames = groups.Select(g => g.Name).Distinct().ToList();
            var existingInventory = _inventoryRepository.GetActiveInventoryBySourceAndName(source, groupNames, newEffectiveDate);

            if (!existingInventory.Any())
                return existingInventory;

            _inventoryRepository.ExpireInventoryGroupsAndManifests(existingInventory, expireDate, newEffectiveDate);

            return existingInventory;
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

        public void AddNewStationInventory(InventoryFileBase inventoryFile, DateTime newEffectiveDate, DateTime newEndDate, int contractedDaypartId)
        {
            if (inventoryFile.InventorySource == null || !inventoryFile.InventorySource.IsActive)
                throw new Exception(string.Format("The selected source type is invalid or inactive."));

            _ExpireExistingInventoryGroups(inventoryFile.InventorySource, newEffectiveDate, newEndDate, contractedDaypartId);

            _inventoryRepository.AddNewInventoryGroups(inventoryFile);
        }

        private void _ExpireExistingInventoryGroups(InventorySource source, DateTime newEffectiveDate, DateTime newEndDate, int contractedDaypartId)
        {
            var dayAfterNewEndDate = newEndDate.AddDays(1);
            var dayBeforeNewEffectiveDate = newEffectiveDate.AddDays(-1);
            var existingInventoryGroups = _inventoryRepository.GetActiveInventoryGroupsBySourceAndContractedDaypart(source, contractedDaypartId, newEffectiveDate, newEndDate);

            // covers case when existing inventory intersects with new inventory and 
            // we can save part of existing inventory that goes after the new inventory date interval
            existingInventoryGroups
                .Where(x => newEndDate >= x.StartDate && newEndDate < x.EndDate)
                .ForEach(x =>
                {
                    x.StartDate = dayAfterNewEndDate;
                    x.Manifests
                        .Where(m => newEndDate >= m.EffectiveDate && newEndDate < m.EndDate)
                        .ForEach(m => m.EffectiveDate = dayAfterNewEndDate);
                });

            existingInventoryGroups
                .Where(x => newEndDate >= x.EndDate && newEffectiveDate <= x.EndDate)
                .ForEach(x =>
                {
                    x.EndDate = dayBeforeNewEffectiveDate;
                    x.Manifests
                        .Where(m => newEndDate >= m.EndDate && newEffectiveDate <= m.EndDate)
                        .ForEach(m => m.EndDate = dayBeforeNewEffectiveDate);
                });

            _inventoryRepository.UpdateInventoryGroupsDateIntervals(existingInventoryGroups);
        }
    }
}
