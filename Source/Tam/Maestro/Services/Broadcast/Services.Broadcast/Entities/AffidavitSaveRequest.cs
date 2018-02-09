using System;
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
        public string LeadInTitle { get; set; }
        public string LeadInGenre { get; set; }
        public string LeadOutTitle { get; set; }
        public string LeadOutGenre { get; set; }
    }
}
