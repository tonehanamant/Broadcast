using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class OpenMarketPricingGuideGridFilterDto
    {
        public enum OpenMarketSpotFilter
        {
            AllPrograms = 1,
            ProgramWithSpots = 2,
            ProgramWithoutSpots = 3
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ProgramNames { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Affiliations { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> Markets { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> Genres { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<DaypartDto> DayParts { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public OpenMarketSpotFilter? SpotFilter { get; set; }
    }
}
