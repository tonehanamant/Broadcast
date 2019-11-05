using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingApiResultDto
    {
        [JsonProperty("minimum_cost")]
        public decimal MinimumCost { get; set; }
    }
}