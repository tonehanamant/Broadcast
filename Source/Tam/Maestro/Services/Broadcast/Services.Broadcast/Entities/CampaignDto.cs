using System;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Dto for Campaign data.
    /// </summary>
    public class CampaignDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the advertiser identifier.
        /// </summary>
        /// <value>
        /// The advertiser identifier.
        /// </value>
        public int AdvertiserId { get; set; }

        /// <summary>
        /// Gets or sets the agency identifier.
        /// </summary>
        /// <value>
        /// The agency identifier.
        /// </value>
        public int AgencyId { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the modified date.
        /// </summary>
        /// <value>
        /// The modified date.
        /// </value>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the modified by.
        /// </summary>
        /// <value>
        /// The modified by.
        /// </value>
        public string ModifiedBy { get; set; }
    }
}