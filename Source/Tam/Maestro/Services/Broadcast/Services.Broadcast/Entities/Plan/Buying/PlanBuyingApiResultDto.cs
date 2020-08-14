using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingApiResultDto
    {
        [JsonProperty("optimal_cpm")]
        public decimal OptimalCpm { get; set; }
        [JsonProperty("spots")]
        public List<PlanBuyingApiResultSpotDto> Spots { get; set; } = new List<PlanBuyingApiResultSpotDto>();
    }

    public class PlanBuyingApiResultSpotDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }
        [JsonProperty("daypart_id")]
        public int DaypartId { get; set; }
        [JsonProperty("impressions")]
        public double Impressions { get; set; }
        [JsonProperty("cost")]
        public decimal Cost { get; set; }
        [JsonProperty("spots")]
        public int Spots { get; set; }
    }
}
