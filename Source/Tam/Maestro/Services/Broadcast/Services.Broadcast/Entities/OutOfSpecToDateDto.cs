using Services.Broadcast.Aggregates;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public class OutOfSpecToDateDto : DetectionReportDataContainer
    {
        public readonly List<ScheduleAudience> ScheduleAudiences;

        public OutOfSpecToDateDto(IEnumerable<ScheduleAudience> scheduleAudiences)
        {
            ScheduleAudiences = scheduleAudiences.ToList();
            ReportData = new List<DetectionReportData>();
        }
    }
}
