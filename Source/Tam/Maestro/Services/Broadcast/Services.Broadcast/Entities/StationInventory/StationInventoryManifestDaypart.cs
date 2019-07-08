﻿using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryManifestDaypart
    {
        public int? Id { get; set; }
        public DisplayDaypart Daypart { get; set; }
        public string ProgramName { get; set; }
        public DaypartCode DaypartCode { get; set; }
        public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
    }
}
