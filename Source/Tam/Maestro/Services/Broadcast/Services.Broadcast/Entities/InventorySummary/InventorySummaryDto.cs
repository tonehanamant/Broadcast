using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public abstract class InventorySummaryDto
    {
        public int InventorySourceId { get; set; }
        public string InventorySourceName { get; set; }
        public QuarterDetailDto Quarter { get; set; }
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

    public class InventorySummaryApiResponse 
    {
        /// <summary>
        /// Gets or sets the property InventorySourceId
        /// </summary>
        public int InventorySourceId { get; set; }
        /// <summary>
        /// Gets or sets the property InventorySourceName
        /// </summary>
        public string InventorySourceName { get; set; }
        /// <summary>
        /// Gets or sets the class QuarterDetailDto
        /// </summary>
        public QuarterDetailDto Quarter { get; set; }
        /// <summary>
        /// Defines whether rates are available for quarters
        /// </summary>
        public bool HasRatesAvailableForQuarter { get; set; }
        /// <summary>
        /// Defines total market for the inventory
        /// </summary>
        public int TotalMarkets { get; set; }
        /// <summary>
        /// Defines total stations for the inventory
        /// </summary>
        public int TotalStations { get; set; }
        /// <summary>
        /// Defines total house hold impressions for the inventory
        /// </summary>
        public double? HouseholdImpressions { get; set; }
        /// <summary>
        /// Defines the last updated date
        /// </summary>
        public DateTime? LastUpdatedDate { get; set; }
        /// <summary>
        /// Gets or sets the property IsUpdating
        /// </summary>
        public bool IsUpdating { get; set; }
        /// <summary>
        /// Gets or sets the class RatesAvailableFromQuarter
        /// </summary>
        public QuarterDetailDto RatesAvailableFromQuarter { get; set; }
        /// <summary>
        /// Gets or sets the class RatesAvailableToQuarter
        /// </summary>
        public QuarterDetailDto RatesAvailableToQuarter { get; set; }
        /// <summary>
        /// Determines whether inventory gaps are avialable
        /// default set to false
        /// </summary>
        public bool? HasInventoryGaps { get; set; } = false;
        /// <summary>
        /// Gets or sets the class ShareBook
        /// </summary>
        public MediaMonthDto ShareBook { get; set; }
        /// <summary>
        /// Gets or sets the class HutBook
        /// </summary>
        public MediaMonthDto HutBook { get; set; }
        /// <summary>
        /// Defines the list of inventory gaps
        /// </summary>
        public List<InventoryGapDetail> InventoryGaps { get; set; } = new List<InventoryGapDetail>();
        /// <summary>
        /// Defines whether inventory has logo or not
        /// </summary>
        public bool HasLogo { get; set; } = false;
        /// <summary>
        ///  Gets or sets the property HasInventoryForSource
        /// </summary>
        public bool HasInventoryForSource { get; set; }
        /// <summary>
        /// Gets or sets the property TotalPrograms
        /// </summary>
        public int TotalPrograms { get; set; }
        /// <summary>
        /// CPM for the inventory
        /// </summary>
        public decimal? CPM { get; set; }
        /// <summary>
        /// Defines total units for the inventory
        /// </summary>
        public int TotalUnits { get; set; }
        /// <summary>
        /// Defines total daypart code for the inventory
        /// </summary>
        public int TotalDaypartCodes { get; set; }
        /// <summary>
        /// Defines the read only property DaypartCode which will be always SYN
        /// </summary>
        public string DaypartCode { get { return "SYN"; } }
        /// <summary>
        /// Defines the details for the inventory
        /// </summary>
        public List<Detail> Details { get; set; } = new List<Detail>();
        public class Detail
        {
            /// <summary>
            /// Gets or sets the property Daypart
            /// </summary>
            public string Daypart { get; set; }
            /// <summary>
            /// Gets or sets the property TotalMarkets
            /// </summary>
            public int TotalMarkets { get; set; }
            /// <summary>
            /// Gets or sets the property TotalCoverage
            /// </summary>
            public double TotalCoverage { get; set; }
            /// <summary>
            /// Gets or sets the property TotalPrograms
            /// </summary>
            public int TotalPrograms { get; set; }
            /// <summary>
            /// Gets or sets the property HouseholdImpressions
            /// </summary>
            public double? HouseholdImpressions { get; set; }
            /// <summary>
            /// Gets or sets the property MinSpotsPerWeek
            /// </summary>
            public int MinSpotsPerWeek { get; set; }
            /// <summary>
            ///  Gets or sets the property MaxSpotsPerWeek
            /// </summary>
            public int MaxSpotsPerWeek { get; set; }
            /// <summary>
            /// Gets or sets the property TotalUnits
            /// </summary>
            public int TotalUnits { get; set; }
            /// <summary>
            /// Gets or sets the property CPM
            /// </summary>
            public decimal? CPM { get; set; }
        }
    }
}
