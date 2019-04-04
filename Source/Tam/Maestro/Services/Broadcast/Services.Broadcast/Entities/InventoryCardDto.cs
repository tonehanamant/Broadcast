using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class InventoryCardDto
    {
        public int InventorySourceId { get; set; }
        public string InventorySourceName { get; set; }
        public InventoryCardQuarter Quarter { get; internal set; }
        public bool HasRatesAvailableForQuarter { get; set; }
        public int TotalMarkets { get; set; }
        public int TotalStations { get; set; }
        public int TotalUnits { get; set; }
        public int TotalDaypartCodes { get; set; }
        public double? HouseholdImpressions { get; set; }
        public List<InventoryCardBooks> InventoryPostingBooks { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public bool IsUpdating { get; set; }
        public InventoryCardQuarter RatesAvailableFromQuarter { get; set; }
        public InventoryCardQuarter RatesAvailableToQuarter { get; set; }
    }
}
