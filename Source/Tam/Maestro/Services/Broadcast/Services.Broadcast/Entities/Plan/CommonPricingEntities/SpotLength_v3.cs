
using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class SpotLength_v3
    {
        [JsonProperty("spot_length_id")]
        public int SpotLengthId { get; set; }

        [JsonProperty("spot_length_goal")]
        public double SpotLengthGoal { get; set; }

        [JsonProperty("spot_scale_factor")]
        public double SpotScaleFactor { get; set; }
    }
}
