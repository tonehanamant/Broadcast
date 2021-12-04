using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingApiRequestDto_v3
    {
        [JsonProperty("contract")]
        public List<PlanBuyingApiRequestWeekDto_v3> Weeks { get; set; } = new List<PlanBuyingApiRequestWeekDto_v3>();

        [JsonProperty("inventory")]
        public List<PlanBuyingApiRequestSpotsDto_v3> Spots { get; set; } = new List<PlanBuyingApiRequestSpotsDto_v3>();
        
        [JsonProperty("configuration")]
        public PlanBuyingApiRequestConfigurationDto Configuration { get; set; } = new PlanBuyingApiRequestConfigurationDto();
    }

    public class PlanBuyingApiRequestConfigurationDto
    {
        [JsonProperty("requirement")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PlanBuyingBudgetCpmLeverEnum BudgetCpmLever { get; set; } = PlanBuyingBudgetCpmLeverEnum.impressions;
    }

    public enum PlanBuyingBudgetCpmLeverEnum
    {
        /// <summary>
        /// "impressions" :  impressions must be greater than or equal to impression goal. Default.
        /// </summary>
        impressions,

        /// <summary>
        /// "budget" : budget must be less than or equal to budget goal
        /// </summary>
        budget,

        /// <summary>
        /// "cpm" : CPM must be less than or equal to CPM goal
        /// </summary>
        cpm
    }

    public class PlanBuyingApiRequestWeekDto_v3
    {
        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }

        [JsonProperty("impression_goal")]
        public double ImpressionGoal { get; set; }

        [JsonProperty("cpm_goal")]
        public decimal CpmGoal { get; set; }

        [JsonProperty("market_coverage_goal")]
        public double MarketCoverageGoal { get; set; }

        [JsonProperty("frequency_cap")]
        public int FrequencyCap { get; set; }

        [JsonProperty("share_of_voice")]
        public List<ShareOfVoice> ShareOfVoice { get; set; }

        [JsonProperty("spot_lengths")]
        public List<SpotLength_v3> SpotLengths { get; set; }

        [JsonProperty("daypart_weighting")]
        public List<DaypartWeighting> DaypartWeighting { get; set; }
    }

    public class PlanBuyingApiRequestSpotsDto_v3
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("week_id")]
        public int MediaWeekId { get; set; }

        [JsonProperty("impressions")]
        public double Impressions30sec { get; set; }

        [JsonProperty("station_id")]
        public int StationId { get; set; }

        [JsonProperty("market_code")]
        public int MarketCode { get; set; }

        [JsonProperty("daypart_id")]
        public int DaypartId { get; set; }

        [JsonProperty("percentage_of_us")]
        public double PercentageOfUs { get; set; }

        [JsonProperty("spot_days")]
        public int SpotDays { get; set; }

        [JsonProperty("spot_hours")]
        public double SpotHours { get; set; }

        [JsonProperty("spot_cost")]
        public List<SpotCost_v3> SpotCost { get; set; }
    }
}