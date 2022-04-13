using System.Collections.Generic;
using Tam.Maestro.Data.Entities;
using Services.Broadcast.Entities.Enums;
using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingInventoryRawDto
    {
        [JsonProperty("posting_type")]
        public PostingTypeEnum PostingType { get; set; }
        [JsonProperty("alloc_mode")]
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
        [JsonProperty("allocated_spots")]
        public List<PlanBuyingSpotRaw> AllocatedSpotsRaw { get; set; } = new List<PlanBuyingSpotRaw>();
        [JsonProperty("unallocated_spots")]
        public List<PlanBuyingSpotRaw> UnallocatedSpotsRaw { get; set; } = new List<PlanBuyingSpotRaw>();
    }

    public class PlanBuyingSpotRaw
    {
        [JsonProperty("manifest_id")]
        public int StationInventoryManifestId { get; set; }
        [JsonProperty("conv_rate")]
        public double PostingTypeConversationRate { get; set; }
        [JsonProperty("imweek_id")]
        public int InventoryMediaWeekId { get; set; }
        [JsonProperty("imps30")]
        public double Impressions30sec { get; set; }
        [JsonProperty("cmweek_id")]
        public int ContractMediaWeekId { get; set; }
        [JsonProperty("daypart_id")]
        public int StandardDaypartId { get; set; }
        [JsonProperty("frequencies")]
        public List<SpotFrequencyRaw> SpotFrequenciesRaw { get; set; } = new List<SpotFrequencyRaw>();
    }

    public class SpotFrequencyRaw
    {
        [JsonProperty("length")]
        public int SpotLengthId { get; set; }
        [JsonProperty("cost")]
        public decimal SpotCost { get; set; }
        [JsonProperty("spots")]
        public int Spots { get; set; }
        [JsonProperty("impressions")]
        public double Impressions { get; set; }
    }
}
