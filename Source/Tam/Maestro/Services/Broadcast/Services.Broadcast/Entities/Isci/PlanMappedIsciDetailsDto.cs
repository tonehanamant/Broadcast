using System;

namespace Services.Broadcast.Entities.Isci
{
    public class PlanMappedIsciDetailsDto
    {
        public int PlanIsciMappingId { get; set; }
        public string Isci { get; set; }
        public string SpotLengthString { get; set; }
        public int SpotLengthId { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public string FlightString { get; set; }
        public DateTime AvailabilityStartDate { get; set; }
        public DateTime AvailabilityEndDate { get; set; }
        public string AvailabilityString { get; set; }
    }
}