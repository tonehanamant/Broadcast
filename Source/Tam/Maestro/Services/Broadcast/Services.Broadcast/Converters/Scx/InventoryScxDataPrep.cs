using Common.Services.ApplicationServices;
using Services.Broadcast.Entities.Scx;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Converters.Scx
{
    public interface IInventoryScxDataPrep
    {
        /// <summary>
        /// Gets the SCX Data for the quarter passed as parameter
        /// </summary>
        /// <param name="inventorySourceId">Inventory source id</param>
        /// <param name="daypartCodeId">Daypart code id</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="unitNames">List of unit names</param>
        /// <returns>List of ScxData objects containing the data required</returns>
        List<ScxData> GetInventoryScxData(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames);
    }
}
