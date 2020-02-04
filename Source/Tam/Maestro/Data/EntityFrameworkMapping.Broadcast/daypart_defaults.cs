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
    
    public partial class daypart_defaults
    {
        public daypart_defaults()
        {
            this.inventory_file_proprietary_header = new HashSet<inventory_file_proprietary_header>();
            this.inventory_summary_quarter_details = new HashSet<inventory_summary_quarter_details>();
            this.plan_version_dayparts = new HashSet<plan_version_dayparts>();
            this.scx_generation_job_files = new HashSet<scx_generation_job_files>();
            this.scx_generation_jobs = new HashSet<scx_generation_jobs>();
            this.station_inventory_manifest_dayparts = new HashSet<station_inventory_manifest_dayparts>();
            this.nti_to_nsi_conversion_rates = new HashSet<nti_to_nsi_conversion_rates>();
        }
    
        public int id { get; set; }
        public int daypart_type { get; set; }
        public int daypart_id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    
        public virtual daypart daypart { get; set; }
        public virtual ICollection<inventory_file_proprietary_header> inventory_file_proprietary_header { get; set; }
        public virtual ICollection<inventory_summary_quarter_details> inventory_summary_quarter_details { get; set; }
        public virtual ICollection<plan_version_dayparts> plan_version_dayparts { get; set; }
        public virtual ICollection<scx_generation_job_files> scx_generation_job_files { get; set; }
        public virtual ICollection<scx_generation_jobs> scx_generation_jobs { get; set; }
        public virtual ICollection<station_inventory_manifest_dayparts> station_inventory_manifest_dayparts { get; set; }
        public virtual ICollection<nti_to_nsi_conversion_rates> nti_to_nsi_conversion_rates { get; set; }
    }
}
