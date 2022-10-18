using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    /// <summary></summary>
    public class SpotExceptionsRecommendedPlansResultsDto
    {
        /// <summary>Initializes a new instance of the 
        /// <see cref="SpotExceptionsRecommendedPlansResultsDto" /> class.</summary>
        public SpotExceptionsRecommendedPlansResultsDto()
        {
            Active = new List<SpotExceptionsRecommendedToDoPlansDto>();
            Completed = new List<SpotExceptionsRecommendedDonePlansDto>();
        }

        /// <summary>
        /// Gets or sets the active.
        /// </summary>
        /// <value>The active.</value>
        public List<SpotExceptionsRecommendedToDoPlansDto> Active { get; set; }

        /// <summary>
        /// Gets or sets the completed.
        /// </summary>
        /// <value>The completed.</value>
        public List<SpotExceptionsRecommendedDonePlansDto> Completed { get; set; }
    }

    /// <summary></summary>
    public class SpotExceptionsRecommendedToDoPlansDto
    {

        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>The plan identifier.</value>
        public int? PlanId { get; set; }

        /// <summary>
        /// Gets or sets the name of the advertiser.
        /// </summary>
        /// <value>The name of the advertiser.</value>
        public string AdvertiserName { get; set; }

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
        public double? Impressions { get; set; }

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
        /// Gets or sets the flight string.
        /// </summary>
        /// <value>The flight string.</value>
        public string FlightString { get; set; }
    }

    /// <summary></summary>
    public class SpotExceptionsRecommendedDonePlansDto
    {
        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>The plan identifier.</value>
        public int? PlanId { get; set; }

        /// <summary>
        /// Gets or sets the name of the advertiser.
        /// </summary>
        /// <value>The name of the advertiser.</value>
        public string AdvertiserName { get; set; }

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
        public double? Impressions { get; set; }

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
        /// Gets or sets the flight string.
        /// </summary>
        /// <value>The flight string.</value>
        public string FlightString { get; set; }
    }
}
