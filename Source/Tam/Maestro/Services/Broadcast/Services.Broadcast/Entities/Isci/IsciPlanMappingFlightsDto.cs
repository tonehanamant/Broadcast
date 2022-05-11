using System;

namespace Services.Broadcast.Entities
{
    public class IsciPlanMappingFlightsDto
    {
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }        
        public int? StartTime { get; set; }
        public int? EndTime { get; set; }
    }
}
