namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Dto for an Advertiser.
    /// </summary>
    public class AdvertiserDto
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
        /// Gets or sets the identifier of the agency which the advertiser belongs to
        /// </summary>
        /// <value>
        /// The agency id.
        /// </value>
        public int AgencyId { get; set; }
    }
}
