using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanDeliveryBudget
    {
        public decimal? Budget { get; set; }
        public double? DeliveryImpressions { get; set; }
        public double? DeliveryRatingPoints { get; set; }
        public decimal? CPM { get; set; }
        public decimal? CPP { get; set; }
        public PlanCurrenciesEnum Currency { get; set; }
        public int AudienceId { get; set; }
        public int MediaMonthId { get; set; }
    }
}
