using System;
using System.Collections.Generic;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;

namespace Services.Broadcast.ApplicationServices
{

    public interface IStationInventoryGroupService : IApplicationService
    {
        string GenerateGroupName(string daypartCode, int slotNumber);
    }

}
