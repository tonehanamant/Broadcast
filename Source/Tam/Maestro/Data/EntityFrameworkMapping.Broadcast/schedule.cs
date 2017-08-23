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
    
    public partial class schedule
    {
        public schedule()
        {
            this.schedule_audiences = new HashSet<schedule_audiences>();
            this.schedule_details = new HashSet<schedule_details>();
            this.schedule_restriction_dayparts = new HashSet<schedule_restriction_dayparts>();
            this.markets = new HashSet<market>();
            this.schedule_iscis = new HashSet<schedule_iscis>();
        }
    
        public int id { get; set; }
        public Nullable<int> estimate_id { get; set; }
        public int advertiser_id { get; set; }
        public string name { get; set; }
        public int posting_book_id { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
        public byte post_type { get; set; }
        public byte inventory_source { get; set; }
        public bool equivalized { get; set; }
    
        public virtual ICollection<schedule_audiences> schedule_audiences { get; set; }
        public virtual ICollection<schedule_details> schedule_details { get; set; }
        public virtual ICollection<schedule_restriction_dayparts> schedule_restriction_dayparts { get; set; }
        public virtual ICollection<market> markets { get; set; }
        public virtual ICollection<schedule_iscis> schedule_iscis { get; set; }
    }
}
