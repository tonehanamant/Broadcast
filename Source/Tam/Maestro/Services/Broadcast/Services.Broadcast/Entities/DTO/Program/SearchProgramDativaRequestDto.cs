using Newtonsoft.Json;

namespace Services.Broadcast.Entities.DTO.Program
{
    public class SearchProgramDativaRequestDto
    {
        [JsonProperty(PropertyName = "name")]
        public string ProgramName { get; set; }

        [JsonProperty(PropertyName = "start")]
        public int? Start { get; set; }

        [JsonProperty(PropertyName = "limit")]
        public int? Limit { get; set; }
    }
}