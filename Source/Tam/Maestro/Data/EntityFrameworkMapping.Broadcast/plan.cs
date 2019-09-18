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
    
    public partial class plan
    {
        public plan()
        {
            this.plan_available_markets = new HashSet<plan_available_markets>();
            this.plan_blackout_markets = new HashSet<plan_blackout_markets>();
            this.plan_dayparts = new HashSet<plan_dayparts>();
            this.plan_flight_hiatus = new HashSet<plan_flight_hiatus>();
            this.plan_secondary_audiences = new HashSet<plan_secondary_audiences>();
            this.plan_summaries = new HashSet<plan_summaries>();
            this.plan_weeks = new HashSet<plan_weeks>();
        }
    
        public int id { get; set; }
        public int campaign_id { get; set; }
        public string name { get; set; }
        public int product_id { get; set; }
        public int spot_length_id { get; set; }
        public bool equivalized { get; set; }
        public int status { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public System.DateTime flight_start_date { get; set; }
        public System.DateTime flight_end_date { get; set; }
        public string flight_notes { get; set; }
        public int audience_type { get; set; }
        public int posting_type { get; set; }
        public int audience_id { get; set; }
        public int share_book_id { get; set; }
        public int hut_book_id { get; set; }
        public decimal budget { get; set; }
        public double delivery_impressions { get; set; }
        public decimal cpm { get; set; }
        public double coverage_goal_percent { get; set; }
        public int goal_breakdown_type { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
        public double delivery_rating_points { get; set; }
        public decimal cpp { get; set; }
        public int currency { get; set; }
        public double delivery { get; set; }
        public double vpvh { get; set; }
        public decimal household_cpm { get; set; }
        public double household_universe { get; set; }
        public decimal household_cpp { get; set; }
        public double household_rating_points { get; set; }
        public double universe { get; set; }
        public double household_delivery_impressions { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual campaign campaign { get; set; }
        public virtual media_months media_months { get; set; }
        public virtual media_months media_months1 { get; set; }
        public virtual ICollection<plan_available_markets> plan_available_markets { get; set; }
        public virtual ICollection<plan_blackout_markets> plan_blackout_markets { get; set; }
        public virtual ICollection<plan_dayparts> plan_dayparts { get; set; }
        public virtual ICollection<plan_flight_hiatus> plan_flight_hiatus { get; set; }
        public virtual ICollection<plan_secondary_audiences> plan_secondary_audiences { get; set; }
        public virtual ICollection<plan_summaries> plan_summaries { get; set; }
        public virtual ICollection<plan_weeks> plan_weeks { get; set; }
        public virtual spot_lengths spot_lengths { get; set; }
    }
}
