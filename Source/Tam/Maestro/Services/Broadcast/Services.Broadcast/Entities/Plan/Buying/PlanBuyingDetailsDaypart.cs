using System;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Plan daypart data for a plan buying
    /// </summary>
    public class PlanBuyingDetailsDaypart
    {
        public int DaypartCodeId { get; set; }
        public int DaypartTypeId { get; set; }
        public int StartTimeSeconds { get; set; }
        public int EndTimeSeconds { get; set; }
        public bool IsStartTimeModified { get; set; }
        public bool IsEndTimeModified { get; set; }
        public bool HasRestrictions { get; set; }
        
    }
}