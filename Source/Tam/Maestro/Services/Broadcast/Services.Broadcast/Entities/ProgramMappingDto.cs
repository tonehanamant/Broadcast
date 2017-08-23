using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ScheduleProgramMappingDto
    {
        public int ScheduleDetailWeekId { get; set; }
        public string ProgramName { get; set; }
        public string ScheduleDaypart { get; set; }
    }

    public class ProgramMappingDto
    {
        public string BvsProgramName { get; set; }
        public List<ScheduleProgramMappingDto> PrimaryScheduleMatches { get; set; }
        public List<ScheduleProgramMappingDto> FollowingScheduleMatches { get; set; }
    }
}
