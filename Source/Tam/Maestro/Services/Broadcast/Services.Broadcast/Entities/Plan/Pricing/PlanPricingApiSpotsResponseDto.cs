using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingApiSpotsResponseDto
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("results")]
        public List<PlanPricingApiSpotsResultDto> Results { get; set; } = new List<PlanPricingApiSpotsResultDto>();
        [JsonProperty("error")]
        public PlanPricingApiSpotsErrorDto Error { get; set; }
    }

    public class PlanPricingApiSpotsErrorDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("messages")]
        public List<string> Messages { get; set; }
    }

    public class PlanPricingApiSpotsResultDto
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }
        [JsonProperty("id")]
        public int ManifestId { get; set; }
        [JsonProperty("frequency")]
        public int Frequency { get; set; }
    }
}
