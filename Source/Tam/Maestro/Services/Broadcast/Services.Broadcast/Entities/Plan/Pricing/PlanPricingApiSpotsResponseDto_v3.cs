using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingApiSpotsResponseDto_v3
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("results")]
        public List<PlanPricingApiSpotsResultDto_v3> Results { get; set; }

        [JsonProperty("error")]
        public PlanPricingApiSpotsErrorDto_v3 Error { get; set; }
    }

    public class PlanPricingApiSpotsResultDto_v3
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }

        [JsonProperty("id")]
        public int ManifestId { get; set; }

        [JsonProperty("frequencies")]
        public List<SpotFrequency> Frequencies { get; set; }
    }

    public class SpotFrequency
    {
        [JsonProperty("spot_length_id")]
        public int SpotLengthId { get; set; }

        [JsonProperty("frequency")]
        public int Frequency { get; set; }
    }

    public class PlanPricingApiSpotsErrorDto_v3
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("messages")]
        public List<string> Messages { get; set; }
    }
}
