using System;

namespace Services.Broadcast.Entities.Aab
{
    /// <summary>
    /// Product object matching AabApi schema.
    /// </summary>
    public class aab_product
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
    }
}