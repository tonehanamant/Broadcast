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
    
    public partial class plan_version_pricing_api_results
    {
        public plan_version_pricing_api_results()
        {
            this.plan_version_pricing_api_result_spots = new HashSet<plan_version_pricing_api_result_spots>();
        }
    
        public int id { get; set; }
        public int plan_version_id { get; set; }
        public decimal optimal_cpm { get; set; }
        public Nullable<int> plan_version_pricing_job_id { get; set; }
    
        public virtual plan_versions plan_versions { get; set; }
        public virtual ICollection<plan_version_pricing_api_result_spots> plan_version_pricing_api_result_spots { get; set; }
        public virtual plan_version_pricing_job plan_version_pricing_job { get; set; }
    }
}
