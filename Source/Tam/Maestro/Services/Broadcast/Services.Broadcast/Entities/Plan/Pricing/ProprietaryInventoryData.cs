using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class ProprietaryInventoryData
    {
        public double TotalImpressions { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalCostWithMargin { get; set; }

        public List<ProprietaryDataByStationByAudience> InventoryProprietarySummaryByStationByAudiences { get; set; } = new List<ProprietaryDataByStationByAudience>();
    }

    public class ProprietaryDataByStationByAudience
    {
        public int ProprietarySummaryId { get; set; }

        public int AudienceId { get; set; }

        public short MarketCode { get; set; }

        public int StationId { get; set; }

        public double ImpressionsPerWeek { get; set; }

        public int SpotsPerWeek { get; set; }

        public decimal CostPerWeek { get; set; }

        public double TotalImpressions { get; set; }

        public int TotalSpots { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalCostWithMargin { get; set; }
    }
}
