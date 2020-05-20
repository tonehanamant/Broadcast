namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingBandTotalsDto
    {
        public int TotalSpots { get; set; }
        public double TotalImpressions { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalCpm { get; set; }
    }
}
