using Services.Broadcast.Aggregates;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class AdvertiserCoreData
    {
        public int Rank { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string Affiliate { get; set; }
        public string ProgramName { get; set; }
        public string GroupedByName { get; set; }
        public int SpotLength { get; set; }
        public int ScheduleDetailId { get; set; }
        public List<AudienceImpressionsAndDelivery> AudienceImpressions { get; set; }
        public int MediaWeekId { get; set; }
        public DateTime DateAired { get; set; }
        public int TimeAired  { get; set; }
        public int ScheduleDetailWeekId { get; set; }
        public bool IsBvsDetail { get; set; }

        public AdvertiserCoreData()
        {
            AudienceImpressions = new List<AudienceImpressionsAndDelivery>();
        }
    }
}
