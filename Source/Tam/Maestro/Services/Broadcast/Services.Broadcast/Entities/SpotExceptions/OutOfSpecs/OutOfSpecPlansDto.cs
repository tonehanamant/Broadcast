using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class OutOfSpecPlansDto
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="OutOfSpecPlansDto" /> class.</summary>
        public OutOfSpecPlansDto()
        {
            SpotLengths = new List<SpotLengthDto>();
        }

        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>The plan identifier.</value>
        public int PlanId { get; set; }

        /// <summary>
        /// Gets or sets the advertiser master identifier.
        /// </summary>
        /// <value>The advertiser master identifier.</value>
        public Guid? AdvertiserMasterId { get; set; }

        /// <summary>
        /// Gets or sets the name of the plan.
        /// </summary>
        /// <value>The name of the plan.</value>
        public string PlanName { get; set; }

        /// <summary>
        /// Gets or sets the affected spots count.
        /// </summary>
        /// <value>The affected spots count.</value>
        public int AffectedSpotsCount { get; set; }

        /// <summary>
        /// Gets or sets the impressions.
        /// </summary>
        /// <value>The impressions.</value>
        public double Impressions { get; set; }

        /// <summary>
        /// Gets or sets the synced timestamp.
        /// </summary>
        /// <value>The synced timestamp.</value>
        public DateTime? SyncedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the spot length string.
        /// </summary>
        /// <value>The spot length string.</value>
        public string SpotLengthString { get; set; }

        /// <summary>
        /// Gets or sets the name of the audience.
        /// </summary>
        /// <value>The name of the audience.</value>
        public string AudienceName { get; set; }

        /// <summary>
        /// Gets or sets the flight start date.
        /// </summary>
        /// <value>The flight start date.</value>
        public DateTime FlightStartDate { get; set; }

        /// <summary>
        /// Gets or sets the flight end date.
        /// </summary>
        /// <value>The flight end date.</value>
        public DateTime FlightEndDate { get; set; }

        /// <summary>
        /// Gets or sets the spot lengths.
        /// </summary>
        /// <value>The spot lengths.</value>
        public List<SpotLengthDto> SpotLengths { get; set; }
    }
}
