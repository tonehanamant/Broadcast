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
        void SaveStationInventoryGroups(InventoryFileSaveRequest request, InventoryFile inventoryFile);
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

        public void SaveStationInventoryGroups(InventoryFileSaveRequest request, InventoryFile inventoryFile)
        {
            //TODO: GetInventorySourceByName needs to change to make the inventory source and inventory type compatible
            var inventorySource = _inventoryRepository.GetInventorySourceByName(inventoryFile.Source.ToString());

            if (inventorySource == null || !inventorySource.IsActive)
                throw new Exception(string.Format("The selected source type is invalid or inactive."));

            inventoryFile.InventorySourceId = inventorySource.Id;
            _inventoryRepository.SaveInventoryGroups(inventoryFile);
            _inventoryRepository.UpdateInventoryGroups(inventoryFile.InventoryGroups);
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
            var dayparts = stationInventoryGroups.SelectMany(m => m.Manifests.SelectMany(d => d.Dayparts));

            dayparts.ForEach(d=> d = _daypartCache.GetDisplayDaypart(d.Id));
        }
        
    }

}
