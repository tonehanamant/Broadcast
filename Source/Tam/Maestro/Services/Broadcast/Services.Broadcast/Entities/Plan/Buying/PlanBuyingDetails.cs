using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Plan data for a plan buying
    /// </summary>
    public class PlanBuyingDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public int FlightActiveDays { get; set; }
        public int FlightHiatusDays { get; set; }
        public decimal? Impressions { get; set; }
        public decimal? Budget { get; set; }
        public double? CPM { get; set; }
        public List<PlanBuyingDetailsDaypart> Dayparts { get; set; }
        public double? CoverageGoalPercent { get; set; }
        public int AvailableMarketsCount { get; set; }
        public int BlackoutMarketsCount { get; set; }
        public int AudienceId { get; set; }
        public int PostingType { get; set; }
        public int SpotLengthId { get; set; }
    }
}