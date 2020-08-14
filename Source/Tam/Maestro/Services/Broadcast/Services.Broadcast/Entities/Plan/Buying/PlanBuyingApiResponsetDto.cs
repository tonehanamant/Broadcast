using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingApiResponsetDto
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("results")]
        public PlanBuyingApiResultDto Results { get; set; }
    }
}