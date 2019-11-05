using Newtonsoft.Json;

namespace Services.Broadcast.Entities.ProgramGuide
{
    public class SearchRequestProgramGenreDto
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }
}