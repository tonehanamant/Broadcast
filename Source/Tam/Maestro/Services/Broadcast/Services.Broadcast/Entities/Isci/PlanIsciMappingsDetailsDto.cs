using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Isci
{
    public class PlanIsciMappingsDetailsDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public string AdvertiserName { get; set; }
        public string SpotLengthString { get; set; }
        public string DaypartCode { get; set; }
        public string DemoString { get; set; }
        public string FlightStartDate { get; set; }
        public string FlightEndDate { get; set; }
        public string FlightString { get; set; }
        public List<IsciMappingDetailsDto> IsciPlanMappings { get; set; }
    }
}