using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Request object for the calculation.
    /// </summary>
    public class PlanCalculateMarketAddedRequest
    {
        /// <summary>
        /// The list of markets before the change is made.
        /// </summary>
        public List<PlanAvailableMarketDto> BeforeMarkets { get; set; } = new List<PlanAvailableMarketDto>();

        /// <summary>
        /// The market being added.
        /// </summary>
        public PlanAvailableMarketDto AddedMarket { get; set; }
    }
}