using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingApiSpotsResponseDto
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("results")]
        public List<PlanBuyingApiSpotsResultDto> Results { get; set; }
        [JsonProperty("error")]
        public PlanBuyingApiSpotsErrorDto Error { get; set; }
    }

    public class PlanBuyingApiSpotsErrorDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("messages")]
        public List<string> Messages { get; set; }
    }

    public class PlanBuyingApiSpotsResultDto
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }
        [JsonProperty("id")]
        public int ManifestId { get; set; }
        [JsonProperty("frequency")]
        public int Frequency { get; set; }
    }
}
