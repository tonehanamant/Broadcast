using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanDeliveryBudget
    {
        public decimal? Budget { get; set; }
        public double? Impressions { get; set; }
        public double? RatingPoints { get; set; }
        public decimal? CPM { get; set; }
        public decimal? CPP { get; set; }
        public double? Universe { get; set; }
        public int? AudienceId { get; set; }
    }
}
