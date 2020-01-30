using System;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Daypart data for a plan buying
    /// </summary>
    public class PlanBuyingDaypart
    {
        public int DaypartId { get; set; }
        public double? GoalPercent { get; set; }
        public double? SharePercent { get; set; }
        public decimal? Budget { get; set; }
        public double? CPM { get; set; }
        public decimal? Impressions { get; set; }
    }
}