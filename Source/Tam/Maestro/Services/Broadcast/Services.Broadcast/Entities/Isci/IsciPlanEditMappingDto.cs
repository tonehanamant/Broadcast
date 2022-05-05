using System.Collections.Generic;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanEditMappingDto
    {
        public int PlanId { get; set; }
        public string Isci { get; set; }
        public List<IsciPlanMappingEditFlightsDto> IsciPlanMappingFlights { get; set; }
    }
}
