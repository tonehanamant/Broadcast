using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class StationInventoryManifestDaypart
    {
        public DisplayDaypart Daypart { get; set; }
        public string ProgramName { get; set; }

        public StationInventoryManifestDaypart() { }

        public StationInventoryManifestDaypart(DisplayDaypart daypart, string programName)
        {
            this.Daypart = daypart;
            this.ProgramName = programName;
        }
    }
}
