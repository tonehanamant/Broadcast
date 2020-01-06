using Newtonsoft.Json;

namespace Services.Broadcast.Entities.DTO.Program
{
    public class SearchProgramDativaResponseDto
    {
        [JsonProperty(PropertyName = "program_id")]
        public string ProgramId { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string ProgramName { get; set; }

        [JsonProperty(PropertyName = "genreid")]
        public string GenreId { get; set; }

        [JsonProperty(PropertyName = "genre")]
        public string Genre { get; set; }

        [JsonProperty(PropertyName = "show_type")]
        public string ShowType { get; set; }

        [JsonProperty(PropertyName = "mpaa_rating")]
        public string MpaaRating { get; set; }

        [JsonProperty(PropertyName = "syn_type")]
        public string SyndicationType { get; set; }
    }
}