using System.Collections.Generic;

/// <summary></summary>
namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    /// <summary></summary>
    public class RecommendedPlanFiltersResultDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendedPlanFiltersResultDto" /> class.
        /// </summary>
        public RecommendedPlanFiltersResultDto()
        {
            Markets = new List<string>();
            Stations = new List<string>();
            InventorySources = new List<string>();
        }

        /// <summary>
        /// Gets or sets the markets.
        /// </summary>
        /// <value>The markets.</value>
        public List<string> Markets { get; set; }

        /// <summary>
        /// Gets or sets the stations.
        /// </summary>
        /// <value>The stations.</value>
        public List<string> Stations { get; set; }

        /// <summary>
        /// Gets or sets the inventory sources.
        /// </summary>
        /// <value>The inventory sources.</value>
        public List<string> InventorySources { get; set; }
    }
}
