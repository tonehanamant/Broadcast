using System.Collections.Generic;
using Services.Broadcast.Entities;

namespace Services.Broadcast.ApplicationServices
{
    public interface ICNNStationInventoryGroupService : IStationInventoryGroupService
    {
        int GetSlotCount(string daypartCode);
    }
    public class CNNStationInventoryGroupService : ICNNStationInventoryGroupService
    {
        public int GetSlotCount(string daypartCode)
        {
            return 5;
        }

        public string GenerateGroupName(string daypartCode,int slotNumber)
        {
            return daypartCode + slotNumber;
        }
    }
}