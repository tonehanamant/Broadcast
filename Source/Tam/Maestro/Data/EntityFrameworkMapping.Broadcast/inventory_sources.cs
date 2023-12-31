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
    
    public partial class inventory_sources
    {
        public inventory_sources()
        {
            this.station_inventory_group = new HashSet<station_inventory_group>();
            this.station_inventory_manifest = new HashSet<station_inventory_manifest>();
            this.station_inventory_manifest_staging = new HashSet<station_inventory_manifest_staging>();
            this.station_inventory_loaded = new HashSet<station_inventory_loaded>();
            this.inventory_files = new HashSet<inventory_files>();
            this.inventory_source_logos = new HashSet<inventory_source_logos>();
            this.inventory_summary = new HashSet<inventory_summary>();
            this.inventory_summary_quarters = new HashSet<inventory_summary_quarters>();
            this.scx_generation_job_files = new HashSet<scx_generation_job_files>();
            this.scx_generation_jobs = new HashSet<scx_generation_jobs>();
            this.inventory_programs_by_source_jobs = new HashSet<inventory_programs_by_source_jobs>();
            this.inventory_export_jobs = new HashSet<inventory_export_jobs>();
            this.inventory_proprietary_daypart_program_mappings = new HashSet<inventory_proprietary_daypart_program_mappings>();
            this.inventory_proprietary_summary = new HashSet<inventory_proprietary_summary>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public bool is_active { get; set; }
        public byte inventory_source_type { get; set; }
    
        public virtual ICollection<station_inventory_group> station_inventory_group { get; set; }
        public virtual ICollection<station_inventory_manifest> station_inventory_manifest { get; set; }
        public virtual ICollection<station_inventory_manifest_staging> station_inventory_manifest_staging { get; set; }
        public virtual ICollection<station_inventory_loaded> station_inventory_loaded { get; set; }
        public virtual ICollection<inventory_files> inventory_files { get; set; }
        public virtual ICollection<inventory_source_logos> inventory_source_logos { get; set; }
        public virtual ICollection<inventory_summary> inventory_summary { get; set; }
        public virtual ICollection<inventory_summary_quarters> inventory_summary_quarters { get; set; }
        public virtual ICollection<scx_generation_job_files> scx_generation_job_files { get; set; }
        public virtual ICollection<scx_generation_jobs> scx_generation_jobs { get; set; }
        public virtual ICollection<inventory_programs_by_source_jobs> inventory_programs_by_source_jobs { get; set; }
        public virtual ICollection<inventory_export_jobs> inventory_export_jobs { get; set; }
        public virtual ICollection<inventory_proprietary_daypart_program_mappings> inventory_proprietary_daypart_program_mappings { get; set; }
        public virtual ICollection<inventory_proprietary_summary> inventory_proprietary_summary { get; set; }
    }
}
