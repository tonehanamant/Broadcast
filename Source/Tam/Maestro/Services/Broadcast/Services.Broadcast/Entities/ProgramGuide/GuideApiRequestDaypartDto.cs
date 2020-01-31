using Newtonsoft.Json;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A request daypart sent to the external api.
    /// </summary>
    public class GuideApiRequestDaypartDto
    {
        [JsonProperty(PropertyName = "id")]
        public string RequestDaypartId { get; set; }

        [JsonProperty(PropertyName = "dayparttext")]
        public string Daypart { get; set; }

        [JsonProperty(PropertyName = "mon")]
        public bool Monday { get; set; }

        [JsonProperty(PropertyName = "tue")]
        public bool Tuesday { get; set; }

        [JsonProperty(PropertyName = "wed")]
        public bool Wednesday { get; set; }

        [JsonProperty(PropertyName = "thu")]
        public bool Thursday { get; set; }

        [JsonProperty(PropertyName = "fri")]
        public bool Friday { get; set; }

        [JsonProperty(PropertyName = "sat")]
        public bool Saturday { get; set; }

        [JsonProperty(PropertyName = "sun")]
        public bool Sunday { get; set; }

        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        [JsonProperty(PropertyName = "starttime")]
        public int StartTime { get; set; }

        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        [JsonProperty(PropertyName = "endtime")]
        public int EndTime { get; set; }
    }
}