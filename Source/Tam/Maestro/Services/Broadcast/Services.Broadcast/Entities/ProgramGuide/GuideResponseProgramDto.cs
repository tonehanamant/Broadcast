using Newtonsoft.Json;

namespace Services.Broadcast.Entities.ProgramGuide
{
    public class GuideResponseProgramDto
    {
        [JsonProperty(PropertyName = "programid")]
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
        public int Occurances { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        public int StartTime { get; set; }

        [JsonProperty(PropertyName = "end_time")]
        public int EndTime { get; set; }
    }
}