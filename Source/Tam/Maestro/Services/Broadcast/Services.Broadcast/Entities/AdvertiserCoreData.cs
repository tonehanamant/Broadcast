using Services.Broadcast.Aggregates;
using System;
using System.Collections.Generic;
using EntityFrameworkMapping.Broadcast;

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
        public schedule_details ScheduleDetail { get; set; }
        public List<bvs_file_details> BvsDetails { get; set; }
        //public List<AudienceImpressionsAndDelivery> AudienceImpressions { get; set; }
       
    }
}
