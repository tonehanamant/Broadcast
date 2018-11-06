namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class BasePricingGuideDto
    {
        public int ProposalDetailId { get; set; }

        public decimal? BudgetGoal { get; set; }

        public double? ImpressionGoal { get; set; }

        public OpenMarketPricingGuideDto OpenMarketPricing { get; set; } = new OpenMarketPricingGuideDto();
    }
}
