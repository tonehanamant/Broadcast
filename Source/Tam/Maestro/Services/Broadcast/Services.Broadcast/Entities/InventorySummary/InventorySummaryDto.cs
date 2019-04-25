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
        public List<InventorySummaryBookDto> InventoryPostingBooks { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public bool IsUpdating { get; set; }
        public QuarterDetailDto RatesAvailableFromQuarter { get; set; }
        public QuarterDetailDto RatesAvailableToQuarter { get; set; }
    }

    public class ProprietaryInventorySummaryDto : InventorySummaryDto
    {
        public int TotalUnits { get; set; }
        public int TotalDaypartCodes { get; set; }
    }

    public class OpenMarketInventorySummaryDto : InventorySummaryDto
    {
        public int TotalPrograms { get; set; }
    }
}
