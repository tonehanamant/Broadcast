using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class DaypartWeighting
    {
        [JsonProperty("daypart_id")]
        public int DaypartId { get; set; }

        [JsonProperty("daypart_goal")]
        public double DaypartGoal { get; set; }
    }
}
