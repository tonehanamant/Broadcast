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
    
    public partial class affidavit_client_scrubs
    {
        public int id { get; set; }
        public long affidavit_file_detail_id { get; set; }
        public int proposal_version_detail_quarter_week_id { get; set; }
        public bool match_program { get; set; }
        public bool match_genre { get; set; }
        public bool match_market { get; set; }
        public bool match_time { get; set; }
        public int status { get; set; }
        public string comment { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public bool lead_in { get; set; }
    
        public virtual affidavit_file_details affidavit_file_details { get; set; }
        public virtual proposal_version_detail_quarter_weeks proposal_version_detail_quarter_weeks { get; set; }
    }
}
