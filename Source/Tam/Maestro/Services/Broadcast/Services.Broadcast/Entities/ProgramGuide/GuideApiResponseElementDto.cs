using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A response element returned from the external api.
    /// </summary>
    public class GuideApiResponseElementDto
    {
        [JsonProperty(PropertyName = "id")]
        public string RequestElementId { get; set; }

        [JsonProperty(PropertyName = "programs")]
        public List<GuideApiResponseProgramDto> Programs { get; set; }

        [JsonProperty(PropertyName = "daypart")]
        public string RequestDaypartId { get; set; }

        [JsonProperty(PropertyName = "station")]
        public string Station { get; set; }

        [JsonProperty(PropertyName = "affiliate")]
        public string Affiliate { get; set; }

        [JsonProperty(PropertyName = "start_date")]
        public string StartDate { get; set; }

        [JsonProperty(PropertyName = "end_date")]
        public string EndDate { get; set; }
    }
}