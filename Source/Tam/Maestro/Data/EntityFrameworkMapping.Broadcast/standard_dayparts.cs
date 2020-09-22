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
    
    public partial class standard_dayparts
    {
        public standard_dayparts()
        {
            this.inventory_file_proprietary_header = new HashSet<inventory_file_proprietary_header>();
            this.inventory_proprietary_daypart_program_mappings = new HashSet<inventory_proprietary_daypart_program_mappings>();
            this.inventory_summary_quarter_details = new HashSet<inventory_summary_quarter_details>();
            this.nti_to_nsi_conversion_rates = new HashSet<nti_to_nsi_conversion_rates>();
            this.plan_version_audience_daypart_vpvh = new HashSet<plan_version_audience_daypart_vpvh>();
            this.plan_version_buying_api_result_spots = new HashSet<plan_version_buying_api_result_spots>();
            this.plan_version_dayparts = new HashSet<plan_version_dayparts>();
            this.plan_version_pricing_api_result_spots = new HashSet<plan_version_pricing_api_result_spots>();
            this.plan_version_weekly_breakdown = new HashSet<plan_version_weekly_breakdown>();
            this.scx_generation_job_files = new HashSet<scx_generation_job_files>();
            this.scx_generation_jobs = new HashSet<scx_generation_jobs>();
            this.station_inventory_manifest_dayparts = new HashSet<station_inventory_manifest_dayparts>();
        }
    
        public int id { get; set; }
        public int daypart_type { get; set; }
        public int daypart_id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int vpvh_calculation_source_type { get; set; }
    
        public virtual daypart daypart { get; set; }
        public virtual ICollection<inventory_file_proprietary_header> inventory_file_proprietary_header { get; set; }
        public virtual ICollection<inventory_proprietary_daypart_program_mappings> inventory_proprietary_daypart_program_mappings { get; set; }
        public virtual ICollection<inventory_summary_quarter_details> inventory_summary_quarter_details { get; set; }
        public virtual ICollection<nti_to_nsi_conversion_rates> nti_to_nsi_conversion_rates { get; set; }
        public virtual ICollection<plan_version_audience_daypart_vpvh> plan_version_audience_daypart_vpvh { get; set; }
        public virtual ICollection<plan_version_buying_api_result_spots> plan_version_buying_api_result_spots { get; set; }
        public virtual ICollection<plan_version_dayparts> plan_version_dayparts { get; set; }
        public virtual ICollection<plan_version_pricing_api_result_spots> plan_version_pricing_api_result_spots { get; set; }
        public virtual ICollection<plan_version_weekly_breakdown> plan_version_weekly_breakdown { get; set; }
        public virtual ICollection<scx_generation_job_files> scx_generation_job_files { get; set; }
        public virtual ICollection<scx_generation_jobs> scx_generation_jobs { get; set; }
        public virtual ICollection<station_inventory_manifest_dayparts> station_inventory_manifest_dayparts { get; set; }
    }
}