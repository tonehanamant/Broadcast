namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public abstract class BasePricingGuideDto
    {
        public int ProposalDetailId { get; set; }

        public decimal? BudgetGoal { get; set; }

        public double? ImpressionGoal { get; set; }

        public OpenMarketPricingGuideDto OpenMarketPricing { get; set; } = new OpenMarketPricingGuideDto();

        public decimal OpenMarketShare { get; set; }
    }
}
