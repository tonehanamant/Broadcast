using System;
using System.Web.Compilation;

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
        /// Gets or sets the identifier of the agency which the advertiser belongs to
        /// </summary>
        /// <value>
        /// The agency id.
        /// </value>
        public int? AgencyId { get; set; }

        /// <summary>
        /// Gets or sets the agency master identifier.
        /// </summary>
        public Guid? AgencyMasterId { get; set; }
    }
}
