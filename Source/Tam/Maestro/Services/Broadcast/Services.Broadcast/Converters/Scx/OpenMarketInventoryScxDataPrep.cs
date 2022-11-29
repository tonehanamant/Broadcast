using Services.Broadcast.Entities.Scx;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Converters.Scx
{
    public interface IOpenMarketInventoryScxDataPrep
    {
        /// <summary>
        /// Gets the SCX Data for the quarter passed as parameter
        /// </summary>
        /// <param name="inventorySourceId">Inventory source id</param>
        /// <param name="daypartIds">Daypart code id</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="exportGenreIds">Genre IDS</param>
        /// <param name="marketRanks">market code</param>
        /// <param name="affiliates">List of affiliates</param>
        /// <returns>List of open market ScxData objects containing the data required</returns>
        List<OpenMarketScxData> GetInventoryScxOpenMarketData(int inventorySourceId, List<int> daypartIds, DateTime startDate, DateTime endDate, string marketRanks, List<int> exportGenreIds, List<string> affiliates);
    }
}
