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
    
    public partial class plan_version_buying_bands
    {
        public plan_version_buying_bands()
        {
            this.plan_version_buying_band_details = new HashSet<plan_version_buying_band_details>();
        }
    
        public int id { get; set; }
        public Nullable<int> plan_version_buying_job_id { get; set; }
        public int total_spots { get; set; }
        public double total_impressions { get; set; }
        public decimal total_cpm { get; set; }
        public decimal total_budget { get; set; }
    
        public virtual ICollection<plan_version_buying_band_details> plan_version_buying_band_details { get; set; }
        public virtual plan_version_buying_job plan_version_buying_job { get; set; }
    }
}
