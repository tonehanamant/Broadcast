using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// The calculation result.
    /// </summary>
    public class PlanAvailableMarketCalculationResult
    {
        /// <summary>
        /// Gets or sets the available markets.
        /// </summary>
        /// <value>
        /// The available markets.
        /// </value>
        public List<PlanAvailableMarketDto> AvailableMarkets { get; set; } = new List<PlanAvailableMarketDto>();

        /// <summary>
        /// Gets or sets the total weight.
        /// </summary>
        /// <value>
        /// The total weight.
        /// </value>
        public double TotalWeight { get; set; }
    }
}