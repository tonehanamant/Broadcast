using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// A class to encapsulate the Plan Delivery Budget properties, including the Posting Type.
    /// </summary>
    public class PlanDeliveryPostingTypeBudget : PlanDeliveryBudget
    {
        /// <summary>
        /// The Posting Type the impressions are provided in.
        /// </summary>
        public PostingTypeEnum PostingType { get; set; }

        /// <summary>
        /// The Standard Daypart Id for the Plan's Primary Daypart
        /// </summary>
        public int StandardDaypartId { get; set; }
    }
}
