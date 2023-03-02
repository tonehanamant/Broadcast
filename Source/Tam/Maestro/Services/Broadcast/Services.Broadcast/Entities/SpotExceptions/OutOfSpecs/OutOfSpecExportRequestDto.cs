using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary>Out Of Spec Export Request</summary>
    public class OutOfSpecExportRequestDto
    {
        /// <summary>
        /// Gets or sets the week start date.
        /// </summary>
        /// <value>The week start date.</value>
        public DateTime WeekStartDate { get; set; }

        /// <summary>
        /// Gets or sets the week end date.
        /// </summary>
        /// <value>The week end date.</value>
        public DateTime WeekEndDate { get; set; }
        /// <summary>
        /// Gets or sets the Inventory Source Names.
        /// </summary>
        /// <value>The Inventory Source Names.</value>
        public List<string> InventorySourceNames { get; set; } = new List<string>();
        /// <summary>
        /// Gets or sets the Advertisers Plan Ids.
        /// </summary>
        /// <value>The Advertisers Plan Ids.</value>
        public List<int> AdvertisersPlanIds { get; set; } = new List<int>();
        /// <summary>
        /// Gets or sets the Advertisers master ids.
        /// </summary>
        /// <value>The AdvertisersMasterIds.</value>
        public List<Guid> AdvertisersMasterIds { get; set; } = new List<Guid>();
    }
}
