using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Request object for the calculation.
    /// </summary>
    public class PlanCalculateMarketWeightChangeRequest
    {
        /// <summary>
        /// Gets or sets the available markets.
        /// </summary>
        /// <value>
        /// The available markets.
        /// </value>
        public List<PlanAvailableMarketDto> AvailableMarkets { get; set; } = new List<PlanAvailableMarketDto>();

        /// <summary>
        /// Gets or sets the modified market code.
        /// </summary>
        /// <value>
        /// The modified market code.
        /// </value>
        public short ModifiedMarketCode { get; set; }

        /// <summary>
        /// Gets or sets the user entered value.
        /// </summary>
        /// <value>
        /// The user entered value.
        /// </value>
        public double? UserEnteredValue { get; set; }
    }
}