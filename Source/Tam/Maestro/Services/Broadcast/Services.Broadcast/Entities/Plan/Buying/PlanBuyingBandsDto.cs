using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingBandsDto
    {
        public int Id { get; set; }
        public int? JobId { get; set; }
        public int PlanVersionId { get; set; }
        public List<PlanBuyingBandDetailDto> Bands { get; set; } = new List<PlanBuyingBandDetailDto>();
        public PlanBuyingBandTotalsDto Totals { get; set; } = new PlanBuyingBandTotalsDto();
    }
}
