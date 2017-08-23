using Newtonsoft.Json;

namespace Services.Broadcast.Entities
{
    public class ProposalTotalFields
    {
        public int TotalSpots { get; set; }
        public decimal TotalCost { get; set; }
        [JsonIgnore]
        public float TotalTargetImpressions { get; set; }

        [JsonProperty(PropertyName = "TotalTargetImpressions")]
        public float TotalTagetImpressionsInThousands
        {
            get { return TotalTargetImpressions / 1000; }
        }
        public decimal TotalTargetCPM { get; set; }
        public float TotalTRP { get; set; }
        public float TotalGRP { get; set; }
        public decimal TotalHHCPM { get; set; }
        [JsonIgnore]
        public float TotalHHImpressions { get; set; }

        [JsonProperty(PropertyName = "TotalHHImpressions")]
        public float TotalHHImpressionsInThousands
        {
            get { return TotalHHImpressions / 1000; }
        }
        [JsonIgnore]
        public float TotalAdditionalAudienceImpressions { get; set; }

        [JsonProperty(PropertyName = "TotalAdditionalAudienceImpressions")]
        public float TotalAdditionalAudienceImpressionsInThousands
        {
            get { return TotalAdditionalAudienceImpressions / 1000; }
        }
        public decimal TotalAdditionalAudienceCPM { get; set; }
    }
}
