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
    
    public partial class scx_generation_jobs
    {
        public scx_generation_jobs()
        {
            this.scx_generation_job_files = new HashSet<scx_generation_job_files>();
            this.scx_generation_job_units = new HashSet<scx_generation_job_units>();
        }
    
        public int id { get; set; }
        public int inventory_source_id { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public int status { get; set; }
        public System.DateTime queued_at { get; set; }
        public Nullable<System.DateTime> completed_at { get; set; }
        public string requested_by { get; set; }
        public int standard_daypart_id { get; set; }
    
        public virtual inventory_sources inventory_sources { get; set; }
        public virtual ICollection<scx_generation_job_files> scx_generation_job_files { get; set; }
        public virtual ICollection<scx_generation_job_units> scx_generation_job_units { get; set; }
        public virtual standard_dayparts standard_dayparts { get; set; }
    }
}
