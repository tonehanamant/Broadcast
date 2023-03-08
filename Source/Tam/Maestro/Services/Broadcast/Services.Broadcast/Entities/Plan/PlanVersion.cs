using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanVersion
    {        
        public int VersionId { get; set; }
        public int? VersionNumber { get; set; }
        public bool? IsDraft { get; set; }
        public bool IsAduPlan { get; set; }
        public int? Status { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<DateTime> HiatusDays { get; set; }
        public int? TargetAudienceId { get; set; }
        public List<PlanDaypartDto> Dayparts { get; set; }
        public decimal? Budget { get; set; }
        public double? TargetImpressions { get; set; }
        public decimal? TargetCPM { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
