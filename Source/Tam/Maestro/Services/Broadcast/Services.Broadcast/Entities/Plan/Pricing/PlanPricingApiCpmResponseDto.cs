using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingApiCpmResponseDto
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("results")]
        public PlanPricingApiCpmResultDto Results { get; set; }
    }

    public class PlanPricingApiCpmResultDto
    {
        [JsonProperty("minimum_cost")]
        public decimal MinimumCost { get; set; }
    }
}
