using System.Collections.Generic;
using Newtonsoft.Json;

namespace Services.Broadcast.Entities.ProgramGuide
{
    public class SearchRequestProgramDto
    {
        [JsonProperty(PropertyName = "name")]
        public string ProgramName { get; set; }

        //[JsonProperty(PropertyName = "start")]
        //public int Start { get; set; }

        //[JsonProperty(PropertyName = "limit")]
        //public int Limit { get; set; } = 20;

        [JsonProperty(PropertyName = "genres")]
        public List<SearchRequestProgramGenreDto> Genres { get; set; }
    }
}