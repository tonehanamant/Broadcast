using System;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanModifiedMappingDto
    {
        public int PlanIsciMappingId { get; set; }
        public Nullable<DateTime> FlightStartDate { get; set; }
        public Nullable<DateTime> FlightEndDate { get; set; }
        public int? StartTime { get; set; }
        public int? EndTime { get; set; }
    }
}
