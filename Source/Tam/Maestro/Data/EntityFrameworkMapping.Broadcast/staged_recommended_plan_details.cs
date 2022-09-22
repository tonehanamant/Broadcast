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
    
    public partial class staged_recommended_plan_details
    {
        public int id { get; set; }
        public int staged_recommended_plan_id { get; set; }
        public int recommended_plan_id { get; set; }
        public long execution_trace_id { get; set; }
        public Nullable<decimal> rate { get; set; }
        public string audience_name { get; set; }
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
        public Nullable<double> contracted_impressions { get; set; }
        public Nullable<double> delivered_impressions { get; set; }
        public string plan_spot_unique_hash_external { get; set; }
        public string plan_execution_id_external { get; set; }
        public Nullable<double> spot_delivered_impression { get; set; }
        public Nullable<double> plan_total_contracted_impressions { get; set; }
        public Nullable<double> plan_total_delivered_impressions { get; set; }
    
        public virtual staged_recommended_plans staged_recommended_plans { get; set; }
    }
}
