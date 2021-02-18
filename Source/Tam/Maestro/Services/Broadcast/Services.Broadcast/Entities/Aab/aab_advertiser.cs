using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Aab
{
    /// <summary>
    /// Advertiser object matching the AabApi schema.
    /// </summary>
    public class aab_advertiser
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        public Guid company_id { get; set; }

        /// <summary>
        /// Gets or sets the products.
        /// </summary>
        /// <value>
        /// The products.
        /// </value>
        public List<aab_product> products { get; set; } = new List<aab_product>();
    }
}