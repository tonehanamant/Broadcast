using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingBand
    {
        public int Id { get; set; }
        public int? JobId { get; set; }
        public int PlanVersionId { get; set; }
        public List<PlanPricingBandDetail> Bands { get; set; } = new List<PlanPricingBandDetail>();
        public PlanPricingBandTotals Totals { get; set; } = new PlanPricingBandTotals();
        public PostingTypeEnum PostingType { get; internal set; }
    }

    public class PlanPricingBandDetail
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
        public bool IsProprietary { get; set; }
    }

    public class PlanPricingBandTotals
    {
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Budget { get; set; }
        public decimal Cpm { get; set; }
    }
}
