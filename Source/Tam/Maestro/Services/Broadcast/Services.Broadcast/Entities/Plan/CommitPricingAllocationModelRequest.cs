using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// A request object.
    /// </summary>
    public class CommitPricingAllocationModelRequest
    {
        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>
        /// The plan identifier.
        /// </value>
        public int PlanId { get; set; }

        /// <summary>
        /// Gets or sets the type of the posting.
        /// </summary>
        /// <value>
        /// The type of the posting.
        /// </value>
        public PostingTypeEnum PostingType { get; set; } = PostingTypeEnum.NSI;

        /// <summary>
        /// Gets or sets the spot allocation model mode.
        /// </summary>
        /// <value>
        /// The spot allocation model mode.
        /// </value>
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; } = SpotAllocationModelMode.Quality;
    }
}