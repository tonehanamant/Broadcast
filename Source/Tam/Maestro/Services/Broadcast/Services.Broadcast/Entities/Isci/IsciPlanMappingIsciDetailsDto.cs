using System;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanMappingIsciDetailsDto
    {
        public string Isci { get; set; }
        public string SpotLengthString { get; set; }
        public int SpotLengthId { get; set; }
        public string AdvertiserName { get; set; }
        public string ProductName { get; set; }
        public DateTime ActiveStartDate { get; set; }
        public DateTime ActiveEndDate { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public string FlightString { get; set; }
    }
}