using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class RecommendedPlanDetailDto
    {
        public RecommendedPlanDetailDto()
        {
            SpotLengths = new List<SpotLengthDto>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<SpotLengthDto> SpotLengths { get; set; }
    }
}
