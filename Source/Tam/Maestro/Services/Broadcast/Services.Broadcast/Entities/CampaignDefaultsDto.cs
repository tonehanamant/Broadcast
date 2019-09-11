namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Gets the default values for creating a new campaign
    /// </summary>
    public class CampaignDefaultsDto
    {
        /// <summary>
        /// Gets or sets the default name value for a campaign.
        /// </summary>
        /// <value>
        /// The default name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default advertiser identifier for a campaign.
        /// </summary>
        /// <value>
        /// The advertiser identifier.
        /// </value>
        public int? AdvertiserId { get; set; }


        /// <summary>
        /// Gets or sets the default agency identifier for a campaign.
        /// </summary>
        /// <value>
        /// The agency identifier.
        /// </value>
        public int? AgencyId { get; set; }

        /// <summary>
        /// Gets or sets the default notes for a campaign.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        public string Notes { get; set; }
    }
}
