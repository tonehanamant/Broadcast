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
        
        public List<LookupDto> Genres { get; set; }

        public StationInventoryManifestDaypart()
        {
            Genres = new List<LookupDto>();
        }

        public StationInventoryManifestDaypart(int id,DisplayDaypart daypart, string programName)
        {
            Id = id;
            Daypart = daypart;
            ProgramName = programName;
            Genres = new List<LookupDto>();
        }
    }
}
