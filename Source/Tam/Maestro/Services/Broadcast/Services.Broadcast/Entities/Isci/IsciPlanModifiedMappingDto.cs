using System;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanModifiedMappingDto
    {
        public int PlanIsciMappingId { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
    }
}
