using Newtonsoft.Json;

namespace Services.Broadcast.Entities.ProgramGuide
{
    public class GuideRequestDaypartDto
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

        /*
         * 11/14/2019
         * The current Api version is incorrect in that the times are only accepted as "HH:mm" rather than "secondsSinceMidnight".
         *
         */

        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        //[JsonProperty(PropertyName = "starttime")]
        [JsonIgnore]
        public int StartTime { get; set; }

        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        //[JsonProperty(PropertyName = "endtime")]
        [JsonIgnore]
        public int EndTime { get; set; }

        [JsonProperty(PropertyName = "starttime")]
        public string StartTimeString { get; set; }

        [JsonProperty(PropertyName = "endtime")]
        public string EndTimeString { get; set; }
    }
}