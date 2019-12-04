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
    
    public partial class daypart_codes
    {
        public daypart_codes()
        {
            this.inventory_file_proprietary_header = new HashSet<inventory_file_proprietary_header>();
            this.inventory_summary_quarter_details = new HashSet<inventory_summary_quarter_details>();
            this.plan_version_dayparts = new HashSet<plan_version_dayparts>();
            this.scx_generation_job_files = new HashSet<scx_generation_job_files>();
            this.scx_generation_jobs = new HashSet<scx_generation_jobs>();
            this.station_inventory_manifest_dayparts = new HashSet<station_inventory_manifest_dayparts>();
        }
    
        public int id { get; set; }
        public string code { get; set; }
        public bool is_active { get; set; }
        public string full_name { get; set; }
        public int daypart_type { get; set; }
        public int default_start_time_seconds { get; set; }
        public int default_end_time_seconds { get; set; }
    
        public virtual ICollection<inventory_file_proprietary_header> inventory_file_proprietary_header { get; set; }
        public virtual ICollection<inventory_summary_quarter_details> inventory_summary_quarter_details { get; set; }
        public virtual ICollection<plan_version_dayparts> plan_version_dayparts { get; set; }
        public virtual ICollection<scx_generation_job_files> scx_generation_job_files { get; set; }
        public virtual ICollection<scx_generation_jobs> scx_generation_jobs { get; set; }
        public virtual ICollection<station_inventory_manifest_dayparts> station_inventory_manifest_dayparts { get; set; }
    }
}
