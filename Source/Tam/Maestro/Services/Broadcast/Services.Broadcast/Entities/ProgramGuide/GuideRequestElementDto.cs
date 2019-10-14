using Newtonsoft.Json;

namespace Services.Broadcast.Entities.ProgramGuide
{
    public class GuideRequestElementDto
    {
        [JsonProperty(PropertyName = "id")]
        public string RequestElementId { get; set; }

        [JsonProperty(PropertyName = "daypart")]
        public GuideRequestDaypartDto Daypart { get; set; }

        [JsonProperty(PropertyName = "startdate")]
        public string StartDate { get; set; }

        [JsonProperty(PropertyName = "enddate")]
        public string EndDate { get; set; }

        [JsonProperty(PropertyName = "station")]
        public string NielsenLegacyStationCallLetters { get; set; }

        [JsonProperty(PropertyName = "affiliate")]
        public string NetworkAffiliate { get; set; }
    }
}