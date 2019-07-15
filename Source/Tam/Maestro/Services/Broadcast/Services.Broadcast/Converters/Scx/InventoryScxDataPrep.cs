using Common.Services.ApplicationServices;
using Services.Broadcast.Entities.Scx;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Converters.Scx
{
    public interface IInventoryScxDataPrep
    {
        List<ScxData> GetInventoryScxData(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames);
    }
}
