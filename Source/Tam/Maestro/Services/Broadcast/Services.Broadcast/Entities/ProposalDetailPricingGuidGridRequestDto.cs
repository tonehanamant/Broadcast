namespace Services.Broadcast.Entities
{
    public class ProposalDetailPricingGuidGridRequestDto
    {
        public int ProposalId { get; set; }
        public int ProposalDetailId { get; set; }
        public OpenMarketPricing OpenMarketPricing { get; set; } = new OpenMarketPricing();
    }
}
