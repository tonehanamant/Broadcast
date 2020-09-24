using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class ProprietaryInventoryData
    {
        public List<ProprietarySummary> ProprietarySummaries { get; set; } = new List<ProprietarySummary>();

        public double TotalImpressions => ProprietarySummaries.Sum(x => x.TotalImpressions);

        public decimal TotalCost => ProprietarySummaries.Sum(x => x.TotalCost);

        public decimal TotalCostWithMargin => ProprietarySummaries.Sum(x => x.TotalCostWithMargin);

        public int TotalSpots => ProprietarySummaries.Sum(x => x.TotalSpots);
    }

    public class ProprietarySummary
    {
        public int ProprietarySummaryId { get; set; }

        public decimal Cpm { get; set; }

        public List<ProprietarySummaryByStation> ProprietarySummaryByStations { get; set; } = new List<ProprietarySummaryByStation>();

        public double TotalImpressions => ProprietarySummaryByStations.Sum(x => x.TotalImpressions);

        public decimal TotalCost => ProprietarySummaryByStations.Sum(x => x.TotalCost);

        public decimal TotalCostWithMargin => ProprietarySummaryByStations.Sum(x => x.TotalCostWithMargin);

        public int TotalSpots => ProprietarySummaryByStations.Sum(x => x.TotalSpots);

        public string ProgramName { get; set; }

        public string Genre { get; set; }
    }

    public class ProprietarySummaryByStation
    {
        public int StationId { get; set; }

        public short MarketCode { get; set; }

        public int SpotsPerWeek { get; set; }

        public decimal CostPerWeek { get; set; }

        public int TotalSpots { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalCostWithMargin { get; set; }

        public List<ProprietarySummaryByAudience> ProprietarySummaryByAudiences { get; set; } = new List<ProprietarySummaryByAudience>();

        public double TotalImpressions => ProprietarySummaryByAudiences.Sum(x => x.TotalImpressions);
    }

    public class ProprietarySummaryByAudience
    {
        public int AudienceId { get; set; }

        public double ImpressionsPerWeek { get; set; }

        public double TotalImpressions { get; set; }
    }
}
