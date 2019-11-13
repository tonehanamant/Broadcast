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
    
    public partial class plan_versions
    {
        public plan_versions()
        {
            this.plan_version_available_markets = new HashSet<plan_version_available_markets>();
            this.plan_version_blackout_markets = new HashSet<plan_version_blackout_markets>();
            this.plan_version_dayparts = new HashSet<plan_version_dayparts>();
            this.plan_version_flight_hiatus_days = new HashSet<plan_version_flight_hiatus_days>();
            this.plan_version_secondary_audiences = new HashSet<plan_version_secondary_audiences>();
            this.plan_version_summaries = new HashSet<plan_version_summaries>();
            this.plan_version_weeks = new HashSet<plan_version_weeks>();
        }
    
        public int id { get; set; }
        public int plan_id { get; set; }
        public bool is_draft { get; set; }
        public int spot_length_id { get; set; }
        public bool equivalized { get; set; }
        public System.DateTime flight_start_date { get; set; }
        public System.DateTime flight_end_date { get; set; }
        public string flight_notes { get; set; }
        public int audience_type { get; set; }
        public int posting_type { get; set; }
        public int target_audience_id { get; set; }
        public int share_book_id { get; set; }
        public Nullable<int> hut_book_id { get; set; }
        public decimal budget { get; set; }
        public double target_impression { get; set; }
        public decimal target_cpm { get; set; }
        public double target_rating_points { get; set; }
        public decimal target_cpp { get; set; }
        public double target_universe { get; set; }
        public double hh_impressions { get; set; }
        public decimal hh_cpm { get; set; }
        public double hh_rating_points { get; set; }
        public decimal hh_cpp { get; set; }
        public double hh_universe { get; set; }
        public int currency { get; set; }
        public double target_vpvh { get; set; }
        public double coverage_goal_percent { get; set; }
        public int goal_breakdown_type { get; set; }
        public int status { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public Nullable<int> version_number { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual media_months share_media_months { get; set; }
        public virtual media_months hut_media_months { get; set; }
        public virtual ICollection<plan_version_available_markets> plan_version_available_markets { get; set; }
        public virtual ICollection<plan_version_blackout_markets> plan_version_blackout_markets { get; set; }
        public virtual ICollection<plan_version_dayparts> plan_version_dayparts { get; set; }
        public virtual ICollection<plan_version_flight_hiatus_days> plan_version_flight_hiatus_days { get; set; }
        public virtual ICollection<plan_version_secondary_audiences> plan_version_secondary_audiences { get; set; }
        public virtual ICollection<plan_version_summaries> plan_version_summaries { get; set; }
        public virtual ICollection<plan_version_weeks> plan_version_weeks { get; set; }
        public virtual plan plan { get; set; }
        public virtual spot_lengths spot_lengths { get; set; }
    }
}
