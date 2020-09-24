using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingStationResult
    {
        public PlanPricingStationResult()
        {
            Stations = new List<PlanPricingStation>();
            Totals = new PlanPricingStationTotals();
        }

        public int Id { get; set; }
        public int? JobId { get; set; }
        public int? PlanVersionId { get; set; }
        public PlanPricingStationTotals Totals { get; set; }
        public List<PlanPricingStation> Stations { get; set; }
    }

    public class PlanPricingStation
    {
        public int Id { get; set; }
        public string Station { get; set; }
        public string Market { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public double ImpressionsPercentage { get; set; }
        public bool IsProprietary { get; set; }
    }

    public class PlanPricingStationTotals
    {
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public int Station { get; set; }
        public double ImpressionsPercentage { get; set; }
    }
}
