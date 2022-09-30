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
    
    public partial class spot_exceptions_recommended_plan_details
    {
        public spot_exceptions_recommended_plan_details()
        {
            this.spot_exceptions_recommended_plan_decision = new HashSet<spot_exceptions_recommended_plan_decision>();
        }
    
        public int id { get; set; }
        public int spot_exceptions_recommended_plan_id { get; set; }
        public int recommended_plan_id { get; set; }
        public long execution_trace_id { get; set; }
        public Nullable<decimal> rate { get; set; }
        public string audience_name { get; set; }
        public Nullable<double> contracted_impressions { get; set; }
        public Nullable<double> delivered_impressions { get; set; }
        public bool is_recommended_plan { get; set; }
        public Nullable<double> plan_clearance_percentage { get; set; }
        public string daypart_code { get; set; }
        public Nullable<int> start_time { get; set; }
        public Nullable<int> end_time { get; set; }
        public Nullable<int> monday { get; set; }
        public Nullable<int> tuesday { get; set; }
        public Nullable<int> wednesday { get; set; }
        public Nullable<int> thursday { get; set; }
        public Nullable<int> friday { get; set; }
        public Nullable<int> saturday { get; set; }
        public Nullable<int> sunday { get; set; }
        public Nullable<double> spot_delivered_impressions { get; set; }
        public Nullable<double> plan_total_contracted_impressions { get; set; }
        public Nullable<double> plan_total_delivered_impressions { get; set; }
        public int ingested_media_week_id { get; set; }
        public string ingested_by { get; set; }
        public System.DateTime ingested_at { get; set; }
        public string spot_unique_hash_external { get; set; }
        public string execution_id_external { get; set; }
    
        public virtual plan plan { get; set; }
        public virtual ICollection<spot_exceptions_recommended_plan_decision> spot_exceptions_recommended_plan_decision { get; set; }
        public virtual spot_exceptions_recommended_plans spot_exceptions_recommended_plans { get; set; }
    }
}
