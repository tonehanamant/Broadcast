using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class StationInventoryGroup
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string DaypartCode { get; set; }
        public int SlotNumber { get; set; }

        public string BuildName()
        {
            return DaypartCode + SlotNumber;
        }
    }
}
