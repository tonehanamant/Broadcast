using Services.Broadcast.Entities.Enums;
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
        public PostingTypeEnum PostingType { get; set; }
    }

    public class PlanPricingBandDetailDto
    {
        public int Id { get; set; }
        public decimal? MinBand { get; set; }
        public decimal? MaxBand { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Budget { get; set; }
        public decimal Cpm { get; set; }
        public double ImpressionsPercentage { get; set; }
        public double AvailableInventoryPercent { get; set; }
    }

    public class PlanPricingBandTotalsDto
    {
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Budget { get; set; }
        public decimal Cpm { get; set; }
    }
}
