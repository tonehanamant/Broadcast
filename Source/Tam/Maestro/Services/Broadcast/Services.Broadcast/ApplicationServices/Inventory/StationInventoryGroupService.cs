using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Repositories;
using Common.Services.Repositories;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Clients;
using Common.Services;
using Services.Broadcast.Converters;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.ApplicationServices
{

    public interface IStationInventoryGroupService : IApplicationService
    {
        void AddNewStationInventoryGroups(InventoryFileSaveRequest request, InventoryFile inventoryFile);
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

        public void AddNewStationInventoryGroups(InventoryFileSaveRequest request, InventoryFile inventoryFile)
        {
            if (inventoryFile.InventorySource == null || !inventoryFile.InventorySource.IsActive)
                throw new Exception(string.Format("The selected source type is invalid or inactive."));

            var expireDate = request.EffectiveDate.AddDays(-1);
            
            _ExpireExistingInventory(inventoryFile.InventoryGroups, inventoryFile.InventorySource, expireDate);
            _inventoryRepository.AddNewInventoryGroups(inventoryFile);
        }

        private List<StationInventoryGroup> _ExpireExistingInventory(IEnumerable<StationInventoryGroup> groups, InventorySource source, DateTime expireDate)
        {
            var groupNames = groups.Select(g => g.Name).Distinct().ToList();
            var existingInventory = _inventoryRepository.GetActiveInventoryBySourceAndName(source, groupNames);

            if (!existingInventory.Any())
                return existingInventory;

            _inventoryRepository.ExpireInventoryGroupsAndManifests(existingInventory, expireDate);

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
        
    }

}
