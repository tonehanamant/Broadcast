using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Services.Broadcast.Entities.ProgramGuide
{
    public class GuideResponseElementDto
    {
        [JsonProperty(PropertyName = "id")]
        public string RequestElementId { get; set; }

        [JsonProperty(PropertyName = "programs")]
        public List<GuideResponseProgramDto> Programs { get; set; }

        [JsonProperty(PropertyName = "daypart")]
        public string RequestDaypartId { get; set; }

        [JsonProperty(PropertyName = "station")]
        public string Station { get; set; }

        [JsonProperty(PropertyName = "startdate")]
        public string StartDate { get; set; }

        [JsonProperty(PropertyName = "enddate")]
        public string EndDate { get; set; }
    }
}