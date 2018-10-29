using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class InboundFileSaveRequest
    {
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public int Source { get; set; }
        public List<InboundFileSaveRequestDetail> Details { get; set; } = new List<InboundFileSaveRequestDetail>();
    }

    public class InboundFileSaveRequestDetail
    {
        public string Station { get; set; }
        public DateTime AirTime { get; set; }
        public string ProgramName { get; set; }
        public string Genre { get; set; }
        public int SpotLength { get; set; }
        public string Isci { get; set; }
        public string LeadInProgramName { get; set; }
        public string LeadInGenre { get; set; }
        public string LeadOutProgramName { get; set; }
        public string LeadOutGenre { get; set; }
        public string Market { get; set; }
        public int? EstimateId { get; set; }
        public AffidavitFileSourceEnum InventorySource { get; set; }
        public double? SpotCost { get; set; }
        public string Affiliate { get; set; }
        public List<ScrubbingDemographics> Demographics { get; set; } = new List<ScrubbingDemographics>();
        public DateTime? LeadInEndTime { get; set; }
        public DateTime? LeadOutStartTime { get; set; }
        public string ShowType { get; set; }
        public string LeadInShowType { get; set; }
        public string LeadOutShowType { get; set; }
        public string SuppliedProgramName { get; set; }
    }
}
