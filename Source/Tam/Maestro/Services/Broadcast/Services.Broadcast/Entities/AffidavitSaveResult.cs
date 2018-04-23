﻿using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class AffidavitSaveRequest
    {
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public int Source { get; set; }
        public List<AffidavitSaveRequestDetail> Details { get; set; }

        public AffidavitSaveRequest()
        {
            Details = new List<AffidavitSaveRequestDetail>();
        }
    }

    public class AffidavitSaveRequestDetail
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
        public int InventorySource { get; set; }
        public double? SpotCost { get; set; }
        public string Affiliate { get; set; }
        public List<Demographics> Demographics { get; set; } = new List<Demographics>();
        public DateTime LeadInEndTime { get; set; }
        public DateTime LeadOutStartTime { get; set; }
        public string ProgramShowType { get; set; }
        public string LeadInShowType { get; set; }
        public string LeadOutShowType { get; set; }
    }
}
