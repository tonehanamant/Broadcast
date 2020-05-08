using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Inventory;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    /// <summary>
    /// A test class to expose some methods for testing.
    /// </summary>
    /// <seealso cref="Services.Broadcast.BusinessEngines.InventoryExportEngine" />
    public class InventoryExportEngineUnitTestClass : InventoryExportEngine
    {
        public List<ColumnDescriptor> UT_GetInventoryWorksheetColumnDescriptors(List<DateTime> weekStartDates)
        {
            return _GetInventoryWorksheetColumnDescriptors(weekStartDates);
        }

        public List<List<object>> UT_TransformToExportLines(List<InventoryExportLineDetail> lineDetails,
            List<int> weekIds,
            List<DisplayBroadcastStation> stations, Dictionary<int, DisplayDaypart> dayparts)
        {
            return _TransformToExportLines(lineDetails, weekIds, stations, dayparts);
        }
    }
}