using System;

namespace Services.Broadcast.Entities.Isci
{
    public class MappingDetailsDto
    {
        public int PlanIsciMappingId { get; set; }
        public string FlightStartDate { get; set; }
        public string FlightEndDate { get; set; }
        public string FlightString { get; set; }
        public int? StartTime { get; set; }
        public int? EndTime { get; set; }
    }
}