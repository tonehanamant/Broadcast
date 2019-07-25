using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryAggregation
    {
        public int InventorySourceId { get; set; }
        public string InventorySourceName { get; set; }
        public InventorySummaryQuarter Quarter { get; internal set; }
        public bool HasRatesAvailableForQuarter { get; set; }
        public int TotalMarkets { get; set; }
        public int TotalStations { get; set; }
        public double? TotalProjectedHouseholdImpressions { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public bool IsUpdating { get; set; }
        public InventorySummaryQuarter RatesAvailableFromQuarter { get; set; }
        public InventorySummaryQuarter RatesAvailableToQuarter { get; set; }
        public bool? HasInventoryGaps { get; set; }
        public int? ShareBookId { get; set; }
        public int? HutBookId { get; set; }
        public List<InventoryGapDetail> InventoryGaps { get; set; }
        public int? TotalUnits { get; set; }
        public int? TotalDaypartCodes { get; set; }
        public int? TotalPrograms { get; set; }
        public decimal? CPM { get; set; }
        public List<Detail> Details { get; set; }

        public class Detail
        {
            public string Daypart { get; set; }
            public int TotalMarkets { get; set; }
            public double TotalCoverage { get; set; }
            public double? TotalProjectedHouseholdImpressions { get; set; }
            public int? TotalUnits { get; set; }
            public decimal? CPM { get; set; }
            public int? MinSpotsPerWeek { get; set; }
            public int? MaxSpotsPerWeek { get; set; }
            public int? TotalPrograms { get; set; }
        }
    }
}
