using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingApiSpotsResponseDto
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("results")]
        public List<PlanPricingApiSpotsResultDto> Results { get; set; }
    }

    public class PlanPricingApiSpotsResultDto
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }
        [JsonProperty("allocated_ids")]
        public List<int> AllocatedManifestIds { get; set; }
    }
}
