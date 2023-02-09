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
            this.plan_iscis = new HashSet<plan_iscis>();
            this.plan_versions = new HashSet<plan_versions>();
            this.spot_exceptions_recommended_plan_details_done = new HashSet<spot_exceptions_recommended_plan_details_done>();
            this.spot_exceptions_recommended_plan_details = new HashSet<spot_exceptions_recommended_plan_details>();
            this.spot_exceptions_out_of_specs_done = new HashSet<spot_exceptions_out_of_specs_done>();
            this.spot_exceptions_out_of_specs = new HashSet<spot_exceptions_out_of_specs>();
        }
    
        public int id { get; set; }
        public int campaign_id { get; set; }
        public string name { get; set; }
        public Nullable<int> product_id { get; set; }
        public int latest_version_id { get; set; }
        public Nullable<System.Guid> product_master_id { get; set; }
        public Nullable<int> spot_allocation_model_mode { get; set; }
        public int plan_mode { get; set; }
        public string deleted_by { get; set; }
        public Nullable<System.DateTime> deleted_at { get; set; }
        public string unified_tactic_line_id { get; set; }
        public Nullable<System.DateTime> unified_campaign_last_sent_at { get; set; }
        public Nullable<System.DateTime> unified_campaign_last_received_at { get; set; }
        public string nielsen_transmittal_code { get; set; }
    
        public virtual campaign campaign { get; set; }
        public virtual ICollection<plan_iscis> plan_iscis { get; set; }
        public virtual ICollection<plan_versions> plan_versions { get; set; }
        public virtual ICollection<spot_exceptions_recommended_plan_details_done> spot_exceptions_recommended_plan_details_done { get; set; }
        public virtual ICollection<spot_exceptions_recommended_plan_details> spot_exceptions_recommended_plan_details { get; set; }
        public virtual ICollection<spot_exceptions_out_of_specs_done> spot_exceptions_out_of_specs_done { get; set; }
        public virtual ICollection<spot_exceptions_out_of_specs> spot_exceptions_out_of_specs { get; set; }
    }
}
