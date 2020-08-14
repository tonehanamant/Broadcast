using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class SpotFrequencyResponse
    {
        [JsonProperty("spot_length_id")]
        public int SpotLengthId { get; set; }

        [JsonProperty("frequency")]
        public int Frequency { get; set; }
    }
}
