using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingApiMinimumCostResultDto
    {
        [JsonProperty("minimum_cost")]
        public decimal MinimumCost { get; set; }
    }
}