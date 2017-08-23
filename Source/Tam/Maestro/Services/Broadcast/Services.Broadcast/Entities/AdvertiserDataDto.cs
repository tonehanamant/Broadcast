using Services.Broadcast.Aggregates;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public class AdvertiserDataDto
    {
        public List<ScheduleAudience> ScheduleAudiences { get; set; }
        public List<BvsReportData> ReportData { get; set; }

        public int? OrderedSpots
        {
            get { return ReportData.Sum(x => x.OrderedSpots); }
        }
        public int? DeliveredSpots
        {
            get { return ReportData.Sum(x => x.DeliveredSpots); }
        }
        public List<ImpressionAndDeliveryDto> ImpressionsAndDelivey { get; private set; }

        public AdvertiserDataDto(List<ScheduleAudience> scheduleAudiences)
        {
            ImpressionsAndDelivey = new List<ImpressionAndDeliveryDto>();
            ScheduleAudiences = scheduleAudiences.ToList();
            ReportData = new List<BvsReportData>();
        }
    }
}
