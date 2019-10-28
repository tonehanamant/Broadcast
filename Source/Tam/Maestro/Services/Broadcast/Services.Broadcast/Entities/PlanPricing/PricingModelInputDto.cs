using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.PlanPricing
{
    public class PricingModelInputDto
    {
        [JsonProperty("contract")]
        public List<PricingModelWeekInputDto> Weeks { get; set; } = new List<PricingModelWeekInputDto>();
        [JsonProperty("inventory")]
        public List<PricingModelSpotsDto> Spots { get; set; } = new List<PricingModelSpotsDto>();
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
