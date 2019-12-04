using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingApiRequestDto
    {
        [JsonProperty("contract")]
        public List<PlanPricingApiRequestWeekDto> Weeks { get; set; } = new List<PlanPricingApiRequestWeekDto>();
        [JsonProperty("inventory")]
        public List<PlanPricingApiRequestSpotsDto> Spots { get; set; } = new List<PlanPricingApiRequestSpotsDto>();
        [JsonProperty("params")]
        public PlanPricingApiRequestParametersDto Parameters { get; set; }
    }

    public class PlanPricingApiRequestSpotsDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }
        [JsonProperty("impressions")]
        public double Impressions { get; set; }
        [JsonProperty("cost")]
        public decimal Cost { get; set; }
    }

    public class PlanPricingApiRequestWeekDto
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }
        [JsonProperty("impression_goal")]
        public double Impressions { get; set; }
    }
}