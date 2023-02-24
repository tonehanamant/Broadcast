using System;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    /// <summary></summary>
    public class SpotExceptionsRecommendedPlanSpotsDto
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
        public DateTime ProgramAirTime { get; set; }

        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>The plan identifier.</value>
        public int? PlanId { get; set; }

        /// <summary>
        /// Gets or sets the recommended plan.
        /// </summary>
        /// <value>The recommended plan.</value>
        public string RecommendedPlanName { get; set; }

        /// <summary>
        /// Gets or sets the impressions.
        /// </summary>
        /// <value>The impressions.</value>
        public double? Impressions { get; set; }

        /// <summary>
        /// Gets or sets the synced timestamp.
        /// </summary>
        /// <value>The synced timestamp.</value>
        public DateTime? SyncedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the spot lengths.
        /// </summary>
        /// <value>The spot lengths.</value>
        public int SpotLength { get; set; }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the inventory source.
        /// </summary>
        /// <value>The inventory source.</value>
        public string InventorySource { get; set; }

        /// <summary>
        /// Gets or sets the affiliate.
        /// </summary>
        /// <value>The affiliate.</value>
        public string Affiliate { get; set; }

        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>The market.</value>
        public string MarketName { get; set; }

        /// <summary>
        /// Gets or sets the station.
        /// </summary>
        /// <value>The station.</value>
        public string Station { get; set; }
    }
}
