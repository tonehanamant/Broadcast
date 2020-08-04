using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingStationResultDto
    {
        public PlanPricingStationResultDto()
        {
            Stations = new List<PlanPricingStationDto>();
            Totals = new PlanPricingStationTotalsDto();
        }

        public int Id { get; set; }
        public int? JobId { get; set; }
        public int? PlanVersionId { get; set; }
        public PlanPricingStationTotalsDto Totals { get; set; }
        public List<PlanPricingStationDto> Stations { get; set; }
    }

    public class PlanPricingStationDto
    {
        public int Id { get; set; }
        public string Station { get; set; }
        public string Market { get; set; }
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public double ImpressionsPercentage { get; set; }
    }

    public class PlanPricingStationTotalsDto
    {
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public int Station { get; set; }
        public double ImpressionsPercentage { get; set; }
    }
}