using Services.Broadcast.Aggregates;
using Services.Broadcast.ApplicationServices;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public class WeeklyDataDto
    {
        public readonly List<ScheduleAudience> ScheduleAudiences;
        public List<ImpressionAndDeliveryDto> ImpressionsAndDelivey { get; private set; }

        public List<WeeklyImpressionAndDeliveryDto> ReportDataByWeek { get; set; }

        public WeeklyDataDto(List<ScheduleAudience> scheduleAudiences)
        {
            ScheduleAudiences = scheduleAudiences.ToList();

            ImpressionsAndDelivey = new List<ImpressionAndDeliveryDto>();
            ReportDataByWeek = new List<WeeklyImpressionAndDeliveryDto>();
        }
    }
}
