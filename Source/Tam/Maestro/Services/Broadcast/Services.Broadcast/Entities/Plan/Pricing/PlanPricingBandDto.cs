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
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; } = SpotAllocationModelMode.Quality;
    }

    public class PlanPricingBandDto_v2
    {
        public int Id { get; set; }
        public int? JobId { get; set; }
        public int PlanVersionId { get; set; }
        public PostingTypePlanPricingResultBands NsiResults { get; set; }
        public PostingTypePlanPricingResultBands NtiResults { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; } = SpotAllocationModelMode.Quality;
    }

    public class PostingTypePlanPricingResultBands
    {
        public PlanPricingBandTotalsDto Totals { get; set; }
        public List<PlanPricingBandDetailDto> BandsDetails { get; set; } = new List<PlanPricingBandDetailDto>();
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
