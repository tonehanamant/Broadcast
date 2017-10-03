using System;
using System.Collections.Generic;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;

namespace Services.Broadcast.ApplicationServices
{

    public interface IStationInventoryGroupService : IApplicationService
    {
        /// <summary>
        /// just an idea to kick around
        /// </summary>
        List<StationInventoryGroup> EnsureGroupsByCode(string daypartCode);
    }
    
}
