using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingBandDto
    {
        public int Id { get; set; }
        public int? JobId { get; set; }
        public int PlanVersionId { get; set; }
        public List<PlanPricingBandDetailDto> Bands { get; set; } = new List<PlanPricingBandDetailDto>();
        public PlanPricingBandTotalsDto Totals { get; set; } = new PlanPricingBandTotalsDto();
    }
}
