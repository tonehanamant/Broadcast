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
    
    public partial class campaign
    {
        public campaign()
        {
            this.proposals = new HashSet<proposal>();
            this.campaign_summaries = new HashSet<campaign_summaries>();
            this.plans = new HashSet<plan>();
            this.campaign_plan_secondary_audiences = new HashSet<campaign_plan_secondary_audiences>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public Nullable<int> advertiser_id { get; set; }
        public Nullable<int> agency_id { get; set; }
        public System.DateTime created_date { get; set; }
        public string created_by { get; set; }
        public System.DateTime modified_date { get; set; }
        public string modified_by { get; set; }
        public string notes { get; set; }
        public Nullable<System.Guid> agency_master_id { get; set; }
        public Nullable<System.Guid> advertiser_master_id { get; set; }
        public string unified_id { get; set; }
        public Nullable<int> max_fluidity_percent { get; set; }
        public Nullable<System.DateTime> unified_campaign_last_sent_at { get; set; }
        public Nullable<System.DateTime> unified_campaign_last_received_at { get; set; }
    
        public virtual ICollection<proposal> proposals { get; set; }
        public virtual ICollection<campaign_summaries> campaign_summaries { get; set; }
        public virtual ICollection<plan> plans { get; set; }
        public virtual ICollection<campaign_plan_secondary_audiences> campaign_plan_secondary_audiences { get; set; }
    }
}
