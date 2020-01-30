using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PlanBuyingRequestDetails
    {
        public int InventorySourceId { get; set; }
        public decimal? Budget { get; set; }
        public double? CPM { get; set; }
        public decimal? Impressions { get; set; }
        public double? SharePercent { get; set; }
        public string EstimateId { get; set; }
        public string Notes { get; set; }
    }
}
