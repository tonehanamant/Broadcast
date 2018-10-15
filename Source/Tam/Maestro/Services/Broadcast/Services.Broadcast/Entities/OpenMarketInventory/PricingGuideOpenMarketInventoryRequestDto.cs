namespace Services.Broadcast.Entities
{
    public class PricingGuideOpenMarketInventoryRequestDto
    {
        public int ProposalId { get; set; }
        public int ProposalDetailId { get; set; }
        public OpenMarketPricing OpenMarketPricing { get; set; } = new OpenMarketPricing();
        public bool? HasPricingGuideChanges { get; set; }
        public decimal? BudgetGoal { get; internal set; }
        public double? ImpressionsGoal { get; internal set; }
    }
}