using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciMappingDetailsDto
    {
        public string Isci { get; set; }
        public List<MappingDetailsDto> Mappings { get; set; }
    }
}