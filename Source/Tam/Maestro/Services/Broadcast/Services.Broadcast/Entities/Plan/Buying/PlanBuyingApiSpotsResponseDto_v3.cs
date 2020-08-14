using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingApiSpotsResponseDto_v3
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("results")]
        public List<PlanBuyingApiSpotsResultDto_v3> Results { get; set; }

        [JsonProperty("error")]
        public PlanBuyingApiSpotsErrorDto_v3 Error { get; set; }
    }

    public class PlanBuyingApiSpotsResultDto_v3
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }

        [JsonProperty("id")]
        public int ManifestId { get; set; }

        [JsonProperty("frequencies")]
        public List<SpotFrequencyResponse> Frequencies { get; set; }
    }

    public class PlanBuyingApiSpotsErrorDto_v3
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("messages")]
        public List<string> Messages { get; set; }
    }
}
