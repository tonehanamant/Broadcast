using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class OpenMarketPricingGuideGridFilterDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ProgramNames { get; set; }
    }
}
