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
    
    public partial class plan_version_pricing_markets
    {
        public plan_version_pricing_markets()
        {
            this.plan_version_pricing_market_details = new HashSet<plan_version_pricing_market_details>();
        }
    
        public int id { get; set; }
        public int plan_version_id { get; set; }
        public Nullable<int> plan_version_pricing_job_id { get; set; }
        public int total_markets { get; set; }
        public double total_coverage_percent { get; set; }
        public int total_stations { get; set; }
        public int total_spots { get; set; }
        public double total_impressions { get; set; }
        public double total_cpm { get; set; }
        public double total_budget { get; set; }
    
        public virtual plan_version_pricing_job plan_version_pricing_job { get; set; }
        public virtual ICollection<plan_version_pricing_market_details> plan_version_pricing_market_details { get; set; }
        public virtual plan_versions plan_versions { get; set; }
    }
}