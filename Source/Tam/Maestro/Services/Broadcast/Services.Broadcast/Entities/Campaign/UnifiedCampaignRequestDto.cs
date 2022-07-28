namespace Services.Broadcast.Entities.Campaign
{
    public class UnifiedCampaignRequestDto
    {
        /// <summary>
        /// The ID of the unified campaign
        /// </summary>       
        public string UnifiedCampaignId { get; set; }
    }
    public class PublishUnifiedCampaignRequestDto
    {
        /// <summary>
        /// The Identifier of the campaign
        /// </summary>
        public int CampaignId { get; set; }
    }
}
