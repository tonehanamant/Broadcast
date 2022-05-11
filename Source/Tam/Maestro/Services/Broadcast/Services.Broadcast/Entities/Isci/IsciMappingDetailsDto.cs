using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciMappingDetailsDto
    {
        public string Isci { get; set; }
        public string SpotLengthString { get; set; }
        public int SpotLengthId { get; set; }
        public List<MappingDetailsDto> PlanIsciMappingsFlights { get; set; }
    }
}