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
    
    public partial class plan_version_summaries
    {
        public plan_version_summaries()
        {
            this.plan_version_summary_quarters = new HashSet<plan_version_summary_quarters>();
        }
    
        public int id { get; set; }
        public Nullable<int> processing_status { get; set; }
        public Nullable<int> hiatus_days_count { get; set; }
        public Nullable<int> active_day_count { get; set; }
        public Nullable<int> available_market_count { get; set; }
        public Nullable<double> available_market_total_us_coverage_percent { get; set; }
        public Nullable<int> blackout_market_count { get; set; }
        public Nullable<double> blackout_market_total_us_coverage_percent { get; set; }
        public string product_name { get; set; }
        public int plan_version_id { get; set; }
        public Nullable<int> available_market_with_sov_count { get; set; }
        public Nullable<double> fluidity_percentage { get; set; }
        public Nullable<bool> is_adu_plan { get; set; }
    
        public virtual ICollection<plan_version_summary_quarters> plan_version_summary_quarters { get; set; }
        public virtual plan_versions plan_versions { get; set; }
    }
}
