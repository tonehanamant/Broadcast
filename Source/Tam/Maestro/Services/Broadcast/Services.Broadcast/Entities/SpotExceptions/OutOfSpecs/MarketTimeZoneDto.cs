using System;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class MarketTimeZoneDto
    {
        /// <summary>
        /// Gets or sets the Name 
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the MarketCode
        /// </summary>
        /// <value>The market code</value>
        public int MarketCode { get; set; }

        /// <summary>
        /// Gets or sets the Code
        /// </summary>
        /// <value>The code.</value>
        public string Code { get; set; }
    }

}