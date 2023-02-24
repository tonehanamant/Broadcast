using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    /// <summary></summary>
    public class SpotExceptionsRecommendedPlanSpotsResultDto
    {
        /// <summary>Initializes a new instance of the <see cref="SpotExceptionsRecommendedPlanSpotsResultDto" /> class.</summary>
        public SpotExceptionsRecommendedPlanSpotsResultDto()
        {
            Active = new List<SpotExceptionsRecommendedToDoPlanSpotsDto>();
            Queued = new List<SpotExceptionsRecommendedDonePlanSpotsDto>();
            Synced = new List<SpotExceptionsRecommendedDonePlanSpotsDto>();
        }

        /// <summary>
        /// Gets or sets the active.
        /// </summary>
        /// <value>The active.</value>
        public List<SpotExceptionsRecommendedToDoPlanSpotsDto> Active { get; set; }

        /// <summary>
        /// Gets or sets the queued.
        /// </summary>
        /// <value>The queued.</value>
        public List<SpotExceptionsRecommendedDonePlanSpotsDto> Queued { get; set; }

        /// <summary>
        /// Gets or sets the synced.
        /// </summary>
        /// <value>The synced.</value>
        public List<SpotExceptionsRecommendedDonePlanSpotsDto> Synced { get; set; }
    }

    /// <summary></summary>
    public class SpotExceptionsRecommendedToDoPlanSpotsDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the estimate identifier.
        /// </summary>
        /// <value>The estimate identifier.</value>
        public int EstimateId { get; set; }

        /// <summary>
        /// Gets or sets the name of the isci.
        /// </summary>
        /// <value>The name of the isci.</value>
        public string IsciName { get; set; }

        /// <summary>
        /// Gets or sets the program air date.
        /// </summary>
        /// <value>The program air date.</value>
        public string ProgramAirDate { get; set; }

        /// <summary>
        /// Gets or sets the program air time.
        /// </summary>
        /// <value>The program air time.</value>
        public string ProgramAirTime { get; set; }

        /// <summary>
        /// Gets or sets the recommended plan.
        /// </summary>
        /// <value>The recommended plan.</value>
        public string RecommendedPlan { get; set; }

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
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the affiliate.
        /// </summary>
        /// <value>The affiliate.</value>
        public string Affiliate { get; set; }

        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>The plan identifier.</value>
        public int? PlanId { get; set; }

        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>The market.</value>
        public string Market { get; set; }

        /// <summary>
        /// Gets or sets the station.
        /// </summary>
        /// <value>The station.</value>
        public string Station { get; set; }

        /// <summary>
        /// Gets or sets the inventory source.
        /// </summary>
        /// <value>The inventory source.</value>
        public string InventorySource { get; set; }
    }

    /// <summary></summary>
    public class SpotExceptionsRecommendedDonePlanSpotsDto
    {

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the estimate identifier.
        /// </summary>
        /// <value>The estimate identifier.</value>
        public int? EstimateId { get; set; }

        /// <summary>
        /// Gets or sets the name of the isci.
        /// </summary>
        /// <value>The name of the isci.</value>
        public string IsciName { get; set; }

        /// <summary>
        /// Gets or sets the program air date.
        /// </summary>
        /// <value>The program air date.</value>
        public string ProgramAirDate { get; set; }

        /// <summary>
        /// Gets or sets the program air time.
        /// </summary>
        /// <value>The program air time.</value>
        public string ProgramAirTime { get; set; }

        /// <summary>
        /// Gets or sets the recommended plan.
        /// </summary>
        /// <value>The recommended plan.</value>
        public string RecommendedPlan { get; set; }

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
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the affiliate.
        /// </summary>
        /// <value>The affiliate.</value>
        public string Affiliate { get; set; }

        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>The plan identifier.</value>
        public int? PlanId { get; set; }

        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>The market.</value>
        public string Market { get; set; }

        /// <summary>
        /// Gets or sets the station.
        /// </summary>
        /// <value>The station.</value>
        public string Station { get; set; }

        /// <summary>
        /// Gets or sets the inventory source.
        /// </summary>
        /// <value>The inventory source.</value>
        public string InventorySource { get; set; }
    }
}
