using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Services.Broadcast.Entities
{
    public class WhosWatchingTVPostProcessingFile
    {
        public List<WhosWatchingTVDetail> Details { get; set; } = new List<WhosWatchingTVDetail>();
    }

    public class WhosWatchingTVDetail
    {
        public int? EstimateId { get; set; }
        public string Market { get; set; }
        public DateTime Date { get; set; }
        public string InventorySource { get; set; }
        public string Station { get; set; }
        public int SpotLength { get; set; }
        public string Time { get; set; }
        public double? SpotCost { get; set; }
        public string ISCI { get; set; }
        public string Affiliate { get; set; }
        public string Program { get; set; }
        public string Genre { get; set; }
        public string LeadInProgram { get; set; }
        public string LeadInGenre { get; set; }
        public string LeadOutProgram { get; set; }
        public string LeadOutGenre { get; set; }
        public List<Demographics> Demographics { get; set; } = new List<Demographics>();
        public string LeadInEndTime { get; set; }
        public string LeadOutStartTime { get; set; }
        public string ProgramShowType { get; set; }
        public string LeadInShowType { get; set; }
        public string LeadOutShowType { get; set; }
    }

    public class Demographics
    {
        public int AudienceId { get; set; }
        public string Demographic { get; set; }
        public double OvernightRating { get; set; }
        public int OvernightImpressions { get; set; }
    }
}
