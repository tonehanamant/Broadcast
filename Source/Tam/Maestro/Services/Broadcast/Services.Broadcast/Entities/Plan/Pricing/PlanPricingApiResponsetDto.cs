using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.Pricing
{
        public class PlanPricingApiResponsetDto
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("results")]
        public PlanPricingApiResultDto Results { get; set; }
    }
}