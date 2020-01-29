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
    }

    public class PlanPricingApiRequestSpotsDto
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

        // Not supported by Data Science API yet.

        //[JsonProperty("unit")]
        //public string Unit { get; set; }

        //[JsonProperty("inventory_source")]
        //public string InventorySource { get; set; }

        //[JsonProperty("inventory_source_type")]
        //public string InventorySourceType { get; set; }
    }

    public class PlanPricingApiRequestWeekDto
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }
        [JsonProperty("impression_goal")]
        public double ImpressionGoal { get; set; }
        [JsonProperty("cpm_goal")]
        public decimal CpmGoal { get; set; }
    }
}