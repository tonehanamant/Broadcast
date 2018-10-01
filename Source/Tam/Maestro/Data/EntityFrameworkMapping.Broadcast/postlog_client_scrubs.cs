//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntityFrameworkMapping.Broadcast
{
    using System;
    using System.Collections.Generic;
    
    public partial class postlog_client_scrubs
    {
        public postlog_client_scrubs()
        {
            this.postlog_client_scrub_audiences = new HashSet<postlog_client_scrub_audiences>();
        }
    
        public int id { get; set; }
        public long postlog_file_detail_id { get; set; }
        public int proposal_version_detail_quarter_week_id { get; set; }
        public bool lead_in { get; set; }
        public string effective_program_name { get; set; }
        public string effective_genre { get; set; }
        public string effective_show_type { get; set; }
        public string effective_isci { get; set; }
        public string effective_client_isci { get; set; }
        public bool match_program { get; set; }
        public bool match_genre { get; set; }
        public bool match_market { get; set; }
        public bool match_time { get; set; }
        public bool match_station { get; set; }
        public bool match_isci_days { get; set; }
        public Nullable<bool> match_date { get; set; }
        public bool match_show_type { get; set; }
        public bool match_isci { get; set; }
        public string comment { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public int status { get; set; }
        public bool status_override { get; set; }
    
        public virtual ICollection<postlog_client_scrub_audiences> postlog_client_scrub_audiences { get; set; }
        public virtual postlog_file_details postlog_file_details { get; set; }
        public virtual proposal_version_detail_quarter_weeks proposal_version_detail_quarter_weeks { get; set; }
    }
}
