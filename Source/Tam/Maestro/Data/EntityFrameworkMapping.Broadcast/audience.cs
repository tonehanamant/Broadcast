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
    
    public partial class audience
    {
        public audience()
        {
            this.audience_audiences = new HashSet<audience_audiences>();
            this.bvs_post_details = new HashSet<bvs_post_details>();
            this.post_file_detail_impressions = new HashSet<post_file_detail_impressions>();
            this.schedule_detail_audiences = new HashSet<schedule_detail_audiences>();
            this.station_program_flight_audiences = new HashSet<station_program_flight_audiences>();
            this.proposal_version_audiences = new HashSet<proposal_version_audiences>();
            this.schedule_audiences = new HashSet<schedule_audiences>();
        }
    
        public int id { get; set; }
        public byte category_code { get; set; }
        public string sub_category_code { get; set; }
        public Nullable<int> range_start { get; set; }
        public Nullable<int> range_end { get; set; }
        public bool custom { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    
        public virtual ICollection<audience_audiences> audience_audiences { get; set; }
        public virtual ICollection<bvs_post_details> bvs_post_details { get; set; }
        public virtual ICollection<post_file_detail_impressions> post_file_detail_impressions { get; set; }
        public virtual ICollection<schedule_detail_audiences> schedule_detail_audiences { get; set; }
        public virtual ICollection<station_program_flight_audiences> station_program_flight_audiences { get; set; }
        public virtual ICollection<proposal_version_audiences> proposal_version_audiences { get; set; }
        public virtual ICollection<schedule_audiences> schedule_audiences { get; set; }
    }
}
