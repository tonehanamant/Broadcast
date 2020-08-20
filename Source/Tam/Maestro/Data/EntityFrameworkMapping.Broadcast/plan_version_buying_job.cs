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
    
    public partial class plan_version_buying_job
    {
        public plan_version_buying_job()
        {
            this.plan_version_buying_api_results = new HashSet<plan_version_buying_api_results>();
            this.plan_version_buying_bands = new HashSet<plan_version_buying_bands>();
            this.plan_version_buying_job_inventory_source_estimates = new HashSet<plan_version_buying_job_inventory_source_estimates>();
            this.plan_version_buying_stations = new HashSet<plan_version_buying_stations>();
            this.plan_version_buying_markets = new HashSet<plan_version_buying_markets>();
            this.plan_version_buying_parameters = new HashSet<plan_version_buying_parameters>();
            this.plan_version_buying_results = new HashSet<plan_version_buying_results>();
        }
    
        public int id { get; set; }
        public Nullable<int> plan_version_id { get; set; }
        public int status { get; set; }
        public System.DateTime queued_at { get; set; }
        public Nullable<System.DateTime> completed_at { get; set; }
        public string error_message { get; set; }
        public string diagnostic_result { get; set; }
        public string hangfire_job_id { get; set; }
    
        public virtual ICollection<plan_version_buying_api_results> plan_version_buying_api_results { get; set; }
        public virtual ICollection<plan_version_buying_bands> plan_version_buying_bands { get; set; }
        public virtual ICollection<plan_version_buying_job_inventory_source_estimates> plan_version_buying_job_inventory_source_estimates { get; set; }
        public virtual ICollection<plan_version_buying_stations> plan_version_buying_stations { get; set; }
        public virtual plan_versions plan_versions { get; set; }
        public virtual ICollection<plan_version_buying_markets> plan_version_buying_markets { get; set; }
        public virtual ICollection<plan_version_buying_parameters> plan_version_buying_parameters { get; set; }
        public virtual ICollection<plan_version_buying_results> plan_version_buying_results { get; set; }
    }
}