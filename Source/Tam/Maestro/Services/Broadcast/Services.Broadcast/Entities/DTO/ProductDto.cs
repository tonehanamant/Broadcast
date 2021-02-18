using System;

namespace Services.Broadcast.Entities.DTO
{
    /// <summary>
    /// Dto for an Advertiser's Product
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the master identifier.
        /// </summary>
        /// <value>
        /// The master identifier.
        /// </value>
        public Guid? MasterId { get; set; }

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
        public int? AdvertiserId { get; set; }

        /// <summary>
        /// Gets or sets the advertiser master identifier.
        /// </summary>
        /// <value>
        /// The advertiser master identifier.
        /// </value>
        public Guid? AdvertiserMasterId { get; set; }
    }
}
