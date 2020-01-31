using System;
using Newtonsoft.Json;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A response program returned from the external api.
    /// </summary>
    public class GuideApiResponseProgramDto
    {
        [JsonProperty(PropertyName = "program_id")]
        public string ProgramId { get; set; }

        [JsonProperty(PropertyName = "program")]
        public string ProgramName { get; set; }

        [JsonProperty(PropertyName = "genreid")]
        public string GenreId { get; set; }

        [JsonProperty(PropertyName = "genre")]
        public string Genre { get; set; }

        [JsonProperty(PropertyName = "showtype")]
        public string ShowType { get; set; }

        [JsonProperty(PropertyName = "syndicationtype")]
        public string SyndicationType { get; set; }

        [JsonProperty(PropertyName = "occurances")]
        public int Occurrences { get; set; }

        [JsonProperty(PropertyName = "startdate")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "enddate")]
        public DateTime EndDate { get; set; }

        [JsonProperty(PropertyName = "starttime")]
        public string StartTimeString { get; set; }

        [JsonProperty(PropertyName = "endtime")]
        public string EndTimeString { get; set; }

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
    }
}