using Newtonsoft.Json;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A request element sent to the external api.
    /// </summary>
    public class GuideApiRequestElementDto
    {
        [JsonProperty(PropertyName = "id")]
        public string RequestElementId { get; set; }

        [JsonProperty(PropertyName = "daypart")]
        public GuideApiRequestDaypartDto Daypart { get; set; }

        [JsonProperty(PropertyName = "startdate")]
        public string StartDate { get; set; }

        [JsonProperty(PropertyName = "enddate")]
        public string EndDate { get; set; }

        [JsonProperty(PropertyName = "station")]
        public string StationCallLetters { get; set; }

        [JsonProperty(PropertyName = "affiliate")]
        public string NetworkAffiliate { get; set; }
    }
}