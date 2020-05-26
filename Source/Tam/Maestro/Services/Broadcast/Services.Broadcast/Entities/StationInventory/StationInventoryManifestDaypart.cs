using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryManifestDaypart
    {
        public int? Id { get; set; }
        public DisplayDaypart Daypart { get; set; }
        public string ProgramName { get; set; }
        public DaypartDefaultDto DaypartDefault { get; set; }
        public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
        public int? PrimaryProgramId { get; set; }
        public List<StationInventoryManifestDaypartProgram> Programs = new List<StationInventoryManifestDaypartProgram>();
    }
}
