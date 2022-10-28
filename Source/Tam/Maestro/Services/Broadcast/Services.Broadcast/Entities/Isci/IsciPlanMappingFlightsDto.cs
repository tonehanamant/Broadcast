using System;

namespace Services.Broadcast.Entities
{
    public class IsciPlanMappingFlightsDto
    {
        public Nullable<DateTime> FlightStartDate { get; set; }
        public Nullable<DateTime> FlightEndDate { get; set; }        
        public int? StartTime { get; set; }
        public int? EndTime { get; set; }
    }
}
