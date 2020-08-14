using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingStationResultDto
    {
        public int Id { get; set; }
        public int? JobId { get; set; }
        public int? PlanVersionId { get; set; }
        public PlanBuyingStationTotalsDto Totals { get; set; } = new PlanBuyingStationTotalsDto();
        public List<PlanBuyingStationDto> Stations { get; set; } = new List<PlanBuyingStationDto>();
    }

    public class PlanBuyingStationDto
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

    public class PlanBuyingStationTotalsDto
    {
        public int Spots { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Budget { get; set; }
        public int Station { get; set; }
        public double ImpressionsPercentage { get; set; }
    }
}