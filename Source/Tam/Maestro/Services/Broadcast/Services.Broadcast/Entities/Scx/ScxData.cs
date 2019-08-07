using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities.Scx
{
    public class ScxData
    {
        public string UnitName { get; set; }
        public string DaypartCode { get; set; }
        public int DaypartCodeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public InventorySource InventorySource { get; set; }
        public List<OrderData> Orders { get; set; }
        public List<DemoData> Demos { get; set; }
        public IOrderedEnumerable<MediaWeek> AllSortedMediaWeeks { get; set; }
    }

    public class OrderData
    {
        public string SurveyString { get; set; }
        public int TotalSpots { get; set; }
        public decimal TotalCost { get; set; }
        public List<ScxMarketDto> InventoryMarkets { get; set; }
    }

    public class DemoData
    {
        public int DemoRank { get; set; }
        public BroadcastAudience Demo { get; set; }
    }
}
