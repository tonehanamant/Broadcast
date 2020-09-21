﻿namespace Services.Broadcast.Entities.Plan.Pricing
{
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
        public bool IsProprietary { get; set; }
    }
}
