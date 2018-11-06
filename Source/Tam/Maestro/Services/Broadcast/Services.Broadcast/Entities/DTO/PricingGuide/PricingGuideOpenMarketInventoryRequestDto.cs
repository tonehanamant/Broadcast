namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class PricingGuideOpenMarketInventoryRequestDto : BasePricingGuideDto
    {
        public int ProposalId { get; set; }
        public bool? HasPricingGuideChanges { get; set; }
    }
}