using System.Collections.Generic;
using Services.Broadcast.Entities;

namespace Services.Broadcast.ApplicationServices
{
    public interface ICNNStationInventoryGroupService : IStationInventoryGroupService
    {
        
    }
    public class CNNStationInventoryGroupService : ICNNStationInventoryGroupService
    {
        //private IStationGroupInventoryRepository _InventoryGroupRepository;
        public CNNStationInventoryGroupService()
        {
        }
        public List<StationInventoryGroup> EnsureGroupsByCode(string daypartCode)
        {
            return new List<StationInventoryGroup>();
        }
    }
}