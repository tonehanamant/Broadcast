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
    
    public partial class scx_generation_job_files
    {
        public int id { get; set; }
        public int scx_generation_job_id { get; set; }
        public string file_name { get; set; }
        public int inventory_source_id { get; set; }
        public int daypart_code_id { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public string unit_name { get; set; }
    
        public virtual inventory_sources inventory_sources { get; set; }
        public virtual scx_generation_jobs scx_generation_jobs { get; set; }
        public virtual daypart_codes daypart_codes { get; set; }
    }
}
