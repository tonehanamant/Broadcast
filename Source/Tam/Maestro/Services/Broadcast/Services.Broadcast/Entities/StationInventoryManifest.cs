using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class StationInventoryManifest
    {
        public int? Id { get; set; }
        public DisplayBroadcastStation Station { get; set; }
        public string DaypartCode { get; set; }
        public int SpotLengthId { get; set; } 
        public int SpotsPerWeek { get; set; }
        public int SpotsPerDay { get; set; }
        public List<DisplayDaypart> Dayparts { get; set; }
    }
}
