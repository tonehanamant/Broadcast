using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class DistributionProgram
    {
        public LookupDto Market { get; set; }
        public DisplayScheduleStation Station { get; set; }
        public LookupDto ManifestDaypart { get; set; }
        public LookupDto Daypart { get; set; }
        public decimal BlendedCpm { get; set; }
        public int Spots { get; set; }
        public double ImpressionsPerSpot { get; set; }
        public decimal CostPerSpot { get; set; }
        public double StationImpressionsPerSpot { get; set; }
        public List<LookupDto> Genres { get; set; }
    }
}
