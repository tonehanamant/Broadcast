using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Request object for the calculation.
    /// </summary>
    public class PlanCalculateMarketRemovedRequest
    {
        /// <summary>
        /// Gets or sets the before markets.
        /// </summary>
        /// <value>
        /// The before markets.
        /// </value>
        public List<PlanAvailableMarketDto> BeforeMarkets { get; set; } = new List<PlanAvailableMarketDto>();

        /// <summary>
        /// The removed market code.
        /// </summary>
        public short RemovedMarketCode { get; set; }
    }
}