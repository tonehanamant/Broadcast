using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities.Scx
{
    public class OpenMarketScxData
    {
        public string Affiliate { get; set; }
        public int MarketCode { get; set; }
        public string DaypartCode { get; set; }
        public int DaypartCodeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public InventorySource InventorySource { get; set; }
        public List<OrderData> Orders { get; set; }
        public List<DemoData> Demos { get; set; }
        public IOrderedEnumerable<MediaWeek> AllSortedMediaWeeks { get; set; }
    }
}
