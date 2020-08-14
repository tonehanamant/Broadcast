using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingApiCpmResponseDto
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("results")]
        public PlanBuyingApiCpmResultDto Results { get; set; }
    }

    public class PlanBuyingApiCpmResultDto
    {
        [JsonProperty("minimum_cost")]
        public decimal MinimumCost { get; set; }
    }
}
