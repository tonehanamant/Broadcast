
using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class SpotCost_v3
    {
        [JsonProperty("spot_length_id")]
        public int SpotLengthId { get; set; }

        [JsonProperty("spot_length_cost")]
        public decimal SpotLengthCost { get; set; }
    }
}
