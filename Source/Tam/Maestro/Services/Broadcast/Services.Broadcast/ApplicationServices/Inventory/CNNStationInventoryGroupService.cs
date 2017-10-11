using System.Collections.Generic;
using Services.Broadcast.Entities;

namespace Services.Broadcast.ApplicationServices
{
    public interface ICNNStationInventoryGroupService 
    {
        int GetSlotCount(string daypartCode);
        string GenerateGroupName(string daypartCode, int slotNumber);
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