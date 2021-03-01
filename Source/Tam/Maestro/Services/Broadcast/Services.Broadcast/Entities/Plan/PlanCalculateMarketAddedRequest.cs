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
        /// The markets being added.
        /// </summary>
        public List<PlanAvailableMarketDto> AddedMarkets { get; set; }
    }
}