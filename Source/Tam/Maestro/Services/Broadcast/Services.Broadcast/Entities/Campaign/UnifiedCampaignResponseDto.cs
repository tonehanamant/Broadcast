using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Campaign
{
    public class UnifiedCampaignResponseDto
    {
        /// <summary>
        /// Returns True if message posted without error otherwise false
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }
        /// <summary>
        /// Return response message with unified campaign id
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }        
    }
}
