using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    public class RecommendedPlanDetailDto
    {
        public RecommendedPlanDetailDto()
        {
            SpotLengths = new List<SpotLengthDto>();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

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

        /// <summary>
        /// Gets or sets the advertiser master identifier.
        /// </summary>
        /// <value>The advertiser master identifier.</value>
        public Guid? AdvertiserMasterId { get; set; }

        /// <summary>
        /// Gets or sets the product master identifier.
        /// </summary>
        /// <value>The product master identifier.</value>
        public Guid? ProductMasterId { get; set; }
    }
}
