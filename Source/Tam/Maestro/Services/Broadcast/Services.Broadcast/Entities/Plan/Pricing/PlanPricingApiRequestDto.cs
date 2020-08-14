using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
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

        [JsonProperty("station_id")]
        public int StationId { get; set; }

        [JsonProperty("market_code")]
        public int MarketCode { get; set; }

        [JsonProperty("percentage_of_us")]
        public double PercentageOfUs { get; set; }

        [JsonProperty("spot_days")]
        public int SpotDays { get; set; }

        [JsonProperty("spot_hours")]
        public double SpotHours { get; set; }
    }

    public class PlanPricingApiRequestWeekDto
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }

        [JsonProperty("impression_goal")]
        public double ImpressionGoal { get; set; }

        [JsonProperty("cpm_goal")]
        public decimal CpmGoal { get; set; }

        [JsonProperty("market_coverage_goal")]
        public double MarketCoverageGoal { get; set; }

        [JsonProperty("frequency_cap_spots")]
        public int FrequencyCapSpots { get; set; }

        [JsonProperty("frequency_cap_time")]
        public double FrequencyCapTime { get; set; }

        [JsonProperty("frequency_cap_unit")]
        public string FrequencyCapUnit { get; set; }

        [JsonProperty("share_of_voice")]
        public List<ShareOfVoice> ShareOfVoice { get; set; }

        [JsonProperty("daypart_weighting")]
        public List<DaypartWeighting> DaypartWeighting { get; set; }
    }
}