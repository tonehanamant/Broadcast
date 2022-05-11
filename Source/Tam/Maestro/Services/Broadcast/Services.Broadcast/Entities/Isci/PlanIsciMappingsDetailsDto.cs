﻿using System;
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
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public string FlightString { get; set; }
        public List<IsciMappingDetailsDto> PlanIsciMappings { get; set; }
    }
}