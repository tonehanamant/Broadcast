using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public abstract class InventorySummaryDto
    {
        public int InventorySourceId { get; set; }
        public string InventorySourceName { get; set; }
        public QuarterDetailDto Quarter { get; internal set; }
        public bool HasRatesAvailableForQuarter { get; set; }
        public int TotalMarkets { get; set; }
        public int TotalStations { get; set; }        
        public double? HouseholdImpressions { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public bool IsUpdating { get; set; }
        public QuarterDetailDto RatesAvailableFromQuarter { get; set; }
        public QuarterDetailDto RatesAvailableToQuarter { get; set; }
        public bool? HasInventoryGaps { get; set; } = false;
        public MediaMonthDto ShareBook { get; set; }
        public MediaMonthDto HutBook { get; set; }
        public List<InventoryGapDetail> InventoryGaps { get; set; } = new List<InventoryGapDetail>();
        public bool HasLogo { get; set; } = false;
        public bool HasInventoryForSource { get; set; }
    }

    public class BarterInventorySummaryDto : InventorySummaryDto
    {
        public int TotalUnits { get; set; }
        public int TotalDaypartCodes { get; set; }
        public List<Detail> Details { get; set; } = new List<Detail>();

        public class Detail
        {
            public string Daypart { get; set; }
            public int TotalMarkets { get; set; }
            public double TotalCoverage { get; set; }
            public double? HouseholdImpressions { get; set; }
            public int TotalUnits { get; set; }
            public decimal? CPM { get; set; }
        }
    }

    public class OpenMarketInventorySummaryDto : InventorySummaryDto
    {
        public int TotalPrograms { get; set; }
        public object Details { get; set; }
    }

    public class ProprietaryOAndOInventorySummaryDto : InventorySummaryDto
    {
        public int TotalPrograms { get; set; }
        public int TotalDaypartCodes { get; set; }
        public List<Detail> Details { get; set; } = new List<Detail>();

        public class Detail
        {
            public string Daypart { get; set; }
            public int TotalMarkets { get; set; }
            public double TotalCoverage { get; set; }
            public int TotalPrograms { get; set; }
            public double? HouseholdImpressions { get; set; }
            public int MinSpotsPerWeek { get; set; }
            public int MaxSpotsPerWeek { get; set; }
            public decimal? CPM { get; set; }
        }
    }

    public class SyndicationInventorySummaryDto : InventorySummaryDto
    {
        public int TotalPrograms { get; set; }
        public string DaypartCode { get { return "SYN"; } }
        public object Details { get; set; }
    }

    public class DiginetInventorySummaryDto : InventorySummaryDto
    {
        public decimal? CPM { get; set; }
        public int TotalDaypartCodes { get; set; }
        public List<Detail> Details { get; set; } = new List<Detail>();
        public class Detail
        {
            public string Daypart { get; set; }
            public double? HouseholdImpressions { get; set; }
            public decimal? CPM { get; set; }
        }
    }

    public class InventorySummaryApiResponse : InventorySummaryDto
    {
       
    }
}
