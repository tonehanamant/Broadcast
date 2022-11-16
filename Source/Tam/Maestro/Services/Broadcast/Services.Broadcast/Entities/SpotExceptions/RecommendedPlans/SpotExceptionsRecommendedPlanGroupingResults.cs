using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    /// <summary></summary>
    public class SpotExceptionsRecommendedPlanGroupingResults
    {
        /// <summary>Initializes a new instance of the 
        /// <see cref="SpotExceptionsRecommendedPlanGroupingResults" /> class.</summary>
        public SpotExceptionsRecommendedPlanGroupingResults()
        {
            Active = new List<SpotExceptionsRecommendedPlanGroupingToDoResults>();
            Completed = new List<SpotExceptionsRecommendedPlanGroupingDoneResults>();
        }

        /// <summary>
        /// Gets or sets the active.
        /// </summary>
        /// <value>The active.</value>
        public List<SpotExceptionsRecommendedPlanGroupingToDoResults> Active { get; set; }

        /// <summary>
        /// Gets or sets the completed.
        /// </summary>
        /// <value>The completed.</value>
        public List<SpotExceptionsRecommendedPlanGroupingDoneResults> Completed { get; set; }
    }

    /// <summary></summary>
    public class SpotExceptionsRecommendedPlanGroupingToDoResults
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
        /// Gets or sets the synced timestamp.
        /// </summary>
        /// <value>The synced timestamp.</value>
        public string SyncedTimestamp { get; set; }

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
    public class SpotExceptionsRecommendedPlanGroupingDoneResults
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
        /// Gets or sets the synced timestamp.
        /// </summary>
        /// <value>The synced timestamp.</value>
        public string SyncedTimestamp { get; set; }

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
