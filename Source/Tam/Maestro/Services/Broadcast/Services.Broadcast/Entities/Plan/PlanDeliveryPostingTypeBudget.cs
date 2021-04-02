using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// A class to encapsulate the Plan Delivery Budget properties, including the Posting Type.
    /// </summary>
    public class PlanDeliveryPostingTypeBudget
    {
        /// <summary>
        /// The Posting Type the impressions are provided in.
        /// </summary>
        public PostingTypeEnum PostingType { get; set; } = PostingTypeEnum.NSI;

        /// <summary>
        /// The Standard Daypart Id for the Plan's Primary Daypart
        /// </summary>
        public int StandardDaypartId { get; set; }

        public decimal? Budget { get; set; }
        public double? DeliveryImpressions { get; set; }
        public double? DeliveryRatingPoints { get; set; }
        public decimal? CPM { get; set; }
        public decimal? CPP { get; set; }
        public double? Universe { get; set; }
        public int AudienceId { get; set; }
    }
}
