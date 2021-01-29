using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingBandsDto
    {
        public int Id { get; set; }
        public int? BuyingJobId { get; set; }
        public int PlanVersionId { get; set; }
        public List<PlanBuyingBandDetailDto> Details { get; set; } = new List<PlanBuyingBandDetailDto>();
        public PlanBuyingProgramTotalsDto Totals { get; set; } = new PlanBuyingProgramTotalsDto();
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
    }

    public class PlanBuyingBandDetailDto
    {
        public decimal? MinBand { get; set; }
        public decimal? MaxBand { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Budget { get; set; }
        public decimal Cpm { get; set; }
        public double ImpressionsPercentage { get; set; }
        public double AvailableInventoryPercent { get; set; }
    }
}
