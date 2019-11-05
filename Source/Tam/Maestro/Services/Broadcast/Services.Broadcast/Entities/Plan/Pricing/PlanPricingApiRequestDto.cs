using Newtonsoft.Json;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingApiRequestDto
    {
        [JsonProperty("contract")]
        public List<PricingModelWeekInputDto> Weeks { get; set; } = new List<PricingModelWeekInputDto>();
        [JsonProperty("inventory")]
        public List<PricingModelSpotsDto> Spots { get; set; } = new List<PricingModelSpotsDto>();
        [JsonProperty("params")]
        public PlanPricingApiRequestParametersDto Parameters { get; set; }
    }

    public class PricingModelSpotsDto
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

    public class PricingModelWeekInputDto
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }
        [JsonProperty("impression_goal")]
        public double Impressions { get; set; }
    }
}