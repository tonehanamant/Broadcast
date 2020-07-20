using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services.Repositories;
using Microsoft.Practices.ObjectBuilder2;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.ApplicationServices
{

    public interface ICNNStationInventoryGroupService 
    {
        int GetSlotCount(string daypartCode);
        string GenerateGroupName(string daypartCode, int slotNumber);
    }

    public class CNNStationInventoryGroupService : ICNNStationInventoryGroupService
    {
        private IInventoryRepository _InventoryRepository;

        public CNNStationInventoryGroupService(IDataRepositoryFactory dataRepositoryFactory)
        {
            _InventoryRepository = dataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        public int GetSlotCount(string daypartCode)
        {
            return 5;
        }
        public string GenerateGroupName(string daypartCode, int slotNumber)
        {
            return daypartCode + slotNumber;
        }
    }
}