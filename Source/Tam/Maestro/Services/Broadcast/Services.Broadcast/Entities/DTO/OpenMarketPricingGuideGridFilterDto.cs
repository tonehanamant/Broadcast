using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class OpenMarketPricingGuideGridFilterDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ProgramNames { get; set; }

        public List<string> Affiliations { get; set; }

        public List<int> Markets { get; set; }

        public List<int> Genres { get; set; }

        public List<DaypartDto> DayParts { get; set; }
    }
}
