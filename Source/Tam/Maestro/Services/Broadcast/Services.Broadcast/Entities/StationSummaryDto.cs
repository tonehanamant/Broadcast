using Services.Broadcast.Aggregates;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public class StationSummaryDto : BvsReportDataContainer
    {
        public StationSummaryDto() { }

        public readonly List<ScheduleAudience> ScheduleAudiences;

        public StationSummaryDto(IEnumerable<ScheduleAudience> scheduleAudiences)
        {
            ScheduleAudiences = scheduleAudiences.ToList();

            ReportData = new List<BvsReportData>();
        }
    }
}
