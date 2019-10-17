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
    
    public partial class campaign_summaries
    {
        public int id { get; set; }
        public int campaign_id { get; set; }
        public int processing_status { get; set; }
        public string processing_status_error_msg { get; set; }
        public System.DateTime queued_at { get; set; }
        public string queued_by { get; set; }
        public Nullable<System.DateTime> flight_start_Date { get; set; }
        public Nullable<System.DateTime> flight_end_Date { get; set; }
        public Nullable<int> flight_hiatus_days { get; set; }
        public Nullable<int> flight_active_days { get; set; }
        public Nullable<double> budget { get; set; }
        public Nullable<double> household_cpm { get; set; }
        public Nullable<double> household_delivery_impressions { get; set; }
        public Nullable<double> household_rating_points { get; set; }
        public Nullable<int> plan_status_count_working { get; set; }
        public Nullable<int> plan_status_count_reserved { get; set; }
        public Nullable<int> plan_status_count_client_approval { get; set; }
        public Nullable<int> plan_status_count_contracted { get; set; }
        public Nullable<int> plan_status_count_live { get; set; }
        public Nullable<int> plan_status_count_complete { get; set; }
        public Nullable<int> campaign_status { get; set; }
        public Nullable<System.DateTime> components_modified { get; set; }
        public Nullable<System.DateTime> last_aggregated { get; set; }
        public Nullable<int> plan_status_count_scenario { get; set; }
        public Nullable<int> plan_status_count_canceled { get; set; }
        public Nullable<int> plan_status_count_rejected { get; set; }
    
        public virtual campaign campaign { get; set; }
    }
}
