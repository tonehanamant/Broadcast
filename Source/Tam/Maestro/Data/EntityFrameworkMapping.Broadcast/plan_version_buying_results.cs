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
    
    public partial class plan_version_buying_results
    {
        public plan_version_buying_results()
        {
            this.plan_version_buying_band_details = new HashSet<plan_version_buying_band_details>();
            this.plan_version_buying_ownership_group_details = new HashSet<plan_version_buying_ownership_group_details>();
            this.plan_version_buying_result_spots = new HashSet<plan_version_buying_result_spots>();
            this.plan_version_buying_station_details = new HashSet<plan_version_buying_station_details>();
            this.plan_version_buying_rep_firm_details = new HashSet<plan_version_buying_rep_firm_details>();
            this.plan_version_buying_market_details = new HashSet<plan_version_buying_market_details>();
            this.plan_version_buying_result_spot_stations = new HashSet<plan_version_buying_result_spot_stations>();
        }
    
        public int id { get; set; }
        public decimal optimal_cpm { get; set; }
        public int total_market_count { get; set; }
        public int total_station_count { get; set; }
        public decimal total_avg_cpm { get; set; }
        public double total_avg_impressions { get; set; }
        public bool goal_fulfilled_by_proprietary { get; set; }
        public double total_impressions { get; set; }
        public decimal total_budget { get; set; }
        public Nullable<int> plan_version_buying_job_id { get; set; }
        public int total_spots { get; set; }
        public double total_market_coverage_percent { get; set; }
        public int spot_allocation_model_mode { get; set; }
        public int posting_type { get; set; }
    
        public virtual ICollection<plan_version_buying_band_details> plan_version_buying_band_details { get; set; }
        public virtual ICollection<plan_version_buying_ownership_group_details> plan_version_buying_ownership_group_details { get; set; }
        public virtual ICollection<plan_version_buying_result_spots> plan_version_buying_result_spots { get; set; }
        public virtual ICollection<plan_version_buying_station_details> plan_version_buying_station_details { get; set; }
        public virtual ICollection<plan_version_buying_rep_firm_details> plan_version_buying_rep_firm_details { get; set; }
        public virtual ICollection<plan_version_buying_market_details> plan_version_buying_market_details { get; set; }
        public virtual ICollection<plan_version_buying_result_spot_stations> plan_version_buying_result_spot_stations { get; set; }
        public virtual plan_version_buying_job plan_version_buying_job { get; set; }
    }
}
