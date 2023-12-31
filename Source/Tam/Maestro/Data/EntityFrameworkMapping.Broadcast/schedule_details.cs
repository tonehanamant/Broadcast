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
    
    public partial class schedule_details
    {
        public schedule_details()
        {
            this.schedule_detail_audiences = new HashSet<schedule_detail_audiences>();
            this.schedule_detail_weeks = new HashSet<schedule_detail_weeks>();
        }
    
        public int id { get; set; }
        public int schedule_id { get; set; }
        public string market { get; set; }
        public string network { get; set; }
        public string program { get; set; }
        public int daypart_id { get; set; }
        public decimal total_cost { get; set; }
        public int total_spots { get; set; }
        public decimal spot_cost { get; set; }
        public string spot_length { get; set; }
        public Nullable<int> spot_length_id { get; set; }
    
        public virtual ICollection<schedule_detail_audiences> schedule_detail_audiences { get; set; }
        public virtual ICollection<schedule_detail_weeks> schedule_detail_weeks { get; set; }
        public virtual daypart daypart { get; set; }
        public virtual spot_lengths spot_lengths { get; set; }
        public virtual schedule schedule { get; set; }
    }
}
